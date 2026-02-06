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
    /// 採購單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// </summary>
    public class PurchaseOrderReportService : IPurchaseOrderReportService
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<PurchaseOrderReportService>? _logger;

        public PurchaseOrderReportService(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<PurchaseOrderReportService>? logger = null)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成採購單報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int purchaseOrderId)
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

            // 建構格式化文件
            return BuildFormattedDocument(purchaseOrder, orderDetails, supplier, company, productDict);
        }

        /// <summary>
        /// 直接列印採購單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(purchaseOrderId);
                
                _logger?.LogInformation("開始列印採購單 {OrderId}，使用配置：{ReportId}", purchaseOrderId, reportId);

                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印採購單 {OrderId} 時發生錯誤", purchaseOrderId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId)
        {
            var document = await GenerateReportAsync(purchaseOrderId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（用於預覽）
        /// 根據指定紙張設定計算頁面尺寸
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(purchaseOrderId);
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
                // 根據條件查詢採購單
                var purchaseOrders = await _purchaseOrderService.GetByBatchCriteriaAsync(criteria);

                if (purchaseOrders == null || !purchaseOrders.Any())
                {
                    return ServiceResult.Failure($"無符合條件的採購單\n篩選條件：{criteria.GetSummary()}");
                }

                _logger?.LogInformation("開始批次列印 {Count} 張採購單，使用配置：{ReportId}", purchaseOrders.Count, reportId);

                // 逐一列印
                foreach (var purchaseOrder in purchaseOrders)
                {
                    var result = await DirectPrintAsync(purchaseOrder.Id, reportId, 1);
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning("批次列印中採購單 {OrderId} 失敗：{ErrorMessage}", purchaseOrder.Id, result.ErrorMessage);
                    }
                }

                _logger?.LogInformation("已完成 {Count} 張採購單的列印", purchaseOrders.Count);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印採購單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 建構報表文件
        private FormattedDocument BuildFormattedDocument(
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> orderDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"採購單-{purchaseOrder.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f); // 縮小邊距，尤其是上下邊距

            // === 頁首區（每頁都會重複顯示）===
            doc.BeginHeader(header =>
            {
                // 左側留空（給公司圖），中間公司名稱+採購單（置中），右側單號/日期/頁次（緊湊排列）
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("採 購 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{purchaseOrder.Code ?? ""}",
                        $"日期：{purchaseOrder.OrderDate:yyyy/MM/dd}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(3);

                // === 廠商資訊區（第一行）===
                header.AddKeyValueRow(
                    ("廠商名稱", supplier?.CompanyName ?? ""),
                    ("統一編號", supplier?.TaxNumber ?? ""),
                    ("聯絡人", supplier?.ContactPerson ?? ""),
                    ("連絡電話", supplier?.ContactPhone ?? ""));

                // === 廠商資訊區（第二行）===
                header.AddKeyValueRow(
                    ("送貨地址", company?.Address ?? ""));

                header.AddSpacing(3);
            });

            // === 明細表格（主要內容）===
            doc.AddTable(table =>
            {
                // 定義欄位（含明細備註）
                table.AddColumn("序號", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("品名", 1.8f, Models.Reports.TextAlignment.Left)
                     .AddColumn("數量", 0.7f, Models.Reports.TextAlignment.Right)
                     .AddColumn("單位", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("單價", 0.8f, Models.Reports.TextAlignment.Right)
                     .AddColumn("小計", 0.9f, Models.Reports.TextAlignment.Right)
                     .AddColumn("備註", 2.7f, Models.Reports.TextAlignment.Left)
                     .ShowBorder(false)              // 不顯示表格邊框
                     .ShowHeaderBackground(false)    // 不顯示表頭背景
                     .ShowHeaderSeparator(false)     // 不顯示表頭 | 分隔符
                     .SetRowHeight(20);

                // 新增資料列
                int rowNum = 1;
                foreach (var detail in orderDetails)
                {
                    var product = productDict.GetValueOrDefault(detail.ProductId);
                    table.AddRow(
                        rowNum.ToString(),
                        product?.Name ?? "",
                        NumberFormatHelper.FormatSmart(detail.OrderQuantity),
                        "個",
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
                // 合計區（備註在左、金額在右）
                var remarkLines = new List<string>();
                var remarkText = !string.IsNullOrWhiteSpace(purchaseOrder.Remarks) ? purchaseOrder.Remarks : "";
                remarkLines.Add($"備 註：{remarkText}");

                var amountLines = new List<string>
                {
                    $"小　計：{NumberFormatHelper.FormatSmart(purchaseOrder.TotalAmount)}",
                    $"稅　額：{NumberFormatHelper.FormatSmart(purchaseOrder.PurchaseTaxAmount)}",
                    $"總　計：{NumberFormatHelper.FormatSmart(purchaseOrder.PurchaseTotalAmountIncludingTax)}"
                };

                footer.AddSpacing(5)
                      .AddTwoColumnSection(
                          leftContent: remarkLines,
                          leftTitle: null,
                          leftHasBorder: false,
                          rightContent: amountLines,
                          leftWidthRatio: 0.8f);

                // 簽名區
                footer.AddSpacing(20)
                      .AddSignatureSection("採購人員", "核准人員", "收貨確認");
            });

            return doc;
        }

        #endregion
    }
}
