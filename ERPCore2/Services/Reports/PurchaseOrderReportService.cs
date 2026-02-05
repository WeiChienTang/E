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

        // 採購單表格的基準欄位寬度（基於 80 字元總寬度）
        // 序號(4) | 品名(30) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 備註(10) = 80
        private static readonly int[] BaseColumnWidths = { 4, 30, 8, 6, 10, 12, 10 };

        /// <summary>
        /// 生成純文字格式的採購單報表（使用預設版面配置）
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int purchaseOrderId)
        {
            return await GeneratePlainTextReportAsync(purchaseOrderId, PaperLayout.Default);
        }

        /// <summary>
        /// 生成純文字格式的採購單報表（根據紙張版面配置）
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int purchaseOrderId, PaperLayout layout)
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

            // 計算調整後的欄位寬度
            var columnWidths = PaperLayoutCalculator.CalculateColumnWidths(layout, BaseColumnWidths);

            // 生成純文字報表（使用動態版面配置）
            return GeneratePlainTextContent(purchaseOrder, orderDetails, supplier, company, productDict, layout, columnWidths);
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
        /// 使用預設 80 字元寬度
        /// </summary>
        private static string GeneratePlainTextContent(
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> orderDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict)
        {
            // 使用預設版面配置和欄位寬度
            return GeneratePlainTextContent(purchaseOrder, orderDetails, supplier, company, productDict, 
                PaperLayout.Default, BaseColumnWidths);
        }

        /// <summary>
        /// 生成純文字內容（根據版面配置動態調整寬度）
        /// </summary>
        private static string GeneratePlainTextContent(
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> orderDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            PaperLayout layout,
            int[] columnWidths)
        {
            var sb = new StringBuilder();
            var lineWidth = layout.LineWidth;

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

            // === 明細表頭（使用動態欄位寬度）===
            sb.AppendLine(FormatTableRowDynamic(
                new[] { "序號", "品名", "數量", "單位", "單價", "小計", "備註" },
                columnWidths));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
            int rowNum = 1;
            // 品名欄位的最大寬度（用於截斷）
            var productNameMaxWidth = Math.Max(4, columnWidths[1] - 2);
            var remarksMaxWidth = Math.Max(2, columnWidths[6] - 2);

            foreach (var detail in orderDetails)
            {
                var product = productDict.GetValueOrDefault(detail.ProductId);
                var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", productNameMaxWidth);
                var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", remarksMaxWidth);

                sb.AppendLine(FormatTableRowDynamic(
                    new[] {
                        rowNum.ToString(),
                        productName,
                        PlainTextFormatter.FormatQuantity(detail.OrderQuantity),
                        "個",
                        PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
                        PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
                        remarks
                    },
                    columnWidths));
                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區（根據行寬動態調整標籤位置）===
            var taxMethodText = purchaseOrder.TaxCalculationMethod switch
            {
                TaxCalculationMethod.TaxExclusive => "外加稅",
                TaxCalculationMethod.TaxInclusive => "內含稅",
                TaxCalculationMethod.NoTax => "免稅",
                _ => ""
            };

            // 計算合計區的標籤前空白寬度（根據行寬按比例調整）
            var totalLabelWidth = Math.Max(20, lineWidth - 30);
            var totalAmountWidth = Math.Max(10, lineWidth - totalLabelWidth - 10);

            sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", purchaseOrder.TotalAmount, "", totalLabelWidth, totalAmountWidth));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", purchaseOrder.PurchaseTaxAmount, taxMethodText, totalLabelWidth, totalAmountWidth));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", purchaseOrder.PurchaseTotalAmountIncludingTax, "", totalLabelWidth, totalAmountWidth));

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
        /// 使用預設欄位寬度，適用於 80 字元寬度的報表
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

        /// <summary>
        /// 格式化表格行（動態寬度）- 根據紙張版面配置調整欄位寬度
        /// </summary>
        /// <param name="values">欄位值陣列（序號、品名、數量、單位、單價、小計、備註）</param>
        /// <param name="columnWidths">欄位寬度陣列</param>
        /// <returns>格式化後的表格行</returns>
        private static string FormatTableRowDynamic(string[] values, int[] columnWidths)
        {
            // 對齊方式：序號(左)、品名(左)、數量(右)、單位(左)、單價(右)、小計(右)、備註(左)
            var alignments = new PlainTextAlignment[]
            {
                PlainTextAlignment.Left,   // 序號
                PlainTextAlignment.Left,   // 品名
                PlainTextAlignment.Right,  // 數量
                PlainTextAlignment.Left,   // 單位
                PlainTextAlignment.Right,  // 單價
                PlainTextAlignment.Right,  // 小計
                PlainTextAlignment.Left    // 備註
            };

            var columns = new List<(string Text, int Width, PlainTextAlignment Alignment)>();
            for (int i = 0; i < Math.Min(values.Length, columnWidths.Length); i++)
            {
                columns.Add((values[i], columnWidths[i], alignments[i]));
            }

            return PlainTextFormatter.FormatTableRow(columns);
        }

        #endregion
    }
}
