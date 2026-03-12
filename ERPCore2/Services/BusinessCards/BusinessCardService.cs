using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 名片照片服務實作
    /// </summary>
    public class BusinessCardService : GenericManagementService<BusinessCard>, IBusinessCardService
    {
        public BusinessCardService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<BusinessCard>> logger) : base(contextFactory, logger)
        {
        }

        public BusinessCardService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<BusinessCard> BuildGetAllQuery(AppDbContext context)
        {
            return context.BusinessCards
                .OrderBy(b => b.OwnerType)
                .ThenBy(b => b.OwnerId)
                .ThenBy(b => b.SortOrder);
        }

        public override async Task<List<BusinessCard>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lower = searchTerm.ToLower();
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.BusinessCards
                    .Where(b => (b.ContactPersonName != null && b.ContactPersonName.ToLower().Contains(lower)) ||
                                (b.JobTitle != null && b.JobTitle.ToLower().Contains(lower)))
                    .OrderBy(b => b.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(BusinessCard entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.OwnerType))
                    errors.Add("擁有者類型不可為空");

                if (entity.OwnerId <= 0)
                    errors.Add("擁有者 ID 無效");

                if (string.IsNullOrWhiteSpace(entity.PhotoPath))
                    errors.Add("照片路徑不可為空");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<List<BusinessCard>> GetByOwnerAsync(string ownerType, int ownerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.BusinessCards
                    .Where(b => b.OwnerType == ownerType && b.OwnerId == ownerId)
                    .OrderBy(b => b.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByOwnerAsync), GetType(), _logger, new { OwnerType = ownerType, OwnerId = ownerId });
                throw;
            }
        }
    }
}
