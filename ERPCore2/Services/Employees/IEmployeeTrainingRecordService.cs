using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工受訓紀錄服務介面
    /// </summary>
    public interface IEmployeeTrainingRecordService : IGenericManagementService<EmployeeTrainingRecord>
    {
        /// <summary>
        /// 取得指定員工的所有受訓紀錄（依受訓日期降序）
        /// </summary>
        Task<List<EmployeeTrainingRecord>> GetByEmployeeAsync(int employeeId);
    }
}
