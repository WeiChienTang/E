using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工證照服務介面
    /// </summary>
    public interface IEmployeeLicenseService : IGenericManagementService<EmployeeLicense>
    {
        /// <summary>
        /// 取得指定員工的所有證照紀錄（依取得日期降序）
        /// </summary>
        Task<List<EmployeeLicense>> GetByEmployeeAsync(int employeeId);
    }
}
