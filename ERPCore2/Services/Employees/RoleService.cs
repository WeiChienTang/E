using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 角色服務實作
    /// </summary>
    public class RoleService : GenericManagementService<Role>, IRoleService
    {
        public RoleService(
            AppDbContext context, 
            ILogger<GenericManagementService<Role>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        // 覆寫 GetAllAsync 以載入相關資料
        public override async Task<List<Role>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all roles");
                throw;
            }
        }

        // 覆寫 GetByIdAsync 以載入相關資料
        public override async Task<Role?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    RoleId = id 
                });
                _logger.LogError(ex, "Error getting role by ID {RoleId}", id);
                throw;
            }
        }

        /// <summary>
        /// 根據角色名稱取得角色
        /// </summary>
        public async Task<ServiceResult<Role>> GetByNameAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return ServiceResult<Role>.Failure("角色名稱不能為空");

                var role = await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.RoleName == roleName && !r.IsDeleted);

                if (role == null)
                    return ServiceResult<Role>.Failure("找不到指定的角色");

                return ServiceResult<Role>.Success(role);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByNameAsync),
                    ServiceType = GetType().Name,
                    RoleName = roleName 
                });
                _logger.LogError(ex, "Error getting role by name {RoleName}", roleName);
                return ServiceResult<Role>.Failure($"取得角色資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得系統角色清單
        /// </summary>
        public async Task<ServiceResult<List<Role>>> GetSystemRolesAsync()
        {
            try
            {
                var roles = await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(r => r.IsSystemRole && !r.IsDeleted)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetSystemRolesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting system roles");
                return ServiceResult<List<Role>>.Failure($"取得系統角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得自訂角色清單
        /// </summary>
        public async Task<ServiceResult<List<Role>>> GetCustomRolesAsync()
        {
            try
            {
                var roles = await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(r => !r.IsSystemRole && !r.IsDeleted)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetCustomRolesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting custom roles");
                return ServiceResult<List<Role>>.Failure($"取得自訂角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查角色名稱是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsRoleNameExistsAsync(string roleName, int? excludeRoleId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return ServiceResult<bool>.Failure("角色名稱不能為空");

                var query = _dbSet.Where(r => r.RoleName == roleName && !r.IsDeleted);

                if (excludeRoleId.HasValue)
                    query = query.Where(r => r.Id != excludeRoleId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsRoleNameExistsAsync),
                    ServiceType = GetType().Name,
                    RoleName = roleName,
                    ExcludeRoleId = excludeRoleId 
                });
                _logger.LogError(ex, "Error checking if role name exists {RoleName}", roleName);
                return ServiceResult<bool>.Failure($"檢查角色名稱時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 為角色指派權限
        /// </summary>
        public async Task<ServiceResult> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            try
            {
                var role = await GetByIdAsync(roleId);
                if (role == null)
                    return ServiceResult.Failure("角色不存在");

                if (role.IsSystemRole)
                    return ServiceResult.Failure("無法修改系統角色的權限");

                // 檢查權限是否都存在
                var validPermissions = await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id) && !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (validPermissions.Count != permissionIds.Count)
                    return ServiceResult.Failure("部分權限不存在或已被刪除");

                // 先清除現有權限
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(existingRolePermissions);

                // 添加新權限
                var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = EntityStatus.Active
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(newRolePermissions);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(AssignPermissionsToRoleAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId,
                    PermissionIds = permissionIds 
                });
                _logger.LogError(ex, "Error assigning permissions to role {RoleId}", roleId);
                return ServiceResult.Failure($"指派權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 移除角色的權限
        /// </summary>
        public async Task<ServiceResult> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds)
        {
            try
            {
                var role = await GetByIdAsync(roleId);
                if (role == null)
                    return ServiceResult.Failure("角色不存在");

                if (role.IsSystemRole)
                    return ServiceResult.Failure("無法修改系統角色的權限");

                var rolePermissionsToRemove = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(rolePermissionsToRemove);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(RemovePermissionsFromRoleAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId,
                    PermissionIds = permissionIds 
                });
                _logger.LogError(ex, "Error removing permissions from role {RoleId}", roleId);
                return ServiceResult.Failure($"移除權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 清除角色的所有權限
        /// </summary>
        public async Task<ServiceResult> ClearRolePermissionsAsync(int roleId)
        {
            try
            {
                var role = await GetByIdAsync(roleId);
                if (role == null)
                    return ServiceResult.Failure("角色不存在");

                if (role.IsSystemRole)
                    return ServiceResult.Failure("無法修改系統角色的權限");

                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(rolePermissions);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ClearRolePermissionsAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId 
                });
                _logger.LogError(ex, "Error clearing role permissions for role {RoleId}", roleId);
                return ServiceResult.Failure($"清除角色權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 複製角色權限到另一個角色
        /// </summary>
        public async Task<ServiceResult> CopyRolePermissionsAsync(int sourceRoleId, int targetRoleId)
        {
            try
            {
                var sourceRole = await GetByIdAsync(sourceRoleId);
                if (sourceRole == null)
                    return ServiceResult.Failure("來源角色不存在");

                var targetRole = await GetByIdAsync(targetRoleId);
                if (targetRole == null)
                    return ServiceResult.Failure("目標角色不存在");

                if (targetRole.IsSystemRole)
                    return ServiceResult.Failure("無法修改系統角色的權限");

                // 取得來源角色的權限
                var sourcePermissionIds = sourceRole.RolePermissions
                    .Where(rp => !rp.IsDeleted)
                    .Select(rp => rp.PermissionId)
                    .ToList();

                if (!sourcePermissionIds.Any())
                    return ServiceResult.Success(); // 來源角色沒有權限，直接成功

                // 複製權限到目標角色
                return await AssignPermissionsToRoleAsync(targetRoleId, sourcePermissionIds);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CopyRolePermissionsAsync),
                    ServiceType = GetType().Name,
                    SourceRoleId = sourceRoleId,
                    TargetRoleId = targetRoleId 
                });
                _logger.LogError(ex, "Error copying role permissions from {SourceRoleId} to {TargetRoleId}", sourceRoleId, targetRoleId);
                return ServiceResult.Failure($"複製角色權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得角色的員工數量
        /// </summary>
        public async Task<ServiceResult<int>> GetEmployeeCountByRoleAsync(int roleId)
        {
            try
            {
                var count = await _context.Employees
                    .Where(e => e.RoleId == roleId && !e.IsDeleted)
                    .CountAsync();

                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetEmployeeCountByRoleAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId 
                });
                _logger.LogError(ex, "Error getting employee count for role {RoleId}", roleId);
                return ServiceResult<int>.Failure($"取得角色員工數量時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查角色是否可以刪除（沒有員工使用且非系統角色）
        /// </summary>
        public async Task<ServiceResult<bool>> CanDeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await GetByIdAsync(roleId);
                if (role == null)
                    return ServiceResult<bool>.Failure("角色不存在");

                if (role.IsSystemRole)
                    return ServiceResult<bool>.Success(false); // 系統角色不能刪除

                var employeeCount = await GetEmployeeCountByRoleAsync(roleId);
                if (!employeeCount.IsSuccess)
                    return ServiceResult<bool>.Failure(employeeCount.ErrorMessage);

                var canDelete = employeeCount.Data == 0;
                return ServiceResult<bool>.Success(canDelete);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteRoleAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId 
                });
                _logger.LogError(ex, "Error checking if role can be deleted {RoleId}", roleId);
                return ServiceResult<bool>.Failure($"檢查角色刪除權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 搜尋角色
        /// </summary>
        public async Task<ServiceResult<List<Role>>> SearchRolesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return ServiceResult<List<Role>>.Success(await GetAllAsync());

                var roles = await _dbSet
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(r => !r.IsDeleted && 
                               (r.RoleName.Contains(searchTerm) ||
                                (r.Description != null && r.Description.Contains(searchTerm))))
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchRolesAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                _logger.LogError(ex, "Error searching roles with term {SearchTerm}", searchTerm);
                return ServiceResult<List<Role>>.Failure($"搜尋角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得可指派的角色清單（排除系統管理員等特殊角色）
        /// </summary>
        public async Task<ServiceResult<List<Role>>> GetAssignableRolesAsync()
        {
            try
            {
                var roles = await _dbSet
                    .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAssignableRolesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting assignable roles");
                return ServiceResult<List<Role>>.Failure($"取得可指派角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證角色資料
        /// </summary>
        public ServiceResult<bool> ValidateRoleData(Role role)
        {
            try
            {
                if (role == null)
                    return ServiceResult<bool>.Failure("角色資料不能為空");

                if (string.IsNullOrWhiteSpace(role.RoleName))
                    return ServiceResult<bool>.Failure("角色名稱不能為空");

                if (role.RoleName.Length > 100)
                    return ServiceResult<bool>.Failure("角色名稱長度不能超過100個字元");

                if (!string.IsNullOrEmpty(role.Description) && role.Description.Length > 500)
                    return ServiceResult<bool>.Failure("角色描述長度不能超過500個字元");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating role data");
                return ServiceResult<bool>.Failure($"驗證角色資料時發生錯誤：{ex.Message}");
            }
        }        // 覆寫 ValidateAsync 以加入業務驗證
        public override async Task<ServiceResult> ValidateAsync(Role entity)
        {
            try
            {
                // 執行業務特定驗證
                var businessValidation = ValidateRoleData(entity);
                if (!businessValidation.IsSuccess)
                    return ServiceResult.Failure(businessValidation.ErrorMessage);

                // 檢查角色名稱是否已存在
                var nameCheck = await IsRoleNameExistsAsync(entity.RoleName, entity.Id);
                if (!nameCheck.IsSuccess)
                    return ServiceResult.Failure(nameCheck.ErrorMessage);

                if (nameCheck.Data)
                    return ServiceResult.Failure("角色名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity?.Id,
                    EntityName = entity?.RoleName 
                });
                _logger.LogError(ex, "Error validating role {RoleName}", entity?.RoleName);
                return ServiceResult.Failure($"驗證角色時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 SearchAsync 實作搜尋邏輯
        public override async Task<List<Role>> SearchAsync(string searchTerm)
        {
            try
            {
                var result = await SearchRolesAsync(searchTerm);
                return result.IsSuccess ? result.Data ?? new List<Role>() : new List<Role>();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                _logger.LogError(ex, "Error in SearchAsync with term {SearchTerm}", searchTerm);
                return new List<Role>();
            }
        }

        // 覆寫 DeleteAsync 以加入額外檢查
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var canDeleteResult = await CanDeleteRoleAsync(id);
                if (!canDeleteResult.IsSuccess)
                    return ServiceResult.Failure(canDeleteResult.ErrorMessage);

                if (!canDeleteResult.Data)
                    return ServiceResult.Failure("此角色無法刪除，因為仍有員工使用此角色或為系統角色");

                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    RoleId = id 
                });
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return ServiceResult.Failure($"刪除角色時發生錯誤：{ex.Message}");
            }
        }
    }
}
