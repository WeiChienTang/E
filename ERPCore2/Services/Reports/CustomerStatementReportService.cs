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
    /// 客戶對帳單報表服務實作
    /// 產生指定期間的客戶對帳單，含期初餘額、出貨/退貨/收款明細及期末餘額
    /// </summary>
    public class CustomerStatementReportService : ICustomerStatementReportService
    {
        private readonly ISalesDeliveryService _salesDeliveryService;
        private readonly ISalesReturnService _salesReturnService;
        private readonly ISetoffDocumentService _setoffDocumentService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerStatementReportService>? _logger;

        public CustomerStatementReportService(
            ISalesDeliveryService salesDeliveryService,
            ISalesReturnService salesReturnService,
            ISetoffDocumentService setoffDocumentService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerStatementReportService>? logger = null)
        {
            _salesDeliveryService = salesDeliveryService;
            _salesReturnService = salesReturnService;
            _setoffDocumentService = setoffDocumentService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染客戶對帳單報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerStatementCriteria criteria)
        {
            try
            {
                var customerGroups = await GetStatementDataAsync(criteria);

                if (!customerGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的對帳資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildStatementDocument(customerGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalRecords = customerGroups.Sum(g => g.Transactions.Count);
                return BatchPreviewResult.Success(images, document, totalRecords);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生客戶對帳單報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢對帳單資料，合併出貨、退貨、收款，依客戶分組
        /// </summary>
        private async Task<List<CustomerStatementGroup>> GetStatementDataAsync(CustomerStatementCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;

            // 收集所有相關客戶的交易（期間內 + 期前用於計算期初餘額）
            var periodTransactions = new List<StatementTransaction>();
            var prePeriodTransactions = new List<StatementTransaction>();

            // === 查詢出貨單 ===
            if (criteria.IncludeDeliveries)
            {
                var allDeliveries = await _salesDeliveryService.GetAllAsync();

                if (criteria.ExcludeCancelled)
                    allDeliveries = allDeliveries.Where(d => d.Status != EntityStatus.Inactive).ToList();

                if (criteria.CustomerIds.Any())
                    allDeliveries = allDeliveries.Where(d => criteria.CustomerIds.Contains(d.CustomerId)).ToList();

                // 期間內出貨
                var periodDeliveries = allDeliveries
                    .Where(d => d.DeliveryDate >= startDate.Date && d.DeliveryDate <= endDate.Date)
                    .ToList();

                foreach (var d in periodDeliveries)
                {
                    periodTransactions.Add(new StatementTransaction
                    {
                        CustomerId = d.CustomerId,
                        CustomerCode = d.Customer?.Code ?? "",
                        CustomerName = d.Customer?.CompanyName ?? "未知客戶",
                        TransactionDate = d.DeliveryDate,
                        DocumentCode = d.Code ?? "",
                        TransactionType = "出貨",
                        DebitAmount = d.TotalAmountWithTax,
                        CreditAmount = 0
                    });
                }

                // 期前出貨（用於計算期初餘額）
                var preDeliveries = allDeliveries
                    .Where(d => d.DeliveryDate < startDate.Date)
                    .ToList();

                foreach (var d in preDeliveries)
                {
                    prePeriodTransactions.Add(new StatementTransaction
                    {
                        CustomerId = d.CustomerId,
                        CustomerCode = d.Customer?.Code ?? "",
                        CustomerName = d.Customer?.CompanyName ?? "未知客戶",
                        DebitAmount = d.TotalAmountWithTax,
                        CreditAmount = 0
                    });
                }
            }

            // === 查詢退貨單 ===
            if (criteria.IncludeReturns)
            {
                // 期間內退貨
                var periodReturns = await _salesReturnService.GetByDateRangeAsync(startDate, endDate);

                if (criteria.ExcludeCancelled)
                    periodReturns = periodReturns.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.CustomerIds.Any())
                    periodReturns = periodReturns.Where(r => criteria.CustomerIds.Contains(r.CustomerId)).ToList();

                foreach (var r in periodReturns)
                {
                    periodTransactions.Add(new StatementTransaction
                    {
                        CustomerId = r.CustomerId,
                        CustomerCode = r.Customer?.Code ?? "",
                        CustomerName = r.Customer?.CompanyName ?? "未知客戶",
                        TransactionDate = r.ReturnDate,
                        DocumentCode = r.Code ?? "",
                        TransactionType = "退貨",
                        DebitAmount = 0,
                        CreditAmount = r.TotalReturnAmountWithTax
                    });
                }

                // 期前退貨（用於計算期初餘額）
                // 使用一個較早的日期作為起始
                var earlyDate = new DateTime(2000, 1, 1);
                var preReturns = await _salesReturnService.GetByDateRangeAsync(earlyDate, startDate.AddDays(-1));

                if (criteria.ExcludeCancelled)
                    preReturns = preReturns.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.CustomerIds.Any())
                    preReturns = preReturns.Where(r => criteria.CustomerIds.Contains(r.CustomerId)).ToList();

                foreach (var r in preReturns)
                {
                    prePeriodTransactions.Add(new StatementTransaction
                    {
                        CustomerId = r.CustomerId,
                        CustomerCode = r.Customer?.Code ?? "",
                        CustomerName = r.Customer?.CompanyName ?? "未知客戶",
                        DebitAmount = 0,
                        CreditAmount = r.TotalReturnAmountWithTax
                    });
                }
            }

            // === 查詢沖款單（收款）===
            if (criteria.IncludePayments)
            {
                var allSetoffs = await _setoffDocumentService.GetBySetoffTypeAsync(SetoffType.AccountsReceivable);

                if (criteria.CustomerIds.Any())
                    allSetoffs = allSetoffs.Where(s => criteria.CustomerIds.Contains(s.RelatedPartyId)).ToList();

                // 期間內收款
                var periodSetoffs = allSetoffs
                    .Where(s => s.SetoffDate >= startDate.Date && s.SetoffDate <= endDate.Date)
                    .ToList();

                foreach (var s in periodSetoffs)
                {
                    var totalReceived = s.TotalCollectionAmount + s.TotalAllowanceAmount + s.PrepaymentSetoffAmount;
                    if (totalReceived > 0)
                    {
                        periodTransactions.Add(new StatementTransaction
                        {
                            CustomerId = s.RelatedPartyId,
                            CustomerCode = "",  // 稍後從客戶分組取得
                            CustomerName = s.RelatedPartyName,
                            TransactionDate = s.SetoffDate,
                            DocumentCode = s.Code ?? "",
                            TransactionType = "收款",
                            DebitAmount = 0,
                            CreditAmount = totalReceived,
                            CollectionAmount = s.TotalCollectionAmount,
                            AllowanceAmount = s.TotalAllowanceAmount
                        });
                    }
                }

                // 期前收款（用於計算期初餘額）
                var preSetoffs = allSetoffs
                    .Where(s => s.SetoffDate < startDate.Date)
                    .ToList();

                foreach (var s in preSetoffs)
                {
                    var totalReceived = s.TotalCollectionAmount + s.TotalAllowanceAmount + s.PrepaymentSetoffAmount;
                    if (totalReceived > 0)
                    {
                        prePeriodTransactions.Add(new StatementTransaction
                        {
                            CustomerId = s.RelatedPartyId,
                            DebitAmount = 0,
                            CreditAmount = totalReceived
                        });
                    }
                }
            }

            // === 合併所有客戶 ID ===
            var allCustomerIds = periodTransactions.Select(t => t.CustomerId)
                .Union(prePeriodTransactions.Select(t => t.CustomerId))
                .Distinct()
                .ToList();

            // === 按客戶分組 ===
            var groups = new List<CustomerStatementGroup>();

            foreach (var customerId in allCustomerIds)
            {
                // 期前交易計算期初餘額
                var preTxs = prePeriodTransactions.Where(t => t.CustomerId == customerId).ToList();
                var openingBalance = preTxs.Sum(t => t.DebitAmount) - preTxs.Sum(t => t.CreditAmount);

                // 期間內交易
                var periodTxs = periodTransactions
                    .Where(t => t.CustomerId == customerId)
                    .OrderBy(t => t.TransactionDate)
                    .ThenBy(t => t.DocumentCode)
                    .ToList();

                // 如果該客戶期初為 0 且期間內沒有交易，跳過
                if (openingBalance == 0 && !periodTxs.Any())
                    continue;

                // 取得客戶資訊（從交易記錄中取得）
                var sampleTx = periodTxs.FirstOrDefault() ?? preTxs.FirstOrDefault();
                var customerCode = sampleTx?.CustomerCode ?? "";
                var customerName = sampleTx?.CustomerName ?? "未知客戶";

                // 計算累計餘額
                var runningBalance = openingBalance;
                foreach (var tx in periodTxs)
                {
                    runningBalance += tx.DebitAmount - tx.CreditAmount;
                    tx.RunningBalance = runningBalance;
                }

                var closingBalance = runningBalance;

                groups.Add(new CustomerStatementGroup
                {
                    CustomerId = customerId,
                    CustomerCode = customerCode,
                    CustomerName = customerName,
                    OpeningBalance = openingBalance,
                    ClosingBalance = closingBalance,
                    Transactions = periodTxs,
                    TotalDebit = periodTxs.Sum(t => t.DebitAmount),
                    TotalCredit = periodTxs.Sum(t => t.CreditAmount),
                    DeliveryCount = periodTxs.Count(t => t.TransactionType == "出貨"),
                    ReturnCount = periodTxs.Count(t => t.TransactionType == "退貨"),
                    PaymentCount = periodTxs.Count(t => t.TransactionType == "收款")
                });
            }

            return groups.OrderBy(g => g.CustomerCode).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構客戶對帳單報表
        /// </summary>
        private FormattedDocument BuildStatementDocument(
            List<CustomerStatementGroup> groups,
            Company? company,
            CustomerStatementCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";
            var totalRecords = groups.Sum(g => g.Transactions.Count);

            var doc = new FormattedDocument()
                .SetDocumentName($"客戶對帳單-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客 戶 對 帳 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"對帳期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"客戶數：{groups.Count} / 交易筆數：{totalRecords}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 各客戶對帳明細 ===
            var grandTotalDebit = 0m;
            var grandTotalCredit = 0m;
            var grandDeliveryCount = 0;
            var grandReturnCount = 0;
            var grandPaymentCount = 0;

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                grandTotalDebit += group.TotalDebit;
                grandTotalCredit += group.TotalCredit;
                grandDeliveryCount += group.DeliveryCount;
                grandReturnCount += group.ReturnCount;
                grandPaymentCount += group.PaymentCount;

                // 客戶標題
                doc.AddText($"【{group.CustomerCode}】{group.CustomerName}", fontSize: 11, bold: true);
                doc.AddSpacing(3);

                // 期初餘額
                doc.AddText($"期初餘額：{group.OpeningBalance:N0}", fontSize: 9);
                doc.AddSpacing(3);

                // 該客戶的交易表格
                doc.AddTable(table =>
                {
                    table.AddColumn("日期", 0.7f, TextAlignment.Center)
                         .AddColumn("單號", 1.0f, TextAlignment.Left)
                         .AddColumn("類型", 0.4f, TextAlignment.Center)
                         .AddColumn("應收金額", 0.9f, TextAlignment.Right)
                         .AddColumn("收款金額", 0.9f, TextAlignment.Right)
                         .AddColumn("餘額", 0.9f, TextAlignment.Right)
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
                            tx.DebitAmount > 0 ? tx.DebitAmount.ToString("N0") : "",
                            tx.CreditAmount > 0 ? tx.CreditAmount.ToString("N0") : "",
                            tx.RunningBalance.ToString("N0")
                        );
                    }

                    // 小計列
                    table.AddRow(
                        "",
                        $"小計（出貨 {group.DeliveryCount} / 退貨 {group.ReturnCount} / 收款 {group.PaymentCount}）",
                        "",
                        group.TotalDebit.ToString("N0"),
                        group.TotalCredit.ToString("N0"),
                        ""
                    );
                });

                // 期末餘額
                doc.AddSpacing(2);
                doc.AddText($"期末餘額：{group.ClosingBalance:N0}", fontSize: 9, bold: true);

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

                var grandOpeningTotal = groups.Sum(g => g.OpeningBalance);
                var grandClosingTotal = groups.Sum(g => g.ClosingBalance);

                var summaryLines = new List<string>
                {
                    $"客戶總數：{groups.Count} 家",
                    $"出貨筆數：{grandDeliveryCount:N0} 筆",
                    $"退貨筆數：{grandReturnCount:N0} 筆",
                    $"收款筆數：{grandPaymentCount:N0} 筆",
                    $"本期應收合計：{grandTotalDebit:N0}",
                    $"本期收款合計：{grandTotalCredit:N0}",
                    $"期初餘額合計：{grandOpeningTotal:N0}",
                    $"期末餘額合計：{grandClosingTotal:N0}"
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
        /// 對帳單交易記錄
        /// </summary>
        private class StatementTransaction
        {
            public int CustomerId { get; set; }
            public string CustomerCode { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public DateTime TransactionDate { get; set; }
            public string DocumentCode { get; set; } = "";
            public string TransactionType { get; set; } = "";
            /// <summary>
            /// 應收金額（出貨增加應收）
            /// </summary>
            public decimal DebitAmount { get; set; }
            /// <summary>
            /// 沖銷金額（退貨、收款減少應收）
            /// </summary>
            public decimal CreditAmount { get; set; }
            /// <summary>
            /// 累計餘額
            /// </summary>
            public decimal RunningBalance { get; set; }
            /// <summary>
            /// 實際收款金額（僅收款類型使用）
            /// </summary>
            public decimal CollectionAmount { get; set; }
            /// <summary>
            /// 折讓金額（僅收款類型使用）
            /// </summary>
            public decimal AllowanceAmount { get; set; }
        }

        /// <summary>
        /// 客戶對帳單分組
        /// </summary>
        private class CustomerStatementGroup
        {
            public int CustomerId { get; set; }
            public string CustomerCode { get; set; } = "";
            public string CustomerName { get; set; } = "";
            /// <summary>
            /// 期初餘額
            /// </summary>
            public decimal OpeningBalance { get; set; }
            /// <summary>
            /// 期末餘額
            /// </summary>
            public decimal ClosingBalance { get; set; }
            public List<StatementTransaction> Transactions { get; set; } = new();
            /// <summary>
            /// 本期應收合計（借方）
            /// </summary>
            public decimal TotalDebit { get; set; }
            /// <summary>
            /// 本期沖銷合計（貸方）
            /// </summary>
            public decimal TotalCredit { get; set; }
            public int DeliveryCount { get; set; }
            public int ReturnCount { get; set; }
            public int PaymentCount { get; set; }
        }

        #endregion
    }
}
