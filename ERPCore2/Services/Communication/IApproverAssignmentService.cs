using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Communication
{
    /// <summary>
    /// 審核人員指派服務介面
    /// </summary>
    public interface IApproverAssignmentService
    {
        /// <summary>
        /// 取得指定模組的所有審核人員 EmployeeId
        /// </summary>
        Task<List<int>> GetApproverEmployeeIdsAsync(string moduleName);

        /// <summary>
        /// 取得指定模組的所有審核人員指派記錄
        /// </summary>
        Task<List<ApproverAssignment>> GetByModuleAsync(string moduleName);

        /// <summary>
        /// 儲存指定模組的審核人員指派（整批替換）
        /// </summary>
        Task<ServiceResult> SaveAssignmentsAsync(string moduleName, List<ApproverAssignment> assignments);
    }
}
