using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
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
        }

        /// <summary>
        /// 初始化角色權限關聯
        /// </summary>
        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;

            // 取得角色和權限
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "管理員");
            var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "辦公室員工");
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

            // 辦公室員工擁有除了系統管理、角色管理、權限管理相關權限之外的所有權限
            if (employeeRole != null)
            {
                var officeEmployeePermissions = allPermissions
                    .Where(p => p.Code != null && 
                               !p.Code.StartsWith("System") && 
                               !p.Code.StartsWith("Role") && 
                               !p.Code.StartsWith("EmployeeEdit_Account_Password") &&
                               !p.Code.StartsWith("Permission") &&
                               !p.Code.StartsWith("Company"))
                    .ToList();

                rolePermissions.AddRange(officeEmployeePermissions.Select(p => new RolePermission
                {
                    RoleId = employeeRole.Id,
                    PermissionId = p.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }));
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
