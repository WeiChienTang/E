using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;

namespace ERPCore2.Services
{
    /// <summary>
    /// 價格歷史服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class PriceHistoryService : GenericManagementService<PriceHistory>, IPriceHistoryService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public PriceHistoryService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PriceHistory>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public PriceHistoryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 必須實作的抽象方法

        /// <summary>
        /// 搜尋價格歷史記錄
        /// </summary>
        public override async Task<List<PriceHistory>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<PriceHistory>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => !ph.IsDeleted && (
                        (ph.Product.Name != null && ph.Product.Name.Contains(searchTerm)) ||
                        (ph.Product.Code != null && ph.Product.Code.Contains(searchTerm)) ||
                        (ph.ChangeReason != null && ph.ChangeReason.Contains(searchTerm)) ||
                        (ph.ChangedByUserName != null && ph.ChangedByUserName.Contains(searchTerm)) ||
                        (ph.RelatedCustomer != null && ph.RelatedCustomer.CompanyName != null && ph.RelatedCustomer.CompanyName.Contains(searchTerm)) ||
                        (ph.RelatedSupplier != null && ph.RelatedSupplier.CompanyName != null && ph.RelatedSupplier.CompanyName.Contains(searchTerm))
                    ))
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 驗證價格歷史記錄
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(PriceHistory entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證
                if (entity.ProductId <= 0)
                    errors.Add("商品為必填欄位");

                if (entity.OldPrice < 0)
                    errors.Add("原價格不能為負數");

                if (entity.NewPrice < 0)
                    errors.Add("新價格不能為負數");

                if (string.IsNullOrWhiteSpace(entity.ChangeReason))
                    errors.Add("變更原因為必填欄位");

                if (entity.ChangedByUserId <= 0)
                    errors.Add("變更人員為必填欄位");

                if (entity.ChangeDate > DateTime.Now)
                    errors.Add("變更日期不能大於目前時間");

                // 檢查商品是否存在
                if (entity.ProductId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var productExists = await context.Products
                        .AnyAsync(p => p.Id == entity.ProductId && !p.IsDeleted);
                    if (!productExists)
                        errors.Add("指定的商品不存在");
                }

                // 檢查關聯客戶是否存在
                if (entity.RelatedCustomerId.HasValue && entity.RelatedCustomerId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var customerExists = await context.Customers
                        .AnyAsync(c => c.Id == entity.RelatedCustomerId && !c.IsDeleted);
                    if (!customerExists)
                        errors.Add("指定的關聯客戶不存在");
                }

                // 檢查關聯供應商是否存在
                if (entity.RelatedSupplierId.HasValue && entity.RelatedSupplierId > 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var supplierExists = await context.Suppliers
                        .AnyAsync(s => s.Id == entity.RelatedSupplierId && !s.IsDeleted);
                    if (!supplierExists)
                        errors.Add("指定的關聯供應商不存在");
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
                    ProductId = entity.ProductId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底方法

        /// <summary>
        /// 取得所有價格歷史記錄（包含關聯資料）
        /// </summary>
        public override async Task<List<PriceHistory>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 根據ID取得價格歷史記錄（包含關聯資料）
        /// </summary>
        public override async Task<PriceHistory?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .FirstOrDefaultAsync(ph => ph.Id == id && !ph.IsDeleted);
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
        /// 根據商品ID取得價格歷史
        /// </summary>
        public async Task<List<PriceHistory>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => ph.ProductId == productId && !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 根據商品ID和價格類型取得價格歷史
        /// </summary>
        public async Task<List<PriceHistory>> GetByProductIdAndPriceTypeAsync(int productId, PriceType priceType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => ph.ProductId == productId && ph.PriceType == priceType && !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAndPriceTypeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAndPriceTypeAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    PriceType = priceType 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 根據日期範圍取得價格歷史
        /// </summary>
        public async Task<List<PriceHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => ph.ChangeDate >= startDate && ph.ChangeDate <= endDate && !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 根據變更人員取得價格歷史
        /// </summary>
        public async Task<List<PriceHistory>> GetByChangedByUserIdAsync(int userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => ph.ChangedByUserId == userId && !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByChangedByUserIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByChangedByUserIdAsync),
                    ServiceType = GetType().Name,
                    UserId = userId 
                });
                return new List<PriceHistory>();
            }
        }

        /// <summary>
        /// 取得商品的最新價格歷史記錄
        /// </summary>
        public async Task<PriceHistory?> GetLatestByProductIdAndPriceTypeAsync(int productId, PriceType priceType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PriceHistories
                    .Include(ph => ph.Product)
                    .Include(ph => ph.RelatedCustomer)
                    .Include(ph => ph.RelatedSupplier)
                    .Where(ph => ph.ProductId == productId && ph.PriceType == priceType && !ph.IsDeleted)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLatestByProductIdAndPriceTypeAsync), GetType(), _logger, new { 
                    Method = nameof(GetLatestByProductIdAndPriceTypeAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    PriceType = priceType 
                });
                return null;
            }
        }

        /// <summary>
        /// 記錄價格變更
        /// </summary>
        public async Task<ServiceResult> RecordPriceChangeAsync(int productId, PriceType priceType, decimal oldPrice, decimal newPrice, string changeReason, int changedByUserId, string? changedByUserName = null, int? relatedCustomerId = null, int? relatedSupplierId = null, string? changeDetails = null)
        {
            try
            {
                var priceHistory = new PriceHistory
                {
                    ProductId = productId,
                    PriceType = priceType,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    ChangeReason = changeReason,
                    ChangeDate = DateTime.Now,
                    ChangedByUserId = changedByUserId,
                    ChangedByUserName = changedByUserName,
                    RelatedCustomerId = relatedCustomerId,
                    RelatedSupplierId = relatedSupplierId,
                    ChangeDetails = changeDetails
                };

                var result = await CreateAsync(priceHistory);
                if (result.IsSuccess)
                    return ServiceResult.Success();
                else
                    return ServiceResult.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RecordPriceChangeAsync), GetType(), _logger, new { 
                    Method = nameof(RecordPriceChangeAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    PriceType = priceType,
                    OldPrice = oldPrice,
                    NewPrice = newPrice 
                });
                return ServiceResult.Failure("記錄價格變更時發生錯誤");
            }
        }

        #endregion
    }
}
