using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 生產排程表報表服務實作
    /// 依排程單分組顯示排程項目、數量、狀態、預計日期等明細
    /// </summary>
    public class ProductionScheduleReportService : IProductionScheduleReportService
    {
        private readonly IProductionScheduleService _scheduleService;
        private readonly IProductionScheduleItemService _itemService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<ProductionScheduleReportService>? _logger;

        public ProductionScheduleReportService(
            IProductionScheduleService scheduleService,
            IProductionScheduleItemService itemService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<ProductionScheduleReportService>? logger = null)
        {
            _scheduleService = scheduleService;
            _itemService = itemService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染生產排程表報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductionScheduleCriteria criteria)
        {
            try
            {
                var scheduleGroups = await GetScheduleDataAsync(criteria);

                if (!scheduleGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的生產排程資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildScheduleDocument(scheduleGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalItems = scheduleGroups.Sum(g => g.Items.Count);
                return BatchPreviewResult.Success(images, document, totalItems);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生生產排程表報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region IEntityReportService<ProductionSchedule> 實作

        /// <summary>
        /// 生成單一排程的格式化報表文件
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<FormattedDocument> GenerateReportAsync(int entityId)
        {
            var criteria = new ProductionScheduleCriteria
            {
                ScheduleIds = new List<int> { entityId },
                IncludeClosed = true
            };

            var scheduleGroups = await GetScheduleDataAsync(criteria);
            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildScheduleDocument(scheduleGroups, company, criteria);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張設定）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int entityId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(entityId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印生產排程表時發生錯誤，排程ID: {ScheduleId}", entityId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印（BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var productionCriteria = new ProductionScheduleCriteria
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate
                };

                var scheduleGroups = await GetScheduleDataAsync(productionCriteria);

                if (!scheduleGroups.Any())
                {
                    return ServiceResult.Failure($"無符合條件的生產排程資料\n篩選條件：{criteria.GetSummary()}");
                }

                foreach (var group in scheduleGroups)
                {
                    var result = await DirectPrintAsync(group.ScheduleId, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中生產排程 {ScheduleId} 失敗：{ErrorMessage}", group.ScheduleId, result.ErrorMessage);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印生產排程表時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var productionCriteria = new ProductionScheduleCriteria
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate,
                    PaperSetting = criteria.PaperSetting
                };

                return await RenderBatchToImagesAsync(productionCriteria);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染生產排程表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢生產排程資料並分組
        /// </summary>
        private async Task<List<ScheduleGroup>> GetScheduleDataAsync(ProductionScheduleCriteria criteria)
        {
            List<ProductionSchedule> schedules;

            // 優先使用指定的排程單 ID（從編輯畫面列印時使用）
            if (criteria.ScheduleIds.Any())
            {
                var tasks = criteria.ScheduleIds.Select(id => _scheduleService.GetByIdAsync(id));
                var results = await Task.WhenAll(tasks);
                schedules = results.Where(s => s != null).Cast<ProductionSchedule>().ToList();
            }
            else
            {
                var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
                var endDate = criteria.EndDate ?? DateTime.Today;

                // 取得日期範圍內的排程主檔
                schedules = await _scheduleService.GetByDateRangeAsync(startDate.Date, endDate.Date);
            }

            // 篩選客戶
            if (criteria.CustomerIds.Any())
            {
                schedules = schedules
                    .Where(s => s.CustomerId.HasValue && criteria.CustomerIds.Contains(s.CustomerId.Value))
                    .ToList();
            }

            if (!schedules.Any())
                return new List<ScheduleGroup>();

            // 取得所有排程項目（含 Product、ProductionSchedule 等導航屬性）
            var allItems = await _itemService.GetAllAsync();

            var scheduleIds = schedules.Select(s => s.Id).ToHashSet();

            // 篩選屬於目標排程的項目
            var filteredItems = allItems
                .Where(item => scheduleIds.Contains(item.ProductionScheduleId))
                .ToList();

            // 篩選狀態
            if (criteria.StatusFilters.Any())
            {
                filteredItems = filteredItems
                    .Where(item => criteria.StatusFilters.Contains(item.ProductionItemStatus))
                    .ToList();
            }

            // 排除已結案
            if (!criteria.IncludeClosed)
            {
                filteredItems = filteredItems.Where(item => !item.IsClosed).ToList();
            }

            // 按排程主檔分組
            var groups = schedules
                .Select(schedule =>
                {
                    var items = filteredItems
                        .Where(item => item.ProductionScheduleId == schedule.Id)
                        .OrderBy(item => item.Priority)
                        .ThenBy(item => item.Id)
                        .ToList();

                    return new ScheduleGroup
                    {
                        ScheduleId = schedule.Id,
                        ScheduleCode = schedule.Code ?? "",
                        ScheduleDate = schedule.ScheduleDate,
                        CustomerName = schedule.Customer?.CompanyName ?? "",
                        CustomerCode = schedule.Customer?.Code ?? "",
                        CreatedByName = schedule.CreatedByEmployee?.Name ?? "",
                        Items = items
                    };
                })
                .Where(g => g.Items.Any()) // 只保留有項目的排程
                .OrderByDescending(g => g.ScheduleDate)
                .ThenByDescending(g => g.ScheduleId)
                .ToList();

            return groups;
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構生產排程表報表
        /// </summary>
        private FormattedDocument BuildScheduleDocument(
            List<ScheduleGroup> groups,
            Company? company,
            ProductionScheduleCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";

            var totalItems = groups.Sum(g => g.Items.Count);
            var totalScheduledQty = groups.Sum(g => g.Items.Sum(i => i.ScheduledQuantity));
            var totalCompletedQty = groups.Sum(g => g.Items.Sum(i => i.CompletedQuantity));

            var doc = new FormattedDocument()
                .SetDocumentName($"生產排程表-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("生 產 排 程 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"查詢期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"排程數：{groups.Count} | 項目數：{totalItems}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依排程分組顯示 ===
            foreach (var group in groups)
            {
                // 排程標題
                var customerInfo = !string.IsNullOrEmpty(group.CustomerName)
                    ? $"客戶：{group.CustomerName}"
                    : "客戶：（無）";

                var employeeInfo = !string.IsNullOrEmpty(group.CreatedByName)
                    ? $"製單人員：{group.CreatedByName}"
                    : "";

                doc.AddKeyValueRow(
                    ("排程單號", group.ScheduleCode),
                    ("排程日期", group.ScheduleDate.ToString("yyyy/MM/dd")));

                doc.AddKeyValueRow(
                    ("客戶", !string.IsNullOrEmpty(group.CustomerName) ? group.CustomerName : "（無）"),
                    ("製單人員", group.CreatedByName));

                doc.AddSpacing(3);

                // 排程項目表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.35f, TextAlignment.Center)
                         .AddColumn("品號", 0.8f, TextAlignment.Left)
                         .AddColumn("品名", 1.3f, TextAlignment.Left)
                         .AddColumn("排程數量", 0.65f, TextAlignment.Right)
                         .AddColumn("已完成", 0.65f, TextAlignment.Right)
                         .AddColumn("待完成", 0.65f, TextAlignment.Right)
                         .AddColumn("狀態", 0.5f, TextAlignment.Center)
                         .AddColumn("預計開始", 0.7f, TextAlignment.Center)
                         .AddColumn("預計完成", 0.7f, TextAlignment.Center)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var item in group.Items)
                    {
                        var statusText = item.ProductionItemStatus switch
                        {
                            ProductionItemStatus.Pending => "待生產",
                            ProductionItemStatus.InProgress => "生產中",
                            ProductionItemStatus.Completed => "已完成",
                            ProductionItemStatus.Discontinued => "已停產",
                            _ => ""
                        };

                        if (item.IsClosed)
                            statusText += "(結)";

                        table.AddRow(
                            rowNum.ToString(),
                            item.Product?.Code ?? "",
                            item.Product?.Name ?? "",
                            item.ScheduledQuantity.ToString("N0"),
                            item.CompletedQuantity.ToString("N0"),
                            item.PendingQuantity.ToString("N0"),
                            statusText,
                            item.PlannedStartDate?.ToString("MM/dd") ?? "",
                            item.PlannedEndDate?.ToString("MM/dd") ?? ""
                        );
                        rowNum++;
                    }
                });

                // 排程小計
                var groupScheduledQty = group.Items.Sum(i => i.ScheduledQuantity);
                var groupCompletedQty = group.Items.Sum(i => i.CompletedQuantity);
                var groupPendingQty = group.Items.Sum(i => i.PendingQuantity);

                doc.AddKeyValueRow(
                    ("小計", $"項目：{group.Items.Count}  排程：{groupScheduledQty:N0}  完成：{groupCompletedQty:N0}  待完成：{groupPendingQty:N0}"));

                doc.AddSpacing(8);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                // 狀態統計
                var allItems = groups.SelectMany(g => g.Items).ToList();
                var pendingCount = allItems.Count(i => i.ProductionItemStatus == ProductionItemStatus.Pending);
                var inProgressCount = allItems.Count(i => i.ProductionItemStatus == ProductionItemStatus.InProgress);
                var completedCount = allItems.Count(i => i.ProductionItemStatus == ProductionItemStatus.Completed);
                var discontinuedCount = allItems.Count(i => i.ProductionItemStatus == ProductionItemStatus.Discontinued);

                var summaryLines = new List<string>
                {
                    $"排程單數：{groups.Count}",
                    $"項目總數：{totalItems}",
                    $"排程數量合計：{totalScheduledQty:N0}",
                    $"已完成合計：{totalCompletedQty:N0}",
                    $"待完成合計：{(totalScheduledQty - totalCompletedQty):N0}",
                    "",
                    "狀態統計：",
                    $"  待生產：{pendingCount}  生產中：{inProgressCount}  已完成：{completedCount}  已停產：{discontinuedCount}"
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "生產主管" });
            });

            return doc;
        }

        #endregion

        #region 內部資料模型

        /// <summary>
        /// 排程分組資料
        /// </summary>
        private class ScheduleGroup
        {
            public int ScheduleId { get; set; }
            public string ScheduleCode { get; set; } = "";
            public DateTime ScheduleDate { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerCode { get; set; } = "";
            public string CreatedByName { get; set; } = "";
            public List<ProductionScheduleItem> Items { get; set; } = new();
        }

        #endregion
    }
}
