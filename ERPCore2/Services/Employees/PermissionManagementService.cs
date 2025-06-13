using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 權限管理服務實作
    /// </summary>
    public class PermissionManagementService : GenericManagementService<Permission>, IPermissionManagementService
    {
        public PermissionManagementService(AppDbContext context) : base(context)
        {
        }

        // 覆寫 GetAllAsync 以提供排序
        public override async Task<List<Permission>> GetAllAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.PermissionCode)
                .ToListAsync();
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

                var permission = await _dbSet
                    .FirstOrDefaultAsync(p => p.PermissionCode == permissionCode && !p.IsDeleted);

                if (permission == null)
                    return ServiceResult<Permission>.Failure("找不到指定的權限");

                return ServiceResult<Permission>.Success(permission);
            }
            catch (Exception ex)
            {
                return ServiceResult<Permission>.Failure($"取得權限資料時發生錯誤：{ex.Message}");
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

                var permissions = await _dbSet
                    .Where(p => p.PermissionCode.StartsWith(modulePrefix + ".") && !p.IsDeleted)
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Permission>>.Failure($"取得模組權限時發生錯誤：{ex.Message}");
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

                var query = _dbSet.Where(p => p.PermissionCode == permissionCode && !p.IsDeleted);

                if (excludePermissionId.HasValue)
                    query = query.Where(p => p.Id != excludePermissionId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
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
                var modules = await _dbSet
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.PermissionCode.Substring(0, p.PermissionCode.IndexOf('.')))
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                return ServiceResult<List<string>>.Success(modules);
            }
            catch (Exception ex)
            {
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

                // 設定建立時間和狀態
                foreach (var permission in permissions)
                {
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.UpdatedAt = DateTime.UtcNow;
                    permission.Status = EntityStatus.Active;
                }

                await _dbSet.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"批次建立權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 初始化系統預設權限
        /// </summary>
        public async Task<ServiceResult> InitializeDefaultPermissionsAsync()
        {
            try
            {
                var defaultPermissions = new List<Permission>
                {
                    // 客戶管理權限                    
                    new Permission { PermissionCode = "Customer.View", PermissionName = "檢視客戶", Remarks = "可以檢視客戶資料" },
                    new Permission { PermissionCode = "Customer.Create", PermissionName = "建立客戶", Remarks = "可以建立新客戶" },
                    new Permission { PermissionCode = "Customer.Edit", PermissionName = "編輯客戶", Remarks = "可以編輯客戶資料" },
                    new Permission { PermissionCode = "Customer.Delete", PermissionName = "刪除客戶", Remarks = "可以刪除客戶" },
                    new Permission { PermissionCode = "Customer.Export", PermissionName = "匯出客戶", Remarks = "可以匯出客戶資料" },

                    // 廠商管理權限
                    new Permission { PermissionCode = "Supplier.View", PermissionName = "檢視廠商", Remarks = "可以檢視廠商資料" },
                    new Permission { PermissionCode = "Supplier.Create", PermissionName = "建立廠商", Remarks = "可以建立新廠商" },
                    new Permission { PermissionCode = "Supplier.Edit", PermissionName = "編輯廠商", Remarks = "可以編輯廠商資料" },
                    new Permission { PermissionCode = "Supplier.Delete", PermissionName = "刪除廠商", Remarks = "可以刪除廠商" },
                    new Permission { PermissionCode = "Supplier.Export", PermissionName = "匯出廠商", Remarks = "可以匯出廠商資料" },

                    // 員工管理權限
                    new Permission { PermissionCode = "Employee.View", PermissionName = "檢視員工", Remarks = "可以檢視員工資料" },
                    new Permission { PermissionCode = "Employee.Create", PermissionName = "建立員工", Remarks = "可以建立新員工" },
                    new Permission { PermissionCode = "Employee.Edit", PermissionName = "編輯員工", Remarks = "可以編輯員工資料" },
                    new Permission { PermissionCode = "Employee.Delete", PermissionName = "刪除員工", Remarks = "可以刪除員工" },

                    // 角色管理權限
                    new Permission { PermissionCode = "Role.View", PermissionName = "檢視角色", Remarks = "可以檢視角色資料" },
                    new Permission { PermissionCode = "Role.Create", PermissionName = "建立角色", Remarks = "可以建立新角色" },
                    new Permission { PermissionCode = "Role.Edit", PermissionName = "編輯角色", Remarks = "可以編輯角色資料" },
                    new Permission { PermissionCode = "Role.Delete", PermissionName = "刪除角色", Remarks = "可以刪除角色" },
                    new Permission { PermissionCode = "Role.ManagePermissions", PermissionName = "管理權限", Remarks = "可以管理角色權限" },

                    // 權限管理權限
                    new Permission { PermissionCode = "Permission.View", PermissionName = "檢視權限", Remarks = "可以檢視權限資料" },
                    new Permission { PermissionCode = "Permission.Create", PermissionName = "建立權限", Remarks = "可以建立新權限" },
                    new Permission { PermissionCode = "Permission.Edit", PermissionName = "編輯權限", Remarks = "可以編輯權限資料" },
                    new Permission { PermissionCode = "Permission.Delete", PermissionName = "刪除權限", Remarks = "可以刪除權限" },

                    // 系統管理權限
                    new Permission { PermissionCode = "System.Admin", PermissionName = "系統管理", Remarks = "完整的系統管理權限" },
                    new Permission { PermissionCode = "System.ViewLogs", PermissionName = "檢視日誌", Remarks = "可以檢視系統日誌" },
                    new Permission { PermissionCode = "System.Backup", PermissionName = "系統備份", Remarks = "可以執行系統備份" },
                    new Permission { PermissionCode = "System.Configuration", PermissionName = "系統設定", Remarks = "可以修改系統設定" },

                    // 報表權限
                    new Permission { PermissionCode = "Report.View", PermissionName = "檢視報表", Remarks = "可以檢視報表" },
                    new Permission { PermissionCode = "Report.Export", PermissionName = "匯出報表", Remarks = "可以匯出報表" },
                    new Permission { PermissionCode = "Report.Create", PermissionName = "建立報表", Remarks = "可以建立自訂報表" }
                };

                // 過濾已存在的權限
                var existingCodes = await _dbSet
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.PermissionCode)
                    .ToListAsync();

                var newPermissions = defaultPermissions
                    .Where(p => !existingCodes.Contains(p.PermissionCode))
                    .ToList();

                if (newPermissions.Any())
                {
                    return await CreatePermissionsBatchAsync(newPermissions);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"初始化預設權限時發生錯誤：{ex.Message}");
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

                var permissions = await _dbSet                    .Where(p => !p.IsDeleted && 
                               (p.PermissionCode.Contains(searchTerm) ||
                                p.PermissionName.Contains(searchTerm) ||
                                (p.Remarks != null && p.Remarks.Contains(searchTerm))))
                    .OrderBy(p => p.PermissionCode)
                    .ToListAsync();

                return ServiceResult<List<Permission>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Permission>>.Failure($"搜尋權限時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證權限代碼格式
        /// </summary>
        public ServiceResult<bool> ValidatePermissionCode(string permissionCode)
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
        }        // 覆寫 ValidateAsync 以加入業務驗證
        public override async Task<ServiceResult> ValidateAsync(Permission entity)
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

        // 覆寫 SearchAsync 實作搜尋邏輯
        public override async Task<List<Permission>> SearchAsync(string searchTerm)
        {
            var result = await SearchPermissionsAsync(searchTerm);
            return result.IsSuccess ? result.Data ?? new List<Permission>() : new List<Permission>();
        }

        // 覆寫 DeleteAsync 以檢查是否有角色使用該權限
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                // 檢查是否有角色使用該權限
                var isUsed = await _context.RolePermissions
                    .AnyAsync(rp => rp.PermissionId == id && !rp.IsDeleted);

                if (isUsed)
                    return ServiceResult.Failure("無法刪除此權限，因為仍有角色使用此權限");

                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"刪除權限時發生錯誤：{ex.Message}");
            }
        }
    }
}
