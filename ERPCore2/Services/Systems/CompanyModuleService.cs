using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERPCore2.Services
{
    /// <summary>
    /// 公司模組管理服務實作
    /// 使用記憶體快取（30 分鐘）減少資料庫查詢
    /// </summary>
    public class CompanyModuleService : ICompanyModuleService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "CompanyModules_IsEnabled";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

        public CompanyModuleService(
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public async Task<List<CompanyModule>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CompanyModules
                .AsNoTracking()
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.DisplayName)
                .ToListAsync();
        }

        public async Task<bool> IsModuleEnabledAsync(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                return true;

            var enabledModules = await GetEnabledModuleKeysAsync();

            // 若快取中找不到此模組（尚未建立），預設允許存取
            if (!enabledModules.ContainsKey(moduleKey))
                return true;

            return enabledModules[moduleKey];
        }

        public async Task<ServiceResult> UpdateModulesAsync(List<CompanyModule> modules, string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                foreach (var module in modules)
                {
                    var existing = await context.CompanyModules
                        .FirstOrDefaultAsync(m => m.Id == module.Id);

                    if (existing != null)
                    {
                        existing.IsEnabled = module.IsEnabled;
                        existing.UpdatedBy = updatedBy;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();

                // 清除快取，讓下次查詢重新載入
                ClearCache();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"儲存模組設定時發生錯誤：{ex.Message}");
            }
        }

        public void ClearCache()
        {
            _cache.Remove(CacheKey);
        }

        // ===== 私有輔助方法 =====

        private async Task<Dictionary<string, bool>> GetEnabledModuleKeysAsync()
        {
            if (_cache.TryGetValue(CacheKey, out Dictionary<string, bool>? cached) && cached != null)
                return cached;

            using var context = await _contextFactory.CreateDbContextAsync();
            var modules = await context.CompanyModules
                .AsNoTracking()
                .Select(m => new { m.ModuleKey, m.IsEnabled })
                .ToListAsync();

            var dict = modules.ToDictionary(m => m.ModuleKey, m => m.IsEnabled);

            _cache.Set(CacheKey, dict, CacheExpiry);
            return dict;
        }
    }
}
