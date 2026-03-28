using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 訂單照片服務實作
    /// </summary>
    public class SalesOrderPhotoService : GenericManagementService<SalesOrderPhoto>, ISalesOrderPhotoService
    {
        public SalesOrderPhotoService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesOrderPhoto>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public SalesOrderPhotoService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<SalesOrderPhoto> BuildGetAllQuery(AppDbContext context)
        {
            return context.SalesOrderPhotos
                .OrderBy(p => p.SalesOrderId)
                .ThenBy(p => p.SortOrder);
        }

        public override async Task<ServiceResult> ValidateAsync(SalesOrderPhoto entity)
        {
            try
            {
                if (entity.SalesOrderId <= 0)
                    return ServiceResult.Failure("訂單ID無效");

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

        public override async Task<List<SalesOrderPhoto>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderPhotos
                    .Where(p => p.Caption != null && p.Caption.Contains(searchTerm))
                    .OrderBy(p => p.SalesOrderId)
                    .ThenBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public async Task<List<SalesOrderPhoto>> GetBySalesOrderAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderPhotos
                    .Where(p => p.SalesOrderId == salesOrderId)
                    .OrderBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderAsync), GetType(), _logger, new { SalesOrderId = salesOrderId });
                throw;
            }
        }
    }
}
