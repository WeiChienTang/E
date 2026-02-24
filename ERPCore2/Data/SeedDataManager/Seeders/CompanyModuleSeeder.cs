using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Navigation;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 公司模組種子資料
    /// 從 NavigationConfig 父級選單自動建立 CompanyModule 記錄
    /// 當 NavigationConfig 新增帶有 ModuleKey 的父級選單時，Seeder 會自動建立對應模組
    /// </summary>
    public class CompanyModuleSeeder : IDataSeeder
    {
        public int Order => 5; // 在權限等基礎資料之前執行，確保模組資料先就位
        public string Name => "公司模組";

        public async Task SeedAsync(AppDbContext context)
        {
            // 從 NavigationConfig 取得所有帶有 ModuleKey 的父級選單項目
            var parentModules = NavigationConfig.GetAllNavigationItems()
                .Where(item => item.IsParent && !string.IsNullOrEmpty(item.ModuleKey))
                .ToList();

            if (!parentModules.Any())
            {
                return;
            }

            // 取得資料庫中已存在的 ModuleKey
            var existingModuleKeys = await context.CompanyModules
                .Select(m => m.ModuleKey)
                .ToListAsync();

            // 建立尚未存在的模組記錄
            var newModules = new List<CompanyModule>();
            int sortOrder = existingModuleKeys.Count > 0
                ? (await context.CompanyModules.MaxAsync(m => m.SortOrder)) + 10
                : 10;

            foreach (var parentItem in parentModules)
            {
                // 如果 ModuleKey 已存在，跳過（避免違反唯一索引）
                if (existingModuleKeys.Contains(parentItem.ModuleKey!))
                {
                    continue;
                }

                var module = new CompanyModule
                {
                    ModuleKey = parentItem.ModuleKey!,
                    DisplayName = parentItem.Category ?? $"{parentItem.Name}管理",
                    Description = parentItem.Description,
                    IsEnabled = true,
                    SortOrder = sortOrder,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                };

                newModules.Add(module);
                sortOrder += 10;
            }

            // 批次新增
            if (newModules.Any())
            {
                await context.CompanyModules.AddRangeAsync(newModules);
                await context.SaveChangesAsync();
            }
        }
    }
}
