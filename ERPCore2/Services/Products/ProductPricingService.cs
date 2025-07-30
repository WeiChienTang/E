using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品定價服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class ProductPricingService : GenericManagementService<ProductPricing>, IProductPricingService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public ProductPricingService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<ProductPricing>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public ProductPricingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 必須實作的抽象方法

        /// <summary>
        /// 搜尋商品定價
        /// </summary>
        public override async Task<List<ProductPricing>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<ProductPricing>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => !pp.IsDeleted && (
                        pp.Product.ProductName.Contains(searchTerm) ||
                        pp.Product.ProductCode.Contains(searchTerm) ||
                        (pp.Customer != null && pp.Customer.CompanyName.Contains(searchTerm)) ||
                        (pp.Customer != null && pp.Customer.CustomerCode.Contains(searchTerm)) ||
                        pp.Currency.Contains(searchTerm) ||
                        (pp.PricingDescription != null && pp.PricingDescription.Contains(searchTerm))
                    ))
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 驗證商品定價
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(ProductPricing entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (entity.ProductId <= 0)
                    errors.Add("商品為必填欄位");

                if (entity.Price < 0)
                    errors.Add("價格不能為負數");

                if (string.IsNullOrWhiteSpace(entity.Currency))
                    errors.Add("貨幣代碼為必填欄位");

                if (entity.MinQuantity.HasValue && entity.MinQuantity < 0)
                    errors.Add("最小數量不能為負數");

                if (entity.MaxQuantity.HasValue && entity.MaxQuantity < 0)
                    errors.Add("最大數量不能為負數");

                if (entity.MinQuantity.HasValue && entity.MaxQuantity.HasValue && entity.MinQuantity > entity.MaxQuantity)
                    errors.Add("最小數量不能大於最大數量");

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

                // 檢查客戶是否存在
                if (entity.CustomerId.HasValue && entity.CustomerId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var customerExists = await context.Customers
                        .AnyAsync(c => c.Id == entity.CustomerId && !c.IsDeleted);
                    if (!customerExists)
                        errors.Add("指定的客戶不存在");
                }

                // 檢查重複的定價設定
                if (entity.ProductId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var duplicateQuery = context.ProductPricings
                        .Where(pp => pp.ProductId == entity.ProductId &&
                                    pp.PricingType == entity.PricingType &&
                                    pp.CustomerId == entity.CustomerId &&
                                    !pp.IsDeleted);
                    
                    if (entity.Id > 0)
                        duplicateQuery = duplicateQuery.Where(pp => pp.Id != entity.Id);

                    // 檢查日期重疊
                    duplicateQuery = duplicateQuery.Where(pp => 
                        (entity.ExpiryDate == null || pp.EffectiveDate < entity.ExpiryDate) &&
                        (pp.ExpiryDate == null || pp.ExpiryDate > entity.EffectiveDate));

                    if (await duplicateQuery.AnyAsync())
                        errors.Add("該商品在指定時間區間已有相同類型的定價設定");
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
                    CustomerId = entity.CustomerId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底方法

        /// <summary>
        /// 取得所有商品定價（包含關聯資料）
        /// </summary>
        public override async Task<List<ProductPricing>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => !pp.IsDeleted)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 根據ID取得商品定價（包含關聯資料）
        /// </summary>
        public override async Task<ProductPricing?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .FirstOrDefaultAsync(pp => pp.Id == id && !pp.IsDeleted);
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
        /// 根據商品ID取得商品定價
        /// </summary>
        public async Task<List<ProductPricing>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && !pp.IsDeleted)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 根據客戶ID取得定價
        /// </summary>
        public async Task<List<ProductPricing>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.CustomerId == customerId && !pp.IsDeleted)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 根據商品ID和客戶ID取得定價
        /// </summary>
        public async Task<List<ProductPricing>> GetByProductIdAndCustomerIdAsync(int productId, int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && pp.CustomerId == customerId && !pp.IsDeleted)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAndCustomerIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAndCustomerIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    CustomerId = customerId 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 根據定價類型取得定價
        /// </summary>
        public async Task<List<ProductPricing>> GetByPricingTypeAsync(PricingType pricingType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.PricingType == pricingType && !pp.IsDeleted)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPricingTypeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPricingTypeAsync),
                    ServiceType = GetType().Name,
                    PricingType = pricingType 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 取得有效的商品定價（未過期）
        /// </summary>
        public async Task<List<ProductPricing>> GetEffectivePricingAsync(int productId, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && 
                                !pp.IsDeleted &&
                                pp.EffectiveDate <= checkDate &&
                                (pp.ExpiryDate == null || pp.ExpiryDate > checkDate))
                    .OrderByDescending(pp => pp.Priority)
                    .ThenBy(pp => pp.Price)
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
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 取得商品對特定客戶的最佳價格
        /// </summary>
        public async Task<ProductPricing?> GetBestPriceForCustomerAsync(int productId, int? customerId, int quantity = 1, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 優先順序：客戶專屬 > 數量折扣 > 標準價格
                var query = context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && 
                                !pp.IsDeleted &&
                                pp.EffectiveDate <= checkDate &&
                                (pp.ExpiryDate == null || pp.ExpiryDate > checkDate) &&
                                (pp.MinQuantity == null || pp.MinQuantity <= quantity) &&
                                (pp.MaxQuantity == null || pp.MaxQuantity >= quantity));

                // 如果有指定客戶，優先查找客戶專屬價格
                if (customerId.HasValue)
                {
                    var customerSpecificPrice = await query
                        .Where(pp => pp.CustomerId == customerId)
                        .OrderByDescending(pp => pp.Priority)
                        .ThenBy(pp => pp.Price)
                        .FirstOrDefaultAsync();

                    if (customerSpecificPrice != null)
                        return customerSpecificPrice;
                }

                // 如果沒有客戶專屬價格，查找通用價格
                return await query
                    .Where(pp => pp.CustomerId == null)
                    .OrderByDescending(pp => pp.Priority)
                    .ThenBy(pp => pp.Price)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBestPriceForCustomerAsync), GetType(), _logger, new { 
                    Method = nameof(GetBestPriceForCustomerAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    CustomerId = customerId,
                    Quantity = quantity,
                    AsOfDate = asOfDate 
                });
                return null;
            }
        }

        /// <summary>
        /// 根據商品ID和數量取得適用的定價
        /// </summary>
        public async Task<List<ProductPricing>> GetApplicablePricingAsync(int productId, int quantity, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && 
                                !pp.IsDeleted &&
                                pp.EffectiveDate <= checkDate &&
                                (pp.ExpiryDate == null || pp.ExpiryDate > checkDate) &&
                                (pp.MinQuantity == null || pp.MinQuantity <= quantity) &&
                                (pp.MaxQuantity == null || pp.MaxQuantity >= quantity))
                    .OrderByDescending(pp => pp.Priority)
                    .ThenBy(pp => pp.Price)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetApplicablePricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetApplicablePricingAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    Quantity = quantity,
                    AsOfDate = asOfDate 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 取得商品的標準價格
        /// </summary>
        public async Task<ProductPricing?> GetStandardPricingAsync(int productId, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && 
                                pp.PricingType == PricingType.Standard &&
                                pp.CustomerId == null &&
                                !pp.IsDeleted &&
                                pp.EffectiveDate <= checkDate &&
                                (pp.ExpiryDate == null || pp.ExpiryDate > checkDate))
                    .OrderByDescending(pp => pp.EffectiveDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStandardPricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetStandardPricingAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    AsOfDate = asOfDate 
                });
                return null;
            }
        }

        /// <summary>
        /// 取得即將到期的定價（指定天數內到期）
        /// </summary>
        public async Task<List<ProductPricing>> GetExpiringPricingAsync(int daysFromNow = 30)
        {
            try
            {
                var checkDate = DateTime.Today.AddDays(daysFromNow);
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => !pp.IsDeleted &&
                                pp.ExpiryDate.HasValue &&
                                pp.ExpiryDate <= checkDate &&
                                pp.ExpiryDate > DateTime.Today)
                    .OrderBy(pp => pp.ExpiryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetExpiringPricingAsync), GetType(), _logger, new { 
                    Method = nameof(GetExpiringPricingAsync),
                    ServiceType = GetType().Name,
                    DaysFromNow = daysFromNow 
                });
                return new List<ProductPricing>();
            }
        }

        /// <summary>
        /// 根據優先順序取得定價
        /// </summary>
        public async Task<List<ProductPricing>> GetByPriorityAsync(int productId, int? customerId = null, DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.Today;
                
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductPricings
                    .Include(pp => pp.Product)
                    .Include(pp => pp.Customer)
                    .Where(pp => pp.ProductId == productId && 
                                !pp.IsDeleted &&
                                pp.EffectiveDate <= checkDate &&
                                (pp.ExpiryDate == null || pp.ExpiryDate > checkDate));

                if (customerId.HasValue)
                    query = query.Where(pp => pp.CustomerId == null || pp.CustomerId == customerId);

                return await query
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPriorityAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPriorityAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    CustomerId = customerId,
                    AsOfDate = asOfDate 
                });
                return new List<ProductPricing>();
            }
        }

        #endregion
    }
}
