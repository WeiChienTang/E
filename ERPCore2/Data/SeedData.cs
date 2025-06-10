using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ERPCore2.Data
{    public static class SeedData
    {        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 確保資料庫已建立
            await context.Database.EnsureCreatedAsync();

            // 初始化認證系統資料
            await SeedAuthDataAsync(context);

            // 檢查是否已有資料
            if (await context.ContactTypes.AnyAsync())
            {
                return; // 資料已存在
            }

            // 新增聯絡類型資料
            var contactTypes = new[]
            {
                new ContactType {
                    TypeName = "電話",
                    Description = "固定電話號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    TypeName = "手機", 
                    Description = "行動電話號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    TypeName = "Email", 
                    Description = "電子郵件地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    TypeName = "傳真", 
                    Description = "傳真號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                }
            };

            await context.ContactTypes.AddRangeAsync(contactTypes);

            // 新增地址類型資料
            var addressTypes = new[]
            {
                new AddressType { TypeName = "公司地址", Description = "公司營業地址" },
                new AddressType { TypeName = "通訊地址", Description = "通訊聯絡地址" },
                new AddressType { TypeName = "帳單地址", Description = "帳單寄送地址" },
                new AddressType { TypeName = "送貨地址", Description = "商品送貨地址" }
            };

            await context.AddressTypes.AddRangeAsync(addressTypes);

            // 新增客戶類型資料
            var customerTypes = new[]
            {
                new CustomerType { TypeName = "VIP客戶", Description = "重要客戶" },
                new CustomerType { TypeName = "一般客戶", Description = "一般合作客戶" },
                new CustomerType { TypeName = "潛在客戶", Description = "有合作意向的客戶" },
                new CustomerType { TypeName = "合作夥伴", Description = "策略合作夥伴" }
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            // 新增行業別資料
            var industryTypes = new[]
            {
                new IndustryType { IndustryTypeName = "製造業", IndustryTypeCode = "MFG" },
                new IndustryType { IndustryTypeName = "資訊科技業", IndustryTypeCode = "IT" },
                new IndustryType { IndustryTypeName = "服務業", IndustryTypeCode = "SVC" },
                new IndustryType { IndustryTypeName = "貿易業", IndustryTypeCode = "TRD" },
                new IndustryType { IndustryTypeName = "建築業", IndustryTypeCode = "CON" },
                new IndustryType { IndustryTypeName = "金融業", IndustryTypeCode = "FIN" },
                new IndustryType { IndustryTypeName = "零售業", IndustryTypeCode = "RTL" },
                new IndustryType { IndustryTypeName = "餐飲業", IndustryTypeCode = "F&B" }
            };

            await context.IndustryTypes.AddRangeAsync(industryTypes);

            // 儲存變更
            await context.SaveChangesAsync();
        }

        #region 認證系統種子資料

    /// <summary>
    /// 初始化認證系統的基本資料
    /// </summary>
    public static async Task SeedAuthDataAsync(AppDbContext context)
    {
        try
        {
            // 確保在交易中執行
            await using var transaction = await context.Database.BeginTransactionAsync();

            await SeedPermissionsAsync(context);
            await context.SaveChangesAsync(); // 先儲存權限

            await SeedRolesAsync(context);
            await context.SaveChangesAsync(); // 再儲存角色

            await SeedRolePermissionsAsync(context);
            await SeedDefaultAdminAsync(context);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine("認證系統種子資料初始化完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"認證系統種子資料初始化失敗: {ex.Message}");
            throw;
        }
    }

        /// <summary>
        /// 初始化權限資料
        /// </summary>
        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;            var permissions = new List<Permission>
            {
                // 系統管理權限
                new Permission
                {
                    PermissionCode = "System.Admin",
                    PermissionName = "系統管理",
                    Module = "System",
                    Action = "Admin",
                    PermissionGroup = "系統管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 使用者管理權限
                new Permission
                {
                    PermissionCode = "User.View",
                    PermissionName = "檢視使用者",
                    Module = "User",
                    Action = "View",
                    PermissionGroup = "使用者管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "User.Create",
                    PermissionName = "新增使用者",
                    Module = "User",
                    Action = "Create",
                    PermissionGroup = "使用者管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "User.Edit",
                    PermissionName = "編輯使用者",
                    Module = "User",
                    Action = "Edit",
                    PermissionGroup = "使用者管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "User.Delete",
                    PermissionName = "刪除使用者",
                    Module = "User",
                    Action = "Delete",
                    PermissionGroup = "使用者管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 角色管理權限
                new Permission
                {
                    PermissionCode = "Role.View",
                    PermissionName = "檢視角色",
                    Module = "Role",
                    Action = "View",
                    PermissionGroup = "角色管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Role.Create",
                    PermissionName = "新增角色",
                    Module = "Role",
                    Action = "Create",
                    PermissionGroup = "角色管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Role.Edit",
                    PermissionName = "編輯角色",
                    Module = "Role",
                    Action = "Edit",
                    PermissionGroup = "角色管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Role.Delete",
                    PermissionName = "刪除角色",
                    Module = "Role",
                    Action = "Delete",
                    PermissionGroup = "角色管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 客戶管理權限
                new Permission
                {
                    PermissionCode = "Customer.View",
                    PermissionName = "檢視客戶",
                    Module = "Customer",
                    Action = "View",
                    PermissionGroup = "客戶管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Customer.Create",
                    PermissionName = "新增客戶",
                    Module = "Customer",
                    Action = "Create",
                    PermissionGroup = "客戶管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Customer.Edit",
                    PermissionName = "編輯客戶",
                    Module = "Customer",
                    Action = "Edit",
                    PermissionGroup = "客戶管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "Customer.Delete",
                    PermissionName = "刪除客戶",
                    Module = "Customer",
                    Action = "Delete",
                    PermissionGroup = "客戶管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 報表權限
                new Permission
                {
                    PermissionCode = "Report.View",
                    PermissionName = "檢視報表",
                    Module = "Report",
                    Action = "View",
                    PermissionGroup = "報表管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Permission
                {
                    PermissionCode = "REPORT_EXPORT",
                    PermissionName = "匯出報表",
                    Module = "Report",
                    Action = "Export",
                    PermissionGroup = "報表管理",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Permissions.AddRange(permissions);
        }

        /// <summary>
        /// 初始化角色資料
        /// </summary>
        private static async Task SeedRolesAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new List<Role>
            {
                new Role
                {
                    RoleName = "系統管理員",
                    Description = "擁有系統所有權限的超級管理員",
                    IsSystemRole = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Role
                {
                    RoleName = "管理員",
                    Description = "擁有大部分管理權限的管理員",
                    IsSystemRole = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Role
                {
                    RoleName = "一般使用者",
                    Description = "擁有基本操作權限的一般使用者",
                    IsSystemRole = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Role
                {
                    RoleName = "唯讀使用者",
                    Description = "只能檢視資料的唯讀使用者",
                    IsSystemRole = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Roles.AddRange(roles);
        }

        /// <summary>
        /// 初始化角色權限關聯
        /// </summary>
        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;

            // 取得所有權限
            var permissions = await context.Permissions.ToListAsync();
            
            // 取得所有角色
            var systemAdminRole = await context.Roles.FirstAsync(r => r.RoleName == "系統管理員");
            var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "管理員");
            var userRole = await context.Roles.FirstAsync(r => r.RoleName == "一般使用者");
            var readOnlyRole = await context.Roles.FirstAsync(r => r.RoleName == "唯讀使用者");

            var rolePermissions = new List<RolePermission>();

            // 系統管理員 - 所有權限
            foreach (var permission in permissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = systemAdminRole.Id,
                    PermissionId = permission.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }            // 管理員 - 除了系統管理外的所有權限
            var adminPermissions = permissions.Where(p => p.PermissionCode != "System.Admin");
            foreach (var permission in adminPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }

            // 一般使用者 - 基本的檢視和新增編輯權限
            var userPermissionCodes = new[] { "Customer.View", "Customer.Create", "Customer.Edit", "Report.View" };
            var userPermissions = permissions.Where(p => userPermissionCodes.Contains(p.PermissionCode));
            foreach (var permission in userPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }            // 唯讀使用者 - 只有檢視權限
            var readOnlyPermissionCodes = new[] { "Customer.View", "Report.View" };
            var readOnlyPermissions = permissions.Where(p => readOnlyPermissionCodes.Contains(p.PermissionCode));
            foreach (var permission in readOnlyPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = readOnlyRole.Id,
                    PermissionId = permission.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }

            context.RolePermissions.AddRange(rolePermissions);
        }

        /// <summary>
        /// 建立預設系統管理員帳號
        /// </summary>
        private static async Task SeedDefaultAdminAsync(AppDbContext context)
        {
            if (await context.Employees.AnyAsync())
                return;

            var systemAdminRole = await context.Roles.FirstAsync(r => r.RoleName == "系統管理員");

            var admin = new Employee
            {
                EmployeeCode = "ADMIN001",
                Name = "系統管理員",
                FirstName = "System",
                LastName = "Administrator",
                Username = "admin",
                PasswordHash = HashPassword("Admin@123456"), // 預設密碼
                Email = "admin@erpcore2.com",
                Department = "IT",
                Position = "系統管理員",
                RoleId = systemAdminRole.Id,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Employees.Add(admin);
        }

        /// <summary>
        /// 密碼雜湊
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        #endregion
    }
}
