using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services.Payroll
{
    public interface IAttendanceDailyRecordService
    {
        /// <summary>取得指定員工某月所有逐日記錄，依日期排序</summary>
        Task<List<AttendanceDailyRecord>> GetByEmployeePeriodAsync(int employeeId, int year, int month);

        /// <summary>取得指定月份所有員工的逐日記錄</summary>
        Task<List<AttendanceDailyRecord>> GetByPeriodAsync(int year, int month);

        /// <summary>新增或更新單筆逐日記錄（以 EmployeeId + Date 為唯一鍵）</summary>
        Task<ServiceResult<AttendanceDailyRecord>> UpsertAsync(AttendanceDailyRecord record, string? operatedBy = null);

        /// <summary>刪除單筆逐日記錄</summary>
        Task<ServiceResult> DeleteAsync(int id);

        /// <summary>
        /// 批次初始化：為指定員工的指定月份建立逐日記錄框架
        /// 已存在的記錄不會被覆蓋
        /// </summary>
        Task<ServiceResult> BatchInitAsync(int employeeId, int year, int month,
            AttendanceInitMode mode, string? createdBy = null);

        /// <summary>
        /// 批次初始化：為指定月份所有在職員工建立逐日記錄框架
        /// </summary>
        Task<ServiceResult> BatchInitAllEmployeesAsync(int year, int month,
            AttendanceInitMode mode, string? createdBy = null);
    }
}
