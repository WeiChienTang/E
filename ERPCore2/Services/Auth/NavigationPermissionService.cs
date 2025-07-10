using ERPCore2.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ERPCore2.Services.Auth
{
    /// <summary>
    /// 導航權限檢查服務
    /// </summary>
    public interface INavigationPermissionService
    {
        Task<bool> CanAccessAsync(string permission);
        Task<bool> CanAccessModuleAsync(string module);
        Task<int> GetCurrentEmployeeIdAsync();
    }

    public class NavigationPermissionService : INavigationPermissionService
    {
        private readonly IPermissionService _permissionService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<NavigationPermissionService> _logger;

        public NavigationPermissionService(
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<NavigationPermissionService> logger)
        {
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
        }

        public async Task<bool> CanAccessAsync(string permission)
        {
            try
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId <= 0) return false;

                var result = await _permissionService.HasPermissionAsync(employeeId, permission);
                return result.IsSuccess && result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"檢查權限時發生錯誤: {permission}");
                return false;
            }
        }

        public async Task<bool> CanAccessModuleAsync(string module)
        {
            try
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId <= 0) return false;

                var result = await _permissionService.CanAccessModuleAsync(employeeId, module);
                return result.IsSuccess && result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"檢查模組權限時發生錯誤: {module}");
                return false;
            }
        }

        public async Task<int> GetCurrentEmployeeIdAsync()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (!user.Identity?.IsAuthenticated ?? true)
                    return 0;

                var employeeIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(employeeIdClaim, out int employeeId))
                    return employeeId;

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得目前員工ID時發生錯誤");
                return 0;
            }
        }
    }
}
