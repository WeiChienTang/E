using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶投訴服務實作
    /// </summary>
    public class CustomerComplaintService : GenericManagementService<CustomerComplaint>, ICustomerComplaintService
    {
        public CustomerComplaintService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CustomerComplaint>> logger) : base(contextFactory, logger)
        {
        }

        public CustomerComplaintService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<CustomerComplaint> BuildGetAllQuery(AppDbContext context)
        {
            return context.CustomerComplaints
                .Include(c => c.Customer)
                .Include(c => c.Employee)
                .OrderByDescending(c => c.ComplaintDate);
        }

        public override async Task<CustomerComplaint?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerComplaints
                    .Include(c => c.Customer)
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<CustomerComplaint>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerComplaints
                    .Include(c => c.Customer)
                    .Include(c => c.Employee)
                    .Where(c => c.Title.ToLower().Contains(lowerSearchTerm) ||
                                (c.Description != null && c.Description.ToLower().Contains(lowerSearchTerm)) ||
                                (c.Customer != null && c.Customer.CompanyName != null && c.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(c => c.ComplaintDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerComplaint entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.CustomerId <= 0)
                    errors.Add("請選擇客戶");

                if (string.IsNullOrWhiteSpace(entity.Title))
                    errors.Add("請輸入投訴標題");

                if (entity.ComplaintDate == default)
                    errors.Add("請輸入投訴日期");

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

        public async Task<List<CustomerComplaint>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerComplaints
                    .Include(c => c.Employee)
                    .Where(c => c.CustomerId == customerId)
                    .OrderByDescending(c => c.ComplaintDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerAsync), GetType(), _logger, new { CustomerId = customerId });
                throw;
            }
        }

        public async Task<(List<CustomerComplaint> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CustomerComplaint>, IQueryable<CustomerComplaint>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<CustomerComplaint> query = context.CustomerComplaints
                    .Include(c => c.Customer)
                    .Include(c => c.Employee);
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(c => c.ComplaintDate)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<CustomerComplaint>(), 0);
            }
        }
    }
}
