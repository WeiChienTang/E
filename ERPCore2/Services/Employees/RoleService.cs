using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 角色服務實作
    /// </summary>
    public class RoleService : GenericManagementService<Role>, IRoleService
    {
        private readonly IPermissionService _permissionService;

        public RoleService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Role>> logger,
            IPermissionService permissionService) : base(contextFactory, logger)
        {
            _permissionService = permissionService;
        }

        public RoleService(IDbContextFactory<AppDbContext> contextFactory, IPermissionService permissionService) : base(contextFactory)
        {
            _permissionService = permissionService;
        }

        // 覆寫 GetAllAsync 以載入相關資料
        public override async Task<List<Role>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .AsQueryable()
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<Role>();
            }
        }

        // 覆寫 GetByIdAsync 以載入相關資料
        public override async Task<Role?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { RoleId = id });
                return null;
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

                using var context = await _contextFactory.CreateDbContextAsync();
                var role = await context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name == roleName);

                if (role == null)
                    return ServiceResult<Role>.Failure("找不到指定的角色");

                return ServiceResult<Role>.Success(role);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByNameAsync), GetType(), _logger, new { RoleName = roleName });
                return ServiceResult<Role>.Failure($"取得角色資料時發生錯誤：{ex.Message}");
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

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Roles.Where(r => r.Name == roleName);

                if (excludeRoleId.HasValue)
                    query = query.Where(r => r.Id != excludeRoleId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsRoleNameExistsAsync), GetType(), _logger, new { RoleName = roleName, ExcludeRoleId = excludeRoleId });
                return ServiceResult<bool>.Failure($"檢查角色名稱時發生錯誤：{ex.Message}");
            }
        }

        public async Task<bool> IsRoleCodeExistsAsync(string roleCode, int? excludeRoleId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Roles.Where(r => r.Code == roleCode);

                if (excludeRoleId.HasValue)
                    query = query.Where(r => r.Id != excludeRoleId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsRoleCodeExistsAsync), GetType(), _logger, new { 
                    RoleCode = roleCode, 
                    ExcludeRoleId = excludeRoleId,
                    Method = nameof(IsRoleCodeExistsAsync)
                });
                return false;
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

                // 只有管理員或admin角色不能修改權限
                var roleName = role.Name?.Trim().ToLower();
                if (roleName == "管理員" || roleName == "admin")
                    return ServiceResult.Failure("無法修改管理員角色的權限");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得所有權限（包括要新增的權限）
                var allPermissions = await context.Permissions
                    .AsQueryable()
                    .Select(p => p.Id)
                    .ToListAsync();

                // 檢查選擇的權限是否都存在
                var invalidPermissions = permissionIds.Except(allPermissions).ToList();
                if (invalidPermissions.Any())
                    return ServiceResult.Failure("部分權限不存在或已被刪除");

                // 取得現有的角色權限關聯
                var existingRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                var now = DateTime.UtcNow;

                // 更新現有的角色權限狀態
                foreach (var rolePermission in existingRolePermissions)
                {
                    if (permissionIds.Contains(rolePermission.PermissionId))
                    {
                        // 應該啟用的權限
                        rolePermission.Status = EntityStatus.Active;
                        rolePermission.UpdatedAt = now;
                    }
                    else
                    {
                        // 應該停用的權限
                        rolePermission.Status = EntityStatus.Inactive;
                        rolePermission.UpdatedAt = now;
                    }
                }

                // 新增不存在的權限關聯
                var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToList();
                var newPermissionIds = permissionIds.Except(existingPermissionIds).ToList();
                
                var newRolePermissions = newPermissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Status = EntityStatus.Active,
                    
                }).ToList();

                await context.RolePermissions.AddRangeAsync(newRolePermissions);
                await context.SaveChangesAsync();

                // 清除該角色所有員工的權限快取
                await ClearRoleEmployeePermissionCacheAsync(roleId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AssignPermissionsToRoleAsync), GetType(), _logger, new { RoleId = roleId, PermissionIds = permissionIds });
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

                // 只有管理員或admin角色不能修改權限
                var roleName = role.Name?.Trim().ToLower();
                if (roleName == "管理員" || roleName == "admin")
                    return ServiceResult.Failure("無法修改管理員角色的權限");

                using var context = await _contextFactory.CreateDbContextAsync();
                var rolePermissionsToRemove = await context.RolePermissions
                    .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                    .ToListAsync();

                context.RolePermissions.RemoveRange(rolePermissionsToRemove);
                await context.SaveChangesAsync();

                // 清除該角色所有員工的權限快取
                await ClearRoleEmployeePermissionCacheAsync(roleId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RemovePermissionsFromRoleAsync), GetType(), _logger, new { RoleId = roleId, PermissionIds = permissionIds });
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

                // 只有管理員或admin角色不能修改權限
                var roleName = role.Name?.Trim().ToLower();
                if (roleName == "管理員" || roleName == "admin")
                    return ServiceResult.Failure("無法修改管理員角色的權限");

                using var context = await _contextFactory.CreateDbContextAsync();
                var rolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                context.RolePermissions.RemoveRange(rolePermissions);
                await context.SaveChangesAsync();

                // 清除該角色所有員工的權限快取
                await ClearRoleEmployeePermissionCacheAsync(roleId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ClearRolePermissionsAsync), GetType(), _logger, new { RoleId = roleId });
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

                // 只有管理員或admin角色不能修改權限
                var targetRoleName = targetRole.Name?.Trim().ToLower();
                if (targetRoleName == "管理員" || targetRoleName == "admin")
                    return ServiceResult.Failure("無法修改管理員角色的權限");

                // 取得來源角色的權限
                var sourcePermissionIds = sourceRole.RolePermissions
                    .AsQueryable()
                    .Select(rp => rp.PermissionId)
                    .ToList();

                if (!sourcePermissionIds.Any())
                    return ServiceResult.Success(); // 來源角色沒有權限，直接成功

                // 複製權限到目標角色
                return await AssignPermissionsToRoleAsync(targetRoleId, sourcePermissionIds);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyRolePermissionsAsync), GetType(), _logger, new { SourceRoleId = sourceRoleId, TargetRoleId = targetRoleId });
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var count = await context.Employees
                    .Where(e => e.RoleId.HasValue && e.RoleId.Value == roleId)
                    .CountAsync();

                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEmployeeCountByRoleAsync), GetType(), _logger, new { RoleId = roleId });
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

                // 只有管理員或admin角色不能刪除
                var roleName = role.Name?.Trim().ToLower();
                if (roleName == "管理員" || roleName == "admin")
                    return ServiceResult<bool>.Success(false); // 管理員角色不能刪除

                var employeeCount = await GetEmployeeCountByRoleAsync(roleId);
                if (!employeeCount.IsSuccess)
                    return ServiceResult<bool>.Failure(employeeCount.ErrorMessage);

                var canDelete = employeeCount.Data == 0;
                return ServiceResult<bool>.Success(canDelete);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteRoleAsync), GetType(), _logger, new { RoleId = roleId });
                return ServiceResult<bool>.Failure($"檢查角色刪除權限時發生錯誤：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作角色特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Role entity)
        {
            try
            {
                // 先檢查是否為系統角色
                var roleName = entity.Name?.Trim().ToLower();
                if (roleName == "管理員" || roleName == "admin")
                {
                    return ServiceResult.Failure("系統管理員角色無法刪除");
                }
                
                // 檢查依賴關係
                var dependencyCheck = await DependencyCheckHelper.CheckRoleDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("角色"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    RoleId = entity.Id 
                });
                return ServiceResult.Failure("檢查角色刪除條件時發生錯誤");
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

                using var context = await _contextFactory.CreateDbContextAsync();
                var roles = await context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(r => r.Name.Contains(searchTerm))
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchRolesAsync), GetType(), _logger, new { SearchTerm = searchTerm });
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var roles = await context.Roles
                    .Where(r => r.Status == EntityStatus.Active)
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAssignableRolesAsync), GetType(), _logger);
                return ServiceResult<List<Role>>.Failure($"取得可指派角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 根據當前使用者角色取得可指派的角色清單
        /// </summary>
        public async Task<ServiceResult<List<Role>>> GetAssignableRolesForCurrentUserAsync(string currentUserRole)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Roles
                    .Where(r => r.Status == EntityStatus.Active);

                // 檢查當前使用者是否為管理員
                var currentRoleLower = currentUserRole?.Trim().ToLower() ?? "";
                bool isAdmin = currentRoleLower == "管理員" || currentRoleLower == "admin" || 
                               currentRoleLower.Contains("管理員") || currentRoleLower.Contains("admin");

                // 如果當前使用者不是管理員，則排除管理員角色
                if (!isAdmin)
                {
                    query = query.Where(r => 
                        r.Name.ToLower() != "管理員" && 
                        r.Name.ToLower() != "admin" && 
                        !r.Name.ToLower().Contains("管理員") && 
                        !r.Name.ToLower().Contains("admin"));
                }

                var roles = await query
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return ServiceResult<List<Role>>.Success(roles);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAssignableRolesForCurrentUserAsync), GetType(), _logger);
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

                if (string.IsNullOrWhiteSpace(role.Name))
                    return ServiceResult<bool>.Failure("角色名稱不能為空");

                if (role.Name.Length > 100)
                    return ServiceResult<bool>.Failure("角色名稱長度不能超過100個字元");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateRoleData), GetType(), _logger);
                return ServiceResult<bool>.Failure($"驗證角色資料時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 ValidateAsync 以加入業務驗證
        public override async Task<ServiceResult> ValidateAsync(Role entity)
        {
            try
            {
                // 執行業務特定驗證
                var businessValidation = ValidateRoleData(entity);
                if (!businessValidation.IsSuccess)
                    return ServiceResult.Failure(businessValidation.ErrorMessage);

                // 檢查角色名稱是否已存在
                var nameCheck = await IsRoleNameExistsAsync(entity.Name, entity.Id);
                if (!nameCheck.IsSuccess)
                    return ServiceResult.Failure(nameCheck.ErrorMessage);

                if (nameCheck.Data)
                    return ServiceResult.Failure("角色名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity?.Id, EntityName = entity?.Name });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
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

                return await base.PermanentDeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { RoleId = id });
                return ServiceResult.Failure($"刪除角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 清除指定角色所有員工的權限快取
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>操作結果</returns>
        private async Task<ServiceResult> ClearRoleEmployeePermissionCacheAsync(int roleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employeeIds = await context.Employees
                    .Where(e => e.RoleId.HasValue && e.RoleId.Value == roleId)
                    .Select(e => e.Id)
                    .ToListAsync();

                // 清除每個員工的權限快取
                foreach (var employeeId in employeeIds)
                {
                    await _permissionService.RefreshEmployeePermissionCacheAsync(employeeId);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ClearRoleEmployeePermissionCacheAsync), GetType(), _logger, new { RoleId = roleId });
                return ServiceResult.Failure($"清除角色員工權限快取時發生錯誤：{ex.Message}");
            }
        }
    }
}


