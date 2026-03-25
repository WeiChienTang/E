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
/// 客戶拜訪報告服務
/// 依客戶、拜訪人員與日期篩選拜訪記錄，
/// 顯示拜訪日期、客戶名稱、拜訪方式、拜訪人員、目的與內容摘要
/// </summary>
public class CustomerVisitReportService : ICustomerVisitReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly ILogger<CustomerVisitReportService>? _logger;

    public CustomerVisitReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService,
        ILogger<CustomerVisitReportService>? logger = null)
    {
        _contextFactory = contextFactory;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerVisitReportCriteria criteria)
    {
        try
        {
            var visits = await BuildVisitLinesAsync(criteria);

            if (!visits.Any())
                return BatchPreviewResult.Failure($"無符合條件的拜訪記錄\n篩選條件：{criteria.GetSummary()}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            var document = BuildVisitDocument(visits, company, criteria);

            var images = criteria.PaperSetting != null
                ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                : _formattedPrintService.RenderToImages(document);

            return BatchPreviewResult.Success(images, document, visits.Count, new List<FormattedDocument> { document });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "產生客戶拜訪報告時發生錯誤");
            return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
        }
    }

    // ===== 資料查詢 =====

    private async Task<List<VisitLine>> BuildVisitLinesAsync(CustomerVisitReportCriteria criteria)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.CustomerVisits
            .Include(v => v.Customer)
            .Include(v => v.Employee)
            .Where(v => v.Status == EntityStatus.Active);

        if (criteria.StartDate.HasValue)
            query = query.Where(v => v.VisitDate >= criteria.StartDate.Value.Date);

        if (criteria.EndDate.HasValue)
            query = query.Where(v => v.VisitDate <= criteria.EndDate.Value.Date);

        if (criteria.CustomerIds.Any())
            query = query.Where(v => criteria.CustomerIds.Contains(v.CustomerId));

        if (criteria.EmployeeIds.Any())
            query = query.Where(v => v.EmployeeId.HasValue && criteria.EmployeeIds.Contains(v.EmployeeId.Value));

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword.Trim();
            query = query.Where(v =>
                (v.Customer != null && v.Customer.CompanyName != null && v.Customer.CompanyName.Contains(keyword)) ||
                (v.Purpose != null && v.Purpose.Contains(keyword)) ||
                (v.Content != null && v.Content.Contains(keyword)));
        }

        var visits = await query
            .OrderBy(v => v.VisitDate)
            .ThenBy(v => v.Customer!.CompanyName)
            .Select(v => new VisitLine
            {
                VisitDate = v.VisitDate,
                CustomerName = v.Customer != null ? (v.Customer.CompanyName ?? v.Customer.Code ?? v.Customer.Id.ToString()) : "未知客戶",
                CustomerId = v.CustomerId,
                VisitType = v.VisitType,
                EmployeeName = v.Employee != null ? (v.Employee.Name ?? v.Employee.Code ?? "") : "",
                Purpose = v.Purpose ?? "",
                Content = v.Content ?? "",
                NextFollowUpDate = v.NextFollowUpDate
            })
            .ToListAsync();

        return visits;
    }

    // ===== 報表建構 =====

    [SupportedOSPlatform("windows6.1")]
    private FormattedDocument BuildVisitDocument(
        List<VisitLine> visits,
        Company? company,
        CustomerVisitReportCriteria criteria)
    {
        var doc = new FormattedDocument()
            .SetDocumentName($"拜訪報告-{DateTime.Now:yyyyMMdd}")
            .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

        // === 頁首 ===
        var dateRangeText = criteria.StartDate.HasValue || criteria.EndDate.HasValue
            ? $"期間：{criteria.StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {criteria.EndDate?.ToString("yyyy/MM/dd") ?? "不限"}"
            : $"列印日期：{DateTime.Today:yyyy/MM/dd}";

        doc.BeginHeader(header =>
        {
            header.AddReportHeaderBlock(
                centerLines: new List<(string, float, bool)>
                {
                    (company?.CompanyName ?? "公司名稱", 14f, true),
                    ("客戶拜訪報告", 16f, true),
                    (dateRangeText, 10f, false)
                },
                rightLines: new List<string>
                {
                    $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                    $"筆數：{visits.Count}",
                    "頁次：{PAGE}/{PAGES}"
                },
                rightFontSize: 10f);
            header.AddSpacing(5);
        });

        // === 依客戶分組 ===
        var customerGroups = visits
            .GroupBy(v => v.CustomerName)
            .OrderBy(g => g.Key);

        foreach (var group in customerGroups)
        {
            doc.AddKeyValueRow(("客戶", group.Key), ("拜訪次數", group.Count().ToString()));
            doc.AddSpacing(2);

            doc.AddTable(table =>
            {
                table.AddColumn("拜訪日期", 0.80f, TextAlignment.Center)
                     .AddColumn("拜訪方式", 0.60f, TextAlignment.Center)
                     .AddColumn("拜訪人員", 0.65f, TextAlignment.Left)
                     .AddColumn("拜訪目的", 1.20f, TextAlignment.Left)
                     .AddColumn("拜訪內容", 2.00f, TextAlignment.Left)
                     .AddColumn("下次追蹤", 0.80f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                foreach (var visit in group)
                {
                    // 截取內容避免過長
                    var contentPreview = visit.Content.Length > 60
                        ? visit.Content[..60] + "..."
                        : visit.Content;

                    table.AddRow(
                        visit.VisitDate.ToString("yyyy/MM/dd"),
                        GetVisitTypeName(visit.VisitType),
                        visit.EmployeeName,
                        visit.Purpose,
                        contentPreview,
                        visit.NextFollowUpDate?.ToString("yyyy/MM/dd") ?? ""
                    );
                }
            });

            doc.AddSpacing(5);
        }

        // === 頁尾 ===
        doc.BeginFooter(footer =>
        {
            footer.AddSpacing(10);

            // 拜訪方式統計
            var typeStats = visits
                .GroupBy(v => v.VisitType)
                .OrderByDescending(g => g.Count())
                .Select(g => $"{GetVisitTypeName(g.Key)}：{g.Count()} 次");

            var summaryLines = new List<string>
            {
                $"拜訪總數：{visits.Count} 次　　客戶數：{visits.Select(v => v.CustomerId).Distinct().Count()} 家",
                $"拜訪方式分布：{string.Join("、", typeStats)}",
                string.Empty
            };

            footer.AddTwoColumnSection(
                leftContent: summaryLines,
                leftTitle: null,
                leftHasBorder: false,
                rightContent: new List<string>(),
                leftWidthRatio: 0.7f);

            footer.AddSpacing(20)
                  .AddSignatureSection(new[] { "製表人員", "業務主管" });
        });

        return doc;
    }

    private static string GetVisitTypeName(VisitType visitType)
    {
        return visitType switch
        {
            VisitType.Phone => "電話",
            VisitType.OnSite => "現場拜訪",
            VisitType.VideoConference => "視訊會議",
            VisitType.Email => "Email",
            VisitType.Other => "其他",
            _ => visitType.ToString()
        };
    }
}

/// <summary>
/// 客戶拜訪報告單筆明細
/// </summary>
internal class VisitLine
{
    public DateTime VisitDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public VisitType VisitType { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? NextFollowUpDate { get; set; }
}
