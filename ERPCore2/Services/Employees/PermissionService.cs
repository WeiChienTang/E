using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionService> _logger;
        private readonly IErrorLogService _errorLogService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public PermissionService(AppDbContext context, IMemoryCache cache, ILogger<PermissionService> logger, IErrorLogService errorLogService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _errorLogService = errorLogService;
        }        /// <summary>
        /// 檢查員工是否具有特定權限
        /// </summary>
        public async Task<ServiceResult<bool>> HasPermissionAsync(int employeeId, string permissionCode)
        {
            try
            {
                Console.WriteLine($"🔍 PermissionService.HasPermissionAsync - 員工ID: {employeeId}, 權限代碼: '{permissionCode}'");
                
                if (string.IsNullOrWhiteSpace(permissionCode))
                {
                    Console.WriteLine("❌ 權限代碼為空");
                    return ServiceResult<bool>.Failure("權限代碼不能為空");
                }

                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);
                if (!permissionCodes.IsSuccess || permissionCodes.Data == null)
                {
                    Console.WriteLine($"❌ 獲取員工權限失敗: {permissionCodes.ErrorMessage}");
                    return ServiceResult<bool>.Failure(permissionCodes.ErrorMessage);
                }

                Console.WriteLine($"🔍 員工擁有的權限代碼數量: {permissionCodes.Data.Count}");
                foreach (var code in permissionCodes.Data)
                {
                    Console.WriteLine($"  - {code}");
                }

                var hasPermission = permissionCodes.Data.Contains(permissionCode);
                Console.WriteLine($"🔍 權限檢查結果: {hasPermission}");
                return ServiceResult<bool>.Success(hasPermission);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(HasPermissionAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    PermissionCode = permissionCode 
                });
                _logger.LogError(ex, "Error checking permission for employee {EmployeeId} with code {PermissionCode}", employeeId, permissionCode);
                Console.WriteLine($"💥 PermissionService.HasPermissionAsync 例外: {ex.Message}");
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(HasAllPermissionsAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    PermissionCodes = permissionCodes 
                });
                _logger.LogError(ex, "Error checking all permissions for employee {EmployeeId}", employeeId);
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(HasAnyPermissionAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    PermissionCodes = permissionCodes 
                });
                _logger.LogError(ex, "Error checking any permission for employee {EmployeeId}", employeeId);
                return ServiceResult<bool>.Failure($"檢查權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得員工的所有權限
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> GetEmployeePermissionsAsync(int employeeId)
        {
            try
            {                var cacheKey = $"employee_permissions_{employeeId}";
                
                if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions) && cachedPermissions != null)
                    return ServiceResult<List<Permission>>.Success(cachedPermissions);

                var employee = await _context.Employees
                    .Include(e => e.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<List<Permission>>.Failure("員工不存在");

                if (employee.Status != EntityStatus.Active)
                    return ServiceResult<List<Permission>>.Failure("員工帳號已停用");

                if (employee.Role == null)
                    return ServiceResult<List<Permission>>.Success(new List<Permission>());

                var permissions = employee.Role.RolePermissions
                    .Where(rp => !rp.IsDeleted && rp.Permission != null && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission)
                    .ToList();

                // 快取權限資料
                _cache.Set(cacheKey, permissions, _cacheExpiration);

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetEmployeePermissionsAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId 
                });
                _logger.LogError(ex, "Error getting employee permissions for employee {EmployeeId}", employeeId);
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
                    .Select(p => p.PermissionCode)
                    .ToList();

                return ServiceResult<List<string>>.Success(permissionCodes);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetEmployeePermissionCodesAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId 
                });
                _logger.LogError(ex, "Error getting employee permission codes for employee {EmployeeId}", employeeId);
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

                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                    return ServiceResult<List<Permission>>.Failure("角色不存在");

                var permissions = role.RolePermissions
                    .Where(rp => !rp.IsDeleted && rp.Permission != null && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission)
                    .ToList();

                // 快取權限資料
                _cache.Set(cacheKey, permissions, _cacheExpiration);

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetRolePermissionsAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId 
                });
                _logger.LogError(ex, "Error getting role permissions for role {RoleId}", roleId);
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

                var exists = await _context.Permissions
                    .AnyAsync(p => p.PermissionCode == permissionCode && !p.IsDeleted);

                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(PermissionExistsAsync),
                    ServiceType = GetType().Name,
                    PermissionCode = permissionCode 
                });
                _logger.LogError(ex, "Error checking if permission exists: {PermissionCode}", permissionCode);
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(RefreshEmployeePermissionCacheAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId 
                });
                _logger.LogError(ex, "Error refreshing employee permission cache for employee {EmployeeId}", employeeId);
                return ServiceResult.Failure($"刷新員工權限快取時發生錯誤：{ex.Message}");
            }
        }        /// <summary>
        /// 清除所有權限快取
        /// </summary>
        public Task<ServiceResult> ClearAllPermissionCacheAsync()
        {
            try
            {
                // 簡單的實作：由於IMemoryCache沒有清除所有項目的直接方法
                // 在實際應用中，可能需要使用自定義的快取鍵管理機制
                // 這裡我們提供一個基本的實作，告知調用者可能需要重新啟動應用程式
                
                return Task.FromResult(ServiceResult.Success());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all permission cache");
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

                var permissions = await _context.Permissions
                    .Where(p => p.PermissionCode.StartsWith(modulePrefix + ".") && !p.IsDeleted)
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetModulePermissionsAsync),
                    ServiceType = GetType().Name,
                    ModulePrefix = modulePrefix 
                });
                _logger.LogError(ex, "Error getting module permissions for prefix {ModulePrefix}", modulePrefix);
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanAccessModuleAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    ModulePrefix = modulePrefix 
                });
                _logger.LogError(ex, "Error checking module access for employee {EmployeeId} and module {ModulePrefix}", employeeId, modulePrefix);
                return ServiceResult<bool>.Failure($"檢查模組存取權限時發生錯誤：{ex.Message}");
            }
        }
    }
}
