using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 沖款單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 同時支援應收沖款單（FN003）和應付沖款單（FN004），根據 SetoffType 自動切換標題與佈局
    /// </summary>
    public class SetoffDocumentReportService : ISetoffDocumentReportService
    {
        private readonly ISetoffDocumentService _setoffDocumentService;
        private readonly ISetoffPaymentService _setoffPaymentService;
        private readonly ISetoffProductDetailService _setoffProductDetailService;
        private readonly ISetoffPrepaymentUsageService _setoffPrepaymentUsageService;
        private readonly ICustomerService _customerService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IBankService _bankService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SetoffDocumentReportService>? _logger;

        public SetoffDocumentReportService(
            ISetoffDocumentService setoffDocumentService,
            ISetoffPaymentService setoffPaymentService,
            ISetoffProductDetailService setoffProductDetailService,
            ISetoffPrepaymentUsageService setoffPrepaymentUsageService,
            ICustomerService customerService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            IBankService bankService,
            IPaymentMethodService paymentMethodService,
            IFormattedPrintService formattedPrintService,
            ILogger<SetoffDocumentReportService>? logger = null)
        {
            _setoffDocumentService = setoffDocumentService;
            _setoffPaymentService = setoffPaymentService;
            _setoffProductDetailService = setoffProductDetailService;
            _setoffPrepaymentUsageService = setoffPrepaymentUsageService;
            _customerService = customerService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _bankService = bankService;
            _paymentMethodService = paymentMethodService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成沖款單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int setoffDocumentId)
        {
            // 載入主單據
            var setoffDocument = await _setoffDocumentService.GetByIdAsync(setoffDocumentId);
            if (setoffDocument == null)
            {
                throw new ArgumentException($"找不到沖款單 ID: {setoffDocumentId}");
            }

            // 載入沖銷商品明細
            var productDetails = await _setoffProductDetailService.GetBySetoffDocumentIdAsync(setoffDocumentId);

            // 載入收款/付款記錄
            var payments = await _setoffPaymentService.GetBySetoffDocumentIdAsync(setoffDocumentId);

            // 載入預收付款項使用記錄
            var prepaymentUsages = await _setoffPrepaymentUsageService.GetBySetoffDocumentIdAsync(setoffDocumentId);

            // 載入關聯方（客戶或廠商）
            Customer? customer = null;
            Supplier? supplier = null;
            if (setoffDocument.IsAccountsReceivable && setoffDocument.RelatedPartyId > 0)
            {
                customer = await _customerService.GetByIdAsync(setoffDocument.RelatedPartyId);
            }
            else if (setoffDocument.IsAccountsPayable && setoffDocument.RelatedPartyId > 0)
            {
                supplier = await _supplierService.GetByIdAsync(setoffDocument.RelatedPartyId);
            }

            // 載入公司資訊
            var company = await _companyService.GetPrimaryCompanyAsync();

            // 載入商品字典
            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            // 載入銀行字典
            var allBanks = await _bankService.GetAllAsync();
            var bankDict = allBanks.ToDictionary(b => b.Id, b => b);

            // 載入付款方式字典
            var allPaymentMethods = await _paymentMethodService.GetAllAsync();
            var paymentMethodDict = allPaymentMethods.ToDictionary(pm => pm.Id, pm => pm);

            // 建構格式化文件
            return BuildFormattedDocument(
                setoffDocument, productDetails, payments, prepaymentUsages,
                customer, supplier, company,
                productDict, bankDict, paymentMethodDict);
        }

        /// <summary>
        /// 直接列印沖款單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int setoffDocumentId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(setoffDocumentId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印沖款單 {SetoffDocumentId} 時發生錯誤", setoffDocumentId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int setoffDocumentId)
        {
            var document = await GenerateReportAsync(setoffDocumentId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int setoffDocumentId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(setoffDocumentId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 批次直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var setoffDocuments = await GetSetoffDocumentsByCriteriaAsync(criteria);

                if (setoffDocuments == null || !setoffDocuments.Any())
                {
                    return ServiceResult.Failure($"無符合條件的沖款單\n篩選條件：{criteria.GetSummary()}");
                }

                foreach (var doc in setoffDocuments)
                {
                    var result = await DirectPrintAsync(doc.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中沖款單 {SetoffDocumentId} 失敗：{ErrorMessage}", doc.Id, result.ErrorMessage);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印沖款單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（用於批次預覽）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var entities = await GetSetoffDocumentsByCriteriaAsync(criteria);

            return await BatchReportHelper.RenderBatchToImagesAsync(
                entities,
                (id, _) => GenerateReportAsync(id),
                _formattedPrintService,
                "沖款單",
                criteria.PaperSetting,
                criteria.GetSummary(),
                _logger);
        }

        #endregion

        #region 私有方法 - 查詢批次資料

        /// <summary>
        /// 根據批次列印條件查詢沖款單
        /// </summary>
        private async Task<List<SetoffDocument>> GetSetoffDocumentsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            List<SetoffDocument> results;

            // 優先使用日期範圍篩選
            if (criteria.StartDate.HasValue && criteria.EndDate.HasValue)
            {
                results = await _setoffDocumentService.GetByDateRangeAsync(
                    criteria.StartDate.Value, criteria.EndDate.Value);
            }
            else
            {
                results = await _setoffDocumentService.GetAllAsync();
            }

            // 篩選關聯方（客戶或廠商）
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(d => criteria.RelatedEntityIds.Contains(d.RelatedPartyId)).ToList();
            }

            // 篩選公司
            if (criteria.CompanyId.HasValue)
            {
                results = results.Where(d => d.CompanyId == criteria.CompanyId.Value).ToList();
            }

            // 單據編號關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                results = results.Where(d =>
                    d.Code != null && d.Code.Contains(criteria.DocumentNumberKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 根據 ReportType 篩選沖款類型
            if (!string.IsNullOrEmpty(criteria.ReportType))
            {
                if (criteria.ReportType == ReportIds.AccountsReceivableSetoff)
                {
                    results = results.Where(d => d.SetoffType == SetoffType.AccountsReceivable).ToList();
                }
                else if (criteria.ReportType == ReportIds.AccountsPayableSetoff)
                {
                    results = results.Where(d => d.SetoffType == SetoffType.AccountsPayable).ToList();
                }
            }

            // 最大筆數限制
            if (criteria.MaxResults.HasValue)
            {
                results = results.Take(criteria.MaxResults.Value).ToList();
            }

            return results;
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildFormattedDocument(
            SetoffDocument setoffDocument,
            List<SetoffProductDetail> productDetails,
            List<SetoffPayment> payments,
            List<SetoffPrepaymentUsage> prepaymentUsages,
            Customer? customer,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Bank> bankDict,
            Dictionary<int, PaymentMethod> paymentMethodDict)
        {
            // 根據沖款類型決定標題和相關文字
            var isReceivable = setoffDocument.IsAccountsReceivable;
            var reportTitle = isReceivable ? "應 收 沖 款 單" : "應 付 沖 款 單";
            var partyLabel = isReceivable ? "客戶名稱" : "廠商名稱";
            var partyName = isReceivable
                ? (customer?.CompanyName ?? setoffDocument.RelatedPartyName)
                : (supplier?.CompanyName ?? setoffDocument.RelatedPartyName);
            var partyTaxNumber = isReceivable
                ? (customer?.TaxNumber ?? "")
                : (supplier?.TaxNumber ?? "");
            var partyContact = isReceivable
                ? (customer?.ContactPerson ?? "")
                : (supplier?.ContactPerson ?? "");
            var partyPhone = isReceivable
                ? (customer?.ContactPhone ?? "")
                : (supplier?.ContactPhone ?? supplier?.SupplierContactPhone ?? "");
            var collectionLabel = isReceivable ? "收款" : "付款";
            var docNamePrefix = isReceivable ? "應收沖款單" : "應付沖款單";

            var doc = new FormattedDocument()
                .SetDocumentName($"{docNamePrefix}-{setoffDocument.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 中間公司名稱+報表標題（置中），右側單號/日期/頁次
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        (reportTitle, 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{setoffDocument.Code ?? ""}",
                        $"日期：{setoffDocument.SetoffDate:yyyy/MM/dd}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(3);

                // === 公司聯絡資訊 ===
                header.AddKeyValueRow(
                    ("電話", company?.Phone ?? ""),
                    ("傳真", company?.Fax ?? ""));

                header.AddKeyValueRow(
                    ("地址", company?.Address ?? ""));

                header.AddSpacing(3);

                // === 關聯方資訊（客戶或廠商）===
                header.AddKeyValueRow(
                    (partyLabel, partyName),
                    ("統一編號", partyTaxNumber),
                    ("聯絡人", partyContact),
                    ("連絡電話", partyPhone));

                header.AddSpacing(3);
            });

            // === 沖銷商品明細表格（主要內容）===
            if (productDetails.Any())
            {
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                         .AddColumn("來源單號", 1.2f, Models.Reports.TextAlignment.Left)
                         .AddColumn("來源類型", 0.8f, Models.Reports.TextAlignment.Center)
                         .AddColumn("品名", 1.5f, Models.Reports.TextAlignment.Left)
                         .AddColumn("沖款金額", 1.0f, Models.Reports.TextAlignment.Right)
                         .AddColumn("折讓金額", 1.0f, Models.Reports.TextAlignment.Right)
                         .AddColumn("累計沖款", 1.0f, Models.Reports.TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var detail in productDetails)
                    {
                        var product = productDict.GetValueOrDefault(detail.ProductId);
                        var sourceTypeText = GetSourceDetailTypeText(detail.SourceDetailType);

                        table.AddRow(
                            rowNum.ToString(),
                            $"#{detail.SourceDetailId}",
                            sourceTypeText,
                            product?.Name ?? "",
                            NumberFormatHelper.FormatSmart(detail.CurrentSetoffAmount),
                            NumberFormatHelper.FormatSmart(detail.CurrentAllowanceAmount),
                            NumberFormatHelper.FormatSmart(detail.TotalSetoffAmount)
                        );
                        rowNum++;
                    }
                });
            }

            // === 收款/付款明細區 ===
            if (payments.Any())
            {
                doc.AddSpacing(10);
                doc.AddText($"【{collectionLabel}明細】", 11, Models.Reports.TextAlignment.Left, true);
                doc.AddSpacing(3);

                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                         .AddColumn($"{collectionLabel}方式", 1.2f, Models.Reports.TextAlignment.Left)
                         .AddColumn("銀行", 1.2f, Models.Reports.TextAlignment.Left)
                         .AddColumn($"{collectionLabel}金額", 1.0f, Models.Reports.TextAlignment.Right)
                         .AddColumn("折讓金額", 1.0f, Models.Reports.TextAlignment.Right)
                         .AddColumn("票號", 1.0f, Models.Reports.TextAlignment.Left)
                         .AddColumn("到期日", 1.0f, Models.Reports.TextAlignment.Center)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var payment in payments)
                    {
                        var paymentMethodName = payment.PaymentMethodId.HasValue
                            ? paymentMethodDict.GetValueOrDefault(payment.PaymentMethodId.Value)?.Name ?? ""
                            : "";
                        var bankName = payment.BankId.HasValue
                            ? bankDict.GetValueOrDefault(payment.BankId.Value)?.BankName ?? ""
                            : "";

                        table.AddRow(
                            rowNum.ToString(),
                            paymentMethodName,
                            bankName,
                            NumberFormatHelper.FormatSmart(payment.ReceivedAmount),
                            NumberFormatHelper.FormatSmart(payment.AllowanceAmount),
                            payment.CheckNumber ?? "",
                            payment.DueDate?.ToString("yyyy/MM/dd") ?? ""
                        );
                        rowNum++;
                    }
                });
            }

            // === 預收付款項沖抵明細 ===
            if (prepaymentUsages.Any())
            {
                var prepaymentLabel = isReceivable ? "預收款項沖抵" : "預付款項沖抵";
                doc.AddSpacing(10);
                doc.AddText($"【{prepaymentLabel}明細】", 11, Models.Reports.TextAlignment.Left, true);
                doc.AddSpacing(3);

                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                         .AddColumn("來源單號", 2.0f, Models.Reports.TextAlignment.Left)
                         .AddColumn("沖抵金額", 1.2f, Models.Reports.TextAlignment.Right)
                         .AddColumn("沖抵日期", 1.2f, Models.Reports.TextAlignment.Center)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var usage in prepaymentUsages)
                    {
                        table.AddRow(
                            rowNum.ToString(),
                            usage.SourceDocumentCode,
                            NumberFormatHelper.FormatSmart(usage.UsedAmount),
                            usage.UsageDate.ToString("yyyy/MM/dd")
                        );
                        rowNum++;
                    }
                });
            }

            // === 頁尾區（只在最後一頁顯示）===
            doc.BeginFooter(footer =>
            {
                // 合計區（說明在左、金額在右）
                var leftLines = new List<string>();
                if (!string.IsNullOrWhiteSpace(setoffDocument.Remarks))
                {
                    leftLines.Add("【備註】");
                    leftLines.Add(setoffDocument.Remarks);
                }

                var amountLines = new List<string>
                {
                    $"本期應沖金額：{NumberFormatHelper.FormatSmart(setoffDocument.TotalSetoffAmount)}",
                    $"本期沖款金額：{NumberFormatHelper.FormatSmart(setoffDocument.CurrentSetoffAmount)}",
                    $"{collectionLabel}合計：{NumberFormatHelper.FormatSmart(setoffDocument.TotalCollectionAmount)}",
                    $"折讓合計：{NumberFormatHelper.FormatSmart(setoffDocument.TotalAllowanceAmount)}"
                };

                // 有預收付沖抵時顯示
                if (setoffDocument.PrepaymentSetoffAmount > 0)
                {
                    var prepaymentSetoffLabel = isReceivable ? "預收沖抵" : "預付沖抵";
                    amountLines.Add($"{prepaymentSetoffLabel}：{NumberFormatHelper.FormatSmart(setoffDocument.PrepaymentSetoffAmount)}");
                }

                footer.AddSpacing(5)
                      .AddTwoColumnSection(
                          leftContent: leftLines,
                          leftTitle: null,
                          leftHasBorder: false,
                          rightContent: amountLines,
                          leftWidthRatio: 0.6f);

                // 簽名區
                var signatureLabels = isReceivable
                    ? new[] { "製單人員", "會計人員", "核准人員" }
                    : new[] { "製單人員", "會計人員", "核准人員" };
                footer.AddSpacing(20)
                      .AddSignatureSection(signatureLabels);
            });

            return doc;
        }

        /// <summary>
        /// 取得來源明細類型的顯示文字
        /// </summary>
        private static string GetSourceDetailTypeText(SetoffDetailType sourceDetailType)
        {
            return sourceDetailType switch
            {
                SetoffDetailType.SalesOrderDetail => "銷貨",
                SetoffDetailType.SalesReturnDetail => "銷退",
                SetoffDetailType.PurchaseReceivingDetail => "進貨",
                SetoffDetailType.PurchaseReturnDetail => "進退",
                SetoffDetailType.SalesDeliveryDetail => "出貨",
                _ => sourceDetailType.ToString()
            };
        }

        #endregion
    }
}