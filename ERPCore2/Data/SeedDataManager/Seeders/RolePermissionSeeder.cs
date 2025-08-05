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
            var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Employee");
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

            // 員工角色不分配任何權限 (保留此註解以明確說明設計意圖)

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
