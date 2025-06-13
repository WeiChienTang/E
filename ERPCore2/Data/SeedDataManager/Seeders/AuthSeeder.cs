using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 認證系統種子器
    /// </summary>
    public class AuthSeeder : IDataSeeder
    {
        public int Order => 0;
        public string Name => "認證系統";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPermissionsAsync(context);
            await SeedRolesAsync(context);
            await SeedRolePermissionsAsync(context);
            await SeedDefaultAdminAsync(context);
        }

        /// <summary>
        /// 初始化權限資料
        /// </summary>
        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            var permissions = new[]
            {
                // 系統管理權限
                new Permission { PermissionCode = "System.Admin", PermissionName = "系統管理員權限", Module = "System", Action = "Admin", PermissionGroup = "系統管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Create", PermissionName = "建立使用者", Module = "User", Action = "Create", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Read", PermissionName = "檢視使用者", Module = "User", Action = "Read", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Update", PermissionName = "修改使用者", Module = "User", Action = "Update", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Delete", PermissionName = "刪除使用者", Module = "User", Action = "Delete", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 客戶管理權限
                new Permission { PermissionCode = "Customer.Create", PermissionName = "建立客戶", Module = "Customer", Action = "Create", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Read", PermissionName = "檢視客戶", Module = "Customer", Action = "Read", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Update", PermissionName = "修改客戶", Module = "Customer", Action = "Update", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Delete", PermissionName = "刪除客戶", Module = "Customer", Action = "Delete", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                  // 供應商管理權限
                new Permission { PermissionCode = "Supplier.Create", PermissionName = "建立供應商", Module = "Supplier", Action = "Create", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Read", PermissionName = "檢視供應商", Module = "Supplier", Action = "Read", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Update", PermissionName = "修改供應商", Module = "Supplier", Action = "Update", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Delete", PermissionName = "刪除供應商", Module = "Supplier", Action = "Delete", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 員工管理權限
                new Permission { PermissionCode = "Employee.Create", PermissionName = "建立員工", Module = "Employee", Action = "Create", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Read", PermissionName = "檢視員工", Module = "Employee", Action = "Read", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Update", PermissionName = "修改員工", Module = "Employee", Action = "Update", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Delete", PermissionName = "刪除員工", Module = "Employee", Action = "Delete", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 產品管理權限
                new Permission { PermissionCode = "Product.Create", PermissionName = "建立產品", Module = "Product", Action = "Create", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Read", PermissionName = "檢視產品", Module = "Product", Action = "Read", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Update", PermissionName = "修改產品", Module = "Product", Action = "Update", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Delete", PermissionName = "刪除產品", Module = "Product", Action = "Delete", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化角色資料
        /// </summary>
        private static async Task SeedRolesAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new[]
            {
                new Role { RoleName = "Administrator", Description = "系統管理員", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Role { RoleName = "Manager", Description = "部門主管", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Role { RoleName = "Employee", Description = "一般員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Role { RoleName = "Sales", Description = "銷售人員", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Role { RoleName = "Purchasing", Description = "採購人員", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化角色權限關聯
        /// </summary>
        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;

            // 取得角色和權限
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
            var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Manager");
            var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Employee");
            var salesRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Sales");
            var purchasingRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Purchasing");

            var allPermissions = await context.Permissions.ToListAsync();
            var rolePermissions = new List<RolePermission>();

            // 管理員擁有所有權限
            if (adminRole != null)
            {
                rolePermissions.AddRange(allPermissions.Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }));
            }

            // 部門主管權限（除了系統管理）
            if (managerRole != null)
            {
                var managerPermissions = allPermissions.Where(p => !p.PermissionCode.StartsWith("System."));
                rolePermissions.AddRange(managerPermissions.Select(p => new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = p.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }));
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 建立預設系統管理員帳號
        /// </summary>
        private static async Task SeedDefaultAdminAsync(AppDbContext context)
        {
            if (await context.Employees.AnyAsync())
                return;

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");

            var admin = new Employee
            {
                EmployeeCode = "ADMIN001",
                FirstName = "系統",
                LastName = "管理員",
                Username = "admin",
                PasswordHash = SeedDataHelper.HashPassword("admin123"),
                Email = "admin@erpcore2.com",
                Department = "IT",
                Position = "系統管理員",
                RoleId = adminRole?.Id ?? 1,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            await context.Employees.AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
