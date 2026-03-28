using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項合成明細服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ItemCompositionDetailService : GenericManagementService<ItemCompositionDetail>, IItemCompositionDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ItemCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ItemCompositionDetail>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ItemCompositionDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<ServiceResult<ItemCompositionDetail>> CreateAsync(ItemCompositionDetail entity)
        {
            try
            {
                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ItemCompositionDetail>.Failure(validationResult.ErrorMessage);
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
                entity.ItemComposition = null!;
                entity.ComponentItem = null!;
                entity.Unit = null;
                
                var dbSet = context.Set<ItemCompositionDetail>();
                dbSet.Add(entity);
                await context.SaveChangesAsync();

                return ServiceResult<ItemCompositionDetail>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    ItemCompositionId = entity.ItemCompositionId,
                    ComponentItemId = entity.ComponentItemId,
                    Quantity = entity.Quantity,
                    UnitId = entity.UnitId
                });
                
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<ItemCompositionDetail>.Failure($"建立資料時發生錯誤: {innerMessage}");
            }
        }

        public override async Task<ServiceResult<ItemCompositionDetail>> UpdateAsync(ItemCompositionDetail entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<ItemCompositionDetail>();
                
                // 檢查實體是否存在
                var existingEntity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                if (existingEntity == null)
                {
                    return ServiceResult<ItemCompositionDetail>.Failure("找不到要更新的資料");
                }

                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ItemCompositionDetail>.Failure(validationResult.ErrorMessage);
                }

                // 保持原建立資訊
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;
                
                // 更新時間
                entity.UpdatedAt = DateTime.UtcNow;

                // 清除 Navigation Properties，只保留外鍵 ID（避免 EF Core 追蹤錯誤）
                entity.ItemComposition = null!;
                entity.ComponentItem = null!;
                entity.Unit = null;

                // 分離舊實體並附加新實體
                context.Entry(existingEntity).State = EntityState.Detached;
                context.Entry(entity).State = EntityState.Modified;
                
                await context.SaveChangesAsync();

                return ServiceResult<ItemCompositionDetail>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new
                {
                    Id = entity.Id,
                    ItemCompositionId = entity.ItemCompositionId,
                    ComponentItemId = entity.ComponentItemId,
                    Quantity = entity.Quantity,
                    UnitId = entity.UnitId
                });
                
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<ItemCompositionDetail>.Failure($"更新資料時發生錯誤: {innerMessage}");
            }
        }

        protected override IQueryable<ItemCompositionDetail> BuildGetAllQuery(AppDbContext context)
        {
            return context.ItemCompositionDetails
                .Include(pcd => pcd.ItemComposition)
                    .ThenInclude(pc => pc.ParentItem)
                .Include(pcd => pcd.ComponentItem)
                .Include(pcd => pcd.Unit)
                .OrderBy(pcd => pcd.ItemCompositionId);
        }

        public override async Task<ItemCompositionDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositionDetails
                    .Include(pcd => pcd.ItemComposition)
                        .ThenInclude(pc => pc.ParentItem)
                    .Include(pcd => pcd.ComponentItem)
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

        public override async Task<List<ItemCompositionDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCompositionDetails
                    .Include(pcd => pcd.ItemComposition)
                        .ThenInclude(pc => pc.ParentItem)
                    .Include(pcd => pcd.ComponentItem)
                    .Include(pcd => pcd.Unit)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(pcd =>
                        pcd.ComponentItem!.Name!.ToLower().Contains(lowerSearchTerm) ||
                        (pcd.ComponentItem.Code != null && pcd.ComponentItem.Code.ToLower().Contains(lowerSearchTerm)));
                }

                return await query
                    .OrderBy(pcd => pcd.ItemCompositionId)
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
                return new List<ItemCompositionDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ItemCompositionDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (entity.ItemCompositionId <= 0)
                    errors.Add("請選擇品項合成主檔");

                if (entity.ComponentItemId <= 0)
                    errors.Add("請選擇組件品項");

                if (entity.Quantity <= 0)
                    errors.Add("所需數量必須大於 0");

                // 組件重複驗證（修正：新增和更新都要正確排除自己）
                var excludeId = entity.Id > 0 ? entity.Id : (int?)null;
                if (await IsComponentExistsInCompositionAsync(entity.ItemCompositionId, entity.ComponentItemId, excludeId))
                    errors.Add("此組件已存在於配方中");

                // 防止循環參考：組件不能是成品本身
                using var context = await _contextFactory.CreateDbContextAsync();
                var composition = await context.ItemCompositions.FindAsync(entity.ItemCompositionId);
                if (composition != null && composition.ParentItemId == entity.ComponentItemId)
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
                    CompositionId = entity.ItemCompositionId,
                    ComponentItemId = entity.ComponentItemId
                });
                return ServiceResult.Failure($"驗證品項合成明細時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region IItemCompositionDetailService 實作

        public async Task<List<ItemCompositionDetail>> GetDetailsByCompositionIdAsync(int compositionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositionDetails
                    .Include(pcd => pcd.ComponentItem)
                    .Include(pcd => pcd.Unit)
                    .Where(pcd => pcd.ItemCompositionId == compositionId)
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
                return new List<ItemCompositionDetail>();
            }
        }

        public async Task<bool> IsComponentExistsInCompositionAsync(int compositionId, int componentItemId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCompositionDetails
                    .Where(pcd => pcd.ItemCompositionId == compositionId && pcd.ComponentItemId == componentItemId);

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
                    ComponentItemId = componentItemId,
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
                var detail = await context.ItemCompositionDetails
                    .Include(pcd => pcd.ItemComposition)
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

        public async Task<List<ItemComposition>> GetCompositionsUsingComponentAsync(int componentItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositionDetails
                    .Include(pcd => pcd.ItemComposition)
                        .ThenInclude(pc => pc.ParentItem)
                    .Where(pcd => pcd.ComponentItemId == componentItemId)
                    .Select(pcd => pcd.ItemComposition)
                    .Distinct()
                    .OrderBy(pc => pc.ParentItemId)
                    .ThenBy(pc => pc.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompositionsUsingComponentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetCompositionsUsingComponentAsync),
                    ServiceType = GetType().Name,
                    ComponentItemId = componentItemId
                });
                return new List<ItemComposition>();
            }
        }

        #endregion
    }
}
