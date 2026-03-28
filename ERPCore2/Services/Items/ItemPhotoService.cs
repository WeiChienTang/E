using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項照片服務實作
    /// </summary>
    public class ItemPhotoService : GenericManagementService<ItemPhoto>, IItemPhotoService
    {
        public ItemPhotoService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ItemPhoto>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public ItemPhotoService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<ItemPhoto> BuildGetAllQuery(AppDbContext context)
        {
            return context.ItemPhotos
                .OrderBy(p => p.ItemId)
                .ThenBy(p => p.SortOrder);
        }

        public override async Task<ServiceResult> ValidateAsync(ItemPhoto entity)
        {
            try
            {
                if (entity.ItemId <= 0)
                    return ServiceResult.Failure("品項ID無效");

                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.PhotoPath))
                    && string.IsNullOrWhiteSpace(entity.PhotoPath))
                    return ServiceResult.Failure("照片路徑不可為空");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<ItemPhoto>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemPhotos
                    .Where(p => p.Caption != null && p.Caption.Contains(searchTerm))
                    .OrderBy(p => p.ItemId)
                    .ThenBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public async Task<List<ItemPhoto>> GetByItemAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ItemPhotos
                    .Where(p => p.ItemId == productId)
                    .OrderBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByItemAsync), GetType(), _logger, new { ItemId = productId });
                throw;
            }
        }
    }
}
