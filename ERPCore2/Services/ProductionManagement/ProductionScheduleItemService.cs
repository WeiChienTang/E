using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        public ProductionScheduleItemService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public ProductionScheduleItemService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionScheduleItem>> logger) : base(contextFactory, logger)
        {
        }

        // 覆寫 GetAllAsync 以包含相關資料
        public override async Task<List<ProductionScheduleItem>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Product)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ThenBy(psi => psi.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductionScheduleItem>();
            }
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionScheduleItem?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.WarehouseLocation)
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

                if (entity.ProductId <= 0)
                    errors.Add("商品為必填");

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

                // 檢查商品是否存在
                var productExists = await context.Products
                    .AnyAsync(p => p.Id == entity.ProductId);

                if (!productExists)
                    errors.Add("商品不存在");

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
                    ProductId = entity.ProductId
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
                    .Include(psi => psi.Product)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Where(psi => psi.Product.Name.Contains(searchTerm) ||
                                 (psi.Product.Code != null && psi.Product.Code.Contains(searchTerm)) ||
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
                    .Include(psi => psi.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentProduct)
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

        public async Task<List<ProductionScheduleItem>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleItems
                    .Include(psi => psi.ProductionSchedule)
                    .Include(psi => psi.Product)
                    .Include(psi => psi.SalesOrderDetail)
                    .Where(psi => psi.ProductId == productId)
                    .OrderByDescending(psi => psi.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId
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
                    .Include(psi => psi.Product)
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
                    .Include(psi => psi.Product)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
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
                    .Include(psi => psi.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psi => psi.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                            .ThenInclude(so => so.Customer)
                    .Include(psi => psi.Warehouse)
                    .Include(psi => psi.WarehouseLocation)
                    .Include(psi => psi.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentProduct)
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

                if (item.ProductionItemStatus != ProductionItemStatus.Pending)
                    return ServiceResult.Failure("只有待生產的項目可以開始生產");

                // 更新狀態
                item.ProductionItemStatus = ProductionItemStatus.InProgress;
                item.ActualStartDate = DateTime.Now;
                item.UpdatedAt = DateTime.Now;

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

        public async Task<ServiceResult> CompleteProductionAsync(int itemId, decimal completedQuantity, int? warehouseId = null, int? warehouseLocationId = null)
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

                // 建立完成入庫紀錄
                var completion = new ProductionScheduleCompletion
                {
                    ProductionScheduleItemId = itemId,
                    CompletedQuantity = completedQuantity,
                    CompletionDate = DateTime.Now,
                    WarehouseId = warehouseId ?? item.WarehouseId,
                    WarehouseLocationId = warehouseLocationId ?? item.WarehouseLocationId,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                };

                context.ProductionScheduleCompletions.Add(completion);

                // 更新項目的已完成數量
                item.CompletedQuantity = newTotalCompleted;
                item.UpdatedAt = DateTime.Now;

                // 檢查是否全部完成
                if (item.CompletedQuantity >= item.ScheduledQuantity)
                {
                    item.ProductionItemStatus = ProductionItemStatus.Completed;
                    item.ActualEndDate = DateTime.Now;
                }

                // TODO: 增加成品庫存、扣除 InProductionStock
                // 這部分需要在 Phase 3 實作，需要與 InventoryStockService 整合

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
                    item.CreatedAt = DateTime.Now;
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
                item.UpdatedAt = DateTime.Now;

                // 更新狀態
                if (completedQuantity >= item.ScheduledQuantity)
                {
                    item.ProductionItemStatus = ProductionItemStatus.Completed;
                    item.ActualEndDate ??= DateTime.Now;
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
