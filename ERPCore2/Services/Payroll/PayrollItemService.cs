using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class PayrollItemService : GenericManagementService<PayrollItem>, IPayrollItemService
    {
        public PayrollItemService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public PayrollItemService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PayrollItem>> logger) : base(contextFactory, logger) { }

        protected override IQueryable<PayrollItem> BuildGetAllQuery(AppDbContext context)
        {
            return context.PayrollItems
                .OrderBy(x => x.ItemType)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Name);
        }

        public override async Task<List<PayrollItem>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.PayrollItems
                    .Where(x => x.Name.ToUpper().Contains(upper) ||
                                (x.Code != null && x.Code.ToUpper().Contains(upper)))
                    .OrderBy(x => x.ItemType)
                    .ThenBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<PayrollItem>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PayrollItem entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("項目名稱不能為空");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("項目代碼不能為空");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("項目代碼已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PayrollItems.Where(x => x.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(x => x.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger);
                return false;
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(PayrollItem entity)
        {
            try
            {
                if (entity.IsSystemItem)
                    return ServiceResult.Failure("系統內建薪資項目不可刪除");

                using var context = await _contextFactory.CreateDbContextAsync();
                bool inUse = await context.PayrollRecordDetails
                    .AnyAsync(d => d.PayrollItemId == entity.Id);

                if (inUse)
                    return ServiceResult.Failure("此薪資項目已有使用紀錄，無法刪除（可改為停用）");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        public async Task<(List<PayrollItem> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<PayrollItem>, IQueryable<PayrollItem>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<PayrollItem> query = context.PayrollItems;

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(x => x.ItemType)
                    .ThenBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<PayrollItem>(), 0);
            }
        }
    }
}
