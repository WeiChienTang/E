using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶拜訪紀錄服務實作
    /// </summary>
    public class CustomerVisitService : GenericManagementService<CustomerVisit>, ICustomerVisitService
    {
        public CustomerVisitService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CustomerVisit>> logger) : base(contextFactory, logger)
        {
        }

        public CustomerVisitService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<CustomerVisit>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerVisits
                    .Include(v => v.Customer)
                    .Include(v => v.Employee)
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<CustomerVisit?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerVisits
                    .Include(v => v.Customer)
                    .Include(v => v.Employee)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<CustomerVisit>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerVisits
                    .Include(v => v.Customer)
                    .Include(v => v.Employee)
                    .Where(v => (v.Purpose != null && v.Purpose.ToLower().Contains(lowerSearchTerm)) ||
                                (v.Content != null && v.Content.ToLower().Contains(lowerSearchTerm)) ||
                                (v.Result != null && v.Result.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerVisit entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.CustomerId <= 0)
                    errors.Add("請選擇客戶");

                if (entity.VisitDate == default)
                    errors.Add("請輸入拜訪日期");

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

        public async Task<List<CustomerVisit>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerVisits
                    .Include(v => v.Employee)
                    .Where(v => v.CustomerId == customerId)
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerAsync), GetType(), _logger, new { CustomerId = customerId });
                throw;
            }
        }
    }
}
