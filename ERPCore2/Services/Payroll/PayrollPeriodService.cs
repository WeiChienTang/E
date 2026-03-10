using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class PayrollPeriodService : GenericManagementService<PayrollPeriod>, IPayrollPeriodService
    {
        public PayrollPeriodService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public PayrollPeriodService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PayrollPeriod>> logger) : base(contextFactory, logger) { }

        protected override IQueryable<PayrollPeriod> BuildGetAllQuery(AppDbContext context)
        {
            return context.PayrollPeriods
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month);
        }

        public override async Task<List<PayrollPeriod>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();

                // 支援年份或月份搜尋（如 "114" 或 "3"）
                if (!int.TryParse(searchTerm.Trim(), out int num))
                    return new List<PayrollPeriod>();

                return await context.PayrollPeriods
                    .Where(x => x.Year == num || x.Month == num)
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<PayrollPeriod>();
            }
        }

        public async Task<ServiceResult<PayrollPeriod>> OpenPeriodAsync(int year, int month, string? createdBy = null)
        {
            try
            {
                if (year < 100 || year > 200)
                    return ServiceResult<PayrollPeriod>.Failure("年份格式錯誤（民國年，如 114）");

                if (month < 1 || month > 12)
                    return ServiceResult<PayrollPeriod>.Failure("月份必須在 1～12 之間");

                if (await PeriodExistsAsync(year, month))
                    return ServiceResult<PayrollPeriod>.Failure($"薪資週期 {year} 年 {month} 月 已存在");

                var period = new PayrollPeriod
                {
                    Year = year,
                    Month = month,
                    PeriodStatus = PayrollPeriodStatus.Draft,
                    Code = $"{year:D3}{month:D2}",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };

                using var context = await _contextFactory.CreateDbContextAsync();
                context.PayrollPeriods.Add(period);
                await context.SaveChangesAsync();

                return ServiceResult<PayrollPeriod>.Success(period);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(OpenPeriodAsync), GetType(), _logger);
                return ServiceResult<PayrollPeriod>.Failure("開立薪資週期時發生錯誤");
            }
        }

        public async Task<ServiceResult> ClosePeriodAsync(int periodId, string? closedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var period = await context.PayrollPeriods.FindAsync(periodId);

                if (period == null)
                    return ServiceResult.Failure("找不到指定的薪資週期");

                if (period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult.Failure("此薪資週期已關帳");

                period.PeriodStatus = PayrollPeriodStatus.Closed;
                period.ClosedAt = DateTime.Now;
                period.ClosedBy = closedBy;
                period.UpdatedAt = DateTime.Now;
                period.UpdatedBy = closedBy;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ClosePeriodAsync), GetType(), _logger);
                return ServiceResult.Failure("關帳時發生錯誤");
            }
        }

        public async Task<PayrollPeriod?> GetCurrentOpenPeriodAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollPeriods
                    .Where(x => x.PeriodStatus != PayrollPeriodStatus.Closed)
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCurrentOpenPeriodAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<bool> PeriodExistsAsync(int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollPeriods.AnyAsync(x => x.Year == year && x.Month == month);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PeriodExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<int> GetRecordCountAsync(int periodId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollRecords.CountAsync(r => r.PayrollPeriodId == periodId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRecordCountAsync), GetType(), _logger);
                return 0;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PayrollPeriod entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.Year < 100 || entity.Year > 200)
                    errors.Add("年份格式錯誤（民國年，如 114）");

                if (entity.Month < 1 || entity.Month > 12)
                    errors.Add("月份必須在 1～12 之間");

                // 重複檢查（排除自身）
                if (errors.Count == 0)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    bool duplicate = await context.PayrollPeriods
                        .AnyAsync(x => x.Year == entity.Year && x.Month == entity.Month && x.Id != entity.Id);
                    if (duplicate)
                        errors.Add($"薪資週期 {entity.Year} 年 {entity.Month} 月 已存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(PayrollPeriod entity)
        {
            try
            {
                if (entity.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult.Failure("已關帳的薪資週期不可刪除");

                int recordCount = await GetRecordCountAsync(entity.Id);
                if (recordCount > 0)
                    return ServiceResult.Failure($"此薪資週期已有 {recordCount} 筆薪資記錄，無法刪除");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }
    }
}
