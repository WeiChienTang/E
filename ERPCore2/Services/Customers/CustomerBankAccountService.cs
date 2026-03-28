using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Customers;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Customers
{
    public class CustomerBankAccountService : GenericManagementService<CustomerBankAccount>, ICustomerBankAccountService
    {
        public CustomerBankAccountService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public CustomerBankAccountService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CustomerBankAccount>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        protected override IQueryable<CustomerBankAccount> BuildGetAllQuery(AppDbContext context)
        {
            return context.CustomerBankAccounts
                .Include(x => x.Customer)
                .Include(x => x.Bank)
                .OrderBy(x => x.Customer.Code)
                .ThenByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Bank.BankName);
        }

        public override async Task<List<CustomerBankAccount>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.CustomerBankAccounts
                    .Include(x => x.Customer)
                    .Include(x => x.Bank)
                    .Where(x => x.AccountNumber.ToUpper().Contains(upper) ||
                                x.AccountName.ToUpper().Contains(upper) ||
                                x.Bank.BankName.ToUpper().Contains(upper) ||
                                (x.Customer.Code != null && x.Customer.Code.ToUpper().Contains(upper)))
                    .OrderBy(x => x.Customer.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<CustomerBankAccount>();
            }
        }

        public override async Task<CustomerBankAccount?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerBankAccounts
                    .Include(x => x.Customer)
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<CustomerBankAccount>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerBankAccounts
                    .Include(x => x.Bank)
                    .Where(x => x.CustomerId == customerId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Bank.BankName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger);
                return new List<CustomerBankAccount>();
            }
        }

        public async Task<CustomerBankAccount?> GetPrimaryAccountAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomerBankAccounts
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.IsPrimary);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryAccountAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var allAccounts = await context.CustomerBankAccounts
                    .Where(x => x.CustomerId == customerId)
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

        public override async Task<ServiceResult> ValidateAsync(CustomerBankAccount entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.CustomerId <= 0)
                    errors.Add("請選擇客戶");

                if (entity.BankId <= 0)
                    errors.Add("請選擇銀行");

                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.AccountNumber))
                    && string.IsNullOrWhiteSpace(entity.AccountNumber))
                    errors.Add("帳號不能為空");

                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.AccountName))
                    && string.IsNullOrWhiteSpace(entity.AccountName))
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

        protected override async Task<ServiceResult> CanDeleteAsync(CustomerBankAccount entity)
        {
            try
            {
                if (entity.IsPrimary)
                    return ServiceResult.Failure("主要帳戶不可直接刪除，請先設定其他帳戶為主要帳戶");

                return await Task.FromResult(ServiceResult.Success());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }
    }
}
