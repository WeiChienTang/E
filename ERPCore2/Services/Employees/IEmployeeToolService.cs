using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工工具配給紀錄服務介面
    /// </summary>
    public interface IEmployeeToolService : IGenericManagementService<EmployeeTool>
    {
        /// <summary>
        /// 取得指定員工的所有工具配給紀錄（依配給日期降序）
        /// </summary>
        Task<List<EmployeeTool>> GetByEmployeeAsync(int employeeId);
    }
}
