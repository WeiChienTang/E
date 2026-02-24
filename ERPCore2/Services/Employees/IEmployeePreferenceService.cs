using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工個人化設定服務介面
    /// </summary>
    public interface IEmployeePreferenceService : IGenericManagementService<EmployeePreference>
    {
        /// <summary>
        /// 根據員工 ID 取得個人化設定（不存在時回傳預設值，不寫入 DB）
        /// </summary>
        Task<EmployeePreference> GetByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// 儲存員工個人化設定（不存在則新增，存在則更新）
        /// </summary>
        Task<ServiceResult> SavePreferenceAsync(int employeeId, EmployeePreference preference);
    }
}
