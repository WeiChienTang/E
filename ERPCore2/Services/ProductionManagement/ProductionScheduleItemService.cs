using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Schedule;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程項目服務實作
    /// </summary>
    public class ProductionScheduleItemService : GenericManagementService<ProductionScheduleItem>, IProductionScheduleItemService
    {
        private readonly IInventoryStockService _inventoryStockService;

        public ProductionScheduleItemService(
            IDbContextFactory<AppDbContext> contextFactory,
            IInventoryStockService inventoryStockService) : base(contextFactory)
        {
            _inventoryStockService = inventoryStockService;
        }

        public ProductionScheduleItemService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionScheduleItem>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        /// <summary>
        /// 檢查製令單號是否已存在（供 EntityCodeGenerationHelper 使用）
        /// </summary>
        public async Task<bool> IsProductionScheduleItemCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductionScheduleItems.Where(psi => psi.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(psi => psi.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductionScheduleItemCodeExistsAsync), GetType(), _logger, new { code });
                return false;
            }
        }

        /// <summary>
        /// 分頁查詢（含篩選）- 供製令單 Index 頁面使用
        /// </summary>
        public async Task<(List<ProductionScheduleItem> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ProductionScheduleItem>, IQueryable<ProductionScheduleItem>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<ProductionScheduleItem> query = context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.ResponsibleEmployee);

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(psi => psi.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<ProductionScheduleItem>(), 0);
            }
        }

        // 覆寫 GetAllAsync 以包含相關資料
        protected override IQueryable<ProductionScheduleItem> BuildGetAllQuery(AppDbContext context)
        {
            return context.ProductionScheduleItems
                .Include(psi => psi.ProductionSchedule)
                .Include(psi => psi.Item)
                .Include(psi => psi.SalesOrderDetail)
                    .ThenInclude(sod => sod!.SalesOrder)
                .Include(psi => psi.Warehouse)
                .Include(psi => psi.ResponsibleEmployee)
                .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                .ThenBy(psi => psi.Priority);
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionScheduleItem?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                        .ThenInclude(p => p.ItemCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.WarehouseLocation)
                    .Include(psi => psi.ResponsibleEmployee)
                    .FirstOrDefaultAsync(psi => psi.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        // 實作 ValidateAsync
        public override async Task<ServiceResult> ValidateAsync(ProductionScheduleItem entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ProductionScheduleId <= 0)
                    errors.Add("生產排程主檔為必填");

                if (entity.ItemId <= 0)
                    errors.Add("品項為必填");

                if (entity.ScheduledQuantity <= 0)
                    errors.Add("排程數量必須大於0");

                if (entity.CompletedQuantity < 0)
                    errors.Add("已完成數量不可為負數");

                if (entity.CompletedQuantity > entity.ScheduledQuantity)
                    errors.Add("已完成數量不可大於排程數量");

                // 檢查排程是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var scheduleExists = await context.ProductionSchedules
                    .AnyAsync(ps => ps.Id == entity.ProductionScheduleId);

                if (!scheduleExists)
                    errors.Add("生產排程不存在");

                // 檢查品項是否存在
                var productExists = await context.Items
                    .AnyAsync(p => p.Id == entity.ItemId);

                if (!productExists)
                    errors.Add("品項不存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    ScheduleId = entity.ProductionScheduleId,
                    ItemId = entity.ItemId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作 SearchAsync
        public override async Task<List<ProductionScheduleItem>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Where(psi => psi.Item!.Name!.Contains(searchTerm) ||
                                 (psi.Item.Code != null && psi.Item.Code.Contains(searchTerm)) ||
                                 (psi.ProductionSchedule.Code != null && psi.ProductionSchedule.Code.Contains(searchTerm)))
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<ProductionScheduleItem>();
            }
        }

        // === 業務特定方法 ===

        public async Task<List<ProductionScheduleItem>> GetByScheduleIdAsync(int scheduleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.Item)
                        .ThenInclude(p => p.ItemCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentItem)
                    .Where(psi => psi.ProductionScheduleId == scheduleId)
                    .OrderBy(psi => psi.Priority)
                    .ThenBy(psi => psi.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByScheduleIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByScheduleIdAsync),
                    ServiceType = GetType().Name,
                    ScheduleId = scheduleId
                });
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<List<ProductionScheduleItem>> GetByItemIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                    .Where(psi => psi.ItemId == productId)
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByItemIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByItemIdAsync),
                    ServiceType = GetType().Name,
                    ItemId = productId
                });
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<List<ProductionScheduleItem>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Where(psi => psi.SalesOrderDetailId == salesOrderDetailId)
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderDetailIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId
                });
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<List<ProductionScheduleItem>> GetByStatusAsync(ProductionItemStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                            .ThenInclude(so => so.Customer)
                    .Where(psi => psi.ProductionItemStatus == status)
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ThenBy(psi => psi.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByStatusAsync),
                    ServiceType = GetType().Name,
                    Status = status.ToString()
                });
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<List<ProductionScheduleItem>> GetPendingItemsAsync()
        {
            return await GetByStatusAsync(ProductionItemStatus.Pending);
        }

        public async Task<List<ProductionScheduleItem>> GetInProgressItemsAsync()
        {
            return await GetByStatusAsync(ProductionItemStatus.InProgress);
        }

        public async Task<ProductionScheduleItem?> GetWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                        .ThenInclude(p => p.ItemCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                            .ThenInclude(so => so.Customer)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.WarehouseLocation)
                    .Include(psi => psi.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentItem)
                    .Include(psi => psi.ScheduleDetails)
                        .ThenInclude(psd => psd.Warehouse)
                    .Include(psi => psi.Completions)
                    .FirstOrDefaultAsync(psi => psi.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public async Task<ServiceResult> StartProductionAsync(int itemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var item = await context.ProductionScheduleItems
                    .Include(psi => psi.ScheduleDetails)
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (item.ProductionItemStatus != ProductionItemStatus.Pending &&
                    item.ProductionItemStatus != ProductionItemStatus.WaitingMaterial)
                    return ServiceResult.Failure("只有待生產或等待領料的項目可以開始生產");

                // 更新狀態
                item.ProductionItemStatus = ProductionItemStatus.InProgress;
                item.ActualStartDate = DateTime.Now;
                item.UpdatedAt = DateTime.UtcNow;

                // TODO: 扣除組件庫存（加入 InProductionStock）
                // 這部分需要在 Phase 3 實作，需要與 InventoryStockService 整合

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(StartProductionAsync), GetType(), _logger, new
                {
                    Method = nameof(StartProductionAsync),
                    ServiceType = GetType().Name,
                    ItemId = itemId
                });
                return ServiceResult.Failure("開始生產過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CompleteProductionAsync(int itemId, decimal completedQuantity,
            int? warehouseId = null, int? warehouseLocationId = null,
            List<MaterialSettlementDto>? settlements = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (item.ProductionItemStatus == ProductionItemStatus.Completed)
                    return ServiceResult.Failure("此項目已完成");

                if (completedQuantity <= 0)
                    return ServiceResult.Failure("完成數量必須大於0");

                var newTotalCompleted = item.CompletedQuantity + completedQuantity;
                if (newTotalCompleted > item.ScheduledQuantity)
                    return ServiceResult.Failure($"完成數量超過排程數量（排程: {item.ScheduledQuantity}, 已完成: {item.CompletedQuantity}, 本次: {completedQuantity}）");

                // D7：若尚未設定開始時間，自動補設
                if (item.ActualStartDate == null)
                    item.ActualStartDate = DateTime.Now;

                var effectiveWarehouseId = warehouseId ?? item.WarehouseId;
                var effectiveLocationId = warehouseLocationId ?? item.WarehouseLocationId;
                var isLastCompletion = newTotalCompleted >= item.ScheduledQuantity;

                // 取得 BOM 組件明細，計算成品成本（D1/D6：每次完工都計算當下加權平均）
                var schedule = await context.ProductionSchedules
                    .FirstOrDefaultAsync(ps => ps.Id == item.ProductionScheduleId);
                var transactionCode = schedule?.Code ?? $"PS-{item.ProductionScheduleId}";

                var bomDetails = await context.ProductionScheduleDetails
                    .Where(d => d.ProductionScheduleItemId == itemId && d.Status == EntityStatus.Active)
                    .ToListAsync();

                decimal? productionUnitCost = null;

                if (bomDetails.Any())
                {
                    // 對每個 BOM 組件，從所有關聯領料明細計算加權平均成本
                    foreach (var bomDetail in bomDetails)
                    {
                        var relatedIssues = await context.MaterialIssueDetails
                            .Where(d => d.ProductionScheduleDetailId == bomDetail.Id && d.UnitCost.HasValue && d.Status == EntityStatus.Active)
                            .ToListAsync();

                        if (relatedIssues.Any())
                        {
                            var totalQty = relatedIssues.Sum(d => d.IssueQuantity);
                            var totalCost = relatedIssues.Sum(d => d.IssueQuantity * d.UnitCost!.Value);
                            bomDetail.ActualUnitCost = totalQty > 0 ? totalCost / totalQty : null;
                        }
                    }

                    // 計算成品單位成本（物料總成本 / 排程數量）
                    var totalMaterialCost = bomDetails.Sum(d => d.RequiredQuantity * (d.ActualUnitCost ?? 0));
                    productionUnitCost = item.ScheduledQuantity > 0
                        ? totalMaterialCost / item.ScheduledQuantity
                        : null;
                }

                // 寫入成品庫存
                if (effectiveWarehouseId.HasValue)
                {
                    var stockResult = await _inventoryStockService.AddStockAsync(
                        item.ItemId,
                        effectiveWarehouseId.Value,
                        completedQuantity,
                        InventoryTransactionTypeEnum.ProductionCompletion,
                        transactionCode,
                        unitCost: productionUnitCost,
                        locationId: effectiveLocationId,
                        remarks: $"生產完工入庫 排程:{transactionCode}",
                        sourceDocumentType: InventorySourceDocumentTypes.ProductionCompletion,
                        sourceDocumentId: itemId);

                    if (!stockResult.IsSuccess)
                        return ServiceResult.Failure($"寫入成品庫存失敗：{stockResult.ErrorMessage}");
                }

                // 建立完工紀錄
                var completion = new ProductionScheduleCompletion
                {
                    ProductionScheduleItemId = itemId,
                    CompletedQuantity = completedQuantity,
                    CompletionDate = DateTime.Now,
                    ActualUnitCost = productionUnitCost,
                    WarehouseId = effectiveWarehouseId,
                    WarehouseLocationId = effectiveLocationId,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
                context.ProductionScheduleCompletions.Add(completion);

                // 更新項目的已完成數量
                item.CompletedQuantity = newTotalCompleted;
                item.UpdatedAt = DateTime.UtcNow;

                // 最後一次完工：自動結案（D7/§3.6）
                if (isLastCompletion)
                {
                    item.ProductionItemStatus = ProductionItemStatus.Completed;
                    item.ActualEndDate = DateTime.Now;
                    item.IsClosed = true;

                    // 更新 SalesOrderDetail.ProducedQuantity（§4.10）
                    if (item.SalesOrderDetailId.HasValue)
                    {
                        var salesDetail = await context.SalesOrderDetails
                            .FirstOrDefaultAsync(d => d.Id == item.SalesOrderDetailId.Value);
                        if (salesDetail != null)
                            salesDetail.ProducedQuantity += newTotalCompleted;
                    }

                    // 用料結算：退料入庫（§3.5，settlements != null 時執行）
                    if (settlements != null && bomDetails.Any())
                    {
                        foreach (var settlement in settlements)
                        {
                            var bomDetail = bomDetails.FirstOrDefault(d => d.Id == settlement.ProductionScheduleDetailId);
                            if (bomDetail == null) continue;

                            // 寫回結算欄位
                            bomDetail.ActualUsedQty = settlement.ActualUsedQty;
                            bomDetail.ReturnQty = settlement.ReturnQty ?? 0;
                            bomDetail.ReturnWarehouseId = settlement.ReturnWarehouseId;
                            bomDetail.ReturnLocationId = settlement.ReturnLocationId;
                            bomDetail.ScrapQty = settlement.ScrapQty ?? 0;
                            bomDetail.ScrapReason = settlement.ScrapReason;

                            // 退料入庫
                            if (settlement.ReturnQty > 0 && settlement.ReturnWarehouseId.HasValue)
                            {
                                var returnResult = await _inventoryStockService.AddStockAsync(
                                    bomDetail.ComponentItemId,
                                    settlement.ReturnWarehouseId.Value,
                                    settlement.ReturnQty ?? 0,
                                    InventoryTransactionTypeEnum.MaterialReturn,
                                    transactionCode,
                                    locationId: settlement.ReturnLocationId,
                                    remarks: $"生產退料 排程:{transactionCode}",
                                    sourceDocumentType: InventorySourceDocumentTypes.ProductionCompletion,
                                    sourceDocumentId: itemId);

                                if (!returnResult.IsSuccess)
                                    return ServiceResult.Failure($"退料入庫失敗：{returnResult.ErrorMessage}");
                            }
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteProductionAsync), GetType(), _logger, new
                {
                    Method = nameof(CompleteProductionAsync),
                    ServiceType = GetType().Name,
                    ItemId = itemId,
                    CompletedQuantity = completedQuantity
                });
                return ServiceResult.Failure("完成生產過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateItemsFromSalesOrderAsync(int scheduleId, List<ProductionScheduleItem> items)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult.Failure("項目列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程是否存在
                var scheduleExists = await context.ProductionSchedules.AnyAsync(ps => ps.Id == scheduleId);
                if (!scheduleExists)
                    return ServiceResult.Failure("生產排程不存在");

                // 設定排程ID與初始值
                foreach (var item in items)
                {
                    item.ProductionScheduleId = scheduleId;
                    item.ProductionItemStatus = ProductionItemStatus.Pending;
                    item.CompletedQuantity = 0;
                    item.CreatedAt = DateTime.UtcNow;
                    item.Status = EntityStatus.Active;
                }

                // 驗證所有項目
                foreach (var item in items)
                {
                    var validationResult = await ValidateAsync(item);
                    if (!validationResult.IsSuccess)
                        return validationResult;
                }

                await context.ProductionScheduleItems.AddRangeAsync(items);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateItemsFromSalesOrderAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateItemsFromSalesOrderAsync),
                    ServiceType = GetType().Name,
                    ScheduleId = scheduleId,
                    ItemCount = items?.Count ?? 0
                });
                return ServiceResult.Failure("建立排程項目過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateCompletedQuantityAsync(int itemId, decimal completedQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (completedQuantity < 0)
                    return ServiceResult.Failure("已完成數量不可為負數");

                if (completedQuantity > item.ScheduledQuantity)
                    return ServiceResult.Failure("已完成數量不可大於排程數量");

                item.CompletedQuantity = completedQuantity;
                item.UpdatedAt = DateTime.UtcNow;

                // 更新狀態
                if (completedQuantity >= item.ScheduledQuantity)
                {
                    item.ProductionItemStatus = ProductionItemStatus.Completed;
                    item.ActualEndDate ??= DateTime.Now;
                    item.IsClosed = true;
                }
                else if (completedQuantity > 0)
                {
                    item.ProductionItemStatus = ProductionItemStatus.InProgress;
                    item.ActualStartDate ??= DateTime.Now;
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateCompletedQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateCompletedQuantityAsync),
                    ServiceType = GetType().Name,
                    ItemId = itemId,
                    CompletedQuantity = completedQuantity
                });
                return ServiceResult.Failure("更新已完成數量過程發生錯誤");
            }
        }

        public async Task<List<ProductionScheduleItem>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var endOfDay = endDate.Date.AddDays(1).AddTicks(-1);
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                            .ThenInclude(so => so!.Customer)
                    .Include(psi => psi.ScheduleDetails)
                    .Where(psi => psi.PlannedStartDate.HasValue
                               && psi.PlannedStartDate >= startDate.Date
                               && psi.PlannedStartDate <= endOfDay)
                    .OrderBy(psi => psi.PlannedStartDate)
                    .ThenBy(psi => psi.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { startDate, endDate });
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<List<ProductionScheduleItem>> GetUnscheduledAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Item)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                            .ThenInclude(so => so!.Customer)
                    .Where(psi => psi.PlannedStartDate == null && !psi.IsClosed
                               && psi.ProductionItemStatus != ProductionItemStatus.Completed)
                    .OrderBy(psi => psi.SalesOrderDetail!.SalesOrder!.ExpectedDeliveryDate)
                    .ThenBy(psi => psi.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnscheduledAsync), GetType(), _logger, null);
                return new List<ProductionScheduleItem>();
            }
        }

        public async Task<ServiceResult> UpdatePlannedDateAsync(int itemId, DateTime? plannedStartDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.ProductionScheduleItems.FirstOrDefaultAsync(psi => psi.Id == itemId);
                if (item == null)
                    return ServiceResult.Failure("排程項目不存在");

                item.PlannedStartDate = plannedStartDate.HasValue ? plannedStartDate.Value.Date : null;
                item.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePlannedDateAsync), GetType(), _logger, new { itemId, plannedStartDate });
                return ServiceResult.Failure("更新計畫日期時發生錯誤");
            }
        }

        public async Task<Dictionary<int, decimal>> GetScheduledQuantityMapAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Where(psi => psi.SalesOrderDetailId.HasValue)
                    .GroupBy(psi => psi.SalesOrderDetailId!.Value)
                    .Select(g => new { SalesOrderDetailId = g.Key, Total = g.Sum(x => x.ScheduledQuantity) })
                    .ToDictionaryAsync(x => x.SalesOrderDetailId, x => x.Total);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetScheduledQuantityMapAsync), GetType(), _logger, null);
                return new Dictionary<int, decimal>();
            }
        }

        public async Task<Dictionary<int, ProductionItemStatus>> GetAggregateStatusMapAsync(IEnumerable<int> salesOrderDetailIds)
        {
            var ids = salesOrderDetailIds.ToList();
            if (ids.Count == 0) return new Dictionary<int, ProductionItemStatus>();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var rows = await context.ProductionScheduleItems
                    .Where(psi => psi.SalesOrderDetailId.HasValue && ids.Contains(psi.SalesOrderDetailId.Value))
                    .Select(psi => new { psi.SalesOrderDetailId, psi.ProductionItemStatus })
                    .ToListAsync();

                return rows
                    .GroupBy(x => x.SalesOrderDetailId!.Value)
                    .ToDictionary(g => g.Key, g =>
                    {
                        var statuses = g.Select(x => x.ProductionItemStatus).ToList();
                        if (statuses.Any(s => s == ProductionItemStatus.InProgress)) return ProductionItemStatus.InProgress;
                        if (statuses.Any(s => s == ProductionItemStatus.Paused)) return ProductionItemStatus.Paused;
                        if (statuses.Any(s => s == ProductionItemStatus.WaitingMaterial)) return ProductionItemStatus.WaitingMaterial;
                        if (statuses.All(s => s == ProductionItemStatus.Completed || s == ProductionItemStatus.Aborted))
                        {
                            if (statuses.Any(s => s == ProductionItemStatus.Aborted)) return ProductionItemStatus.Aborted;
                            return ProductionItemStatus.Completed;
                        }
                        return ProductionItemStatus.Pending;
                    });
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAggregateStatusMapAsync), GetType(), _logger, null);
                return new Dictionary<int, ProductionItemStatus>();
            }
        }

        public async Task<ServiceResult<bool>> ReturnToSidebarAsync(int itemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var item = await context.ProductionScheduleItems
                    .Include(i => i.ScheduleDetails)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                    return ServiceResult<bool>.Failure("找不到排程項目");

                if (item.CompletedQuantity > 0)
                    return ServiceResult<bool>.Failure("已有完成數量，無法退回待排清單");

                // 已有領料記錄時封鎖退回：庫存已扣除，需先透過停產功能處理物料
                if (item.ScheduleDetails?.Any(d => d.IssuedQuantity > 0) == true)
                    return ServiceResult<bool>.Failure("已有領料記錄，無法退回待排清單。請先使用停產功能處理物料");

                // 扣回 SalesOrderDetail.ScheduledQuantity
                if (item.SalesOrderDetailId.HasValue)
                {
                    var detail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(d => d.Id == item.SalesOrderDetailId.Value);
                    if (detail != null)
                    {
                        detail.ScheduledQuantity = Math.Max(0, detail.ScheduledQuantity - item.ScheduledQuantity);
                    }
                }

                // 刪除明細與主檔
                // 先解除 MaterialIssueDetail 對 ProductionScheduleDetail 的 FK 參照，避免刪除衝突
                if (item.ScheduleDetails?.Any() == true)
                {
                    var detailIds = item.ScheduleDetails.Select(d => d.Id).ToList();
                    var linkedIssueDetails = await context.MaterialIssueDetails
                        .Where(mid => mid.ProductionScheduleDetailId.HasValue
                                   && detailIds.Contains(mid.ProductionScheduleDetailId.Value))
                        .ToListAsync();
                    foreach (var mid in linkedIssueDetails)
                        mid.ProductionScheduleDetailId = null;

                    context.ProductionScheduleDetails.RemoveRange(item.ScheduleDetails);
                }
                context.ProductionScheduleItems.Remove(item);

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(false);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReturnToSidebarAsync), GetType(), _logger, new { itemId });
                return ServiceResult<bool>.Failure("退回待排清單時發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdatePrioritiesAsync(List<(int Id, int Priority)> updates)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var ids = updates.Select(u => u.Id).ToList();
                var items = await context.ProductionScheduleItems
                    .Where(i => ids.Contains(i.Id))
                    .ToListAsync();

                foreach (var item in items)
                {
                    var update = updates.FirstOrDefault(u => u.Id == item.Id);
                    item.Priority = update.Priority;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePrioritiesAsync), GetType(), _logger, new { Count = updates.Count });
                return ServiceResult.Failure("更新優先順序失敗");
            }
        }

        public async Task<(int? WarehouseId, int? LocationId)> GetLastCompletionWarehouseAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var lastCompletion = await context.ProductionScheduleCompletions
                    .Include(c => c.ProductionScheduleItem)
                    .Where(c =>
                        c.ProductionScheduleItem.ItemId == productId &&
                        c.WarehouseId.HasValue &&
                        c.Status == EntityStatus.Active)
                    .OrderByDescending(c => c.CompletionDate)
                    .FirstOrDefaultAsync();

                return (lastCompletion?.WarehouseId, lastCompletion?.WarehouseLocationId);
            }
            catch
            {
                return (null, null);
            }
        }

        public async Task<ServiceResult> PauseProductionAsync(int itemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (item.ProductionItemStatus != ProductionItemStatus.InProgress)
                    return ServiceResult.Failure("只有生產中的項目可以暫停");

                item.ProductionItemStatus = ProductionItemStatus.Paused;
                item.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PauseProductionAsync), GetType(), _logger, new { itemId });
                return ServiceResult.Failure("暫停生產過程發生錯誤");
            }
        }

        public async Task<ServiceResult> ResumeProductionAsync(int itemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (item.ProductionItemStatus != ProductionItemStatus.Paused)
                    return ServiceResult.Failure("只有已暫停的項目可以繼續生產");

                item.ProductionItemStatus = ProductionItemStatus.InProgress;
                item.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResumeProductionAsync), GetType(), _logger, new { itemId });
                return ServiceResult.Failure("繼續生產過程發生錯誤");
            }
        }

        public async Task<ServiceResult> AbortProductionAsync(int itemId, List<MaterialSettlementDto>? settlements = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == itemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                if (item.ProductionItemStatus == ProductionItemStatus.Completed)
                    return ServiceResult.Failure("此項目已完成，無法終止");

                // 允許 InProgress、Paused、WaitingMaterial、Pending（含已領料情境）
                if (item.ProductionItemStatus != ProductionItemStatus.InProgress &&
                    item.ProductionItemStatus != ProductionItemStatus.Paused &&
                    item.ProductionItemStatus != ProductionItemStatus.WaitingMaterial &&
                    item.ProductionItemStatus != ProductionItemStatus.Pending)
                    return ServiceResult.Failure("此狀態的項目無法終止");

                var schedule = await context.ProductionSchedules
                    .FirstOrDefaultAsync(ps => ps.Id == item.ProductionScheduleId);
                var transactionCode = schedule?.Code ?? $"PS-{item.ProductionScheduleId}";

                // 用料結算：退料入庫
                if (settlements != null && settlements.Any())
                {
                    var bomDetails = await context.ProductionScheduleDetails
                        .Where(d => d.ProductionScheduleItemId == itemId && d.Status == EntityStatus.Active)
                        .ToListAsync();

                    foreach (var settlement in settlements)
                    {
                        var bomDetail = bomDetails.FirstOrDefault(d => d.Id == settlement.ProductionScheduleDetailId);
                        if (bomDetail == null) continue;

                        bomDetail.ActualUsedQty = settlement.ActualUsedQty;
                        bomDetail.ReturnQty = settlement.ReturnQty ?? 0;
                        bomDetail.ReturnWarehouseId = settlement.ReturnWarehouseId;
                        bomDetail.ReturnLocationId = settlement.ReturnLocationId;
                        bomDetail.ScrapQty = settlement.ScrapQty ?? 0;
                        bomDetail.ScrapReason = settlement.ScrapReason;

                        if (settlement.ReturnQty > 0 && settlement.ReturnWarehouseId.HasValue)
                        {
                            var returnResult = await _inventoryStockService.AddStockAsync(
                                bomDetail.ComponentItemId,
                                settlement.ReturnWarehouseId.Value,
                                settlement.ReturnQty ?? 0,
                                InventoryTransactionTypeEnum.MaterialReturn,
                                transactionCode,
                                locationId: settlement.ReturnLocationId,
                                remarks: $"停產退料 排程:{transactionCode}",
                                sourceDocumentType: InventorySourceDocumentTypes.ProductionCompletion,
                                sourceDocumentId: itemId);

                            if (!returnResult.IsSuccess)
                                return ServiceResult.Failure($"退料入庫失敗：{returnResult.ErrorMessage}");
                        }
                    }
                }

                // 更新 SalesOrderDetail.ProducedQuantity（累計部分已完工數量）
                if (item.SalesOrderDetailId.HasValue && item.CompletedQuantity > 0)
                {
                    var salesDetail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(d => d.Id == item.SalesOrderDetailId.Value);
                    if (salesDetail != null)
                        salesDetail.ProducedQuantity += item.CompletedQuantity;
                }

                // 結案（不改 ScheduledQuantity，確保不重出現在待排 Sidebar）
                item.ProductionItemStatus = ProductionItemStatus.Aborted;
                item.ActualEndDate = DateTime.Now;
                item.IsClosed = true;
                item.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AbortProductionAsync), GetType(), _logger, new { itemId });
                return ServiceResult.Failure("停產過程發生錯誤");
            }
        }

        // 覆寫刪除前檢查
        protected override async Task<ServiceResult> CanDeleteAsync(ProductionScheduleItem entity)
        {
            try
            {
                if (entity.ProductionItemStatus != ProductionItemStatus.Pending)
                    return ServiceResult.Failure("只有待生產的項目可以刪除");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否有完成紀錄
                var hasCompletions = await context.ProductionScheduleCompletions
                    .AnyAsync(psc => psc.ProductionScheduleItemId == entity.Id);

                if (hasCompletions)
                    return ServiceResult.Failure("無法刪除，此項目已有完成入庫紀錄");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }
    }
}
