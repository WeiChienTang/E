using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class BankService : GenericManagementService<Bank>, IBankService
    {
        public BankService(IDbContextFactory<AppDbContext> contextFactory, ILogger<GenericManagementService<Bank>> logger)
            : base(contextFactory, logger)
        {
        }

        public override async Task<List<Bank>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Banks
                    .Where(b => b.BankName.Contains(searchTerm) || 
                               (b.BankNameEn != null && b.BankNameEn.Contains(searchTerm)) ||
                               (b.SwiftCode != null && b.SwiftCode.Contains(searchTerm)))
                    .OrderBy(b => b.BankName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<Bank>();
            }
        }

        public async Task<bool> IsBankCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Banks.Where(b => b.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(b => b.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsBankCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsBankCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<bool> IsBankNameExistsAsync(string bankName, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bankName))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Banks.Where(b => b.BankName == bankName);
                if (excludeId.HasValue)
                    query = query.Where(b => b.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsBankNameExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsBankNameExistsAsync),
                    ServiceType = GetType().Name,
                    BankName = bankName,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<Bank?> GetBySwiftCodeAsync(string swiftCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(swiftCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Banks.Where(b => b.SwiftCode == swiftCode).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySwiftCodeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySwiftCodeAsync),
                    ServiceType = GetType().Name,
                    SwiftCode = swiftCode
                });
                return null;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Bank entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("銀行代碼為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.BankName))
                    errors.Add("銀行名稱為必填欄位");

                if (!string.IsNullOrWhiteSpace(entity.Code) && 
                    await IsBankCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("銀行代碼已存在");

                if (!string.IsNullOrWhiteSpace(entity.BankName) && 
                    await IsBankNameExistsAsync(entity.BankName, entity.Id == 0 ? null : entity.Id))
                    errors.Add("銀行名稱已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.BankName
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
