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
    /// 生產排程明細服務實作
    /// 明細現在關聯到 ProductionScheduleItem（生產項目）
    /// </summary>
    public class ProductionScheduleDetailService : GenericManagementService<ProductionScheduleDetail>, IProductionScheduleDetailService
    {
        public ProductionScheduleDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionScheduleDetail>> logger) : base(contextFactory, logger)
        {
        }

        public ProductionScheduleDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以包含相關資料
        public override async Task<List<ProductionScheduleDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .OrderBy(psd => psd.ProductionScheduleItemId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductionScheduleDetail>();
            }
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionScheduleDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .FirstOrDefaultAsync(psd => psd.Id == id);
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

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(ProductionScheduleDetail entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ProductionScheduleItemId <= 0)
                    errors.Add("生產排程項目為必填");

                if (entity.ComponentProductId <= 0)
                    errors.Add("組件商品為必填");

                if (entity.RequiredQuantity <= 0)
                    errors.Add("需求數量必須大於0");

                if (entity.EstimatedUnitCost.HasValue && entity.EstimatedUnitCost.Value < 0)
                    errors.Add("預估單位成本不可為負數");

                if (entity.ActualUnitCost.HasValue && entity.ActualUnitCost.Value < 0)
                    errors.Add("實際單位成本不可為負數");

                if (entity.TotalCost.HasValue && entity.TotalCost.Value < 0)
                    errors.Add("總成本不可為負數");

                // 檢查排程項目是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var itemExists = await context.ProductionScheduleItems
                    .AnyAsync(psi => psi.Id == entity.ProductionScheduleItemId);

                if (!itemExists)
                    errors.Add("生產排程項目不存在");

                // 檢查商品是否存在
                var productExists = await context.Products
                    .AnyAsync(p => p.Id == entity.ComponentProductId);

                if (!productExists)
                    errors.Add("組件商品不存在");

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
                    ScheduleItemId = entity.ProductionScheduleItemId,
                    ProductId = entity.ComponentProductId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<ProductionScheduleDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ComponentProduct.Name.Contains(searchTerm) ||
                                 (psd.ComponentProduct.Code != null && psd.ComponentProduct.Code.Contains(searchTerm)) ||
                                 (psd.ProductionScheduleItem.ProductionSchedule.Code != null && 
                                  psd.ProductionScheduleItem.ProductionSchedule.Code.Contains(searchTerm)))
                    .OrderBy(psd => psd.ProductionScheduleItemId)
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
                return new List<ProductionScheduleDetail>();
            }
        }

        // 業務特定方法 - 根據生產項目取得明細
        public async Task<List<ProductionScheduleDetail>> GetByScheduleItemIdAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ComponentProduct)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ProductionScheduleItemId == scheduleItemId)
                    .OrderBy(psd => psd.Id)
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
                return new List<ProductionScheduleDetail>();
            }
        }

        public async Task<List<ProductionScheduleDetail>> GetByComponentProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ComponentProductId == productId)
                    .OrderByDescending(psd => psd.ProductionScheduleItem.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByComponentProductIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByComponentProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId
                });
                return new List<ProductionScheduleDetail>();
            }
        }

        public async Task<List<ProductionScheduleDetail>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.WarehouseId == warehouseId)
                    .OrderByDescending(psd => psd.ProductionScheduleItem.ProductionSchedule.ScheduleDate)
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
                return new List<ProductionScheduleDetail>();
            }
        }

        public async Task<ServiceResult> CreateDetailsForItemAsync(int scheduleItemId, List<ProductionScheduleDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Failure("明細列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程項目是否存在
                var itemExists = await context.ProductionScheduleItems.AnyAsync(psi => psi.Id == scheduleItemId);
                if (!itemExists)
                    return ServiceResult.Failure("生產排程項目不存在");

                // 設定排程項目ID
                foreach (var detail in details)
                {
                    detail.ProductionScheduleItemId = scheduleItemId;
                    detail.CreatedAt = DateTime.Now;
                    detail.Status = EntityStatus.Active;
                }

                // 驗證所有明細
                foreach (var detail in details)
                {
                    var validationResult = await ValidateAsync(detail);
                    if (!validationResult.IsSuccess)
                        return validationResult;
                }

                await context.ProductionScheduleDetails.AddRangeAsync(details);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateDetailsForItemAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateDetailsForItemAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId,
                    DetailCount = details?.Count ?? 0
                });
                return ServiceResult.Failure("建立明細過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateDetailsForItemAsync(int scheduleItemId, List<ProductionScheduleDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Failure("明細列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程項目是否存在
                var itemExists = await context.ProductionScheduleItems.AnyAsync(psi => psi.Id == scheduleItemId);
                if (!itemExists)
                    return ServiceResult.Failure("生產排程項目不存在");

                // 刪除舊明細
                var oldDetails = await context.ProductionScheduleDetails
                    .Where(psd => psd.ProductionScheduleItemId == scheduleItemId)
                    .ToListAsync();
                
                context.ProductionScheduleDetails.RemoveRange(oldDetails);

                // 新增新明細
                foreach (var detail in details)
                {
                    detail.ProductionScheduleItemId = scheduleItemId;
                    detail.CreatedAt = DateTime.Now;
                    detail.Status = EntityStatus.Active;
                }

                // 驗證所有明細
                foreach (var detail in details)
                {
                    var validationResult = await ValidateAsync(detail);
                    if (!validationResult.IsSuccess)
                        return validationResult;
                }

                await context.ProductionScheduleDetails.AddRangeAsync(details);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsForItemAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateDetailsForItemAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId,
                    DetailCount = details?.Count ?? 0
                });
                return ServiceResult.Failure("更新明細過程發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteByScheduleItemIdAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var details = await context.ProductionScheduleDetails
                    .Where(psd => psd.ProductionScheduleItemId == scheduleItemId)
                    .ToListAsync();

                if (details.Any())
                {
                    context.ProductionScheduleDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByScheduleItemIdAsync), GetType(), _logger, new
                {
                    Method = nameof(DeleteByScheduleItemIdAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId
                });
                return ServiceResult.Failure("刪除明細過程發生錯誤");
            }
        }
    }
}
