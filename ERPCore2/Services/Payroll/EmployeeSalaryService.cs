using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class EmployeeSalaryService : GenericManagementService<EmployeeSalary>, IEmployeeSalaryService
    {
        public EmployeeSalaryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public EmployeeSalaryService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeeSalary>> logger) : base(contextFactory, logger) { }

        protected override IQueryable<EmployeeSalary> BuildGetAllQuery(AppDbContext context)
        {
            return context.EmployeeSalaries
                .Include(x => x.Employee)
                .Include(x => x.AllowanceItems).ThenInclude(i => i.PayrollItem)
                .OrderBy(x => x.Employee.Name)
                .ThenByDescending(x => x.EffectiveDate);
        }

        public override async Task<List<EmployeeSalary>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.EmployeeSalaries
                    .Include(x => x.Employee)
                    .Where(x => x.Employee!.Name!.ToUpper().Contains(upper) ||
                                (x.Employee.Code != null && x.Employee.Code.ToUpper().Contains(upper)))
                    .OrderBy(x => x.Employee.Name)
                    .ThenByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<EmployeeSalary>();
            }
        }

        public override async Task<EmployeeSalary?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeSalaries
                    .Include(x => x.Employee)
                    .Include(x => x.AllowanceItems).ThenInclude(i => i.PayrollItem)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<EmployeeSalary>> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeSalaries
                    .Include(x => x.Employee)
                    .Where(x => x.EmployeeId == employeeId)
                    .OrderByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger);
                return new List<EmployeeSalary>();
            }
        }

        public async Task<EmployeeSalary?> GetCurrentSalaryAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeSalaries
                    .Include(x => x.Employee)
                    .Include(x => x.AllowanceItems).ThenInclude(i => i.PayrollItem)
                    .Where(x => x.EmployeeId == employeeId && x.ExpiryDate == null)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCurrentSalaryAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult> AddWithExpiryAsync(EmployeeSalary newSalary)
        {
            try
            {
                // 驗證新薪資設定
                var validation = await ValidateAsync(newSalary);
                if (!validation.IsSuccess)
                    return validation;

                using var context = await _contextFactory.CreateDbContextAsync();

                // 將同一員工目前有效的薪資設定自動設置失效日期（新生效日前一天）
                var current = await context.EmployeeSalaries
                    .Where(x => x.EmployeeId == newSalary.EmployeeId && x.ExpiryDate == null)
                    .ToListAsync();

                // 新薪資生效日必須晚於現有所有有效薪資的生效日，否則會產生 ExpiryDate < EffectiveDate 的無效記錄
                foreach (var existing in current)
                {
                    if (newSalary.EffectiveDate <= existing.EffectiveDate)
                        return ServiceResult.Failure($"新薪資生效日期（{newSalary.EffectiveDate:yyyy-MM-dd}）必須晚於目前有效薪資的生效日期（{existing.EffectiveDate:yyyy-MM-dd}）");
                }

                foreach (var existing in current)
                {
                    existing.ExpiryDate = newSalary.EffectiveDate.AddDays(-1);
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                newSalary.CreatedAt = DateTime.UtcNow;
                // AllowanceItems 由呼叫端設定好後一併 cascade 儲存
                foreach (var item in newSalary.AllowanceItems)
                    item.EmployeeSalaryId = 0; // 確保 EF 視為新增
                context.EmployeeSalaries.Add(newSalary);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddWithExpiryAsync), GetType(), _logger);
                return ServiceResult.Failure("新增薪資設定時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeSalary entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EmployeeId <= 0)
                    errors.Add("請選擇員工");

                if (entity.BaseSalary <= 0)
                    errors.Add("本薪必須大於 0");

                if (entity.EffectiveDate == default)
                    errors.Add("請設定生效日期");

                if (entity.LaborInsuredSalary < 0)
                    errors.Add("勞保投保薪資不能為負數");

                if (entity.HealthInsuredAmount < 0)
                    errors.Add("健保投保金額不能為負數");

                if (entity.VoluntaryRetirementRate < 0 || entity.VoluntaryRetirementRate > 6)
                    errors.Add("自願提繳率必須在 0%～6% 之間");

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

        public async Task<bool> IsEmployeeSalaryCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeSalaries
                    .AnyAsync(x => x.Code == code && (!excludeId.HasValue || x.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEmployeeSalaryCodeExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<ServiceResult> UpdateWithAllowanceItemsAsync(EmployeeSalary entity, List<EmployeeSalaryItem> allowanceItems)
        {
            try
            {
                var validation = await ValidateAsync(entity);
                if (!validation.IsSuccess)
                    return validation;

                using var context = await _contextFactory.CreateDbContextAsync();

                // 先刪除舊的津貼項目
                var oldItems = await context.EmployeeSalaryItems
                    .Where(i => i.EmployeeSalaryId == entity.Id)
                    .ToListAsync();
                context.EmployeeSalaryItems.RemoveRange(oldItems);

                // 更新主體
                entity.UpdatedAt = DateTime.UtcNow;
                context.EmployeeSalaries.Update(entity);

                // 新增新的津貼項目
                foreach (var item in allowanceItems)
                {
                    item.Id = 0;
                    item.EmployeeSalaryId = entity.Id;
                    context.EmployeeSalaryItems.Add(item);
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateWithAllowanceItemsAsync), GetType(), _logger);
                return ServiceResult.Failure("更新薪資設定時發生錯誤");
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(EmployeeSalary entity)
        {
            try
            {
                // 若為目前有效設定（ExpiryDate = null），不允許直接刪除（應以調薪方式取代）
                if (!entity.ExpiryDate.HasValue)
                    return ServiceResult.Failure("目前有效的薪資設定不可直接刪除，請以新增調薪記錄方式取代");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #region 伺服器端分頁

        public async Task<(List<EmployeeSalary> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EmployeeSalary>, IQueryable<EmployeeSalary>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<EmployeeSalary> query = context.EmployeeSalaries
                    .Include(s => s.Employee)
                    .Include(s => s.AllowanceItems).ThenInclude(i => i.PayrollItem);
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(s => s.EffectiveDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<EmployeeSalary>(), 0);
            }
        }

        #endregion
    }
}
