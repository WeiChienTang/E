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
    /// 進貨退出單報表服務實作 - 純文字版本
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public class PurchaseReturnReportService : IPurchaseReturnReportService
    {
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPlainTextPrintService _plainTextPrintService;
        private readonly ILogger<PurchaseReturnReportService>? _logger;

        public PurchaseReturnReportService(
            IPurchaseReturnService purchaseReturnService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService,
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPlainTextPrintService plainTextPrintService,
            ILogger<PurchaseReturnReportService>? logger = null)
        {
            _purchaseReturnService = purchaseReturnService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _systemParameterService = systemParameterService;
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _plainTextPrintService = plainTextPrintService;
            _logger = logger;
        }

        #region 純文字報表生成

        /// <summary>
        /// 生成純文字格式的進貨退出單報表
        /// 直接生成格式化的純文字，適合直接列印和預覽
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int purchaseReturnId)
        {
            // 載入資料
            var purchaseReturn = await _purchaseReturnService.GetWithDetailsAsync(purchaseReturnId);
            if (purchaseReturn == null)
            {
                throw new ArgumentException($"找不到進貨退出單 ID: {purchaseReturnId}");
            }

            var returnDetails = purchaseReturn.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
            
            Supplier? supplier = null;
            if (purchaseReturn.SupplierId > 0)
            {
                supplier = await _supplierService.GetByIdAsync(purchaseReturn.SupplierId);
            }

            Company? company = await _companyService.GetPrimaryCompanyAsync();

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            decimal taxRate = 5.0m;
            try
            {
                taxRate = await _systemParameterService.GetTaxRateAsync();
            }
            catch
            {
                // 使用預設稅率
            }

            // 生成純文字報表
            return GeneratePlainTextContent(purchaseReturn, returnDetails, supplier, company, productDict, taxRate);
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

                // 根據條件查詢進貨退出單
                var purchaseReturns = await _purchaseReturnService.GetByBatchCriteriaAsync(criteria);

                if (purchaseReturns == null || !purchaseReturns.Any())
                {
                    return $"無符合條件的進貨退出單\n篩選條件：{criteria.GetSummary()}";
                }

                // 載入共用資料
                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                decimal taxRate = 5.0m;
                try
                {
                    taxRate = await _systemParameterService.GetTaxRateAsync();
                }
                catch
                {
                    // 使用預設稅率
                }

                var sb = new StringBuilder();
                var pageBreak = "\f"; // Form Feed 字元，用於分頁

                for (int i = 0; i < purchaseReturns.Count; i++)
                {
                    var purchaseReturn = purchaseReturns[i];

                    // 載入該進貨退出單的相關資料
                    var returnDetails = purchaseReturn.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
                    
                    Supplier? supplier = null;
                    if (purchaseReturn.SupplierId > 0)
                    {
                        supplier = await _supplierService.GetByIdAsync(purchaseReturn.SupplierId);
                    }

                    Company? company = await _companyService.GetPrimaryCompanyAsync();

                    // 生成該進貨退出單的純文字內容
                    sb.Append(GeneratePlainTextContent(purchaseReturn, returnDetails, supplier, company, productDict, taxRate));

                    // 加入分頁符號（最後一張不需要）
                    if (i < purchaseReturns.Count - 1)
                    {
                        sb.Append(pageBreak);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次進貨退出單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成純文字內容（固定寬度格式，適合等寬字型列印）
        /// </summary>
        private static string GeneratePlainTextContent(
            PurchaseReturn purchaseReturn,
            List<PurchaseReturnDetail> returnDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            decimal taxRate)
        {
            var sb = new StringBuilder();
            const int lineWidth = PlainTextFormatter.DefaultLineWidth;

            // === 標題區 ===
            sb.Append(PlainTextFormatter.BuildTitleSection(
                company?.CompanyName ?? "公司名稱",
                "進 貨 退 出 單",
                lineWidth));

            // === 基本資訊區 ===
            sb.AppendLine($"退出單號：{purchaseReturn.Code,-20} 退出日期：{PlainTextFormatter.FormatDate(purchaseReturn.ReturnDate)}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine($"廠商名稱：{supplier?.CompanyName ?? ""}");
            sb.AppendLine($"聯 絡 人：{supplier?.ContactPerson ?? "",-20} 統一編號：{supplier?.TaxNumber ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細表頭 ===
            // 序號(4) | 品名(30) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 備註(10)
            sb.AppendLine(FormatTableRow("序號", "品名", "退出數量", "單位", "單價", "小計", "備註"));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
            int rowNum = 1;
            foreach (var detail in returnDetails)
            {
                var product = productDict.GetValueOrDefault(detail.ProductId);
                var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", 28);
                var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", 8);

                sb.AppendLine(FormatTableRow(
                    rowNum.ToString(),
                    productName,
                    PlainTextFormatter.FormatQuantity(detail.ReturnQuantity),
                    "個",
                    PlainTextFormatter.FormatAmountWithDecimals(detail.OriginalUnitPrice),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.ReturnSubtotalAmount),
                    remarks
                ));
                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區 ===
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", purchaseReturn.TotalReturnAmount));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", purchaseReturn.ReturnTaxAmount, $"{taxRate:F2}%"));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", purchaseReturn.TotalReturnAmountWithTax));

            // === 備註 ===
            if (!string.IsNullOrWhiteSpace(purchaseReturn.Remarks))
            {
                sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
                sb.AppendLine($"備註：{purchaseReturn.Remarks}");
            }

            sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

            // === 簽名區 ===
            sb.Append(PlainTextFormatter.BuildSignatureSection(
                new[] { "處理人員", "倉管人員", "核准人員" },
                lineWidth));

            return sb.ToString();
        }

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印進貨退出單（使用 System.Drawing.Printing）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int purchaseReturnId, string printerName)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseReturnId);

                _logger?.LogInformation("開始直接列印進貨退出單 {OrderId}，印表機：{PrinterName}", purchaseReturnId, printerName);

                // 使用共用的列印服務
                var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"進貨退出單-{purchaseReturnId}");
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("進貨退出單 {OrderId} 列印完成", purchaseReturnId);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印進貨退出單 {OrderId} 時發生錯誤", purchaseReturnId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印進貨退出單（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseReturnId, string reportId)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseReturnId);

                _logger?.LogInformation("開始列印進貨退出單 {OrderId}，使用配置：{ReportId}", purchaseReturnId, reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"進貨退出單-{purchaseReturnId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印進貨退出單 {OrderId} 時發生錯誤，ReportId: {ReportId}", purchaseReturnId, reportId);
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

                _logger?.LogInformation("開始批次列印進貨退出單，使用配置：{ReportId}", reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, "進貨退出單批次列印");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印進貨退出單時發生錯誤");
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 純文字格式化輔助方法（保留供表格格式化使用）

        /// <summary>
        /// 格式化表格行（固定寬度）- 進貨退出單專用格式
        /// </summary>
        private static string FormatTableRow(string col1, string col2, string col3, string col4, string col5, string col6, string col7)
        {
            // 序號(4) | 品名(30) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 備註(10)
            return PlainTextFormatter.FormatTableRow(new (string, int, PlainTextAlignment)[]
            {
                (col1, 4, PlainTextAlignment.Left),
                (col2, 30, PlainTextAlignment.Left),
                (col3, 8, PlainTextAlignment.Right),
                (col4, 6, PlainTextAlignment.Left),
                (col5, 10, PlainTextAlignment.Right),
                (col6, 12, PlainTextAlignment.Right),
                (col7, 10, PlainTextAlignment.Left)
            });
        }

        #endregion
    }
}
