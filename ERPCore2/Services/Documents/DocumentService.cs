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

        protected override IQueryable<Document> BuildGetAllQuery(AppDbContext context)
        {
            return context.Documents
                .Include(d => d.DocumentCategory)
                .Include(d => d.DocumentFiles)
                .OrderByDescending(d => d.CreatedAt);
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
                    .Include(d => d.DocumentFiles)
                    .Where(d => d.Title.Contains(searchTerm) ||
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

        public async Task<Document?> GetWithFilesAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .Include(d => d.DocumentFiles.OrderBy(f => f.SortOrder))
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithFilesAsync), GetType(), _logger, new
                {
                    Method = nameof(GetWithFilesAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public async Task<List<Document>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .Include(d => d.DocumentFiles)
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

        public async Task<List<Document>> GetByRelatedEntityAsync(string entityType, int entityId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Documents
                    .Include(d => d.DocumentCategory)
                    .Include(d => d.DocumentFiles)
                    .Where(d => d.RelatedEntityType == entityType && d.RelatedEntityId == entityId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByRelatedEntityAsync), GetType(), _logger, new
                {
                    EntityType = entityType,
                    EntityId = entityId
                });
                return new List<Document>();
            }
        }

        public async Task<string?> GetRelatedEntityDisplayNameAsync(string? entityType, int? entityId)
        {
            if (string.IsNullOrEmpty(entityType) || !entityId.HasValue || entityId.Value <= 0)
                return null;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return entityType switch
                {
                    "Customer" => (await context.Customers.FindAsync(entityId.Value))?.CompanyName,
                    "Supplier" => (await context.Suppliers.FindAsync(entityId.Value))?.CompanyName,
                    "Employee" => (await context.Employees.FindAsync(entityId.Value))?.Name,
                    "Company" => (await context.Companies.FindAsync(entityId.Value))?.CompanyName,
                    "GovernmentAgency" => (await context.GovernmentAgencies.FindAsync(entityId.Value))?.AgencyName,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRelatedEntityDisplayNameAsync), GetType(), _logger);
                return null;
            }
        }

        #region 伺服器端分頁

        public async Task<(List<Document> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Document>, IQueryable<Document>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<Document> query = context.Documents
                    .Include(d => d.DocumentCategory);
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<Document>(), 0);
            }
        }

        #endregion
    }
}
