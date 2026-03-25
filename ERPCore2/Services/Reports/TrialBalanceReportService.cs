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
    /// 試算表報表服務實作
    /// 顯示指定期間各科目的「期初餘額（借/貸）」「本期發生額（借/貸）」和「期末累計餘額（借/貸）」
    /// 期初餘額：StartDate 前的所有已過帳分錄（含期初餘額傳票）的累積餘額
    /// 本期發生額：EntryDate 在 StartDate ~ EndDate 之間
    /// 期末累計餘額：期初餘額加本期發生額的淨值
    /// </summary>
    public class TrialBalanceReportService : ITrialBalanceReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<TrialBalanceReportService>? _logger;

        public TrialBalanceReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<TrialBalanceReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(TrialBalanceCriteria criteria)
        {
            try
            {
                var lines = await BuildAccountBalanceLinesAsync(criteria);

                if (!lines.Any())
                    return BatchPreviewResult.Failure($"無符合條件的傳票資料\n篩選條件：{criteria.GetSummary()}");

                var company = criteria.CompanyId.HasValue
                    ? await _companyService.GetByIdAsync(criteria.CompanyId.Value)
                    : await _companyService.GetPrimaryCompanyAsync();
                var document = BuildTrialBalanceDocument(lines, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, lines.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生試算表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        // ===== 資料查詢 =====

        private async Task<List<AccountBalanceLine>> BuildAccountBalanceLinesAsync(TrialBalanceCriteria criteria)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // CompanyId 篩選（留空時自動使用主要公司）
            var primaryCompany = await _companyService.GetPrimaryCompanyAsync();
            var companyId = criteria.CompanyId ?? primaryCompany?.Id;

            // Query 1: 期初餘額（StartDate 之前的所有已過帳分錄，包含期初餘額傳票）
            var openingQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted)
                .Where(l => !companyId.HasValue || l.JournalEntry.CompanyId == companyId.Value);

            if (criteria.StartDate.HasValue)
                openingQuery = openingQuery.Where(l => l.JournalEntry.EntryDate < criteria.StartDate.Value.Date);

            // Query 2: 本期發生額（StartDate ~ EndDate）
            var periodQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted)
                .Where(l => !companyId.HasValue || l.JournalEntry.CompanyId == companyId.Value);

            if (criteria.StartDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);
            if (criteria.EndDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            var openingLines = criteria.StartDate.HasValue ? await openingQuery.ToListAsync() : new List<JournalEntryLine>();
            var periodLines  = await periodQuery.ToListAsync();

            // 合併：以 AccountItem 為鍵
            var accountMap = new Dictionary<int, AccountBalanceLine>();

            // 輔助：確保帳目行存在
            AccountBalanceLine EnsureEntry(int id, JournalEntryLine l)
            {
                if (!accountMap.TryGetValue(id, out var e))
                {
                    e = new AccountBalanceLine
                    {
                        AccountItemId = id,
                        Code = l.AccountItem.Code ?? string.Empty,
                        Name = l.AccountItem.Name,
                        AccountType = l.AccountItem.AccountType,
                        NormalDirection = l.AccountItem.Direction,
                        SortOrder = l.AccountItem.SortOrder
                    };
                    accountMap[id] = e;
                }
                return e;
            }

            // 處理期初餘額
            foreach (var line in openingLines)
            {
                var entry = EnsureEntry(line.AccountItemId, line);
                if (line.Direction == AccountDirection.Debit)
                    entry.OpeningDebit += line.Amount;
                else
                    entry.OpeningCredit += line.Amount;
            }

            // 處理本期發生額
            foreach (var line in periodLines)
            {
                var entry = EnsureEntry(line.AccountItemId, line);
                if (line.Direction == AccountDirection.Debit)
                    entry.PeriodDebit += line.Amount;
                else
                    entry.PeriodCredit += line.Amount;
            }

            // ShowZeroBalance = true 時，補齊從未有傳票的科目（金額全為 0）
            if (criteria.ShowZeroBalance)
            {
                var existingIds = accountMap.Keys.ToHashSet();
                var missingAccounts = await context.AccountItems
                    .Where(a => a.Status == EntityStatus.Active && a.IsDetailAccount)
                    .Where(a => !existingIds.Contains(a.Id))
                    .Where(a => criteria.AccountTypes.Count == 0 || criteria.AccountTypes.Contains(a.AccountType))
                    .ToListAsync();

                foreach (var acct in missingAccounts)
                {
                    accountMap[acct.Id] = new AccountBalanceLine
                    {
                        AccountItemId = acct.Id,
                        Code = acct.Code ?? string.Empty,
                        Name = acct.Name,
                        AccountType = acct.AccountType,
                        NormalDirection = acct.Direction,
                        SortOrder = acct.SortOrder
                    };
                }
            }

            var result = accountMap.Values
                .Where(l => criteria.AccountTypes.Count == 0 || criteria.AccountTypes.Contains(l.AccountType))
                .Where(l => criteria.ShowZeroBalance ||
                    l.OpeningDebit != 0 || l.OpeningCredit != 0 ||
                    l.PeriodDebit != 0 || l.PeriodCredit != 0 ||
                    l.EndingDebitBalance != 0 || l.EndingCreditBalance != 0)
                .OrderBy(l => l.AccountType)
                .ThenBy(l => l.SortOrder)
                .ThenBy(l => l.Code)
                .ToList();

            return result;
        }

        // ===== 報表建構 =====

        [SupportedOSPlatform("windows6.1")]
        private FormattedDocument BuildTrialBalanceDocument(
            List<AccountBalanceLine> lines,
            Company? company,
            TrialBalanceCriteria criteria)
        {
            var periodLabel = criteria.StartDate.HasValue && criteria.EndDate.HasValue
                ? $"{criteria.StartDate:yyyy/MM/dd} ~ {criteria.EndDate:yyyy/MM/dd}"
                : criteria.EndDate.HasValue
                    ? $"截至 {criteria.EndDate:yyyy/MM/dd}"
                    : $"截至 {DateTime.Today:yyyy/MM/dd}";

            var doc = new FormattedDocument()
                .SetDocumentName($"試算表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("試  算  表", 16f, true),
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
                    table.AddColumn("科目代碼", 0.65f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目名稱", 1.30f, Models.Reports.TextAlignment.Left)
                         .AddColumn("期初借方", 0.85f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期初貸方", 0.85f, Models.Reports.TextAlignment.Right)
                         .AddColumn("本期借方", 0.85f, Models.Reports.TextAlignment.Right)
                         .AddColumn("本期貸方", 0.85f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期末借方", 0.85f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期末貸方", 0.85f, Models.Reports.TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    foreach (var item in group)
                    {
                        table.AddRow(
                            item.Code,
                            item.Name,
                            item.OpeningDebitBalance > 0  ? item.OpeningDebitBalance.ToString("N2")  : string.Empty,
                            item.OpeningCreditBalance > 0 ? item.OpeningCreditBalance.ToString("N2") : string.Empty,
                            item.PeriodDebit > 0  ? item.PeriodDebit.ToString("N2")  : string.Empty,
                            item.PeriodCredit > 0 ? item.PeriodCredit.ToString("N2") : string.Empty,
                            item.EndingDebitBalance > 0  ? item.EndingDebitBalance.ToString("N2")  : string.Empty,
                            item.EndingCreditBalance > 0 ? item.EndingCreditBalance.ToString("N2") : string.Empty
                        );
                    }

                    // 小計
                    var gOpenD  = group.Sum(l => l.OpeningDebitBalance);
                    var gOpenC  = group.Sum(l => l.OpeningCreditBalance);
                    var gPeriodD = group.Sum(l => l.PeriodDebit);
                    var gPeriodC = group.Sum(l => l.PeriodCredit);
                    var gEndD   = group.Sum(l => l.EndingDebitBalance);
                    var gEndC   = group.Sum(l => l.EndingCreditBalance);

                    table.AddRow(
                        string.Empty,
                        $"{typeName} 小計",
                        gOpenD  > 0 ? gOpenD.ToString("N2")   : string.Empty,
                        gOpenC  > 0 ? gOpenC.ToString("N2")   : string.Empty,
                        gPeriodD > 0 ? gPeriodD.ToString("N2") : string.Empty,
                        gPeriodC > 0 ? gPeriodC.ToString("N2") : string.Empty,
                        gEndD   > 0 ? gEndD.ToString("N2")   : string.Empty,
                        gEndC   > 0 ? gEndC.ToString("N2")   : string.Empty
                    );
                });

                doc.AddSpacing(5);
            }

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalOpenDebit  = lines.Sum(l => l.OpeningDebitBalance);
                var totalOpenCredit = lines.Sum(l => l.OpeningCreditBalance);
                var totalPeriodDebit  = lines.Sum(l => l.PeriodDebit);
                var totalPeriodCredit = lines.Sum(l => l.PeriodCredit);
                var totalEndDebit  = lines.Sum(l => l.EndingDebitBalance);
                var totalEndCredit = lines.Sum(l => l.EndingCreditBalance);
                var openingBalanced = totalOpenDebit  == totalOpenCredit;
                var periodBalanced  = totalPeriodDebit == totalPeriodCredit;
                var endingBalanced  = totalEndDebit   == totalEndCredit;

                var summaryLines = new List<string>
                {
                    $"期初借方合計：{totalOpenDebit:N2}　期初貸方合計：{totalOpenCredit:N2}　{(openingBalanced ? "✓ 平衡" : $"差額：{totalOpenDebit - totalOpenCredit:N2}")}",
                    $"本期借方合計：{totalPeriodDebit:N2}　本期貸方合計：{totalPeriodCredit:N2}　{(periodBalanced ? "✓ 平衡" : $"差額：{totalPeriodDebit - totalPeriodCredit:N2}")}",
                    $"期末借方餘額：{totalEndDebit:N2}　期末貸方餘額：{totalEndCredit:N2}　{(endingBalanced ? "✓ 平衡" : $"差額：{totalEndDebit - totalEndCredit:N2}")}",
                    ""
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.7f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "財務主管" });
            });

            return doc;
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

            // 期初餘額（StartDate 前的累積）
            public decimal OpeningDebit { get; set; }
            public decimal OpeningCredit { get; set; }

            // 本期發生額（日期範圍篩選）
            public decimal PeriodDebit { get; set; }
            public decimal PeriodCredit { get; set; }

            // 期初淨餘額（依正常借貸方向）
            private decimal NetOpening => NormalDirection == AccountDirection.Debit
                ? OpeningDebit - OpeningCredit
                : OpeningCredit - OpeningDebit;

            // 期初借貸餘額顯示
            public decimal OpeningDebitBalance =>
                (NetOpening > 0 && NormalDirection == AccountDirection.Debit) ||
                (NetOpening < 0 && NormalDirection == AccountDirection.Credit)
                    ? Math.Abs(NetOpening) : 0;

            public decimal OpeningCreditBalance =>
                (NetOpening > 0 && NormalDirection == AccountDirection.Credit) ||
                (NetOpening < 0 && NormalDirection == AccountDirection.Debit)
                    ? Math.Abs(NetOpening) : 0;

            // 期末累計（期初 + 本期）
            private decimal TotalDebit => OpeningDebit + PeriodDebit;
            private decimal TotalCredit => OpeningCredit + PeriodCredit;

            // 期末淨餘額
            private decimal NetCumulative => NormalDirection == AccountDirection.Debit
                ? TotalDebit - TotalCredit
                : TotalCredit - TotalDebit;

            // 期末借貸餘額顯示
            public decimal EndingDebitBalance =>
                (NetCumulative > 0 && NormalDirection == AccountDirection.Debit) ||
                (NetCumulative < 0 && NormalDirection == AccountDirection.Credit)
                    ? Math.Abs(NetCumulative) : 0;

            public decimal EndingCreditBalance =>
                (NetCumulative > 0 && NormalDirection == AccountDirection.Credit) ||
                (NetCumulative < 0 && NormalDirection == AccountDirection.Debit)
                    ? Math.Abs(NetCumulative) : 0;
        }
    }
}
