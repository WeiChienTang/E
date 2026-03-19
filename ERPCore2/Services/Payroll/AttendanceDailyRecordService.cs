using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class AttendanceDailyRecordService : IAttendanceDailyRecordService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AttendanceDailyRecordService>? _logger;

        public AttendanceDailyRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<AttendanceDailyRecordService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<AttendanceDailyRecord>> GetByEmployeePeriodAsync(int employeeId, int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                int adYear = year + 1911;
                var from = new DateOnly(adYear, month, 1);
                var to = new DateOnly(adYear, month, DateTime.DaysInMonth(adYear, month));
                return await context.AttendanceDailyRecords
                    .Where(r => r.EmployeeId == employeeId && r.Date >= from && r.Date <= to)
                    .OrderBy(r => r.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeePeriodAsync), GetType(), _logger);
                return new List<AttendanceDailyRecord>();
            }
        }

        public async Task<List<AttendanceDailyRecord>> GetByPeriodAsync(int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                int adYear = year + 1911;
                var from = new DateOnly(adYear, month, 1);
                var to = new DateOnly(adYear, month, DateTime.DaysInMonth(adYear, month));
                return await context.AttendanceDailyRecords
                    .Include(r => r.Employee)
                    .Where(r => r.Date >= from && r.Date <= to)
                    .OrderBy(r => r.Employee.Name).ThenBy(r => r.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPeriodAsync), GetType(), _logger);
                return new List<AttendanceDailyRecord>();
            }
        }

        public async Task<ServiceResult<AttendanceDailyRecord>> UpsertAsync(AttendanceDailyRecord record, string? operatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.AttendanceDailyRecords
                    .FirstOrDefaultAsync(r => r.EmployeeId == record.EmployeeId && r.Date == record.Date);

                if (existing == null)
                {
                    record.CreatedAt = DateTime.UtcNow;
                    record.CreatedBy = operatedBy;
                    context.AttendanceDailyRecords.Add(record);
                }
                else
                {
                    existing.Status = record.Status;
                    existing.WorkHours = record.WorkHours;
                    existing.OvertimeHours1 = record.OvertimeHours1;
                    existing.OvertimeHours2 = record.OvertimeHours2;
                    existing.HolidayOvertimeHours = record.HolidayOvertimeHours;
                    existing.NationalHolidayHours = record.NationalHolidayHours;
                    existing.Remarks = record.Remarks;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = operatedBy;
                    record = existing;
                }

                await context.SaveChangesAsync();
                return ServiceResult<AttendanceDailyRecord>.Success(record);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpsertAsync), GetType(), _logger);
                return ServiceResult<AttendanceDailyRecord>.Failure("儲存逐日出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.AttendanceDailyRecords.FindAsync(id);
                if (item == null)
                    return ServiceResult.Failure("找不到指定的逐日記錄");

                context.AttendanceDailyRecords.Remove(item);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除逐日出勤記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> BatchInitAsync(int employeeId, int year, int month,
            AttendanceInitMode mode, string? createdBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                int adYear = year + 1911;
                int daysInMonth = DateTime.DaysInMonth(adYear, month);
                var from = new DateOnly(adYear, month, 1);
                var to = new DateOnly(adYear, month, daysInMonth);

                var existing = await context.AttendanceDailyRecords
                    .Where(r => r.EmployeeId == employeeId && r.Date >= from && r.Date <= to)
                    .Select(r => r.Date)
                    .ToHashSetAsync();

                int added = 0;
                for (int d = 1; d <= daysInMonth; d++)
                {
                    var date = new DateOnly(adYear, month, d);
                    if (existing.Contains(date)) continue;

                    var dow = date.DayOfWeek;
                    bool isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

                    DailyAttendanceStatus status = mode switch
                    {
                        AttendanceInitMode.WorkdaysAsPresent => isWeekend
                            ? DailyAttendanceStatus.RestDay
                            : DailyAttendanceStatus.Present,
                        AttendanceInitMode.AllAsRestDay => DailyAttendanceStatus.RestDay,
                        _ => DailyAttendanceStatus.RestDay
                    };

                    context.AttendanceDailyRecords.Add(new AttendanceDailyRecord
                    {
                        EmployeeId = employeeId,
                        Date = date,
                        Status = status,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy
                    });
                    added++;
                }

                await context.SaveChangesAsync();
                return new ServiceResult { IsSuccess = true, ErrorMessage = $"已建立 {added} 筆，{existing.Count} 筆已存在跳過" };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchInitAsync), GetType(), _logger);
                return ServiceResult.Failure("批次初始化逐日記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> BatchInitAllEmployeesAsync(int year, int month,
            AttendanceInitMode mode, string? createdBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employeeIds = await context.Employees
                    .Where(e => e.EmploymentStatus == EmployeeStatus.Active
                             || e.EmploymentStatus == EmployeeStatus.Probation)
                    .Where(e => !e.IsSuperAdmin)
                    .Select(e => e.Id)
                    .ToListAsync();

                int totalAdded = 0;
                foreach (var empId in employeeIds)
                {
                    var result = await BatchInitAsync(empId, year, month, mode, createdBy);
                    if (result.IsSuccess && int.TryParse(result.ErrorMessage?.Split(' ').FirstOrDefault(), out int n))
                        totalAdded += n;
                }

                return new ServiceResult { IsSuccess = true, ErrorMessage = $"已為 {employeeIds.Count} 名員工建立逐日記錄" };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchInitAllEmployeesAsync), GetType(), _logger);
                return ServiceResult.Failure("批次初始化時發生錯誤");
            }
        }
    }
}
