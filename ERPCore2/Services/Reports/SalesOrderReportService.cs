using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Common;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 銷貨單報表服務實作 - 使用精確尺寸控制與通用分頁框架
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

        public SalesOrderReportService(
            ISalesOrderService salesOrderService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            ISystemParameterService systemParameterService)
        {
            _salesOrderService = salesOrderService;
            _customerService = customerService;
            _productService = productService;
            _companyService = companyService;
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _systemParameterService = systemParameterService;
        }

        public async Task<string> GenerateSalesOrderReportAsync(int salesOrderId, ReportFormat format = ReportFormat.Html)
        {
            return await GenerateSalesOrderReportAsync(salesOrderId, format, null);
        }

        public async Task<string> GenerateSalesOrderReportAsync(
            int salesOrderId, 
            ReportFormat format, 
            ReportPrintConfiguration? reportPrintConfig)
        {
            try
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

                // 取得主要公司
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

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => GenerateHtmlReport(salesOrder, orderDetails, customer, employee, company, 
                        productDict, warehouseDict, taxRate),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成銷貨單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 銷貨單明細包裝類別（實作 IReportDetailItem 介面）
        /// </summary>
        private class SalesOrderDetailWrapper : IReportDetailItem
        {
            public SalesOrderDetail Detail { get; }

            public SalesOrderDetailWrapper(SalesOrderDetail detail)
            {
                Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            }

            public string GetRemarks()
            {
                return Detail.Remarks ?? string.Empty;
            }

            public decimal GetExtraHeightFactor()
            {
                // 銷貨單明細目前無額外高度因素
                return 0m;
            }
        }

        private string GenerateHtmlReport(
            SalesOrder salesOrder,
            List<SalesOrderDetail> orderDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            decimal taxRate)
        {
            var html = new StringBuilder();

            // HTML 文件開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>銷貨單 - {salesOrder.Code}</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // 準備明細清單
            var detailsList = orderDetails ?? new List<SalesOrderDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<SalesOrderDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new SalesOrderDetailWrapper(d))
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
                GeneratePage(html, salesOrder, pageDetails, customer, employee, company, 
                    productDict, warehouseDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
                
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
            SalesOrder salesOrder,
            List<SalesOrderDetail> pageDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            decimal taxRate,
            int currentPage,
            int totalPages,
            bool isLastPage,
            int startRowNum)
        {
            html.AppendLine("    <div class='print-container'>");
            html.AppendLine("        <div class='print-single-layout'>");

            // 公司標頭（每頁都顯示）
            GenerateHeader(html, salesOrder, customer, company, currentPage, totalPages);

            // 銷貨資訊區塊（每頁都顯示）
            GenerateInfoSection(html, salesOrder, customer, employee, company);

            // 明細表格
            html.AppendLine("            <div class='print-table-container'>");
            GenerateDetailTable(html, pageDetails, productDict, warehouseDict, startRowNum);
            html.AppendLine("            </div>");

            // 統計區域（只在最後一頁顯示）
            if (isLastPage)
            {
                GenerateSummarySection(html, salesOrder, taxRate);
                // 簽名區域（只在最後一頁顯示）
                GenerateSignatureSection(html);
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
        }

        private void GenerateHeader(StringBuilder html, SalesOrder salesOrder, Customer? customer, Company? company, int currentPage, int totalPages)
        {
            var headerBuilder = new ReportHeaderBuilder();
            headerBuilder
                .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
                .SetTitle(company?.CompanyName, "銷貨單")
                .SetPageInfo(currentPage, totalPages);

            html.Append(headerBuilder.Build());
        }

        private void GenerateInfoSection(StringBuilder html, SalesOrder salesOrder, Customer? customer, Employee? employee, Company? company)
        {
            var infoBuilder = new ReportInfoSectionBuilder();
            infoBuilder
                .AddField("銷貨單號", salesOrder.Code)
                .AddDateField("訂單日期", salesOrder.OrderDate)
                .AddField("客戶名稱", customer?.CompanyName)
                .AddField("聯絡人", customer?.ContactPerson)
                .AddField("統一編號", customer?.TaxNumber)
                .AddField("業務員", employee?.Name);

            html.Append(infoBuilder.Build());
        }

        private void GenerateDetailTable(
            StringBuilder html, 
            List<SalesOrderDetail> orderDetails, 
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            int startRowNum)
        {
            var tableBuilder = new ReportTableBuilder<SalesOrderDetail>();
            tableBuilder
                .AddIndexColumn("序號", "4%", startRowNum)
                .AddTextColumn("品名", "20%", detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "", "text-left")
                .AddQuantityColumn("數量", "7%", detail => detail.OrderQuantity)
                .AddTextColumn("單位", "5%", detail => "個", "text-center")
                .AddAmountColumn("單價", "10%", detail => detail.UnitPrice)
                .AddAmountColumn("折扣%", "8%", detail => detail.DiscountPercentage)
                .AddAmountColumn("小計", "12%", detail => detail.SubtotalAmount)
                .AddTextColumn("倉庫", "12%", detail => warehouseDict.GetValueOrDefault(detail.WarehouseId ?? 0)?.Name ?? "", "text-center")
                .AddTextColumn("備註", "22%", detail => detail.Remarks ?? "", "text-left");

            html.Append(tableBuilder.Build(orderDetails, startRowNum));
        }

        private void GenerateSummarySection(StringBuilder html, SalesOrder salesOrder, decimal taxRate)
        {
            var summaryBuilder = new ReportSummaryBuilder();
            summaryBuilder
                .SetRemarks(salesOrder.Remarks)
                .AddAmountItem("金額小計", salesOrder.TotalAmount)
                .AddSummaryItem($"稅額({taxRate:F2}%)", salesOrder.SalesTaxAmount.ToString("N2"))
                .AddAmountItem("含稅總計", salesOrder.TotalAmountWithTax);

            html.Append(summaryBuilder.Build());
        }

        private void GenerateSignatureSection(StringBuilder html)
        {
            var signatureBuilder = new ReportSignatureBuilder();
            signatureBuilder
                .AddSignatures("製單人員", "業務人員", "核准人員");

            html.Append(signatureBuilder.Build());
        }

        /// <summary>
        /// 批次生成銷貨單報表（支援多條件篩選）
        /// </summary>
        public async Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            ReportPrintConfiguration? reportPrintConfig = null)
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
                    // 返回空結果提示頁面
                    return GenerateEmptyResultPage(criteria);
                }

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => await GenerateBatchHtmlReportAsync(salesOrders, reportPrintConfig, criteria),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次銷貨單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成空結果提示頁面
        /// </summary>
        private string GenerateEmptyResultPage(BatchPrintCriteria criteria)
        {
            return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>批次列印 - 無符合條件的資料</title>
    <link href='/css/print-styles.css' rel='stylesheet' />
</head>
<body>
    <div style='text-align: center; padding: 50px;'>
        <h1>無符合條件的銷貨單</h1>
        <p>篩選條件：{criteria.GetSummary()}</p>
        <p>請調整篩選條件後重新查詢。</p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// 生成批次 HTML 報表（合併多個銷貨單）
        /// </summary>
        private async Task<string> GenerateBatchHtmlReportAsync(
            List<SalesOrder> salesOrders,
            ReportPrintConfiguration? reportPrintConfig,
            BatchPrintCriteria criteria)
        {
            var html = new StringBuilder();

            // HTML 文件開始（只需一次）
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>銷貨單批次列印 ({salesOrders.Count} 筆)</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // 批次列印資訊頁（可選，顯示篩選條件摘要）
            html.AppendLine(GenerateBatchPrintInfoPage(salesOrders, criteria));

            // 載入共用資料（避免每個報表都重複載入）
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

            // 逐一生成每張銷貨單報表
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

                // 取得主要公司
                Company? company = await _companyService.GetPrimaryCompanyAsync();

                // 生成該銷貨單的 HTML（重複使用現有的單筆報表邏輯）
                GenerateSingleReportInBatch(html, salesOrder, orderDetails, customer, employee, company, 
                    productDict, warehouseDict, taxRate, i + 1, salesOrders.Count);
            }

            // 列印腳本（自動列印）
            html.AppendLine(GetPrintScript());

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// 在批次報表中生成單一銷貨單（使用現有的分頁邏輯）
        /// </summary>
        private void GenerateSingleReportInBatch(
            StringBuilder html,
            SalesOrder salesOrder,
            List<SalesOrderDetail> orderDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            decimal taxRate,
            int currentDoc,
            int totalDocs)
        {
            // 準備明細清單
            var detailsList = orderDetails ?? new List<SalesOrderDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<SalesOrderDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new SalesOrderDetailWrapper(d))
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
                GeneratePage(html, salesOrder, pageDetails, customer, employee, company, 
                    productDict, warehouseDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
                
                startRowNum += pageDetails.Count;
            }
        }

        /// <summary>
        /// 生成批次列印資訊頁（顯示篩選條件摘要）
        /// </summary>
        private string GenerateBatchPrintInfoPage(List<SalesOrder> salesOrders, BatchPrintCriteria criteria)
        {
            var html = new StringBuilder();
            
            html.AppendLine("    <div class='batch-print-info-page' style='display: none;'>"); // 預設隱藏，避免影響列印
            html.AppendLine("        <div class='info-header'>");
            html.AppendLine("            <h2>批次列印資訊</h2>");
            html.AppendLine($"            <p>列印時間：{DateTime.Now:yyyy/MM/dd HH:mm:ss}</p>");
            html.AppendLine($"            <p>共 {salesOrders.Count} 筆銷貨單</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='info-criteria'>");
            html.AppendLine($"            <p>篩選條件：{criteria.GetSummary()}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='info-list'>");
            html.AppendLine("            <h3>單據清單</h3>");
            html.AppendLine("            <ol>");
            
            foreach (var order in salesOrders)
            {
                html.AppendLine($"                <li>{order.Code} - {order.Customer?.CompanyName ?? "未指定客戶"} - {order.OrderDate:yyyy/MM/dd}</li>");
            }
            
            html.AppendLine("            </ol>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            return html.ToString();
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
