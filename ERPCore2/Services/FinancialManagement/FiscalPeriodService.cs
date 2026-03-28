using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class FiscalPeriodService : GenericManagementService<FiscalPeriod>, IFiscalPeriodService
    {
        private readonly IAccountingAuditLogService _auditLogService;

        public FiscalPeriodService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<FiscalPeriod>> logger,
            IAccountingAuditLogService auditLogService,
            IFieldDisplaySettingService? fieldDisplaySettingService = null)
            : base(contextFactory, logger)
        {
            _auditLogService = auditLogService;
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        protected override IQueryable<FiscalPeriod> BuildGetAllQuery(AppDbContext context)
        {
            return context.FiscalPeriods
                .Include(f => f.Company)
                .OrderByDescending(f => f.FiscalYear)
                .ThenBy(f => f.PeriodNumber);
        }

        public override async Task<List<FiscalPeriod>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods
                    .Include(f => f.Company)
                    .Where(f => f.FiscalYear.ToString().Contains(searchTerm) ||
                               f.PeriodNumber.ToString().Contains(searchTerm))
                    .OrderByDescending(f => f.FiscalYear)
                    .ThenBy(f => f.PeriodNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<FiscalPeriod>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(FiscalPeriod entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.FiscalYear < 1900 || entity.FiscalYear > 2100)
                    errors.Add("會計年度必須介於 1900 至 2100 之間");

                if (entity.PeriodNumber < 1 || entity.PeriodNumber > 12)
                    errors.Add("期間編號必須介於 1 至 12 之間");

                if (entity.StartDate > entity.EndDate)
                    errors.Add("開始日期不可晚於結束日期");

                if (entity.CompanyId <= 0)
                    errors.Add("公司為必填欄位");

                // 檢查同公司同年度期間是否重複
                if (await IsYearPeriodExistsAsync(entity.FiscalYear, entity.PeriodNumber, entity.CompanyId,
                    entity.Id == 0 ? null : entity.Id))
                    errors.Add($"該公司 {entity.FiscalYear} 年第 {entity.PeriodNumber} 期已存在");

                return errors.Count == 0
                    ? ServiceResult.Success()
                    : ServiceResult.Failure(string.Join("；", errors));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證時發生錯誤");
            }
        }

        public async Task<List<FiscalPeriod>> GetByYearAsync(int year, int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods
                    .Include(f => f.Company)
                    .Where(f => f.FiscalYear == year && f.CompanyId == companyId)
                    .OrderBy(f => f.PeriodNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByYearAsync), GetType(), _logger,
                    new { Year = year, CompanyId = companyId });
                return new List<FiscalPeriod>();
            }
        }

        public async Task<FiscalPeriod?> GetByYearAndPeriodAsync(int year, int periodNumber, int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods
                    .Include(f => f.Company)
                    .FirstOrDefaultAsync(f =>
                        f.FiscalYear == year &&
                        f.PeriodNumber == periodNumber &&
                        f.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByYearAndPeriodAsync), GetType(), _logger,
                    new { Year = year, PeriodNumber = periodNumber, CompanyId = companyId });
                return null;
            }
        }

        public async Task<FiscalPeriod?> GetCurrentPeriodAsync(int companyId)
        {
            try
            {
                var today = DateTime.Today;
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods
                    .Include(f => f.Company)
                    .Where(f =>
                        f.CompanyId == companyId &&
                        f.PeriodStatus == FiscalPeriodStatus.Open &&
                        f.StartDate <= today &&
                        f.EndDate >= today)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCurrentPeriodAsync), GetType(), _logger,
                    new { CompanyId = companyId });
                return null;
            }
        }

        public async Task<List<FiscalPeriod>> GetOpenPeriodsAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods
                    .Where(f => f.CompanyId == companyId && f.PeriodStatus == FiscalPeriodStatus.Open)
                    .OrderByDescending(f => f.FiscalYear)
                    .ThenBy(f => f.PeriodNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOpenPeriodsAsync), GetType(), _logger,
                    new { CompanyId = companyId });
                return new List<FiscalPeriod>();
            }
        }

        public async Task<bool> IsDateInOpenPeriodAsync(DateTime date, int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FiscalPeriods.AnyAsync(f =>
                    f.CompanyId == companyId &&
                    f.PeriodStatus == FiscalPeriodStatus.Open &&
                    f.StartDate <= date &&
                    f.EndDate >= date);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDateInOpenPeriodAsync), GetType(), _logger,
                    new { Date = date, CompanyId = companyId });
                return false;
            }
        }

        public async Task<bool> IsYearPeriodExistsAsync(int year, int periodNumber, int companyId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.FiscalPeriods.Where(f =>
                    f.FiscalYear == year &&
                    f.PeriodNumber == periodNumber &&
                    f.CompanyId == companyId);

                if (excludeId.HasValue)
                    query = query.Where(f => f.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsYearPeriodExistsAsync), GetType(), _logger,
                    new { Year = year, PeriodNumber = periodNumber, CompanyId = companyId });
                return false;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> ClosePeriodAsync(int id, int? closedByEmployeeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var period = await context.FiscalPeriods.FindAsync(id);
                if (period == null)
                    return (false, "找不到指定期間");

                if (period.PeriodStatus != FiscalPeriodStatus.Open)
                    return (false, "僅開放中的期間可以關帳");

                period.PeriodStatus = FiscalPeriodStatus.Closed;
                period.ClosedAt = DateTime.UtcNow;
                period.ClosedByEmployeeId = closedByEmployeeId;
                period.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                await _auditLogService.LogAsync("ClosePeriod", "FiscalPeriod", period.Id,
                    period.DisplayName, $"關帳期間 {period.DisplayName}",
                    "Open", "Closed", period.CompanyId, closedByEmployeeId?.ToString());

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ClosePeriodAsync), GetType(), _logger,
                    new { Id = id });
                return (false, "關帳失敗，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> LockPeriodAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var period = await context.FiscalPeriods.FindAsync(id);
                if (period == null)
                    return (false, "找不到指定期間");

                if (period.PeriodStatus != FiscalPeriodStatus.Closed)
                    return (false, "僅已關帳的期間可以鎖定");

                period.PeriodStatus = FiscalPeriodStatus.Locked;
                period.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                await _auditLogService.LogAsync("LockPeriod", "FiscalPeriod", period.Id,
                    period.DisplayName, $"永久鎖定期間 {period.DisplayName}",
                    "Closed", "Locked", period.CompanyId, null);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LockPeriodAsync), GetType(), _logger,
                    new { Id = id });
                return (false, "鎖定失敗，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> ReopenPeriodAsync(int id, string? reason = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                    return (false, "重新開放期間必須填寫原因（稽核要求）");

                using var context = await _contextFactory.CreateDbContextAsync();
                var period = await context.FiscalPeriods.FindAsync(id);
                if (period == null)
                    return (false, "找不到指定期間");

                if (period.PeriodStatus != FiscalPeriodStatus.Closed)
                    return (false, "僅已關帳的期間可以重新開放（已鎖定的期間無法重新開放）");

                period.PeriodStatus = FiscalPeriodStatus.Open;
                period.ReopenReason = reason;
                period.ReopenedAt = DateTime.UtcNow;
                period.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                await _auditLogService.LogAsync("ReopenPeriod", "FiscalPeriod", period.Id,
                    period.DisplayName, $"重開期間 {period.DisplayName}，原因：{reason}",
                    "Closed", "Open", period.CompanyId, null);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReopenPeriodAsync), GetType(), _logger,
                    new { Id = id });
                return (false, "重新開放失敗，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage, int CreatedCount)> InitializeYearAsync(int year, int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                int created = 0;

                for (int month = 1; month <= 12; month++)
                {
                    var exists = await context.FiscalPeriods.AnyAsync(f =>
                        f.FiscalYear == year && f.PeriodNumber == month && f.CompanyId == companyId);

                    if (exists) continue;

                    var startDate = new DateTime(year, month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);

                    context.FiscalPeriods.Add(new FiscalPeriod
                    {
                        FiscalYear = year,
                        PeriodNumber = month,
                        StartDate = startDate,
                        EndDate = endDate,
                        PeriodStatus = FiscalPeriodStatus.Open,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    created++;
                }

                if (created > 0)
                    await context.SaveChangesAsync();

                return (true, string.Empty, created);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(InitializeYearAsync), GetType(), _logger,
                    new { Year = year, CompanyId = companyId });
                return (false, "初始化年度期間失敗，請稍後再試", 0);
            }
        }

        public async Task<(List<FiscalPeriod> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<FiscalPeriod>, IQueryable<FiscalPeriod>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<FiscalPeriod> query = context.FiscalPeriods
                    .Include(f => f.Company);

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(f => f.FiscalYear)
                    .ThenBy(f => f.PeriodNumber)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger,
                    new { PageNumber = pageNumber, PageSize = pageSize });
                return (new List<FiscalPeriod>(), 0);
            }
        }
    }
}
