using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;

namespace ERPCore2.Services
{
    /// <summary>
    /// 供應商定價服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierPricingService : GenericManagementService<SupplierPricing>, ISupplierPricingService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SupplierPricingService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SupplierPricing>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SupplierPricingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 必須實作的抽象方法

        /// <summary>
        /// 搜尋供應商定價
        /// </summary>
        public override async Task<List<SupplierPricing>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<SupplierPricing>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => !sp.IsDeleted && (
                        sp.Product.ProductName.Contains(searchTerm) ||
                        sp.Product.ProductCode.Contains(searchTerm) ||
                        sp.Supplier.CompanyName.Contains(searchTerm) ||
                        sp.Supplier.SupplierCode.Contains(searchTerm) ||
                        (sp.SupplierProductCode != null && sp.SupplierProductCode.Contains(searchTerm)) ||
                        sp.Currency.Contains(searchTerm)
                    ))
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 驗證供應商定價
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SupplierPricing entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (entity.ProductId <= 0)
                    errors.Add("商品為必填欄位");

                if (entity.SupplierId <= 0)
                    errors.Add("供應商為必填欄位");

                if (entity.PurchasePrice < 0)
                    errors.Add("採購價格不能為負數");

                if (string.IsNullOrWhiteSpace(entity.Currency))
                    errors.Add("貨幣代碼為必填欄位");

                if (entity.MinOrderQuantity.HasValue && entity.MinOrderQuantity < 0)
                    errors.Add("最小訂購量不能為負數");

                if (entity.LeadTimeDays.HasValue && entity.LeadTimeDays < 0)
                    errors.Add("交期天數不能為負數");

                if (entity.EffectiveDate == default)
                    errors.Add("生效日期為必填欄位");

                if (entity.ExpiryDate.HasValue && entity.ExpiryDate <= entity.EffectiveDate)
                    errors.Add("失效日期必須大於生效日期");

                // 檢查商品是否存在
                if (entity.ProductId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var productExists = await context.Products
                        .AnyAsync(p => p.Id == entity.ProductId && !p.IsDeleted);
                    if (!productExists)
                        errors.Add("指定的商品不存在");
                }

                // 檢查供應商是否存在
                if (entity.SupplierId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var supplierExists = await context.Suppliers
                        .AnyAsync(s => s.Id == entity.SupplierId && !s.IsDeleted);
                    if (!supplierExists)
                        errors.Add("指定的供應商不存在");
                }

                // 檢查供應商商品代碼是否重複
                if (!string.IsNullOrWhiteSpace(entity.SupplierProductCode) && entity.SupplierId > 0)
                {
                    var isCodeExists = await IsSupplierProductCodeExistsAsync(entity.SupplierId, entity.SupplierProductCode, entity.Id == 0 ? null : entity.Id);
                    if (isCodeExists)
                        errors.Add("供應商商品代碼已存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    ProductId = entity.ProductId,
                    SupplierId = entity.SupplierId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底方法

        /// <summary>
        /// 取得所有供應商定價（包含關聯資料）
        /// </summary>
        public override async Task<List<SupplierPricing>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => !sp.IsDeleted)
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 根據ID取得供應商定價（包含關聯資料）
        /// </summary>
        public override async Task<SupplierPricing?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .FirstOrDefaultAsync(sp => sp.Id == id && !sp.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return null;
            }
        }

        #endregion

        #region 專屬業務方法

        /// <summary>
        /// 根據商品ID取得供應商定價
        /// </summary>
        public async Task<List<SupplierPricing>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.ProductId == productId && !sp.IsDeleted)
                    .OrderBy(sp => sp.IsPrimarySupplier ? 0 : 1)
                    .ThenByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 根據供應商ID取得定價
        /// </summary>
        public async Task<List<SupplierPricing>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.SupplierId == supplierId && !sp.IsDeleted)
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 根據商品ID和供應商ID取得定價
        /// </summary>
        public async Task<List<SupplierPricing>> GetByProductIdAndSupplierIdAsync(int productId, int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.ProductId == productId && sp.SupplierId == supplierId && !sp.IsDeleted)
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAndSupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAndSupplierIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    SupplierId = supplierId 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 取得有效的供應商定價（未過期）
        /// </summary>
        public async Task<List<SupplierPricing>> GetEffectivePricingAsync(int productId, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.ProductId == productId && 
                                !sp.IsDeleted &&
                                sp.EffectiveDate <= checkDate &&
                                (sp.ExpiryDate == null || sp.ExpiryDate > checkDate))
                    .OrderBy(sp => sp.IsPrimarySupplier ? 0 : 1)
                    .ThenBy(sp => sp.PurchasePrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEffectivePricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetEffectivePricingAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    AsOfDate = asOfDate 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 根據商品ID取得主要供應商的定價
        /// </summary>
        public async Task<SupplierPricing?> GetPrimarySupplierPricingAsync(int productId, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.ProductId == productId && 
                                sp.IsPrimarySupplier &&
                                !sp.IsDeleted &&
                                sp.EffectiveDate <= checkDate &&
                                (sp.ExpiryDate == null || sp.ExpiryDate > checkDate))
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimarySupplierPricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetPrimarySupplierPricingAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    AsOfDate = asOfDate 
                });
                return null;
            }
        }

        /// <summary>
        /// 檢查供應商商品代碼是否已存在
        /// </summary>
        public async Task<bool> IsSupplierProductCodeExistsAsync(int supplierId, string supplierProductCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplierProductCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SupplierPricings
                    .Where(sp => sp.SupplierId == supplierId && 
                                sp.SupplierProductCode == supplierProductCode && 
                                !sp.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(sp => sp.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSupplierProductCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsSupplierProductCodeExistsAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    SupplierProductCode = supplierProductCode,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 根據供應商商品代碼查詢
        /// </summary>
        public async Task<List<SupplierPricing>> GetBySupplierProductCodeAsync(int supplierId, string supplierProductCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplierProductCode))
                    return new List<SupplierPricing>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.SupplierId == supplierId && 
                                sp.SupplierProductCode == supplierProductCode && 
                                !sp.IsDeleted)
                    .OrderByDescending(sp => sp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierProductCodeAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierProductCodeAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    SupplierProductCode = supplierProductCode 
                });
                return new List<SupplierPricing>();
            }
        }

        /// <summary>
        /// 取得即將到期的定價（指定天數內到期）
        /// </summary>
        public async Task<List<SupplierPricing>> GetExpiringPricingAsync(int daysFromNow = 30)
        {
            try
            {
                var checkDate = DateTime.Today.AddDays(daysFromNow);
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierPricings
                    .Include(sp => sp.Product)
                    .Include(sp => sp.Supplier)
                    .Where(sp => !sp.IsDeleted &&
                                sp.ExpiryDate.HasValue &&
                                sp.ExpiryDate <= checkDate &&
                                sp.ExpiryDate > DateTime.Today)
                    .OrderBy(sp => sp.ExpiryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetExpiringPricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetExpiringPricingAsync),
                    ServiceType = GetType().Name,
                    DaysFromNow = daysFromNow 
                });
                return new List<SupplierPricing>();
            }
        }

        #endregion
    }
}
