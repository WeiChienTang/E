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
    /// 客戶銷售分析報表服務實作
    /// 根據出貨單（SalesDelivery）資料，按客戶彙總銷售金額，由高至低排列
    /// </summary>
    public class CustomerSalesAnalysisReportService : ICustomerSalesAnalysisReportService
    {
        private readonly ISalesDeliveryService _salesDeliveryService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerSalesAnalysisReportService>? _logger;

        public CustomerSalesAnalysisReportService(
            ISalesDeliveryService salesDeliveryService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerSalesAnalysisReportService>? logger = null)
        {
            _salesDeliveryService = salesDeliveryService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染客戶銷售分析報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerSalesAnalysisCriteria criteria)
        {
            try
            {
                var analysisData = await GetSalesAnalysisDataAsync(criteria);

                if (!analysisData.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的銷售資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildAnalysisDocument(analysisData, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, analysisData.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生客戶銷售分析報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region 私有方法 - 資料查詢與彙總

        /// <summary>
        /// 查詢並彙總客戶銷售分析資料（以出貨單為資料來源）
        /// </summary>
        private async Task<List<CustomerSalesAnalysisItem>> GetSalesAnalysisDataAsync(CustomerSalesAnalysisCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;

            // 取得所有出貨單，再依日期範圍篩選
            var deliveries = await _salesDeliveryService.GetAllAsync();

            deliveries = deliveries
                .Where(d => d.DeliveryDate >= startDate.Date && d.DeliveryDate <= endDate.Date)
                .ToList();

            // 排除已取消
            if (criteria.ExcludeCancelled)
            {
                deliveries = deliveries.Where(d => d.Status != EntityStatus.Inactive).ToList();
            }

            // 篩選指定客戶
            if (criteria.CustomerIds.Any())
            {
                deliveries = deliveries.Where(d => criteria.CustomerIds.Contains(d.CustomerId)).ToList();
            }

            // 按客戶彙總
            var grouped = deliveries
                .GroupBy(d => new { d.CustomerId, CustomerCode = d.Customer?.Code ?? "", CustomerName = d.Customer?.CompanyName ?? "未知客戶" })
                .Select(g => new CustomerSalesAnalysisItem
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerCode = g.Key.CustomerCode,
                    CustomerName = g.Key.CustomerName,
                    DeliveryCount = g.Count(),
                    TotalAmount = g.Sum(d => d.TotalAmount),
                    TotalTaxAmount = g.Sum(d => d.TaxAmount),
                    TotalAmountWithTax = g.Sum(d => d.TotalAmountWithTax),
                    TotalDiscountAmount = g.Sum(d => d.DiscountAmount)
                })
                .OrderByDescending(x => x.TotalAmountWithTax)
                .ToList();

            // 計算佔比
            var grandTotal = grouped.Sum(x => x.TotalAmountWithTax);
            int rank = 1;
            decimal cumulativePercent = 0m;
            foreach (var item in grouped)
            {
                item.Rank = rank++;
                item.Percentage = grandTotal > 0
                    ? Math.Round(item.TotalAmountWithTax / grandTotal * 100, 2)
                    : 0;
                cumulativePercent += item.Percentage;
                item.CumulativePercentage = cumulativePercent;
            }

            return grouped;
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構客戶銷售分析報表
        /// </summary>
        private FormattedDocument BuildAnalysisDocument(
            List<CustomerSalesAnalysisItem> data,
            Company? company,
            CustomerSalesAnalysisCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";

            var doc = new FormattedDocument()
                .SetDocumentName($"客戶銷售分析-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客 戶 銷 售 分 析", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"分析期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"客戶數：{data.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 銷售分析表格 ===
            var grandTotalAmount = data.Sum(x => x.TotalAmount);
            var grandTotalTax = data.Sum(x => x.TotalTaxAmount);
            var grandTotalWithTax = data.Sum(x => x.TotalAmountWithTax);
            var grandTotalDeliveries = data.Sum(x => x.DeliveryCount);

            doc.AddTable(table =>
            {
                table.AddColumn("排名", 0.35f, TextAlignment.Center)
                     .AddColumn("客戶編號", 0.7f, TextAlignment.Left)
                     .AddColumn("客戶名稱", 1.5f, TextAlignment.Left)
                     .AddColumn("出貨數", 0.5f, TextAlignment.Right)
                     .AddColumn("銷售金額", 1.0f, TextAlignment.Right)
                     .AddColumn("稅額", 0.7f, TextAlignment.Right)
                     .AddColumn("含稅金額", 1.0f, TextAlignment.Right)
                     .AddColumn("佔比(%)", 0.55f, TextAlignment.Right)
                     .AddColumn("累計(%)", 0.55f, TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                foreach (var item in data)
                {
                    table.AddRow(
                        item.Rank.ToString(),
                        item.CustomerCode,
                        item.CustomerName,
                        item.DeliveryCount.ToString("N0"),
                        item.TotalAmount.ToString("N0"),
                        item.TotalTaxAmount.ToString("N0"),
                        item.TotalAmountWithTax.ToString("N0"),
                        item.Percentage.ToString("N2"),
                        item.CumulativePercentage.ToString("N2")
                    );
                }
            });

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var summaryLines = new List<string>
                {
                    $"客戶總數：{data.Count} 家",
                    $"出貨總數：{grandTotalDeliveries:N0} 筆",
                    $"銷售金額合計：{grandTotalAmount:N0}",
                    $"稅額合計：{grandTotalTax:N0}",
                    $"含稅金額合計：{grandTotalWithTax:N0}"
                };

                // 顯示前五大客戶摘要
                var topCustomers = data.Take(5).ToList();
                if (topCustomers.Any())
                {
                    summaryLines.Add("");
                    summaryLines.Add("前五大客戶：");
                    foreach (var top in topCustomers)
                    {
                        summaryLines.Add($"  {top.Rank}. {top.CustomerName}：{top.TotalAmountWithTax:N0} ({top.Percentage:N2}%)");
                    }
                }

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

        #region 內部資料模型

        /// <summary>
        /// 客戶銷售分析彙總項目
        /// </summary>
        private class CustomerSalesAnalysisItem
        {
            public int Rank { get; set; }
            public int CustomerId { get; set; }
            public string CustomerCode { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public int DeliveryCount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal TotalTaxAmount { get; set; }
            public decimal TotalAmountWithTax { get; set; }
            public decimal TotalDiscountAmount { get; set; }
            public decimal Percentage { get; set; }
            public decimal CumulativePercentage { get; set; }
        }

        #endregion
    }
}
