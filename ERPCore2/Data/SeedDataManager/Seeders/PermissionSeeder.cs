using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 權限種子器 — 從 PermissionRegistry 完整同步所有權限至資料庫
    /// • 新增：Registry 中有但 DB 中無的權限
    /// • 更新：DB 中已存在但屬性與 Registry 不同的權限（Name / Level / Remarks）
    /// • 清理：DB 中存在但 Registry 中已移除的過時權限
    /// </summary>
    public class PermissionSeeder : IDataSeeder
    {
        public int Order => 0;
        public string Name => "權限管理";

        public async Task SeedAsync(AppDbContext context)
        {
            var registryCodes = PermissionRegistry.GetAllPermissions()
                .Select(d => d.Code)
                .ToHashSet();

            var existingPermissions = await context.Permissions.ToListAsync();
            var existingCodes = existingPermissions.Select(p => p.Code).ToHashSet();

            // 新增：Registry 中有但資料庫中沒有的權限
            var toAdd = PermissionRegistry.GetAllPermissions()
                .Where(d => !existingCodes.Contains(d.Code))
                .Select(d => new Permission
                {
                    Code = d.Code,
                    Name = d.Name,
                    Level = d.Level,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Remarks = d.Remarks
                })
                .ToArray();

            // 清理：資料庫中有但 Registry 中已不存在的過時權限
            var toRemove = existingPermissions
                .Where(p => !string.IsNullOrEmpty(p.Code) && !registryCodes.Contains(p.Code))
                .ToArray();

            // 更新：資料庫中已存在但屬性與 Registry 不同的權限
            var registryLookup = PermissionRegistry.GetAllPermissions()
                .ToDictionary(d => d.Code);
            var updatedCount = 0;
            foreach (var existing in existingPermissions.Where(p => !string.IsNullOrEmpty(p.Code) && registryCodes.Contains(p.Code)))
            {
                if (registryLookup.TryGetValue(existing.Code!, out var def))
                {
                    if (existing.Name != def.Name || existing.Level != def.Level || existing.Remarks != def.Remarks)
                    {
                        existing.Name = def.Name;
                        existing.Level = def.Level;
                        existing.Remarks = def.Remarks;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = "System";
                        updatedCount++;
                    }
                }
            }

            if (toRemove.Length > 0)
            {
                context.Permissions.RemoveRange(toRemove);
            }

            if (toAdd.Length > 0)
            {
                await context.Permissions.AddRangeAsync(toAdd);
            }

            if (toAdd.Length > 0 || toRemove.Length > 0 || updatedCount > 0)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
