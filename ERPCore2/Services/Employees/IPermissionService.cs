using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 權限服務介面
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 檢查員工是否具有特定權限
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="permissionCode">權限代碼</param>
        /// <returns>權限檢查結果</returns>
        Task<ServiceResult<bool>> HasPermissionAsync(int employeeId, string permissionCode);

        /// <summary>
        /// 檢查員工是否具有多個權限（需全部具備）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="permissionCodes">權限代碼清單</param>
        /// <returns>權限檢查結果</returns>
        Task<ServiceResult<bool>> HasAllPermissionsAsync(int employeeId, IEnumerable<string> permissionCodes);

        /// <summary>
        /// 檢查員工是否具有任一權限（至少一個）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="permissionCodes">權限代碼清單</param>
        /// <returns>權限檢查結果</returns>
        Task<ServiceResult<bool>> HasAnyPermissionAsync(int employeeId, IEnumerable<string> permissionCodes);

        /// <summary>
        /// 取得員工的所有權限
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>員工權限清單</returns>
        Task<ServiceResult<List<Permission>>> GetEmployeePermissionsAsync(int employeeId);

        /// <summary>
        /// 取得員工的所有權限代碼
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>員工權限代碼清單</returns>
        Task<ServiceResult<List<string>>> GetEmployeePermissionCodesAsync(int employeeId);

        /// <summary>
        /// 取得角色的所有權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>角色權限清單</returns>
        Task<ServiceResult<List<Permission>>> GetRolePermissionsAsync(int roleId);

        /// <summary>
        /// 檢查權限代碼是否存在
        /// </summary>
        /// <param name="permissionCode">權限代碼</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> PermissionExistsAsync(string permissionCode);

        /// <summary>
        /// 刷新員工權限快取
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> RefreshEmployeePermissionCacheAsync(int employeeId);

        /// <summary>
        /// 清除所有權限快取
        /// </summary>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ClearAllPermissionCacheAsync();

        /// <summary>
        /// 取得模組的所有權限
        /// </summary>
        /// <param name="modulePrefix">模組前綴（例如：Customer, Product）</param>
        /// <returns>模組權限清單</returns>
        Task<ServiceResult<List<Permission>>> GetModulePermissionsAsync(string modulePrefix);

        /// <summary>
        /// 檢查員工是否可以存取指定模組
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="modulePrefix">模組前綴</param>
        /// <returns>存取檢查結果</returns>
        Task<ServiceResult<bool>> CanAccessModuleAsync(int employeeId, string modulePrefix);
    }
}

