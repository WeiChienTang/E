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
    /// 採購單報表服務實作 - 純文字版本
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public class PurchaseOrderReportService : IPurchaseOrderReportService
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPlainTextPrintService _plainTextPrintService;
        private readonly ILogger<PurchaseOrderReportService>? _logger;

        public PurchaseOrderReportService(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService,
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPlainTextPrintService plainTextPrintService,
            ILogger<PurchaseOrderReportService>? logger = null)
        {
            _purchaseOrderService = purchaseOrderService;
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
        /// 生成純文字格式的採購單報表
        /// 直接生成格式化的純文字，適合直接列印和預覽
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int purchaseOrderId)
        {
            // 載入資料
            var purchaseOrder = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                throw new ArgumentException($"找不到採購單 ID: {purchaseOrderId}");
            }

            var orderDetails = await _purchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);
            
            Supplier? supplier = null;
            if (purchaseOrder.SupplierId > 0)
            {
                supplier = await _supplierService.GetByIdAsync(purchaseOrder.SupplierId);
            }

            Company? company = null;
            if (purchaseOrder.CompanyId > 0)
            {
                company = await _companyService.GetByIdAsync(purchaseOrder.CompanyId);
            }

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            // 生成純文字報表
            return GeneratePlainTextContent(purchaseOrder, orderDetails, supplier, company, productDict);
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

                // 根據條件查詢採購單
                var purchaseOrders = await _purchaseOrderService.GetByBatchCriteriaAsync(criteria);

                if (purchaseOrders == null || !purchaseOrders.Any())
                {
                    return $"無符合條件的採購單\n篩選條件：{criteria.GetSummary()}";
                }

                // 載入共用資料
                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var sb = new StringBuilder();
                var pageBreak = "\f"; // Form Feed 字元，用於分頁

                for (int i = 0; i < purchaseOrders.Count; i++)
                {
                    var purchaseOrder = purchaseOrders[i];

                    // 載入該採購單的相關資料
                    var orderDetails = await _purchaseOrderService.GetOrderDetailsAsync(purchaseOrder.Id);
                    
                    Supplier? supplier = null;
                    if (purchaseOrder.SupplierId > 0)
                    {
                        supplier = await _supplierService.GetByIdAsync(purchaseOrder.SupplierId);
                    }

                    Company? company = null;
                    if (purchaseOrder.CompanyId > 0)
                    {
                        company = await _companyService.GetByIdAsync(purchaseOrder.CompanyId);
                    }

                    // 生成該採購單的純文字內容
                    sb.Append(GeneratePlainTextContent(purchaseOrder, orderDetails, supplier, company, productDict));

                    // 加入分頁符號（最後一張不需要）
                    if (i < purchaseOrders.Count - 1)
                    {
                        sb.Append(pageBreak);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次採購單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成純文字內容（固定寬度格式，適合等寬字型列印）
        /// </summary>
        private static string GeneratePlainTextContent(
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> orderDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict)
        {
            var sb = new StringBuilder();
            const int lineWidth = PlainTextFormatter.DefaultLineWidth;

            // === 標題區 ===
            sb.Append(PlainTextFormatter.BuildTitleSection(
                company?.CompanyName ?? "公司名稱",
                "採 購 單",
                lineWidth));

            // === 基本資訊區 ===
            sb.AppendLine($"採購單號：{purchaseOrder.Code,-20} 採購日期：{PlainTextFormatter.FormatDate(purchaseOrder.OrderDate)}");
            sb.AppendLine($"交貨日期：{PlainTextFormatter.FormatDate(purchaseOrder.ExpectedDeliveryDate)}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine($"廠商名稱：{supplier?.CompanyName ?? ""}");
            sb.AppendLine($"聯 絡 人：{supplier?.ContactPerson ?? "",-20} 統一編號：{supplier?.TaxNumber ?? ""}");
            sb.AppendLine($"送貨地址：{company?.Address ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細表頭 ===
            // 序號(4) | 品名(30) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 備註(10)
            sb.AppendLine(FormatTableRow("序號", "品名", "數量", "單位", "單價", "小計", "備註"));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
            int rowNum = 1;
            foreach (var detail in orderDetails)
            {
                var product = productDict.GetValueOrDefault(detail.ProductId);
                var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", 28);
                var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", 8);

                sb.AppendLine(FormatTableRow(
                    rowNum.ToString(),
                    productName,
                    PlainTextFormatter.FormatQuantity(detail.OrderQuantity),
                    "個",
                    PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
                    remarks
                ));
                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區 ===
            var taxMethodText = purchaseOrder.TaxCalculationMethod switch
            {
                TaxCalculationMethod.TaxExclusive => "外加稅",
                TaxCalculationMethod.TaxInclusive => "內含稅",
                TaxCalculationMethod.NoTax => "免稅",
                _ => ""
            };

            sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", purchaseOrder.TotalAmount));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", purchaseOrder.PurchaseTaxAmount, taxMethodText));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", purchaseOrder.PurchaseTotalAmountIncludingTax));

            // === 備註 ===
            if (!string.IsNullOrWhiteSpace(purchaseOrder.Remarks))
            {
                sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
                sb.AppendLine($"備註：{purchaseOrder.Remarks}");
            }

            sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

            // === 簽名區 ===
            sb.Append(PlainTextFormatter.BuildSignatureSection(
                new[] { "採購人員", "核准人員", "收貨確認" },
                lineWidth));

            return sb.ToString();
        }

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印採購單（使用 System.Drawing.Printing）
        /// 與 PrinterTestService.PrintUsingSystemDrawing 相同方式
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string printerName)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseOrderId);

                _logger?.LogInformation("開始直接列印採購單 {OrderId}，印表機：{PrinterName}", purchaseOrderId, printerName);

                // 使用共用的列印服務
                var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"採購單-{purchaseOrderId}");
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("採購單 {OrderId} 列印完成", purchaseOrderId);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印採購單 {OrderId} 時發生錯誤", purchaseOrderId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印採購單（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseOrderId, string reportId)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseOrderId);

                _logger?.LogInformation("開始列印採購單 {OrderId}，使用配置：{ReportId}", purchaseOrderId, reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"採購單-{purchaseOrderId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印採購單 {OrderId} 時發生錯誤，ReportId: {ReportId}", purchaseOrderId, reportId);
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

                _logger?.LogInformation("開始批次列印採購單，使用配置：{ReportId}", reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, "採購單批次列印");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印採購單時發生錯誤");
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 純文字格式化輔助方法（保留供表格格式化使用）

        /// <summary>
        /// 格式化表格行（固定寬度）- 採購單專用格式
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
