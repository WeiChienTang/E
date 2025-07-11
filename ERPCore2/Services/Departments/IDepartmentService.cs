using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 部門服務介面
    /// </summary>
    public interface IDepartmentService : IGenericManagementService<Department>
    {
        /// <summary>
        /// 檢查部門代碼是否已存在
        /// </summary>
        /// <param name="departmentCode">部門代碼</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeId = null);

        /// <summary>
        /// 獲取頂級部門（無上級部門）
        /// </summary>
        /// <returns>頂級部門列表</returns>
        Task<List<Department>> GetTopLevelDepartmentsAsync();

        /// <summary>
        /// 獲取指定部門的下級部門
        /// </summary>
        /// <param name="parentId">上級部門ID</param>
        /// <returns>下級部門列表</returns>
        Task<List<Department>> GetChildDepartmentsAsync(int parentId);

        /// <summary>
        /// 獲取部門階層樹狀結構
        /// </summary>
        /// <returns>部門樹狀結構</returns>
        Task<List<Department>> GetDepartmentTreeAsync();

        /// <summary>
        /// 檢查是否可以刪除部門（沒有員工和下級部門）
        /// </summary>
        /// <param name="departmentId">部門ID</param>
        /// <returns>是否可以刪除</returns>
        Task<bool> CanDeleteDepartmentAsync(int departmentId);
    }
}
