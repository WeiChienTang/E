using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Payroll
{
    public interface IMonthlyAttendanceSummaryService
    {
        /// <summary>取得指定月份所有員工的出勤彙總</summary>
        Task<List<MonthlyAttendanceSummary>> GetByPeriodAsync(int year, int month);

        /// <summary>取得特定員工在指定月份的出勤彙總</summary>
        Task<MonthlyAttendanceSummary?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month);

        Task<ServiceResult<MonthlyAttendanceSummary>> CreateAsync(MonthlyAttendanceSummary model, string? createdBy = null);
        Task<ServiceResult<MonthlyAttendanceSummary>> UpdateAsync(MonthlyAttendanceSummary model, string? updatedBy = null);
        Task<ServiceResult> DeleteAsync(int id);

        /// <summary>
        /// 批次初始化：為指定月份所有在職員工建立預設全勤出勤記錄（已有記錄者跳過）
        /// </summary>
        Task<ServiceResult> BatchInitializeAsync(int year, int month, string? createdBy = null);

        /// <summary>鎖定出勤彙總（薪資計算後呼叫）</summary>
        Task<ServiceResult> LockAsync(int employeeId, int year, int month);

        /// <summary>取得所有已有出勤記錄的年月組合</summary>
        Task<List<(int Year, int Month)>> GetAvailablePeriodsAsync();

        /// <summary>
        /// 從逐日出勤記錄重新彙總，更新（或建立）MonthlyAttendanceSummary
        /// 月薪員工：彙總出勤天數、加班時數
        /// 時薪員工：另外加總 TotalWorkHours
        /// </summary>
        Task<ServiceResult<MonthlyAttendanceSummary>> RebuildFromDailyAsync(
            int employeeId, int year, int month, string? updatedBy = null);
    }
}
