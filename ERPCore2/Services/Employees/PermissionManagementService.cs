using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 權限管理服務實作
    /// </summary>
    public class PermissionManagementService : GenericManagementService<Permission>, IPermissionManagementService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public PermissionManagementService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Permission>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public PermissionManagementService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以提供排序
        public override async Task<List<Permission>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Permissions
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        /// <summary>
        /// 根據權限代碼取得權限
        /// </summary>
        public async Task<ServiceResult<Permission>> GetByCodeAsync(string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<Permission>.Failure("權限代碼不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionCode == permissionCode && !p.IsDeleted);

                if (permission == null)
                    return ServiceResult<Permission>.Failure("找不到指定的權限");

                return ServiceResult<Permission>.Success(permission);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, 
                    new { PermissionCode = permissionCode });
                return ServiceResult<Permission>.Failure("取得權限資料時發生錯誤");
            }
        }

        /// <summary>
        /// 取得模組的所有權限
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> GetPermissionsByModuleAsync(string modulePrefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modulePrefix))
                    return ServiceResult<List<Permission>>.Failure("模組前綴不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var permissions = await context.Permissions
                    .Where(p => p.PermissionCode.StartsWith(modulePrefix + ".") && !p.IsDeleted)
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPermissionsByModuleAsync), GetType(), _logger, 
                    new { ModulePrefix = modulePrefix });
                return ServiceResult<List<Permission>>.Failure("取得模組權限時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查權限代碼是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsPermissionCodeExistsAsync(string permissionCode, int? excludePermissionId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<bool>.Failure("權限代碼不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Permissions.Where(p => p.PermissionCode == permissionCode && !p.IsDeleted);

                if (excludePermissionId.HasValue)
                    query = query.Where(p => p.Id != excludePermissionId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPermissionCodeExistsAsync), GetType(), _logger, 
                    new { PermissionCode = permissionCode, ExcludePermissionId = excludePermissionId });
                return ServiceResult<bool>.Failure($"檢查權限代碼時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得所有模組清單
        /// </summary>
        public async Task<ServiceResult<List<string>>> GetAllModulesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 先取得所有權限代碼，然後在客戶端處理
                var permissionCodes = await context.Permissions
                    .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.PermissionCode))
                    .Select(p => p.PermissionCode)
                    .ToListAsync();

                // 在客戶端提取模組名稱（權限代碼中第一個點之前的部分）
                var modules = permissionCodes
                    .Where(code => code.Contains('.'))
                    .Select(code => code.Substring(0, code.IndexOf('.')))
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                return ServiceResult<List<string>>.Success(modules);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllModulesAsync), GetType(), _logger);
                return ServiceResult<List<string>>.Failure($"取得模組清單時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 批次建立權限
        /// </summary>
        public async Task<ServiceResult> CreatePermissionsBatchAsync(List<Permission> permissions)
        {
            try
            {
                if (permissions == null || !permissions.Any())
                    return ServiceResult.Failure("權限清單不能為空");

                // 驗證每個權限
                foreach (var permission in permissions)
                {
                    var validation = ValidatePermissionCode(permission.PermissionCode);
                    if (!validation.IsSuccess)
                        return ServiceResult.Failure($"權限代碼 '{permission.PermissionCode}' 格式錯誤：{validation.ErrorMessage}");

                    var existsResult = await IsPermissionCodeExistsAsync(permission.PermissionCode);
                    if (!existsResult.IsSuccess)
                        return ServiceResult.Failure(existsResult.ErrorMessage);

                    if (existsResult.Data)
                        return ServiceResult.Failure($"權限代碼 '{permission.PermissionCode}' 已存在");
                }

                // 設定建立時間和狀態，並自動解析 Module 和 Action
                foreach (var permission in permissions)
                {
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.UpdatedAt = DateTime.UtcNow;
                    permission.Status = EntityStatus.Active;

                    // 自動解析 Module 和 Action 從 PermissionCode
                    var parts = permission.PermissionCode.Split('.');
                    if (parts.Length == 2)
                    {
                        permission.Module = parts[0];
                        permission.Action = parts[1];
                    }
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreatePermissionsBatchAsync), GetType(), _logger, 
                    new { PermissionCount = permissions?.Count });
                return ServiceResult.Failure($"批次建立權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 搜尋權限
        /// </summary>
        public async Task<ServiceResult<List<Permission>>> SearchPermissionsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return ServiceResult<List<Permission>>.Success(await GetAllAsync());

                using var context = await _contextFactory.CreateDbContextAsync();
                var permissions = await context.Permissions
                    .Where(p => !p.IsDeleted && 
                               (p.PermissionCode.Contains(searchTerm) ||
                                p.PermissionName.Contains(searchTerm) ||
                                (p.Remarks != null && p.Remarks.Contains(searchTerm))))
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchPermissionsAsync), GetType(), _logger, 
                    new { SearchTerm = searchTerm });
                return ServiceResult<List<Permission>>.Failure($"搜尋權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證權限代碼格式
        /// </summary>
        public ServiceResult<bool> ValidatePermissionCode(string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<bool>.Failure("權限代碼不能為空");

                if (permissionCode.Length > 100)
                    return ServiceResult<bool>.Failure("權限代碼長度不能超過100個字元");

                // 權限代碼格式：Module.Action (例如：Customer.View, Role.Create)
                var regex = new Regex(@"^[A-Za-z][A-Za-z0-9]*\.[A-Za-z][A-Za-z0-9]*$");
                if (!regex.IsMatch(permissionCode))
                    return ServiceResult<bool>.Failure("權限代碼格式錯誤，應為 'Module.Action' 格式，例如：Customer.View");

                var parts = permissionCode.Split('.');
                if (parts.Length != 2)
                    return ServiceResult<bool>.Failure("權限代碼必須包含一個點號分隔模組和動作");

                var module = parts[0];
                var action = parts[1];

                if (module.Length < 2 || module.Length > 50)
                    return ServiceResult<bool>.Failure("模組名稱長度必須在2-50個字元之間");

                if (action.Length < 2 || action.Length > 50)
                    return ServiceResult<bool>.Failure("動作名稱長度必須在2-50個字元之間");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidatePermissionCode), GetType(), _logger, 
                    new { PermissionCode = permissionCode });
                return ServiceResult<bool>.Failure($"驗證權限代碼時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 ValidateAsync 以加入業務驗證
        public override async Task<ServiceResult> ValidateAsync(Permission entity)
        {
            try
            {
                // 驗證權限代碼格式
                var codeValidation = ValidatePermissionCode(entity.PermissionCode);
                if (!codeValidation.IsSuccess)
                    return ServiceResult.Failure(codeValidation.ErrorMessage);

                // 檢查權限代碼是否已存在
                var existsResult = await IsPermissionCodeExistsAsync(entity.PermissionCode, entity.Id);
                if (!existsResult.IsSuccess)
                    return ServiceResult.Failure(existsResult.ErrorMessage);

                if (existsResult.Data)
                    return ServiceResult.Failure("權限代碼已存在");

                // 檢查權限名稱
                if (string.IsNullOrWhiteSpace(entity.PermissionName))
                    return ServiceResult.Failure("權限名稱不能為空");

                if (entity.PermissionName.Length > 100)
                    return ServiceResult.Failure("權限名稱長度不能超過100個字元");

                // 檢查備註長度
                if (!string.IsNullOrEmpty(entity.Remarks) && entity.Remarks.Length > 500)
                    return ServiceResult.Failure("權限備註長度不能超過500個字元");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { EntityId = entity?.Id, PermissionCode = entity?.PermissionCode });
                return ServiceResult.Failure($"驗證權限時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 SearchAsync 實作搜尋邏輯
        public override async Task<List<Permission>> SearchAsync(string searchTerm)
        {
            try
            {
                var result = await SearchPermissionsAsync(searchTerm);
                return result.IsSuccess ? result.Data ?? new List<Permission>() : new List<Permission>();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, 
                    new { SearchTerm = searchTerm });
                return new List<Permission>();
            }
        }

        // 覆寫 DeleteAsync 以檢查是否有角色使用該權限
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                // 檢查是否有角色使用該權限
                using var context = await _contextFactory.CreateDbContextAsync();
                var isUsed = await context.RolePermissions
                    .AnyAsync(rp => rp.PermissionId == id && !rp.IsDeleted);

                if (isUsed)
                    return ServiceResult.Failure("無法刪除此權限，因為仍有角色使用此權限");

                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, 
                    new { PermissionId = id });
                return ServiceResult.Failure($"刪除權限時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 CreateAsync 以自動解析 Module 和 Action
        public override async Task<ServiceResult<Permission>> CreateAsync(Permission entity)
        {
            try
            {
                // 自動解析 Module 和 Action 從 PermissionCode
                var parts = entity.PermissionCode.Split('.');
                if (parts.Length == 2)
                {
                    entity.Module = parts[0];
                    entity.Action = parts[1];
                }

                return await base.CreateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, 
                    new { PermissionCode = entity?.PermissionCode });
                return ServiceResult<Permission>.Failure($"建立權限時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 UpdateAsync 以自動解析 Module 和 Action
        public override async Task<ServiceResult<Permission>> UpdateAsync(Permission entity)
        {
            try
            {
                // 自動解析 Module 和 Action 從 PermissionCode
                var parts = entity.PermissionCode.Split('.');
                if (parts.Length == 2)
                {
                    entity.Module = parts[0];
                    entity.Action = parts[1];
                }

                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, 
                    new { PermissionCode = entity?.PermissionCode });
                return ServiceResult<Permission>.Failure($"更新權限時發生錯誤：{ex.Message}");
            }
        }
    }
}

