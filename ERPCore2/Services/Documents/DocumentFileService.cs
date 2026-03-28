using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class DocumentFileService : GenericManagementService<DocumentFile>, IDocumentFileService
    {
        public DocumentFileService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<DocumentFile>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null)
            : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public override async Task<List<DocumentFile>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DocumentFiles
                    .Where(f => f.DisplayName.Contains(searchTerm) || f.FileName.Contains(searchTerm))
                    .OrderBy(f => f.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<DocumentFile>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(DocumentFile entity)
        {
            try
            {
                var errors = new List<string>();

                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.DisplayName))
                    && string.IsNullOrWhiteSpace(entity.DisplayName))
                    errors.Add("附件名稱為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.FilePath))
                    errors.Add("檔案路徑為必填欄位");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<List<DocumentFile>> GetByDocumentAsync(int documentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DocumentFiles
                    .Where(f => f.DocumentId == documentId)
                    .OrderBy(f => f.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDocumentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDocumentAsync),
                    DocumentId = documentId
                });
                return new List<DocumentFile>();
            }
        }
    }
}
