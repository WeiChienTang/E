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
    /// 訂單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public class SalesOrderReportService : ISalesOrderReportService
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IUnitService _unitService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SalesOrderReportService>? _logger;

        public SalesOrderReportService(
            ISalesOrderService salesOrderService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IUnitService unitService,
            IFormattedPrintService formattedPrintService,
            ILogger<SalesOrderReportService>? logger = null)
        {
            _salesOrderService = salesOrderService;
            _customerService = customerService;
            _productService = productService;
            _companyService = companyService;
            _employeeService = employeeService;
            _unitService = unitService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成訂單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int salesOrderId)
        {
            // 載入資料
            var salesOrder = await _salesOrderService.GetByIdAsync(salesOrderId);
            if (salesOrder == null)
            {
                throw new ArgumentException($"找不到訂單 ID: {salesOrderId}");
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

            var allUnits = await _unitService.GetAllAsync();
            var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

            // 建構格式化文件
            return BuildFormattedDocument(salesOrder, orderDetails, customer, employee, company, productDict, unitDict);
        }

        /// <summary>
        /// 直接列印訂單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int salesOrderId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(salesOrderId);

                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印訂單 {SalesOrderId} 時發生錯誤", salesOrderId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesOrderId)
        {
            var document = await GenerateReportAsync(salesOrderId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesOrderId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(salesOrderId);
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
                // 根據條件查詢訂單
                var salesOrders = await _salesOrderService.GetByBatchCriteriaAsync(criteria);

                if (salesOrders == null || !salesOrders.Any())
                {
                    return ServiceResult.Failure($"無符合條件的訂單\n篩選條件：{criteria.GetSummary()}");
                }

                // 逐一列印
                foreach (var salesOrder in salesOrders)
                {
                    var result = await DirectPrintAsync(salesOrder.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中訂單 {SalesOrderId} 失敗：{ErrorMessage}", salesOrder.Id, result.ErrorMessage);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印訂單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（用於批次預覽）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var entities = await _salesOrderService.GetByBatchCriteriaAsync(criteria);

            return await BatchReportHelper.RenderBatchToImagesAsync(
                entities,
                (id, _) => GenerateReportAsync(id),
                _formattedPrintService,
                "銷貨單",
                criteria.PaperSetting,
                criteria.GetSummary(),
                _logger);
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildFormattedDocument(
            SalesOrder salesOrder,
            List<SalesOrderDetail> orderDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"訂單-{salesOrder.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 中間公司名稱+訂單（置中），右側單號/日期/頁次
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("銷 貨 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{salesOrder.Code ?? ""}",
                        $"日期：{salesOrder.OrderDate:yyyy/MM/dd}",
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

                // === 業務員 ===
                header.AddKeyValueRow(
                    ("業務員", employee?.Name ?? ""));

                header.AddSpacing(3);
            });

            // === 明細表格（主要內容）===
            doc.AddTable(table =>
            {
                // 定義欄位
                table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                     .AddColumn("品名/規格", 2.2f, Models.Reports.TextAlignment.Left)
                     .AddColumn("單位", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("數量", 0.7f, Models.Reports.TextAlignment.Right)
                     .AddColumn("單價", 0.8f, Models.Reports.TextAlignment.Right)
                     .AddColumn("折扣%", 0.5f, Models.Reports.TextAlignment.Right)
                     .AddColumn("總價", 0.9f, Models.Reports.TextAlignment.Right)
                     .AddColumn("備註", 1.0f, Models.Reports.TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                // 新增資料列
                int rowNum = 1;
                foreach (var detail in orderDetails)
                {
                    var product = productDict.GetValueOrDefault(detail.ProductId);
                    var unit = detail.UnitId.HasValue ? unitDict.GetValueOrDefault(detail.UnitId.Value) : null;
                    
                    // 組合商品名稱與規格說明
                    var productName = product?.Name ?? "";
                    var specification = product?.Specification ?? "";
                    var displayName = string.IsNullOrEmpty(specification) 
                        ? productName 
                        : $"{productName}\n規格：{specification}";

                    table.AddRow(
                        rowNum.ToString(),
                        displayName,
                        unit?.Name ?? "",
                        NumberFormatHelper.FormatSmart(detail.OrderQuantity),
                        NumberFormatHelper.FormatSmart(detail.UnitPrice),
                        NumberFormatHelper.FormatSmart(detail.DiscountPercentage),
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
                var taxMethodText = salesOrder.TaxCalculationMethod switch
                {
                    TaxCalculationMethod.TaxExclusive => "外加稅",
                    TaxCalculationMethod.TaxInclusive => "內含稅",
                    TaxCalculationMethod.NoTax => "免稅",
                    _ => ""
                };

                // 合計區（說明在左、金額在右）
                var leftLines = new List<string>();
                if (!string.IsNullOrWhiteSpace(salesOrder.Remarks))
                {
                    leftLines.Add("【訂單備註】");
                    leftLines.Add(salesOrder.Remarks);
                }

                var amountLines = new List<string>
                {
                    $"小　計：{NumberFormatHelper.FormatSmart(salesOrder.TotalAmount)}",
                    $"稅　額：{NumberFormatHelper.FormatSmart(salesOrder.SalesTaxAmount)} ({taxMethodText})",
                    $"總　計：{NumberFormatHelper.FormatSmart(salesOrder.TotalAmountWithTax)}"
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
                      .AddSignatureSection("製單人員", "業務人員", "核准人員");
            });

            return doc;
        }

        #endregion
    }
}
