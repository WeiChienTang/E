using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單明細服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class QuotationDetailService : GenericManagementService<QuotationDetail>, IQuotationDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public QuotationDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<QuotationDetail>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public QuotationDetailService(
            IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<QuotationDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationDetails
                    .Include(qd => qd.Quotation)
                    .Include(qd => qd.Product)
                    .Include(qd => qd.Unit)
                    .AsQueryable()
                    .OrderBy(qd => qd.QuotationId)
                    .ThenBy(qd => qd.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<QuotationDetail>();
            }
        }

        public override async Task<QuotationDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationDetails
                    .Include(qd => qd.Quotation)
                    .Include(qd => qd.Product)
                    .Include(qd => qd.Unit)
                    .FirstOrDefaultAsync(qd => qd.Id == id);
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

        public override async Task<List<QuotationDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.QuotationDetails
                    .Include(qd => qd.Quotation)
                    .Include(qd => qd.Product)
                    .Include(qd => qd.Unit)
                    .Where(qd => (
                        qd.Product.Name.ToLower().Contains(lowerSearchTerm) ||
                        (qd.Product.Code != null && qd.Product.Code.ToLower().Contains(lowerSearchTerm)) ||
                        qd.Quotation.QuotationNumber.ToLower().Contains(lowerSearchTerm) ||
                        (qd.ProductDescription != null && qd.ProductDescription.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderBy(qd => qd.QuotationId)
                    .ThenBy(qd => qd.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<QuotationDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(QuotationDetail entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.QuotationId <= 0)
                    errors.Add("報價單為必選項目");

                if (entity.ProductId <= 0)
                    errors.Add("產品為必選項目");

                if (entity.Quantity <= 0)
                    errors.Add("報價數量必須大於 0");

                if (entity.UnitPrice < 0)
                    errors.Add("單價不能為負數");

                if (entity.DiscountPercentage < 0 || entity.DiscountPercentage > 100)
                    errors.Add("折扣百分比必須介於 0 到 100 之間");

                if (entity.ConvertedQuantity < 0)
                    errors.Add("已轉單數量不能為負數");

                if (entity.ConvertedQuantity > entity.Quantity)
                    errors.Add("已轉單數量不能大於報價數量");

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
                    QuotationId = entity.QuotationId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自定義方法

        public async Task<List<QuotationDetail>> GetByQuotationIdAsync(int quotationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationDetails
                    .Include(qd => qd.Product)
                    .Include(qd => qd.Unit)
                    .Where(qd => qd.QuotationId == quotationId)
                    .OrderBy(qd => qd.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByQuotationIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByQuotationIdAsync),
                    ServiceType = GetType().Name,
                    QuotationId = quotationId
                });
                return new List<QuotationDetail>();
            }
        }

        public async Task<ServiceResult> DeleteByQuotationIdAsync(int quotationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.QuotationDetails
                    .Where(qd => qd.QuotationId == quotationId)
                    .ToListAsync();

                if (details.Any())
                {
                    context.QuotationDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByQuotationIdAsync), GetType(), _logger, new {
                    Method = nameof(DeleteByQuotationIdAsync),
                    ServiceType = GetType().Name,
                    QuotationId = quotationId
                });
                return ServiceResult.Failure("刪除報價單明細過程發生錯誤");
            }
        }

        public async Task<List<QuotationDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationDetails
                    .Include(qd => qd.Quotation)
                    .Include(qd => qd.Product)
                    .Include(qd => qd.Unit)
                    .Where(qd => qd.ProductId == productId)
                    .OrderByDescending(qd => qd.Quotation.QuotationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId
                });
                return new List<QuotationDetail>();
            }
        }

        public async Task<bool> CanConvertToOrderAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.QuotationDetails
                    .Include(qd => qd.Quotation)
                    .FirstOrDefaultAsync(qd => qd.Id == detailId);

                if (detail == null)
                    return false;

                // 檢查報價單是否已核准
                if (!detail.Quotation.IsApproved)
                    return false;

                // 檢查是否還有剩餘數量可以轉單
                if (detail.RemainingQuantity <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanConvertToOrderAsync), GetType(), _logger, new {
                    Method = nameof(CanConvertToOrderAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId
                });
                return false;
            }
        }

        public async Task<ServiceResult> UpdateConvertedQuantityAsync(int detailId, decimal convertedQuantity)
        {
            try
            {
                if (convertedQuantity < 0)
                    return ServiceResult.Failure("已轉單數量不能為負數");

                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.QuotationDetails.FindAsync(detailId);

                if (detail == null)
                    return ServiceResult.Failure("找不到指定的報價單明細");

                if (convertedQuantity > detail.Quantity)
                    return ServiceResult.Failure("已轉單數量不能大於報價數量");

                detail.ConvertedQuantity = convertedQuantity;
                detail.UpdatedAt = DateTime.Now;

                // 更新主檔的轉單狀態
                var quotation = await context.Quotations
                    .Include(q => q.QuotationDetails)
                    .FirstOrDefaultAsync(q => q.Id == detail.QuotationId);

                if (quotation != null)
                {
                    // 如果所有明細都已全部轉單，則標記主檔為已轉單
                    var allConverted = quotation.QuotationDetails.All(qd => 
                        qd.Id == detailId ? convertedQuantity >= qd.Quantity : qd.ConvertedQuantity >= qd.Quantity);
                    
                    if (allConverted)
                    {
                        quotation.IsConverted = true;
                        quotation.ConvertedDate = DateTime.Now;
                    }
                    else
                    {
                        quotation.IsConverted = false;
                        quotation.ConvertedDate = null;
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateConvertedQuantityAsync), GetType(), _logger, new {
                    Method = nameof(UpdateConvertedQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId,
                    ConvertedQuantity = convertedQuantity
                });
                return ServiceResult.Failure("更新已轉單數量過程發生錯誤");
            }
        }

        #endregion
    }
}
