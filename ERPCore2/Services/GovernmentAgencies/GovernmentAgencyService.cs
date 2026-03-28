using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.GovernmentAgencies
{
    /// <summary>
    /// 公家機關服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class GovernmentAgencyService : GenericManagementService<GovernmentAgency>, IGovernmentAgencyService
    {
        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public GovernmentAgencyService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<GovernmentAgency>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public GovernmentAgencyService(
            IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<GovernmentAgency> BuildGetAllQuery(AppDbContext context)
        {
            return context.GovernmentAgencies
                .OrderBy(g => g.AgencyName);
        }

        public override async Task<GovernmentAgency?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.GovernmentAgencies
                    .FirstOrDefaultAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<GovernmentAgency>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.GovernmentAgencies
                    .Where(g => (g.AgencyName != null && g.AgencyName.Contains(searchTerm)) ||
                                (g.Code != null && g.Code.Contains(searchTerm)) ||
                                (g.AgencyCode != null && g.AgencyCode.Contains(searchTerm)) ||
                                (g.ContactPerson != null && g.ContactPerson.Contains(searchTerm)))
                    .OrderBy(g => g.AgencyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(GovernmentAgency entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var errors = new List<string>();

                // 檢查必要欄位
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("機關編號為必填");

                // 檢查機關名稱必填
                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.AgencyName))
                    && string.IsNullOrWhiteSpace(entity.AgencyName))
                    errors.Add("機關名稱為必填");

                // 檢查長度限制
                if (entity.Code?.Length > 20)
                    errors.Add("機關編號不可超過20個字元");

                if (entity.AgencyName?.Length > 100)
                    errors.Add("機關名稱不可超過100個字元");

                if (!string.IsNullOrEmpty(entity.ContactPerson) && entity.ContactPerson.Length > 50)
                    errors.Add("聯絡人不可超過50個字元");

                if (!string.IsNullOrEmpty(entity.AgencyCode) && entity.AgencyCode.Length > 20)
                    errors.Add("機關代碼不可超過20個字元");

                // 檢查編號是否重複
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    var isDuplicate = await context.GovernmentAgencies
                        .Where(g => g.Code == entity.Code)
                        .Where(g => g.Id != entity.Id)
                        .AnyAsync();

                    if (isDuplicate)
                        errors.Add("機關編號已存在");
                }

                // 檢查機關名稱是否重複
                if (!string.IsNullOrWhiteSpace(entity.AgencyName))
                {
                    var isNameDuplicate = await context.GovernmentAgencies
                        .Where(g => g.AgencyName == entity.AgencyName)
                        .Where(g => g.Id != entity.Id)
                        .AnyAsync();

                    if (isNameDuplicate)
                        errors.Add("機關名稱已存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id, Code = entity.Code });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<GovernmentAgency?> GetByAgencyCodeAsync(string code)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.GovernmentAgencies
                    .FirstOrDefaultAsync(g => g.Code == code);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByAgencyCodeAsync), GetType(), _logger,
                    new { Code = code });
                throw;
            }
        }

        public async Task<bool> IsGovernmentAgencyCodeExistsAsync(string code, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.GovernmentAgencies.Where(g => g.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(g => g.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsGovernmentAgencyCodeExistsAsync), GetType(), _logger,
                    new { Code = code, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<bool> IsAgencyNameExistsAsync(string agencyName, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(agencyName))
                    return false;

                var query = context.GovernmentAgencies.Where(g => g.AgencyName == agencyName);

                if (excludeId.HasValue)
                    query = query.Where(g => g.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsAgencyNameExistsAsync), GetType(), _logger,
                    new { AgencyName = agencyName, ExcludeId = excludeId });
                throw;
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewAgency(GovernmentAgency agency)
        {
            try
            {
                agency.Code = string.Empty;
                agency.AgencyName = string.Empty;
                agency.Status = EntityStatus.Active;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(InitializeNewAgency), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicRequiredFieldsCount()
        {
            try
            {
                return 2; // Code, AgencyName
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicRequiredFieldsCount), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicCompletedFieldsCount(GovernmentAgency agency)
        {
            try
            {
                int count = 0;

                if (!string.IsNullOrWhiteSpace(agency.Code))
                    count++;

                if (!string.IsNullOrWhiteSpace(agency.AgencyName))
                    count++;

                return count;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicCompletedFieldsCount), GetType(), _logger,
                    new { AgencyId = agency.Id });
                throw;
            }
        }

        #endregion

        #region 伺服器端分頁

        public async Task<(List<GovernmentAgency> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<GovernmentAgency>, IQueryable<GovernmentAgency>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<GovernmentAgency> query = context.GovernmentAgencies;
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(g => g.AgencyName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<GovernmentAgency>(), 0);
            }
        }

        #endregion
    }
}
