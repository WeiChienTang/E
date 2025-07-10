using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 角色權限關聯種子器
    /// </summary>
    public class RolePermissionSeeder : IDataSeeder
    {
        public int Order => 2;
        public string Name => "角色權限關聯";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedRolePermissionsAsync(context);
            await SeedDefaultAdminAsync(context);
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

            // 取得系統管理員角色
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");

            // 建立系統管理員帳號
            var adminEmployee = new Employee
            {
                EmployeeCode = "ADMIN001",
                FirstName = "系統",
                LastName = "管理員",
                Username = "admin",
                PasswordHash = SeedDataHelper.HashPassword("admin123"),
                Department = "IT",
                RoleId = adminRole?.Id ?? 1,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();

            // 為系統管理員添加Email聯絡資料
            var emailContactType = await context.ContactTypes
                .FirstOrDefaultAsync(ct => ct.TypeName == "Email");

            if (emailContactType != null)
            {
                var emailContact = new EmployeeContact
                {
                    EmployeeId = adminEmployee.Id,
                    ContactTypeId = emailContactType.Id,
                    ContactValue = $"{adminEmployee.Username}@erpcore2.com",
                    IsPrimary = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                };

                await context.EmployeeContacts.AddAsync(emailContact);
                await context.SaveChangesAsync();
            }
        }
    }
}
