using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class QuotationService : GenericManagementService<Quotation>, IQuotationService
    {
        private readonly IQuotationDetailService? _detailService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger 和 QuotationDetailService
        /// </summary>
        public QuotationService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Quotation>> logger,
            IQuotationDetailService? detailService = null) : base(contextFactory, logger)
        {
            _detailService = detailService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public QuotationService(
            IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<Quotation>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .AsQueryable()
                    .OrderByDescending(q => q.QuotationDate)
                    .ThenBy(q => q.QuotationNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<Quotation>();
            }
        }

        public override async Task<Quotation?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Include(q => q.ApprovedByUser)
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Product)
                    .FirstOrDefaultAsync(q => q.Id == id);
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

        public override async Task<List<Quotation>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Where(q => (
                        q.QuotationNumber.ToLower().Contains(lowerSearchTerm) ||
                        q.Customer.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                        (q.Description != null && q.Description.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderByDescending(q => q.QuotationDate)
                    .ThenBy(q => q.QuotationNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<Quotation>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Quotation entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.QuotationNumber))
                    errors.Add("報價單號不能為空");

                if (entity.CustomerId <= 0)
                    errors.Add("客戶為必選項目");

                if (entity.QuotationDate == default)
                    errors.Add("報價日期不能為空");

                if (entity.ValidUntilDate.HasValue && entity.ValidUntilDate.Value < entity.QuotationDate)
                    errors.Add("有效期限不能早於報價日期");

                if (!string.IsNullOrWhiteSpace(entity.QuotationNumber) &&
                    await IsQuotationNumberExistsAsync(entity.QuotationNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("報價單號已存在");

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
                    EntityName = entity.QuotationNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自定義方法

        public async Task<bool> IsQuotationNumberExistsAsync(string quotationNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Quotations.Where(q => q.QuotationNumber == quotationNumber);
                if (excludeId.HasValue)
                    query = query.Where(q => q.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsQuotationNumberExistsAsync), GetType(), _logger, new {
                    Method = nameof(IsQuotationNumberExistsAsync),
                    ServiceType = GetType().Name,
                    QuotationNumber = quotationNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<Quotation>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Where(q => q.CustomerId == customerId)
                    .OrderByDescending(q => q.QuotationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<Quotation>();
            }
        }

        public async Task<List<Quotation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Where(q => q.QuotationDate >= startDate && q.QuotationDate <= endDate)
                    .OrderByDescending(q => q.QuotationDate)
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
                return new List<Quotation>();
            }
        }

        public async Task<Quotation?> GetWithDetailsAsync(int quotationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Include(q => q.ApprovedByUser)
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Product)
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Unit)
                    .FirstOrDefaultAsync(q => q.Id == quotationId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    QuotationId = quotationId
                });
                return null;
            }
        }

        public async Task<ServiceResult> CalculateTotalAmountAsync(int quotationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var quotation = await context.Quotations
                    .Include(q => q.QuotationDetails)
                    .FirstOrDefaultAsync(q => q.Id == quotationId);

                if (quotation == null)
                    return ServiceResult.Failure("找不到指定的報價單");

                var totalAmount = quotation.QuotationDetails.Sum(d => d.SubtotalAmount);
                var taxAmount = totalAmount * 0.05m; // 假設稅率 5%

                quotation.TotalAmount = totalAmount - quotation.DiscountAmount;
                quotation.TaxAmount = taxAmount;
                quotation.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalAmountAsync), GetType(), _logger, new {
                    Method = nameof(CalculateTotalAmountAsync),
                    ServiceType = GetType().Name,
                    QuotationId = quotationId
                });
                return ServiceResult.Failure("計算報價單總金額時發生錯誤");
            }
        }

        public async Task<List<Quotation>> GetUnconvertedQuotationsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Where(q => !q.IsConverted)
                    .OrderByDescending(q => q.QuotationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnconvertedQuotationsAsync), GetType(), _logger, new {
                    Method = nameof(GetUnconvertedQuotationsAsync),
                    ServiceType = GetType().Name
                });
                return new List<Quotation>();
            }
        }

        public async Task<List<Quotation>> GetApprovedUnconvertedQuotationsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Where(q => q.IsApproved && !q.IsConverted)
                    .OrderByDescending(q => q.QuotationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetApprovedUnconvertedQuotationsAsync), GetType(), _logger, new {
                    Method = nameof(GetApprovedUnconvertedQuotationsAsync),
                    ServiceType = GetType().Name
                });
                return new List<Quotation>();
            }
        }

        #endregion

        #region 刪除限制檢查

        /// <summary>
        /// 檢查報價單是否可以被刪除
        /// 如果報價單已經轉單，則不能刪除
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Quotation entity)
        {
            try
            {
                // 1. 基礎檢查（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                    return baseResult;

                // 2. 檢查是否已轉單
                if (entity.IsConverted)
                {
                    return ServiceResult.Failure($"無法刪除報價單「{entity.QuotationNumber}」，因為已經轉換成銷貨訂單");
                }

                // 3. 檢查是否已核准且已接受
                if (entity.IsApproved && entity.QuotationStatus == QuotationStatus.Accepted)
                {
                    return ServiceResult.Failure($"無法刪除報價單「{entity.QuotationNumber}」，因為已經核准並接受");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    QuotationNumber = entity.QuotationNumber
                });
                return ServiceResult.Failure("檢查刪除權限時發生錯誤");
            }
        }

        #endregion

        #region 覆寫刪除方法

        /// <summary>
        /// 覆寫刪除方法 - 刪除主檔時同步刪除明細
        /// </summary>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 獲取報價單資料（在刪除前）
                    var quotation = await GetByIdAsync(id);
                    if (quotation == null)
                        return ServiceResult.Failure("找不到要刪除的報價單");

                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(quotation);
                    if (!canDeleteResult.IsSuccess)
                        return canDeleteResult;

                    // 3. 刪除報價單明細（如果有明細服務）
                    if (_detailService != null)
                    {
                        var deleteDetailsResult = await _detailService.DeleteByQuotationIdAsync(id);
                        if (!deleteDetailsResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult.Failure($"刪除報價單明細失敗：{deleteDetailsResult.ErrorMessage}");
                        }
                    }

                    // 4. 執行軟刪除（主檔）
                    var entity = await context.Quotations.FirstOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }

                    entity.UpdatedAt = DateTime.UtcNow;

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return ServiceResult.Success();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("刪除報價單過程發生錯誤");
            }
        }

        /// <summary>
        /// 永久刪除報價單
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 先取得主記錄（含詳細資料）
                    var entity = await context.Quotations
                        .Include(q => q.QuotationDetails)
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                        return ServiceResult.Failure("找不到要刪除的資料");
                    
                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                        return canDeleteResult;

                    // 3. 永久刪除明細資料
                    var detailsToDelete = await context.QuotationDetails
                        .Where(qd => qd.QuotationId == id)
                        .ToListAsync();
                        
                    if (detailsToDelete.Any())
                    {
                        context.QuotationDetails.RemoveRange(detailsToDelete);
                    }

                    // 4. 永久刪除主檔
                    context.Quotations.Remove(entity);

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return ServiceResult.Success();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("永久刪除報價單過程發生錯誤");
            }
        }

        #endregion

        #region 批次列印查詢

        /// <summary>
        /// 根據批次列印條件查詢報價單（批次列印專用）
        /// </summary>
        public async Task<List<Quotation>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 建立基礎查詢（包含必要關聯資料）
                IQueryable<Quotation> query = context.Quotations
                    .Include(q => q.Customer)
                    .Include(q => q.Employee)
                    .Include(q => q.QuotationDetails)
                        .ThenInclude(qd => qd.Product)
                    .AsQueryable();

                // 日期範圍篩選
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(q => q.QuotationDate >= criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate.HasValue)
                {
                    var endDate = criteria.EndDate.Value.Date.AddDays(1);
                    query = query.Where(q => q.QuotationDate < endDate);
                }

                // 關聯實體篩選（客戶）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(q => criteria.RelatedEntityIds.Contains(q.CustomerId));
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(q => q.QuotationNumber.Contains(criteria.DocumentNumberKeyword));
                }

                // 排序：先按客戶分組，同客戶內再按日期和單據編號排序
                query = criteria.SortDirection == Models.SortDirection.Ascending
                    ? query.OrderBy(q => q.Customer.CompanyName)
                           .ThenBy(q => q.QuotationDate)
                           .ThenBy(q => q.QuotationNumber)
                    : query.OrderBy(q => q.Customer.CompanyName)
                           .ThenByDescending(q => q.QuotationDate)
                           .ThenBy(q => q.QuotationNumber);

                // 限制最大筆數
                if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
                {
                    query = query.Take(criteria.MaxResults.Value);
                }

                // 執行查詢
                var results = await query.ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByBatchCriteriaAsync),
                    ServiceType = GetType().Name,
                    Criteria = new
                    {
                        criteria.StartDate,
                        criteria.EndDate,
                        RelatedEntityCount = criteria.RelatedEntityIds?.Count ?? 0,
                        criteria.MaxResults
                    }
                });
                return new List<Quotation>();
            }
        }

        #endregion
    }
}
