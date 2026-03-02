using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 行事曆事項服務實作
    /// </summary>
    public class CalendarEventService : ICalendarEventService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<CalendarEventService> _logger;

        public CalendarEventService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<CalendarEventService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<CalendarEvent>> GetByMonthAsync(int employeeId, int year, int month)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CalendarEvents
                    .Where(e => e.EmployeeId == employeeId
                             && e.EventDate.Year == year
                             && e.EventDate.Month == month)
                    .OrderBy(e => e.EventDate)
                    .ThenBy(e => e.EventTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByMonthAsync), GetType(), _logger);
                return new List<CalendarEvent>();
            }
        }

        public async Task<List<CalendarEvent>> GetUpcomingAsync(int employeeId, int days = 7)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var endDate = today.AddDays(days);

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CalendarEvents
                    .Where(e => e.EmployeeId == employeeId
                             && e.EventDate >= today
                             && e.EventDate <= endDate)
                    .OrderBy(e => e.EventDate)
                    .ThenBy(e => e.EventTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUpcomingAsync), GetType(), _logger);
                return new List<CalendarEvent>();
            }
        }

        public async Task<ServiceResult<CalendarEvent>> CreateAsync(
            int employeeId,
            string title,
            DateOnly date,
            TimeOnly? time,
            CalendarEventColor color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult<CalendarEvent>.Failure("事項名稱不可為空");

                if (title.Length > 200)
                    return ServiceResult<CalendarEvent>.Failure("事項名稱不可超過 200 字元");

                using var context = await _contextFactory.CreateDbContextAsync();
                var calEvent = new CalendarEvent
                {
                    EmployeeId = employeeId,
                    Title = title.Trim(),
                    EventDate = date,
                    EventTime = time,
                    Color = color,
                    EventType = CalendarEventType.Personal,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.CalendarEvents.Add(calEvent);
                await context.SaveChangesAsync();
                return ServiceResult<CalendarEvent>.Success(calEvent);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<CalendarEvent>.Failure("新增事項失敗");
            }
        }

        public async Task<ServiceResult<CalendarEvent>> UpdateAsync(
            int eventId,
            int employeeId,
            string title,
            DateOnly date,
            TimeOnly? time,
            CalendarEventColor color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult<CalendarEvent>.Failure("事項名稱不可為空");

                if (title.Length > 200)
                    return ServiceResult<CalendarEvent>.Failure("事項名稱不可超過 200 字元");

                using var context = await _contextFactory.CreateDbContextAsync();
                var calEvent = await context.CalendarEvents
                    .FirstOrDefaultAsync(e => e.Id == eventId && e.EmployeeId == employeeId);

                if (calEvent == null)
                    return ServiceResult<CalendarEvent>.Failure("事項不存在或無權限修改");

                calEvent.Title = title.Trim();
                calEvent.EventDate = date;
                calEvent.EventTime = time;
                calEvent.Color = color;
                calEvent.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult<CalendarEvent>.Success(calEvent);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<CalendarEvent>.Failure("更新事項失敗");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int eventId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var calEvent = await context.CalendarEvents
                    .FirstOrDefaultAsync(e => e.Id == eventId && e.EmployeeId == employeeId);

                if (calEvent == null)
                    return ServiceResult.Failure("事項不存在或無權限刪除");

                context.CalendarEvents.Remove(calEvent);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除事項失敗");
            }
        }
    }
}
