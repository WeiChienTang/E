using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Suppliers;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Suppliers
{
    public class SupplierBankAccountService : GenericManagementService<SupplierBankAccount>, ISupplierBankAccountService
    {
        public SupplierBankAccountService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        public SupplierBankAccountService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SupplierBankAccount>> logger) : base(contextFactory, logger) { }

        protected override IQueryable<SupplierBankAccount> BuildGetAllQuery(AppDbContext context)
        {
            return context.SupplierBankAccounts
                .Include(x => x.Supplier)
                .Include(x => x.Bank)
                .OrderBy(x => x.Supplier.Code)
                .ThenByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Bank.BankName);
        }

        public override async Task<List<SupplierBankAccount>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var upper = searchTerm.Trim().ToUpper();

                return await context.SupplierBankAccounts
                    .Include(x => x.Supplier)
                    .Include(x => x.Bank)
                    .Where(x => x.AccountNumber.ToUpper().Contains(upper) ||
                                x.AccountName.ToUpper().Contains(upper) ||
                                x.Bank.BankName.ToUpper().Contains(upper) ||
                                (x.Supplier.Code != null && x.Supplier.Code.ToUpper().Contains(upper)))
                    .OrderBy(x => x.Supplier.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<SupplierBankAccount>();
            }
        }

        public override async Task<SupplierBankAccount?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierBankAccounts
                    .Include(x => x.Supplier)
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<SupplierBankAccount>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierBankAccounts
                    .Include(x => x.Bank)
                    .Where(x => x.SupplierId == supplierId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Bank.BankName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger);
                return new List<SupplierBankAccount>();
            }
        }

        public async Task<SupplierBankAccount?> GetPrimaryAccountAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierBankAccounts
                    .Include(x => x.Bank)
                    .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.IsPrimary);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryAccountAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var allAccounts = await context.SupplierBankAccounts
                    .Where(x => x.SupplierId == supplierId)
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

        public override async Task<ServiceResult> ValidateAsync(SupplierBankAccount entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.SupplierId <= 0)
                    errors.Add("請選擇廠商");

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

        protected override async Task<ServiceResult> CanDeleteAsync(SupplierBankAccount entity)
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
