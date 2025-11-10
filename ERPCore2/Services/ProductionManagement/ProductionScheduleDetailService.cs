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
                    .Include(psd => psd.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .OrderBy(psd => psd.ProductionScheduleId)
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
                    .Include(psd => psd.ProductionSchedule)
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

                if (entity.ProductionScheduleId <= 0)
                    errors.Add("生產排程主檔為必填");

                if (entity.ComponentProductId <= 0)
                    errors.Add("組件產品為必填");

                if (entity.RequiredQuantity <= 0)
                    errors.Add("需求數量必須大於0");

                if (entity.EstimatedUnitCost.HasValue && entity.EstimatedUnitCost.Value < 0)
                    errors.Add("預估單位成本不可為負數");

                if (entity.ActualUnitCost.HasValue && entity.ActualUnitCost.Value < 0)
                    errors.Add("實際單位成本不可為負數");

                if (entity.TotalCost.HasValue && entity.TotalCost.Value < 0)
                    errors.Add("總成本不可為負數");

                // 檢查排程是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var scheduleExists = await context.ProductionSchedules
                    .AnyAsync(ps => ps.Id == entity.ProductionScheduleId);

                if (!scheduleExists)
                    errors.Add("生產排程不存在");

                // 檢查產品是否存在
                var productExists = await context.Products
                    .AnyAsync(p => p.Id == entity.ComponentProductId);

                if (!productExists)
                    errors.Add("組件產品不存在");

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
                    .Include(psd => psd.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ComponentProduct.Name.Contains(searchTerm) ||
                                 (psd.ComponentProduct.Code != null && psd.ComponentProduct.Code.Contains(searchTerm)) ||
                                 (psd.ProductionSchedule.Code != null && psd.ProductionSchedule.Code.Contains(searchTerm)))
                    .OrderBy(psd => psd.ProductionScheduleId)
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

        // 業務特定方法
        public async Task<List<ProductionScheduleDetail>> GetByScheduleIdAsync(int scheduleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ComponentProduct)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(psd => psd.ProductCompositionDetail)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ProductionScheduleId == scheduleId)
                    .OrderBy(psd => psd.Id)
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
                return new List<ProductionScheduleDetail>();
            }
        }

        public async Task<List<ProductionScheduleDetail>> GetByComponentProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.ComponentProductId == productId)
                    .OrderByDescending(psd => psd.ProductionSchedule.ScheduleDate)
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
                    .Include(psd => psd.ProductionSchedule)
                    .Include(psd => psd.ComponentProduct)
                    .Include(psd => psd.Warehouse)
                    .Where(psd => psd.WarehouseId == warehouseId)
                    .OrderByDescending(psd => psd.ProductionSchedule.ScheduleDate)
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

        public async Task<ServiceResult> CreateDetailsAsync(int scheduleId, List<ProductionScheduleDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Failure("明細列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程是否存在
                var scheduleExists = await context.ProductionSchedules.AnyAsync(ps => ps.Id == scheduleId);
                if (!scheduleExists)
                    return ServiceResult.Failure("生產排程不存在");

                // 設定排程ID
                foreach (var detail in details)
                {
                    detail.ProductionScheduleId = scheduleId;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateDetailsAsync),
                    ServiceType = GetType().Name,
                    ScheduleId = scheduleId,
                    DetailCount = details?.Count ?? 0
                });
                return ServiceResult.Failure("建立明細過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateDetailsAsync(int scheduleId, List<ProductionScheduleDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Failure("明細列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程是否存在
                var scheduleExists = await context.ProductionSchedules.AnyAsync(ps => ps.Id == scheduleId);
                if (!scheduleExists)
                    return ServiceResult.Failure("生產排程不存在");

                // 刪除舊明細
                var oldDetails = await context.ProductionScheduleDetails
                    .Where(psd => psd.ProductionScheduleId == scheduleId)
                    .ToListAsync();
                
                context.ProductionScheduleDetails.RemoveRange(oldDetails);

                // 新增新明細
                foreach (var detail in details)
                {
                    detail.ProductionScheduleId = scheduleId;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    ScheduleId = scheduleId,
                    DetailCount = details?.Count ?? 0
                });
                return ServiceResult.Failure("更新明細過程發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteByScheduleIdAsync(int scheduleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var details = await context.ProductionScheduleDetails
                    .Where(psd => psd.ProductionScheduleId == scheduleId)
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByScheduleIdAsync), GetType(), _logger, new
                {
                    Method = nameof(DeleteByScheduleIdAsync),
                    ServiceType = GetType().Name,
                    ScheduleId = scheduleId
                });
                return ServiceResult.Failure("刪除明細過程發生錯誤");
            }
        }
    }
}
