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
        private readonly ISystemParameterService _systemParameterService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger 和 ISystemParameterService
        /// </summary>
        public QuotationDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<QuotationDetail>> logger,
            ISystemParameterService systemParameterService) : base(contextFactory, logger)
        {
            _systemParameterService = systemParameterService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger，但需要 ISystemParameterService
        /// </summary>
        public QuotationDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ISystemParameterService systemParameterService) : base(contextFactory)
        {
            _systemParameterService = systemParameterService;
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
                        (qd.Quotation.Code != null && qd.Quotation.Code.ToLower().Contains(lowerSearchTerm))
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

        /// <summary>
        /// 獲取客戶最近一次完整的報價單明細（智能下單用）
        /// </summary>
        public async Task<List<QuotationDetail>> GetLastCompleteQuotationAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否啟用報價單審核
                var isApprovalEnabled = await _systemParameterService.IsQuotationApprovalEnabledAsync();
                
                // 建立查詢
                var query = context.Quotations
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Product)
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Unit)
                    .Where(q => q.CustomerId == customerId 
                              && q.QuotationDetails.Any());
                
                // 如果啟用審核，則只查詢已核准的報價單；否則查詢所有報價單
                if (isApprovalEnabled)
                {
                    query = query.Where(q => q.IsApproved);
                }
                
                var lastQuotation = await query
                    .OrderByDescending(q => q.QuotationDate)
                    .ThenByDescending(q => q.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastQuotation == null)
                {
                    return new List<QuotationDetail>();
                }
                
                // 返回該報價單的所有明細
                return lastQuotation.QuotationDetails
                    .Where(qd => qd.Product != null)
                    .OrderBy(qd => qd.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastCompleteQuotationAsync), GetType(), _logger, new { 
                    CustomerId = customerId
                });
                return new List<QuotationDetail>();
            }
        }

        #endregion
    }
}
