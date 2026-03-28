using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項分類服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ItemCategoryService : GenericManagementService<ItemCategory>, IItemCategoryService
    {
        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public ItemCategoryService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ItemCategory>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public ItemCategoryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<ItemCategory> BuildGetAllQuery(AppDbContext context)
        {
            return context.ItemCategories
                .Include(pc => pc.Items.AsQueryable())
                .OrderBy(pc => pc.Name);
        }

        public override async Task<ItemCategory?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCategories
                    .Include(pc => pc.Items.AsQueryable())
                    .FirstOrDefaultAsync(pc => pc.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                return null;
            }
        }

        public override async Task<List<ItemCategory>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCategories
                    .Where(pc => (pc.Name != null && pc.Name.Contains(searchTerm) ||
                                (pc.Code != null && pc.Code.Contains(searchTerm))))
                    .OrderBy(pc => pc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<ItemCategory>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ItemCategory entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.Name))
                    && string.IsNullOrWhiteSpace(entity.Name))
                {
                    errors.Add("分類名稱為必填欄位");
                }
                else
                {
                    // 檢查名稱是否重複
                    var isDuplicate = await IsCategoryNameExistsAsync(entity.Name, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("分類名稱已存在");
                    }
                }

                // 驗證分類編號唯一性（如果有提供）
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    var isDuplicate = await IsItemCategoryCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("分類編號已存在");
                    }
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    CategoryName = entity.Name
                });
                return ServiceResult.Failure("驗證過程中發生錯誤");
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                return await IsCategoryNameExistsAsync(name, excludeId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Name = name,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(ItemCategory entity)
        {
            try
            {
                var canDelete = await CanDeleteCategoryAsync(entity.Id);
                return canDelete 
                    ? ServiceResult.Success() 
                    : ServiceResult.Failure("無法刪除此品項分類，因為有品項或子分類正在使用此分類");
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("檢查刪除權限時發生錯誤");
            }
        }

        #endregion

        public async Task<(List<ItemCategory> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ItemCategory>, IQueryable<ItemCategory>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<ItemCategory> query = context.ItemCategories;

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(pc => pc.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new {
                    Method = nameof(GetPagedWithFiltersAsync),
                    ServiceType = GetType().Name,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
                return (new List<ItemCategory>(), 0);
            }
        }

        #region 業務特定方法

        public async Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCategories.Where(pc => pc.Name == categoryName);
                
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCategoryNameExistsAsync), GetType(), _logger, new { 
                    CategoryName = categoryName,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<bool> IsItemCategoryCodeExistsAsync(string categoryCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ItemCategories.Where(pc => pc.Code == categoryCode);
                
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsItemCategoryCodeExistsAsync), GetType(), _logger, new { 
                    CategoryCode = categoryCode,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<ItemCategory?> GetByCategoryNameAsync(string categoryName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCategories
                    .FirstOrDefaultAsync(pc => pc.Name == categoryName);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCategoryNameAsync), GetType(), _logger, new { 
                    CategoryName = categoryName
                });
                return null;
            }
        }

        public async Task<ItemCategory?> GetByCategoryCodeAsync(string categoryCode)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemCategories
                    .FirstOrDefaultAsync(pc => pc.Code == categoryCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCategoryCodeAsync), GetType(), _logger, new { 
                    CategoryCode = categoryCode
                });
                return null;
            }
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查是否有品項使用此分類
                var hasItems = await context.Items
                    .AnyAsync(p => p.ItemCategoryId == categoryId);

                return !hasItems;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteCategoryAsync), GetType(), _logger, new { 
                    CategoryId = categoryId
                });
                return false;
            }
        }

        #endregion
    }
}


