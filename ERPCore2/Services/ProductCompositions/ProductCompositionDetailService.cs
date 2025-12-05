using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品合成明細服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductCompositionDetailService : GenericManagementService<ProductCompositionDetail>, IProductCompositionDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ProductCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductCompositionDetail>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ProductCompositionDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<ServiceResult<ProductCompositionDetail>> CreateAsync(ProductCompositionDetail entity)
        {
            try
            {
                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductCompositionDetail>.Failure(validationResult.ErrorMessage);
                }

                // 設定建立資訊
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                
                if (entity.Status == default)
                {
                    entity.Status = EntityStatus.Active;
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 清除 Navigation Properties，只保留外鍵 ID（避免 EF Core 追蹤錯誤）
                entity.ProductComposition = null!;
                entity.ComponentProduct = null!;
                entity.Unit = null;
                
                var dbSet = context.Set<ProductCompositionDetail>();
                dbSet.Add(entity);
                await context.SaveChangesAsync();

                return ServiceResult<ProductCompositionDetail>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    ProductCompositionId = entity.ProductCompositionId,
                    ComponentProductId = entity.ComponentProductId,
                    Quantity = entity.Quantity,
                    UnitId = entity.UnitId
                });
                
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<ProductCompositionDetail>.Failure($"建立資料時發生錯誤: {innerMessage}");
            }
        }

        public override async Task<ServiceResult<ProductCompositionDetail>> UpdateAsync(ProductCompositionDetail entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<ProductCompositionDetail>();
                
                // 檢查實體是否存在
                var existingEntity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                if (existingEntity == null)
                {
                    return ServiceResult<ProductCompositionDetail>.Failure("找不到要更新的資料");
                }

                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductCompositionDetail>.Failure(validationResult.ErrorMessage);
                }

                // 保持原建立資訊
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;
                
                // 更新時間
                entity.UpdatedAt = DateTime.UtcNow;

                // 清除 Navigation Properties，只保留外鍵 ID（避免 EF Core 追蹤錯誤）
                entity.ProductComposition = null!;
                entity.ComponentProduct = null!;
                entity.Unit = null;

                // 分離舊實體並附加新實體
                context.Entry(existingEntity).State = EntityState.Detached;
                context.Entry(entity).State = EntityState.Modified;
                
                await context.SaveChangesAsync();

                return ServiceResult<ProductCompositionDetail>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new
                {
                    Id = entity.Id,
                    ProductCompositionId = entity.ProductCompositionId,
                    ComponentProductId = entity.ComponentProductId,
                    Quantity = entity.Quantity,
                    UnitId = entity.UnitId
                });
                
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<ProductCompositionDetail>.Failure($"更新資料時發生錯誤: {innerMessage}");
            }
        }

        public override async Task<List<ProductCompositionDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCompositionDetails
                    .Include(pcd => pcd.ProductComposition)
                        .ThenInclude(pc => pc.ParentProduct)
                    .Include(pcd => pcd.ComponentProduct)
                    .Include(pcd => pcd.Unit)
                    .OrderBy(pcd => pcd.ProductCompositionId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductCompositionDetail>();
            }
        }

        public override async Task<ProductCompositionDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCompositionDetails
                    .Include(pcd => pcd.ProductComposition)
                        .ThenInclude(pc => pc.ParentProduct)
                    .Include(pcd => pcd.ComponentProduct)
                    .Include(pcd => pcd.Unit)
                    .FirstOrDefaultAsync(pcd => pcd.Id == id);
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

        public override async Task<List<ProductCompositionDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductCompositionDetails
                    .Include(pcd => pcd.ProductComposition)
                        .ThenInclude(pc => pc.ParentProduct)
                    .Include(pcd => pcd.ComponentProduct)
                    .Include(pcd => pcd.Unit)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(pcd =>
                        pcd.ComponentProduct.Name.ToLower().Contains(lowerSearchTerm) ||
                        (pcd.ComponentProduct.Code != null && pcd.ComponentProduct.Code.ToLower().Contains(lowerSearchTerm)));
                }

                return await query
                    .OrderBy(pcd => pcd.ProductCompositionId)
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
                return new List<ProductCompositionDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ProductCompositionDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (entity.ProductCompositionId <= 0)
                    errors.Add("請選擇商品合成主檔");

                if (entity.ComponentProductId <= 0)
                    errors.Add("請選擇組件商品");

                if (entity.Quantity <= 0)
                    errors.Add("所需數量必須大於 0");

                // 組件重複驗證（修正：新增和更新都要正確排除自己）
                var excludeId = entity.Id > 0 ? entity.Id : (int?)null;
                if (await IsComponentExistsInCompositionAsync(entity.ProductCompositionId, entity.ComponentProductId, excludeId))
                    errors.Add("此組件已存在於配方中");

                // 防止循環參考：組件不能是成品本身
                using var context = await _contextFactory.CreateDbContextAsync();
                var composition = await context.ProductCompositions.FindAsync(entity.ProductCompositionId);
                if (composition != null && composition.ParentProductId == entity.ComponentProductId)
                    errors.Add("組件不能是成品本身（會造成循環參考）");

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
                    CompositionId = entity.ProductCompositionId,
                    ComponentProductId = entity.ComponentProductId
                });
                return ServiceResult.Failure($"驗證商品合成明細時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region IProductCompositionDetailService 實作

        public async Task<List<ProductCompositionDetail>> GetDetailsByCompositionIdAsync(int compositionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCompositionDetails
                    .Include(pcd => pcd.ComponentProduct)
                    .Include(pcd => pcd.Unit)
                    .Where(pcd => pcd.ProductCompositionId == compositionId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailsByCompositionIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDetailsByCompositionIdAsync),
                    ServiceType = GetType().Name,
                    CompositionId = compositionId
                });
                return new List<ProductCompositionDetail>();
            }
        }

        public async Task<bool> IsComponentExistsInCompositionAsync(int compositionId, int componentProductId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductCompositionDetails
                    .Where(pcd => pcd.ProductCompositionId == compositionId && pcd.ComponentProductId == componentProductId);

                if (excludeId.HasValue)
                    query = query.Where(pcd => pcd.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsComponentExistsInCompositionAsync), GetType(), _logger, new
                {
                    Method = nameof(IsComponentExistsInCompositionAsync),
                    ServiceType = GetType().Name,
                    CompositionId = compositionId,
                    ComponentProductId = componentProductId,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<decimal> CalculateActualQuantityAsync(int detailId, decimal productionQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.ProductCompositionDetails
                    .Include(pcd => pcd.ProductComposition)
                    .FirstOrDefaultAsync(pcd => pcd.Id == detailId);

                if (detail == null)
                    return 0;

                // 計算實際用料（直接使用組件數量乘以生產數量）
                var actualQuantity = detail.Quantity * productionQuantity;

                return actualQuantity;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateActualQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(CalculateActualQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId,
                    ProductionQuantity = productionQuantity
                });
                return 0;
            }
        }

        public async Task<List<ProductComposition>> GetCompositionsUsingComponentAsync(int componentProductId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCompositionDetails
                    .Include(pcd => pcd.ProductComposition)
                        .ThenInclude(pc => pc.ParentProduct)
                    .Where(pcd => pcd.ComponentProductId == componentProductId)
                    .Select(pcd => pcd.ProductComposition)
                    .Distinct()
                    .OrderBy(pc => pc.ParentProductId)
                    .ThenBy(pc => pc.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompositionsUsingComponentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetCompositionsUsingComponentAsync),
                    ServiceType = GetType().Name,
                    ComponentProductId = componentProductId
                });
                return new List<ProductComposition>();
            }
        }

        #endregion
    }
}
