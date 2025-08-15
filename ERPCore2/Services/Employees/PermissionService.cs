using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 權限服務實作
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionService> _logger;
        private readonly IErrorLogService _errorLogService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public PermissionService(IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache, ILogger<PermissionService> logger, IErrorLogService errorLogService)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _logger = logger;
            _errorLogService = errorLogService;
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public PermissionService(IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache, IErrorLogService errorLogService)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _logger = null!; // 簡易建構子可以不提供 logger
            _errorLogService = errorLogService;
        }        /// <summary>
        /// 檢查員工是否具有特定權限
        /// </summary>
        public async Task<ServiceResult<bool>> HasPermissionAsync(int employeeId, string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                {
                    return ServiceResult<bool>.Failure("權限代碼不能為空");
                }

                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);
                if (!permissionCodes.IsSuccess || permissionCodes.Data == null)
                {
                    return ServiceResult<bool>.Failure(permissionCodes.ErrorMessage);
                }

                var hasPermission = permissionCodes.Data.Contains(permissionCode);
                return ServiceResult<bool>.Success(hasPermission);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(HasPermissionAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId, PermissionCode = permissionCode });
                
                return ServiceResult<bool>.Failure($"檢查權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查員工是否具有多個權限（需全部具備）
        /// </summary>
        public async Task<ServiceResult<bool>> HasAllPermissionsAsync(int employeeId, IEnumerable<string> permissionCodes)
        {
            try
            {
                if (permissionCodes == null || !permissionCodes.Any())
                    return ServiceResult<bool>.Success(true);                var employeePermissionCodes = await GetEmployeePermissionCodesAsync(employeeId);
                if (!employeePermissionCodes.IsSuccess || employeePermissionCodes.Data == null)
                    return ServiceResult<bool>.Failure(employeePermissionCodes.ErrorMessage ?? "無法取得員工權限");

                var hasAllPermissions = permissionCodes.All(code => employeePermissionCodes.Data.Contains(code));
                return ServiceResult<bool>.Success(hasAllPermissions);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(HasAllPermissionsAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId, PermissionCodes = permissionCodes });
                
                return ServiceResult<bool>.Failure($"檢查權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查員工是否具有任一權限（至少一個）
        /// </summary>
        public async Task<ServiceResult<bool>> HasAnyPermissionAsync(int employeeId, IEnumerable<string> permissionCodes)
        {
            try
            {
                if (permissionCodes == null || !permissionCodes.Any())
                    return ServiceResult<bool>.Success(false);                var employeePermissionCodes = await GetEmployeePermissionCodesAsync(employeeId);
                if (!employeePermissionCodes.IsSuccess || employeePermissionCodes.Data == null)
                    return ServiceResult<bool>.Failure(employeePermissionCodes.ErrorMessage ?? "無法取得員工權限");

                var hasAnyPermission = permissionCodes.Any(code => employeePermissionCodes.Data.Contains(code));
                return ServiceResult<bool>.Success(hasAnyPermission);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(HasAnyPermissionAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId, PermissionCodes = permissionCodes });
                
                return ServiceResult<bool>.Failure($"檢查權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得員工的所有權限
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> GetEmployeePermissionsAsync(int employeeId)
        {
            try
            {
                var cacheKey = $"employee_permissions_{employeeId}";
                
                if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions) && cachedPermissions != null)
                    return ServiceResult<List<Permission>>.Success(cachedPermissions);

                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .Include(e => e.Role)
                    .ThenInclude(r => r != null ? r.RolePermissions : null!)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<List<Permission>>.Failure("員工不存在");

                if (employee.Status != EntityStatus.Active)
                    return ServiceResult<List<Permission>>.Failure("員工帳號已停用");

                if (employee.Role == null)
                    return ServiceResult<List<Permission>>.Success(new List<Permission>());

                var permissions = employee.Role.RolePermissions
                    .Where(rp => !rp.IsDeleted && rp.Status == EntityStatus.Active && rp.Permission != null && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission!)
                    .ToList();

                // 快取權限資料
                _cache.Set(cacheKey, permissions, _cacheExpiration);

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetEmployeePermissionsAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId });
                
                return ServiceResult<List<Permission>>.Failure($"取得員工權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得員工的所有權限代碼
        /// </summary>
        public async Task<ServiceResult<List<string>>> GetEmployeePermissionCodesAsync(int employeeId)
        {            try
            {
                var permissionsResult = await GetEmployeePermissionsAsync(employeeId);
                if (!permissionsResult.IsSuccess || permissionsResult.Data == null)
                    return ServiceResult<List<string>>.Failure(permissionsResult.ErrorMessage ?? "無法取得員工權限");

                var permissionCodes = permissionsResult.Data
                    .Where(p => p.Code != null)
                    .Select(p => p.Code!)
                    .ToList();

                return ServiceResult<List<string>>.Success(permissionCodes);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetEmployeePermissionCodesAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId });
                
                return ServiceResult<List<string>>.Failure($"取得員工權限代碼時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得角色的所有權限
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> GetRolePermissionsAsync(int roleId)
        {
            try
            {                var cacheKey = $"role_permissions_{roleId}";
                
                if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions) && cachedPermissions != null)
                    return ServiceResult<List<Permission>>.Success(cachedPermissions);

                using var context = await _contextFactory.CreateDbContextAsync();
                var role = await context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                    return ServiceResult<List<Permission>>.Failure("角色不存在");

                var permissions = role.RolePermissions
                    .Where(rp => !rp.IsDeleted && rp.Status == EntityStatus.Active && rp.Permission != null && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission)
                    .ToList();

                // 快取權限資料
                _cache.Set(cacheKey, permissions, _cacheExpiration);

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetRolePermissionsAsync), 
                    GetType(), 
                    _logger, 
                    new { RoleId = roleId });
                
                return ServiceResult<List<Permission>>.Failure($"取得角色權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查權限代碼是否存在
        /// </summary>        /// <summary>
        /// 檢查權限代碼是否存在 - 簡化版本，不支援排除特定ID
        /// 如需更完整功能請使用 PermissionManagementService.IsPermissionCodeExistsAsync
        /// </summary>
        public async Task<ServiceResult<bool>> PermissionExistsAsync(string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<bool>.Failure("權限代碼不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var exists = await context.Permissions
                    .AnyAsync(p => p.Code == permissionCode && !p.IsDeleted);

                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(PermissionExistsAsync), 
                    GetType(), 
                    _logger, 
                    new { PermissionCode = permissionCode });
                
                return ServiceResult<bool>.Failure($"檢查權限代碼時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 刷新員工權限快取
        /// </summary>
        public async Task<ServiceResult> RefreshEmployeePermissionCacheAsync(int employeeId)
        {
            try
            {
                var cacheKey = $"employee_permissions_{employeeId}";
                _cache.Remove(cacheKey);

                // 重新載入權限到快取
                await GetEmployeePermissionsAsync(employeeId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(RefreshEmployeePermissionCacheAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId });
                
                return ServiceResult.Failure($"刷新員工權限快取時發生錯誤：{ex.Message}");
            }
        }        /// <summary>
        /// 清除所有權限快取
        /// </summary>
        public Task<ServiceResult> ClearAllPermissionCacheAsync()
        {
            try
            {
                // 由於IMemoryCache沒有提供清除所有項目的直接方法
                // 這裡使用反射來清除，如果失敗就回傳成功（因為下次載入時會重新取得資料）
                try
                {
                    if (_cache is MemoryCache memoryCache)
                    {
                        var fieldInfo = typeof(MemoryCache).GetField("_entries", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (fieldInfo?.GetValue(memoryCache) is System.Collections.IDictionary entries)
                        {
                            entries.Clear();
                        }
                    }
                }
                catch
                {
                    // 如果清除失敗，不影響主要功能，快取會自然過期
                }
                
                return Task.FromResult(ServiceResult.Success());
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(
                    ex, 
                    nameof(ClearAllPermissionCacheAsync), 
                    GetType(), 
                    _logger);
                
                return Task.FromResult(ServiceResult.Failure($"清除權限快取時發生錯誤：{ex.Message}"));
            }
        }        /// <summary>
        /// 取得模組的所有權限 - 委派給 PermissionManagementService
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> GetModulePermissionsAsync(string modulePrefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modulePrefix))
                    return ServiceResult<List<Permission>>.Failure("模組前綴不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var permissions = await context.Permissions
                    .Where(p => p.Code != null && p.Code.StartsWith(modulePrefix + ".") && !p.IsDeleted)
                    .OrderBy(p => p.Code)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetModulePermissionsAsync), 
                    GetType(), 
                    _logger, 
                    new { ModulePrefix = modulePrefix });
                
                return ServiceResult<List<Permission>>.Failure($"取得模組權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查員工是否可以存取指定模組
        /// </summary>
        public async Task<ServiceResult<bool>> CanAccessModuleAsync(int employeeId, string modulePrefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modulePrefix))
                    return ServiceResult<bool>.Failure("模組前綴不能為空");                var employeePermissionCodes = await GetEmployeePermissionCodesAsync(employeeId);
                if (!employeePermissionCodes.IsSuccess || employeePermissionCodes.Data == null)
                    return ServiceResult<bool>.Failure(employeePermissionCodes.ErrorMessage ?? "無法取得員工權限");

                // 檢查是否有任何該模組的權限
                var hasModuleAccess = employeePermissionCodes.Data
                    .Any(code => code.StartsWith(modulePrefix + "."));

                return ServiceResult<bool>.Success(hasModuleAccess);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(CanAccessModuleAsync), 
                    GetType(), 
                    _logger, 
                    new { EmployeeId = employeeId, ModulePrefix = modulePrefix });
                
                return ServiceResult<bool>.Failure($"檢查模組存取權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 清除特定角色的權限快取
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>操作結果</returns>
        public Task<ServiceResult> ClearRolePermissionCacheAsync(int roleId)
        {
            try
            {
                var cacheKey = $"role_permissions_{roleId}";
                _cache.Remove(cacheKey);
                
                return Task.FromResult(ServiceResult.Success());
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(
                    ex, 
                    nameof(ClearRolePermissionCacheAsync), 
                    GetType(), 
                    _logger);
                
                return Task.FromResult(ServiceResult.Failure($"清除角色權限快取時發生錯誤：{ex.Message}"));
            }
        }
    }
}

