using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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
        /// 初始化角色權限關聯（逐筆檢查，支援新增權限後自動補充）
        /// </summary>
        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            // 取得角色和所有權限
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "管理員");
            var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "一般員工");
            var allPermissions = await context.Permissions.ToListAsync();

            // 取得已存在的角色權限對（RoleId + PermissionId）
            var existingPairs = await context.RolePermissions
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToListAsync();
            var existingPairSet = existingPairs
                .Select(rp => (rp.RoleId, rp.PermissionId))
                .ToHashSet();

            var rolePermissions = new List<RolePermission>();

            // 管理員擁有所有權限
            if (adminRole != null)
            {
                rolePermissions.AddRange(allPermissions
                    .Where(p => !existingPairSet.Contains((adminRole.Id, p.Id)))
                    .Select(p => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = p.Id,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "System"
                    }));
            }

            // 一般員工只擁有 Normal 級別的權限
            if (employeeRole != null)
            {
                rolePermissions.AddRange(allPermissions
                    .Where(p => p.Level == PermissionLevel.Normal && !existingPairSet.Contains((employeeRole.Id, p.Id)))
                    .Select(p => new RolePermission
                    {
                        RoleId = employeeRole.Id,
                        PermissionId = p.Id,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "System"
                    }));
            }

            if (rolePermissions.Any())
            {
                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
