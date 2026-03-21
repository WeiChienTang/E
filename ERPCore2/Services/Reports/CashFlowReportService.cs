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

namespace ERPCore2.Services.Reports;

/// <summary>
/// 現金流量表報表服務（IAS 7 間接法）
///
/// 間接法編製邏輯：
///   1. 本期淨利（損）          ← 損益科目彙總（Revenue/Cost/Expense/NonOperating/ComprehensiveIncome）
///   2. 加：非現金調整項目       ← CashFlowCategory.OperatingNonCash 科目之正常方向餘額
///      （折舊、攤銷等非現金費用加回）
///   3. 加減：流動資金變動       ← CashFlowCategory.OperatingWorkingCapital 科目之 (貸方 - 借方)
///      資產增加 → 現金流出（負）；負債增加 → 現金流入（正）
///   = 營業活動淨現金流量
///   4. 投資活動淨現金流量       ← CashFlowCategory.Investing 科目之 (貸方 - 借方)
///   5. 籌資活動淨現金流量       ← CashFlowCategory.Financing 科目之 (貸方 - 借方)
///   = 本期現金淨變動
///   + 期初現金餘額             ← CashFlowCategory.Cash 科目，累計至期初前一天
///   = 期末現金餘額
///
/// CashFlowCategory 標記說明（OperatingNonCash）：
///   - 標記折舊費用科目（Debit-normal）或累計折舊科目（Credit-normal）均可
///   - 折舊費用科目（Debit-normal）：normal_balance = debit - credit = +20 ✓
///   - 累計折舊科目（Credit-normal）：normal_balance = credit - debit = +20 ✓
/// </summary>
public class CashFlowReportService : ICashFlowReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly ILogger<CashFlowReportService>? _logger;

    // 損益表科目大類（用於計算本期淨利/損）
    private static readonly AccountType[] IncomeStatementTypes =
    {
        AccountType.Revenue,
        AccountType.Cost,
        AccountType.Expense,
        AccountType.NonOperatingIncomeAndExpense,
        AccountType.ComprehensiveIncome
    };

    public CashFlowReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService,
        ILogger<CashFlowReportService>? logger = null)
    {
        _contextFactory = contextFactory;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CashFlowCriteria criteria)
    {
        try
        {
            if (!criteria.Validate(out var validationError))
                return BatchPreviewResult.Failure(validationError ?? "篩選條件無效");

            var data = await BuildCashFlowDataAsync(criteria);
            var company = criteria.CompanyId.HasValue
                ? await _companyService.GetByIdAsync(criteria.CompanyId.Value)
                : await _companyService.GetPrimaryCompanyAsync();

            var document = BuildCashFlowDocument(data, company, criteria);
            var images = criteria.PaperSetting != null
                ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                : _formattedPrintService.RenderToImages(document);

            return BatchPreviewResult.Success(images, document, 1, new List<FormattedDocument> { document });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "產生現金流量表時發生錯誤");
            return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
        }
    }

    // ===== 資料查詢 =====

    private async Task<CashFlowData> BuildCashFlowDataAsync(CashFlowCriteria criteria)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var primaryCompany = await _companyService.GetPrimaryCompanyAsync();
        var companyId = criteria.CompanyId ?? primaryCompany?.Id;

        var startDate      = criteria.StartDate!.Value.Date;
        var endDate        = criteria.EndDate!.Value.Date.AddDays(1).AddTicks(-1);
        var dayBeforeStart = startDate.AddTicks(-1);

        // ---- 1. 期間損益科目（計算本期淨利）----
        var incomeLines = await context.JournalEntryLines
            .Include(l => l.AccountItem)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted
                     && IncomeStatementTypes.Contains(l.AccountItem.AccountType)
                     && l.JournalEntry.EntryDate >= startDate
                     && l.JournalEntry.EntryDate <= endDate
                     && (!companyId.HasValue || l.JournalEntry.CompanyId == companyId.Value))
            .ToListAsync();

        // 淨利計算：
        //   收入科目（Credit-normal）: balance = credit - debit → 正值 = 收入
        //   成本/費用科目（Debit-normal）: balance = debit - credit → 正值 = 支出
        //   淨利 = 收入合計 - 支出合計
        var netIncome = incomeLines
            .GroupBy(l => l.AccountItemId)
            .Sum(g =>
            {
                var item = g.First().AccountItem;
                var d = g.Sum(l => l.Direction == AccountDirection.Debit  ? l.Amount : 0m);
                var c = g.Sum(l => l.Direction == AccountDirection.Credit ? l.Amount : 0m);
                var balance = item.Direction == AccountDirection.Credit ? c - d : d - c;
                // 收入類加入淨利；成本/費用類從淨利扣除
                return item.Direction == AccountDirection.Credit ? balance : -balance;
            });

        // ---- 2. 期間 CashFlowCategory 科目明細（非 Cash、非 Excluded）----
        var cfLines = await context.JournalEntryLines
            .Include(l => l.AccountItem)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted
                     && l.AccountItem.CashFlowCategory != null
                     && l.AccountItem.CashFlowCategory != CashFlowCategory.Cash
                     && l.AccountItem.CashFlowCategory != CashFlowCategory.Excluded
                     && l.JournalEntry.EntryDate >= startDate
                     && l.JournalEntry.EntryDate <= endDate
                     && (!companyId.HasValue || l.JournalEntry.CompanyId == companyId.Value))
            .ToListAsync();

        // ---- 3. 期初現金餘額（累計至 startDate 前一刻）----
        var openingCashLines = await context.JournalEntryLines
            .Include(l => l.AccountItem)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted
                     && l.AccountItem.CashFlowCategory == CashFlowCategory.Cash
                     && l.JournalEntry.EntryDate <= dayBeforeStart
                     && (!companyId.HasValue || l.JournalEntry.CompanyId == companyId.Value))
            .ToListAsync();

        // 彙整 CashFlowCategory 科目的期間活動
        var periodSummaries = cfLines
            .GroupBy(l => l.AccountItemId)
            .Select(g =>
            {
                var item = g.First().AccountItem;
                var totalDebit  = g.Sum(l => l.Direction == AccountDirection.Debit  ? l.Amount : 0m);
                var totalCredit = g.Sum(l => l.Direction == AccountDirection.Credit ? l.Amount : 0m);
                return new CashFlowLine
                {
                    AccountItemId   = item.Id,
                    Code            = item.Code ?? string.Empty,
                    Name            = item.Name,
                    Category        = item.CashFlowCategory!.Value,
                    NormalDirection = item.Direction,
                    TotalDebit      = totalDebit,
                    TotalCredit     = totalCredit
                };
            })
            .ToList();

        // 期初現金（Cash 科目通常為 Asset / Debit-normal）
        var openingCash = openingCashLines
            .GroupBy(l => l.AccountItemId)
            .Sum(g =>
            {
                var item = g.First().AccountItem;
                var d = g.Sum(l => l.Direction == AccountDirection.Debit  ? l.Amount : 0m);
                var c = g.Sum(l => l.Direction == AccountDirection.Credit ? l.Amount : 0m);
                return item.Direction == AccountDirection.Debit ? d - c : c - d;
            });

        // ---- 4. 確認是否有 Cash 科目已標記（不限日期，只查科目設定）----
        var hasCashAccounts = await context.AccountItems
            .AnyAsync(a => a.CashFlowCategory == CashFlowCategory.Cash
                        && a.Status == EntityStatus.Active);

        return new CashFlowData
        {
            NetIncome        = netIncome,
            PeriodLines      = periodSummaries,
            OpeningCash      = openingCash,
            StartDate        = startDate,
            EndDate          = criteria.EndDate!.Value.Date,
            HasCashAccounts  = hasCashAccounts
        };
    }

    // ===== 報表建構 =====

    [SupportedOSPlatform("windows6.1")]
    private FormattedDocument BuildCashFlowDocument(
        CashFlowData data,
        Company? company,
        CashFlowCriteria criteria)
    {
        var periodLabel = $"{data.StartDate:yyyy/MM/dd} ~ {data.EndDate:yyyy/MM/dd}";

        var doc = new FormattedDocument()
            .SetDocumentName($"現金流量表-{DateTime.Now:yyyyMMdd}")
            .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

        // === 頁首 ===
        doc.BeginHeader(header =>
        {
            header.AddReportHeaderBlock(
                centerLines: new List<(string, float, bool)>
                {
                    (company?.CompanyName ?? "公司名稱", 14f, true),
                    ("現  金  流  量  表", 16f, true),
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

        // === 現金科目未設定警告（在報表主體前顯示）===
        if (!data.HasCashAccounts)
        {
            doc.AddText("⚠ 警告：尚未設定現金及約當現金科目分類", 10f, Models.Reports.TextAlignment.Left, bold: true);
            doc.AddText("   請至「會計科目表」頁面，將現金相關科目（如庫存現金、活期存款等）的「現金流量分類」設為「現金及約當現金」。", 9f);
            doc.AddText("   未設定前，期初及期末現金餘額均顯示為零，報表數據不完整。", 9f);
            doc.AddSpacing(8);
        }

        // === 間接法主體 ===
        var nonCashLines    = data.PeriodLines.Where(l => l.Category == CashFlowCategory.OperatingNonCash).ToList();
        var workingCapLines = data.PeriodLines.Where(l => l.Category == CashFlowCategory.OperatingWorkingCapital).ToList();
        var investingLines  = data.PeriodLines.Where(l => l.Category == CashFlowCategory.Investing).ToList();
        var financingLines  = data.PeriodLines.Where(l => l.Category == CashFlowCategory.Financing).ToList();

        // 非現金調整：加回「正常方向餘額」
        var totalNonCash = nonCashLines.Sum(l =>
            l.NormalDirection == AccountDirection.Debit
                ? l.TotalDebit - l.TotalCredit
                : l.TotalCredit - l.TotalDebit);

        // 流動資金變動：一律 (credit - debit)
        var totalWorkingCapital = workingCapLines.Sum(l => l.TotalCredit - l.TotalDebit);

        var operatingCashFlow = data.NetIncome + totalNonCash + totalWorkingCapital;

        // 投資/籌資：credit - debit
        var totalInvesting = investingLines.Sum(l => l.TotalCredit - l.TotalDebit);
        var totalFinancing = financingLines.Sum(l => l.TotalCredit - l.TotalDebit);

        var netCashChange = operatingCashFlow + totalInvesting + totalFinancing;
        var closingCash   = data.OpeningCash + netCashChange;

        // === 一、營業活動 ===
        doc.AddText("一、營業活動之現金流量", 11f, Models.Reports.TextAlignment.Left, bold: true);
        doc.AddSpacing(2);
        doc.AddKeyValueRow(("    1. 本期淨利（損）", $"{data.NetIncome:N2}"));
        doc.AddSpacing(2);

        if (nonCashLines.Any())
        {
            doc.AddKeyValueRow(("    2. 加：非現金調整", $"{totalNonCash:N2}"));
            doc.AddSpacing(2);
            doc.AddTable(table =>
            {
                table.AddColumn(string.Empty, 0.60f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                     .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var line in nonCashLines)
                {
                    var amount = line.NormalDirection == AccountDirection.Debit
                        ? line.TotalDebit - line.TotalCredit
                        : line.TotalCredit - line.TotalDebit;
                    table.AddRow(string.Empty, line.Code, line.Name, amount.ToString("N2"));
                }
            });
            doc.AddSpacing(3);
        }

        if (workingCapLines.Any())
        {
            doc.AddKeyValueRow(("    3. 流動資金變動", $"{totalWorkingCapital:N2}"));
            doc.AddSpacing(2);
            doc.AddTable(table =>
            {
                table.AddColumn(string.Empty, 0.60f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                     .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var line in workingCapLines)
                {
                    var amount = line.TotalCredit - line.TotalDebit;
                    table.AddRow(string.Empty, line.Code, line.Name, amount.ToString("N2"));
                }
            });
            doc.AddSpacing(3);
        }

        doc.AddKeyValueRow(("    營業活動淨現金流量", $"{operatingCashFlow:N2}"));
        doc.AddSpacing(8);

        // === 二、投資活動 ===
        doc.AddText("二、投資活動之現金流量", 11f, Models.Reports.TextAlignment.Left, bold: true);
        doc.AddSpacing(2);

        if (investingLines.Any())
        {
            doc.AddTable(table =>
            {
                table.AddColumn(string.Empty, 0.60f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                     .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var line in investingLines)
                {
                    var amount = line.TotalCredit - line.TotalDebit;
                    table.AddRow(string.Empty, line.Code, line.Name, amount.ToString("N2"));
                }
            });
            doc.AddSpacing(3);
        }
        else
        {
            doc.AddKeyValueRow(("    （本期無投資活動）", string.Empty));
            doc.AddSpacing(3);
        }

        doc.AddKeyValueRow(("    投資活動淨現金流量", $"{totalInvesting:N2}"));
        doc.AddSpacing(8);

        // === 三、籌資活動 ===
        doc.AddText("三、籌資活動之現金流量", 11f, Models.Reports.TextAlignment.Left, bold: true);
        doc.AddSpacing(2);

        if (financingLines.Any())
        {
            doc.AddTable(table =>
            {
                table.AddColumn(string.Empty, 0.60f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目代碼", 0.70f, Models.Reports.TextAlignment.Left)
                     .AddColumn("科目名稱", 1.80f, Models.Reports.TextAlignment.Left)
                     .AddColumn("金額", 0.90f, Models.Reports.TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var line in financingLines)
                {
                    var amount = line.TotalCredit - line.TotalDebit;
                    table.AddRow(string.Empty, line.Code, line.Name, amount.ToString("N2"));
                }
            });
            doc.AddSpacing(3);
        }
        else
        {
            doc.AddKeyValueRow(("    （本期無籌資活動）", string.Empty));
            doc.AddSpacing(3);
        }

        doc.AddKeyValueRow(("    籌資活動淨現金流量", $"{totalFinancing:N2}"));
        doc.AddSpacing(8);

        // === 四、現金淨變動 ===
        doc.AddLine(LineStyle.Solid, 1);
        doc.AddSpacing(3);
        doc.AddKeyValueRow(("本期現金及約當現金淨增加（減少）", $"{netCashChange:N2}"));
        doc.AddSpacing(5);
        doc.AddKeyValueRow(("期初現金及約當現金餘額", $"{data.OpeningCash:N2}"));
        doc.AddSpacing(3);
        doc.AddKeyValueRow(("期末現金及約當現金餘額", $"{closingCash:N2}"));
        doc.AddSpacing(8);

        // === 附註：尚未標記任何非現金/流動/投資/籌資科目時的提示 ===
        if (!data.PeriodLines.Any())
        {
            doc.AddSpacing(10);
            doc.AddText("* 備註：本期尚無已標記現金流量分類的科目發生交易。", 9f);
            doc.AddText("  請至「會計科目表」頁面，為各科目設定現金流量分類（CashFlowCategory），", 9f);
            doc.AddText("  現金流量表才能正確反映各項活動。", 9f);
        }

        // === 頁尾 ===
        doc.BeginFooter(footer =>
        {
            footer.AddSpacing(10);

            var summaryLines = new List<string>
            {
                $"營業活動淨現金流量：{operatingCashFlow:N2}",
                $"投資活動淨現金流量：{totalInvesting:N2}",
                $"籌資活動淨現金流量：{totalFinancing:N2}",
                $"本期現金淨變動：{netCashChange:N2}",
                $"期末現金餘額：{closingCash:N2}",
                string.Empty
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

    private class CashFlowData
    {
        public decimal NetIncome           { get; set; }
        public List<CashFlowLine> PeriodLines { get; set; } = new();
        public decimal OpeningCash         { get; set; }
        public DateTime StartDate          { get; set; }
        public DateTime EndDate            { get; set; }
        /// <summary>是否有任何科目標記為 CashFlowCategory.Cash（期末現金餘額是否可信）</summary>
        public bool HasCashAccounts        { get; set; }
    }

    private class CashFlowLine
    {
        public int AccountItemId           { get; set; }
        public string Code                 { get; set; } = string.Empty;
        public string Name                 { get; set; } = string.Empty;
        public CashFlowCategory Category   { get; set; }
        public AccountDirection NormalDirection { get; set; }
        public decimal TotalDebit          { get; set; }
        public decimal TotalCredit         { get; set; }
    }
}
