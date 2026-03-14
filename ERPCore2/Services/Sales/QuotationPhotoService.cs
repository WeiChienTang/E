using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單照片服務實作
    /// </summary>
    public class QuotationPhotoService : GenericManagementService<QuotationPhoto>, IQuotationPhotoService
    {
        public QuotationPhotoService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<QuotationPhoto>> logger) : base(contextFactory, logger)
        {
        }

        public QuotationPhotoService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<QuotationPhoto> BuildGetAllQuery(AppDbContext context)
        {
            return context.QuotationPhotos
                .OrderBy(p => p.QuotationId)
                .ThenBy(p => p.SortOrder);
        }

        public override async Task<ServiceResult> ValidateAsync(QuotationPhoto entity)
        {
            try
            {
                if (entity.QuotationId <= 0)
                    return ServiceResult.Failure("報價單ID無效");

                if (string.IsNullOrWhiteSpace(entity.PhotoPath))
                    return ServiceResult.Failure("照片路徑不可為空");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<QuotationPhoto>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationPhotos
                    .Where(p => p.Caption != null && p.Caption.Contains(searchTerm))
                    .OrderBy(p => p.QuotationId)
                    .ThenBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public async Task<List<QuotationPhoto>> GetByQuotationAsync(int quotationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationPhotos
                    .Where(p => p.QuotationId == quotationId)
                    .OrderBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByQuotationAsync), GetType(), _logger, new { QuotationId = quotationId });
                throw;
            }
        }
    }
}
