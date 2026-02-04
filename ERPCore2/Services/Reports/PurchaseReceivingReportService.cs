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
    /// 進貨單（入庫單）報表服務實作 - 純文字版本
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public class PurchaseReceivingReportService : IPurchaseReceivingReportService
    {
        private readonly IPurchaseReceivingService _purchaseReceivingService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IWarehouseService _warehouseService;
        private readonly IWarehouseLocationService _warehouseLocationService;
        private readonly ISystemParameterService _systemParameterService;
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPlainTextPrintService _plainTextPrintService;
        private readonly ILogger<PurchaseReceivingReportService>? _logger;

        public PurchaseReceivingReportService(
            IPurchaseReceivingService purchaseReceivingService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            IWarehouseService warehouseService,
            IWarehouseLocationService warehouseLocationService,
            ISystemParameterService systemParameterService,
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPlainTextPrintService plainTextPrintService,
            ILogger<PurchaseReceivingReportService>? logger = null)
        {
            _purchaseReceivingService = purchaseReceivingService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _warehouseService = warehouseService;
            _warehouseLocationService = warehouseLocationService;
            _systemParameterService = systemParameterService;
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _plainTextPrintService = plainTextPrintService;
            _logger = logger;
        }

        #region 純文字報表生成

        /// <summary>
        /// 生成純文字格式的進貨單報表
        /// 直接生成格式化的純文字，適合直接列印和預覽
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int purchaseReceivingId)
        {
            // 載入資料
            var purchaseReceiving = await _purchaseReceivingService.GetByIdAsync(purchaseReceivingId);
            if (purchaseReceiving == null)
            {
                throw new ArgumentException($"找不到進貨單 ID: {purchaseReceivingId}");
            }

            var receivingDetails = purchaseReceiving.PurchaseReceivingDetails?.ToList() ?? new List<PurchaseReceivingDetail>();
            
            Supplier? supplier = null;
            if (purchaseReceiving.SupplierId > 0)
            {
                supplier = await _supplierService.GetByIdAsync(purchaseReceiving.SupplierId);
            }

            Company? company = await _companyService.GetPrimaryCompanyAsync();

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allWarehouses = await _warehouseService.GetAllAsync();
            var warehouseDict = allWarehouses.ToDictionary(w => w.Id, w => w);

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
            return GeneratePlainTextContent(purchaseReceiving, receivingDetails, supplier, company, productDict, warehouseDict, taxRate);
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

                // 根據條件查詢進貨單
                var purchaseReceivings = await _purchaseReceivingService.GetByBatchCriteriaAsync(criteria);

                if (purchaseReceivings == null || !purchaseReceivings.Any())
                {
                    return $"無符合條件的進貨單\n篩選條件：{criteria.GetSummary()}";
                }

                // 載入共用資料
                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var allWarehouses = await _warehouseService.GetAllAsync();
                var warehouseDict = allWarehouses.ToDictionary(w => w.Id, w => w);

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

                for (int i = 0; i < purchaseReceivings.Count; i++)
                {
                    var purchaseReceiving = purchaseReceivings[i];

                    // 載入該進貨單的相關資料
                    var receivingDetails = purchaseReceiving.PurchaseReceivingDetails?.ToList() ?? new List<PurchaseReceivingDetail>();
                    
                    Supplier? supplier = null;
                    if (purchaseReceiving.SupplierId > 0)
                    {
                        supplier = await _supplierService.GetByIdAsync(purchaseReceiving.SupplierId);
                    }

                    Company? company = await _companyService.GetPrimaryCompanyAsync();

                    // 生成該進貨單的純文字內容
                    sb.Append(GeneratePlainTextContent(purchaseReceiving, receivingDetails, supplier, company, productDict, warehouseDict, taxRate));

                    // 加入分頁符號（最後一張不需要）
                    if (i < purchaseReceivings.Count - 1)
                    {
                        sb.Append(pageBreak);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次進貨單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成純文字內容（固定寬度格式，適合等寬字型列印）
        /// </summary>
        private static string GeneratePlainTextContent(
            PurchaseReceiving purchaseReceiving,
            List<PurchaseReceivingDetail> receivingDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            decimal taxRate)
        {
            var sb = new StringBuilder();
            const int lineWidth = PlainTextFormatter.DefaultLineWidth;

            // === 標題區 ===
            sb.Append(PlainTextFormatter.BuildTitleSection(
                company?.CompanyName ?? "公司名稱",
                "進 貨 單",
                lineWidth));

            // === 基本資訊區 ===
            sb.AppendLine($"進貨單號：{purchaseReceiving.Code,-20} 進貨日期：{PlainTextFormatter.FormatDate(purchaseReceiving.ReceiptDate)}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine($"廠商名稱：{supplier?.CompanyName ?? ""}");
            sb.AppendLine($"聯 絡 人：{supplier?.ContactPerson ?? "",-20} 統一編號：{supplier?.TaxNumber ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細表頭 ===
            // 序號(4) | 品名(26) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 倉庫(14)
            sb.AppendLine(FormatTableRow("序號", "品名", "數量", "單位", "單價", "小計", "倉庫"));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
            int rowNum = 1;
            foreach (var detail in receivingDetails)
            {
                var product = productDict.GetValueOrDefault(detail.ProductId);
                var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", 24);
                var warehouse = warehouseDict.GetValueOrDefault(detail.WarehouseId);
                var warehouseName = PlainTextFormatter.TruncateText(warehouse?.Name ?? "", 12);

                sb.AppendLine(FormatTableRow(
                    rowNum.ToString(),
                    productName,
                    PlainTextFormatter.FormatQuantity(detail.ReceivedQuantity),
                    "個",
                    PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
                    warehouseName
                ));
                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區 ===
            sb.AppendLine($"{"",50}小　計：{purchaseReceiving.TotalAmount,12:N0}");
            sb.AppendLine($"{"",-50}稅　額：{purchaseReceiving.PurchaseReceivingTaxAmount,12:N0} ({taxRate:F2}%)");
            sb.AppendLine($"{"",-50}總　計：{purchaseReceiving.PurchaseReceivingTotalAmountIncludingTax,12:N0}");

            // === 備註 ===
            if (!string.IsNullOrWhiteSpace(purchaseReceiving.Remarks))
            {
                sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
                sb.AppendLine($"備註：{purchaseReceiving.Remarks}");
            }

            sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

            // === 簽名區 ===
            sb.Append(PlainTextFormatter.BuildSignatureSection(
                new[] { "驗收人員", "倉管人員", "核准人員" },
                lineWidth));

            return sb.ToString();
        }

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印進貨單（使用 System.Drawing.Printing）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int purchaseReceivingId, string printerName)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseReceivingId);

                _logger?.LogInformation("開始直接列印進貨單 {OrderId}，印表機：{PrinterName}", purchaseReceivingId, printerName);

                // 使用共用的列印服務
                var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"進貨單-{purchaseReceivingId}");
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("進貨單 {OrderId} 列印完成", purchaseReceivingId);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印進貨單 {OrderId} 時發生錯誤", purchaseReceivingId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印進貨單（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseReceivingId, string reportId)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(purchaseReceivingId);

                _logger?.LogInformation("開始列印進貨單 {OrderId}，使用配置：{ReportId}", purchaseReceivingId, reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"進貨單-{purchaseReceivingId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印進貨單 {OrderId} 時發生錯誤，ReportId: {ReportId}", purchaseReceivingId, reportId);
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

                _logger?.LogInformation("開始批次列印進貨單，使用配置：{ReportId}", reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, "進貨單批次列印");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印進貨單時發生錯誤");
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 純文字格式化輔助方法（保留供表格格式化使用）

        /// <summary>
        /// 格式化表格行（固定寬度）- 進貨單專用格式
        /// </summary>
        private static string FormatTableRow(string col1, string col2, string col3, string col4, string col5, string col6, string col7)
        {
            // 序號(4) | 品名(26) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 倉庫(14)
            return PlainTextFormatter.FormatTableRow(new (string, int, PlainTextAlignment)[]
            {
                (col1, 4, PlainTextAlignment.Left),
                (col2, 26, PlainTextAlignment.Left),
                (col3, 8, PlainTextAlignment.Right),
                (col4, 6, PlainTextAlignment.Left),
                (col5, 10, PlainTextAlignment.Right),
                (col6, 12, PlainTextAlignment.Right),
                (col7, 14, PlainTextAlignment.Left)
            });
        }

        #endregion
    }
}
