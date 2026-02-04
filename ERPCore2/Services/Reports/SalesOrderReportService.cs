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
    /// 銷貨單報表服務實作 - 純文字版本
    /// 設計理念：統一使用純文字格式，支援直接列印和預覽
    /// </summary>
    public class SalesOrderReportService : ISalesOrderReportService
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IWarehouseService _warehouseService;
        private readonly ISystemParameterService _systemParameterService;
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPlainTextPrintService _plainTextPrintService;
        private readonly ILogger<SalesOrderReportService>? _logger;

        public SalesOrderReportService(
            ISalesOrderService salesOrderService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            ISystemParameterService systemParameterService,
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPlainTextPrintService plainTextPrintService,
            ILogger<SalesOrderReportService>? logger = null)
        {
            _salesOrderService = salesOrderService;
            _customerService = customerService;
            _productService = productService;
            _companyService = companyService;
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _systemParameterService = systemParameterService;
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _plainTextPrintService = plainTextPrintService;
            _logger = logger;
        }

        #region 純文字報表生成

        /// <summary>
        /// 生成純文字格式的銷貨單報表
        /// 直接生成格式化的純文字，適合直接列印和預覽
        /// </summary>
        public async Task<string> GeneratePlainTextReportAsync(int salesOrderId)
        {
            // 載入資料
            var salesOrder = await _salesOrderService.GetByIdAsync(salesOrderId);
            if (salesOrder == null)
            {
                throw new ArgumentException($"找不到銷貨單 ID: {salesOrderId}");
            }

            var orderDetails = salesOrder.SalesOrderDetails?.ToList() ?? new List<SalesOrderDetail>();
            
            Customer? customer = null;
            if (salesOrder.CustomerId > 0)
            {
                customer = await _customerService.GetByIdAsync(salesOrder.CustomerId);
            }

            Employee? employee = null;
            if (salesOrder.EmployeeId.HasValue && salesOrder.EmployeeId.Value > 0)
            {
                employee = await _employeeService.GetByIdAsync(salesOrder.EmployeeId.Value);
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
            return GeneratePlainTextContent(salesOrder, orderDetails, customer, employee, company, productDict, warehouseDict, taxRate);
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

                // 根據條件查詢銷貨單
                var salesOrders = await _salesOrderService.GetByBatchCriteriaAsync(criteria);

                if (salesOrders == null || !salesOrders.Any())
                {
                    return $"無符合條件的銷貨單\n篩選條件：{criteria.GetSummary()}";
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

                for (int i = 0; i < salesOrders.Count; i++)
                {
                    var salesOrder = salesOrders[i];

                    // 載入該銷貨單的相關資料
                    var orderDetails = salesOrder.SalesOrderDetails?.ToList() ?? new List<SalesOrderDetail>();
                    
                    Customer? customer = null;
                    if (salesOrder.CustomerId > 0)
                    {
                        customer = await _customerService.GetByIdAsync(salesOrder.CustomerId);
                    }

                    Employee? employee = null;
                    if (salesOrder.EmployeeId.HasValue && salesOrder.EmployeeId.Value > 0)
                    {
                        employee = await _employeeService.GetByIdAsync(salesOrder.EmployeeId.Value);
                    }

                    Company? company = await _companyService.GetPrimaryCompanyAsync();

                    // 生成該銷貨單的純文字內容
                    sb.Append(GeneratePlainTextContent(salesOrder, orderDetails, customer, employee, company, productDict, warehouseDict, taxRate));

                    // 加入分頁符號（最後一張不需要）
                    if (i < salesOrders.Count - 1)
                    {
                        sb.Append(pageBreak);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次銷貨單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成純文字內容（固定寬度格式，適合等寬字型列印）
        /// </summary>
        private static string GeneratePlainTextContent(
            SalesOrder salesOrder,
            List<SalesOrderDetail> orderDetails,
            Customer? customer,
            Employee? employee,
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
                "銷 貨 單",
                lineWidth));

            // === 基本資訊區 ===
            sb.AppendLine($"銷貨單號：{salesOrder.Code,-20} 訂單日期：{PlainTextFormatter.FormatDate(salesOrder.OrderDate)}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
            sb.AppendLine($"客戶名稱：{customer?.CompanyName ?? ""}");
            sb.AppendLine($"聯 絡 人：{customer?.ContactPerson ?? "",-20} 統一編號：{customer?.TaxNumber ?? ""}");
            sb.AppendLine($"業 務 員：{employee?.Name ?? ""}");
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細表頭 ===
            // 序號(4) | 品名(26) | 數量(8) | 單位(6) | 單價(10) | 折扣(6) | 小計(12) | 備註(8)
            sb.AppendLine(FormatTableRow("序號", "品名", "數量", "單位", "單價", "折扣%", "小計", "備註"));
            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 明細內容 ===
            int rowNum = 1;
            foreach (var detail in orderDetails)
            {
                var product = productDict.GetValueOrDefault(detail.ProductId);
                var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", 24);
                var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", 6);

                sb.AppendLine(FormatTableRow(
                    rowNum.ToString(),
                    productName,
                    PlainTextFormatter.FormatQuantity(detail.OrderQuantity),
                    "個",
                    PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
                    PlainTextFormatter.FormatQuantity(detail.DiscountPercentage),
                    PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
                    remarks
                ));
                rowNum++;
            }

            sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

            // === 合計區 ===
            var taxMethodText = $"({taxRate:F2}%)";
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", salesOrder.TotalAmount));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", salesOrder.SalesTaxAmount, taxMethodText));
            sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", salesOrder.TotalAmountWithTax));

            // === 備註 ===
            if (!string.IsNullOrWhiteSpace(salesOrder.Remarks))
            {
                sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
                sb.AppendLine($"備註：{salesOrder.Remarks}");
            }

            sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

            // === 簽名區 ===
            sb.Append(PlainTextFormatter.BuildSignatureSection(
                new[] { "製單人員", "業務人員", "核准人員" },
                lineWidth));

            return sb.ToString();
        }

        #endregion

        #region 直接列印

        /// <summary>
        /// 直接列印銷貨單（使用 System.Drawing.Printing）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int salesOrderId, string printerName)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(salesOrderId);

                _logger?.LogInformation("開始直接列印銷貨單 {OrderId}，印表機：{PrinterName}", salesOrderId, printerName);

                // 使用共用的列印服務
                var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"銷貨單-{salesOrderId}");
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("銷貨單 {OrderId} 列印完成", salesOrderId);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印銷貨單 {OrderId} 時發生錯誤", salesOrderId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印銷貨單（使用報表列印配置）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintByReportIdAsync(int salesOrderId, string reportId)
        {
            try
            {
                // 生成純文字報表
                var textContent = await GeneratePlainTextReportAsync(salesOrderId);

                _logger?.LogInformation("開始列印銷貨單 {OrderId}，使用配置：{ReportId}", salesOrderId, reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"銷貨單-{salesOrderId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印銷貨單 {OrderId} 時發生錯誤，ReportId: {ReportId}", salesOrderId, reportId);
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

                _logger?.LogInformation("開始批次列印銷貨單，使用配置：{ReportId}", reportId);

                // 使用共用的列印服務
                return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, "銷貨單批次列印");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印銷貨單時發生錯誤");
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 純文字格式化輔助方法（保留供表格格式化使用）

        /// <summary>
        /// 格式化表格行（固定寬度）- 銷貨單專用格式
        /// </summary>
        private static string FormatTableRow(string col1, string col2, string col3, string col4, string col5, string col6, string col7, string col8)
        {
            // 序號(4) | 品名(26) | 數量(8) | 單位(6) | 單價(10) | 折扣(6) | 小計(12) | 備註(8)
            return PlainTextFormatter.FormatTableRow(new (string, int, PlainTextAlignment)[]
            {
                (col1, 4, PlainTextAlignment.Left),
                (col2, 26, PlainTextAlignment.Left),
                (col3, 8, PlainTextAlignment.Right),
                (col4, 6, PlainTextAlignment.Left),
                (col5, 10, PlainTextAlignment.Right),
                (col6, 6, PlainTextAlignment.Right),
                (col7, 12, PlainTextAlignment.Right),
                (col8, 8, PlainTextAlignment.Left)
            });
        }

        #endregion
    }
}
