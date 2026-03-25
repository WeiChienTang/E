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
                .Include(x => x.Bank)
                .OrderBy(x => x.Employee.Name)
                .ThenByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Bank.BankName);
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
                    .Include(x => x.Bank)
                    .Where(x => x.Employee.Name!.ToUpper().Contains(upper) ||
                                x.Bank.BankName.ToUpper().Contains(upper) ||
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
                    .Include(x => x.Bank)
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
                    .Include(x => x.Bank)
                    .Where(x => x.EmployeeId == employeeId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Bank.BankName)
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
                    .Include(x => x.Bank)
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

                var allAccounts = await context.EmployeeBankAccounts
                    .Where(x => x.EmployeeId == employeeId)
                    .ToListAsync();

                foreach (var acc in allAccounts)
                {
                    acc.IsPrimary = acc.Id == bankAccountId;
                    acc.UpdatedAt = DateTime.UtcNow;
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

                if (entity.BankId <= 0)
                    errors.Add("請選擇銀行");

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
                {
                    // 若該員工只剩這一個帳戶，允許刪除（刪除後無主要帳戶）
                    using var context = await _contextFactory.CreateDbContextAsync();
                    int accountCount = await context.EmployeeBankAccounts
                        .CountAsync(x => x.EmployeeId == entity.EmployeeId);

                    if (accountCount > 1)
                        return ServiceResult.Failure("主要轉帳帳戶不可直接刪除，請先設定其他帳戶為主要帳戶");
                }

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
