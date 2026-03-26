using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Navigation;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 代碼自動產生設定資料種子器
    /// </summary>
    public class CodeSettingSeeder : IDataSeeder
    {
        public int Order => 3; // 在系統參數之後執行
        public string Name => "代碼自動產生設定";

        public async Task SeedAsync(AppDbContext context)
        {
            var existingKeys = await context.CodeSettings
                .Select(c => c.ModuleKey)
                .ToListAsync();

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);

            var missingSettings = CodeSettingDefaults.DefaultModules
                .Where(m => !existingKeys.Contains(m.ModuleKey))
                .Select(m => new CodeSetting
                {
                    ModuleKey = m.ModuleKey,
                    ModuleDisplayName = m.DisplayName,
                    IsAutoCode = m.IsAutoCode,
                    Prefix = m.Prefix,
                    FormatTemplate = m.FormatTemplate,
                    CurrentSeq = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy
                }).ToArray();

            if (missingSettings.Length > 0)
            {
                await context.CodeSettings.AddRangeAsync(missingSettings);
                await context.SaveChangesAsync();
            }
        }
    }
}
