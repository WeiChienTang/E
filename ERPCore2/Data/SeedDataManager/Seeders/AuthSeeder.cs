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
                new Permission { PermissionCode = "Product.Delete", PermissionName = "刪除產品", Module = "Product", Action = "Delete", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ViewCost", PermissionName = "檢視成本價", Module = "Product", Action = "ViewCost", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageStock", PermissionName = "管理庫存", Module = "Product", Action = "ManageStock", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageSupplier", PermissionName = "管理供應商關聯", Module = "Product", Action = "ManageSupplier", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                  // 產品分類管理權限
                new Permission { PermissionCode = "ProductCategory.Create", PermissionName = "建立產品分類", Module = "ProductCategory", Action = "Create", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Read", PermissionName = "檢視產品分類", Module = "ProductCategory", Action = "Read", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Update", PermissionName = "修改產品分類", Module = "ProductCategory", Action = "Update", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Delete", PermissionName = "刪除產品分類", Module = "ProductCategory", Action = "Delete", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // BOM基礎元素 - 材質管理權限
                new Permission { PermissionCode = "Material.Create", PermissionName = "建立材質", Module = "Material", Action = "Create", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Read", PermissionName = "檢視材質", Module = "Material", Action = "Read", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Update", PermissionName = "修改材質", Module = "Material", Action = "Update", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Delete", PermissionName = "刪除材質", Module = "Material", Action = "Delete", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // BOM基礎元素 - 天氣管理權限
                new Permission { PermissionCode = "Weather.Create", PermissionName = "建立天氣", Module = "Weather", Action = "Create", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Read", PermissionName = "檢視天氣", Module = "Weather", Action = "Read", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Update", PermissionName = "修改天氣", Module = "Weather", Action = "Update", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Delete", PermissionName = "刪除天氣", Module = "Weather", Action = "Delete", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // BOM基礎元素 - 顏色管理權限
                new Permission { PermissionCode = "Color.Create", PermissionName = "建立顏色", Module = "Color", Action = "Create", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Read", PermissionName = "檢視顏色", Module = "Color", Action = "Read", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Update", PermissionName = "修改顏色", Module = "Color", Action = "Update", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Delete", PermissionName = "刪除顏色", Module = "Color", Action = "Delete", PermissionGroup = "BOM基礎元素", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
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
        /// 建立預設系統管理員帳號和測試用戶
        /// </summary>
        private static async Task SeedDefaultAdminAsync(AppDbContext context)
        {
            if (await context.Employees.AnyAsync())
                return;

            // 取得各角色
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
            var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Manager");
            var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Employee");
            var salesRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Sales");
            var purchasingRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Purchasing");

            // 建立測試用戶
            var testEmployees = new[]
            {
                // 系統管理員
                new Employee
                {
                    EmployeeCode = "ADMIN001",
                    FirstName = "系統",
                    LastName = "管理員",
                    Username = "admin",
                    PasswordHash = SeedDataHelper.HashPassword("admin123"),
                    Department = "IT",
                    Position = "系統管理員",
                    RoleId = adminRole?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                // 部門主管
                new Employee
                {
                    EmployeeCode = "MGR001",
                    FirstName = "王",
                    LastName = "主管",
                    Username = "manager",
                    PasswordHash = SeedDataHelper.HashPassword("manager123"),
                    Department = "營運",
                    Position = "營運主管",
                    RoleId = managerRole?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                // 銷售人員
                new Employee
                {
                    EmployeeCode = "SALES001",
                    FirstName = "李",
                    LastName = "業務",
                    Username = "sales",
                    PasswordHash = SeedDataHelper.HashPassword("sales123"),
                    Department = "銷售",
                    Position = "業務代表",
                    RoleId = salesRole?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                // 採購人員
                new Employee
                {
                    EmployeeCode = "PUR001",
                    FirstName = "陳",
                    LastName = "採購",
                    Username = "purchasing",
                    PasswordHash = SeedDataHelper.HashPassword("purchasing123"),
                    Department = "採購",
                    Position = "採購專員",
                    RoleId = purchasingRole?.Id ?? 5,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                // 一般員工
                new Employee
                {
                    EmployeeCode = "EMP001",
                    FirstName = "張",
                    LastName = "員工",
                    Username = "employee",
                    PasswordHash = SeedDataHelper.HashPassword("employee123"),
                    Department = "倉庫",
                    Position = "倉庫管理員",
                    RoleId = employeeRole?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                // 測試用的停用帳號
                new Employee
                {
                    EmployeeCode = "TEST001",
                    FirstName = "測試",
                    LastName = "帳號",
                    Username = "testuser",
                    PasswordHash = SeedDataHelper.HashPassword("test123"),
                    Department = "測試",
                    Position = "測試人員",
                    RoleId = employeeRole?.Id ?? 3,
                    Status = EntityStatus.Inactive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.Employees.AddRangeAsync(testEmployees);
            await context.SaveChangesAsync();

            // 為每個員工添加Email聯絡資料
            var emailContactType = await context.ContactTypes
                .FirstOrDefaultAsync(ct => ct.TypeName == "Email");

            if (emailContactType != null)
            {
                var employeeContacts = new List<EmployeeContact>();
                var employees = await context.Employees.ToListAsync();

                foreach (var employee in employees)
                {
                    var emailContact = new EmployeeContact
                    {
                        EmployeeId = employee.Id,
                        ContactTypeId = emailContactType.Id,
                        ContactValue = $"{employee.Username}@erpcore2.com",
                        IsPrimary = true,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "System"
                    };
                    employeeContacts.Add(emailContact);
                }

                await context.EmployeeContacts.AddRangeAsync(employeeContacts);
                await context.SaveChangesAsync();
            }
        }
    }
}
