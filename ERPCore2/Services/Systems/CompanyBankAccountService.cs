using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Systems;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Systems
{
    public class CompanyBankAccountService : GenericManagementService<CompanyBankAccount>, ICompanyBankAccountService
    {
        public CompanyBankAccountService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public CompanyBankAccountService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CompanyBankAccount>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        protected override IQueryable<CompanyBankAccount> BuildGetAllQuery(AppDbContext context)
        {
            return context.CompanyBankAccounts
                .Include(x => x.Company)
                .Include(x => x.Bank)
                .OrderBy(x => x.Company.CompanyName)
                .ThenByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Bank.BankName);
        }

        public override async Task<List<CompanyBankAccount>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.CompanyBankAccounts
                    .Include(x => x.Company)
                    .Include(x => x.Bank)
                    .Where(x => x.AccountNumber.ToUpper().Contains(upper) ||
                                x.AccountName.ToUpper().Contains(upper) ||
                                x.Bank.BankName.ToUpper().Contains(upper) ||
                                x.Company.CompanyName.ToUpper().Contains(upper))
                    .OrderBy(x => x.Company.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<CompanyBankAccount>();
            }
        }

        public override async Task<CompanyBankAccount?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CompanyBankAccounts
                    .Include(x => x.Company)
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<CompanyBankAccount>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CompanyBankAccounts
                    .Include(x => x.Bank)
                    .Where(x => x.CompanyId == companyId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Bank.BankName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCompanyIdAsync), GetType(), _logger);
                return new List<CompanyBankAccount>();
            }
        }

        public async Task<CompanyBankAccount?> GetPrimaryAccountAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CompanyBankAccounts
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.IsPrimary);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryAccountAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var allAccounts = await context.CompanyBankAccounts
                    .Where(x => x.CompanyId == companyId)
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

        public override async Task<ServiceResult> ValidateAsync(CompanyBankAccount entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.CompanyId <= 0)
                    errors.Add("請選擇公司");

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
    }
}
