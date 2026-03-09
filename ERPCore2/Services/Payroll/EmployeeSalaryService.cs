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

        public override async Task<List<EmployeeSalary>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeSalaries
                    .Include(x => x.Employee)
                    .OrderBy(x => x.Employee.Name)
                    .ThenByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<EmployeeSalary>();
            }
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
                    .Where(x => x.Employee.Name.ToUpper().Contains(upper) ||
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

                foreach (var existing in current)
                {
                    existing.ExpiryDate = newSalary.EffectiveDate.AddDays(-1);
                    existing.UpdatedAt = DateTime.Now;
                }

                newSalary.CreatedAt = DateTime.Now;
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
    }
}
