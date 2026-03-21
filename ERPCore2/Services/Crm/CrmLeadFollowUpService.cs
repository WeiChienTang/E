using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Crm
{
    /// <summary>
    /// 潛在客戶跟進紀錄服務實作
    /// </summary>
    public class CrmLeadFollowUpService : GenericManagementService<CrmLeadFollowUp>, ICrmLeadFollowUpService
    {
        public CrmLeadFollowUpService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CrmLeadFollowUp>> logger) : base(contextFactory, logger)
        {
        }

        protected override IQueryable<CrmLeadFollowUp> BuildGetAllQuery(AppDbContext context)
        {
            return context.CrmLeadFollowUps
                .Include(f => f.CrmLead)
                .Include(f => f.Employee)
                .OrderByDescending(f => f.FollowUpDate);
        }

        public override async Task<CrmLeadFollowUp?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeadFollowUps
                    .Include(f => f.CrmLead)
                    .Include(f => f.Employee)
                    .FirstOrDefaultAsync(f => f.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CrmLeadFollowUp entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.CrmLeadId <= 0)
                    errors.Add("請選擇潛在客戶");

                if (entity.FollowUpDate == default)
                    errors.Add("請輸入跟進日期");

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

        public override async Task<List<CrmLeadFollowUp>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();
                var lower = searchTerm.ToLower();
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeadFollowUps
                    .Include(f => f.CrmLead)
                    .Include(f => f.Employee)
                    .Where(f => (f.Content != null && f.Content.ToLower().Contains(lower)) ||
                                (f.Conclusion != null && f.Conclusion.ToLower().Contains(lower)))
                    .OrderByDescending(f => f.FollowUpDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public async Task<List<CrmLeadFollowUp>> GetByLeadAsync(int leadId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeadFollowUps
                    .Include(f => f.Employee)
                    .Where(f => f.CrmLeadId == leadId)
                    .OrderByDescending(f => f.FollowUpDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByLeadAsync), GetType(), _logger, new { LeadId = leadId });
                throw;
            }
        }

        public async Task<(List<CrmLeadFollowUp> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CrmLeadFollowUp>, IQueryable<CrmLeadFollowUp>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<CrmLeadFollowUp> query = context.CrmLeadFollowUps
                    .Include(f => f.CrmLead);

                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(f => f.FollowUpDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<CrmLeadFollowUp>(), 0);
            }
        }
    }
}
