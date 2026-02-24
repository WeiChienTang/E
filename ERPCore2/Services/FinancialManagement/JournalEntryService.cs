using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class JournalEntryService : GenericManagementService<JournalEntry>, IJournalEntryService
    {
        public JournalEntryService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<JournalEntry>> logger)
            : base(contextFactory, logger)
        {
        }

        public override async Task<List<JournalEntry>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .OrderByDescending(je => je.EntryDate)
                    .ThenByDescending(je => je.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<JournalEntry>();
            }
        }

        public override async Task<List<JournalEntry>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Where(je =>
                        (je.Code != null && je.Code.Contains(searchTerm)) ||
                        (je.Description != null && je.Description.Contains(searchTerm)) ||
                        (je.SourceDocumentCode != null && je.SourceDocumentCode.Contains(searchTerm)))
                    .OrderByDescending(je => je.EntryDate)
                    .ThenByDescending(je => je.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    SearchTerm = searchTerm
                });
                return new List<JournalEntry>();
            }
        }

        public async Task<bool> IsJournalEntryCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.JournalEntries.Where(je => je.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(je => je.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsJournalEntryCodeExistsAsync), GetType(), _logger, new
                {
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<JournalEntry>> GetByFiscalPeriodAsync(int fiscalYear, int fiscalPeriod)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Include(je => je.Lines)
                        .ThenInclude(l => l.AccountItem)
                    .Where(je => je.FiscalYear == fiscalYear && je.FiscalPeriod == fiscalPeriod)
                    .OrderByDescending(je => je.EntryDate)
                    .ThenByDescending(je => je.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByFiscalPeriodAsync), GetType(), _logger, new
                {
                    FiscalYear = fiscalYear,
                    FiscalPeriod = fiscalPeriod
                });
                return new List<JournalEntry>();
            }
        }

        public async Task<JournalEntry?> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Include(je => je.Lines)
                        .ThenInclude(l => l.AccountItem)
                    .FirstOrDefaultAsync(je =>
                        je.SourceDocumentType == sourceDocumentType &&
                        je.SourceDocumentId == sourceDocumentId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySourceDocumentAsync), GetType(), _logger, new
                {
                    SourceDocumentType = sourceDocumentType,
                    SourceDocumentId = sourceDocumentId
                });
                return null;
            }
        }

        public async Task<List<JournalEntry>> GetDraftEntriesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Where(je => je.JournalEntryStatus == JournalEntryStatus.Draft)
                    .OrderByDescending(je => je.EntryDate)
                    .ThenByDescending(je => je.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDraftEntriesAsync), GetType(), _logger);
                return new List<JournalEntry>();
            }
        }

        public async Task<JournalEntry?> GetWithLinesAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Include(je => je.Lines)
                        .ThenInclude(l => l.AccountItem)
                    .FirstOrDefaultAsync(je => je.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithLinesAsync), GetType(), _logger, new
                {
                    Id = id
                });
                return null;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> PostEntryAsync(int id, string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entry = await context.JournalEntries
                    .Include(je => je.Lines)
                    .FirstOrDefaultAsync(je => je.Id == id);

                if (entry == null)
                    return (false, "找不到傳票");

                if (entry.JournalEntryStatus != JournalEntryStatus.Draft)
                    return (false, "只有草稿狀態的傳票才能過帳");

                if (!entry.Lines.Any())
                    return (false, "傳票必須至少有一筆分錄");

                var debitTotal = entry.Lines
                    .Where(l => l.Direction == AccountDirection.Debit)
                    .Sum(l => l.Amount);
                var creditTotal = entry.Lines
                    .Where(l => l.Direction == AccountDirection.Credit)
                    .Sum(l => l.Amount);

                if (debitTotal != creditTotal)
                    return (false, $"借貸不平衡：借方 {debitTotal:N2}，貸方 {creditTotal:N2}，差額 {Math.Abs(debitTotal - creditTotal):N2}");

                if (debitTotal == 0)
                    return (false, "傳票金額不能為零");

                entry.JournalEntryStatus = JournalEntryStatus.Posted;
                entry.UpdatedAt = DateTime.Now;
                entry.UpdatedBy = updatedBy;

                await context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PostEntryAsync), GetType(), _logger, new
                {
                    Id = id,
                    UpdatedBy = updatedBy
                });
                return (false, "過帳時發生錯誤，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage, JournalEntry? ReversalEntry)> ReverseEntryAsync(
            int id, DateTime reversalDate, string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var original = await context.JournalEntries
                    .Include(je => je.Lines)
                    .FirstOrDefaultAsync(je => je.Id == id);

                if (original == null)
                    return (false, "找不到傳票", null);

                if (original.JournalEntryStatus != JournalEntryStatus.Posted)
                    return (false, "只有已過帳的傳票才能沖銷", null);

                if (original.IsReversed)
                    return (false, "此傳票已被沖銷過", null);

                // 建立沖銷傳票（借貸對調）
                var reversalEntry = new JournalEntry
                {
                    EntryDate = reversalDate,
                    EntryType = JournalEntryType.Reversing,
                    JournalEntryStatus = JournalEntryStatus.Posted,
                    Description = $"沖銷傳票 {original.Code}",
                    CompanyId = original.CompanyId,
                    FiscalYear = reversalDate.Year,
                    FiscalPeriod = reversalDate.Month,
                    SourceDocumentType = original.SourceDocumentType,
                    SourceDocumentId = original.SourceDocumentId,
                    SourceDocumentCode = original.SourceDocumentCode,
                    TotalDebitAmount = original.TotalCreditAmount,
                    TotalCreditAmount = original.TotalDebitAmount,
                    CreatedAt = DateTime.Now,
                    CreatedBy = updatedBy,
                    Lines = original.Lines.Select((l, index) => new JournalEntryLine
                    {
                        LineNumber = index + 1,
                        AccountItemId = l.AccountItemId,
                        Direction = l.Direction == AccountDirection.Debit
                            ? AccountDirection.Credit
                            : AccountDirection.Debit,
                        Amount = l.Amount,
                        LineDescription = $"沖銷：{l.LineDescription}",
                        CreatedAt = DateTime.Now,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                context.JournalEntries.Add(reversalEntry);
                await context.SaveChangesAsync();

                // 更新原傳票的沖銷狀態
                original.IsReversed = true;
                original.JournalEntryStatus = JournalEntryStatus.Reversed;
                original.ReversalEntryId = reversalEntry.Id;
                original.UpdatedAt = DateTime.Now;
                original.UpdatedBy = updatedBy;

                await context.SaveChangesAsync();
                return (true, string.Empty, reversalEntry);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReverseEntryAsync), GetType(), _logger, new
                {
                    Id = id,
                    ReversalDate = reversalDate
                });
                return (false, "沖銷時發生錯誤，請稍後再試", null);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SaveWithLinesAsync(
            JournalEntry journalEntry, string savedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 重新計算合計
                var lines = journalEntry.Lines.ToList();
                journalEntry.TotalDebitAmount = lines
                    .Where(l => l.Direction == AccountDirection.Debit)
                    .Sum(l => l.Amount);
                journalEntry.TotalCreditAmount = lines
                    .Where(l => l.Direction == AccountDirection.Credit)
                    .Sum(l => l.Amount);

                if (journalEntry.Id == 0)
                {
                    // 新增
                    journalEntry.CreatedAt = DateTime.Now;
                    journalEntry.CreatedBy = savedBy;
                    journalEntry.JournalEntryStatus = JournalEntryStatus.Draft;

                    // 清除導航屬性，防止 EF Core 將已存在的實體誤判為 Added 並嘗試插入
                    journalEntry.Company = null!;
                    foreach (var line in lines)
                    {
                        line.AccountItem = null!;
                        line.CreatedAt = DateTime.Now;
                        line.CreatedBy = savedBy;
                    }

                    context.JournalEntries.Add(journalEntry);
                }
                else
                {
                    // 更新：先刪除舊分錄，再插入新分錄
                    if (journalEntry.JournalEntryStatus == JournalEntryStatus.Posted)
                        return (false, "已過帳的傳票不可修改，請先沖銷後重新建立");

                    var existing = await context.JournalEntries
                        .Include(je => je.Lines)
                        .FirstOrDefaultAsync(je => je.Id == journalEntry.Id);

                    if (existing == null)
                        return (false, "找不到傳票");

                    // 移除舊分錄
                    context.JournalEntryLines.RemoveRange(existing.Lines);

                    // 更新主檔
                    existing.EntryDate = journalEntry.EntryDate;
                    existing.EntryType = journalEntry.EntryType;
                    existing.Description = journalEntry.Description;
                    existing.CompanyId = journalEntry.CompanyId;
                    existing.FiscalYear = journalEntry.FiscalYear;
                    existing.FiscalPeriod = journalEntry.FiscalPeriod;
                    existing.SourceDocumentType = journalEntry.SourceDocumentType;
                    existing.SourceDocumentId = journalEntry.SourceDocumentId;
                    existing.SourceDocumentCode = journalEntry.SourceDocumentCode;
                    existing.TotalDebitAmount = journalEntry.TotalDebitAmount;
                    existing.TotalCreditAmount = journalEntry.TotalCreditAmount;
                    existing.Remarks = journalEntry.Remarks;
                    existing.UpdatedAt = DateTime.Now;
                    existing.UpdatedBy = savedBy;

                    // 加入新分錄（清除導航屬性，防止 EF Core 誤判為 Added）
                    foreach (var line in lines)
                    {
                        line.Id = 0;
                        line.JournalEntryId = existing.Id;
                        line.AccountItem = null!;
                        line.CreatedAt = DateTime.Now;
                        line.CreatedBy = savedBy;
                        context.JournalEntryLines.Add(line);
                    }
                }

                await context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveWithLinesAsync), GetType(), _logger, new
                {
                    JournalEntryId = journalEntry.Id
                });
                return (false, "儲存傳票時發生錯誤，請稍後再試");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(JournalEntry entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EntryDate == default)
                    errors.Add("傳票日期為必填欄位");

                if (entity.CompanyId <= 0)
                    errors.Add("公司為必填欄位");

                if (entity.FiscalYear < 2000 || entity.FiscalYear > 2100)
                    errors.Add("會計年度無效");

                if (entity.FiscalPeriod < 1 || entity.FiscalPeriod > 12)
                    errors.Add("會計期間必須介於 1 到 12 之間");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsJournalEntryCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("傳票號碼已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
