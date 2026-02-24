using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 明細分類帳報表服務實作
    /// 依科目關鍵字篩選特定科目，顯示帳戶卡片（期初餘額 + 逐筆明細 + 期末餘額）
    /// 適合查看應收帳款按客戶、應付帳款按廠商等子科目明細
    /// </summary>
    public class SubsidiaryLedgerReportService : ISubsidiaryLedgerReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SubsidiaryLedgerReportService>? _logger;

        public SubsidiaryLedgerReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<SubsidiaryLedgerReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(SubsidiaryLedgerCriteria criteria)
        {
            try
            {
                var accountLedgers = await BuildAccountLedgersAsync(criteria);

                if (!accountLedgers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的傳票資料\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSubsidiaryLedgerDocument(accountLedgers, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, accountLedgers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生明細分類帳時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        // ===== 資料查詢 =====

        private async Task<List<AccountLedger>> BuildAccountLedgersAsync(SubsidiaryLedgerCriteria criteria)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var keyword = criteria.AccountKeyword?.Trim();

            // Query 1: 期初餘額（StartDate 之前所有 Posted 傳票）
            var openingQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.StartDate.HasValue)
                openingQuery = openingQuery.Where(l => l.JournalEntry.EntryDate < criteria.StartDate.Value.Date);

            // Query 2: 本期明細（StartDate ~ EndDate 的所有 Posted 傳票）
            var periodQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.StartDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);
            if (criteria.EndDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            // 科目關鍵字篩選
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                openingQuery = openingQuery.Where(l =>
                    (l.AccountItem.Code != null && l.AccountItem.Code.Contains(keyword)) ||
                    l.AccountItem.Name.Contains(keyword));
                periodQuery = periodQuery.Where(l =>
                    (l.AccountItem.Code != null && l.AccountItem.Code.Contains(keyword)) ||
                    l.AccountItem.Name.Contains(keyword));
            }

            // 科目大類篩選
            if (criteria.AccountTypes.Any())
            {
                openingQuery = openingQuery.Where(l => criteria.AccountTypes.Contains(l.AccountItem.AccountType));
                periodQuery = periodQuery.Where(l => criteria.AccountTypes.Contains(l.AccountItem.AccountType));
            }

            var openingLines = await openingQuery.ToListAsync();
            var periodLines = await periodQuery
                .OrderBy(l => l.JournalEntry.EntryDate)
                .ThenBy(l => l.JournalEntry.Id)
                .ThenBy(l => l.LineNumber)
                .ToListAsync();

            // 建立期初餘額字典
            var openingMap = new Dictionary<int, (decimal Debit, decimal Credit, string Code, string Name, AccountType Type, AccountDirection Direction, int SortOrder)>();
            foreach (var line in openingLines)
            {
                var key = line.AccountItemId;
                if (!openingMap.TryGetValue(key, out var entry))
                {
                    entry = (0m, 0m, line.AccountItem.Code ?? string.Empty, line.AccountItem.Name,
                             line.AccountItem.AccountType, line.AccountItem.Direction, line.AccountItem.SortOrder);
                    openingMap[key] = entry;
                }
                if (line.Direction == AccountDirection.Debit)
                    openingMap[key] = entry with { Debit = entry.Debit + line.Amount };
                else
                    openingMap[key] = entry with { Credit = entry.Credit + line.Amount };
            }

            // 建立本期明細字典
            var periodMap = new Dictionary<int, List<LedgerEntry>>();
            var accountInfoMap = new Dictionary<int, (string Code, string Name, AccountType Type, AccountDirection Direction, int SortOrder)>();

            foreach (var line in periodLines)
            {
                if (!periodMap.ContainsKey(line.AccountItemId))
                    periodMap[line.AccountItemId] = new List<LedgerEntry>();

                periodMap[line.AccountItemId].Add(new LedgerEntry
                {
                    EntryDate = line.JournalEntry.EntryDate,
                    EntryCode = line.JournalEntry.Code ?? string.Empty,
                    Description = line.LineDescription ?? line.JournalEntry.Description ?? string.Empty,
                    DebitAmount = line.Direction == AccountDirection.Debit ? line.Amount : 0m,
                    CreditAmount = line.Direction == AccountDirection.Credit ? line.Amount : 0m
                });

                accountInfoMap[line.AccountItemId] = (
                    line.AccountItem.Code ?? string.Empty,
                    line.AccountItem.Name,
                    line.AccountItem.AccountType,
                    line.AccountItem.Direction,
                    line.AccountItem.SortOrder);
            }

            // 合併所有出現過的 AccountItemId
            var allAccountIds = new HashSet<int>(openingMap.Keys);
            allAccountIds.UnionWith(periodMap.Keys);

            var result = new List<AccountLedger>();

            foreach (var accountId in allAccountIds)
            {
                string code, name;
                AccountType accountType;
                AccountDirection direction;
                int sortOrder;

                if (accountInfoMap.TryGetValue(accountId, out var info))
                    (code, name, accountType, direction, sortOrder) = info;
                else if (openingMap.TryGetValue(accountId, out var opInfo))
                {
                    code = opInfo.Code;
                    name = opInfo.Name;
                    accountType = opInfo.Type;
                    direction = opInfo.Direction;
                    sortOrder = opInfo.SortOrder;
                }
                else continue;

                if (criteria.AccountTypes.Any() && !criteria.AccountTypes.Contains(accountType))
                    continue;

                decimal openingDebit = 0m, openingCredit = 0m;
                if (openingMap.TryGetValue(accountId, out var op))
                {
                    openingDebit = op.Debit;
                    openingCredit = op.Credit;
                }
                decimal openingBalance = openingDebit - openingCredit;

                var entries = periodMap.TryGetValue(accountId, out var list) ? list : new List<LedgerEntry>();

                decimal periodTotalDebit = entries.Sum(e => e.DebitAmount);
                decimal periodTotalCredit = entries.Sum(e => e.CreditAmount);
                decimal closingBalance = openingBalance + periodTotalDebit - periodTotalCredit;

                // 計算流水餘額
                decimal runningBalance = openingBalance;
                foreach (var entry in entries)
                {
                    runningBalance += entry.DebitAmount - entry.CreditAmount;
                    entry.RunningBalance = runningBalance;
                }

                result.Add(new AccountLedger
                {
                    AccountItemId = accountId,
                    Code = code,
                    Name = name,
                    AccountType = accountType,
                    NormalDirection = direction,
                    SortOrder = sortOrder,
                    OpeningBalance = openingBalance,
                    Entries = entries,
                    PeriodTotalDebit = periodTotalDebit,
                    PeriodTotalCredit = periodTotalCredit,
                    ClosingBalance = closingBalance
                });
            }

            return result
                .OrderBy(l => l.AccountType)
                .ThenBy(l => l.SortOrder)
                .ThenBy(l => l.Code)
                .ToList();
        }

        // ===== 報表建構 =====

        [SupportedOSPlatform("windows6.1")]
        private FormattedDocument BuildSubsidiaryLedgerDocument(
            List<AccountLedger> ledgers,
            Company? company,
            SubsidiaryLedgerCriteria criteria)
        {
            var periodLabel = criteria.StartDate.HasValue && criteria.EndDate.HasValue
                ? $"{criteria.StartDate:yyyy/MM/dd} ~ {criteria.EndDate:yyyy/MM/dd}"
                : criteria.EndDate.HasValue
                    ? $"截至 {criteria.EndDate:yyyy/MM/dd}"
                    : $"截至 {DateTime.Today:yyyy/MM/dd}";

            var keywordLabel = !string.IsNullOrWhiteSpace(criteria.AccountKeyword)
                ? $"  科目關鍵字：{criteria.AccountKeyword}"
                : string.Empty;

            var doc = new FormattedDocument()
                .SetDocumentName($"明細分類帳-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("明  細  分  類  帳", 16f, true),
                        ($"期間：{periodLabel}{keywordLabel}", 10f, false)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"科目數：{ledgers.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);
                header.AddSpacing(5);
            });

            // === 依科目大類分組 ===
            var groups = ledgers.GroupBy(l => l.AccountType).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var typeName = GetAccountTypeName(group.Key);
                doc.AddKeyValueRow(("科目大類", typeName));
                doc.AddSpacing(3);

                foreach (var ledger in group)
                {
                    doc.AddKeyValueRow(($"{ledger.Code}", $"{ledger.Name}"));
                    doc.AddSpacing(2);

                    doc.AddTable(table =>
                    {
                        table.AddColumn("日期", 0.75f, Models.Reports.TextAlignment.Center)
                             .AddColumn("傳票號碼", 0.85f, Models.Reports.TextAlignment.Center)
                             .AddColumn("摘要", 1.80f, Models.Reports.TextAlignment.Left)
                             .AddColumn("借方", 0.90f, Models.Reports.TextAlignment.Right)
                             .AddColumn("貸方", 0.90f, Models.Reports.TextAlignment.Right)
                             .AddColumn("餘額", 0.90f, Models.Reports.TextAlignment.Right)
                             .ShowBorder(false)
                             .ShowHeaderBackground(false)
                             .ShowHeaderSeparator(false)
                             .SetRowHeight(20);

                        table.AddRow(
                            string.Empty, string.Empty,
                            "期初餘額",
                            string.Empty, string.Empty,
                            FormatBalance(ledger.OpeningBalance));

                        foreach (var entry in ledger.Entries)
                        {
                            table.AddRow(
                                entry.EntryDate.ToString("yyyy/MM/dd"),
                                entry.EntryCode,
                                entry.Description,
                                entry.DebitAmount > 0 ? entry.DebitAmount.ToString("N2") : string.Empty,
                                entry.CreditAmount > 0 ? entry.CreditAmount.ToString("N2") : string.Empty,
                                FormatBalance(entry.RunningBalance));
                        }

                        table.AddRow(
                            string.Empty, string.Empty,
                            "本期合計",
                            ledger.PeriodTotalDebit > 0 ? ledger.PeriodTotalDebit.ToString("N2") : "—",
                            ledger.PeriodTotalCredit > 0 ? ledger.PeriodTotalCredit.ToString("N2") : "—",
                            FormatBalance(ledger.ClosingBalance));
                    });

                    doc.AddSpacing(8);
                }

                doc.AddSpacing(5);
            }

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);
                footer.AddSignatureSection(new[] { "製表人員", "財務主管" });
            });

            return doc;
        }

        private static string FormatBalance(decimal balance)
        {
            if (balance == 0) return "0.00";
            return balance > 0
                ? balance.ToString("N2")
                : $"({Math.Abs(balance):N2})";
        }

        private static string GetAccountTypeName(AccountType t) => t switch
        {
            AccountType.Asset => "資產",
            AccountType.Liability => "負債",
            AccountType.Equity => "權益",
            AccountType.Revenue => "營業收入",
            AccountType.Cost => "營業成本",
            AccountType.Expense => "營業費用",
            AccountType.NonOperatingIncomeAndExpense => "營業外收益及費損",
            AccountType.ComprehensiveIncome => "綜合損益總額",
            _ => t.ToString()
        };

        // ===== 內部資料模型 =====

        private class AccountLedger
        {
            public int AccountItemId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public AccountType AccountType { get; set; }
            public AccountDirection NormalDirection { get; set; }
            public int SortOrder { get; set; }
            public decimal OpeningBalance { get; set; }
            public List<LedgerEntry> Entries { get; set; } = new();
            public decimal PeriodTotalDebit { get; set; }
            public decimal PeriodTotalCredit { get; set; }
            public decimal ClosingBalance { get; set; }
        }

        private class LedgerEntry
        {
            public DateTime EntryDate { get; set; }
            public string EntryCode { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal DebitAmount { get; set; }
            public decimal CreditAmount { get; set; }
            public decimal RunningBalance { get; set; }
        }
    }
}
