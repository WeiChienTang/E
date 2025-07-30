using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品分類服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductCategoryService : GenericManagementService<ProductCategory>, IProductCategoryService
    {
        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public ProductCategoryService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<ProductCategory>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public ProductCategoryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<ProductCategory>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCategories
                    .Where(pc => !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<ProductCategory>();
            }
        }

        public override async Task<ProductCategory?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCategories
                    .Include(pc => pc.Products.Where(p => !p.IsDeleted))
                    .FirstOrDefaultAsync(pc => pc.Id == id && !pc.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                return null;
            }
        }

        public override async Task<List<ProductCategory>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCategories
                    .Where(pc => !pc.IsDeleted &&
                               (pc.CategoryName.Contains(searchTerm) ||
                                (pc.CategoryCode != null && pc.CategoryCode.Contains(searchTerm)) ||
                                (pc.Description != null && pc.Description.Contains(searchTerm))))
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<ProductCategory>();
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

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    CategoryName = entity.CategoryName
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("檢查刪除權限時發生錯誤");
            }
        }

        #endregion

        #region 業務特定方法

        public async Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductCategories.Where(pc => pc.CategoryName == categoryName && !pc.IsDeleted);
                
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

        public async Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductCategories.Where(pc => pc.CategoryCode == categoryCode && !pc.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCategoryCodeExistsAsync), GetType(), _logger, new { 
                    CategoryCode = categoryCode,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<ProductCategory?> GetByCategoryNameAsync(string categoryName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.CategoryName == categoryName && !pc.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCategoryNameAsync), GetType(), _logger, new { 
                    CategoryName = categoryName
                });
                return null;
            }
        }

        public async Task<ProductCategory?> GetByCategoryCodeAsync(string categoryCode)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.CategoryCode == categoryCode && !pc.IsDeleted);
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
                // 檢查是否有商品使用此分類
                var hasProducts = await context.Products
                    .AnyAsync(p => p.ProductCategoryId == categoryId && !p.IsDeleted);

                return !hasProducts;
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

