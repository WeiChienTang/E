using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 紙張設定服務實作
    /// </summary>
    public class PaperSettingService : GenericManagementService<PaperSetting>, IPaperSettingService
    {
        public PaperSettingService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PaperSetting>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<PaperSetting>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaperSettings
                    .Where(p => (p.Code!.Contains(searchTerm) ||
                         p.Name.Contains(searchTerm) ||
                         (p.Remarks != null && p.Remarks.Contains(searchTerm))))
                    .OrderBy(p => p.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<PaperSetting>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PaperSetting entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("紙張名稱不能為空");

                if (string.IsNullOrWhiteSpace(entity.PaperType))
                    errors.Add("紙張類型不能為空");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("紙張編號不能為空");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("紙張編號已存在");

                if (!ValidatePaperSize(entity.Width, entity.Height))
                    errors.Add("紙張尺寸設定不合理");

                if (!ValidateMargins(entity))
                    errors.Add("邊距設定不合理（邊距總和不能超過紙張尺寸）");

                if (!IsValidOrientation(entity.Orientation))
                    errors.Add("紙張方向必須為 Portrait 或 Landscape");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.Name
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<PaperSetting?> GetDefaultPaperSettingAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 返回第一個啟用的 A4 紙張設定作為預設，如果沒有則返回第一個啟用的
                var a4Setting = await context.PaperSettings
                    .Where(p => p.Status == EntityStatus.Active && p.PaperType == "A4")
                    .FirstOrDefaultAsync();

                if (a4Setting != null)
                    return a4Setting;

                // 如果沒有 A4，返回第一個啟用的紙張設定
                return await context.PaperSettings
                    .Where(p => p.Status == EntityStatus.Active)
                    .OrderBy(p => p.Code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDefaultPaperSettingAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDefaultPaperSettingAsync),
                    ServiceType = GetType().Name
                });
                return null;
            }
        }

        public async Task<PaperSetting?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaperSettings
                    .Where(p => p.Code == code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCodeAsync),
                    ServiceType = GetType().Name,
                    Code = code
                });
                return null;
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PaperSettings.Where(p => p.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<bool> IsPaperSettingCodeExistsAsync(string code, int? excludeId = null)
        {
            // 委派給現有的 IsCodeExistsAsync 方法
            return await IsCodeExistsAsync(code, excludeId);
        }

        public async Task<List<PaperSetting>> GetActivePaperSettingsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaperSettings
                    .Where(p => p.Status == EntityStatus.Active)
                    .OrderBy(p => p.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActivePaperSettingsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetActivePaperSettingsAsync),
                    ServiceType = GetType().Name
                });
                return new List<PaperSetting>();
            }
        }

        public async Task<List<PaperSetting>> GetByPaperTypeAsync(string paperType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paperType))
                    return new List<PaperSetting>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaperSettings
                    .Where(p => p.Status == EntityStatus.Active && p.PaperType == paperType)
                    .OrderBy(p => p.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPaperTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPaperTypeAsync),
                    ServiceType = GetType().Name,
                    PaperType = paperType
                });
                return new List<PaperSetting>();
            }
        }

        public async Task<ServiceResult> SetAsDefaultAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查目標紙張設定是否存在且啟用
                var targetSetting = await context.PaperSettings
                    .Where(p => p.Id == id && p.Status == EntityStatus.Active)
                    .FirstOrDefaultAsync();

                if (targetSetting == null)
                    return ServiceResult.Failure("找不到指定的紙張設定或該設定未啟用");

                // 由於沒有 IsDefault 屬性，這個方法主要用於驗證紙張設定存在且有效
                // 實際的預設邏輯由 GetDefaultPaperSettingAsync 處理
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetAsDefaultAsync), GetType(), _logger, new
                {
                    Method = nameof(SetAsDefaultAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure("設定預設紙張時發生錯誤");
            }
        }

        public bool ValidatePaperSize(decimal width, decimal height)
        {
            // 檢查基本範圍
            if (width <= 0 || height <= 0)
                return false;

            if (width > 9999 || height > 9999)
                return false;

            // 檢查最小合理尺寸（不能小於10mm）
            if (width < 10 || height < 10)
                return false;

            return true;
        }

        public bool ValidateMargins(PaperSetting paperSetting)
        {
            // 檢查邊距不能為負值
            if (paperSetting.TopMargin < 0 || paperSetting.BottomMargin < 0 ||
                paperSetting.LeftMargin < 0 || paperSetting.RightMargin < 0)
                return false;

            // 檢查邊距總和不能超過紙張尺寸
            var totalHorizontalMargin = paperSetting.LeftMargin + paperSetting.RightMargin;
            var totalVerticalMargin = paperSetting.TopMargin + paperSetting.BottomMargin;

            if (totalHorizontalMargin >= paperSetting.Width)
                return false;

            if (totalVerticalMargin >= paperSetting.Height)
                return false;

            return true;
        }

        public Dictionary<string, (decimal Width, decimal Height)> GetStandardPaperSizes()
        {
            return new Dictionary<string, (decimal Width, decimal Height)>
            {
                { "A4", (210, 297) },
                { "A3", (297, 420) },
                { "A5", (148, 210) },
                { "Letter", (216, 279) },
                { "Legal", (216, 356) },
                { "B4", (250, 353) },
                { "B5", (176, 250) }
            };
        }

        private static bool IsValidOrientation(string orientation)
        {
            return orientation == "Portrait" || orientation == "Landscape";
        }
    }
}

