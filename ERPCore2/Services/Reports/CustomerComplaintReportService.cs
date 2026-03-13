using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 客訴報告服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class CustomerComplaintReportService : ICustomerComplaintReportService
    {
        private readonly ICustomerComplaintService _complaintService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerComplaintReportService>? _logger;

        public CustomerComplaintReportService(
            ICustomerComplaintService complaintService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerComplaintReportService>? logger = null)
        {
            _complaintService = complaintService;
            _customerService = customerService;
            _employeeService = employeeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一客訴紀錄報表
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int complaintId)
        {
            var complaint = await _complaintService.GetByIdAsync(complaintId);
            if (complaint == null)
                throw new ArgumentException($"找不到客訴 ID: {complaintId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildComplaintDocument(new List<CustomerComplaint> { complaint }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int complaintId)
        {
            var document = await GenerateReportAsync(complaintId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int complaintId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(complaintId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆客訴
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int complaintId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(complaintId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印客訴 {ComplaintId} 時發生錯誤", complaintId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var complaints = await GetComplaintsByCriteriaAsync(criteria);
                if (!complaints.Any())
                    return ServiceResult.Failure($"無符合條件的客訴\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildComplaintDocument(complaints, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印客訴報告時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（標準 BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var complaints = await GetComplaintsByCriteriaAsync(criteria);
                if (!complaints.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客訴\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildComplaintDocument(complaints, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, complaints.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客訴報告時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region CustomerComplaintReportCriteria 批次報表

        /// <summary>
        /// 以客訴報告專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerComplaintReportCriteria criteria)
        {
            try
            {
                var complaints = await GetComplaintsByTypedCriteriaAsync(criteria);
                if (!complaints.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客訴\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildComplaintDocument(complaints, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, complaints.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客訴報告（Criteria）時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<CustomerComplaint>> GetComplaintsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var all = await _complaintService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
            {
                var kw = criteria.DocumentNumberKeyword;
                all = all.Where(c =>
                    (c.Title != null && c.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Customer?.CompanyName != null && c.Customer.CompanyName.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!criteria.IncludeCancelled)
                all = all.Where(c => c.Status == EntityStatus.Active).ToList();

            return all.OrderByDescending(c => c.ComplaintDate).ToList();
        }

        private async Task<List<CustomerComplaint>> GetComplaintsByTypedCriteriaAsync(CustomerComplaintReportCriteria criteria)
        {
            var all = await _complaintService.GetAllAsync();

            if (criteria.CustomerIds.Any())
                all = all.Where(c => criteria.CustomerIds.Contains(c.CustomerId)).ToList();

            if (criteria.EmployeeIds.Any())
                all = all.Where(c => c.EmployeeId.HasValue && criteria.EmployeeIds.Contains(c.EmployeeId.Value)).ToList();

            if (criteria.Categories.Any())
                all = all.Where(c => criteria.Categories.Contains((int)c.Category)).ToList();

            if (criteria.Statuses.Any())
                all = all.Where(c => criteria.Statuses.Contains((int)c.ComplaintStatus)).ToList();

            if (criteria.StartDate.HasValue)
                all = all.Where(c => c.ComplaintDate >= criteria.StartDate.Value).ToList();

            if (criteria.EndDate.HasValue)
                all = all.Where(c => c.ComplaintDate <= criteria.EndDate.Value.AddDays(1).AddSeconds(-1)).ToList();

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword;
                all = all.Where(c =>
                    (c.Title != null && c.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Customer?.CompanyName != null && c.Customer.CompanyName.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Description != null && c.Description.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return all.OrderByDescending(c => c.ComplaintDate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private static string GetCategoryText(ComplaintCategory category) => category switch
        {
            ComplaintCategory.ProductQuality  => "產品品質",
            ComplaintCategory.DeliveryDelay   => "交期延誤",
            ComplaintCategory.ServiceAttitude => "服務態度",
            ComplaintCategory.PriceDispute    => "價格爭議",
            ComplaintCategory.Other           => "其他",
            _ => category.ToString()
        };

        private static string GetStatusText(ComplaintStatus status) => status switch
        {
            ComplaintStatus.Open       => "待處理",
            ComplaintStatus.InProgress => "處理中",
            ComplaintStatus.Resolved   => "已解決",
            ComplaintStatus.Closed     => "已關閉",
            _ => status.ToString()
        };

        /// <summary>
        /// 建構客訴報告文件（清單式）
        /// </summary>
        private FormattedDocument BuildComplaintDocument(
            List<CustomerComplaint> complaints,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"客訴報告-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客  訴  報  告", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"筆數：{complaints.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 客訴資料表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("項次", 0.30f, TextAlignment.Center)
                     .AddColumn("投訴日期", 0.75f, TextAlignment.Center)
                     .AddColumn("客戶名稱", 1.10f, TextAlignment.Left)
                     .AddColumn("投訴標題", 1.50f, TextAlignment.Left)
                     .AddColumn("類別", 0.75f, TextAlignment.Center)
                     .AddColumn("狀態", 0.65f, TextAlignment.Center)
                     .AddColumn("負責人員", 0.65f, TextAlignment.Left)
                     .AddColumn("解決日期", 0.75f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                int rowNum = 1;
                foreach (var c in complaints)
                {
                    table.AddRow(
                        rowNum.ToString(),
                        c.ComplaintDate.ToString("yyyy/MM/dd"),
                        c.Customer?.CompanyName ?? "",
                        c.Title,
                        GetCategoryText(c.Category),
                        GetStatusText(c.ComplaintStatus),
                        c.Employee?.Name ?? "",
                        c.ResolvedDate.HasValue ? c.ResolvedDate.Value.ToString("yyyy/MM/dd") : ""
                    );
                    rowNum++;
                }
            });

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                // 狀態統計
                var openCount       = complaints.Count(c => c.ComplaintStatus == ComplaintStatus.Open);
                var inProgressCount = complaints.Count(c => c.ComplaintStatus == ComplaintStatus.InProgress);
                var resolvedCount   = complaints.Count(c => c.ComplaintStatus == ComplaintStatus.Resolved);
                var closedCount     = complaints.Count(c => c.ComplaintStatus == ComplaintStatus.Closed);

                var summaryLines = new List<string>
                {
                    $"客訴總數：{complaints.Count} 筆",
                    $"待處理：{openCount}  處理中：{inProgressCount}  已解決：{resolvedCount}  已關閉：{closedCount}"
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "主管" });
            });

            return doc;
        }

        #endregion
    }
}
