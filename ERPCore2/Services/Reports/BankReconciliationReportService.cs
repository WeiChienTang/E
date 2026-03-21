using System.Runtime.Versioning;
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Reports;

/// <summary>
/// 銀行存款餘額調節表報表服務
///
/// 報表結構（每份對帳單一頁區塊）：
///   標頭：公司名稱、銀行帳號、對帳期間
///   ① 對帳單概況：期初/期末餘額、本期借/貸合計
///   ② 配對狀況：總筆數 / 已配對 / 未配對
///   ③ 已配對明細：交易日期、說明、借方、貸方、傳票編號
///   ④ 未配對明細（需補開傳票）：交易日期、說明、借方、貸方
/// </summary>
public class BankReconciliationReportService : IBankReconciliationReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly ILogger<BankReconciliationReportService>? _logger;

    public BankReconciliationReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService,
        ILogger<BankReconciliationReportService>? logger = null)
    {
        _contextFactory = contextFactory;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BankReconciliationCriteria criteria)
    {
        try
        {
            if (!criteria.Validate(out var validationError))
                return BatchPreviewResult.Failure(validationError ?? "篩選條件無效");

            var statements = await BuildStatementsDataAsync(criteria);

            if (!statements.Any())
                return BatchPreviewResult.Failure($"查無符合條件的銀行對帳單\n篩選條件：{criteria.GetSummary()}");

            var company = criteria.CompanyId.HasValue
                ? await _companyService.GetByIdAsync(criteria.CompanyId.Value)
                : await _companyService.GetPrimaryCompanyAsync();

            var documents = new List<FormattedDocument>();
            foreach (var stmt in statements)
            {
                var doc = BuildStatementDocument(stmt, company, criteria);
                documents.Add(doc);
            }

            // 合併所有對帳單為單一預覽文件
            var merged = new FormattedDocument()
                .SetDocumentName($"銀行存款餘額調節表-{criteria.PeriodStart:yyyyMM}")
                .SetMargins(1.5f, 0.5f, 1.5f, 0.5f);

            for (int i = 0; i < documents.Count; i++)
            {
                merged.MergeFrom(documents[i]);
                if (i < documents.Count - 1)
                    merged.AddPageBreak();
            }

            var images = criteria.PaperSetting != null
                ? _formattedPrintService.RenderToImages(merged, criteria.PaperSetting)
                : _formattedPrintService.RenderToImages(merged);

            return BatchPreviewResult.Success(images, merged, statements.Count, documents);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "產生銀行存款餘額調節表時發生錯誤");
            return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
        }
    }

    // ===== 資料查詢 =====

    private async Task<List<BankStatement>> BuildStatementsDataAsync(BankReconciliationCriteria criteria)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.BankStatements
            .Include(bs => bs.Company)
            .Include(bs => bs.CompanyBankAccount)
                .ThenInclude(cba => cba.Bank)
            .Include(bs => bs.Lines.OrderBy(l => l.SortOrder).ThenBy(l => l.TransactionDate))
                .ThenInclude(l => l.MatchedJournalEntryLine)
                    .ThenInclude(jel => jel!.JournalEntry)
            .AsQueryable();

        if (criteria.CompanyId.HasValue)
            query = query.Where(bs => bs.CompanyId == criteria.CompanyId.Value);

        if (criteria.PeriodStart.HasValue)
            query = query.Where(bs => bs.PeriodStart >= criteria.PeriodStart.Value);

        if (criteria.PeriodEnd.HasValue)
            query = query.Where(bs => bs.PeriodEnd <= criteria.PeriodEnd.Value.AddDays(1));

        return await query
            .OrderBy(bs => bs.CompanyBankAccount.AccountNumber)
            .ThenBy(bs => bs.PeriodStart)
            .ToListAsync();
    }

    // ===== 文件建立 =====

    [SupportedOSPlatform("windows6.1")]
    private FormattedDocument BuildStatementDocument(
        BankStatement stmt,
        Company? company,
        BankReconciliationCriteria criteria)
    {
        var doc = new FormattedDocument()
            .SetDocumentName($"銀行存款餘額調節表");

        var bankAccount = stmt.CompanyBankAccount;
        var bankName = bankAccount?.Bank?.BankName ?? "";
        var accountDisplay = $"{bankName} {bankAccount?.AccountNumber ?? ""} ({bankAccount?.AccountName ?? ""})";
        var period = $"{stmt.PeriodStart:yyyy/MM/dd} ~ {stmt.PeriodEnd:yyyy/MM/dd}";
        var statusText = stmt.StatementStatus switch
        {
            Models.Enums.BankStatementStatus.Draft => "草稿",
            Models.Enums.BankStatementStatus.InProgress => "對帳中",
            Models.Enums.BankStatementStatus.Completed => "已完成",
            _ => ""
        };

        // ── 頁首 ──
        doc.BeginHeader(header =>
        {
            header.AddReportHeaderBlock(
                centerLines: new List<(string, float, bool)>
                {
                    (company?.CompanyName ?? "公司名稱", 12f, false),
                    ("銀行存款餘額調節表", 16f, true),
                    ($"對帳期間：{period}", 10f, false)
                },
                rightLines: new List<string>
                {
                    $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                    $"對帳狀態：{statusText}",
                    "頁次：{PAGE}/{PAGES}"
                },
                rightFontSize: 10f);
            header.AddSpacing(4);
        });

        // ── 銀行帳號資訊 ──
        doc.AddKeyValueRow(
            ("銀行帳號", accountDisplay),
            ("對帳日期", stmt.StatementDate.ToString("yyyy/MM/dd")));

        doc.AddSpacing(4);

        // ── ① 對帳單概況 ──
        var lines = stmt.Lines.ToList();
        var totalDebit = lines.Sum(l => l.DebitAmount);
        var totalCredit = lines.Sum(l => l.CreditAmount);

        doc.AddText("【對帳單概況】", 10f, TextAlignment.Left, bold: true);
        doc.AddTable(table =>
        {
            table.AddColumn("項目", 2.0f, TextAlignment.Left)
                 .AddColumn("金額", 1.8f, TextAlignment.Right)
                 .AddColumn("項目", 2.0f, TextAlignment.Left)
                 .AddColumn("金額", 1.8f, TextAlignment.Right)
                 .ShowBorder(true)
                 .ShowHeaderBackground(false)
                 .ShowHeaderSeparator(false)
                 .SetRowHeight(18);

            table.AddRow("期初餘額", stmt.OpeningBalance.ToString("N2"),
                         "本期借方（支出）合計", totalDebit.ToString("N2"));
            table.AddRow("期末餘額", stmt.ClosingBalance.ToString("N2"),
                         "本期貸方（收入）合計", totalCredit.ToString("N2"));
        });

        doc.AddSpacing(6);

        // ── ② 配對狀況摘要 ──
        var matchedLines = lines.Where(l => l.IsMatched).ToList();
        var unmatchedLines = lines.Where(l => !l.IsMatched).ToList();

        doc.AddText("【配對狀況摘要】", 10f, TextAlignment.Left, bold: true);
        doc.AddTable(table =>
        {
            table.AddColumn("明細行總數", 2.0f, TextAlignment.Center)
                 .AddColumn("已配對", 2.0f, TextAlignment.Center)
                 .AddColumn("未配對", 2.0f, TextAlignment.Center)
                 .AddColumn("配對率", 1.6f, TextAlignment.Center)
                 .ShowBorder(true)
                 .ShowHeaderBackground(true)
                 .ShowHeaderSeparator(true)
                 .SetRowHeight(18);

            var matchRate = lines.Count > 0
                ? $"{(double)matchedLines.Count / lines.Count:P0}"
                : "N/A";
            table.AddRow(
                $"{lines.Count} 筆",
                $"{matchedLines.Count} 筆",
                $"{unmatchedLines.Count} 筆",
                matchRate);
        });

        doc.AddSpacing(8);

        // ── ③ 已配對明細 ──
        doc.AddText("一、已配對明細", 10f, TextAlignment.Left, bold: true);
        doc.AddSpacing(2);

        if (!matchedLines.Any())
        {
            doc.AddText("（無已配對明細）", 9f, TextAlignment.Left);
        }
        else
        {
            doc.AddTable(table =>
            {
                table.AddColumn("交易日期", 0.90f, TextAlignment.Center)
                     .AddColumn("說明", 2.60f, TextAlignment.Left)
                     .AddColumn("借方（支出）", 1.15f, TextAlignment.Right)
                     .AddColumn("貸方（收入）", 1.15f, TextAlignment.Right)
                     .AddColumn("傳票編號", 1.00f, TextAlignment.Center)
                     .AddColumn("傳票日期", 0.90f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(true)
                     .ShowHeaderSeparator(true)
                     .SetRowHeight(18);

                decimal subDebit = 0, subCredit = 0;
                foreach (var line in matchedLines)
                {
                    var entry = line.MatchedJournalEntryLine?.JournalEntry;
                    var entryCode = entry?.Code ?? "";
                    var entryDate = entry != null ? entry.EntryDate.ToString("yyyy/MM/dd") : "";

                    table.AddRow(
                        line.TransactionDate.ToString("yyyy/MM/dd"),
                        line.Description,
                        line.DebitAmount > 0 ? line.DebitAmount.ToString("N2") : "",
                        line.CreditAmount > 0 ? line.CreditAmount.ToString("N2") : "",
                        entryCode,
                        entryDate);

                    subDebit += line.DebitAmount;
                    subCredit += line.CreditAmount;
                }

                table.AddRow(
                    "", "小計",
                    subDebit > 0 ? subDebit.ToString("N2") : "",
                    subCredit > 0 ? subCredit.ToString("N2") : "",
                    "", "");
            });
        }

        doc.AddSpacing(8);

        // ── ④ 未配對明細 ──
        doc.AddText("二、未配對明細（需補開傳票）", 10f, TextAlignment.Left, bold: true);
        doc.AddSpacing(2);

        if (!unmatchedLines.Any())
        {
            doc.AddText("（無未配對明細，對帳完成）", 9f, TextAlignment.Left);
        }
        else
        {
            doc.AddTable(table =>
            {
                table.AddColumn("交易日期", 0.90f, TextAlignment.Center)
                     .AddColumn("說明", 3.80f, TextAlignment.Left)
                     .AddColumn("借方（支出）", 1.15f, TextAlignment.Right)
                     .AddColumn("貸方（收入）", 1.15f, TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(true)
                     .ShowHeaderSeparator(true)
                     .SetRowHeight(18);

                decimal subDebit = 0, subCredit = 0;
                foreach (var line in unmatchedLines)
                {
                    table.AddRow(
                        line.TransactionDate.ToString("yyyy/MM/dd"),
                        line.Description,
                        line.DebitAmount > 0 ? line.DebitAmount.ToString("N2") : "",
                        line.CreditAmount > 0 ? line.CreditAmount.ToString("N2") : "");

                    subDebit += line.DebitAmount;
                    subCredit += line.CreditAmount;
                }

                table.AddRow(
                    "", "小計",
                    subDebit > 0 ? subDebit.ToString("N2") : "",
                    subCredit > 0 ? subCredit.ToString("N2") : "");
            });
        }

        doc.AddSpacing(8);

        // ── 差異摘要 ──
        doc.AddLine();
        doc.AddSpacing(4);

        var adjBalance = stmt.ClosingBalance - unmatchedLines.Sum(l => l.CreditAmount - l.DebitAmount);
        var bookBalance = stmt.OpeningBalance + totalCredit - totalDebit
                          - unmatchedLines.Sum(l => l.CreditAmount - l.DebitAmount);

        doc.AddTwoColumnSection(
            leftContent: new List<string>
            {
                $"銀行對帳單期末餘額：{stmt.ClosingBalance:N2}",
                $"  減：未配對貸方（銀行已收/帳上未記）：({unmatchedLines.Sum(l => l.CreditAmount):N2})",
                $"  加：未配對借方（銀行已扣/帳上未記）：{unmatchedLines.Sum(l => l.DebitAmount):N2}",
                $"調整後帳面應有餘額：{adjBalance:N2}"
            },
            leftTitle: "【差異分析】",
            leftHasBorder: true,
            rightContent: new List<string>
            {
                unmatchedLines.Any()
                    ? $"⚠ 尚有 {unmatchedLines.Count} 筆未配對明細，請補開傳票"
                    : "✓ 全部明細已配對，對帳完成"
            },
            leftWidthRatio: 0.65f);

        // ── 頁尾簽名 ──
        doc.BeginFooter(footer =>
        {
            footer.AddSpacing(15);
            footer.AddSignatureSection("製表人員", "財務主管", "審核");
        });

        return doc;
    }
}
