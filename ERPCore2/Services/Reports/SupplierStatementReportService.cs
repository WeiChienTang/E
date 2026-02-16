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
    /// 廠商對帳單報表服務實作
    /// 產生指定期間的廠商對帳單，含期初餘額、進貨/退貨/付款明細及期末餘額
    /// </summary>
    public class SupplierStatementReportService : ISupplierStatementReportService
    {
        private readonly IPurchaseReceivingService _purchaseReceivingService;
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly ISetoffDocumentService _setoffDocumentService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SupplierStatementReportService>? _logger;

        public SupplierStatementReportService(
            IPurchaseReceivingService purchaseReceivingService,
            IPurchaseReturnService purchaseReturnService,
            ISetoffDocumentService setoffDocumentService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<SupplierStatementReportService>? logger = null)
        {
            _purchaseReceivingService = purchaseReceivingService;
            _purchaseReturnService = purchaseReturnService;
            _setoffDocumentService = setoffDocumentService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染廠商對帳單報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(SupplierStatementCriteria criteria)
        {
            try
            {
                var supplierGroups = await GetStatementDataAsync(criteria);

                if (!supplierGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的對帳資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildStatementDocument(supplierGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalRecords = supplierGroups.Sum(g => g.Transactions.Count);
                return BatchPreviewResult.Success(images, document, totalRecords);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生廠商對帳單報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢對帳單資料，合併進貨、退貨、付款，依廠商分組
        /// </summary>
        private async Task<List<SupplierStatementGroup>> GetStatementDataAsync(SupplierStatementCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;

            var periodTransactions = new List<StatementTransaction>();
            var prePeriodTransactions = new List<StatementTransaction>();

            // === 查詢進貨單 ===
            if (criteria.IncludeReceivings)
            {
                // 期間內進貨
                var periodReceivings = await _purchaseReceivingService.GetByDateRangeAsync(startDate, endDate);

                if (criteria.ExcludeCancelled)
                    periodReceivings = periodReceivings.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.SupplierIds.Any())
                    periodReceivings = periodReceivings.Where(r => criteria.SupplierIds.Contains(r.SupplierId)).ToList();

                foreach (var r in periodReceivings)
                {
                    periodTransactions.Add(new StatementTransaction
                    {
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier?.Code ?? "",
                        SupplierName = r.Supplier?.CompanyName ?? "未知廠商",
                        TransactionDate = r.ReceiptDate,
                        DocumentCode = r.Code ?? "",
                        TransactionType = "進貨",
                        DebitAmount = r.PurchaseReceivingTotalAmountIncludingTax,
                        CreditAmount = 0
                    });
                }

                // 期前進貨（用於計算期初餘額）
                var earlyDate = new DateTime(2000, 1, 1);
                var preReceivings = await _purchaseReceivingService.GetByDateRangeAsync(earlyDate, startDate.AddDays(-1));

                if (criteria.ExcludeCancelled)
                    preReceivings = preReceivings.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.SupplierIds.Any())
                    preReceivings = preReceivings.Where(r => criteria.SupplierIds.Contains(r.SupplierId)).ToList();

                foreach (var r in preReceivings)
                {
                    prePeriodTransactions.Add(new StatementTransaction
                    {
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier?.Code ?? "",
                        SupplierName = r.Supplier?.CompanyName ?? "未知廠商",
                        DebitAmount = r.PurchaseReceivingTotalAmountIncludingTax,
                        CreditAmount = 0
                    });
                }
            }

            // === 查詢退貨單 ===
            if (criteria.IncludeReturns)
            {
                // 期間內退貨
                var periodReturns = await _purchaseReturnService.GetByDateRangeAsync(startDate, endDate);

                if (criteria.ExcludeCancelled)
                    periodReturns = periodReturns.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.SupplierIds.Any())
                    periodReturns = periodReturns.Where(r => criteria.SupplierIds.Contains(r.SupplierId)).ToList();

                foreach (var r in periodReturns)
                {
                    periodTransactions.Add(new StatementTransaction
                    {
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier?.Code ?? "",
                        SupplierName = r.Supplier?.CompanyName ?? "未知廠商",
                        TransactionDate = r.ReturnDate,
                        DocumentCode = r.Code ?? "",
                        TransactionType = "退貨",
                        DebitAmount = 0,
                        CreditAmount = r.TotalReturnAmountWithTax
                    });
                }

                // 期前退貨（用於計算期初餘額）
                var earlyDate = new DateTime(2000, 1, 1);
                var preReturns = await _purchaseReturnService.GetByDateRangeAsync(earlyDate, startDate.AddDays(-1));

                if (criteria.ExcludeCancelled)
                    preReturns = preReturns.Where(r => r.Status != EntityStatus.Inactive).ToList();

                if (criteria.SupplierIds.Any())
                    preReturns = preReturns.Where(r => criteria.SupplierIds.Contains(r.SupplierId)).ToList();

                foreach (var r in preReturns)
                {
                    prePeriodTransactions.Add(new StatementTransaction
                    {
                        SupplierId = r.SupplierId,
                        SupplierCode = r.Supplier?.Code ?? "",
                        SupplierName = r.Supplier?.CompanyName ?? "未知廠商",
                        DebitAmount = 0,
                        CreditAmount = r.TotalReturnAmountWithTax
                    });
                }
            }

            // === 查詢沖款單（付款）===
            if (criteria.IncludePayments)
            {
                var allSetoffs = await _setoffDocumentService.GetBySetoffTypeAsync(SetoffType.AccountsPayable);

                if (criteria.SupplierIds.Any())
                    allSetoffs = allSetoffs.Where(s => criteria.SupplierIds.Contains(s.RelatedPartyId)).ToList();

                // 期間內付款
                var periodSetoffs = allSetoffs
                    .Where(s => s.SetoffDate >= startDate.Date && s.SetoffDate <= endDate.Date)
                    .ToList();

                foreach (var s in periodSetoffs)
                {
                    var totalPaid = s.TotalCollectionAmount + s.TotalAllowanceAmount + s.PrepaymentSetoffAmount;
                    if (totalPaid > 0)
                    {
                        periodTransactions.Add(new StatementTransaction
                        {
                            SupplierId = s.RelatedPartyId,
                            SupplierCode = "",
                            SupplierName = s.RelatedPartyName,
                            TransactionDate = s.SetoffDate,
                            DocumentCode = s.Code ?? "",
                            TransactionType = "付款",
                            DebitAmount = 0,
                            CreditAmount = totalPaid,
                            PaymentAmount = s.TotalCollectionAmount,
                            AllowanceAmount = s.TotalAllowanceAmount
                        });
                    }
                }

                // 期前付款（用於計算期初餘額）
                var preSetoffs = allSetoffs
                    .Where(s => s.SetoffDate < startDate.Date)
                    .ToList();

                foreach (var s in preSetoffs)
                {
                    var totalPaid = s.TotalCollectionAmount + s.TotalAllowanceAmount + s.PrepaymentSetoffAmount;
                    if (totalPaid > 0)
                    {
                        prePeriodTransactions.Add(new StatementTransaction
                        {
                            SupplierId = s.RelatedPartyId,
                            DebitAmount = 0,
                            CreditAmount = totalPaid
                        });
                    }
                }
            }

            // === 合併所有廠商 ID ===
            var allSupplierIds = periodTransactions.Select(t => t.SupplierId)
                .Union(prePeriodTransactions.Select(t => t.SupplierId))
                .Distinct()
                .ToList();

            // === 按廠商分組 ===
            var groups = new List<SupplierStatementGroup>();

            foreach (var supplierId in allSupplierIds)
            {
                // 期前交易計算期初餘額
                var preTxs = prePeriodTransactions.Where(t => t.SupplierId == supplierId).ToList();
                var openingBalance = preTxs.Sum(t => t.DebitAmount) - preTxs.Sum(t => t.CreditAmount);

                // 期間內交易
                var periodTxs = periodTransactions
                    .Where(t => t.SupplierId == supplierId)
                    .OrderBy(t => t.TransactionDate)
                    .ThenBy(t => t.DocumentCode)
                    .ToList();

                // 如果該廠商期初為 0 且期間內沒有交易，跳過
                if (openingBalance == 0 && !periodTxs.Any())
                    continue;

                // 取得廠商資訊
                var sampleTx = periodTxs.FirstOrDefault() ?? preTxs.FirstOrDefault();
                var supplierCode = sampleTx?.SupplierCode ?? "";
                var supplierName = sampleTx?.SupplierName ?? "未知廠商";

                // 計算累計餘額
                var runningBalance = openingBalance;
                foreach (var tx in periodTxs)
                {
                    runningBalance += tx.DebitAmount - tx.CreditAmount;
                    tx.RunningBalance = runningBalance;
                }

                var closingBalance = runningBalance;

                groups.Add(new SupplierStatementGroup
                {
                    SupplierId = supplierId,
                    SupplierCode = supplierCode,
                    SupplierName = supplierName,
                    OpeningBalance = openingBalance,
                    ClosingBalance = closingBalance,
                    Transactions = periodTxs,
                    TotalDebit = periodTxs.Sum(t => t.DebitAmount),
                    TotalCredit = periodTxs.Sum(t => t.CreditAmount),
                    ReceivingCount = periodTxs.Count(t => t.TransactionType == "進貨"),
                    ReturnCount = periodTxs.Count(t => t.TransactionType == "退貨"),
                    PaymentCount = periodTxs.Count(t => t.TransactionType == "付款")
                });
            }

            return groups.OrderBy(g => g.SupplierCode).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構廠商對帳單報表
        /// </summary>
        private FormattedDocument BuildStatementDocument(
            List<SupplierStatementGroup> groups,
            Company? company,
            SupplierStatementCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddYears(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";
            var totalRecords = groups.Sum(g => g.Transactions.Count);

            var doc = new FormattedDocument()
                .SetDocumentName($"廠商對帳單-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("廠 商 對 帳 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"對帳期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"廠商數：{groups.Count} / 交易筆數：{totalRecords}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 各廠商對帳明細 ===
            var grandTotalDebit = 0m;
            var grandTotalCredit = 0m;
            var grandReceivingCount = 0;
            var grandReturnCount = 0;
            var grandPaymentCount = 0;

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                grandTotalDebit += group.TotalDebit;
                grandTotalCredit += group.TotalCredit;
                grandReceivingCount += group.ReceivingCount;
                grandReturnCount += group.ReturnCount;
                grandPaymentCount += group.PaymentCount;

                // 廠商標題
                doc.AddText($"【{group.SupplierCode}】{group.SupplierName}", fontSize: 11, bold: true);
                doc.AddSpacing(3);

                // 期初餘額
                doc.AddText($"期初餘額：{group.OpeningBalance:N0}", fontSize: 9);
                doc.AddSpacing(3);

                // 該廠商的交易表格
                doc.AddTable(table =>
                {
                    table.AddColumn("日期", 0.7f, TextAlignment.Center)
                         .AddColumn("單號", 1.0f, TextAlignment.Left)
                         .AddColumn("類型", 0.4f, TextAlignment.Center)
                         .AddColumn("應付金額", 0.9f, TextAlignment.Right)
                         .AddColumn("付款金額", 0.9f, TextAlignment.Right)
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
                        $"小計（進貨 {group.ReceivingCount} / 退貨 {group.ReturnCount} / 付款 {group.PaymentCount}）",
                        "",
                        group.TotalDebit.ToString("N0"),
                        group.TotalCredit.ToString("N0"),
                        ""
                    );
                });

                // 期末餘額
                doc.AddSpacing(2);
                doc.AddText($"期末餘額：{group.ClosingBalance:N0}", fontSize: 9, bold: true);

                // 廠商間分隔
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
                    $"廠商總數：{groups.Count} 家",
                    $"進貨筆數：{grandReceivingCount:N0} 筆",
                    $"退貨筆數：{grandReturnCount:N0} 筆",
                    $"付款筆數：{grandPaymentCount:N0} 筆",
                    $"本期應付合計：{grandTotalDebit:N0}",
                    $"本期付款合計：{grandTotalCredit:N0}",
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
            public int SupplierId { get; set; }
            public string SupplierCode { get; set; } = "";
            public string SupplierName { get; set; } = "";
            public DateTime TransactionDate { get; set; }
            public string DocumentCode { get; set; } = "";
            public string TransactionType { get; set; } = "";
            /// <summary>
            /// 應付金額（進貨增加應付）
            /// </summary>
            public decimal DebitAmount { get; set; }
            /// <summary>
            /// 沖銷金額（退貨、付款減少應付）
            /// </summary>
            public decimal CreditAmount { get; set; }
            /// <summary>
            /// 累計餘額
            /// </summary>
            public decimal RunningBalance { get; set; }
            /// <summary>
            /// 實際付款金額（僅付款類型使用）
            /// </summary>
            public decimal PaymentAmount { get; set; }
            /// <summary>
            /// 折讓金額（僅付款類型使用）
            /// </summary>
            public decimal AllowanceAmount { get; set; }
        }

        /// <summary>
        /// 廠商對帳單分組
        /// </summary>
        private class SupplierStatementGroup
        {
            public int SupplierId { get; set; }
            public string SupplierCode { get; set; } = "";
            public string SupplierName { get; set; } = "";
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
            /// 本期應付合計（借方）
            /// </summary>
            public decimal TotalDebit { get; set; }
            /// <summary>
            /// 本期沖銷合計（貸方）
            /// </summary>
            public decimal TotalCredit { get; set; }
            public int ReceivingCount { get; set; }
            public int ReturnCount { get; set; }
            public int PaymentCount { get; set; }
        }

        #endregion
    }
}
