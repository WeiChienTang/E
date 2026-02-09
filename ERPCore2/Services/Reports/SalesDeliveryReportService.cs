using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
    /// 出貨單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public class SalesDeliveryReportService : ISalesDeliveryReportService
    {
        private readonly ISalesDeliveryService _salesDeliveryService;
        private readonly ISalesDeliveryDetailService _salesDeliveryDetailService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IUnitService _unitService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SalesDeliveryReportService>? _logger;

        public SalesDeliveryReportService(
            ISalesDeliveryService salesDeliveryService,
            ISalesDeliveryDetailService salesDeliveryDetailService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IUnitService unitService,
            IFormattedPrintService formattedPrintService,
            ILogger<SalesDeliveryReportService>? logger = null)
        {
            _salesDeliveryService = salesDeliveryService;
            _salesDeliveryDetailService = salesDeliveryDetailService;
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
        /// 生成出貨單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int salesDeliveryId)
        {
            // 載入資料
            var salesDelivery = await _salesDeliveryService.GetByIdAsync(salesDeliveryId);
            if (salesDelivery == null)
            {
                throw new ArgumentException($"找不到出貨單 ID: {salesDeliveryId}");
            }

            var deliveryDetails = await _salesDeliveryDetailService.GetByDeliveryIdAsync(salesDeliveryId);

            Customer? customer = null;
            if (salesDelivery.CustomerId > 0)
            {
                customer = await _customerService.GetByIdAsync(salesDelivery.CustomerId);
            }

            Employee? employee = null;
            if (salesDelivery.EmployeeId.HasValue && salesDelivery.EmployeeId.Value > 0)
            {
                employee = await _employeeService.GetByIdAsync(salesDelivery.EmployeeId.Value);
            }

            Company? company = await _companyService.GetPrimaryCompanyAsync();

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allUnits = await _unitService.GetAllAsync();
            var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

            // 建構格式化文件
            return BuildFormattedDocument(salesDelivery, deliveryDetails, customer, employee, company, productDict, unitDict);
        }

        /// <summary>
        /// 直接列印出貨單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int salesDeliveryId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(salesDeliveryId);
                
                _logger?.LogInformation("開始列印出貨單 {SalesDeliveryId}，使用配置：{ReportId}", salesDeliveryId, reportId);

                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印出貨單 {SalesDeliveryId} 時發生錯誤", salesDeliveryId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesDeliveryId)
        {
            var document = await GenerateReportAsync(salesDeliveryId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesDeliveryId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(salesDeliveryId);
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
                // 根據條件查詢出貨單
                var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
                var endDate = criteria.EndDate ?? DateTime.Today;
                
                var allDeliveries = await _salesDeliveryService.GetAllAsync();
                var filteredDeliveries = allDeliveries
                    .Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate)
                    .ToList();

                // 如果有指定客戶ID，進行篩選
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    filteredDeliveries = filteredDeliveries
                        .Where(d => criteria.RelatedEntityIds.Contains(d.CustomerId))
                        .ToList();
                }

                if (filteredDeliveries == null || !filteredDeliveries.Any())
                {
                    return ServiceResult.Failure($"無符合條件的出貨單\n篩選條件：{criteria.GetSummary()}");
                }

                _logger?.LogInformation("開始批次列印 {Count} 張出貨單，使用配置：{ReportId}", filteredDeliveries.Count, reportId);

                // 逐一列印
                foreach (var delivery in filteredDeliveries)
                {
                    var result = await DirectPrintAsync(delivery.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中出貨單 {SalesDeliveryId} 失敗：{ErrorMessage}", delivery.Id, result.ErrorMessage);
                    }
                }

                _logger?.LogInformation("已完成 {Count} 張出貨單的列印", filteredDeliveries.Count);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印出貨單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（用於批次預覽）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            // 手動篩選邏輯
            var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;

            var allDeliveries = await _salesDeliveryService.GetAllAsync();
            var filteredDeliveries = allDeliveries
                .Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate)
                .ToList();

            if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
            {
                filteredDeliveries = filteredDeliveries
                    .Where(d => criteria.RelatedEntityIds.Contains(d.CustomerId))
                    .ToList();
            }

            return await BatchReportHelper.RenderBatchToImagesAsync(
                filteredDeliveries,
                (id, _) => GenerateReportAsync(id),
                _formattedPrintService,
                "出貨單",
                criteria.PaperSetting,
                criteria.GetSummary(),
                _logger);
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildFormattedDocument(
            SalesDelivery salesDelivery,
            List<SalesDeliveryDetail> deliveryDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"出貨單-{salesDelivery.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 中間公司名稱+出貨單（置中），右側單號/日期/頁次
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("出 貨 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{salesDelivery.Code ?? ""}",
                        $"日期：{salesDelivery.DeliveryDate:yyyy/MM/dd}",
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
                    ("送貨地址", salesDelivery.DeliveryAddress ?? customer?.ShippingAddress ?? ""));

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
                foreach (var detail in deliveryDetails)
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
                        NumberFormatHelper.FormatSmart(detail.DeliveryQuantity),
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
                var taxMethodText = salesDelivery.TaxCalculationMethod switch
                {
                    TaxCalculationMethod.TaxExclusive => "外加稅",
                    TaxCalculationMethod.TaxInclusive => "內含稅",
                    TaxCalculationMethod.NoTax => "免稅",
                    _ => ""
                };

                // 合計區（說明在左、金額在右）
                var leftLines = new List<string>();
                if (!string.IsNullOrWhiteSpace(salesDelivery.Remarks))
                {
                    leftLines.Add("【出貨備註】");
                    leftLines.Add(salesDelivery.Remarks);
                }

                var amountLines = new List<string>
                {
                    $"小　計：{NumberFormatHelper.FormatSmart(salesDelivery.TotalAmount)}",
                    $"稅　額：{NumberFormatHelper.FormatSmart(salesDelivery.TaxAmount)} ({taxMethodText})",
                    $"總　計：{NumberFormatHelper.FormatSmart(salesDelivery.TotalAmountWithTax)}"
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
                      .AddSignatureSection("製單人員", "業務人員", "客戶簽收");
            });

            return doc;
        }

        #endregion
    }
}
