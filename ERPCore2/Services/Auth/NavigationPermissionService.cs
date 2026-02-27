using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Data.Context;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// 檢查目前登入員工是否為最高系統管理者（IsSuperAdmin = true）
        /// IsSuperAdmin 為唯一繞過公司模組限制的身分，不受任何存取控制
        /// </summary>
        Task<bool> IsCurrentEmployeeSuperAdminAsync();
    }

    public class NavigationPermissionService : INavigationPermissionService
    {
        private readonly IPermissionService _permissionService;
        private readonly ICompanyModuleService _companyModuleService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<NavigationPermissionService> _logger;
        private readonly IMemoryCache _cache;
        private const string ALL_PERMS_CACHE_PREFIX = "all_nav_perms_";
        private const string SUPERADMIN_CACHE_PREFIX = "is_superadmin_";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public NavigationPermissionService(
            IPermissionService permissionService,
            ICompanyModuleService companyModuleService,
            IDbContextFactory<AppDbContext> contextFactory,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<NavigationPermissionService> logger,
            IMemoryCache cache)
        {
            _permissionService = permissionService;
            _companyModuleService = companyModuleService;
            _contextFactory = contextFactory;
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

                // IsSuperAdmin 為最高權限，繞過所有模組限制（包含公司層級）
                // System.Admin 權限不能繞過公司層級模組限制
                if (await GetIsSuperAdminAsync(employeeId))
                    return true;

                // 公司層級：檢查此模組是否已啟用（System.Admin 亦受此限制）
                if (!await _companyModuleService.IsModuleEnabledAsync(module))
                    return false;

                // 使用者層級：檢查是否有該模組的任何權限或 System.Admin
                var allPermissions = await GetAllEmployeePermissionsAsync(employeeId);

                // System.Admin 在模組啟用前提下，擁有所有功能存取權
                if (allPermissions.Contains("System.Admin"))
                    return true;

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
        /// 注意：不會自動注入 System.Admin，IsSuperAdmin 的判斷由 GetIsSuperAdminAsync 處理
        /// </summary>
        public async Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId)
        {
            var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
            try
            {

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
                // 快取空結果 30 秒，避免 DB 不穩定時 NavMenu 50+ 個 CanAccessAsync 呼叫
                // 每次都重試 DB → 級聯失敗 → 初始化時間超過 200 秒 → 永遠「載入中...」
                _cache.Set(cacheKey, new HashSet<string>(), TimeSpan.FromSeconds(30));
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
                var superAdminCacheKey = $"{SUPERADMIN_CACHE_PREFIX}{employeeId}";
                _cache.Remove(superAdminCacheKey);
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

        /// <summary>
        /// 檢查目前登入員工是否為最高系統管理者（IsSuperAdmin = true）
        /// IsSuperAdmin 為唯一繞過公司模組限制的身分，不受任何存取控制
        /// </summary>
        public async Task<bool> IsCurrentEmployeeSuperAdminAsync()
        {
            try
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId <= 0) return false;
                return await GetIsSuperAdminAsync(employeeId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCurrentEmployeeSuperAdminAsync), GetType(), _logger, new {
                    Method = nameof(IsCurrentEmployeeSuperAdminAsync),
                    ServiceType = GetType().Name
                });
                return false;
            }
        }

        /// <summary>
        /// 從資料庫查詢員工的 IsSuperAdmin 狀態（含快取）
        /// </summary>
        private async Task<bool> GetIsSuperAdminAsync(int employeeId)
        {
            var cacheKey = $"{SUPERADMIN_CACHE_PREFIX}{employeeId}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            using var context = await _contextFactory.CreateDbContextAsync();
            var isSuperAdmin = await context.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => e.IsSuperAdmin)
                .FirstOrDefaultAsync();

            _cache.Set(cacheKey, isSuperAdmin, _cacheExpiration);
            return isSuperAdmin;
        }
    }
}
