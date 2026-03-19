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
/// 應收帳款帳齡分析報表服務
/// 計算邏輯：
///   應收金額 = SalesDelivery.TotalAmount + TaxAmount（含稅）
///   已沖金額 = SUM(SetoffItemDetail.CurrentSetoffAmount) WHERE SourceDetailType=SalesDeliveryDetail(5) AND SourceDetailId IN detail IDs
///   未收金額 = 應收金額 - 已沖金額（> 0 才列入）
///   到期日   = DeliveryDate + Customer.PaymentDays
///   帳齡     = AsOfDate - 到期日（負值表示未到期）
/// 帳齡區間：未到期 / 逾期1-30天 / 31-60天 / 61-90天 / 91-120天 / 121天以上
/// </summary>
public class ARAgingReportService : IARAgingReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly ILogger<ARAgingReportService>? _logger;

    public ARAgingReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService,
        ILogger<ARAgingReportService>? logger = null)
    {
        _contextFactory = contextFactory;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ARAgingCriteria criteria)
    {
        try
        {
            var lines = await BuildAgingLinesAsync(criteria);

            if (!lines.Any())
                return BatchPreviewResult.Failure($"無符合條件的應收帳款資料\n篩選條件：{criteria.GetSummary()}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            var document = BuildAgingDocument(lines, company, criteria);

            var images = criteria.PaperSetting != null
                ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                : _formattedPrintService.RenderToImages(document);

            return BatchPreviewResult.Success(images, document, lines.Count, new List<FormattedDocument> { document });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "產生應收帳款帳齡分析時發生錯誤");
            return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
        }
    }

    // ===== 資料查詢 =====

    private async Task<List<ARAgingLine>> BuildAgingLinesAsync(ARAgingCriteria criteria)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var asOfDate = criteria.AsOfDate?.Date ?? DateTime.Today;

        // 查詢所有出貨單（含客戶與業務篩選）
        var deliveryQuery = context.SalesDeliveries
            .Include(sd => sd.Customer)
            .Include(sd => sd.Salesperson)
            .Where(sd => sd.Status == ERPCore2.Models.Enums.EntityStatus.Active);

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
            return new List<ARAgingLine>();

        // 批次查詢所有相關沖款明細（SourceDetailType = SalesDeliveryDetail = 5）
        var allDetailIds = deliveries.SelectMany(d => d.DetailIds).ToList();
        var setoffMap = new Dictionary<int, decimal>(); // DetailId → 已沖金額

        if (allDetailIds.Any())
        {
            var setoffItems = await context.SetoffItemDetails
                .Where(s => s.SourceDetailType == SetoffDetailType.SalesDeliveryDetail
                         && allDetailIds.Contains(s.SourceDetailId))
                .Select(s => new { s.SourceDetailId, s.CurrentSetoffAmount })
                .ToListAsync();

            foreach (var item in setoffItems)
            {
                if (setoffMap.ContainsKey(item.SourceDetailId))
                    setoffMap[item.SourceDetailId] += item.CurrentSetoffAmount;
                else
                    setoffMap[item.SourceDetailId] = item.CurrentSetoffAmount;
            }
        }

        var result = new List<ARAgingLine>();

        foreach (var d in deliveries)
        {
            // 計算已沖金額（出貨單旗下所有明細的沖款加總）
            var settledAmount = d.DetailIds.Sum(id => setoffMap.TryGetValue(id, out var amt) ? amt : 0m);
            var outstandingAmount = d.TaxInclusiveAmount - settledAmount;

            if (!criteria.ShowZeroBalance && outstandingAmount <= 0)
                continue;

            if (outstandingAmount < 0)
                outstandingAmount = 0; // 防止過沖顯示負數

            var dueDate = d.DeliveryDate.AddDays(d.CustomerPaymentDays);
            var daysOverdue = (int)(asOfDate - dueDate).TotalDays; // 負值 = 未到期

            result.Add(new ARAgingLine
            {
                DeliveryCode = d.Code ?? string.Empty,
                DeliveryDate = d.DeliveryDate,
                DueDate = dueDate,
                DaysOverdue = daysOverdue,
                CustomerName = d.CustomerName,
                TaxInclusiveAmount = d.TaxInclusiveAmount,
                SettledAmount = settledAmount,
                OutstandingAmount = outstandingAmount,
                CustomerId = d.CustomerId ?? 0
            });
        }

        return result
            .OrderBy(l => l.CustomerName)
            .ThenByDescending(l => l.DaysOverdue)
            .ToList();
    }

    // ===== 報表建構 =====

    [SupportedOSPlatform("windows6.1")]
    private FormattedDocument BuildAgingDocument(
        List<ARAgingLine> lines,
        Company? company,
        ARAgingCriteria criteria)
    {
        var asOfDate = criteria.AsOfDate?.Date ?? DateTime.Today;

        var doc = new FormattedDocument()
            .SetDocumentName($"應收帳款帳齡分析-{asOfDate:yyyyMMdd}")
            .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

        // === 頁首 ===
        doc.BeginHeader(header =>
        {
            header.AddReportHeaderBlock(
                centerLines: new List<(string, float, bool)>
                {
                    (company?.CompanyName ?? "公司名稱", 14f, true),
                    ("應收帳款帳齡分析表", 16f, true),
                    ($"截止日期：{asOfDate:yyyy/MM/dd}", 10f, false)
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

        // 全域合計
        decimal totalCurrentAmount = 0;
        decimal totalBucket1 = 0, totalBucket2 = 0, totalBucket3 = 0, totalBucket4 = 0, totalBucket5 = 0;

        foreach (var group in customerGroups)
        {
            doc.AddKeyValueRow(("客戶", group.Key));
            doc.AddSpacing(2);

            doc.AddTable(table =>
            {
                table.AddColumn("出貨單號", 1.00f, Models.Reports.TextAlignment.Left)
                     .AddColumn("出貨日", 0.75f, Models.Reports.TextAlignment.Center)
                     .AddColumn("到期日", 0.75f, Models.Reports.TextAlignment.Center)
                     .AddColumn("應收金額", 0.90f, Models.Reports.TextAlignment.Right)
                     .AddColumn("未到期", 0.80f, Models.Reports.TextAlignment.Right)
                     .AddColumn("逾期1-30", 0.80f, Models.Reports.TextAlignment.Right)
                     .AddColumn("逾期31-60", 0.80f, Models.Reports.TextAlignment.Right)
                     .AddColumn("逾期61-90", 0.80f, Models.Reports.TextAlignment.Right)
                     .AddColumn("逾期91-120", 0.85f, Models.Reports.TextAlignment.Right)
                     .AddColumn("逾期121+", 0.80f, Models.Reports.TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                decimal cCurrent = 0, cB1 = 0, cB2 = 0, cB3 = 0, cB4 = 0, cB5 = 0;

                foreach (var item in group)
                {
                    var (notDue, b1, b2, b3, b4, b5) = BucketAmount(item.OutstandingAmount, item.DaysOverdue);
                    cCurrent += notDue; cB1 += b1; cB2 += b2; cB3 += b3; cB4 += b4; cB5 += b5;

                    table.AddRow(
                        item.DeliveryCode,
                        item.DeliveryDate.ToString("yyyy/MM/dd"),
                        item.DueDate.ToString("yyyy/MM/dd"),
                        item.OutstandingAmount.ToString("N2"),
                        notDue > 0 ? notDue.ToString("N2") : string.Empty,
                        b1 > 0 ? b1.ToString("N2") : string.Empty,
                        b2 > 0 ? b2.ToString("N2") : string.Empty,
                        b3 > 0 ? b3.ToString("N2") : string.Empty,
                        b4 > 0 ? b4.ToString("N2") : string.Empty,
                        b5 > 0 ? b5.ToString("N2") : string.Empty
                    );
                }

                // 客戶小計
                var cTotal = cCurrent + cB1 + cB2 + cB3 + cB4 + cB5;
                table.AddRow(
                    string.Empty, string.Empty, "小計",
                    cTotal.ToString("N2"),
                    cCurrent > 0 ? cCurrent.ToString("N2") : string.Empty,
                    cB1 > 0 ? cB1.ToString("N2") : string.Empty,
                    cB2 > 0 ? cB2.ToString("N2") : string.Empty,
                    cB3 > 0 ? cB3.ToString("N2") : string.Empty,
                    cB4 > 0 ? cB4.ToString("N2") : string.Empty,
                    cB5 > 0 ? cB5.ToString("N2") : string.Empty
                );

                totalCurrentAmount += cCurrent;
                totalBucket1 += cB1; totalBucket2 += cB2;
                totalBucket3 += cB3; totalBucket4 += cB4; totalBucket5 += cB5;
            });

            doc.AddSpacing(5);
        }

        // === 頁尾：合計 ===
        doc.BeginFooter(footer =>
        {
            footer.AddSpacing(10);

            var grandTotal = totalCurrentAmount + totalBucket1 + totalBucket2 + totalBucket3 + totalBucket4 + totalBucket5;
            var summaryLines = new List<string>
            {
                $"未到期：{totalCurrentAmount:N2}　　逾期1-30天：{totalBucket1:N2}　　逾期31-60天：{totalBucket2:N2}",
                $"逾期61-90天：{totalBucket3:N2}　　逾期91-120天：{totalBucket4:N2}　　逾期121天以上：{totalBucket5:N2}",
                $"應收帳款總計：{grandTotal:N2}",
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

    /// <summary>
    /// 依帳齡天數將未收金額分配至各區間
    /// </summary>
    /// <returns>(未到期, 逾期1-30, 逾期31-60, 逾期61-90, 逾期91-120, 逾期121+)</returns>
    private static (decimal notDue, decimal b1, decimal b2, decimal b3, decimal b4, decimal b5)
        BucketAmount(decimal amount, int daysOverdue)
    {
        if (daysOverdue <= 0) return (amount, 0, 0, 0, 0, 0);
        if (daysOverdue <= 30) return (0, amount, 0, 0, 0, 0);
        if (daysOverdue <= 60) return (0, 0, amount, 0, 0, 0);
        if (daysOverdue <= 90) return (0, 0, 0, amount, 0, 0);
        if (daysOverdue <= 120) return (0, 0, 0, 0, amount, 0);
        return (0, 0, 0, 0, 0, amount);
    }
}

/// <summary>
/// 應收帳款帳齡分析單筆明細
/// </summary>
internal class ARAgingLine
{
    public string DeliveryCode { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TaxInclusiveAmount { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
}
