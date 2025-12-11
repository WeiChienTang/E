using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品-供應商關聯服務實作
    /// </summary>
    public class ProductSupplierService : GenericManagementService<ProductSupplier>, IProductSupplierService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ProductSupplierService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductSupplier>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ProductSupplierService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<ProductSupplier>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .OrderBy(ps => ps.ProductId)
                    .ThenBy(ps => ps.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<ProductSupplier?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .FirstOrDefaultAsync(ps => ps.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ProductSupplier entity)
        {
            try
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

                // 檢查重複綁定
                if (entity.ProductId > 0 && entity.SupplierId > 0)
                {
                    var isDuplicate = await IsBindingExistsAsync(entity.ProductId, entity.SupplierId, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("此商品與供應商的綁定已存在");
                    }
                }

                // 驗證優先順序範圍
                if (entity.Priority < 1 || entity.Priority > 999)
                {
                    errors.Add("優先順序必須介於 1 到 999 之間");
                }

                // 驗證交貨天數範圍
                if (entity.LeadTimeDays.HasValue && (entity.LeadTimeDays.Value < 0 || entity.LeadTimeDays.Value > 365))
                {
                    errors.Add("交貨天數必須介於 0 到 365 之間");
                }

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { EntityId = entity.Id, ProductId = entity.ProductId, SupplierId = entity.SupplierId });
                return ServiceResult.Failure($"驗證商品-供應商綁定時發生錯誤: {ex.Message}");
            }
        }

        public override async Task<List<ProductSupplier>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .Where(ps => (ps.Product!.Name != null && ps.Product.Name.Contains(searchTerm)) ||
                                (ps.Product!.Code != null && ps.Product.Code.Contains(searchTerm)) ||
                                (ps.Supplier!.CompanyName != null && ps.Supplier.CompanyName.Contains(searchTerm)) ||
                                (ps.Supplier!.Code != null && ps.Supplier.Code.Contains(searchTerm)) ||
                                (ps.SupplierProductCode != null && ps.SupplierProductCode.Contains(searchTerm)))
                    .OrderByDescending(ps => ps.IsPreferred)
                    .ThenBy(ps => ps.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        #endregion

        #region 查詢方法

        /// <summary>
        /// 依商品ID取得所有綁定的供應商
        /// </summary>
        public async Task<List<ProductSupplier>> GetByProductIdAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId)
                    .OrderByDescending(ps => ps.IsPreferred)
                    .ThenBy(ps => ps.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, 
                    new { ProductId = productId });
                throw;
            }
        }

        /// <summary>
        /// 依供應商ID取得所有綁定的商品
        /// </summary>
        public async Task<List<ProductSupplier>> GetBySupplierIdAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.SupplierId == supplierId)
                    .OrderByDescending(ps => ps.IsPreferred)
                    .ThenBy(ps => ps.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, 
                    new { SupplierId = supplierId });
                throw;
            }
        }

        /// <summary>
        /// 取得指定商品的常用供應商列表（依優先順序排序）
        /// </summary>
        public async Task<List<ProductSupplier>> GetPreferredSuppliersByProductIdAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.ProductSuppliers
                    .Include(ps => ps.Product)
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.ProductId == productId && ps.IsPreferred)
                    .OrderBy(ps => ps.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPreferredSuppliersByProductIdAsync), GetType(), _logger, 
                    new { ProductId = productId });
                throw;
            }
        }

        /// <summary>
        /// 檢查商品-供應商綁定是否存在
        /// </summary>
        public async Task<bool> IsBindingExistsAsync(int productId, int supplierId, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.ProductSuppliers
                    .Where(ps => ps.ProductId == productId && ps.SupplierId == supplierId);

                if (excludeId.HasValue)
                {
                    query = query.Where(ps => ps.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsBindingExistsAsync), GetType(), _logger, 
                    new { ProductId = productId, SupplierId = supplierId, ExcludeId = excludeId });
                throw;
            }
        }

        #endregion

        #region 批次操作

        /// <summary>
        /// 從採購歷史自動建立供應商-商品綁定
        /// </summary>
        public async Task<int> ImportFromPurchaseHistoryAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 查詢該供應商的所有採購記錄（含商品、價格、日期）
                var purchaseData = await context.PurchaseOrders
                    .Where(po => po.SupplierId == supplierId && po.Status == EntityStatus.Active)
                    .SelectMany(po => po.PurchaseOrderDetails.Select(pod => new
                    {
                        ProductId = pod.ProductId,
                        pod.UnitPrice,
                        po.OrderDate
                    }))
                    .GroupBy(x => x.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        LastPrice = g.OrderByDescending(x => x.OrderDate).First().UnitPrice,
                        LastDate = g.Max(x => x.OrderDate),
                        PurchaseCount = g.Count()
                    })
                    .ToListAsync();

                // 取得已存在的綁定
                var existingBindings = await context.ProductSuppliers
                    .Where(ps => ps.SupplierId == supplierId)
                    .Select(ps => ps.ProductId)
                    .ToListAsync();

                // 篩選出尚未綁定的商品
                var newBindings = purchaseData
                    .Where(pd => !existingBindings.Contains(pd.ProductId))
                    .Select(pd => new ProductSupplier
                    {
                        ProductId = pd.ProductId,
                        SupplierId = supplierId,
                        IsPreferred = pd.PurchaseCount >= 3,  // 採購3次以上自動設為常用
                        Priority = pd.PurchaseCount >= 3 ? 100 : 999,
                        LastPurchasePrice = pd.LastPrice,
                        LastPurchaseDate = pd.LastDate,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now
                    })
                    .ToList();

                if (newBindings.Any())
                {
                    await context.ProductSuppliers.AddRangeAsync(newBindings);
                    await context.SaveChangesAsync();
                }

                return newBindings.Count;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ImportFromPurchaseHistoryAsync), GetType(), _logger, 
                    new { SupplierId = supplierId });
                throw;
            }
        }

        /// <summary>
        /// 批次更新最近採購價格（從採購單資料）
        /// </summary>
        public async Task<int> BatchUpdateLastPurchasePriceAsync(int? supplierId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.ProductSuppliers.AsQueryable();

                if (supplierId.HasValue)
                {
                    query = query.Where(ps => ps.SupplierId == supplierId.Value);
                }

                var bindings = await query.ToListAsync();
                int updatedCount = 0;

                foreach (var binding in bindings)
                {
                    // 查詢最近的採購記錄
                    var lastPurchase = await context.PurchaseOrders
                        .Where(po => po.SupplierId == binding.SupplierId && po.Status == EntityStatus.Active)
                        .SelectMany(po => po.PurchaseOrderDetails.Where(pod => pod.ProductId == binding.ProductId)
                            .Select(pod => new
                            {
                                pod.UnitPrice,
                                po.OrderDate
                            }))
                        .OrderByDescending(x => x.OrderDate)
                        .FirstOrDefaultAsync();

                    if (lastPurchase != null)
                    {
                        binding.LastPurchasePrice = lastPurchase.UnitPrice;
                        binding.LastPurchaseDate = lastPurchase.OrderDate;
                        binding.UpdatedAt = DateTime.Now;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                }

                return updatedCount;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateLastPurchasePriceAsync), GetType(), _logger, 
                    new { SupplierId = supplierId });
                throw;
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 更新指定綁定的最近採購價格和日期
        /// </summary>
        public async Task UpdateLastPurchaseInfoAsync(int productId, int supplierId, decimal price, DateTime date)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var binding = await context.ProductSuppliers
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.SupplierId == supplierId);

                if (binding != null)
                {
                    binding.LastPurchasePrice = price;
                    binding.LastPurchaseDate = date;
                    binding.UpdatedAt = DateTime.Now;

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateLastPurchaseInfoAsync), GetType(), _logger, 
                    new { ProductId = productId, SupplierId = supplierId, Price = price, Date = date });
                // 不拋出例外，避免影響主流程
            }
        }

        #endregion
    }
}
