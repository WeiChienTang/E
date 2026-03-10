using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class EmployeeBankAccountService : GenericManagementService<EmployeeBankAccount>, IEmployeeBankAccountService
    {
        public EmployeeBankAccountService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public EmployeeBankAccountService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeeBankAccount>> logger) : base(contextFactory, logger) { }

        protected override IQueryable<EmployeeBankAccount> BuildGetAllQuery(AppDbContext context)
        {
            return context.EmployeeBankAccounts
                .Include(x => x.Employee)
                .OrderBy(x => x.Employee.Name)
                .ThenByDescending(x => x.IsPrimary)
                .ThenBy(x => x.BankCode);
        }

        public override async Task<List<EmployeeBankAccount>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.EmployeeBankAccounts
                    .Include(x => x.Employee)
                    .Where(x => x.Employee.Name!.ToUpper().Contains(upper) ||
                                x.BankCode.ToUpper().Contains(upper) ||
                                (x.BankName != null && x.BankName.ToUpper().Contains(upper)) ||
                                x.AccountNumber.ToUpper().Contains(upper) ||
                                x.AccountName.ToUpper().Contains(upper))
                    .OrderBy(x => x.Employee.Name)
                    .ThenByDescending(x => x.IsPrimary)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<EmployeeBankAccount>();
            }
        }

        public override async Task<EmployeeBankAccount?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeBankAccounts
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<EmployeeBankAccount>> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeBankAccounts
                    .Include(x => x.Employee)
                    .Where(x => x.EmployeeId == employeeId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.BankCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger);
                return new List<EmployeeBankAccount>();
            }
        }

        public async Task<EmployeeBankAccount?> GetPrimaryAccountAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeBankAccounts
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsPrimary);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryAccountAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取消同員工所有帳戶的主要標記
                var allAccounts = await context.EmployeeBankAccounts
                    .Where(x => x.EmployeeId == employeeId)
                    .ToListAsync();

                foreach (var acc in allAccounts)
                {
                    acc.IsPrimary = acc.Id == bankAccountId;
                    acc.UpdatedAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetPrimaryAsync), GetType(), _logger);
                return ServiceResult.Failure("設定主要帳戶時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeBankAccount entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EmployeeId <= 0)
                    errors.Add("請選擇員工");

                if (string.IsNullOrWhiteSpace(entity.BankCode))
                    errors.Add("銀行代號不能為空");

                if (string.IsNullOrWhiteSpace(entity.AccountNumber))
                    errors.Add("帳號不能為空");

                if (string.IsNullOrWhiteSpace(entity.AccountName))
                    errors.Add("戶名不能為空");

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

        protected override async Task<ServiceResult> CanDeleteAsync(EmployeeBankAccount entity)
        {
            try
            {
                if (entity.IsPrimary)
                    return ServiceResult.Failure("主要轉帳帳戶不可直接刪除，請先設定其他帳戶為主要帳戶");

                return await Task.FromResult(ServiceResult.Success());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        public async Task<(List<EmployeeBankAccount> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EmployeeBankAccount>, IQueryable<EmployeeBankAccount>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<EmployeeBankAccount> query = context.EmployeeBankAccounts
                    .Include(x => x.Employee);

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(x => x.Employee!.Name)
                    .ThenByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.BankCode)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<EmployeeBankAccount>(), 0);
            }
        }
    }
}
