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
                    .AsQueryable()
                    .OrderBy(p => p.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        /// <summary>
        /// 根據權限編號取得權限
        /// </summary>
        public async Task<ServiceResult<Permission>> GetByCodeAsync(string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<Permission>.Failure("權限編號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode);

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
                    .Where(p => p.Code != null && p.Code.StartsWith(modulePrefix + "."))
                    .OrderBy(p => p.Code)
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
        /// 檢查權限編號是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsPermissionCodeExistsAsync(string permissionCode, int? excludePermissionId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<bool>.Failure("權限編號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Permissions.Where(p => p.Code == permissionCode);

                if (excludePermissionId.HasValue)
                    query = query.Where(p => p.Id != excludePermissionId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPermissionCodeExistsAsync), GetType(), _logger, 
                    new { PermissionCode = permissionCode, ExcludePermissionId = excludePermissionId });
                return ServiceResult<bool>.Failure($"檢查權限編號時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查權限名稱是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsPermissionNameExistsAsync(string permissionName, int? excludePermissionId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionName))
                    return ServiceResult<bool>.Failure("權限名稱不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Permissions.Where(p => p.Name == permissionName);

                if (excludePermissionId.HasValue)
                    query = query.Where(p => p.Id != excludePermissionId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPermissionNameExistsAsync), GetType(), _logger, 
                    new { PermissionName = permissionName, ExcludePermissionId = excludePermissionId });
                return ServiceResult<bool>.Failure($"檢查權限名稱時發生錯誤：{ex.Message}");
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
                
                // 先取得所有權限編號，然後在客戶端處理
                var permissionCodes = await context.Permissions
                    .Where(p => !string.IsNullOrEmpty(p.Code))
                    .Select(p => p.Code)
                    .ToListAsync();

                // 在客戶端提取模組名稱（權限編號中第一個點之前的部分）
                var modules = permissionCodes
                    .Where(code => code != null && code.Contains('.'))
                    .Select(code => code!.Substring(0, code.IndexOf('.')))
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
                    if (permission.Code == null)
                        return ServiceResult.Failure("權限編號不能為空");
                        
                    var validation = ValidatePermissionCode(permission.Code);
                    if (!validation.IsSuccess)
                        return ServiceResult.Failure($"權限編號 '{permission.Code}' 格式錯誤：{validation.ErrorMessage}");

                    var existsResult = await IsPermissionCodeExistsAsync(permission.Code);
                    if (!existsResult.IsSuccess)
                        return ServiceResult.Failure(existsResult.ErrorMessage);

                    if (existsResult.Data)
                        return ServiceResult.Failure($"權限編號 '{permission.Code}' 已存在");
                }

                // 設定建立時間和狀態，並自動解析 Module 和 Action
                foreach (var permission in permissions)
                {
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.UpdatedAt = DateTime.UtcNow;
                    permission.Status = EntityStatus.Active;
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
                    .Where(p => ((p.Code != null && p.Code.Contains(searchTerm)) ||
                                (p.Name != null && p.Name.Contains(searchTerm)) ||
                                (p.Remarks != null && p.Remarks.Contains(searchTerm))))
                    .OrderBy(p => p.Code)
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
        /// 驗證權限編號格式
        /// </summary>
        public ServiceResult<bool> ValidatePermissionCode(string permissionCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionCode))
                    return ServiceResult<bool>.Failure("權限編號不能為空");

                if (permissionCode.Length > 100)
                    return ServiceResult<bool>.Failure("權限編號長度不能超過100個字元");

                // 權限編號格式：Module.Action (例如：Customer.View, Role.Create)
                var regex = new Regex(@"^[A-Za-z][A-Za-z0-9]*\.[A-Za-z][A-Za-z0-9]*$");
                if (!regex.IsMatch(permissionCode))
                    return ServiceResult<bool>.Failure("權限編號格式錯誤，應為 'Module.Action' 格式，例如：Customer.View");

                var parts = permissionCode.Split('.');
                if (parts.Length != 2)
                    return ServiceResult<bool>.Failure("權限編號必須包含一個點號分隔模組和動作");

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
                return ServiceResult<bool>.Failure($"驗證權限編號時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 ValidateAsync 以加入業務驗證
        public override async Task<ServiceResult> ValidateAsync(Permission entity)
        {
            try
            {
                // 驗證權限編號
                if (entity.Code == null)
                    return ServiceResult.Failure("權限編號不能為空");
                    
                // 驗證權限編號格式
                var codeValidation = ValidatePermissionCode(entity.Code);
                if (!codeValidation.IsSuccess)
                    return ServiceResult.Failure(codeValidation.ErrorMessage);

                // 檢查權限編號是否已存在
                var existsResult = await IsPermissionCodeExistsAsync(entity.Code, entity.Id);
                if (!existsResult.IsSuccess)
                    return ServiceResult.Failure(existsResult.ErrorMessage);

                if (existsResult.Data)
                    return ServiceResult.Failure("權限編號已存在");

                // 檢查權限名稱
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("權限名稱不能為空");

                if (entity.Name.Length > 100)
                    return ServiceResult.Failure("權限名稱長度不能超過100個字元");

                // 檢查備註長度
                if (!string.IsNullOrEmpty(entity.Remarks) && entity.Remarks.Length > 500)
                    return ServiceResult.Failure("權限備註長度不能超過500個字元");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { EntityId = entity?.Id, PermissionCode = entity?.Code });
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
                    .AnyAsync(rp => rp.PermissionId == id);

                if (isUsed)
                    return ServiceResult.Failure("無法刪除此權限，因為仍有角色使用此權限");

                return await base.PermanentDeleteAsync(id);
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
                return await base.CreateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, 
                    new { PermissionCode = entity?.Code });
                return ServiceResult<Permission>.Failure($"建立權限時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 UpdateAsync 以自動解析 Module 和 Action
        public override async Task<ServiceResult<Permission>> UpdateAsync(Permission entity)
        {
            try
            {
                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, 
                    new { PermissionCode = entity?.Code });
                return ServiceResult<Permission>.Failure($"更新權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作權限特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Permission entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckPermissionDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("權限"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    PermissionId = entity.Id 
                });
                return ServiceResult.Failure("檢查權限刪除條件時發生錯誤");
            }
        }
    }
}


