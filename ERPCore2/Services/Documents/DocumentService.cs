using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class DocumentService : GenericManagementService<Document>, IDocumentService
    {
        public DocumentService(IDbContextFactory<AppDbContext> contextFactory, ILogger<GenericManagementService<Document>> logger)
            : base(contextFactory, logger)
        {
        }

        public override async Task<List<Document>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<Document>();
            }
        }

        public override async Task<List<Document>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .Where(d => d.Title.Contains(searchTerm) ||
                               d.FileName.Contains(searchTerm) ||
                               (d.IssuedBy != null && d.IssuedBy.Contains(searchTerm)) ||
                               (d.Code != null && d.Code.Contains(searchTerm)))
                    .OrderByDescending(d => d.CreatedAt)
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
                return new List<Document>();
            }
        }

        public async Task<List<Document>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .Where(d => d.DocumentCategoryId == categoryId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCategoryAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCategoryAsync),
                    ServiceType = GetType().Name,
                    CategoryId = categoryId
                });
                return new List<Document>();
            }
        }

        public async Task<bool> IsDocumentCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Documents.Where(d => d.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDocumentCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsDocumentCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Document entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Title))
                    errors.Add("文件標題為必填欄位");

                if (entity.DocumentCategoryId <= 0)
                    errors.Add("請選擇檔案分類");

                if (string.IsNullOrWhiteSpace(entity.FilePath))
                    errors.Add("請上傳檔案");

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
                    EntityTitle = entity.Title
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
