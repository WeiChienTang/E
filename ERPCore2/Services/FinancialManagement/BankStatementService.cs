using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class BankStatementService : GenericManagementService<BankStatement>, IBankStatementService
    {
        public BankStatementService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<BankStatement>> logger)
            : base(contextFactory, logger)
        {
        }

        protected override IQueryable<BankStatement> BuildGetAllQuery(AppDbContext context)
        {
            return context.BankStatements
                .Include(bs => bs.Company)
                .Include(bs => bs.CompanyBankAccount)
                    .ThenInclude(cba => cba.Bank)
                .Include(bs => bs.Lines)
                .OrderByDescending(bs => bs.StatementDate)
                .ThenByDescending(bs => bs.Id);
        }

        public override async Task<List<BankStatement>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.BankStatements
                    .Include(bs => bs.Company)
                    .Include(bs => bs.CompanyBankAccount)
                        .ThenInclude(cba => cba.Bank)
                    .Include(bs => bs.Lines)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(bs =>
                        (bs.Remarks != null && bs.Remarks.Contains(searchTerm)) ||
                        bs.CompanyBankAccount.AccountNumber.Contains(searchTerm) ||
                        bs.CompanyBankAccount.AccountName.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(bs => bs.StatementDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<BankStatement>();
            }
        }

        public async Task<(List<BankStatement> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<BankStatement>, IQueryable<BankStatement>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<BankStatement> query = context.BankStatements
                    .Include(bs => bs.Company)
                    .Include(bs => bs.CompanyBankAccount)
                        .ThenInclude(cba => cba.Bank)
                    .Include(bs => bs.Lines);

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(bs => bs.StatementDate)
                    .ThenByDescending(bs => bs.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<BankStatement>(), 0);
            }
        }

        public async Task<BankStatement?> GetWithLinesAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.BankStatements
                    .Include(bs => bs.Company)
                    .Include(bs => bs.CompanyBankAccount)
                        .ThenInclude(cba => cba.Bank)
                    .Include(bs => bs.Lines.OrderBy(l => l.SortOrder).ThenBy(l => l.TransactionDate))
                    .FirstOrDefaultAsync(bs => bs.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithLinesAsync), GetType(), _logger,
                    new { Id = id });
                return null;
            }
        }

        public override Task<ServiceResult> ValidateAsync(BankStatement entity)
        {
            var errors = new List<string>();
            if (entity.CompanyId <= 0)
                errors.Add("請選擇公司");
            if (entity.CompanyBankAccountId <= 0)
                errors.Add("請選擇銀行帳號");
            if (entity.PeriodEnd < entity.PeriodStart)
                errors.Add("對帳期間結束日不可早於起始日");
            if (errors.Any())
                return Task.FromResult(ServiceResult.Failure(string.Join("; ", errors)));
            return Task.FromResult(ServiceResult.Success());
        }

        public async Task<ServiceResult> SaveWithLinesAsync(BankStatement statement, List<BankStatementLine> lines, string currentUser)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (statement.Id == 0)
                {
                    // 新增
                    statement.CreatedAt = DateTime.UtcNow;
                    statement.CreatedBy = currentUser;
                    statement.UpdatedAt = DateTime.UtcNow;
                    statement.UpdatedBy = currentUser;
                    context.BankStatements.Add(statement);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // 更新主檔
                    var existing = await context.BankStatements.FindAsync(statement.Id);
                    if (existing == null)
                        return ServiceResult.Failure("找不到對帳單");

                    existing.CompanyId = statement.CompanyId;
                    existing.CompanyBankAccountId = statement.CompanyBankAccountId;
                    existing.StatementDate = statement.StatementDate;
                    existing.PeriodStart = statement.PeriodStart;
                    existing.PeriodEnd = statement.PeriodEnd;
                    existing.OpeningBalance = statement.OpeningBalance;
                    existing.ClosingBalance = statement.ClosingBalance;
                    existing.Remarks = statement.Remarks;
                    existing.Status = statement.Status;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = currentUser;
                    await context.SaveChangesAsync();
                }

                // Upsert 明細行
                var existingLines = await context.BankStatementLines
                    .Where(l => l.BankStatementId == statement.Id)
                    .ToListAsync();

                var incomingIds = lines.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
                var toDelete = existingLines.Where(l => !incomingIds.Contains(l.Id)).ToList();
                context.BankStatementLines.RemoveRange(toDelete);

                foreach (var line in lines)
                {
                    line.BankStatementId = statement.Id;
                    if (line.Id == 0)
                    {
                        line.CreatedAt = DateTime.UtcNow;
                        line.CreatedBy = currentUser;
                        line.UpdatedAt = DateTime.UtcNow;
                        line.UpdatedBy = currentUser;
                        context.BankStatementLines.Add(line);
                    }
                    else
                    {
                        var existingLine = existingLines.FirstOrDefault(l => l.Id == line.Id);
                        if (existingLine != null)
                        {
                            existingLine.TransactionDate = line.TransactionDate;
                            existingLine.Description = line.Description;
                            existingLine.DebitAmount = line.DebitAmount;
                            existingLine.CreditAmount = line.CreditAmount;
                            existingLine.IsMatched = line.IsMatched;
                            existingLine.MatchedJournalEntryLineId = line.MatchedJournalEntryLineId;
                            existingLine.SortOrder = line.SortOrder;
                            existingLine.UpdatedAt = DateTime.UtcNow;
                            existingLine.UpdatedBy = currentUser;
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveWithLinesAsync), GetType(), _logger,
                    new { StatementId = statement.Id });
                return ServiceResult.Failure("儲存對帳單時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleLineMatchAsync(int lineId, int? journalEntryLineId, string currentUser)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var line = await context.BankStatementLines.FindAsync(lineId);
                if (line == null)
                    return ServiceResult.Failure("找不到對帳明細行");

                line.IsMatched = journalEntryLineId.HasValue;
                line.MatchedJournalEntryLineId = journalEntryLineId;
                line.UpdatedAt = DateTime.UtcNow;
                line.UpdatedBy = currentUser;
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ToggleLineMatchAsync), GetType(), _logger,
                    new { LineId = lineId });
                return ServiceResult.Failure("更新配對狀態時發生錯誤");
            }
        }

        public async Task<List<BankStatement>> GetByBankAccountAsync(int companyBankAccountId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.BankStatements
                    .Where(bs => bs.CompanyBankAccountId == companyBankAccountId)
                    .Include(bs => bs.Lines)
                    .OrderByDescending(bs => bs.StatementDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBankAccountAsync), GetType(), _logger,
                    new { CompanyBankAccountId = companyBankAccountId });
                return new List<BankStatement>();
            }
        }
    }
}
