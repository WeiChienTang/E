using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 權限管理服務介面
    /// </summary>
    public interface IPermissionManagementService : IGenericManagementService<Permission>
    {
        /// <summary>
        /// 根據權限代碼取得權限
        /// </summary>
        /// <param name="permissionCode">權限代碼</param>
        /// <returns>權限資料</returns>
        Task<ServiceResult<Permission>> GetByCodeAsync(string permissionCode);

        /// <summary>
        /// 取得模組的所有權限
        /// </summary>
        /// <param name="modulePrefix">模組前綴</param>
        /// <returns>模組權限清單</returns>
        Task<ServiceResult<List<Permission>>> GetPermissionsByModuleAsync(string modulePrefix);

        /// <summary>
        /// 檢查權限代碼是否已存在
        /// </summary>
        /// <param name="permissionCode">權限代碼</param>
        /// <param name="excludePermissionId">排除的權限ID</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> IsPermissionCodeExistsAsync(string permissionCode, int? excludePermissionId = null);

        /// <summary>
        /// 取得所有模組清單
        /// </summary>
        /// <returns>模組前綴清單</returns>
        Task<ServiceResult<List<string>>> GetAllModulesAsync();

        /// <summary>
        /// 批次建立權限
        /// </summary>
        /// <param name="permissions">權限清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CreatePermissionsBatchAsync(List<Permission> permissions);

        /// <summary>
        /// 搜尋權限
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字</param>
        /// <returns>權限清單</returns>
        Task<ServiceResult<List<Permission>>> SearchPermissionsAsync(string searchTerm);

        /// <summary>
        /// 驗證權限代碼格式
        /// </summary>
        /// <param name="permissionCode">權限代碼</param>
        /// <returns>驗證結果</returns>
        ServiceResult<bool> ValidatePermissionCode(string permissionCode);
    }
}

