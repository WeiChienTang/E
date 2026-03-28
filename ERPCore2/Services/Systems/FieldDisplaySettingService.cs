using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ERPCore2.Services
{
    /// <summary>
    /// 欄位顯示設定服務實作
    /// 使用記憶體快取（30 分鐘）減少資料庫查詢，按模組分別快取
    /// </summary>
    public class FieldDisplaySettingService : IFieldDisplaySettingService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;

        private const string CacheKeyPrefix = "FieldDisplaySettings_";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

        public FieldDisplaySettingService(
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public async Task<List<FieldDisplaySetting>> GetByModuleAsync(string targetModule)
        {
            if (string.IsNullOrWhiteSpace(targetModule))
                return new List<FieldDisplaySetting>();

            var cacheKey = CacheKeyPrefix + targetModule;

            if (_cache.TryGetValue(cacheKey, out List<FieldDisplaySetting>? cached) && cached != null)
                return cached;

            using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await context.FieldDisplaySettings
                .AsNoTracking()
                .Where(s => s.TargetModule == targetModule)
                .OrderBy(s => s.SortOrder ?? int.MaxValue)
                .ThenBy(s => s.FieldName)
                .ToListAsync();

            _cache.Set(cacheKey, settings, CacheExpiry);
            return settings;
        }

        public async Task<ServiceResult> SaveModuleSettingsAsync(
            string targetModule,
            List<FieldDisplaySetting> settings,
            string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得資料庫中該模組現有的設定
                var existingSettings = await context.FieldDisplaySettings
                    .Where(s => s.TargetModule == targetModule)
                    .ToListAsync();

                var now = DateTime.UtcNow;

                foreach (var setting in settings)
                {
                    // 判斷此設定是否全部為預設值（null）— 如果是，不需要儲存
                    if (IsDefaultSetting(setting))
                    {
                        // 如果資料庫中已有此設定，刪除它（恢復為程式碼預設值）
                        var toRemove = existingSettings
                            .FirstOrDefault(e => e.FieldName == setting.FieldName);
                        if (toRemove != null)
                            context.FieldDisplaySettings.Remove(toRemove);

                        continue;
                    }

                    var existing = existingSettings
                        .FirstOrDefault(e => e.FieldName == setting.FieldName);

                    if (existing != null)
                    {
                        // 更新現有設定
                        existing.DisplayNameOverride = setting.DisplayNameOverride;
                        existing.ShowInForm = setting.ShowInForm;
                        existing.ShowInList = setting.ShowInList;
                        existing.IsRequiredOverride = setting.IsRequiredOverride;
                        existing.SortOrder = setting.SortOrder;
                        existing.HelpTextOverride = setting.HelpTextOverride;
                        existing.UpdatedBy = updatedBy;
                        existing.UpdatedAt = now;
                    }
                    else
                    {
                        // 新增設定
                        setting.TargetModule = targetModule;
                        setting.CreatedBy = updatedBy;
                        setting.CreatedAt = now;
                        context.FieldDisplaySettings.Add(setting);
                    }
                }

                await context.SaveChangesAsync();
                ClearCache(targetModule);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"儲存欄位設定時發生錯誤：{ex.Message}");
            }
        }

        public async Task<ServiceResult> ResetModuleSettingsAsync(string targetModule)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var settings = await context.FieldDisplaySettings
                    .Where(s => s.TargetModule == targetModule)
                    .ToListAsync();

                if (settings.Any())
                {
                    context.FieldDisplaySettings.RemoveRange(settings);
                    await context.SaveChangesAsync();
                }

                ClearCache(targetModule);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"重設欄位設定時發生錯誤：{ex.Message}");
            }
        }

        public void ClearCache(string targetModule)
        {
            _cache.Remove(CacheKeyPrefix + targetModule);
        }

        /// <summary>
        /// 判斷設定是否所有覆蓋值都是 null（= 全部使用程式碼預設值，不需要儲存）
        /// </summary>
        private static bool IsDefaultSetting(FieldDisplaySetting setting)
        {
            return setting.DisplayNameOverride == null
                && setting.ShowInForm == null
                && setting.ShowInList == null
                && setting.IsRequiredOverride == null
                && setting.SortOrder == null
                && setting.HelpTextOverride == null;
        }
    }
}
