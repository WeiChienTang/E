using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項合成服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ItemCompositionService : GenericManagementService<ItemComposition>, IItemCompositionService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ItemCompositionService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ItemComposition>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ItemCompositionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<ItemComposition> BuildGetAllQuery(AppDbContext context)
        {
            return context.ItemCompositions
                .Include(pc => pc.ParentItem)
                .Include(pc => pc.Customer)
                .Include(pc => pc.CreatedByEmployee)
                .Include(pc => pc.CompositionDetails)
                    .ThenInclude(pcd => pcd.ComponentItem)
                .OrderBy(pc => pc.ParentItemId)
                .ThenBy(pc => pc.Code);
        }

        public override async Task<ItemComposition?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositions
                    .Include(pc => pc.ParentItem)
                    .Include(pc => pc.Customer)
                    .Include(pc => pc.CreatedByEmployee)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.ComponentItem)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.Unit)
                    .FirstOrDefaultAsync(pc => pc.Id == id);
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

        public override async Task<List<ItemComposition>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCompositions
                    .Include(pc => pc.ParentItem)
                    .Include(pc => pc.Customer)
                    .Include(pc => pc.CreatedByEmployee)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.ComponentItem)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(pc =>
                        (pc.Code != null && pc.Code.ToLower().Contains(lowerSearchTerm)) ||
                        (pc.ParentItem != null && pc.ParentItem.Name != null && pc.ParentItem.Name.ToLower().Contains(lowerSearchTerm)) ||
                        (pc.Specification != null && pc.Specification.ToLower().Contains(lowerSearchTerm)) ||
                        (pc.Customer != null && pc.Customer.CompanyName != null && pc.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)));
                }

                return await query
                    .OrderBy(pc => pc.ParentItemId)
                    .ThenBy(pc => pc.Code)
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
                return new List<ItemComposition>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ItemComposition entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (!entity.ParentItemId.HasValue || entity.ParentItemId.Value <= 0)
                    errors.Add("請選擇成品");

                // 檢查物料清單編號是否重複（參考 PurchaseOrderService 的做法）
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var exists = await context.ItemCompositions
                        .AnyAsync(pc => pc.Code == entity.Code && pc.Id != entity.Id);
                    if (exists)
                        errors.Add("物料清單編號已存在");
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
                    ParentItemId = entity.ParentItemId,
                    Code = entity.Code
                });
                return ServiceResult.Failure($"驗證品項物料清單時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region IItemCompositionService 實作

        public async Task<bool> IsItemCompositionCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCompositions.Where(pc => pc.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsItemCompositionCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsItemCompositionCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 取得指定品項的所有物料清單（用於相關單據查詢）
        /// </summary>
        public async Task<List<ItemComposition>> GetByItemIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositions
                    .Include(pc => pc.ParentItem)
                    .Include(pc => pc.Customer)
                    .Include(pc => pc.CreatedByEmployee)
                    .Include(pc => pc.CompositionCategory)
                    .Where(pc => pc.ParentItemId == productId)
                    .OrderByDescending(pc => pc.CreatedAt)
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
                return new List<ItemComposition>();
            }
        }
        
        public async Task<List<ItemComposition>> GetCompositionsByItemIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCompositions
                    .Include(pc => pc.ParentItem)
                    .Include(pc => pc.Customer)
                    .Include(pc => pc.CreatedByEmployee)
                    .Include(pc => pc.CompositionCategory)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.ComponentItem)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.Unit)
                    .Where(pc => pc.ParentItemId == productId)
                    .OrderBy(pc => pc.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompositionsByItemIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetCompositionsByItemIdAsync),
                    ServiceType = GetType().Name,
                    ItemId = productId
                });
                return new List<ItemComposition>();
            }
        }

        public async Task<decimal> CalculateTotalCostAsync(int compositionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var composition = await context.ItemCompositions
                    .Include(pc => pc.CompositionDetails)
                    .FirstOrDefaultAsync(pc => pc.Id == compositionId);

                if (composition == null)
                    return 0;

                decimal totalCost = 0;
                foreach (var detail in composition.CompositionDetails)
                {
                    if (detail.ComponentCost.HasValue)
                    {
                        var quantity = detail.Quantity;
                        totalCost += detail.ComponentCost.Value * quantity;
                    }
                }

                return totalCost;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalCostAsync), GetType(), _logger, new
                {
                    Method = nameof(CalculateTotalCostAsync),
                    ServiceType = GetType().Name,
                    CompositionId = compositionId
                });
                return 0;
            }
        }

        public async Task<object> GetBomTreeAsync(int compositionId, int maxLevel = 10)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var composition = await context.ItemCompositions
                    .Include(pc => pc.ParentItem)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.ComponentItem)
                    .Include(pc => pc.CompositionDetails)
                        .ThenInclude(pcd => pcd.Unit)
                    .FirstOrDefaultAsync(pc => pc.Id == compositionId);

                if (composition == null)
                    return new { };

                return await BuildBomTreeNodeAsync(context, composition, 0, maxLevel, new HashSet<int>());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBomTreeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBomTreeAsync),
                    ServiceType = GetType().Name,
                    CompositionId = compositionId,
                    MaxLevel = maxLevel
                });
                return new { };
            }
        }

        /// <summary>
        /// 遞迴建立 BOM 樹狀節點（防止循環參考）
        /// </summary>
        private async Task<object> BuildBomTreeNodeAsync(
            AppDbContext context,
            ItemComposition composition,
            int currentLevel,
            int maxLevel,
            HashSet<int> processedCompositions)
        {
            // 防止循環參考
            if (processedCompositions.Contains(composition.Id))
            {
                return new
                {
                    id = composition.Id,
                    code = composition.Code,
                    specification = composition.Specification,
                    productId = composition.ParentItemId,
                    productName = composition.ParentItem?.Name,
                    isCircular = true,
                    children = new List<object>()
                };
            }

            processedCompositions.Add(composition.Id);

            var children = new List<object>();

            // 如果未達最大層級，展開子組件
            if (currentLevel < maxLevel)
            {
                foreach (var detail in composition.CompositionDetails)
                {
                    // 檢查組件是否也有 BOM（取第一個配方）
                    var childComposition = await context.ItemCompositions
                        .Include(pc => pc.ParentItem)
                        .Include(pc => pc.CompositionDetails)
                            .ThenInclude(pcd => pcd.ComponentItem)
                        .Include(pc => pc.CompositionDetails)
                            .ThenInclude(pcd => pcd.Unit)
                        .Where(pc => pc.ParentItemId == detail.ComponentItemId)
                        .FirstOrDefaultAsync();

                    if (childComposition != null)
                    {
                        // 遞迴建立子節點
                        var childNode = await BuildBomTreeNodeAsync(
                            context,
                            childComposition,
                            currentLevel + 1,
                            maxLevel,
                            new HashSet<int>(processedCompositions));

                        children.Add(new
                        {
                            detail = new
                            {
                                id = detail.Id,
                                quantity = detail.Quantity,
                                unitName = detail.Unit?.Name
                            },
                            composition = childNode
                        });
                    }
                    else
                    {
                        // 葉節點（無子 BOM）
                        children.Add(new
                        {
                            detail = new
                            {
                                id = detail.Id,
                                quantity = detail.Quantity,
                                unitName = detail.Unit?.Name
                            },
                            component = new
                            {
                                id = detail.ComponentItem.Id,
                                code = detail.ComponentItem.Code,
                                name = detail.ComponentItem.Name,
                                isLeaf = true
                            }
                        });
                    }
                }
            }

            return new
            {
                id = composition.Id,
                code = composition.Code,
                specification = composition.Specification,
                customerId = composition.CustomerId,
                customerName = composition.Customer?.CompanyName,
                createdByEmployeeId = composition.CreatedByEmployeeId,
                createdByEmployeeName = composition.CreatedByEmployee?.Name,
                productId = composition.ParentItemId,
                productName = composition.ParentItem?.Name,
                level = currentLevel,
                children
            };
        }

        #endregion

        #region 伺服器端分頁

        public async Task<(List<ItemComposition> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ItemComposition>, IQueryable<ItemComposition>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<ItemComposition> query = context.ItemCompositions
                    .Include(c => c.ParentItem)
                    .Include(c => c.CompositionCategory);
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(c => c.Code)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<ItemComposition>(), 0);
            }
        }

        #endregion
    }
}
