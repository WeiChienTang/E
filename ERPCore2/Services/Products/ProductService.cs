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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .AsQueryable()
                    .OrderBy(p => p.Name)
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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Include(p => p.ProductSuppliers)
                        .ThenInclude(ps => ps.Supplier)
                    .Include(p => p.ProductSuppliers)
                        .ThenInclude(ps => ps.Unit)
                    .FirstOrDefaultAsync(p => p.Id == id);
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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => ((p.Name != null && p.Name.Contains(searchTerm)) ||
                                (p.Code != null && p.Code.Contains(searchTerm)) ||
                                (p.Barcode != null && p.Barcode.Contains(searchTerm))))
                    .OrderBy(p => p.Name)
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
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("商品代碼為必填欄位");
                }
                else
                {
                    // 檢查商品代碼唯一性
                    var isDuplicate = await IsProductCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("商品代碼已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    errors.Add("商品名稱為必填欄位");
                }

                if (string.IsNullOrWhiteSpace(entity.Barcode))
                {
                    errors.Add("條碼編號為必填欄位");
                }
                
                if (!entity.ProductCategoryId.HasValue || entity.ProductCategoryId <= 0)
                {
                    errors.Add("商品類別為必填欄位");
                }

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id, ProductCode = entity.Code });
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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .FirstOrDefaultAsync(p => p.Code == productCode);
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
                var query = context.Products.Where(p => p.Code == productCode);
                
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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.ProductCategoryId == productCategoryId)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductCategoryAsync), GetType(), _logger, new { ProductCategoryId = productCategoryId });
                throw;
            }
        }

        public async Task<List<Product>> GetBySupplierAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.ProductSuppliers.Any(ps => ps.SupplierId == supplierId && ps.Status == EntityStatus.Active))
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierAsync), GetType(), _logger, new { SupplierId = supplierId });
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
                    .Include(p => p.Unit)
                    .Include(p => p.Size)
                    .Where(p => p.Status == EntityStatus.Active)
                    .OrderBy(p => p.Name)
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
                    .Where(pc => pc.Status == EntityStatus.Active)
                    .OrderBy(pc => pc.Name)
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
                    .Where(s => s.Status == EntityStatus.Active)
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
                    .Where(u => u.Status == EntityStatus.Active)
                    .OrderBy(u => u.Name)
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
                    .Include(ps => ps.Unit)  // 新增 Unit 導航屬性載入
                    .Where(ps => ps.ProductId == productId)
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
                    .Where(ps => ps.ProductId == productId)
                    .ToListAsync();

                // 刪除不在新列表中的關聯
                var relationsToDelete = existingRelations
                    .Where(er => !productSuppliers.Any(ps => ps.Id == er.Id))
                    .ToList();

                foreach (var relation in relationsToDelete)
                {
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
                            UnitId = productSupplier.UnitId,  // 新增單位ID設定
                            Status = EntityStatus.Active,
                            
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
                            existingRelation.UnitId = productSupplier.UnitId;  // 新增單位ID更新
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

        #endregion

        #region 輔助方法

        public void InitializeNewProduct(Product product)
        {
            try
            {
                product.Code = string.Empty;
                product.Name = string.Empty;
                product.UnitId = null;
                product.SizeId = null;
                product.ProductCategoryId = null;
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

                if (!string.IsNullOrWhiteSpace(product.Code))
                    count++;

                if (!string.IsNullOrWhiteSpace(product.Name))
                    count++;

                return count;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicCompletedFieldsCount), GetType(), _logger, new { ProductId = product?.Id });
                throw;
            }
        }

        public override async Task<ServiceResult<Product>> UpdateAsync(Product entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查實體是否存在，並載入相關資料
                var existingEntity = await context.Products
                    .Include(p => p.ProductSuppliers)
                        .ThenInclude(ps => ps.Supplier)
                    .Include(p => p.ProductSuppliers)
                        .ThenInclude(ps => ps.Unit)
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                if (existingEntity == null)
                {
                    return ServiceResult<Product>.Failure("找不到要更新的資料");
                }

                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<Product>.Failure(validationResult.ErrorMessage);
                }

                // 保持審計資訊
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                // 更新基本屬性
                context.Entry(existingEntity).CurrentValues.SetValues(entity);

                // 處理 ProductSuppliers 集合
                if (entity.ProductSuppliers != null)
                {
                    // 移除不存在的 ProductSuppliers
                    var existingSupplierIds = entity.ProductSuppliers.Select(ps => ps.Id).ToList();
                    var toRemove = existingEntity.ProductSuppliers
                        .Where(ps => ps.Id > 0 && !existingSupplierIds.Contains(ps.Id))
                        .ToList();
                    
                    foreach (var item in toRemove)
                    {
                        context.ProductSuppliers.Remove(item);
                    }

                    // 更新或新增 ProductSuppliers
                    foreach (var supplierEntity in entity.ProductSuppliers)
                    {
                        if (supplierEntity.Id > 0)
                        {
                            // 更新existing
                            var existingSupplier = existingEntity.ProductSuppliers
                                .FirstOrDefault(ps => ps.Id == supplierEntity.Id);
                            if (existingSupplier != null)
                            {
                                // 保持審計資訊
                                supplierEntity.CreatedAt = existingSupplier.CreatedAt;
                                supplierEntity.CreatedBy = existingSupplier.CreatedBy;
                                supplierEntity.UpdatedAt = DateTime.UtcNow;
                                
                                context.Entry(existingSupplier).CurrentValues.SetValues(supplierEntity);
                            }
                        }
                        else
                        {
                            // 新增
                            supplierEntity.ProductId = entity.Id;
                            supplierEntity.CreatedAt = DateTime.UtcNow;
                            supplierEntity.UpdatedAt = DateTime.UtcNow;
                            context.ProductSuppliers.Add(supplierEntity);
                        }
                    }
                }

                await context.SaveChangesAsync();

                // 重新載入完整的實體以返回
                var updatedEntity = await GetByIdAsync(entity.Id);
                return ServiceResult<Product>.Success(updatedEntity ?? entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<Product>.Failure($"更新商品時發生錯誤: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作商品特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Product entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckProductDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("商品"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    ProductId = entity.Id 
                });
                return ServiceResult.Failure("檢查商品刪除條件時發生錯誤");
            }
        }

        #endregion
    }
}
