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
    /// 資產負債表報表服務實作
    /// 彙總累積至截止日（EndDate）的 Asset/Liability/Equity 科目餘額
    /// 資產負債表恆等式：資產合計 = 負債合計 + 權益合計
    /// </summary>
    public class BalanceSheetReportService : IBalanceSheetReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<BalanceSheetReportService>? _logger;

        // 資產負債表相關 AccountType
        private static readonly AccountType[] BalanceSheetTypes =
        {
            AccountType.Asset,
            AccountType.Liability,
            AccountType.Equity
        };

        public BalanceSheetReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<BalanceSheetReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BalanceSheetCriteria criteria)
        {
            try
            {
                var accountLines = await BuildAccountLinesAsync(criteria);

                if (!accountLines.Any())
                    return BatchPreviewResult.Failure($"無符合條件的傳票資料\n截止日：{criteria.AsOfDate:yyyy/MM/dd}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildBalanceSheetDocument(accountLines, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, accountLines.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生資產負債表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        // ===== 資料查詢 =====

        private async Task<List<AccountSummaryLine>> BuildAccountLinesAsync(BalanceSheetCriteria criteria)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 資產負債表：累積至截止日（若有 StartDate，則從 StartDate 起累計）
            var asOfDate = criteria.AsOfDate.Date.AddDays(1).AddTicks(-1);

            var query = context.JournalEntryLines
                .Include(l => l.AccountItem)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted
                         && BalanceSheetTypes.Contains(l.AccountItem.AccountType)
                         && l.JournalEntry.EntryDate <= asOfDate);

            if (criteria.StartDate.HasValue)
                query = query.Where(l => l.JournalEntry.EntryDate >= criteria.StartDate.Value.Date);

            var lines = await query.ToListAsync();

            return lines
                .GroupBy(l => l.AccountItemId)
                .Select(g =>
                {
                    var item = g.First().AccountItem;
                    var totalDebit = g.Where(l => l.Direction == AccountDirection.Debit).Sum(l => l.Amount);
                    var totalCredit = g.Where(l => l.Direction == AccountDirection.Credit).Sum(l => l.Amount);

                    // Asset → Debit normal: balance = Debit - Credit
                    // Liability/Equity → Credit normal: balance = Credit - Debit
                    var balance = item.Direction == AccountDirection.Debit
                        ? totalDebit - totalCredit
                        : totalCredit - totalDebit;

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
        private FormattedDocument BuildBalanceSheetDocument(
            List<AccountSummaryLine> accountLines,
            Company? company,
            BalanceSheetCriteria criteria)
        {
            var asOfLabel = criteria.AsOfDate.ToString("yyyy/MM/dd");

            var doc = new FormattedDocument()
                .SetDocumentName($"資產負債表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("資  產  負  債  表", 16f, true),
                        ($"截止日期：{asOfLabel}", 10f, false)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);
                header.AddSpacing(5);
            });

            // === 資產負債表主體 ===
            var assetLines     = accountLines.Where(l => l.AccountType == AccountType.Asset).ToList();
            var liabilityLines = accountLines.Where(l => l.AccountType == AccountType.Liability).ToList();
            var equityLines    = accountLines.Where(l => l.AccountType == AccountType.Equity).ToList();

            var totalAssets      = assetLines.Sum(l => l.Balance);
            var totalLiabilities = liabilityLines.Sum(l => l.Balance);
            var totalEquity      = equityLines.Sum(l => l.Balance);
            var totalLiabEquity  = totalLiabilities + totalEquity;

            // 科目明細表格（helper lambda）
            void AddSection(string sectionTitle, List<AccountSummaryLine> items, decimal total)
            {
                doc.AddKeyValueRow((sectionTitle, $"{total:N2}"));
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
                doc.AddSpacing(6);
            }

            // 資產
            AddSection("【資產】", assetLines, totalAssets);
            doc.AddKeyValueRow(("資產合計", $"{totalAssets:N2}"));
            doc.AddSpacing(10);

            // 負債
            AddSection("【負債】", liabilityLines, totalLiabilities);
            doc.AddKeyValueRow(("負債合計", $"{totalLiabilities:N2}"));
            doc.AddSpacing(10);

            // 權益
            AddSection("【權益】", equityLines, totalEquity);
            doc.AddKeyValueRow(("權益合計", $"{totalEquity:N2}"));
            doc.AddSpacing(10);

            // 負債及權益合計
            doc.AddKeyValueRow(("負債及權益合計", $"{totalLiabEquity:N2}"));
            doc.AddSpacing(5);

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var balanced = Math.Abs(totalAssets - totalLiabEquity) < 0.01m;
                var balanceNote = balanced
                    ? "✓ 借貸平衡（資產 = 負債 + 權益）"
                    : $"⚠ 差額：{totalAssets - totalLiabEquity:N2}（資產 - 負債 - 權益）";

                var summaryLines = new List<string>
                {
                    $"資產合計：{totalAssets:N2}",
                    $"負債合計：{totalLiabilities:N2}",
                    $"權益合計：{totalEquity:N2}",
                    $"負債及權益合計：{totalLiabEquity:N2}",
                    "",
                    balanceNote
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.65f);

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
