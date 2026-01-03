using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 部門服務介面
    /// </summary>
    public interface IDepartmentService : IGenericManagementService<Department>
    {
        /// <summary>
        /// 檢查部門編號是否已存在
        /// </summary>
        /// <param name="departmentCode">部門編號</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeId = null);

        /// <summary>
        /// 檢查是否可以刪除部門（沒有員工）
        /// </summary>
        /// <param name="departmentId">部門ID</param>
        /// <returns>是否可以刪除</returns>
        Task<bool> CanDeleteDepartmentAsync(int departmentId);

        /// <summary>
        /// 取得可用的主管員工列表
        /// </summary>
        /// <returns>員工列表</returns>
        Task<List<Employee>> GetAvailableManagersAsync();
    }
}
