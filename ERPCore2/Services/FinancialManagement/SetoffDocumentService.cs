using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款單服務實作
    /// </summary>
    public class SetoffDocumentService : GenericManagementService<SetoffDocument>, ISetoffDocumentService
    {
        public SetoffDocumentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffDocument>> logger) : base(contextFactory, logger)
        {
        }

        public SetoffDocumentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫 GetAllAsync 以包含關聯資料
        /// </summary>
        public override async Task<List<SetoffDocument>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 覆寫 GetByIdAsync 以包含關聯資料
        /// </summary>
        public override async Task<SetoffDocument?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Include(s => s.FinancialTransactions)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        /// <summary>
        /// 實作搜尋功能
        /// </summary>
        public override async Task<List<SetoffDocument>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s =>
                        s.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        s.Company.CompanyName.ToLower().Contains(searchTermLower) ||
                        s.RelatedPartyName.ToLower().Contains(searchTermLower))
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 實作驗證功能
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffDocument entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.SetoffNumber))
                    errors.Add("沖款單號不能為空");

                if (entity.SetoffDate == default)
                    errors.Add("沖款日期不能為空");

                if (entity.RelatedPartyId <= 0)
                    errors.Add("關聯方為必填");

                if (string.IsNullOrWhiteSpace(entity.RelatedPartyType))
                    errors.Add("關聯方類型不能為空");

                if (entity.CompanyId <= 0)
                    errors.Add("公司為必填");

                if (!string.IsNullOrWhiteSpace(entity.SetoffNumber) &&
                    await IsSetoffNumberExistsAsync(entity.SetoffNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("沖款單號已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    SetoffNumber = entity.SetoffNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 檢查沖款單號是否已存在
        /// </summary>
        public async Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffDocuments.Where(s => s.SetoffNumber == setoffNumber);
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSetoffNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSetoffNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SetoffNumber = setoffNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 根據沖款類型取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetBySetoffTypeAsync(SetoffType setoffType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.SetoffType == setoffType)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySetoffTypeAsync),
                    ServiceType = GetType().Name,
                    SetoffType = setoffType
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據關聯方ID取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByRelatedPartyIdAsync(int relatedPartyId, SetoffType? setoffType = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.RelatedPartyId == relatedPartyId);

                if (setoffType.HasValue)
                    query = query.Where(s => s.SetoffType == setoffType.Value);

                return await query
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByRelatedPartyIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByRelatedPartyIdAsync),
                    ServiceType = GetType().Name,
                    RelatedPartyId = relatedPartyId,
                    SetoffType = setoffType
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據公司ID取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.CompanyId == companyId)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCompanyIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCompanyIdAsync),
                    ServiceType = GetType().Name,
                    CompanyId = companyId
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據日期區間取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.SetoffDate >= startDate && s.SetoffDate <= endDate)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<SetoffDocument>();
            }
        }
    }
}
