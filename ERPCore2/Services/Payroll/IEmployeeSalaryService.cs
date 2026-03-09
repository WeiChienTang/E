using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IEmployeeSalaryService : IGenericManagementService<EmployeeSalary>
    {
        /// <summary>取得指定員工的所有薪資歷史記錄（依生效日期降冪）</summary>
        Task<List<EmployeeSalary>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>取得指定員工目前有效的薪資設定（ExpiryDate = null）</summary>
        Task<EmployeeSalary?> GetCurrentSalaryAsync(int employeeId);

        /// <summary>新增薪資設定時，自動將前一筆有效記錄的 ExpiryDate 設為前一天</summary>
        Task<ServiceResult> AddWithExpiryAsync(EmployeeSalary newSalary);
    }
}
