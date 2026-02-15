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
    /// 客戶交易明細報表服務實作
    /// 查詢客戶的出貨單與退貨單交易記錄，依客戶分組、依日期排序
    /// </summary>
    public class CustomerTransactionReportService : ICustomerTransactionReportService
    {
        private readonly ISalesDeliveryService _salesDeliveryService;
        private readonly ISalesReturnService _salesReturnService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerTransactionReportService>? _logger;

        public CustomerTransactionReportService(
            ISalesDeliveryService salesDeliveryService,
            ISalesReturnService salesReturnService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerTransactionReportService>? logger = null)
        {
            _salesDeliveryService = salesDeliveryService;
            _salesReturnService = salesReturnService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染客戶交易明細報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerTransactionCriteria criteria)
        {
            try
            {
                var customerGroups = await GetTransactionDataAsync(criteria);

                if (!customerGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的交易資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildTransactionDocument(customerGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalRecords = customerGroups.Sum(g => g.Transactions.Count);
                return BatchPreviewResult.Success(images, document, totalRecords);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生客戶交易明細報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢交易資料，合併出貨與退貨，依客戶分組
        /// </summary>
        private async Task<List<CustomerTransactionGroup>> GetTransactionDataAsync(CustomerTransactionCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var transactions = new List<TransactionRecord>();

            // 查詢出貨單
            if (criteria.IncludeDeliveries)
            {
                var deliveries = await _salesDeliveryService.GetAllAsync();

                deliveries = deliveries
                    .Where(d => d.DeliveryDate >= startDate.Date && d.DeliveryDate <= endDate.Date)
                    .ToList();

                if (criteria.ExcludeCancelled)
                    deliveries = deliveries.Where(d => d.Status != EntityStatus.Inactive).ToList();

                if (criteria.CustomerIds.Any())
                    deliveries = deliveries.Where(d => criteria.CustomerIds.Contains(d.CustomerId)).ToList();

                foreach (var d in deliveries)
                {
                    transactions.Add(new TransactionRecord
                    {
                        CustomerId = d.CustomerId,
                        CustomerCode = d.Customer?.Code ?? "",
                        CustomerName = d.Customer?.CompanyName ?? "未知客戶",
                        TransactionDate = d.DeliveryDate,
                        DocumentCode = d.Code ?? "",
                        TransactionType = "出貨",
                        Amount = d.TotalAmount,
                        TaxAmount = d.TaxAmount,
                        AmountWithTax = d.TotalAmountWithTax,
                        DiscountAmount = d.DiscountAmount
                    });
                }
            }

            // 查詢退貨單
            if (criteria.IncludeReturns)
            {
                var returns = await _salesReturnService.GetByDateRangeAsync(startDate, endDate);

                if (criteria.ExcludeCancelled)
                    returns = returns.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.CustomerIds.Any())
                    returns = returns.Where(r => criteria.CustomerIds.Contains(r.CustomerId)).ToList();

                foreach (var r in returns)
                {
                    transactions.Add(new TransactionRecord
                    {
                        CustomerId = r.CustomerId,
                        CustomerCode = r.Customer?.Code ?? "",
                        CustomerName = r.Customer?.CompanyName ?? "未知客戶",
                        TransactionDate = r.ReturnDate,
                        DocumentCode = r.Code ?? "",
                        TransactionType = "退貨",
                        Amount = -r.TotalReturnAmount,
                        TaxAmount = -r.ReturnTaxAmount,
                        AmountWithTax = -r.TotalReturnAmountWithTax,
                        DiscountAmount = r.DiscountAmount
                    });
                }
            }

            // 按客戶分組，客戶內按日期排序
            var groups = transactions
                .GroupBy(t => new { t.CustomerId, t.CustomerCode, t.CustomerName })
                .Select(g =>
                {
                    var sorted = g.OrderBy(t => t.TransactionDate).ThenBy(t => t.DocumentCode).ToList();
                    return new CustomerTransactionGroup
                    {
                        CustomerId = g.Key.CustomerId,
                        CustomerCode = g.Key.CustomerCode,
                        CustomerName = g.Key.CustomerName,
                        Transactions = sorted,
                        DeliveryCount = sorted.Count(t => t.TransactionType == "出貨"),
                        ReturnCount = sorted.Count(t => t.TransactionType == "退貨"),
                        TotalAmount = sorted.Sum(t => t.Amount),
                        TotalTaxAmount = sorted.Sum(t => t.TaxAmount),
                        TotalAmountWithTax = sorted.Sum(t => t.AmountWithTax)
                    };
                })
                .OrderBy(g => g.CustomerCode)
                .ToList();

            return groups;
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構客戶交易明細報表
        /// </summary>
        private FormattedDocument BuildTransactionDocument(
            List<CustomerTransactionGroup> groups,
            Company? company,
            CustomerTransactionCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";
            var totalRecords = groups.Sum(g => g.Transactions.Count);

            var doc = new FormattedDocument()
                .SetDocumentName($"客戶交易明細-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客 戶 交 易 明 細", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"查詢期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"客戶數：{groups.Count} / 交易筆數：{totalRecords}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 各客戶交易明細 ===
            var grandTotalAmount = 0m;
            var grandTotalTax = 0m;
            var grandTotalWithTax = 0m;
            var grandDeliveryCount = 0;
            var grandReturnCount = 0;

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                grandTotalAmount += group.TotalAmount;
                grandTotalTax += group.TotalTaxAmount;
                grandTotalWithTax += group.TotalAmountWithTax;
                grandDeliveryCount += group.DeliveryCount;
                grandReturnCount += group.ReturnCount;

                // 客戶標題
                doc.AddText($"【{group.CustomerCode}】{group.CustomerName}", fontSize: 11, bold: true);
                doc.AddSpacing(3);

                // 該客戶的交易表格
                doc.AddTable(table =>
                {
                    table.AddColumn("日期", 0.7f, TextAlignment.Center)
                         .AddColumn("單號", 1.0f, TextAlignment.Left)
                         .AddColumn("類型", 0.4f, TextAlignment.Center)
                         .AddColumn("銷售金額", 1.0f, TextAlignment.Right)
                         .AddColumn("稅額", 0.8f, TextAlignment.Right)
                         .AddColumn("含稅金額", 1.0f, TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(18);

                    foreach (var tx in group.Transactions)
                    {
                        table.AddRow(
                            tx.TransactionDate.ToString("yyyy/MM/dd"),
                            tx.DocumentCode,
                            tx.TransactionType,
                            tx.Amount.ToString("N0"),
                            tx.TaxAmount.ToString("N0"),
                            tx.AmountWithTax.ToString("N0")
                        );
                    }

                    // 小計列
                    table.AddRow(
                        "",
                        $"小計（出貨 {group.DeliveryCount} 筆 / 退貨 {group.ReturnCount} 筆）",
                        "",
                        group.TotalAmount.ToString("N0"),
                        group.TotalTaxAmount.ToString("N0"),
                        group.TotalAmountWithTax.ToString("N0")
                    );
                });

                // 客戶間分隔
                if (i < groups.Count - 1)
                {
                    doc.AddSpacing(8);
                    doc.AddLine(LineStyle.Dashed, 0.5f);
                    doc.AddSpacing(8);
                }
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var summaryLines = new List<string>
                {
                    $"客戶總數：{groups.Count} 家",
                    $"出貨總筆數：{grandDeliveryCount:N0} 筆",
                    $"退貨總筆數：{grandReturnCount:N0} 筆",
                    $"銷售金額合計：{grandTotalAmount:N0}",
                    $"稅額合計：{grandTotalTax:N0}",
                    $"含稅金額合計：{grandTotalWithTax:N0}"
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

        #region 內部資料模型

        /// <summary>
        /// 單筆交易記錄（出貨或退貨）
        /// </summary>
        private class TransactionRecord
        {
            public int CustomerId { get; set; }
            public string CustomerCode { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public DateTime TransactionDate { get; set; }
            public string DocumentCode { get; set; } = "";
            public string TransactionType { get; set; } = "";
            /// <summary>
            /// 金額（退貨為負值）
            /// </summary>
            public decimal Amount { get; set; }
            /// <summary>
            /// 稅額（退貨為負值）
            /// </summary>
            public decimal TaxAmount { get; set; }
            /// <summary>
            /// 含稅金額（退貨為負值）
            /// </summary>
            public decimal AmountWithTax { get; set; }
            public decimal DiscountAmount { get; set; }
        }

        /// <summary>
        /// 客戶交易分組
        /// </summary>
        private class CustomerTransactionGroup
        {
            public int CustomerId { get; set; }
            public string CustomerCode { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public List<TransactionRecord> Transactions { get; set; } = new();
            public int DeliveryCount { get; set; }
            public int ReturnCount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal TotalTaxAmount { get; set; }
            public decimal TotalAmountWithTax { get; set; }
        }

        #endregion
    }
}
