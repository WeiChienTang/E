using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERPCore2.Services
{
    /// <summary>
    /// Tab 頁籤顯示設定服務實作
    /// 使用記憶體快取（30 分鐘）減少資料庫查詢，按模組分別快取
    /// </summary>
    public class TabDisplaySettingService : ITabDisplaySettingService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;

        private const string CacheKeyPrefix = "TabDisplaySettings_";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

        public TabDisplaySettingService(
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public async Task<List<TabDisplaySetting>> GetByModuleAsync(string targetModule)
        {
            if (string.IsNullOrWhiteSpace(targetModule))
                return new List<TabDisplaySetting>();

            var cacheKey = CacheKeyPrefix + targetModule;

            if (_cache.TryGetValue(cacheKey, out List<TabDisplaySetting>? cached) && cached != null)
                return cached;

            using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await context.TabDisplaySettings
                .AsNoTracking()
                .Where(s => s.TargetModule == targetModule)
                .OrderBy(s => s.SortOrder ?? int.MaxValue)
                .ThenBy(s => s.TabKey)
                .ToListAsync();

            _cache.Set(cacheKey, settings, CacheExpiry);
            return settings;
        }

        public async Task<ServiceResult> SaveModuleSettingsAsync(
            string targetModule,
            List<TabDisplaySetting> settings,
            string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var existingSettings = await context.TabDisplaySettings
                    .Where(s => s.TargetModule == targetModule)
                    .ToListAsync();

                var now = DateTime.UtcNow;

                foreach (var setting in settings)
                {
                    if (setting.IsDefaultSetting())
                    {
                        var toRemove = existingSettings
                            .FirstOrDefault(e => e.TabKey == setting.TabKey);
                        if (toRemove != null)
                            context.TabDisplaySettings.Remove(toRemove);

                        continue;
                    }

                    var existing = existingSettings
                        .FirstOrDefault(e => e.TabKey == setting.TabKey);

                    if (existing != null)
                    {
                        existing.IsVisible = setting.IsVisible;
                        existing.SortOrder = setting.SortOrder;
                        existing.UpdatedBy = updatedBy;
                        existing.UpdatedAt = now;
                    }
                    else
                    {
                        setting.TargetModule = targetModule;
                        setting.CreatedBy = updatedBy;
                        setting.CreatedAt = now;
                        context.TabDisplaySettings.Add(setting);
                    }
                }

                await context.SaveChangesAsync();
                ClearCache(targetModule);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"儲存 Tab 設定時發生錯誤：{ex.Message}");
            }
        }

        public async Task<ServiceResult> ResetModuleSettingsAsync(string targetModule)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var settings = await context.TabDisplaySettings
                    .Where(s => s.TargetModule == targetModule)
                    .ToListAsync();

                if (settings.Any())
                {
                    context.TabDisplaySettings.RemoveRange(settings);
                    await context.SaveChangesAsync();
                }

                ClearCache(targetModule);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"重設 Tab 設定時發生錯誤：{ex.Message}");
            }
        }

        public void ClearCache(string targetModule)
        {
            _cache.Remove(CacheKeyPrefix + targetModule);
        }
    }
}
