using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 報價單報表服務實作 - 純文字版本
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public class QuotationReportService : IQuotationReportService
    {
        private readonly IQuotationService _quotationService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPlainTextPrintService _plainTextPrintService;
        private readonly ILogger<QuotationReportService>? _logger;

        public QuotationReportService(
            IQuotationService quotationService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            IProductService productService,
            IUnitService unitService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService,
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPlainTextPrintService plainTextPrintService,
            ILogger<QuotationReportService>? logger = null)
        {
            _quotationService = quotationService;
            _customerService = customerService;
            _employeeService = employeeService;
            _productService = productService;
            _unitService = unitService;
            _companyService = companyService;
            _systemParameterService = systemParameterService;
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _plainTextPrintService = plainTextPrintService;
            _logger = logger;
        }

        #region 純文字報表生成

        /// <summary>
        /// 生成純文字格式的報價單報表
        /// 直接生成格式化的純文字，適合直接列印和預覽
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int quotationId)
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

            // 生成純文字報表
            return GeneratePlainTextContent(quotation, quotationDetails, customer, employee, company, productDict, unitDict);
        }

        /// <summary>
        /// 批次生成純文字報表（支援多條件篩選）
        /// </summary>
        public async Task<string> GenerateBatchPlainTextReportAsync(BatchPrintCriteria criteria)
        {
            try
            {
                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"批次列印條件驗證失敗：{validation.GetAllErrors()}");
                }

                // 根據條件查詢報價單
                var quotations = await _quotationService.GetByBatchCriteriaAsync(criteria);

                if (quotations == null || !quotations.Any())
                {
                    return $"無符合條件的報價單\n篩選條件：{criteria.GetSummary()}";
                }

                // 載入共用資料
                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var allUnits = await _unitService.GetAllAsync();
                var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

                var sb = new StringBuilder();
                var pageBreak = "\f"; // Form Feed 字元，用於分頁

                for (int i = 0; i < quotations.Count; i++)
                {
                    var quotation = quotations[i];

                    // 載入該報價單的相關資料
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

                    // 生成該報價單的純文字內容
                    sb.Append(GeneratePlainTextContent(quotation, quotationDetails, customer, employee, company, productDict, unitDict));

                    // 加入分頁符號（最後一張不需要）
                    if (i < quotations.Count - 1)
                    {
                        sb.Append(pageBreak);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次報價單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成純文字內容（固定寬度格式，適合等寬字型列印）
        /// </summary>
        private static string GeneratePlainTextContent(
            Quotation quotation,
            List<QuotationDetail> quotationDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var sb = new StringBuilder();
            const int lineWidth = PlainTextFormatter.DefaultLineWidth;

            // === 標題區 ===
            sb.Append(PlainTextFormatter.BuildTitleSection(
                company?.CompanyName ?? "公司名稱",
                "報 價 單",
                lineWidth));

            // === 公司聯絡資訊 ===
            sb.AppendLine($"電話：{company?.Phone ?? ""}  傳真：{company?.Fax ?? ""}");
            sb.AppendLine($"地址：{company?.Address ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 基本資訊區 ===
            sb.AppendLine($"報價單號：{quotation.Code,-20} 報價日期：{PlainTextFormatter.FormatDate(quotation.QuotationDate)}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine($"客戶名稱：{customer?.CompanyName ?? ""}");
            sb.AppendLine($"聯 絡 人：{customer?.ContactPerson ?? "",-20} 統一編號：{customer?.TaxNumber ?? ""}");
            sb.AppendLine($"連絡電話：{customer?.ContactPhone ?? ""}");
            sb.AppendLine($"聯絡地址：{customer?.ContactAddress ?? ""}");
            if (!string.IsNullOrWhiteSpace(quotation.ProjectName))
            {
                sb.AppendLine($"工程名稱：{quotation.ProjectName}");
            }
            sb.AppendLine($"業 務 員：{employee?.Name ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細表頭 ===
            // 項次(4) | 品名/規格(32) | 單位(6) | 數量(8) | 單價(10) | 總價(12) | 備註(8)
            sb.AppendLine(FormatTableRow("項次", "品名/規格", "單位", "數量", "單價", "總價", "備註"));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
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
                    : $"{productName}";
                displayName = PlainTextFormatter.TruncateText(displayName, 30);
                var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", 6);

                sb.AppendLine(FormatTableRow(
                    rowNum.ToString(),
                    displayName,
                    unit?.Name ?? "",
                    PlainTextFormatter.FormatQuantity(detail.Quantity),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
                    remarks
                ));

                // 如果有規格說明，另起一行顯示
                if (!string.IsNullOrEmpty(specification))
                {
                    var specText = PlainTextFormatter.TruncateText($"  規格：{specification}", 76);
                    sb.AppendLine($"    {specText}");
                }

                // BOM 組成明細（如果需要顯示）
                if (product != null && ShouldShowBom(detail, product))
                {
                    var bomText = GetBomText(detail);
                    if (!string.IsNullOrEmpty(bomText))
                    {
                        sb.AppendLine($"    組成：{bomText}");
                    }
                }

                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區 ===
            var taxMethodText = quotation.TaxCalculationMethod switch
            {
                TaxCalculationMethod.TaxExclusive => "外加稅",
                TaxCalculationMethod.TaxInclusive => "內含稅",
                TaxCalculationMethod.NoTax => "免稅",
                _ => ""
            };

            sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", quotation.SubtotalBeforeDiscount));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("折　扣", quotation.DiscountAmount));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", quotation.QuotationTaxAmount, taxMethodText));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", quotation.TotalAmount));

            // === 說明區 ===
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine("【報價說明】");
            
            if (!string.IsNullOrWhiteSpace(quotation.PaymentTerms))
            {
                sb.AppendLine($"付款條件：{quotation.PaymentTerms}");
            }
            if (!string.IsNullOrWhiteSpace(quotation.DeliveryTerms))
            {
                sb.AppendLine($"交貨條件：{quotation.DeliveryTerms}");
            }
            if (!string.IsNullOrWhiteSpace(quotation.Remarks))
            {
                sb.AppendLine($"備　　註：{quotation.Remarks}");
            }

            sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

            // === 簽名區 ===
            sb.Append(PlainTextFormatter.BuildSignatureSection(
                new[] { "業務代表", "主管核准", "客戶確認" },
                lineWidth));

            return sb.ToString();
        }

        /// <summary>
        /// 判斷是否應該在報價單列印時顯示 BOM 組成
        /// </summary>
        private static bool ShouldShowBom(QuotationDetail detail, Product product)
        {
            if (detail.CompositionDetails == null || !detail.CompositionDetails.Any())
                return false;
            
            return detail.CompositionDetails.Any(cd => 
                cd.ComponentProduct?.ShowBomOnPrint == true);
        }

        /// <summary>
        /// 取得 BOM 組成文字（橫向顯示）
        /// </summary>
        private static string GetBomText(QuotationDetail detail)
        {
            var compositions = detail.CompositionDetails?
                .Where(cd => cd.ComponentProduct?.ShowBomOnPrint == true)
                .ToList() ?? new List<QuotationCompositionDetail>();
            
            if (!compositions.Any()) return "";
            
            var componentNames = compositions
                .Select(comp => comp.ComponentProduct?.Name ?? "")
                .Where(name => !string.IsNullOrEmpty(name));
            
            return string.Join("、", componentNames);
        }

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印報價單（使用 System.Drawing.Printing）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int quotationId, string printerName)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(quotationId);

                _logger?.LogInformation("開始直接列印報價單 {QuotationId}，印表機：{PrinterName}", quotationId, printerName);

                // 使用共用的列印服務
                var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"報價單-{quotationId}");
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("報價單 {QuotationId} 列印完成", quotationId);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印報價單 {QuotationId} 時發生錯誤", quotationId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印報價單（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintByReportIdAsync(int quotationId, string reportId)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(quotationId);

                _logger?.LogInformation("開始列印報價單 {QuotationId}，使用配置：{ReportId}", quotationId, reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"報價單-{quotationId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印報價單 {QuotationId} 時發生錯誤，ReportId: {ReportId}", quotationId, reportId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                // 生成批次純文字報表
                var textContent = await GenerateBatchPlainTextReportAsync(criteria);

                _logger?.LogInformation("開始批次列印報價單，使用配置：{ReportId}", reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, "報價單批次列印");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印報價單時發生錯誤");
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 純文字格式化輔助方法（保留供表格格式化使用）

        /// <summary>
        /// 格式化表格行（固定寬度）- 報價單專用格式
        /// </summary>
        private static string FormatTableRow(string col1, string col2, string col3, string col4, string col5, string col6, string col7)
        {
            // 項次(4) | 品名/規格(32) | 單位(6) | 數量(8) | 單價(10) | 總價(12) | 備註(8)
            return PlainTextFormatter.FormatTableRow(new (string, int, PlainTextAlignment)[]
            {
                (col1, 4, PlainTextAlignment.Left),
                (col2, 32, PlainTextAlignment.Left),
                (col3, 6, PlainTextAlignment.Left),
                (col4, 8, PlainTextAlignment.Right),
                (col5, 10, PlainTextAlignment.Right),
                (col6, 12, PlainTextAlignment.Right),
                (col7, 8, PlainTextAlignment.Left)
            });
        }

        #endregion
    }
}
