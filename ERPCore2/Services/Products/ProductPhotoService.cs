using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品照片服務實作
    /// </summary>
    public class ProductPhotoService : GenericManagementService<ProductPhoto>, IProductPhotoService
    {
        public ProductPhotoService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductPhoto>> logger) : base(contextFactory, logger)
        {
        }

        public ProductPhotoService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<ProductPhoto> BuildGetAllQuery(AppDbContext context)
        {
            return context.ProductPhotos
                .OrderBy(p => p.ProductId)
                .ThenBy(p => p.SortOrder);
        }

        public override async Task<ServiceResult> ValidateAsync(ProductPhoto entity)
        {
            try
            {
                if (entity.ProductId <= 0)
                    return ServiceResult.Failure("商品ID無效");

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

        public override async Task<List<ProductPhoto>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPhotos
                    .Where(p => p.Caption != null && p.Caption.Contains(searchTerm))
                    .OrderBy(p => p.ProductId)
                    .ThenBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public async Task<List<ProductPhoto>> GetByProductAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductPhotos
                    .Where(p => p.ProductId == productId)
                    .OrderBy(p => p.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductAsync), GetType(), _logger, new { ProductId = productId });
                throw;
            }
        }
    }
}
