using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 權限種子器 — 從 PermissionRegistry 同步所有權限至資料庫
    /// </summary>
    public class PermissionSeeder : IDataSeeder
    {
        public int Order => 0;
        public string Name => "權限管理";

        public async Task SeedAsync(AppDbContext context)
        {
            var existingCodes = await context.Permissions
                .Select(p => p.Code)
                .ToHashSetAsync();

            var toAdd = PermissionRegistry.GetAllPermissions()
                .Where(d => !existingCodes.Contains(d.Code))
                .Select(d => new Permission
                {
                    Code = d.Code,
                    Name = d.Name,
                    Level = d.Level,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = d.Remarks
                })
                .ToArray();

            if (toAdd.Length > 0)
            {
                await context.Permissions.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
