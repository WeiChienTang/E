using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class MonthlyAttendanceSummaryService : IMonthlyAttendanceSummaryService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IPayrollPeriodService _periodService;
        private readonly ILogger<MonthlyAttendanceSummaryService>? _logger;

        public MonthlyAttendanceSummaryService(
            IDbContextFactory<AppDbContext> contextFactory,
            IPayrollPeriodService periodService,
            ILogger<MonthlyAttendanceSummaryService>? logger = null)
        {
            _contextFactory = contextFactory;
            _periodService = periodService;
            _logger = logger;
        }

        public async Task<List<MonthlyAttendanceSummary>> GetByPeriodAsync(int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MonthlyAttendanceSummaries
                    .Include(a => a.Employee)
                    .Where(a => a.Year == year && a.Month == month)
                    .OrderBy(a => a.Employee.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPeriodAsync), GetType(), _logger);
                return new List<MonthlyAttendanceSummary>();
            }
        }

        public async Task<MonthlyAttendanceSummary?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MonthlyAttendanceSummaries
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Year == year && a.Month == month);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAndPeriodAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult<MonthlyAttendanceSummary>> CreateAsync(MonthlyAttendanceSummary model, string? createdBy = null)
        {
            try
            {
                await _periodService.EnsurePeriodExistsAsync(model.Year, model.Month, createdBy);

                using var context = await _contextFactory.CreateDbContextAsync();

                bool exists = await context.MonthlyAttendanceSummaries
                    .AnyAsync(a => a.EmployeeId == model.EmployeeId && a.Year == model.Year && a.Month == model.Month);
                if (exists)
                    return ServiceResult<MonthlyAttendanceSummary>.Failure("該員工此月份的出勤記錄已存在");

                model.CreatedAt = DateTime.UtcNow;
                model.CreatedBy = createdBy;
                context.MonthlyAttendanceSummaries.Add(model);
                await context.SaveChangesAsync();
                return ServiceResult<MonthlyAttendanceSummary>.Success(model);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<MonthlyAttendanceSummary>.Failure("建立出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult<MonthlyAttendanceSummary>> UpdateAsync(MonthlyAttendanceSummary model, string? updatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.MonthlyAttendanceSummaries.FindAsync(model.Id);
                if (existing == null)
                    return ServiceResult<MonthlyAttendanceSummary>.Failure("找不到指定的出勤記錄");

                if (existing.IsLocked)
                    return ServiceResult<MonthlyAttendanceSummary>.Failure("此出勤記錄已鎖定，無法修改");

                existing.ScheduledWorkDays = model.ScheduledWorkDays;
                existing.ActualWorkDays = model.ActualWorkDays;
                existing.AbsentDays = model.AbsentDays;
                existing.SickLeaveDays = model.SickLeaveDays;
                existing.OvertimeHours1 = model.OvertimeHours1;
                existing.OvertimeHours2 = model.OvertimeHours2;
                existing.HolidayOvertimeHours = model.HolidayOvertimeHours;
                existing.NationalHolidayHours = model.NationalHolidayHours;
                existing.Remarks = model.Remarks;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = updatedBy;

                await context.SaveChangesAsync();
                return ServiceResult<MonthlyAttendanceSummary>.Success(existing);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<MonthlyAttendanceSummary>.Failure("更新出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.MonthlyAttendanceSummaries.FindAsync(id);
                if (item == null)
                    return ServiceResult.Failure("找不到指定的出勤記錄");

                if (item.IsLocked)
                    return ServiceResult.Failure("此出勤記錄已鎖定，無法刪除");

                context.MonthlyAttendanceSummaries.Remove(item);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> BatchInitializeAsync(int year, int month, string? createdBy = null)
        {
            try
            {
                await _periodService.EnsurePeriodExistsAsync(year, month, createdBy);

                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得所有在職員工
                var employees = await context.Employees
                    .Where(e => e.EmploymentStatus == EmployeeStatus.Active
                             || e.EmploymentStatus == EmployeeStatus.Probation)
                    .Select(e => new { e.Id })
                    .ToListAsync();

                // 已有記錄的員工
                var existing = await context.MonthlyAttendanceSummaries
                    .Where(a => a.Year == year && a.Month == month)
                    .Select(a => a.EmployeeId)
                    .ToListAsync();

                // 計算該月份工作日數（扣除週六日）
                int adYear = year + 1911;
                int scheduledDays = GetWorkDaysInMonth(adYear, month);

                int added = 0;
                foreach (var emp in employees)
                {
                    if (existing.Contains(emp.Id)) continue;

                    context.MonthlyAttendanceSummaries.Add(new MonthlyAttendanceSummary
                    {
                        EmployeeId = emp.Id,
                        Year = year,
                        Month = month,
                        ScheduledWorkDays = scheduledDays,
                        ActualWorkDays = scheduledDays,  // 預設全勤
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy
                    });
                    added++;
                }

                await context.SaveChangesAsync();
                // 利用 ErrorMessage 欄位回傳成功摘要訊息給 UI
                var msg = $"已為 {added} 名員工建立出勤記錄（{existing.Count} 筆已存在，跳過）";
                return new ServiceResult { IsSuccess = true, ErrorMessage = msg };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchInitializeAsync), GetType(), _logger);
                return ServiceResult.Failure("批次初始化出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> LockAsync(int employeeId, int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.MonthlyAttendanceSummaries
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Year == year && a.Month == month);

                if (item == null) return ServiceResult.Success(); // 無記錄無需鎖定

                item.IsLocked = true;
                item.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LockAsync), GetType(), _logger);
                return ServiceResult.Failure("鎖定出勤記錄時發生錯誤");
            }
        }

        public async Task<List<(int Year, int Month)>> GetAvailablePeriodsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var raw = await context.MonthlyAttendanceSummaries
                    .Select(a => new { a.Year, a.Month })
                    .Distinct()
                    .OrderByDescending(a => a.Year).ThenByDescending(a => a.Month)
                    .ToListAsync();
                return raw.Select(a => (a.Year, a.Month)).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailablePeriodsAsync), GetType(), _logger);
                return new List<(int Year, int Month)>();
            }
        }

        private static int GetWorkDaysInMonth(int adYear, int month)
        {
            int totalDays = DateTime.DaysInMonth(adYear, month);
            int workDays = 0;
            for (int d = 1; d <= totalDays; d++)
            {
                var dow = new DateTime(adYear, month, d).DayOfWeek;
                if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
                    workDays++;
            }
            return workDays;
        }
    }
}
