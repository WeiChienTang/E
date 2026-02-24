using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace ERPCore2.Services
{
    /// <summary>
    /// 導航權限檢查服務
    /// </summary>
    public interface INavigationPermissionService
    {
        Task<bool> CanAccessAsync(string permission);
        Task<bool> CanAccessModuleAsync(string module);
        Task<int> GetCurrentEmployeeIdAsync();
        
        /// <summary>
        /// 批次取得員工所有權限(含快取)
        /// </summary>
        Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId);
        
        /// <summary>
        /// 清除員工權限快取
        /// </summary>
        void ClearEmployeePermissionCache(int employeeId);
    }

    public class NavigationPermissionService : INavigationPermissionService
    {
        private readonly IPermissionService _permissionService;
        private readonly ICompanyModuleService _companyModuleService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<NavigationPermissionService> _logger;
        private readonly IMemoryCache _cache;
        private const string ALL_PERMS_CACHE_PREFIX = "all_nav_perms_";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public NavigationPermissionService(
            IPermissionService permissionService,
            ICompanyModuleService companyModuleService,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<NavigationPermissionService> logger,
            IMemoryCache cache)
        {
            _permissionService = permissionService;
            _companyModuleService = companyModuleService;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
            _cache = cache;
        }

        public async Task<bool> CanAccessAsync(string permission)
        {
            try
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId <= 0) return false;

                // 使用批次快取
                var allPermissions = await GetAllEmployeePermissionsAsync(employeeId);
                
                // 先檢查 System.Admin
                if (allPermissions.Contains("System.Admin"))
                    return true;
                
                // 再檢查特定權限
                return allPermissions.Contains(permission);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanAccessAsync), GetType(), _logger, new { 
                    Method = nameof(CanAccessAsync),
                    ServiceType = GetType().Name,
                    Permission = permission 
                });
                return false;
            }
        }

        public async Task<bool> CanAccessModuleAsync(string module)
        {
            try
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId <= 0) return false;

                // 使用批次快取取得使用者權限
                var allPermissions = await GetAllEmployeePermissionsAsync(employeeId);

                // SuperAdmin 跳過所有限制
                if (allPermissions.Contains("System.Admin"))
                    return true;

                // 公司層級：檢查此模組是否已啟用
                if (!await _companyModuleService.IsModuleEnabledAsync(module))
                    return false;

                // 使用者層級：檢查是否有該模組的任何權限
                return allPermissions.Any(p => p.StartsWith(module + ".", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanAccessModuleAsync), GetType(), _logger, new {
                    Method = nameof(CanAccessModuleAsync),
                    ServiceType = GetType().Name,
                    Module = module
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCurrentEmployeeIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetCurrentEmployeeIdAsync),
                    ServiceType = GetType().Name 
                });
                return 0;
            }
        }

        /// <summary>
        /// 批次取得員工所有權限(含快取)
        /// </summary>
        public async Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId)
        {
            try
            {
                var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
                
                // 檢查快取
                if (_cache.TryGetValue(cacheKey, out HashSet<string>? cachedPermissions) && cachedPermissions != null)
                {
                    return cachedPermissions;
                }
                
                // 從資料庫載入
                var result = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
                
                var permissions = new HashSet<string>(
                    result.Data ?? new List<string>(), 
                    StringComparer.OrdinalIgnoreCase
                );
                
                // 快取 10 分鐘
                _cache.Set(cacheKey, permissions, _cacheExpiration);
                
                return permissions;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllEmployeePermissionsAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllEmployeePermissionsAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// 清除員工權限快取
        /// </summary>
        public void ClearEmployeePermissionCache(int employeeId)
        {
            try
            {
                var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常
                _ = ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ClearEmployeePermissionCache), GetType(), _logger, new { 
                    Method = nameof(ClearEmployeePermissionCache),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
            }
        }
    }
}
