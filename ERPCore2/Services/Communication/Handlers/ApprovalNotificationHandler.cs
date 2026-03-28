using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services.Communication.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Communication.Handlers
{
    /// <summary>
    /// 審核通知處理器
    /// 當文件被提交審核、核准或駁回時，自動產生通知給相關人員
    /// </summary>
    public class ApprovalNotificationHandler :
        IBusinessEventHandler<DocumentSubmittedForApprovalEvent>,
        IBusinessEventHandler<DocumentApprovedEvent>,
        IBusinessEventHandler<DocumentRejectedEvent>
    {
        private readonly ISystemNotificationService _notificationService;
        private readonly IApproverAssignmentService _approverService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ApprovalNotificationHandler> _logger;

        // 模組顯示名稱對應
        private static readonly Dictionary<string, string> ModuleDisplayNames = new()
        {
            ["SalesOrder"] = "銷貨訂單",
            ["Quotation"] = "報價單",
            ["SalesDelivery"] = "銷貨出貨單",
            ["SalesReturn"] = "銷貨退回單",
            ["PurchaseOrder"] = "採購訂單",
            ["PurchaseReceiving"] = "採購進貨單",
            ["PurchaseReturn"] = "採購退回單",
            ["StockTaking"] = "盤點單"
        };

        public ApprovalNotificationHandler(
            ISystemNotificationService notificationService,
            IApproverAssignmentService approverService,
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<ApprovalNotificationHandler> logger)
        {
            _notificationService = notificationService;
            _approverService = approverService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 文件提交審核 → 通知審核者
        /// </summary>
        public async Task HandleAsync(DocumentSubmittedForApprovalEvent e, CancellationToken ct = default)
        {
            var approverIds = await _approverService.GetApproverEmployeeIdsAsync(e.SourceModule);
            if (approverIds.Count == 0)
            {
                // 未設定審核人員 → 不發送通知（向下相容）
                return;
            }

            var moduleName = GetModuleDisplayName(e.SourceModule);
            var amountText = e.Amount.HasValue ? $"，金額 {e.Amount:N0}" : "";

            var notifications = approverIds.Select(approverId => new SystemNotification
            {
                RecipientEmployeeId = approverId,
                SenderEmployeeId = e.TriggeredBy,
                NotificationType = NotificationType.ApprovalRequest,
                Priority = e.Amount > 100000 ? NotificationPriority.High : NotificationPriority.Normal,
                Title = $"{moduleName} {e.DocumentCode} 待審核",
                Content = $"{moduleName} {e.DocumentCode} 已提交審核{amountText}，請審核。",
                SourceModule = e.SourceModule,
                SourceId = e.SourceId,
                NavigationUrl = GetNavigationUrl(e.SourceModule, e.SourceId)
            }).ToList();

            await _notificationService.CreateNotificationsAsync(notifications);

            _logger.LogInformation(
                "已發送審核請求通知：{Module} {Code} → {Count} 位審核人員",
                e.SourceModule, e.DocumentCode, approverIds.Count);
        }

        /// <summary>
        /// 文件核准 → 通知建立者
        /// </summary>
        public async Task HandleAsync(DocumentApprovedEvent e, CancellationToken ct = default)
        {
            var creatorEmployeeId = await ResolveDocumentCreatorAsync(e.SourceModule, e.SourceId);
            if (creatorEmployeeId == null || creatorEmployeeId == e.ApprovedBy)
            {
                // 找不到建立者，或建立者就是審核者本人 → 不通知
                return;
            }

            var moduleName = GetModuleDisplayName(e.SourceModule);
            var approverName = await GetEmployeeNameAsync(e.ApprovedBy);

            var notification = new SystemNotification
            {
                RecipientEmployeeId = creatorEmployeeId.Value,
                SenderEmployeeId = e.ApprovedBy,
                NotificationType = NotificationType.ApprovalResult,
                Priority = NotificationPriority.Normal,
                Title = $"{moduleName} {e.DocumentCode} 已核准",
                Content = $"您的{moduleName} {e.DocumentCode} 已由 {approverName} 核准。",
                SourceModule = e.SourceModule,
                SourceId = e.SourceId,
                NavigationUrl = GetNavigationUrl(e.SourceModule, e.SourceId)
            };

            await _notificationService.CreateNotificationAsync(notification);

            _logger.LogInformation(
                "已發送核准通知：{Module} {Code} → 建立者 EmployeeId={CreatorId}",
                e.SourceModule, e.DocumentCode, creatorEmployeeId);
        }

        /// <summary>
        /// 文件駁回 → 通知建立者
        /// </summary>
        public async Task HandleAsync(DocumentRejectedEvent e, CancellationToken ct = default)
        {
            var creatorEmployeeId = await ResolveDocumentCreatorAsync(e.SourceModule, e.SourceId);
            if (creatorEmployeeId == null)
            {
                return;
            }

            var moduleName = GetModuleDisplayName(e.SourceModule);
            var rejectorName = await GetEmployeeNameAsync(e.RejectedBy);

            var notification = new SystemNotification
            {
                RecipientEmployeeId = creatorEmployeeId.Value,
                SenderEmployeeId = e.RejectedBy,
                NotificationType = NotificationType.ApprovalResult,
                Priority = NotificationPriority.High,
                Title = $"{moduleName} {e.DocumentCode} 已駁回",
                Content = $"您的{moduleName} {e.DocumentCode} 已由 {rejectorName} 駁回。原因：{e.Reason}",
                SourceModule = e.SourceModule,
                SourceId = e.SourceId,
                NavigationUrl = GetNavigationUrl(e.SourceModule, e.SourceId)
            };

            await _notificationService.CreateNotificationAsync(notification);

            _logger.LogInformation(
                "已發送駁回通知：{Module} {Code} → 建立者 EmployeeId={CreatorId}",
                e.SourceModule, e.DocumentCode, creatorEmployeeId);
        }

        // ===== 內部輔助方法 =====

        /// <summary>
        /// 根據模組和文件 ID 找出建立者的 EmployeeId
        /// BaseEntity.CreatedBy 儲存的是員工姓名，需反查 Employee 表
        /// </summary>
        private async Task<int?> ResolveDocumentCreatorAsync(string sourceModule, int sourceId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得文件的 CreatedBy（員工姓名）
                string? createdByName = sourceModule switch
                {
                    "SalesOrder" => await context.SalesOrders.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "Quotation" => await context.Quotations.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "SalesDelivery" => await context.SalesDeliveries.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "SalesReturn" => await context.SalesReturns.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "PurchaseOrder" => await context.PurchaseOrders.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "PurchaseReceiving" => await context.PurchaseReceivings.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    "PurchaseReturn" => await context.PurchaseReturns.Where(x => x.Id == sourceId).Select(x => x.CreatedBy).FirstOrDefaultAsync(),
                    _ => null
                };

                if (string.IsNullOrEmpty(createdByName)) return null;

                // 反查 Employee 表取得 EmployeeId
                var employeeId = await context.Employees
                    .Where(e => e.Name == createdByName && e.Status == EntityStatus.Active)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync();

                return employeeId > 0 ? employeeId : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析文件建立者失敗：{Module} Id={Id}", sourceModule, sourceId);
                return null;
            }
        }

        /// <summary>
        /// 取得員工姓名
        /// </summary>
        private async Task<string> GetEmployeeNameAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Employees
                    .Where(e => e.Id == employeeId)
                    .Select(e => e.Name ?? "未知")
                    .FirstOrDefaultAsync() ?? "未知";
            }
            catch
            {
                return "未知";
            }
        }

        private static string GetModuleDisplayName(string moduleName)
        {
            return ModuleDisplayNames.TryGetValue(moduleName, out var displayName) ? displayName : moduleName;
        }

        private static string? GetNavigationUrl(string sourceModule, int sourceId)
        {
            // 目前通知點擊後導向對應模組的列表頁面
            // 後續可擴展為直接開啟指定文件的編輯 Modal
            return sourceModule switch
            {
                "SalesOrder" => "/sales-orders",
                "Quotation" => "/quotations",
                "SalesDelivery" => "/sales-deliveries",
                "SalesReturn" => "/sales-returns",
                "PurchaseOrder" => "/purchase-orders",
                "PurchaseReceiving" => "/purchase-receivings",
                "PurchaseReturn" => "/purchase-returns",
                _ => null
            };
        }
    }
}
