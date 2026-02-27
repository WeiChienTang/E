using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class AccountItemService : GenericManagementService<AccountItem>, IAccountItemService
    {
        public AccountItemService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<AccountItem>> logger)
            : base(contextFactory, logger)
        {
        }

        public override async Task<List<AccountItem>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .OrderBy(a => a.AccountLevel)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<AccountItem>();
            }
        }

        public override async Task<List<AccountItem>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .Where(a => (a.Code != null && a.Code.Contains(searchTerm)) ||
                               a.Name.Contains(searchTerm) ||
                               (a.EnglishName != null && a.EnglishName.Contains(searchTerm)))
                    .OrderBy(a => a.AccountLevel)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    SearchTerm = searchTerm
                });
                return new List<AccountItem>();
            }
        }

        public async Task<bool> IsAccountItemCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.AccountItems.Where(a => a.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(a => a.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsAccountItemCodeExistsAsync), GetType(), _logger, new
                {
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<AccountItem?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .FirstOrDefaultAsync(a => a.Code == code && a.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new
                {
                    Code = code
                });
                return null;
            }
        }

        public async Task<List<AccountItem>> GetByAccountTypeAsync(AccountType accountType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .Where(a => a.AccountType == accountType)
                    .OrderBy(a => a.AccountLevel)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByAccountTypeAsync), GetType(), _logger, new
                {
                    AccountType = accountType
                });
                return new List<AccountItem>();
            }
        }

        public async Task<List<AccountItem>> GetByLevelAsync(int level)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .Where(a => a.AccountLevel == level)
                    .OrderBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByLevelAsync), GetType(), _logger, new
                {
                    Level = level
                });
                return new List<AccountItem>();
            }
        }

        public async Task<List<AccountItem>> GetDetailAccountsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .Where(a => a.IsDetailAccount)
                    .OrderBy(a => a.AccountType)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailAccountsAsync), GetType(), _logger);
                return new List<AccountItem>();
            }
        }

        public async Task<List<AccountItem>> GetAllWithParentAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountItems
                    .Include(a => a.Parent)
                    .Include(a => a.LinkedCustomer)
                    .Include(a => a.LinkedSupplier)
                    .Include(a => a.LinkedProduct)
                    .OrderBy(a => a.AccountLevel)
                    .ThenBy(a => a.SortOrder)
                    .ThenBy(a => a.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllWithParentAsync), GetType(), _logger);
                return new List<AccountItem>();
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(AccountItem entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                bool hasJournalLines = await context.JournalEntryLines
                    .AnyAsync(l => l.AccountItemId == entity.Id);

                if (hasJournalLines)
                    return ServiceResult.Failure("無法刪除此科目，因為已有傳票明細正在使用");

                bool hasChildren = await context.AccountItems
                    .AnyAsync(a => a.ParentId == entity.Id);

                if (hasChildren)
                    return ServiceResult.Failure("無法刪除此科目，因為有子科目存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountItem entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("科目代碼為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("科目名稱為必填欄位");

                if (entity.AccountLevel < 1 || entity.AccountLevel > 4)
                    errors.Add("科目層級必須介於 1 至 4 之間");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsAccountItemCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("科目代碼已存在");

                // 更新時：若已有已過帳傳票，鎖定關鍵欄位（科目大類、借貸方向、上層科目）
                if (entity.Id > 0)
                {
                    using var ctx = await _contextFactory.CreateDbContextAsync();
                    var original = await ctx.AccountItems.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == entity.Id);

                    if (original != null)
                    {
                        bool hasPostedLines = await ctx.JournalEntryLines
                            .AnyAsync(l => l.AccountItemId == entity.Id &&
                                           l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

                        if (hasPostedLines)
                        {
                            if (entity.AccountType != original.AccountType)
                                errors.Add("科目大類不可更改（已有已過帳傳票）");
                            if (entity.Direction != original.Direction)
                                errors.Add("借貸方向不可更改（已有已過帳傳票）");
                            if (entity.ParentId != original.ParentId)
                                errors.Add("上層科目不可更改（已有已過帳傳票）");
                        }
                    }
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    EntityId = entity.Id,
                    EntityCode = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
