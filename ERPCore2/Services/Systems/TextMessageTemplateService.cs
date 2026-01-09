using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 文字訊息範本服務實作
    /// </summary>
    public class TextMessageTemplateService : GenericManagementService<TextMessageTemplate>, ITextMessageTemplateService
    {
        public TextMessageTemplateService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<TextMessageTemplate>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 驗證範本實體
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(TextMessageTemplate entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.TemplateCode))
                    errors.Add("「範本代碼」為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.TemplateName))
                    errors.Add("「範本名稱」為必填欄位");

                // 問候語和結語可以為空白，不需驗證

                // 檢查範本代碼是否重複
                if (!string.IsNullOrWhiteSpace(entity.TemplateCode) && 
                    await IsTemplateCodeExistsAsync(entity.TemplateCode, entity.Id > 0 ? entity.Id : null))
                    errors.Add($"範本代碼 '{entity.TemplateCode}' 已存在，請使用其他代碼");

                if (errors.Any())
                    return ServiceResult.Failure("儲存失敗：" + string.Join("；", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    TemplateCode = entity.TemplateCode
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 搜尋範本
        /// </summary>
        public override async Task<List<TextMessageTemplate>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.TextMessageTemplates
                    .Where(t => t.Status != EntityStatus.Deleted &&
                         (t.TemplateCode.Contains(searchTerm) ||
                          t.TemplateName.Contains(searchTerm) ||
                          t.HeaderText.Contains(searchTerm) ||
                          t.FooterText.Contains(searchTerm)))
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.TemplateCode)
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
                return new List<TextMessageTemplate>();
            }
        }

        /// <summary>
        /// 根據範本代碼取得範本
        /// </summary>
        public async Task<TextMessageTemplate?> GetByTemplateCodeAsync(string templateCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.TextMessageTemplates
                    .Where(t => t.TemplateCode == templateCode && 
                                t.IsActive && 
                                t.Status == EntityStatus.Active)
                    .OrderBy(t => t.SortOrder)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTemplateCodeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByTemplateCodeAsync),
                    ServiceType = GetType().Name,
                    TemplateCode = templateCode
                });
                return null;
            }
        }

        /// <summary>
        /// 取得所有啟用的範本
        /// </summary>
        public async Task<List<TextMessageTemplate>> GetActiveTemplatesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.TextMessageTemplates
                    .Where(t => t.IsActive && t.Status == EntityStatus.Active)
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.TemplateCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveTemplatesAsync), GetType(), _logger, new
                {
                    Method = nameof(GetActiveTemplatesAsync),
                    ServiceType = GetType().Name
                });
                return new List<TextMessageTemplate>();
            }
        }

        /// <summary>
        /// 檢查範本代碼是否已存在
        /// </summary>
        public async Task<bool> IsTemplateCodeExistsAsync(string templateCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.TextMessageTemplates
                    .Where(t => t.TemplateCode == templateCode && t.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTemplateCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsTemplateCodeExistsAsync),
                    ServiceType = GetType().Name,
                    TemplateCode = templateCode,
                    ExcludeId = excludeId
                });
                return false;
            }
        }
        
        /// <summary>
        /// 檢查範本 Code 是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// 此方法檢查的是 TextMessageTemplate.Code 欄位，而非 TemplateCode
        /// </summary>
        public async Task<bool> IsTextMessageTemplateCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.TextMessageTemplates
                    .Where(t => t.Code == code && t.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTextMessageTemplateCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsTextMessageTemplateCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }
    }
}
