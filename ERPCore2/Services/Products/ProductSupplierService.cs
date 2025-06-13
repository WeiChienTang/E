using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品供應商關聯服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductSupplierService : GenericManagementService<ProductSupplier>, IProductSupplierService
    {
        private readonly ILogger<ProductSupplierService> _logger;

        public ProductSupplierService(AppDbContext context, ILogger<ProductSupplierService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<ProductSupplier>> GetAllAsync()
        {
            return await _dbSet
                .Include(ps => ps.Product)
                .Include(ps => ps.Supplier)
                .Where(ps => !ps.IsDeleted)
                .OrderBy(ps => ps.Product.ProductName)
                .ThenBy(ps => ps.Supplier.CompanyName)
                .ToListAsync();
        }

        public override async Task<List<ProductSupplier>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(ps => ps.Product)
                .Include(ps => ps.Supplier)
                .Where(ps => !ps.IsDeleted &&
                           (ps.Product.ProductName.Contains(searchTerm) ||
                            ps.Product.ProductCode.Contains(searchTerm) ||
                            ps.Supplier.CompanyName.Contains(searchTerm) ||
                            ps.Supplier.SupplierCode.Contains(searchTerm) ||
                            (ps.SupplierProductCode != null && ps.SupplierProductCode.Contains(searchTerm))))
                .OrderBy(ps => ps.Product.ProductName)
                .ThenBy(ps => ps.Supplier.CompanyName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> ValidateAsync(ProductSupplier entity)
        {
            var errors = new List<string>();

            // 驗證必填欄位
            if (entity.ProductId <= 0)
            {
                errors.Add("商品為必填欄位");
            }

            if (entity.SupplierId <= 0)
            {
                errors.Add("供應商為必填欄位");
            }

            // 檢查是否已存在相同的商品-供應商關聯
            if (entity.ProductId > 0 && entity.SupplierId > 0)
            {
                var exists = await _dbSet.AnyAsync(ps => 
                    ps.ProductId == entity.ProductId && 
                    ps.SupplierId == entity.SupplierId && 
                    ps.Id != entity.Id && 
                    !ps.IsDeleted);

                if (exists)
                {
                    errors.Add("此商品和供應商的關聯已存在");
                }
            }

            // 驗證價格
            if (entity.SupplierPrice.HasValue && entity.SupplierPrice.Value < 0)
            {
                errors.Add("供應商報價不能為負數");
            }

            // 驗證交期
            if (entity.LeadTime.HasValue && entity.LeadTime.Value < 0)
            {
                errors.Add("交期天數不能為負數");
            }

            // 驗證最小訂購量
            if (entity.MinOrderQuantity.HasValue && entity.MinOrderQuantity.Value < 0)
            {
                errors.Add("最小訂購量不能為負數");
            }

            return errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<List<ProductSupplier>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId && !ps.IsDeleted)
                    .OrderBy(ps => ps.Supplier.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<List<ProductSupplier>> GetBySupplierId(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Product)
                    .Where(ps => ps.SupplierId == supplierId && !ps.IsDeleted)
                    .OrderBy(ps => ps.Product.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<ProductSupplier?> GetPrimarySupplierAsync(int productId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Supplier)
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.IsPrimarySupplier && !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary supplier for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<ProductSupplier?> GetByProductAndSupplierAsync(int productId, int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && 
                                              ps.SupplierId == supplierId && 
                                              !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product supplier relation {ProductId}-{SupplierId}", productId, supplierId);
                throw;
            }
        }

        public async Task<List<ProductSupplier>> GetPrimarySuppliersAsync()
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.IsPrimarySupplier && !ps.IsDeleted)
                    .OrderBy(ps => ps.Product.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary suppliers");
                throw;
            }
        }

        #endregion

        #region 業務邏輯操作

        public async Task<ServiceResult> SetPrimarySupplierAsync(int productSupplierId)
        {
            try
            {
                var productSupplier = await GetByIdAsync(productSupplierId);
                if (productSupplier == null)
                {
                    return ServiceResult.Failure("找不到指定的商品供應商關聯");
                }

                // 將同一商品的其他供應商設為非主要
                var otherSuppliers = await _dbSet
                    .Where(ps => ps.ProductId == productSupplier.ProductId && 
                                ps.Id != productSupplierId && 
                                ps.IsPrimarySupplier && 
                                !ps.IsDeleted)
                    .ToListAsync();

                foreach (var otherSupplier in otherSuppliers)
                {
                    otherSupplier.IsPrimarySupplier = false;
                    otherSupplier.UpdatedAt = DateTime.UtcNow;
                }

                // 設定指定供應商為主要
                productSupplier.IsPrimarySupplier = true;
                productSupplier.UpdatedAt = DateTime.UtcNow;

                // 同時更新商品的主要供應商ID
                var product = await _context.Products.FindAsync(productSupplier.ProductId);
                if (product != null)
                {
                    product.PrimarySupplierId = productSupplier.SupplierId;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary supplier {ProductSupplierId}", productSupplierId);
                return ServiceResult.Failure($"設定主要供應商時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchSetProductSuppliersAsync(int productId, List<int> supplierIds)
        {
            try
            {
                // 取得現有關聯
                var existingRelations = await _dbSet
                    .Where(ps => ps.ProductId == productId && !ps.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的關聯
                var relationsToDelete = existingRelations
                    .Where(er => !supplierIds.Contains(er.SupplierId))
                    .ToList();

                foreach (var relation in relationsToDelete)
                {
                    relation.IsDeleted = true;
                    relation.UpdatedAt = DateTime.UtcNow;
                }

                // 新增不存在的關聯
                var existingSupplierIds = existingRelations.Select(er => er.SupplierId).ToList();
                var newSupplierIds = supplierIds.Where(sid => !existingSupplierIds.Contains(sid)).ToList();

                foreach (var supplierId in newSupplierIds)
                {
                    var newRelation = new ProductSupplier
                    {
                        ProductId = productId,
                        SupplierId = supplierId,
                        IsPrimarySupplier = false,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbSet.Add(newRelation);
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch setting product suppliers for product {ProductId}", productId);
                return ServiceResult.Failure($"批次設定商品供應商時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> BatchSetSupplierProductsAsync(int supplierId, List<int> productIds)
        {
            try
            {
                // 取得現有關聯
                var existingRelations = await _dbSet
                    .Where(ps => ps.SupplierId == supplierId && !ps.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的關聯
                var relationsToDelete = existingRelations
                    .Where(er => !productIds.Contains(er.ProductId))
                    .ToList();

                foreach (var relation in relationsToDelete)
                {
                    relation.IsDeleted = true;
                    relation.UpdatedAt = DateTime.UtcNow;
                }

                // 新增不存在的關聯
                var existingProductIds = existingRelations.Select(er => er.ProductId).ToList();
                var newProductIds = productIds.Where(pid => !existingProductIds.Contains(pid)).ToList();

                foreach (var productId in newProductIds)
                {
                    var newRelation = new ProductSupplier
                    {
                        ProductId = productId,
                        SupplierId = supplierId,
                        IsPrimarySupplier = false,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbSet.Add(newRelation);
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch setting supplier products for supplier {SupplierId}", supplierId);
                return ServiceResult.Failure($"批次設定供應商商品時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateSupplierPriceAsync(int productSupplierId, decimal? supplierPrice, string? supplierProductCode = null)
        {
            try
            {
                var productSupplier = await GetByIdAsync(productSupplierId);
                if (productSupplier == null)
                {
                    return ServiceResult.Failure("找不到指定的商品供應商關聯");
                }

                if (supplierPrice.HasValue && supplierPrice.Value < 0)
                {
                    return ServiceResult.Failure("供應商報價不能為負數");
                }

                productSupplier.SupplierPrice = supplierPrice;
                if (supplierProductCode != null)
                {
                    productSupplier.SupplierProductCode = supplierProductCode;
                }
                productSupplier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier price {ProductSupplierId}", productSupplierId);
                return ServiceResult.Failure($"更新供應商價格時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateDeliveryInfoAsync(int productSupplierId, int? leadTime, int? minOrderQuantity)
        {
            try
            {
                var productSupplier = await GetByIdAsync(productSupplierId);
                if (productSupplier == null)
                {
                    return ServiceResult.Failure("找不到指定的商品供應商關聯");
                }

                if (leadTime.HasValue && leadTime.Value < 0)
                {
                    return ServiceResult.Failure("交期天數不能為負數");
                }

                if (minOrderQuantity.HasValue && minOrderQuantity.Value < 0)
                {
                    return ServiceResult.Failure("最小訂購量不能為負數");
                }

                productSupplier.LeadTime = leadTime;
                productSupplier.MinOrderQuantity = minOrderQuantity;
                productSupplier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery info {ProductSupplierId}", productSupplierId);
                return ServiceResult.Failure($"更新交期資訊時發生錯誤: {ex.Message}");
            }
        }

        public async Task<bool> HasSuppliersAsync(int productId)
        {
            try
            {
                return await _dbSet.AnyAsync(ps => ps.ProductId == productId && !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product has suppliers {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> HasProductsAsync(int supplierId)
        {
            try
            {
                return await _dbSet.AnyAsync(ps => ps.SupplierId == supplierId && !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if supplier has products {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<List<ProductSupplier>> GetBestPriceProductsAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Product)
                    .Where(ps => ps.SupplierId == supplierId && 
                                ps.SupplierPrice.HasValue && 
                                !ps.IsDeleted)
                    .OrderBy(ps => ps.SupplierPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best price products for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<List<ProductSupplier>> GetBestPriceSuppliersAsync(int productId)
        {
            try
            {
                return await _dbSet
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId && 
                                ps.SupplierPrice.HasValue && 
                                !ps.IsDeleted)
                    .OrderBy(ps => ps.SupplierPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best price suppliers for product {ProductId}", productId);
                throw;
            }
        }

        #endregion

        #region 統計分析

        public async Task<int> GetSupplierCountAsync(int productId)
        {
            try
            {
                return await _dbSet.CountAsync(ps => ps.ProductId == productId && !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier count for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<int> GetProductCountAsync(int supplierId)
        {
            try
            {
                return await _dbSet.CountAsync(ps => ps.SupplierId == supplierId && !ps.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<double> GetAverageLeadTimeAsync(int productId)
        {
            try
            {
                var leadTimes = await _dbSet
                    .Where(ps => ps.ProductId == productId && 
                                ps.LeadTime.HasValue && 
                                !ps.IsDeleted)
                    .Select(ps => ps.LeadTime!.Value)
                    .ToListAsync();

                return leadTimes.Any() ? leadTimes.Average() : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average lead time for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<(decimal? MinPrice, decimal? MaxPrice)> GetPriceRangeAsync(int productId)
        {
            try
            {
                var prices = await _dbSet
                    .Where(ps => ps.ProductId == productId && 
                                ps.SupplierPrice.HasValue && 
                                !ps.IsDeleted)
                    .Select(ps => ps.SupplierPrice!.Value)
                    .ToListAsync();

                if (!prices.Any())
                    return (null, null);

                return (prices.Min(), prices.Max());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price range for product {ProductId}", productId);
                throw;
            }
        }

        #endregion
    }
}
