using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 代碼自動產生設定服務實作
    /// </summary>
    public class CodeSettingService : GenericManagementService<CodeSetting>, ICodeSettingService
    {
        private const int MaxRetries = 3;

        public CodeSettingService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CodeSetting>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 取得所有模組設定（按 ModuleKey 排序）
        /// </summary>
        public async Task<List<CodeSetting>> GetAllSettingsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CodeSettings
                    .Where(cs => cs.Status == EntityStatus.Active)
                    .OrderBy(cs => cs.ModuleKey)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "取得代碼設定列表失敗");
                return new List<CodeSetting>();
            }
        }

        /// <summary>
        /// 取得特定模組設定
        /// </summary>
        public async Task<CodeSetting?> GetByModuleKeyAsync(string moduleKey)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CodeSettings
                    .FirstOrDefaultAsync(cs => cs.ModuleKey == moduleKey && cs.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "取得模組 {ModuleKey} 的代碼設定失敗", moduleKey);
                return null;
            }
        }

        /// <summary>
        /// 產生並更新代碼（含樂觀鎖重試）
        /// </summary>
        public async Task<ServiceResult<string?>> GenerateCodeAsync(string moduleKey)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var setting = await context.CodeSettings
                        .FirstOrDefaultAsync(cs => cs.ModuleKey == moduleKey && cs.Status == EntityStatus.Active);

                    if (setting == null)
                    {
                        return ServiceResult<string?>.Failure($"找不到模組 {moduleKey} 的代碼設定");
                    }

                    if (!setting.IsAutoCode)
                    {
                        return ServiceResult<string?>.Success(null);
                    }

                    var now = DateTime.Now;
                    var resetMode = InferResetMode(setting.FormatTemplate);

                    // 檢查是否需要重置序號
                    if (resetMode == "每月重置" && (setting.CurrentYear != now.Year || setting.CurrentMonth != now.Month))
                    {
                        setting.CurrentSeq = 0;
                        setting.CurrentYear = now.Year;
                        setting.CurrentMonth = now.Month;
                    }
                    else if (resetMode == "每年重置" && setting.CurrentYear != now.Year)
                    {
                        setting.CurrentSeq = 0;
                        setting.CurrentYear = now.Year;
                        setting.CurrentMonth = null;
                    }

                    // 遞增序號
                    setting.CurrentSeq++;
                    setting.UpdatedAt = DateTime.UtcNow;

                    await context.SaveChangesAsync();

                    // 代入樣板產生代碼
                    var code = ApplyTemplate(setting.FormatTemplate, setting.Prefix, setting.CurrentSeq, now);
                    return ServiceResult<string?>.Success(code);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                    {
                        _logger?.LogWarning("模組 {ModuleKey} 代碼產生失敗，已達最大重試次數 {MaxRetries}", moduleKey, MaxRetries);
                        return ServiceResult<string?>.Failure($"代碼產生失敗（並發衝突），請重試");
                    }
                    // 重試
                    await Task.Delay(50 * (attempt + 1));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "模組 {ModuleKey} 代碼產生時發生錯誤", moduleKey);
                    return ServiceResult<string?>.Failure($"代碼產生時發生錯誤：{ex.Message}");
                }
            }

            return ServiceResult<string?>.Failure("代碼產生失敗");
        }

        /// <summary>
        /// 批次儲存所有設定
        /// </summary>
        public async Task<ServiceResult> SaveAllAsync(List<CodeSetting> settings)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                foreach (var setting in settings)
                {
                    var existing = await context.CodeSettings
                        .FirstOrDefaultAsync(cs => cs.Id == setting.Id);

                    if (existing != null)
                    {
                        existing.IsAutoCode = setting.IsAutoCode;
                        existing.Prefix = setting.Prefix;
                        existing.FormatTemplate = setting.FormatTemplate;
                        existing.ModuleDisplayName = setting.ModuleDisplayName;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次儲存代碼設定失敗");
                return ServiceResult.Failure($"儲存代碼設定失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 重置特定模組的序號
        /// </summary>
        public async Task<ServiceResult> ResetSeqAsync(string moduleKey)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setting = await context.CodeSettings
                    .FirstOrDefaultAsync(cs => cs.ModuleKey == moduleKey && cs.Status == EntityStatus.Active);

                if (setting == null)
                {
                    return ServiceResult.Failure($"找不到模組 {moduleKey} 的代碼設定");
                }

                setting.CurrentSeq = 0;
                setting.CurrentYear = null;
                setting.CurrentMonth = null;
                setting.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "重置模組 {ModuleKey} 序號失敗", moduleKey);
                return ServiceResult.Failure($"重置序號失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 預覽代碼（不實際遞增序號）
        /// </summary>
        public string PreviewCode(string prefix, string formatTemplate, int seq = 1)
        {
            try
            {
                return ApplyTemplate(formatTemplate, prefix, seq, DateTime.Now);
            }
            catch
            {
                return "(格式錯誤)";
            }
        }

        /// <summary>
        /// 從格式樣板推斷重置規則
        /// </summary>
        public string InferResetMode(string formatTemplate)
        {
            if (string.IsNullOrEmpty(formatTemplate)) return "永不重置";
            if (formatTemplate.Contains("{Month:")) return "每月重置";
            if (formatTemplate.Contains("{Year:")) return "每年重置";
            return "永不重置";
        }

        public override Task<ServiceResult> ValidateAsync(CodeSetting entity)
        {
            if (string.IsNullOrWhiteSpace(entity.ModuleKey))
                return Task.FromResult(ServiceResult.Failure("模組識別鍵不可為空"));

            if (string.IsNullOrWhiteSpace(entity.Prefix))
                return Task.FromResult(ServiceResult.Failure("前綴不可為空"));

            if (string.IsNullOrWhiteSpace(entity.FormatTemplate))
                return Task.FromResult(ServiceResult.Failure("格式樣板不可為空"));

            if (!entity.FormatTemplate.Contains("{Seq:"))
                return Task.FromResult(ServiceResult.Failure("格式樣板必須包含 {Seq:N}"));

            return Task.FromResult(ServiceResult.Success());
        }

        public override async Task<List<CodeSetting>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CodeSettings
                    .Where(cs => cs.Status == EntityStatus.Active &&
                           (cs.ModuleKey.Contains(searchTerm) ||
                            cs.ModuleDisplayName.Contains(searchTerm) ||
                            cs.Prefix.Contains(searchTerm)))
                    .OrderBy(cs => cs.ModuleKey)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, "搜尋代碼設定", typeof(CodeSettingService));
                return new List<CodeSetting>();
            }
        }

        // ===== 私有方法 =====

        /// <summary>
        /// 套用格式樣板產生代碼
        /// </summary>
        private static string ApplyTemplate(string formatTemplate, string prefix, int seq, DateTime now)
        {
            var result = formatTemplate
                .Replace("{Prefix}", prefix)
                .Replace("{Year:yyyy}", now.Year.ToString("D4"))
                .Replace("{Year:yy}", (now.Year % 100).ToString("D2"))
                .Replace("{Month:MM}", now.Month.ToString("D2"))
                .Replace("{Day:dd}", now.Day.ToString("D2"));

            // 處理 {Seq:N} — N 為位數
            result = Regex.Replace(result, @"\{Seq:(\d)\}", m =>
            {
                var digits = int.Parse(m.Groups[1].Value);
                return seq.ToString($"D{digits}");
            });

            return result;
        }
    }
}
