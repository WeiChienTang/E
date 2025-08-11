using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 當前使用者資訊輔助類別
    /// </summary>
    public static class CurrentUserHelper
    {
        /// <summary>
        /// 取得當前使用者的完整姓名
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>使用者完整姓名</returns>
        public static async Task<string> GetCurrentUserFullNameAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                
                if (authState?.User?.Identity?.IsAuthenticated == true)
                {
                    // 從 Claims 取得使用者資訊
                    var firstName = authState.User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
                    var lastName = authState.User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
                    var userName = authState.User.Identity.Name ?? "";
                    
                    // 智慧型姓名組合
                    if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                    {
                        var fullName = $"{firstName}{lastName}".Trim();
                        return !string.IsNullOrEmpty(fullName) ? fullName : userName;
                    }
                    
                    return userName;
                }
                
                return "未登入使用者";
            }
            catch
            {
                return "系統使用者";
            }
        }
        
        /// <summary>
        /// 取得當前使用者的使用者名稱
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>使用者名稱</returns>
        public static async Task<string> GetCurrentUserNameAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.Identity?.Name ?? "未知使用者";
            }
            catch
            {
                return "系統使用者";
            }
        }
        
        /// <summary>
        /// 取得當前使用者的員工代碼
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>員工代碼</returns>
        public static async Task<string> GetCurrentEmployeeCodeAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.FindFirst("EmployeeCode")?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 取得當前使用者的部門
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>部門名稱</returns>
        public static async Task<string> GetCurrentDepartmentAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.FindFirst("Department")?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 取得當前使用者的角色名稱
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>角色名稱</returns>
        public static async Task<string> GetCurrentRoleAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 檢查當前使用者是否為管理員
        /// </summary>
        /// <param name="authenticationStateProvider">身份驗證狀態提供者</param>
        /// <returns>是否為管理員</returns>
        public static async Task<bool> IsCurrentUserAdminAsync(AuthenticationStateProvider authenticationStateProvider)
        {
            try
            {
                var roleName = await GetCurrentRoleAsync(authenticationStateProvider);
                var roleNameLower = roleName.Trim().ToLower();
                return roleNameLower == "管理員" || roleNameLower == "admin" || 
                       roleNameLower.Contains("管理員") || roleNameLower.Contains("admin");
            }
            catch
            {
                return false;
            }
        }
    }
}
