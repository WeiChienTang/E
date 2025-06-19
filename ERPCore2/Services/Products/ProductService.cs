using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductService : GenericManagementService<Product>, IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.ProductCategory)
                .Include(p => p.PrimarySupplier)
                .Include(p => p.Unit)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.ProductCategory)
                .Include(p => p.PrimarySupplier)
                .Include(p => p.Unit)
                .Include(p => p.ProductSuppliers)
                    .ThenInclude(ps => ps.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public override async Task<List<Product>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(p => p.ProductCategory)
                .Include(p => p.PrimarySupplier)
                .Include(p => p.Unit)
                .Where(p => !p.IsDeleted &&
                           (p.ProductName.Contains(searchTerm) ||
                            p.ProductCode.Contains(searchTerm) ||
                            (p.Description != null && p.Description.Contains(searchTerm)) ||
                            (p.Specification != null && p.Specification.Contains(searchTerm))))
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> ValidateAsync(Product entity)
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

            // 驗證庫存警戒值
            if (entity.MinStockLevel.HasValue && entity.MaxStockLevel.HasValue)
            {
                if (entity.MinStockLevel.Value > entity.MaxStockLevel.Value)
                {
                    errors.Add("最低庫存量不能大於最高庫存量");
                }
            }

            if (entity.MinStockLevel.HasValue && entity.MinStockLevel.Value < 0)
            {
                errors.Add("最低庫存量不能為負數");
            }

            if (entity.MaxStockLevel.HasValue && entity.MaxStockLevel.Value < 0)
            {
                errors.Add("最高庫存量不能為負數");
            }

            if (entity.CurrentStock < 0)
            {
                errors.Add("現有庫存不能為負數");
            }

            return errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Product?> GetByProductCodeAsync(string productCode)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .FirstOrDefaultAsync(p => p.ProductCode == productCode && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by code {ProductCode}", productCode);
                throw;
            }
        }

        public async Task<bool> IsProductCodeExistsAsync(string productCode, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(p => p.ProductCode == productCode && !p.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product code exists {ProductCode}", productCode);
                throw;
            }
        }

        public async Task<List<Product>> GetByProductCategoryAsync(int productCategoryId)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Where(p => p.ProductCategoryId == productCategoryId && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {ProductCategoryId}", productCategoryId);
                throw;
            }
        }

        public async Task<List<Product>> GetByPrimarySupplierAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Where(p => p.PrimarySupplierId == supplierId && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by primary supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active products");
                throw;
            }
        }

        public async Task<List<Product>> GetLowStockProductsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Where(p => !p.IsDeleted && 
                               p.MinStockLevel.HasValue && 
                               p.CurrentStock <= p.MinStockLevel.Value)
                    .OrderBy(p => p.CurrentStock)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                throw;
            }
        }

        public async Task<List<Product>> GetOverStockProductsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(p => p.ProductCategory)
                    .Include(p => p.PrimarySupplier)
                    .Include(p => p.Unit)
                    .Where(p => !p.IsDeleted && 
                               p.MaxStockLevel.HasValue && 
                               p.CurrentStock >= p.MaxStockLevel.Value)
                    .OrderByDescending(p => p.CurrentStock)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting over stock products");
                throw;
            }
        }

        #endregion

        #region 輔助資料查詢

        public async Task<List<ProductCategory>> GetProductCategoriesAsync()
        {
            try
            {
                return await _context.ProductCategories
                    .Where(pc => pc.Status == EntityStatus.Active && !pc.IsDeleted)
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product categories");
                throw;
            }
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            try
            {
                return await _context.Suppliers
                    .Where(s => s.Status == EntityStatus.Active && !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers");
                throw;
            }
        }

        public async Task<List<Unit>> GetUnitsAsync()
        {
            try
            {
                return await _context.Units
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .OrderBy(u => u.UnitName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting units");
                throw;
            }
        }

        #endregion

        #region 供應商管理

        public async Task<List<ProductSupplier>> GetProductSuppliersAsync(int productId)
        {
            try
            {
                return await _context.ProductSuppliers
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId && !ps.IsDeleted)
                    .OrderBy(ps => ps.Supplier.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product suppliers for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateProductSuppliersAsync(int productId, List<ProductSupplier> productSuppliers)
        {
            try
            {
                // 取得現有供應商關聯
                var existingRelations = await _context.ProductSuppliers
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
                        _context.ProductSuppliers.Add(newProductSupplier);
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product suppliers for product {ProductId}", productId);
                return ServiceResult.Failure($"更新商品供應商關聯時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetPrimarySupplierAsync(int productId, int supplierId)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                product.PrimarySupplierId = supplierId;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary supplier for product {ProductId}", productId);
                return ServiceResult.Failure($"設定主要供應商時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 庫存管理

        public async Task<ServiceResult> UpdateStockAsync(int productId, int newStock)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                if (newStock < 0)
                {
                    return ServiceResult.Failure("庫存數量不能為負數");
                }

                product.CurrentStock = newStock;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
                return ServiceResult.Failure($"更新庫存時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> AdjustStockAsync(int productId, int adjustment, string reason)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                var newStock = product.CurrentStock + adjustment;
                if (newStock < 0)
                {
                    return ServiceResult.Failure("調整後的庫存數量不能為負數");
                }

                product.CurrentStock = newStock;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Stock adjusted for product {ProductId}: {Adjustment} (Reason: {Reason})", 
                    productId, adjustment, reason);
                    
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock for product {ProductId}", productId);
                return ServiceResult.Failure($"調整庫存時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetStockLevelsAsync(int productId, int? minLevel, int? maxLevel)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                if (minLevel.HasValue && minLevel.Value < 0)
                {
                    return ServiceResult.Failure("最低庫存量不能為負數");
                }

                if (maxLevel.HasValue && maxLevel.Value < 0)
                {
                    return ServiceResult.Failure("最高庫存量不能為負數");
                }

                if (minLevel.HasValue && maxLevel.HasValue && minLevel.Value > maxLevel.Value)
                {
                    return ServiceResult.Failure("最低庫存量不能大於最高庫存量");
                }

                product.MinStockLevel = minLevel;
                product.MaxStockLevel = maxLevel;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting stock levels for product {ProductId}", productId);
                return ServiceResult.Failure($"設定庫存警戒值時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 價格管理

        public async Task<ServiceResult> UpdatePricesAsync(int productId, decimal? unitPrice, decimal? costPrice)
        {
            try
            {
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prices for product {ProductId}", productId);
                return ServiceResult.Failure($"更新價格時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchUpdatePricesAsync(List<int> productIds, decimal? priceAdjustment, bool isPercentage)
        {
            try
            {
                var products = await _dbSet
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch updating prices");
                return ServiceResult.Failure($"批次更新價格時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 狀態管理

        public async Task<ServiceResult> ToggleActiveStatusAsync(int productId)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure("找不到指定的商品");
                }

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for product {ProductId}", productId);
                return ServiceResult.Failure($"切換啟用狀態時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchSetActiveStatusAsync(List<int> productIds, bool isActive)
        {
            try
            {
                var products = await _dbSet
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch setting active status");
                return ServiceResult.Failure($"批次設定啟用狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewProduct(Product product)
        {
            product.ProductCode = string.Empty;
            product.ProductName = string.Empty;
            product.Description = string.Empty;
            product.Specification = string.Empty;
            product.UnitId = null;
            product.UnitPrice = null;
            product.CostPrice = null;
            product.MinStockLevel = null;
            product.MaxStockLevel = null;
            product.CurrentStock = 0;
            product.IsActive = true;
            product.ProductCategoryId = null;
            product.PrimarySupplierId = null;
            product.Status = EntityStatus.Active;
        }

        public int GetBasicRequiredFieldsCount()
        {
            return 2; // ProductCode, ProductName
        }

        public int GetBasicCompletedFieldsCount(Product product)
        {
            int count = 0;

            if (!string.IsNullOrWhiteSpace(product.ProductCode))
                count++;

            if (!string.IsNullOrWhiteSpace(product.ProductName))
                count++;

            return count;
        }

        public bool IsStockSufficient(Product product, int requiredQuantity)
        {
            return product.CurrentStock >= requiredQuantity;
        }

        public string GetStockStatus(Product product)
        {
            if (product.MinStockLevel.HasValue && product.CurrentStock <= product.MinStockLevel.Value)
            {
                return "庫存不足";
            }

            if (product.MaxStockLevel.HasValue && product.CurrentStock >= product.MaxStockLevel.Value)
            {
                return "庫存過量";
            }

            return "正常";
        }

        #endregion
    }
}