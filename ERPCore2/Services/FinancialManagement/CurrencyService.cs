using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class CurrencyService : GenericManagementService<Currency>, ICurrencyService
    {
        public CurrencyService(IDbContextFactory<AppDbContext> contextFactory, ILogger<GenericManagementService<Currency>> logger)
            : base(contextFactory, logger)
        {
        }

        public override async Task<List<Currency>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Currencies
                    .Where(c => c.Code!.Contains(searchTerm) || c.Name.Contains(searchTerm))
                    .OrderBy(c => c.Code)
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
                return new List<Currency>();
            }
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Currencies.Where(c => c.Code == code).FirstOrDefaultAsync();
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
                var query = context.Currencies.Where(c => c.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(c => c.Id != excludeId.Value);

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

        public override async Task<ServiceResult> ValidateAsync(Currency entity)
        {
            try
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("貨幣代碼為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("貨幣名稱為必填欄位");

                if (!string.IsNullOrWhiteSpace(entity.Code) && await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("貨幣代碼已存在");

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
    }
}
