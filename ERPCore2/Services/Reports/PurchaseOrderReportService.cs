using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Common;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 採購單報表服務實作 - 新版（使用精確尺寸控制與通用分頁框架）
    /// </summary>
    public class PurchaseOrderReportService : IPurchaseOrderReportService
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;

        public PurchaseOrderReportService(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _systemParameterService = systemParameterService;
        }

        public async Task<string> GeneratePurchaseOrderReportAsync(int purchaseOrderId, ReportFormat format = ReportFormat.Html)
        {
            return await GeneratePurchaseOrderReportAsync(purchaseOrderId, format, null);
        }

        public async Task<string> GeneratePurchaseOrderReportAsync(
            int purchaseOrderId, 
            ReportFormat format, 
            ReportPrintConfiguration? reportPrintConfig)
        {
            try
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

                decimal taxRate = 5.0m;
                try
                {
                    taxRate = await _systemParameterService.GetTaxRateAsync();
                }
                catch
                {
                    // 使用預設稅率
                }

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => GenerateHtmlReport(purchaseOrder, orderDetails, supplier, company, productDict, taxRate),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成採購單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        public ReportConfiguration GetPurchaseOrderReportConfiguration(Company? company = null)
        {
            // 保留相容性，但新版不再使用此方法
            throw new NotImplementedException("新版報表服務不使用此方法");
        }

        /// <summary>
        /// 採購單明細包裝類別（實作 IReportDetailItem 介面）
        /// </summary>
        private class PurchaseOrderDetailWrapper : IReportDetailItem
        {
            public PurchaseOrderDetail Detail { get; }

            public PurchaseOrderDetailWrapper(PurchaseOrderDetail detail)
            {
                Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            }

            public string GetRemarks()
            {
                return Detail.Remarks ?? string.Empty;
            }

            public decimal GetExtraHeightFactor()
            {
                // 採購單明細目前無額外高度因素
                // 未來若有特殊欄位（如圖片、多行規格）可在此加入
                return 0m;
            }
        }

        private string GenerateHtmlReport(
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> orderDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            decimal taxRate)
        {
            var html = new StringBuilder();

            // HTML 文件開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>採購單 - {purchaseOrder.PurchaseOrderNumber}</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // 準備明細清單
            var detailsList = orderDetails ?? new List<PurchaseOrderDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<PurchaseOrderDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new PurchaseOrderDetailWrapper(d))
                .ToList();
            
            // 智能分頁
            var pages = paginator.SplitIntoPages(wrappedDetails);

            // 生成每一頁
            int startRowNum = 0;
            for (int pageNum = 0; pageNum < pages.Count; pageNum++)
            {
                var page = pages[pageNum];
                var pageDetails = page.Items.Select(w => w.Detail).ToList();

                // 生成單頁內容（每個 print-container 會自動分頁）
                GeneratePage(html, purchaseOrder, pageDetails, supplier, company, 
                    productDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
                
                startRowNum += pageDetails.Count;
            }

            // 列印腳本
            html.AppendLine(GetPrintScript());
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void GeneratePage(
            StringBuilder html,
            PurchaseOrder purchaseOrder,
            List<PurchaseOrderDetail> pageDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            decimal taxRate,
            int currentPage,
            int totalPages,
            bool isLastPage,
            int startRowNum)
        {
            html.AppendLine("    <div class='print-container'>");
            html.AppendLine("        <div class='print-single-layout'>");

            // 公司標頭（每頁都顯示）
            GenerateHeader(html, purchaseOrder, supplier, company, currentPage, totalPages);

            // 採購資訊區塊（每頁都顯示）
            GenerateInfoSection(html, purchaseOrder, supplier, company);

            // 明細表格
            html.AppendLine("            <div class='print-table-container'>");
            GenerateDetailTable(html, pageDetails, productDict, startRowNum);
            html.AppendLine("            </div>");

            // 統計區域（只在最後一頁顯示）
            if (isLastPage)
            {
                GenerateSummarySection(html, purchaseOrder, taxRate);
                // 簽名區域（只在最後一頁顯示）
                GenerateSignatureSection(html);
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
        }

        private void GenerateHeader(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company, int currentPage, int totalPages)
        {
            var headerBuilder = new ReportHeaderBuilder();
            headerBuilder
                .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
                .SetTitle(company?.CompanyName, "採購單")
                .SetPageInfo(currentPage, totalPages);

            html.Append(headerBuilder.Build());
        }

        private void GenerateInfoSection(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company)
        {
            var infoBuilder = new ReportInfoSectionBuilder();
            infoBuilder
                .AddField("採購單號", purchaseOrder.PurchaseOrderNumber)
                .AddDateField("採購日期", purchaseOrder.OrderDate)
                .AddDateField("交貨日期", purchaseOrder.ExpectedDeliveryDate)
                .AddField("廠商名稱", supplier?.CompanyName)
                .AddField("聯絡人", supplier?.ContactPerson)
                .AddField("統一編號", supplier?.TaxNumber)
                .AddField("送貨地址", company?.Address, columnSpan: 3);

            html.Append(infoBuilder.Build());
        }

        private void GenerateDetailTable(StringBuilder html, List<PurchaseOrderDetail> orderDetails, Dictionary<int, Product> productDict, int startRowNum)
        {
            var tableBuilder = new ReportTableBuilder<PurchaseOrderDetail>();
            tableBuilder
                .AddIndexColumn("序號", "5%", startRowNum)
                .AddTextColumn("品名", "25%", detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "", "text-left")
                .AddQuantityColumn("數量", "8%", detail => detail.OrderQuantity)
                .AddTextColumn("單位", "5%", detail => "個", "text-center")
                .AddAmountColumn("單價", "12%", detail => detail.UnitPrice)
                .AddAmountColumn("小計", "15%", detail => detail.SubtotalAmount)
                .AddTextColumn("備註", "30%", detail => detail.Remarks ?? "", "text-left");

            html.Append(tableBuilder.Build(orderDetails, startRowNum));
        }

        private void GenerateSummarySection(StringBuilder html, PurchaseOrder purchaseOrder, decimal taxRate)
        {
            var summaryBuilder = new ReportSummaryBuilder();
            summaryBuilder
                .SetRemarks(purchaseOrder.Remarks)
                .AddAmountItem("金額小計", purchaseOrder.TotalAmount)
                .AddSummaryItem($"稅額({taxRate:F2}%)", purchaseOrder.PurchaseTaxAmount.ToString("N2"))
                .AddAmountItem("含稅總計", purchaseOrder.PurchaseTotalAmountIncludingTax);

            html.Append(summaryBuilder.Build());
        }

        private void GenerateSignatureSection(StringBuilder html)
        {
            var signatureBuilder = new ReportSignatureBuilder();
            signatureBuilder
                .AddSignatures("採購人員", "核准人員", "收貨確認");

            html.Append(signatureBuilder.Build());
        }

        private string GetPrintScript()
        {
            return @"
    <script>
        window.addEventListener('load', function() {
            // 檢查是否需要自動列印
            const urlParams = new URLSearchParams(window.location.search);
            if (urlParams.get('autoprint') === 'true') {
                setTimeout(function() {
                    window.print();
                }, 500);
            }
        });
        
        // Ctrl+P 優化列印
        document.addEventListener('keydown', function(e) {
            if (e.ctrlKey && e.key === 'p') {
                e.preventDefault();
                window.print();
            }
        });
    </script>";
        }
    }
}
