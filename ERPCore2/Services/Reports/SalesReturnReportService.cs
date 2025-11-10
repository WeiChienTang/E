using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Common;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 銷貨退回單報表服務實作 - 使用精確尺寸控制與通用分頁框架
    /// </summary>
    public class SalesReturnReportService : ISalesReturnReportService
    {
        private readonly ISalesReturnService _salesReturnService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IWarehouseService _warehouseService;
        private readonly IWarehouseLocationService _warehouseLocationService;
        private readonly ISystemParameterService _systemParameterService;

        public SalesReturnReportService(
            ISalesReturnService salesReturnService,
            ICustomerService customerService,
            IProductService productService,
            ICompanyService companyService,
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            IWarehouseLocationService warehouseLocationService,
            ISystemParameterService systemParameterService)
        {
            _salesReturnService = salesReturnService;
            _customerService = customerService;
            _productService = productService;
            _companyService = companyService;
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _warehouseLocationService = warehouseLocationService;
            _systemParameterService = systemParameterService;
        }

        public async Task<string> GenerateSalesReturnReportAsync(int salesReturnId, ReportFormat format = ReportFormat.Html)
        {
            return await GenerateSalesReturnReportAsync(salesReturnId, format, null);
        }

        public async Task<string> GenerateSalesReturnReportAsync(
            int salesReturnId, 
            ReportFormat format, 
            ReportPrintConfiguration? reportPrintConfig)
        {
            try
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

                // 取得主要公司
                Company? company = await _companyService.GetPrimaryCompanyAsync();

                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var allWarehouses = await _warehouseService.GetAllAsync();
                var warehouseDict = allWarehouses.ToDictionary(w => w.Id, w => w);

                var allLocations = await _warehouseLocationService.GetAllAsync();
                var locationDict = allLocations.ToDictionary(l => l.Id, l => l);

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
                    ReportFormat.Html => GenerateHtmlReport(salesReturn, returnDetails, customer, employee, company, 
                        productDict, warehouseDict, locationDict, taxRate),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成銷貨退回單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 銷貨退回單明細包裝類別（實作 IReportDetailItem 介面）
        /// </summary>
        private class SalesReturnDetailWrapper : IReportDetailItem
        {
            public SalesReturnDetail Detail { get; }

            public SalesReturnDetailWrapper(SalesReturnDetail detail)
            {
                Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            }

            public string GetRemarks()
            {
                return Detail.Remarks ?? string.Empty;
            }

            public decimal GetExtraHeightFactor()
            {
                // 銷貨退回單明細目前無額外高度因素
                return 0m;
            }
        }

        private string GenerateHtmlReport(
            SalesReturn salesReturn,
            List<SalesReturnDetail> returnDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            Dictionary<int, WarehouseLocation> locationDict,
            decimal taxRate)
        {
            var html = new StringBuilder();

            // HTML 文件開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>銷貨退回單 - {salesReturn.Code}</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // 準備明細清單
            var detailsList = returnDetails ?? new List<SalesReturnDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<SalesReturnDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new SalesReturnDetailWrapper(d))
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
                GeneratePage(html, salesReturn, pageDetails, customer, employee, company, 
                    productDict, warehouseDict, locationDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
                
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
            SalesReturn salesReturn,
            List<SalesReturnDetail> pageDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            Dictionary<int, WarehouseLocation> locationDict,
            decimal taxRate,
            int currentPage,
            int totalPages,
            bool isLastPage,
            int startRowNum)
        {
            html.AppendLine("    <div class='print-container'>");
            html.AppendLine("        <div class='print-single-layout'>");

            // 公司標頭（每頁都顯示）
            GenerateHeader(html, salesReturn, customer, company, currentPage, totalPages);

            // 銷貨退回資訊區塊（每頁都顯示）
            GenerateInfoSection(html, salesReturn, customer, employee, company);

            // 明細表格
            html.AppendLine("            <div class='print-table-container'>");
            GenerateDetailTable(html, pageDetails, productDict, warehouseDict, locationDict, startRowNum);
            html.AppendLine("            </div>");

            // 統計區域（只在最後一頁顯示）
            if (isLastPage)
            {
                GenerateSummarySection(html, salesReturn, taxRate);
                // 簽名區域（只在最後一頁顯示）
                GenerateSignatureSection(html);
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
        }

        private void GenerateHeader(StringBuilder html, SalesReturn salesReturn, Customer? customer, Company? company, int currentPage, int totalPages)
        {
            var headerBuilder = new ReportHeaderBuilder();
            headerBuilder
                .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
                .SetTitle(company?.CompanyName, "銷貨退回單")
                .SetPageInfo(currentPage, totalPages);

            html.Append(headerBuilder.Build());
        }

        private void GenerateInfoSection(StringBuilder html, SalesReturn salesReturn, Customer? customer, Employee? employee, Company? company)
        {
            var infoBuilder = new ReportInfoSectionBuilder();
            infoBuilder
                .AddField("退回單號", salesReturn.Code)
                .AddDateField("退回日期", salesReturn.ReturnDate)
                .AddField("客戶名稱", customer?.CompanyName)
                .AddField("聯絡人", customer?.ContactPerson)
                .AddField("統一編號", customer?.TaxNumber)
                .AddField("處理人員", employee?.Name)
                .AddFieldIf(salesReturn.ReturnReason != null, "退回原因", salesReturn.ReturnReason?.Name ?? "");

            html.Append(infoBuilder.Build());
        }

        private void GenerateDetailTable(
            StringBuilder html, 
            List<SalesReturnDetail> returnDetails, 
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            Dictionary<int, WarehouseLocation> locationDict,
            int startRowNum)
        {
            var tableBuilder = new ReportTableBuilder<SalesReturnDetail>();
            tableBuilder
                .AddIndexColumn("序號", "4%", startRowNum)
                .AddTextColumn("品名", "25%", detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "", "text-left")
                .AddQuantityColumn("退回數量", "10%", detail => detail.ReturnQuantity)
                .AddTextColumn("單位", "8%", detail => "個", "text-center")
                .AddAmountColumn("單價", "12%", detail => detail.OriginalUnitPrice)
                .AddAmountColumn("小計", "14%", detail => detail.ReturnSubtotalAmount)
                .AddTextColumn("備註", "27%", detail => detail.Remarks ?? "", "text-left");

            html.Append(tableBuilder.Build(returnDetails, startRowNum));
        }

        private void GenerateSummarySection(StringBuilder html, SalesReturn salesReturn, decimal taxRate)
        {
            var summaryBuilder = new ReportSummaryBuilder();
            summaryBuilder
                .SetRemarks(salesReturn.Remarks)
                .AddAmountItem("退回金額小計", salesReturn.TotalReturnAmount)
                .AddSummaryItem($"稅額({taxRate:F2}%)", salesReturn.ReturnTaxAmount.ToString("N2"))
                .AddAmountItem("退回含稅總計", salesReturn.TotalReturnAmountWithTax);

            html.Append(summaryBuilder.Build());
        }

        private void GenerateSignatureSection(StringBuilder html)
        {
            var signatureBuilder = new ReportSignatureBuilder();
            signatureBuilder
                .AddSignatures("處理人員", "倉管人員", "核准人員");

            html.Append(signatureBuilder.Build());
        }

        /// <summary>
        /// 批次生成銷貨退回單報表（支援多條件篩選）
        /// 設計理念：根據篩選條件查詢多筆銷貨退回單，逐一生成報表後合併為單一 HTML，每個單據自動分頁
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">輸出格式（目前僅支援 HTML）</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表 HTML（包含所有符合條件的銷貨退回單）</returns>
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

                // 根據條件查詢銷貨退回單
                var salesReturns = await _salesReturnService.GetByBatchCriteriaAsync(criteria);

                if (salesReturns == null || !salesReturns.Any())
                {
                    return GenerateEmptyResultPage(criteria);
                }

                // 根據格式生成批次報表
                return format switch
                {
                    ReportFormat.Html => await GenerateBatchHtmlReportAsync(salesReturns, reportPrintConfig, criteria),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次銷貨退回單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成批次 HTML 報表（合併多個銷貨退回單）
        /// </summary>
        private async Task<string> GenerateBatchHtmlReportAsync(
            List<SalesReturn> salesReturns,
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
            html.AppendLine($"    <title>銷貨退回單批次列印 ({salesReturns.Count} 筆)</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // 載入共用資料（避免每個報表都重複載入）
            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allWarehouses = await _warehouseService.GetAllAsync();
            var warehouseDict = allWarehouses.ToDictionary(w => w.Id, w => w);

            var allLocations = await _warehouseLocationService.GetAllAsync();
            var locationDict = allLocations.ToDictionary(l => l.Id, l => l);

            decimal taxRate = 5.0m;
            try
            {
                taxRate = await _systemParameterService.GetTaxRateAsync();
            }
            catch
            {
                // 使用預設稅率
            }

            // 逐一生成每張銷貨退回單報表
            for (int i = 0; i < salesReturns.Count; i++)
            {
                var salesReturn = salesReturns[i];
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

                // 生成單筆報表（嵌入批次報表中）
                GenerateSingleReportInBatch(html, salesReturn, returnDetails, customer, employee, company, 
                    productDict, warehouseDict, locationDict, taxRate, i + 1, salesReturns.Count);
            }

            // 列印腳本
            html.AppendLine(GetPrintScript());

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// 在批次報表中生成單一銷貨退回單（使用現有的分頁邏輯）
        /// </summary>
        private void GenerateSingleReportInBatch(
            StringBuilder html,
            SalesReturn salesReturn,
            List<SalesReturnDetail> returnDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Warehouse> warehouseDict,
            Dictionary<int, WarehouseLocation> locationDict,
            decimal taxRate,
            int currentDoc,
            int totalDocs)
        {
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm();
            var paginator = new ReportPaginator<SalesReturnDetailWrapper>(layout);

            var wrappedDetails = returnDetails
                .Select(d => new SalesReturnDetailWrapper(d))
                .ToList();

            var pages = paginator.SplitIntoPages(wrappedDetails);

            // 生成每一頁
            int startRowNum = 0;
            for (int pageNum = 0; pageNum < pages.Count; pageNum++)
            {
                var page = pages[pageNum];
                var pageDetails = page.Items.Select(w => w.Detail).ToList();

                GeneratePage(html, salesReturn, pageDetails, customer, employee, company, 
                    productDict, warehouseDict, locationDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);

                startRowNum += pageDetails.Count;
            }
        }

        /// <summary>
        /// 生成批次列印資訊頁（顯示篩選條件摘要）
        /// </summary>
        private string GenerateBatchPrintInfoPage(List<SalesReturn> salesReturns, BatchPrintCriteria criteria)
        {
            var info = new StringBuilder();
            info.AppendLine("    <div class='batch-info-page' style='page-break-after: always; padding: 40px;'>");
            info.AppendLine("        <h1 style='text-align: center; margin-bottom: 30px;'>銷貨退回單批次列印</h1>");
            info.AppendLine("        <div style='font-size: 14px; line-height: 2;'>");
            info.AppendLine($"            <p><strong>列印時間：</strong>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            info.AppendLine($"            <p><strong>列印筆數：</strong>{salesReturns.Count} 筆</p>");
            info.AppendLine($"            <p><strong>篩選條件：</strong>{criteria.GetSummary()}</p>");
            info.AppendLine("        </div>");
            info.AppendLine("    </div>");
            return info.ToString();
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
    <title>查無資料</title>
    <link href='/css/print-styles.css' rel='stylesheet' />
</head>
<body>
    <div style='text-align: center; padding: 50px;'>
        <h1>查無符合條件的銷貨退回單</h1>
        <p>篩選條件：{criteria.GetSummary()}</p>
        <p>請調整篩選條件後重試</p>
    </div>
</body>
</html>";
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
