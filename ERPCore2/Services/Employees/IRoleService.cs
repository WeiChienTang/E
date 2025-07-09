using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 角色服務介面
    /// </summary>
    public interface IRoleService : IGenericManagementService<Role>
    {
        /// <summary>
        /// 根據角色名稱取得角色
        /// </summary>
        /// <param name="roleName">角色名稱</param>
        /// <returns>角色資料</returns>
        Task<ServiceResult<Role>> GetByNameAsync(string roleName);

        /// <summary>
        /// 取得系統角色清單
        /// </summary>
        /// <returns>系統角色清單</returns>
        Task<ServiceResult<List<Role>>> GetSystemRolesAsync();

        /// <summary>
        /// 取得自訂角色清單
        /// </summary>
        /// <returns>自訂角色清單</returns>
        Task<ServiceResult<List<Role>>> GetCustomRolesAsync();

        /// <summary>
        /// 檢查角色名稱是否已存在
        /// </summary>
        /// <param name="roleName">角色名稱</param>
        /// <param name="excludeRoleId">排除的角色ID（用於更新時檢查）</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> IsRoleNameExistsAsync(string roleName, int? excludeRoleId = null);

        /// <summary>
        /// 為角色指派權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionIds">權限ID清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);

        /// <summary>
        /// 移除角色的權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionIds">要移除的權限ID清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds);

        /// <summary>
        /// 清除角色的所有權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ClearRolePermissionsAsync(int roleId);

        /// <summary>
        /// 複製角色權限到另一個角色
        /// </summary>
        /// <param name="sourceRoleId">來源角色ID</param>
        /// <param name="targetRoleId">目標角色ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CopyRolePermissionsAsync(int sourceRoleId, int targetRoleId);

        /// <summary>
        /// 取得角色的員工數量
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>員工數量</returns>
        Task<ServiceResult<int>> GetEmployeeCountByRoleAsync(int roleId);

        /// <summary>
        /// 檢查角色是否可以刪除（沒有員工使用且非系統角色）
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> CanDeleteRoleAsync(int roleId);

        /// <summary>
        /// 搜尋角色
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字</param>
        /// <returns>角色清單</returns>
        Task<ServiceResult<List<Role>>> SearchRolesAsync(string searchTerm);

        /// <summary>
        /// 取得可指派的角色清單（排除系統管理員等特殊角色）
        /// </summary>
        /// <returns>可指派角色清單</returns>
        Task<ServiceResult<List<Role>>> GetAssignableRolesAsync();

        /// <summary>
        /// 驗證角色資料
        /// </summary>
        /// <param name="role">角色資料</param>
        /// <returns>驗證結果</returns>
        ServiceResult<bool> ValidateRoleData(Role role);
    }
}

