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
/// 應收帳款報表服務
/// 計算邏輯：
///   應收金額 = SalesDelivery.TotalAmount + TaxAmount（含稅）
///   已收金額 = SUM(SetoffItemDetail.CurrentSetoffAmount + CurrentAllowanceAmount) WHERE SourceDetailType=SalesDeliveryDetail
///   未收餘額 = 應收金額 - 已收金額
///   到期日   = DeliveryDate + Customer.PaymentDays
///   逾期天數 = Today - 到期日（正值表示逾期）
/// </summary>
public class AccountsReceivableReportService : IAccountsReceivableReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly ILogger<AccountsReceivableReportService>? _logger;

    public AccountsReceivableReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService,
        ILogger<AccountsReceivableReportService>? logger = null)
    {
        _contextFactory = contextFactory;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(AccountsReceivableCriteria criteria)
    {
        try
        {
            var lines = await BuildReceivableLinesAsync(criteria);

            if (!lines.Any())
                return BatchPreviewResult.Failure($"無符合條件的應收帳款資料\n篩選條件：{criteria.GetSummary()}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            var document = BuildReceivableDocument(lines, company, criteria);

            var images = criteria.PaperSetting != null
                ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                : _formattedPrintService.RenderToImages(document);

            return BatchPreviewResult.Success(images, document, lines.Count, new List<FormattedDocument> { document });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "產生應收帳款報表時發生錯誤");
            return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
        }
    }

    // ===== 資料查詢 =====

    private async Task<List<ARLine>> BuildReceivableLinesAsync(AccountsReceivableCriteria criteria)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var today = DateTime.Today;

        // 查詢出貨單（含客戶與業務篩選）
        var deliveryQuery = context.SalesDeliveries
            .Include(sd => sd.Customer)
            .Include(sd => sd.Salesperson)
            .Where(sd => sd.Status == EntityStatus.Active);

        if (criteria.StartDate.HasValue)
            deliveryQuery = deliveryQuery.Where(sd => sd.DeliveryDate >= criteria.StartDate.Value.Date);

        if (criteria.EndDate.HasValue)
            deliveryQuery = deliveryQuery.Where(sd => sd.DeliveryDate <= criteria.EndDate.Value.Date);

        if (criteria.CustomerIds.Any())
            deliveryQuery = deliveryQuery.Where(sd => sd.CustomerId.HasValue && criteria.CustomerIds.Contains(sd.CustomerId.Value));

        if (criteria.EmployeeIds.Any())
            deliveryQuery = deliveryQuery.Where(sd => sd.SalespersonId.HasValue && criteria.EmployeeIds.Contains(sd.SalespersonId.Value));

        var deliveries = await deliveryQuery
            .Select(sd => new
            {
                sd.Id,
                sd.Code,
                sd.DeliveryDate,
                sd.CustomerId,
                CustomerName = sd.Customer != null ? (sd.Customer.CompanyName ?? sd.Customer.Code ?? sd.Customer.Id.ToString()) : "未知客戶",
                CustomerPaymentDays = sd.Customer != null ? sd.Customer.PaymentDays : 30,
                SalespersonName = sd.Salesperson != null ? (sd.Salesperson.Name ?? sd.Salesperson.Code ?? "") : "",
                TaxInclusiveAmount = sd.TotalAmount + sd.TaxAmount,
                DetailIds = sd.DeliveryDetails.Select(d => d.Id).ToList()
            })
            .ToListAsync();

        if (!deliveries.Any())
            return new List<ARLine>();

        // 批次查詢所有相關沖款明細
        var allDetailIds = deliveries.SelectMany(d => d.DetailIds).ToList();
        var setoffMap = new Dictionary<int, (decimal setoff, decimal allowance)>();

        if (allDetailIds.Any())
        {
            var setoffItems = await context.SetoffItemDetails
                .Where(s => s.SourceDetailType == SetoffDetailType.SalesDeliveryDetail
                         && allDetailIds.Contains(s.SourceDetailId))
                .Select(s => new { s.SourceDetailId, s.CurrentSetoffAmount, s.CurrentAllowanceAmount })
                .ToListAsync();

            foreach (var item in setoffItems)
            {
                if (setoffMap.ContainsKey(item.SourceDetailId))
                {
                    var existing = setoffMap[item.SourceDetailId];
                    setoffMap[item.SourceDetailId] = (existing.setoff + item.CurrentSetoffAmount,
                                                       existing.allowance + item.CurrentAllowanceAmount);
                }
                else
                {
                    setoffMap[item.SourceDetailId] = (item.CurrentSetoffAmount, item.CurrentAllowanceAmount);
                }
            }
        }

        var result = new List<ARLine>();

        foreach (var d in deliveries)
        {
            var settledAmount = d.DetailIds.Sum(id =>
            {
                if (setoffMap.TryGetValue(id, out var amounts))
                    return amounts.setoff + amounts.allowance;
                return 0m;
            });
            var outstandingAmount = d.TaxInclusiveAmount - settledAmount;

            // 過濾已結清
            if (!criteria.IncludeSettled && outstandingAmount <= 0)
                continue;

            var dueDate = d.DeliveryDate.AddDays(d.CustomerPaymentDays);
            var daysOverdue = (int)(today - dueDate).TotalDays;

            // 過濾非逾期
            if (criteria.OnlyOverdue && daysOverdue <= 0)
                continue;

            if (outstandingAmount < 0)
                outstandingAmount = 0;

            result.Add(new ARLine
            {
                DeliveryCode = d.Code ?? string.Empty,
                DeliveryDate = d.DeliveryDate,
                DueDate = dueDate,
                DaysOverdue = daysOverdue,
                CustomerName = d.CustomerName,
                SalespersonName = d.SalespersonName,
                TaxInclusiveAmount = d.TaxInclusiveAmount,
                SettledAmount = settledAmount,
                OutstandingAmount = outstandingAmount,
                CustomerId = d.CustomerId ?? 0
            });
        }

        return result
            .OrderBy(l => l.CustomerName)
            .ThenBy(l => l.DeliveryDate)
            .ToList();
    }

    // ===== 報表建構 =====

    [SupportedOSPlatform("windows6.1")]
    private FormattedDocument BuildReceivableDocument(
        List<ARLine> lines,
        Company? company,
        AccountsReceivableCriteria criteria)
    {
        var doc = new FormattedDocument()
            .SetDocumentName($"應收帳款報表-{DateTime.Now:yyyyMMdd}")
            .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

        // === 頁首 ===
        var dateRangeText = criteria.StartDate.HasValue || criteria.EndDate.HasValue
            ? $"期間：{criteria.StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {criteria.EndDate?.ToString("yyyy/MM/dd") ?? "不限"}"
            : $"截止日期：{DateTime.Today:yyyy/MM/dd}";

        doc.BeginHeader(header =>
        {
            header.AddReportHeaderBlock(
                centerLines: new List<(string, float, bool)>
                {
                    (company?.CompanyName ?? "公司名稱", 14f, true),
                    ("應收帳款明細表", 16f, true),
                    (dateRangeText, 10f, false)
                },
                rightLines: new List<string>
                {
                    $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                    $"筆數：{lines.Count}",
                    "頁次：{PAGE}/{PAGES}"
                },
                rightFontSize: 10f);
            header.AddSpacing(5);
        });

        // === 依客戶分組 ===
        var customerGroups = lines
            .GroupBy(l => l.CustomerName)
            .OrderBy(g => g.Key);

        decimal grandTotalAmount = 0, grandSettledAmount = 0, grandOutstandingAmount = 0;

        foreach (var group in customerGroups)
        {
            doc.AddKeyValueRow(("客戶", group.Key));
            doc.AddSpacing(2);

            doc.AddTable(table =>
            {
                table.AddColumn("出貨單號", 1.10f, TextAlignment.Left)
                     .AddColumn("出貨日", 0.80f, TextAlignment.Center)
                     .AddColumn("到期日", 0.80f, TextAlignment.Center)
                     .AddColumn("業務", 0.65f, TextAlignment.Left)
                     .AddColumn("應收金額", 1.00f, TextAlignment.Right)
                     .AddColumn("已收金額", 1.00f, TextAlignment.Right)
                     .AddColumn("未收餘額", 1.00f, TextAlignment.Right)
                     .AddColumn("逾期天數", 0.65f, TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                decimal cTotal = 0, cSettled = 0, cOutstanding = 0;

                foreach (var item in group)
                {
                    cTotal += item.TaxInclusiveAmount;
                    cSettled += item.SettledAmount;
                    cOutstanding += item.OutstandingAmount;

                    table.AddRow(
                        item.DeliveryCode,
                        item.DeliveryDate.ToString("yyyy/MM/dd"),
                        item.DueDate.ToString("yyyy/MM/dd"),
                        item.SalespersonName,
                        item.TaxInclusiveAmount.ToString("N2"),
                        item.SettledAmount.ToString("N2"),
                        item.OutstandingAmount.ToString("N2"),
                        item.DaysOverdue > 0 ? item.DaysOverdue.ToString() : ""
                    );
                }

                // 客戶小計
                table.AddRow(
                    string.Empty, string.Empty, string.Empty, "小計",
                    cTotal.ToString("N2"),
                    cSettled.ToString("N2"),
                    cOutstanding.ToString("N2"),
                    string.Empty
                );

                grandTotalAmount += cTotal;
                grandSettledAmount += cSettled;
                grandOutstandingAmount += cOutstanding;
            });

            doc.AddSpacing(5);
        }

        // === 頁尾：合計 ===
        doc.BeginFooter(footer =>
        {
            footer.AddSpacing(10);

            var summaryLines = new List<string>
            {
                $"應收總額：{grandTotalAmount:N2}",
                $"已收總額：{grandSettledAmount:N2}",
                $"未收總額：{grandOutstandingAmount:N2}",
                string.Empty
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
}

/// <summary>
/// 應收帳款報表單筆明細
/// </summary>
internal class ARLine
{
    public string DeliveryCode { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string SalespersonName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TaxInclusiveAmount { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
}
