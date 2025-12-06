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
    /// 生產完成入庫服務實作
    /// </summary>
    public class ProductionScheduleCompletionService : GenericManagementService<ProductionScheduleCompletion>, IProductionScheduleCompletionService
    {
        public ProductionScheduleCompletionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public ProductionScheduleCompletionService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionScheduleCompletion>> logger) : base(contextFactory, logger)
        {
        }

        // 覆寫 GetAllAsync 以包含相關資料
        public override async Task<List<ProductionScheduleCompletion>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psc => psc.Warehouse)
                    .Include(psc => psc.WarehouseLocation)
                    .Include(psc => psc.CompletedByEmployee)
                    .OrderByDescending(psc => psc.CompletionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductionScheduleCompletion>();
            }
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionScheduleCompletion?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psc => psc.Warehouse)
                    .Include(psc => psc.WarehouseLocation)
                    .Include(psc => psc.CompletedByEmployee)
                    .Include(psc => psc.InventoryTransaction)
                    .FirstOrDefaultAsync(psc => psc.Id == id);
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
        public override async Task<ServiceResult> ValidateAsync(ProductionScheduleCompletion entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ProductionScheduleItemId <= 0)
                    errors.Add("生產排程項目為必填");

                if (entity.CompletedQuantity <= 0)
                    errors.Add("完成數量必須大於0");

                // 檢查排程項目是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == entity.ProductionScheduleItemId);

                if (item == null)
                {
                    errors.Add("生產排程項目不存在");
                }
                else
                {
                    // 檢查完成數量是否超過剩餘數量
                    var existingCompleted = await context.ProductionScheduleCompletions
                        .Where(psc => psc.ProductionScheduleItemId == entity.ProductionScheduleItemId && psc.Id != entity.Id)
                        .SumAsync(psc => psc.CompletedQuantity);

                    var totalAfterThis = existingCompleted + entity.CompletedQuantity;
                    if (totalAfterThis > item.ScheduledQuantity)
                        errors.Add($"完成數量超過排程數量（排程: {item.ScheduledQuantity}, 已完成: {existingCompleted}, 本次: {entity.CompletedQuantity}）");
                }

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
                    ScheduleItemId = entity.ProductionScheduleItemId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作 SearchAsync
        public override async Task<List<ProductionScheduleCompletion>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psc => psc.Warehouse)
                    .Where(psc => psc.ProductionScheduleItem.Product.Name.Contains(searchTerm) ||
                                 (psc.ProductionScheduleItem.Product.Code != null && 
                                  psc.ProductionScheduleItem.Product.Code.Contains(searchTerm)) ||
                                 (psc.BatchNumber != null && psc.BatchNumber.Contains(searchTerm)))
                    .OrderByDescending(psc => psc.CompletionDate)
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
                return new List<ProductionScheduleCompletion>();
            }
        }

        // === 業務特定方法 ===

        public async Task<List<ProductionScheduleCompletion>> GetByScheduleItemIdAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.Warehouse)
                    .Include(psc => psc.WarehouseLocation)
                    .Include(psc => psc.CompletedByEmployee)
                    .Where(psc => psc.ProductionScheduleItemId == scheduleItemId)
                    .OrderByDescending(psc => psc.CompletionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByScheduleItemIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByScheduleItemIdAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId
                });
                return new List<ProductionScheduleCompletion>();
            }
        }

        public async Task<List<ProductionScheduleCompletion>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psc => psc.Warehouse)
                    .Where(psc => psc.CompletionDate >= startDate && psc.CompletionDate <= endDate)
                    .OrderByDescending(psc => psc.CompletionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<ProductionScheduleCompletion>();
            }
        }

        public async Task<List<ProductionScheduleCompletion>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Include(psc => psc.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psc => psc.Warehouse)
                    .Where(psc => psc.WarehouseId == warehouseId)
                    .OrderByDescending(psc => psc.CompletionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByWarehouseIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = warehouseId
                });
                return new List<ProductionScheduleCompletion>();
            }
        }

        public async Task<decimal> GetTotalCompletedQuantityAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleCompletions
                    .Where(psc => psc.ProductionScheduleItemId == scheduleItemId)
                    .SumAsync(psc => psc.CompletedQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalCompletedQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTotalCompletedQuantityAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId
                });
                return 0;
            }
        }

        public async Task<ServiceResult> CreateCompletionAsync(ProductionScheduleCompletion completion)
        {
            try
            {
                // 先驗證
                var validationResult = await ValidateAsync(completion);
                if (!validationResult.IsSuccess)
                    return validationResult;

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得對應的排程項目
                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == completion.ProductionScheduleItemId);

                if (item == null)
                    return ServiceResult.Failure("生產排程項目不存在");

                // 設定預設值
                completion.CreatedAt = DateTime.Now;
                completion.Status = EntityStatus.Active;
                if (completion.CompletionDate == default)
                    completion.CompletionDate = DateTime.Now;

                // 新增完成紀錄
                context.ProductionScheduleCompletions.Add(completion);

                // 更新項目的已完成數量
                item.CompletedQuantity += completion.CompletedQuantity;
                item.UpdatedAt = DateTime.Now;

                // 更新狀態
                if (item.CompletedQuantity >= item.ScheduledQuantity)
                {
                    item.ProductionItemStatus = ProductionItemStatus.Completed;
                    item.ActualEndDate ??= DateTime.Now;
                }
                else if (item.CompletedQuantity > 0 && item.ProductionItemStatus == ProductionItemStatus.Pending)
                {
                    item.ProductionItemStatus = ProductionItemStatus.InProgress;
                    item.ActualStartDate ??= DateTime.Now;
                }

                // TODO: 增加成品庫存
                // 這部分需要與 InventoryStockService 整合

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateCompletionAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateCompletionAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = completion.ProductionScheduleItemId,
                    CompletedQuantity = completion.CompletedQuantity
                });
                return ServiceResult.Failure("建立完成紀錄過程發生錯誤");
            }
        }
    }
}
