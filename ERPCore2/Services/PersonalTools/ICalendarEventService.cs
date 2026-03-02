using ERPCore2.Data.Entities;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 行事曆事項服務介面
    /// Phase 1：個人手動新增事項
    /// Phase 2（預留）：ERP 資料整合（採購到期、交貨日等）
    /// </summary>
    public interface ICalendarEventService
    {
        /// <summary>
        /// 取得指定月份的所有事項
        /// </summary>
        Task<List<CalendarEvent>> GetByMonthAsync(int employeeId, int year, int month);

        /// <summary>
        /// 取得未來 N 天內的事項（包含今天）
        /// </summary>
        Task<List<CalendarEvent>> GetUpcomingAsync(int employeeId, int days = 7);

        /// <summary>
        /// 新增事項
        /// </summary>
        Task<ServiceResult<CalendarEvent>> CreateAsync(
            int employeeId,
            string title,
            DateOnly date,
            TimeOnly? time,
            CalendarEventColor color,
            int? reminderMinutes = null);

        /// <summary>
        /// 更新事項
        /// </summary>
        Task<ServiceResult<CalendarEvent>> UpdateAsync(
            int eventId,
            int employeeId,
            string title,
            DateOnly date,
            TimeOnly? time,
            CalendarEventColor color,
            int? reminderMinutes = null);

        /// <summary>
        /// 刪除事項（確保只能刪除自己的）
        /// </summary>
        Task<ServiceResult> DeleteAsync(int eventId, int employeeId);
    }
}
