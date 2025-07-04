using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品分類服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductCategoryService : GenericManagementService<ProductCategory>, IProductCategoryService
    {
        public ProductCategoryService(
            AppDbContext context, 
            ILogger<GenericManagementService<ProductCategory>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<ProductCategory>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .Where(pc => !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all product categories");
                throw;
            }
        }

        public override async Task<ProductCategory?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .Include(pc => pc.ChildCategories.Where(cc => !cc.IsDeleted))
                    .Include(pc => pc.Products.Where(p => !p.IsDeleted))
                    .FirstOrDefaultAsync(pc => pc.Id == id && !pc.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByIdAsync),
                    Id = id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting product category by id {Id}", id);
                throw;
            }
        }

        public override async Task<List<ProductCategory>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .Where(pc => !pc.IsDeleted &&
                               (pc.CategoryName.Contains(searchTerm) ||
                                (pc.CategoryCode != null && pc.CategoryCode.Contains(searchTerm)) ||
                                (pc.Description != null && pc.Description.Contains(searchTerm))))
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching product categories with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ProductCategory entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(entity.CategoryName))
                {
                    errors.Add("分類名稱為必填欄位");
                }
                else
                {
                    // 檢查名稱是否重複
                    var isDuplicate = await IsCategoryNameExistsAsync(entity.CategoryName, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("分類名稱已存在");
                    }
                }

                // 驗證分類代碼唯一性（如果有提供）
                if (!string.IsNullOrWhiteSpace(entity.CategoryCode))
                {
                    var isDuplicate = await IsCategoryCodeExistsAsync(entity.CategoryCode, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("分類代碼已存在");
                    }
                }

                // 驗證父分類設定（避免循環參考）
                if (entity.ParentCategoryId.HasValue)
                {
                    if (entity.ParentCategoryId.Value == entity.Id)
                    {
                        errors.Add("不能設定自己為父分類");
                    }
                    else if (entity.Id != 0) // 只有更新時才檢查
                    {
                        var canSetAsParent = await CanSetAsParentAsync(entity.Id, entity.ParentCategoryId.Value);
                        if (!canSetAsParent)
                        {
                            errors.Add("不能設定此分類為父分類，會造成循環參考");
                        }
                    }
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    CategoryName = entity.CategoryName,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating product category {CategoryName}", entity.CategoryName);
                throw;
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if name exists {Name}", name);
                throw;
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(ProductCategory entity)
        {
            try
            {
                var canDelete = await CanDeleteCategoryAsync(entity.Id);
                return canDelete 
                    ? ServiceResult.Success() 
                    : ServiceResult.Failure("無法刪除此商品分類，因為有商品或子分類正在使用此分類");
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if category can be deleted {CategoryId}", entity.Id);
                throw;
            }
        }

        #endregion

        #region 業務特定方法

        public async Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(pc => pc.CategoryName == categoryName && !pc.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsCategoryNameExistsAsync),
                    CategoryName = categoryName,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking category name exists {CategoryName}", categoryName);
                throw;
            }
        }

        public async Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(pc => pc.CategoryCode == categoryCode && !pc.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsCategoryCodeExistsAsync),
                    CategoryCode = categoryCode,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking category code exists {CategoryCode}", categoryCode);
                throw;
            }
        }

        public async Task<ProductCategory?> GetByCategoryNameAsync(string categoryName)
        {
            try
            {
                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .FirstOrDefaultAsync(pc => pc.CategoryName == categoryName && !pc.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByCategoryNameAsync),
                    CategoryName = categoryName,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting category by name {CategoryName}", categoryName);
                throw;
            }
        }

        public async Task<ProductCategory?> GetByCategoryCodeAsync(string categoryCode)
        {
            try
            {
                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .FirstOrDefaultAsync(pc => pc.CategoryCode == categoryCode && !pc.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByCategoryCodeAsync),
                    CategoryCode = categoryCode,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting category by code {CategoryCode}", categoryCode);
                throw;
            }
        }

        public async Task<List<ProductCategory>> GetTopLevelCategoriesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(pc => pc.ParentCategoryId == null && !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetTopLevelCategoriesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting top level categories");
                throw;
            }
        }

        public async Task<List<ProductCategory>> GetChildCategoriesAsync(int parentCategoryId)
        {
            try
            {
                return await _dbSet
                    .Where(pc => pc.ParentCategoryId == parentCategoryId && !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetChildCategoriesAsync),
                    ParentCategoryId = parentCategoryId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting child categories for parent {ParentCategoryId}", parentCategoryId);
                throw;
            }
        }

        public async Task<List<ProductCategory>> GetCategoryTreeAsync()
        {
            try
            {
                return await _dbSet
                    .Include(pc => pc.ParentCategory)
                    .Include(pc => pc.ChildCategories.Where(cc => !cc.IsDeleted))
                    .Where(pc => !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetCategoryTreeAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting category tree");
                throw;
            }
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            try
            {
                // 檢查是否有商品使用此分類
                var hasProducts = await _context.Products
                    .AnyAsync(p => p.ProductCategoryId == categoryId && !p.IsDeleted);

                if (hasProducts)
                    return false;

                // 檢查是否有子分類
                var hasChildCategories = await _dbSet
                    .AnyAsync(pc => pc.ParentCategoryId == categoryId && !pc.IsDeleted);

                return !hasChildCategories;
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteCategoryAsync),
                    CategoryId = categoryId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if category can be deleted {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<bool> CanSetAsParentAsync(int categoryId, int parentCategoryId)
        {
            try
            {
                // 檢查是否會造成循環參考
                var currentCategory = await _dbSet.FindAsync(parentCategoryId);
                
                while (currentCategory?.ParentCategoryId != null)
                {
                    if (currentCategory.ParentCategoryId == categoryId)
                    {
                        return false; // 會造成循環參考
                    }
                    
                    currentCategory = await _dbSet.FindAsync(currentCategory.ParentCategoryId);
                }

                return true;
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanSetAsParentAsync),
                    CategoryId = categoryId,
                    ParentCategoryId = parentCategoryId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if can set as parent {CategoryId} -> {ParentCategoryId}", 
                    categoryId, parentCategoryId);
                throw;
            }
        }

        #endregion
    }
}
