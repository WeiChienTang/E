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
    /// 明細科目餘額表報表服務實作
    /// 彙總各科目的期初餘額、本期借方、本期貸方、期末餘額（無逐筆明細）
    /// 期初餘額 = StartDate 之前的累計淨額
    /// 期末餘額 = 期初 + 本期借方 - 本期貸方
    /// </summary>
    public class DetailAccountBalanceReportService : IDetailAccountBalanceReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<DetailAccountBalanceReportService>? _logger;

        public DetailAccountBalanceReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<DetailAccountBalanceReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(DetailAccountBalanceCriteria criteria)
        {
            try
            {
                var lines = await BuildAccountBalanceLinesAsync(criteria);

                if (!lines.Any())
                    return BatchPreviewResult.Failure($"無符合條件的傳票資料\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildDetailAccountBalanceDocument(lines, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, lines.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生明細科目餘額表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        // ===== 資料查詢 =====

        private async Task<List<AccountBalanceLine>> BuildAccountBalanceLinesAsync(DetailAccountBalanceCriteria criteria)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Query 1: 期初餘額（StartDate 之前所有 Posted）
            var openingQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.StartDate.HasValue)
                openingQuery = openingQuery.Where(l => l.JournalEntry.EntryDate < criteria.StartDate.Value.Date);

            // Query 2: 本期發生額（StartDate ~ EndDate）
            var periodQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.StartDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);
            if (criteria.EndDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            // 科目大類篩選
            if (criteria.AccountTypes.Any())
            {
                openingQuery = openingQuery.Where(l => criteria.AccountTypes.Contains(l.AccountItem.AccountType));
                periodQuery = periodQuery.Where(l => criteria.AccountTypes.Contains(l.AccountItem.AccountType));
            }

            var openingLines = await openingQuery.ToListAsync();
            var periodLines = await periodQuery.ToListAsync();

            // 建立期初餘額字典
            var openingMap = new Dictionary<int, (decimal Debit, decimal Credit)>();
            foreach (var line in openingLines)
            {
                if (!openingMap.ContainsKey(line.AccountItemId))
                    openingMap[line.AccountItemId] = (0m, 0m);
                var cur = openingMap[line.AccountItemId];
                openingMap[line.AccountItemId] = line.Direction == AccountDirection.Debit
                    ? (cur.Debit + line.Amount, cur.Credit)
                    : (cur.Debit, cur.Credit + line.Amount);
            }

            // 建立本期發生額字典
            var periodMap = new Dictionary<int, (decimal Debit, decimal Credit, string Code, string Name, AccountType Type, AccountDirection Direction, int SortOrder)>();
            foreach (var line in periodLines)
            {
                if (!periodMap.ContainsKey(line.AccountItemId))
                    periodMap[line.AccountItemId] = (0m, 0m,
                        line.AccountItem.Code ?? string.Empty,
                        line.AccountItem.Name,
                        line.AccountItem.AccountType,
                        line.AccountItem.Direction,
                        line.AccountItem.SortOrder);
                var cur = periodMap[line.AccountItemId];
                periodMap[line.AccountItemId] = line.Direction == AccountDirection.Debit
                    ? cur with { Debit = cur.Debit + line.Amount }
                    : cur with { Credit = cur.Credit + line.Amount };
            }

            // 同時從期初查詢補充科目資訊（若某科目只有期初餘額沒有本期發生）
            var accountInfoMap = new Dictionary<int, (string Code, string Name, AccountType Type, AccountDirection Direction, int SortOrder)>();
            foreach (var line in openingLines)
            {
                if (!accountInfoMap.ContainsKey(line.AccountItemId))
                    accountInfoMap[line.AccountItemId] = (
                        line.AccountItem.Code ?? string.Empty,
                        line.AccountItem.Name,
                        line.AccountItem.AccountType,
                        line.AccountItem.Direction,
                        line.AccountItem.SortOrder);
            }
            foreach (var line in periodLines)
            {
                if (!accountInfoMap.ContainsKey(line.AccountItemId))
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

            var result = new List<AccountBalanceLine>();

            foreach (var accountId in allAccountIds)
            {
                if (!accountInfoMap.TryGetValue(accountId, out var info)) continue;

                if (criteria.AccountTypes.Any() && !criteria.AccountTypes.Contains(info.Type))
                    continue;

                var opDebit = openingMap.TryGetValue(accountId, out var op) ? op.Debit : 0m;
                var opCredit = openingMap.TryGetValue(accountId, out op) ? op.Credit : 0m;
                var periodDebit = periodMap.TryGetValue(accountId, out var pd) ? pd.Debit : 0m;
                var periodCredit = periodMap.TryGetValue(accountId, out pd) ? pd.Credit : 0m;

                // 期初餘額（帶正負號，借方為正）
                decimal openingBalance = opDebit - opCredit;
                decimal closingBalance = openingBalance + periodDebit - periodCredit;

                // 零餘額篩選
                if (!criteria.ShowZeroBalance && openingBalance == 0 && periodDebit == 0 && periodCredit == 0 && closingBalance == 0)
                    continue;

                result.Add(new AccountBalanceLine
                {
                    AccountItemId = accountId,
                    Code = info.Code,
                    Name = info.Name,
                    AccountType = info.Type,
                    NormalDirection = info.Direction,
                    SortOrder = info.SortOrder,
                    OpeningBalance = openingBalance,
                    PeriodDebit = periodDebit,
                    PeriodCredit = periodCredit,
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
        private FormattedDocument BuildDetailAccountBalanceDocument(
            List<AccountBalanceLine> lines,
            Company? company,
            DetailAccountBalanceCriteria criteria)
        {
            var periodLabel = criteria.StartDate.HasValue && criteria.EndDate.HasValue
                ? $"{criteria.StartDate:yyyy/MM/dd} ~ {criteria.EndDate:yyyy/MM/dd}"
                : criteria.EndDate.HasValue
                    ? $"截至 {criteria.EndDate:yyyy/MM/dd}"
                    : $"截至 {DateTime.Today:yyyy/MM/dd}";

            var doc = new FormattedDocument()
                .SetDocumentName($"明細科目餘額表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("明  細  科  目  餘  額  表", 16f, true),
                        ($"期間：{periodLabel}", 10f, false)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"科目數：{lines.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);
                header.AddSpacing(5);
            });

            // === 依科目大類分組 ===
            var groups = lines.GroupBy(l => l.AccountType).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var typeName = GetAccountTypeName(group.Key);
                doc.AddKeyValueRow(("科目大類", typeName));
                doc.AddSpacing(2);

                doc.AddTable(table =>
                {
                    table.AddColumn("科目代碼", 0.75f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目名稱", 1.60f, Models.Reports.TextAlignment.Left)
                         .AddColumn("期初餘額", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("本期借方", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("本期貸方", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期末餘額", 0.90f, Models.Reports.TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    foreach (var item in group)
                    {
                        table.AddRow(
                            item.Code,
                            item.Name,
                            FormatBalance(item.OpeningBalance),
                            item.PeriodDebit > 0 ? item.PeriodDebit.ToString("N2") : string.Empty,
                            item.PeriodCredit > 0 ? item.PeriodCredit.ToString("N2") : string.Empty,
                            FormatBalance(item.ClosingBalance)
                        );
                    }

                    // 小計
                    var gOpeningBalance = group.Sum(l => l.OpeningBalance);
                    var gPeriodDebit = group.Sum(l => l.PeriodDebit);
                    var gPeriodCredit = group.Sum(l => l.PeriodCredit);
                    var gClosingBalance = group.Sum(l => l.ClosingBalance);

                    table.AddRow(
                        string.Empty,
                        $"{typeName} 小計",
                        FormatBalance(gOpeningBalance),
                        gPeriodDebit > 0 ? gPeriodDebit.ToString("N2") : "—",
                        gPeriodCredit > 0 ? gPeriodCredit.ToString("N2") : "—",
                        FormatBalance(gClosingBalance)
                    );
                });

                doc.AddSpacing(5);
            }

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalOpening = lines.Sum(l => l.OpeningBalance);
                var totalDebit = lines.Sum(l => l.PeriodDebit);
                var totalCredit = lines.Sum(l => l.PeriodCredit);
                var totalClosing = lines.Sum(l => l.ClosingBalance);

                var summaryLines = new List<string>
                {
                    $"科目數：{lines.Count}　　期初餘額合計：{FormatBalance(totalOpening)}　　本期借方合計：{totalDebit:N2}　　本期貸方合計：{totalCredit:N2}　　期末餘額合計：{FormatBalance(totalClosing)}",
                    ""
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 1.0f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "財務主管" });
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

        private class AccountBalanceLine
        {
            public int AccountItemId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public AccountType AccountType { get; set; }
            public AccountDirection NormalDirection { get; set; }
            public int SortOrder { get; set; }
            public decimal OpeningBalance { get; set; }
            public decimal PeriodDebit { get; set; }
            public decimal PeriodCredit { get; set; }
            public decimal ClosingBalance { get; set; }
        }
    }
}
