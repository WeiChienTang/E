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
    /// 損益表報表服務實作
    /// 彙總指定期間的 Revenue/Cost/Expense/NonOperating 科目，計算毛利、營業損益、稅前損益
    /// </summary>
    public class IncomeStatementReportService : IIncomeStatementReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<IncomeStatementReportService>? _logger;

        // 損益表相關 AccountType
        private static readonly AccountType[] IncomeStatementTypes =
        {
            AccountType.Revenue,
            AccountType.Cost,
            AccountType.Expense,
            AccountType.NonOperatingIncomeAndExpense
        };

        public IncomeStatementReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<IncomeStatementReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(IncomeStatementCriteria criteria)
        {
            try
            {
                var accountLines = await BuildAccountLinesAsync(criteria);

                if (!accountLines.Any())
                    return BatchPreviewResult.Failure($"無符合條件的傳票資料\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildIncomeStatementDocument(accountLines, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, accountLines.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生損益表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        // ===== 資料查詢 =====

        private async Task<List<AccountSummaryLine>> BuildAccountLinesAsync(IncomeStatementCriteria criteria)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted
                         && IncomeStatementTypes.Contains(l.AccountItem.AccountType));

            if (criteria.StartDate.HasValue)
                query = query.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);
            if (criteria.EndDate.HasValue)
                query = query.Where(l => l.JournalEntry.EntryDate <= criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1));

            var lines = await query.ToListAsync();

            return lines
                .GroupBy(l => l.AccountItemId)
                .Select(g =>
                {
                    var item = g.First().AccountItem;
                    var totalDebit = g.Where(l => l.Direction == AccountDirection.Debit).Sum(l => l.Amount);
                    var totalCredit = g.Where(l => l.Direction == AccountDirection.Credit).Sum(l => l.Amount);

                    // 各科目大類的正常餘額方向：
                    // Revenue (4) → Credit normal: balance = Credit - Debit
                    // Cost (5), Expense (6) → Debit normal: balance = Debit - Credit
                    // NonOperating (7) → depends on account Direction
                    var balance = item.Direction == AccountDirection.Credit
                        ? totalCredit - totalDebit
                        : totalDebit - totalCredit;

                    return new AccountSummaryLine
                    {
                        AccountItemId = item.Id,
                        Code = item.Code ?? string.Empty,
                        Name = item.Name,
                        AccountType = item.AccountType,
                        NormalDirection = item.Direction,
                        SortOrder = item.SortOrder,
                        Balance = balance
                    };
                })
                .Where(l => l.Balance != 0)
                .OrderBy(l => l.AccountType)
                .ThenBy(l => l.SortOrder)
                .ThenBy(l => l.Code)
                .ToList();
        }

        // ===== 報表建構 =====

        [SupportedOSPlatform("windows6.1")]
        private FormattedDocument BuildIncomeStatementDocument(
            List<AccountSummaryLine> accountLines,
            Company? company,
            IncomeStatementCriteria criteria)
        {
            var periodLabel = criteria.StartDate.HasValue && criteria.EndDate.HasValue
                ? $"{criteria.StartDate:yyyy/MM/dd} ~ {criteria.EndDate:yyyy/MM/dd}"
                : criteria.EndDate.HasValue
                    ? $"截至 {criteria.EndDate:yyyy/MM/dd}"
                    : $"截至 {DateTime.Today:yyyy/MM/dd}";

            var doc = new FormattedDocument()
                .SetDocumentName($"損益表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("損  益  表", 16f, true),
                        ($"期間：{periodLabel}", 10f, false)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);
                header.AddSpacing(5);
            });

            // === 損益表主體 ===
            var revenueLines   = accountLines.Where(l => l.AccountType == AccountType.Revenue).ToList();
            var costLines      = accountLines.Where(l => l.AccountType == AccountType.Cost).ToList();
            var expenseLines   = accountLines.Where(l => l.AccountType == AccountType.Expense).ToList();
            var nonOpLines     = accountLines.Where(l => l.AccountType == AccountType.NonOperatingIncomeAndExpense).ToList();

            var totalRevenue    = revenueLines.Sum(l => l.Balance);
            var totalCost       = costLines.Sum(l => l.Balance);
            var grossProfit     = totalRevenue - totalCost;
            var totalExpense    = expenseLines.Sum(l => l.Balance);
            var operatingIncome = grossProfit - totalExpense;

            // 營業外：收益為正（Credit-normal），費損為正（Debit-normal）
            // 計算方式：各科目依自身 NormalDirection 已算出 Balance（正值）
            // 但在損益表中，需區分「收益」（加）和「費損」（減）
            var nonOpIncome  = nonOpLines.Where(l => l.NormalDirection == AccountDirection.Credit).Sum(l => l.Balance);
            var nonOpExpense = nonOpLines.Where(l => l.NormalDirection == AccountDirection.Debit).Sum(l => l.Balance);
            var netNonOp     = nonOpIncome - nonOpExpense;
            var preTaxIncome = operatingIncome + netNonOp;

            // 建立科目明細表格（helper lambda）
            void AddAccountTable(List<AccountSummaryLine> items, string header, decimal total, bool isDeduction)
            {
                doc.AddKeyValueRow((header, $"{(isDeduction ? "-  " : "")}{total:N2}"));
                doc.AddSpacing(2);

                if (items.Any())
                {
                    doc.AddTable(table =>
                    {
                        table.AddColumn(string.Empty, 0.30f, Models.Reports.TextAlignment.Left)
                             .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                             .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                             .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                             .ShowBorder(false)
                             .ShowHeaderBackground(false)
                             .ShowHeaderSeparator(false)
                             .SetRowHeight(18);

                        foreach (var item in items)
                        {
                            table.AddRow(
                                string.Empty,
                                item.Code,
                                item.Name,
                                item.Balance.ToString("N2")
                            );
                        }
                    });
                }
                doc.AddSpacing(5);
            }

            // 一、營業收入
            AddAccountTable(revenueLines, "一、銷貨收入", totalRevenue, false);

            // 二、減：營業成本
            AddAccountTable(costLines, "二、減：銷貨成本", totalCost, true);

            // 毛利潤
            doc.AddKeyValueRow(("毛利潤", $"{grossProfit:N2}"));
            doc.AddSpacing(8);

            // 三、減：營業費用
            AddAccountTable(expenseLines, "三、減：營業費用", totalExpense, true);

            // 營業損益
            doc.AddKeyValueRow(("營業損益", $"{operatingIncome:N2}"));
            doc.AddSpacing(8);

            // 四、營業外
            if (nonOpLines.Any())
            {
                var nonOpIncomeLines  = nonOpLines.Where(l => l.NormalDirection == AccountDirection.Credit).ToList();
                var nonOpExpenseLines = nonOpLines.Where(l => l.NormalDirection == AccountDirection.Debit).ToList();

                doc.AddKeyValueRow(("四、營業外收益及費損", $"{netNonOp:N2}"));
                doc.AddSpacing(2);

                doc.AddTable(table =>
                {
                    table.AddColumn(string.Empty, 0.30f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                         .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(18);

                    foreach (var item in nonOpIncomeLines)
                        table.AddRow(string.Empty, item.Code, item.Name, $"+{item.Balance:N2}");
                    foreach (var item in nonOpExpenseLines)
                        table.AddRow(string.Empty, item.Code, item.Name, $"-{item.Balance:N2}");
                });
                doc.AddSpacing(5);
            }

            // 稅前損益
            doc.AddKeyValueRow(("稅前損益", $"{preTaxIncome:N2}"));
            doc.AddSpacing(5);

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var summaryLines = new List<string>
                {
                    $"銷貨收入：{totalRevenue:N2}",
                    $"銷貨成本：{totalCost:N2}",
                    $"毛利潤：{grossProfit:N2}",
                    $"營業費用：{totalExpense:N2}",
                    $"營業損益：{operatingIncome:N2}",
                    $"稅前損益：{preTaxIncome:N2}",
                    ""
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "財務主管" });
            });

            return doc;
        }

        // ===== 內部資料模型 =====

        private class AccountSummaryLine
        {
            public int AccountItemId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public AccountType AccountType { get; set; }
            public AccountDirection NormalDirection { get; set; }
            public int SortOrder { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
