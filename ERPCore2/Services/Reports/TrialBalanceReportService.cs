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
    /// 顯示指定期間各科目的「本期發生額（借/貸）」和「期末累計餘額（借/貸）」
    /// 本期發生額：EntryDate 在 StartDate ~ EndDate 之間
    /// 期末累計餘額：EntryDate 從最早傳票到 EndDate（累積全部歷史）
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

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildTrialBalanceDocument(lines, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, lines.Count);
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

            // Query 1: 本期發生額（StartDate ~ EndDate）
            var periodQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.StartDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);
            if (criteria.EndDate.HasValue)
                periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            // Query 2: 累計餘額（最早 ~ EndDate）
            var cumulativeQuery = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted);

            if (criteria.EndDate.HasValue)
                cumulativeQuery = cumulativeQuery.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            var periodLines = await periodQuery.ToListAsync();
            var cumulativeLines = await cumulativeQuery.ToListAsync();

            // 合併：以 AccountItem 為鍵
            var accountMap = new Dictionary<int, AccountBalanceLine>();

            // 處理本期發生額
            foreach (var line in periodLines)
            {
                if (!accountMap.TryGetValue(line.AccountItemId, out var entry))
                {
                    entry = new AccountBalanceLine
                    {
                        AccountItemId = line.AccountItemId,
                        Code = line.AccountItem.Code ?? string.Empty,
                        Name = line.AccountItem.Name,
                        AccountType = line.AccountItem.AccountType,
                        NormalDirection = line.AccountItem.Direction,
                        SortOrder = line.AccountItem.SortOrder
                    };
                    accountMap[line.AccountItemId] = entry;
                }

                if (line.Direction == AccountDirection.Debit)
                    entry.PeriodDebit += line.Amount;
                else
                    entry.PeriodCredit += line.Amount;
            }

            // 處理累計餘額
            foreach (var line in cumulativeLines)
            {
                if (!accountMap.TryGetValue(line.AccountItemId, out var entry))
                {
                    entry = new AccountBalanceLine
                    {
                        AccountItemId = line.AccountItemId,
                        Code = line.AccountItem.Code ?? string.Empty,
                        Name = line.AccountItem.Name,
                        AccountType = line.AccountItem.AccountType,
                        NormalDirection = line.AccountItem.Direction,
                        SortOrder = line.AccountItem.SortOrder
                    };
                    accountMap[line.AccountItemId] = entry;
                }

                if (line.Direction == AccountDirection.Debit)
                    entry.CumulativeDebit += line.Amount;
                else
                    entry.CumulativeCredit += line.Amount;
            }

            var result = accountMap.Values
                .Where(l => criteria.AccountTypes.Count == 0 || criteria.AccountTypes.Contains(l.AccountType))
                .Where(l => criteria.ShowZeroBalance || l.EndingDebitBalance != 0 || l.EndingCreditBalance != 0 || l.PeriodDebit != 0 || l.PeriodCredit != 0)
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
                    table.AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目名稱", 1.50f, Models.Reports.TextAlignment.Left)
                         .AddColumn("本期借方", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("本期貸方", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期末借方餘額", 0.90f, Models.Reports.TextAlignment.Right)
                         .AddColumn("期末貸方餘額", 0.90f, Models.Reports.TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    foreach (var item in group)
                    {
                        table.AddRow(
                            item.Code,
                            item.Name,
                            item.PeriodDebit > 0 ? item.PeriodDebit.ToString("N2") : string.Empty,
                            item.PeriodCredit > 0 ? item.PeriodCredit.ToString("N2") : string.Empty,
                            item.EndingDebitBalance > 0 ? item.EndingDebitBalance.ToString("N2") : string.Empty,
                            item.EndingCreditBalance > 0 ? item.EndingCreditBalance.ToString("N2") : string.Empty
                        );
                    }

                    // 小計
                    var gPeriodD = group.Sum(l => l.PeriodDebit);
                    var gPeriodC = group.Sum(l => l.PeriodCredit);
                    var gEndD = group.Sum(l => l.EndingDebitBalance);
                    var gEndC = group.Sum(l => l.EndingCreditBalance);

                    table.AddRow(
                        string.Empty,
                        $"{typeName} 小計",
                        gPeriodD > 0 ? gPeriodD.ToString("N2") : string.Empty,
                        gPeriodC > 0 ? gPeriodC.ToString("N2") : string.Empty,
                        gEndD > 0 ? gEndD.ToString("N2") : string.Empty,
                        gEndC > 0 ? gEndC.ToString("N2") : string.Empty
                    );
                });

                doc.AddSpacing(5);
            }

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalPeriodDebit = lines.Sum(l => l.PeriodDebit);
                var totalPeriodCredit = lines.Sum(l => l.PeriodCredit);
                var totalEndDebit = lines.Sum(l => l.EndingDebitBalance);
                var totalEndCredit = lines.Sum(l => l.EndingCreditBalance);
                var periodBalanced = totalPeriodDebit == totalPeriodCredit;
                var endingBalanced = totalEndDebit == totalEndCredit;

                var summaryLines = new List<string>
                {
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

            // 本期發生額（日期範圍篩選）
            public decimal PeriodDebit { get; set; }
            public decimal PeriodCredit { get; set; }

            // 累計餘額（從最早傳票到截止日）
            public decimal CumulativeDebit { get; set; }
            public decimal CumulativeCredit { get; set; }

            // 期末借貸餘額（依正常借貸方向計算）
            public decimal NetCumulative => NormalDirection == AccountDirection.Debit
                ? CumulativeDebit - CumulativeCredit
                : CumulativeCredit - CumulativeDebit;

            public decimal EndingDebitBalance => NetCumulative > 0 && NormalDirection == AccountDirection.Debit ? NetCumulative : 0;
            public decimal EndingCreditBalance => NetCumulative > 0 && NormalDirection == AccountDirection.Credit ? NetCumulative : 0;
        }
    }
}
