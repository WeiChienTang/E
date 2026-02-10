using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 銷貨退回單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public class SalesReturnReportService : ISalesReturnReportService
    {
        private readonly ISalesReturnService _salesReturnService;
        private readonly ISalesReturnDetailService _salesReturnDetailService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IUnitService _unitService;
        private readonly IWarehouseService _warehouseService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SalesReturnReportService>? _logger;

        public SalesReturnReportService(
            ISalesReturnService salesReturnService,
            ISalesReturnDetailService salesReturnDetailService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IUnitService unitService,
            IWarehouseService warehouseService,
            IFormattedPrintService formattedPrintService,
            ILogger<SalesReturnReportService>? logger = null)
        {
            _salesReturnService = salesReturnService;
            _salesReturnDetailService = salesReturnDetailService;
            _customerService = customerService;
            _productService = productService;
            _companyService = companyService;
            _employeeService = employeeService;
            _unitService = unitService;
            _warehouseService = warehouseService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成銷貨退回單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int salesReturnId)
        {
            // 載入資料
            var salesReturn = await _salesReturnService.GetByIdAsync(salesReturnId);
            if (salesReturn == null)
            {
                throw new ArgumentException($"找不到銷貨退回單 ID: {salesReturnId}");
            }

            var returnDetails = salesReturn.SalesReturnDetails?.ToList() ?? new List<SalesReturnDetail>();

            Customer? customer = null;
            if (salesReturn.CustomerId > 0)
            {
                customer = await _customerService.GetByIdAsync(salesReturn.CustomerId);
            }

            Employee? employee = null;
            if (salesReturn.EmployeeId.HasValue && salesReturn.EmployeeId.Value > 0)
            {
                employee = await _employeeService.GetByIdAsync(salesReturn.EmployeeId.Value);
            }

            Company? company = await _companyService.GetPrimaryCompanyAsync();

            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allUnits = await _unitService.GetAllAsync();
            var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

            // 建構格式化文件
            return BuildFormattedDocument(salesReturn, returnDetails, customer, employee, company, productDict, unitDict);
        }

        /// <summary>
        /// 直接列印銷貨退回單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int salesReturnId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(salesReturnId);

                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印銷貨退回單 {SalesReturnId} 時發生錯誤", salesReturnId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesReturnId)
        {
            var document = await GenerateReportAsync(salesReturnId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int salesReturnId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(salesReturnId);
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
                // 根據條件查詢銷貨退回單
                var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
                var endDate = criteria.EndDate ?? DateTime.Today;
                
                var allReturns = await _salesReturnService.GetAllAsync();
                var filteredReturns = allReturns
                    .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate)
                    .ToList();

                // 如果有指定客戶ID，進行篩選
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    filteredReturns = filteredReturns
                        .Where(r => criteria.RelatedEntityIds.Contains(r.CustomerId))
                        .ToList();
                }

                if (filteredReturns == null || !filteredReturns.Any())
                {
                    return ServiceResult.Failure($"無符合條件的銷貨退回單\n篩選條件：{criteria.GetSummary()}");
                }

                // 逐一列印
                foreach (var salesReturn in filteredReturns)
                {
                    var result = await DirectPrintAsync(salesReturn.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中銷貨退回單 {SalesReturnId} 失敗：{ErrorMessage}", salesReturn.Id, result.ErrorMessage);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印銷貨退回單時發生錯誤");
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

            var allReturns = await _salesReturnService.GetAllAsync();
            var filteredReturns = allReturns
                .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate)
                .ToList();

            if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
            {
                filteredReturns = filteredReturns
                    .Where(r => criteria.RelatedEntityIds.Contains(r.CustomerId))
                    .ToList();
            }

            return await BatchReportHelper.RenderBatchToImagesAsync(
                filteredReturns,
                (id, _) => GenerateReportAsync(id),
                _formattedPrintService,
                "銷貨退回單",
                criteria.PaperSetting,
                criteria.GetSummary(),
                _logger);
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildFormattedDocument(
            SalesReturn salesReturn,
            List<SalesReturnDetail> returnDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"銷貨退回單-{salesReturn.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 中間公司名稱+銷貨退回單（置中），右側單號/日期/頁次
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("銷 貨 退 回 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{salesReturn.Code ?? ""}",
                        $"日期：{salesReturn.ReturnDate:yyyy/MM/dd}",
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
                    ("地址", customer?.ShippingAddress ?? customer?.ContactAddress ?? ""));

                // === 處理人員與退回原因 ===
                var returnReasonName = salesReturn.ReturnReason?.Name ?? "";
                header.AddKeyValueRow(
                    ("處理人員", employee?.Name ?? ""),
                    ("退回原因", returnReasonName));

                header.AddSpacing(3);
            });

            // === 明細表格（主要內容）===
            doc.AddTable(table =>
            {
                // 定義欄位
                table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                     .AddColumn("品名/規格", 2.2f, Models.Reports.TextAlignment.Left)
                     .AddColumn("單位", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("退回數量", 0.8f, Models.Reports.TextAlignment.Right)
                     .AddColumn("單價", 0.8f, Models.Reports.TextAlignment.Right)
                     .AddColumn("小計", 0.9f, Models.Reports.TextAlignment.Right)
                     .AddColumn("備註", 1.0f, Models.Reports.TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                // 新增資料列
                int rowNum = 1;
                foreach (var detail in returnDetails)
                {
                    var product = productDict.GetValueOrDefault(detail.ProductId);
                    var unit = product != null ? unitDict.GetValueOrDefault(product.UnitId) : null;
                    
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
                        NumberFormatHelper.FormatSmart(detail.ReturnQuantity),
                        NumberFormatHelper.FormatSmart(detail.OriginalUnitPrice),
                        NumberFormatHelper.FormatSmart(detail.ReturnSubtotalAmount),
                        detail.Remarks ?? ""
                    );
                    rowNum++;
                }
            });

            // === 頁尾區（只在最後一頁顯示）===
            doc.BeginFooter(footer =>
            {
                // 稅別說明
                var taxMethodText = salesReturn.TaxCalculationMethod switch
                {
                    TaxCalculationMethod.TaxExclusive => "外加稅",
                    TaxCalculationMethod.TaxInclusive => "內含稅",
                    TaxCalculationMethod.NoTax => "免稅",
                    _ => ""
                };

                // 合計區（說明在左、金額在右）
                var leftLines = new List<string>();
                if (!string.IsNullOrWhiteSpace(salesReturn.Remarks))
                {
                    leftLines.Add("【退回備註】");
                    leftLines.Add(salesReturn.Remarks);
                }

                var amountLines = new List<string>
                {
                    $"小　計：{NumberFormatHelper.FormatSmart(salesReturn.TotalReturnAmount)}",
                    $"稅　額：{NumberFormatHelper.FormatSmart(salesReturn.ReturnTaxAmount)} ({taxMethodText})",
                    $"總　計：{NumberFormatHelper.FormatSmart(salesReturn.TotalReturnAmountWithTax)}"
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
                      .AddSignatureSection("處理人員", "倉管人員", "核准人員");
            });

            return doc;
        }

        #endregion
    }
}
