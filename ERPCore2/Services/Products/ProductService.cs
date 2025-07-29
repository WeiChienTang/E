using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductService : GenericManagementService<Product>, IProductService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ProductService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Product>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ProductService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<Product>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Include(p => p.ProductSuppliers)
                        .ThenInclude(ps => ps.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Product>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => !p.IsDeleted &&
                               (p.ProductName.Contains(searchTerm) ||
                                p.ProductCode.Contains(searchTerm) ||
                                (p.Description != null && p.Description.Contains(searchTerm)) ||
                                (p.Specification != null && p.Specification.Contains(searchTerm))))
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Product entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(entity.ProductCode))
                {
                    errors.Add("商品代碼為必填欄位");
                }
                else
                {
                    // 檢查商品代碼唯一性
                    var isDuplicate = await IsProductCodeExistsAsync(entity.ProductCode, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("商品代碼已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.ProductName))
                {
                    errors.Add("商品名稱為必填欄位");
                }

                // 驗證價格
                if (entity.UnitPrice.HasValue && entity.UnitPrice.Value < 0)
                {
                    errors.Add("單價不能為負數");
                }

                if (entity.CostPrice.HasValue && entity.CostPrice.Value < 0)
                {
                    errors.Add("成本價不能為負數");
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id, ProductCode = entity.ProductCode });
                return ServiceResult.Failure($"驗證商品時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Product?> GetByProductCodeAsync(string productCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .FirstOrDefaultAsync(p => p.ProductCode == productCode && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductCodeAsync), GetType(), _logger, new { ProductCode = productCode });
                throw;
            }
        }

        public async Task<bool> IsProductCodeExistsAsync(string productCode, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.Products.Where(p => p.ProductCode == productCode && !p.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductCodeExistsAsync), GetType(), _logger, new { ProductCode = productCode, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<List<Product>> GetByProductCategoryAsync(int productCategoryId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.ProductCategoryId == productCategoryId && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductCategoryAsync), GetType(), _logger, new { ProductCategoryId = productCategoryId });
                throw;
            }
        }

        public async Task<List<Product>> GetByPrimarySupplierAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.PrimarySupplierId == supplierId && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPrimarySupplierAsync), GetType(), _logger, new { SupplierId = supplierId });
                throw;
            }
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveProductsAsync), GetType(), _logger);
                throw;
            }
        }

        #endregion

        #region 輔助資料查詢

        public async Task<List<ProductCategory>> GetProductCategoriesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductCategories
                    .Where(pc => pc.Status == EntityStatus.Active && !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetProductCategoriesAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .Where(s => s.Status == EntityStatus.Active && !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSuppliersAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<Unit>> GetUnitsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Units
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .OrderBy(u => u.UnitName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnitsAsync), GetType(), _logger);
                throw;
            }
        }

        #endregion

        #region 供應商管理

        public async Task<List<ProductSupplier>> GetProductSuppliersAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductSuppliers
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId && !ps.IsDeleted)
                    .OrderBy(ps => ps.Supplier.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetProductSuppliersAsync), GetType(), _logger, new { ProductId = productId });
                throw;
            }
        }

        public async Task<ServiceResult> UpdateProductSuppliersAsync(int productId, List<ProductSupplier> productSuppliers)
        {
            try
            {
                // 取得現有供應商關聯
                using var context = await _contextFactory.CreateDbContextAsync();
                var existingRelations = await context.ProductSuppliers
                    .Where(ps => ps.ProductId == productId && !ps.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的關聯
                var relationsToDelete = existingRelations
                    .Where(er => !productSuppliers.Any(ps => ps.Id == er.Id))
                    .ToList();

                foreach (var relation in relationsToDelete)
                {
                    relation.IsDeleted = true;
                    relation.UpdatedAt = DateTime.UtcNow;
                }

                // 更新或新增關聯
                foreach (var productSupplier in productSuppliers)
                {
                    if (productSupplier.Id <= 0) // 新增（包括負數臨時 ID）
                    {
                        // 建立新的產品供應商實體以避免 ID 衝突
                        var newProductSupplier = new ProductSupplier
                        {
                            ProductId = productId,
                            SupplierId = productSupplier.SupplierId,
                            SupplierProductCode = productSupplier.SupplierProductCode,
                            SupplierPrice = productSupplier.SupplierPrice,
                            LeadTime = productSupplier.LeadTime,
                            MinOrderQuantity = productSupplier.MinOrderQuantity,
                            IsPrimarySupplier = productSupplier.IsPrimarySupplier,
                            Status = EntityStatus.Active,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = "System", // TODO: 從認證取得使用者
                            Remarks = productSupplier.Remarks
                        };
                        context.ProductSuppliers.Add(newProductSupplier);
                    }
                    else
                    {
                        // 更新
                        var existingRelation = existingRelations.FirstOrDefault(er => er.Id == productSupplier.Id);
                        if (existingRelation != null)
                        {
                            existingRelation.SupplierId = productSupplier.SupplierId;
                            existingRelation.SupplierProductCode = productSupplier.SupplierProductCode;
                            existingRelation.SupplierPrice = productSupplier.SupplierPrice;
                            existingRelation.LeadTime = productSupplier.LeadTime;
                            existingRelation.MinOrderQuantity = productSupplier.MinOrderQuantity;
                            existingRelation.IsPrimarySupplier = productSupplier.IsPrimarySupplier;
                            existingRelation.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateProductSuppliersAsync), GetType(), _logger, new { ProductId = productId });
                return ServiceResult.Failure($"更新商品供應商關聯時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetPrimarySupplierAsync(int productId, int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                product.PrimarySupplierId = supplierId;
                product.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetPrimarySupplierAsync), GetType(), _logger, new { ProductId = productId, SupplierId = supplierId });
                return ServiceResult.Failure($"設定主要供應商時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 價格管理

        public async Task<ServiceResult> UpdatePricesAsync(int productId, decimal? unitPrice, decimal? costPrice)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                if (unitPrice.HasValue && unitPrice.Value < 0)
                {
                    return ServiceResult.Failure("單價不能為負數");
                }

                if (costPrice.HasValue && costPrice.Value < 0)
                {
                    return ServiceResult.Failure("成本價不能為負數");
                }

                product.UnitPrice = unitPrice;
                product.CostPrice = costPrice;
                product.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePricesAsync), GetType(), _logger, new { ProductId = productId, UnitPrice = unitPrice, CostPrice = costPrice });
                return ServiceResult.Failure($"更新價格時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchUpdatePricesAsync(List<int> productIds, decimal? priceAdjustment, bool isPercentage)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var products = await context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .ToListAsync();

                if (!products.Any())
                {
                    return ServiceResult.Failure("找不到要更新的商品");
                }

                if (!priceAdjustment.HasValue)
                {
                    return ServiceResult.Failure("價格調整值不能為空");
                }

                foreach (var product in products)
                {
                    if (product.UnitPrice.HasValue)
                    {
                        if (isPercentage)
                        {
                            product.UnitPrice = product.UnitPrice.Value * (1 + priceAdjustment.Value / 100);
                        }
                        else
                        {
                            product.UnitPrice = product.UnitPrice.Value + priceAdjustment.Value;
                        }

                        if (product.UnitPrice.Value < 0)
                        {
                            product.UnitPrice = 0;
                        }
                    }

                    product.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdatePricesAsync), GetType(), _logger, new { ProductIds = productIds, PriceAdjustment = priceAdjustment, IsPercentage = isPercentage });
                return ServiceResult.Failure($"批次更新價格時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 狀態管理

        public async Task<ServiceResult> ToggleActiveStatusAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ToggleActiveStatusAsync), GetType(), _logger, new { ProductId = productId });
                return ServiceResult.Failure($"切換啟用狀態時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchSetActiveStatusAsync(List<int> productIds, bool isActive)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var products = await context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .ToListAsync();

                if (!products.Any())
                {
                    return ServiceResult.Failure("找不到要更新的商品");
                }

                foreach (var product in products)
                {
                    product.IsActive = isActive;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchSetActiveStatusAsync), GetType(), _logger, new { ProductIds = productIds, IsActive = isActive });
                return ServiceResult.Failure($"批次設定啟用狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewProduct(Product product)
        {
            try
            {
                product.ProductCode = string.Empty;
                product.ProductName = string.Empty;
                product.Description = string.Empty;
                product.Specification = string.Empty;
                product.UnitId = null;
                product.SizeId = null;
                product.UnitPrice = null;
                product.CostPrice = null;
                product.IsActive = true;
                product.ProductCategoryId = null;
                product.PrimarySupplierId = null;
                product.Status = EntityStatus.Active;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(InitializeNewProduct), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicRequiredFieldsCount()
        {
            try
            {
                return 2; // ProductCode, ProductName
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicRequiredFieldsCount), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicCompletedFieldsCount(Product product)
        {
            try
            {
                int count = 0;

                if (!string.IsNullOrWhiteSpace(product.ProductCode))
                    count++;

                if (!string.IsNullOrWhiteSpace(product.ProductName))
                    count++;

                return count;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicCompletedFieldsCount), GetType(), _logger, new { ProductId = product?.Id });
                throw;
            }
        }

        #endregion
    }
}