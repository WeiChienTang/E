using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Helpers.EditModal;
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

        protected override IQueryable<JournalEntry> BuildGetAllQuery(AppDbContext context)
        {
            return context.JournalEntries
                .Include(je => je.Company)
                .OrderByDescending(je => je.EntryDate)
                .ThenByDescending(je => je.Id);
        }

        protected override async Task<ServiceResult> CanDeleteAsync(JournalEntry entry)
        {
            // 已過帳的傳票不可直接刪除，必須走沖銷流程
            if (entry.JournalEntryStatus == JournalEntryStatus.Posted)
                return ServiceResult.Failure("已過帳的傳票不可刪除，請使用沖銷功能");

            // 已沖銷的傳票保留為歷史記錄，不允許刪除
            if (entry.JournalEntryStatus == JournalEntryStatus.Reversed)
                return ServiceResult.Failure("已沖銷的傳票不可刪除");

            return await base.CanDeleteAsync(entry);
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
                // 排除已沖銷/已取消的傳票，讓業務單據在傳票被沖銷後可以重新轉傳票
                return await context.JournalEntries
                    .Include(je => je.Company)
                    .Include(je => je.Lines)
                        .ThenInclude(l => l.AccountItem)
                    .FirstOrDefaultAsync(je =>
                        je.SourceDocumentType == sourceDocumentType &&
                        je.SourceDocumentId == sourceDocumentId &&
                        je.JournalEntryStatus != JournalEntryStatus.Reversed &&
                        je.JournalEntryStatus != JournalEntryStatus.Cancelled);
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

                // 驗證所有分錄只能使用明細科目（IsDetailAccount = true）
                var accountIds = entry.Lines.Select(l => l.AccountItemId).Distinct().ToList();
                var nonDetailAccounts = await context.AccountItems
                    .Where(a => accountIds.Contains(a.Id) && !a.IsDetailAccount)
                    .Select(a => $"[{a.Code}] {a.Name}")
                    .ToListAsync();
                if (nonDetailAccounts.Any())
                    return (false, $"以下科目非明細科目，不可記帳：{string.Join("、", nonDetailAccounts)}");

                entry.JournalEntryStatus = JournalEntryStatus.Posted;
                entry.UpdatedAt = DateTime.UtcNow;
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
                // 沖銷傳票不繼承 SourceDocumentType/Id/Code，避免與原傳票索引衝突，
                // 且沖銷傳票本身不代表一張業務單據
                var reversalEntry = new JournalEntry
                {
                    EntryDate = reversalDate,
                    EntryType = JournalEntryType.Reversing,
                    JournalEntryStatus = JournalEntryStatus.Posted,
                    Description = $"沖銷傳票 {original.Code}",
                    CompanyId = original.CompanyId,
                    FiscalYear = reversalDate.Year,
                    FiscalPeriod = reversalDate.Month,
                    SourceDocumentType = null,
                    SourceDocumentId = null,
                    SourceDocumentCode = original.SourceDocumentCode, // 保留單號供查詢參考
                    TotalDebitAmount = original.TotalCreditAmount,
                    TotalCreditAmount = original.TotalDebitAmount,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = updatedBy,
                    Lines = original.Lines.Select((l, index) => new JournalEntryLine
                    {
                        LineNumber = index + 1,
                        AccountItemId = l.AccountItemId,
                        Direction = l.Direction == AccountDirection.Debit
                            ? AccountDirection.Credit
                            : AccountDirection.Debit,
                        Amount = l.Amount,
                        LineDescription = $"沖銷：{l.LineDescription}",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                await using var transaction = await context.Database.BeginTransactionAsync();

                context.JournalEntries.Add(reversalEntry);
                await context.SaveChangesAsync();

                // 更新原傳票的沖銷狀態
                original.IsReversed = true;
                original.JournalEntryStatus = JournalEntryStatus.Reversed;
                original.ReversalEntryId = reversalEntry.Id;
                original.UpdatedAt = DateTime.UtcNow;
                original.UpdatedBy = updatedBy;

                // 重置來源業務單據的 IsJournalized，使其重新出現在待轉傳票清單，
                // 讓使用者在沖銷後可修正資料並重新轉傳票（修正 Bug-4）
                if (!string.IsNullOrWhiteSpace(original.SourceDocumentType) && original.SourceDocumentId.HasValue)
                {
                    await ResetSourceDocumentJournalizedAsync(context, original.SourceDocumentType, original.SourceDocumentId.Value);
                    // 清除原傳票的來源單據類型和 ID，釋放 UX_JournalEntry_SourceDocument 唯一索引佔位，
                    // 允許重新轉傳票時建立新傳票（修正 Bug-37）
                    // SourceDocumentCode（字串）保留供稽核查詢，不清除
                    original.SourceDocumentType = null;
                    original.SourceDocumentId = null;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
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

        public async Task<(bool Success, string ErrorMessage)> CancelDraftEntryAsync(int id, string updatedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entry = await context.JournalEntries.FindAsync(id);

                if (entry == null)
                    return (false, "找不到傳票");

                if (entry.JournalEntryStatus != JournalEntryStatus.Draft)
                    return (false, "只有草稿狀態的傳票才能作廢");

                // 自動產生的傳票作廢後，需由呼叫端重置來源業務單據的 IsJournalized
                entry.JournalEntryStatus = JournalEntryStatus.Cancelled;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.UpdatedBy = updatedBy;

                await context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelDraftEntryAsync), GetType(), _logger, new { Id = id });
                return (false, "作廢傳票時發生錯誤，請稍後再試");
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
                    // 強制 FiscalYear/FiscalPeriod 與 EntryDate 一致，不允許傳入值覆蓋
                    journalEntry.FiscalYear = journalEntry.EntryDate.Year;
                    journalEntry.FiscalPeriod = journalEntry.EntryDate.Month;

                    // 若傳票編號為空，自動產生
                    if (string.IsNullOrWhiteSpace(journalEntry.Code))
                    {
                        journalEntry.Code = await EntityCodeGenerationHelper
                            .GenerateForEntity<JournalEntry, IJournalEntryService>(this, "JE");
                    }

                    journalEntry.CreatedAt = DateTime.UtcNow;
                    journalEntry.CreatedBy = savedBy;

                    // 清除導航屬性，防止 EF Core 將已存在的實體誤判為 Added 並嘗試插入
                    journalEntry.Company = null!;
                    foreach (var line in lines)
                    {
                        line.AccountItem = null!;
                        line.CreatedAt = DateTime.UtcNow;
                        line.CreatedBy = savedBy;
                    }

                    context.JournalEntries.Add(journalEntry);
                }
                else
                {
                    // 更新：先載入 DB 實際狀態，再做合法性驗證
                    // 重要：狀態保護必須對 existing（DB 值）做檢查，不可信任前端傳入的 journalEntry.JournalEntryStatus
                    var existing = await context.JournalEntries
                        .Include(je => je.Lines)
                        .FirstOrDefaultAsync(je => je.Id == journalEntry.Id);

                    if (existing == null)
                        return (false, "找不到傳票");

                    // 依 DB 實際狀態阻擋不合法的修改操作
                    if (existing.JournalEntryStatus == JournalEntryStatus.Posted)
                        return (false, "已過帳的傳票不可修改，請先沖銷後重新建立");
                    if (existing.JournalEntryStatus == JournalEntryStatus.Reversed)
                        return (false, "已沖銷的傳票不可修改");
                    if (existing.JournalEntryStatus == JournalEntryStatus.Cancelled)
                        return (false, "已作廢的傳票不可修改");

                    // 自動產生的傳票不允許手動修改內容，需沖銷後由系統重新產生
                    if (existing.EntryType == JournalEntryType.AutoGenerated)
                        return (false, "自動產生的傳票不可手動修改，請沖銷後由系統重新產生");

                    // 移除舊分錄
                    context.JournalEntryLines.RemoveRange(existing.Lines);

                    // 強制 FiscalYear/FiscalPeriod 與 EntryDate 一致
                    var fiscalYear = journalEntry.EntryDate.Year;
                    var fiscalPeriod = journalEntry.EntryDate.Month;

                    // 更新主檔
                    existing.EntryDate = journalEntry.EntryDate;
                    existing.EntryType = journalEntry.EntryType;
                    existing.Description = journalEntry.Description;
                    existing.CompanyId = journalEntry.CompanyId;
                    existing.FiscalYear = fiscalYear;
                    existing.FiscalPeriod = fiscalPeriod;
                    existing.SourceDocumentType = journalEntry.SourceDocumentType;
                    existing.SourceDocumentId = journalEntry.SourceDocumentId;
                    existing.SourceDocumentCode = journalEntry.SourceDocumentCode;
                    existing.TotalDebitAmount = journalEntry.TotalDebitAmount;
                    existing.TotalCreditAmount = journalEntry.TotalCreditAmount;
                    existing.Remarks = journalEntry.Remarks;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = savedBy;

                    // 加入新分錄（清除導航屬性，防止 EF Core 誤判為 Added）
                    foreach (var line in lines)
                    {
                        line.Id = 0;
                        line.JournalEntryId = existing.Id;
                        line.JournalEntry = null!;
                        line.AccountItem = null!;
                        line.CreatedAt = DateTime.UtcNow;
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

        public async Task<(List<JournalEntry> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<JournalEntry>, IQueryable<JournalEntry>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<JournalEntry> query = context.JournalEntries
                    .Include(je => je.Company);

                if (filterFunc != null)
                    query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(je => je.EntryDate)
                    .ThenByDescending(je => je.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<JournalEntry>(), 0);
            }
        }

        public override async Task<ServiceResult> ValidateAsync(JournalEntry entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.Code == null)
                {
                    errors.Add("傳票資料為空");
                    return ServiceResult.Failure(string.Join("; ", errors));
                }
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

        /// <summary>
        /// 沖銷傳票後重置來源業務單據的 IsJournalized = false，
        /// 使其重新出現在待轉傳票清單，允許使用者修正後重新轉傳票
        /// </summary>
        private static async Task ResetSourceDocumentJournalizedAsync(AppDbContext context, string sourceDocumentType, int sourceDocumentId)
        {
            switch (sourceDocumentType)
            {
                case "PurchaseReceiving":
                    var pr = await context.PurchaseReceivings.FindAsync(sourceDocumentId);
                    if (pr != null) { pr.IsJournalized = false; pr.JournalizedAt = null; }
                    break;
                case "PurchaseReturn":
                    var pret = await context.PurchaseReturns.FindAsync(sourceDocumentId);
                    if (pret != null) { pret.IsJournalized = false; pret.JournalizedAt = null; }
                    break;
                case "SalesDelivery":
                    var sd = await context.SalesDeliveries.FindAsync(sourceDocumentId);
                    if (sd != null) { sd.IsJournalized = false; sd.JournalizedAt = null; }
                    break;
                case "SalesReturn":
                    var sr = await context.SalesReturns.FindAsync(sourceDocumentId);
                    if (sr != null) { sr.IsJournalized = false; sr.JournalizedAt = null; }
                    break;
                case "SetoffDocument":
                    var doc = await context.SetoffDocuments.FindAsync(sourceDocumentId);
                    if (doc != null) { doc.IsJournalized = false; doc.JournalizedAt = null; }
                    break;
            }
        }
    }
}
