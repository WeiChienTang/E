using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 報價單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public class QuotationReportService : IQuotationReportService
    {
        private readonly IQuotationService _quotationService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<QuotationReportService>? _logger;

        public QuotationReportService(
            IQuotationService quotationService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            IProductService productService,
            IUnitService unitService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<QuotationReportService>? logger = null)
        {
            _quotationService = quotationService;
            _customerService = customerService;
            _employeeService = employeeService;
            _productService = productService;
            _unitService = unitService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成報價單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int quotationId)
        {
            // 載入資料
            var quotation = await _quotationService.GetWithDetailsAsync(quotationId);
            if (quotation == null)
            {
                throw new ArgumentException($"找不到報價單 ID: {quotationId}");
            }

            var quotationDetails = quotation.QuotationDetails?.ToList() ?? new List<QuotationDetail>();

            Customer? customer = null;
            if (quotation.CustomerId > 0)
            {
                customer = await _customerService.GetByIdAsync(quotation.CustomerId);
            }

            Employee? employee = null;
            if (quotation.EmployeeId.HasValue && quotation.EmployeeId.Value > 0)
            {
                employee = await _employeeService.GetByIdAsync(quotation.EmployeeId.Value);
            }

            Company? company = await _companyService.GetPrimaryCompanyAsync();

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allUnits = await _unitService.GetAllAsync();
            var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

            // 建構格式化文件
            return BuildFormattedDocument(quotation, quotationDetails, customer, employee, company, productDict, unitDict);
        }

        /// <summary>
        /// 直接列印報價單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int quotationId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(quotationId);

                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印報價單 {QuotationId} 時發生錯誤", quotationId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int quotationId)
        {
            var document = await GenerateReportAsync(quotationId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int quotationId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(quotationId);
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
                // 根據條件查詢報價單
                var quotations = await _quotationService.GetByBatchCriteriaAsync(criteria);

                if (quotations == null || !quotations.Any())
                {
                    return ServiceResult.Failure($"無符合條件的報價單\n篩選條件：{criteria.GetSummary()}");
                }

                // 逐一列印
                foreach (var quotation in quotations)
                {
                    var result = await DirectPrintAsync(quotation.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中報價單 {QuotationId} 失敗：{ErrorMessage}", quotation.Id, result.ErrorMessage);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印報價單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（用於批次預覽）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var entities = await _quotationService.GetByBatchCriteriaAsync(criteria);

            return await BatchReportHelper.RenderBatchToImagesAsync(
                entities,
                (id, _) => GenerateReportAsync(id),
                _formattedPrintService,
                "報價單",
                criteria.PaperSetting,
                criteria.GetSummary(),
                _logger);
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildFormattedDocument(
            Quotation quotation,
            List<QuotationDetail> quotationDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"報價單-{quotation.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f); // 縮小邊距，尤其是上下邊距

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 左側留空（給公司圖），中間公司名稱+報價單（置中），右側單號/日期/頁次（緊湊排列）
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("報 價 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{quotation.Code ?? ""}",
                        $"日期：{quotation.QuotationDate:yyyy/MM/dd}",
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

                // === 客戶資訊區（第一行）===
                header.AddKeyValueRow(
                    ("客戶名稱", customer?.CompanyName ?? ""),
                    ("統一編號", customer?.TaxNumber ?? ""),
                    ("聯絡人", customer?.ContactPerson ?? ""),
                    ("連絡電話", customer?.ContactPhone ?? ""));

                // === 客戶資訊區（第二行）===
                header.AddKeyValueRow(
                    ("聯絡地址", customer?.ContactAddress ?? ""));

                // === 工程名稱和業務員 ===
                if (!string.IsNullOrWhiteSpace(quotation.ProjectName))
                {
                    header.AddKeyValueRow(
                        ("工程名稱", quotation.ProjectName),
                        ("業務員", employee?.Name ?? ""));
                }
                else
                {
                    header.AddKeyValueRow(
                        ("業務員", employee?.Name ?? ""));
                }

                header.AddSpacing(3);
            });

            // === 明細表格（主要內容）===
            doc.AddTable(table =>
            {
                // 定義欄位
                table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                     .AddColumn("品名/規格", 2.0f, Models.Reports.TextAlignment.Left)
                     .AddColumn("單位", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("數量", 0.7f, Models.Reports.TextAlignment.Right)
                     .AddColumn("單價", 0.8f, Models.Reports.TextAlignment.Right)
                     .AddColumn("總價", 0.9f, Models.Reports.TextAlignment.Right)
                     .AddColumn("備註", 1.6f, Models.Reports.TextAlignment.Left)
                     .ShowBorder(false)              // 不顯示表格邊框
                     .ShowHeaderBackground(false)    // 不顯示表頭背景
                     .ShowHeaderSeparator(false)     // 不顯示表頭 | 分隔符
                     .SetRowHeight(20);

                // 新增資料列
                int rowNum = 1;
                foreach (var detail in quotationDetails)
                {
                    var product = productDict.GetValueOrDefault(detail.ProductId);
                    var unit = detail.UnitId.HasValue ? unitDict.GetValueOrDefault(detail.UnitId.Value) : null;
                    
                    // 組合商品名稱與規格說明
                    var productName = product?.Name ?? "";
                    var specification = detail.Specification ?? product?.Specification ?? "";
                    var displayName = string.IsNullOrEmpty(specification) 
                        ? productName 
                        : $"{productName}\n規格：{specification}";

                    // BOM 組成明細
                    if (product != null && ShouldShowBom(detail, product))
                    {
                        var bomText = GetBomText(detail);
                        if (!string.IsNullOrEmpty(bomText))
                        {
                            displayName += $"\n組成：{bomText}";
                        }
                    }

                    table.AddRow(
                        rowNum.ToString(),
                        displayName,
                        unit?.Name ?? "",
                        NumberFormatHelper.FormatSmart(detail.Quantity),
                        NumberFormatHelper.FormatSmart(detail.UnitPrice),
                        NumberFormatHelper.FormatSmart(detail.SubtotalAmount),
                        detail.Remarks ?? ""
                    );
                    rowNum++;
                }
            });

            // === 頁尾區（只在最後一頁顯示）===
            doc.BeginFooter(footer =>
            {
                // 稅別說明
                var taxMethodText = quotation.TaxCalculationMethod switch
                {
                    TaxCalculationMethod.TaxExclusive => "外加稅",
                    TaxCalculationMethod.TaxInclusive => "內含稅",
                    TaxCalculationMethod.NoTax => "免稅",
                    _ => ""
                };

                // 合計區（說明在左、金額在右）
                var leftLines = new List<string>();
                leftLines.Add("【報價說明】");
                if (!string.IsNullOrWhiteSpace(quotation.PaymentTerms))
                {
                    leftLines.Add($"付款條件：{quotation.PaymentTerms}");
                }
                if (!string.IsNullOrWhiteSpace(quotation.DeliveryTerms))
                {
                    leftLines.Add($"交貨條件：{quotation.DeliveryTerms}");
                }
                if (!string.IsNullOrWhiteSpace(quotation.Remarks))
                {
                    leftLines.Add($"備　　註：{quotation.Remarks}");
                }

                var amountLines = new List<string>
                {
                    $"小　計：{NumberFormatHelper.FormatSmart(quotation.SubtotalBeforeDiscount)}",
                    $"折　扣：{NumberFormatHelper.FormatSmart(quotation.DiscountAmount)}",
                    $"稅　額：{NumberFormatHelper.FormatSmart(quotation.QuotationTaxAmount)} ({taxMethodText})",
                    $"總　計：{NumberFormatHelper.FormatSmart(quotation.TotalAmount)}"
                };

                footer.AddSpacing(5)
                      .AddTwoColumnSection(
                          leftContent: leftLines,
                          leftTitle: null,
                          leftHasBorder: false,
                          rightContent: amountLines,
                          leftWidthRatio: 0.7f);

                // 簽名區
                footer.AddSpacing(20)
                      .AddSignatureSection("業務代表", "主管核准", "客戶確認");
            });

            return doc;
        }

        /// <summary>
        /// 判斷是否應該在報價單列印時顯示 BOM 組成
        /// </summary>
        private static bool ShouldShowBom(QuotationDetail detail, Product product)
        {
            if (detail.CompositionDetails == null || !detail.CompositionDetails.Any())
                return false;
            
            return detail.CompositionDetails.Any();
        }

        /// <summary>
        /// 取得 BOM 組成文字（橫向顯示）
        /// </summary>
        private static string GetBomText(QuotationDetail detail)
        {
            var compositions = detail.CompositionDetails?.ToList() ?? new List<QuotationCompositionDetail>();
            
            if (!compositions.Any()) return "";
            
            var componentNames = compositions
                .Select(comp => comp.ComponentProduct?.Name ?? "")
                .Where(name => !string.IsNullOrEmpty(name));
            
            return string.Join("、", componentNames);
        }

        #endregion
    }
}
