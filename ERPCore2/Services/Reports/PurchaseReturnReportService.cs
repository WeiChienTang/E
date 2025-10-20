using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Common;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 進貨退出單報表服務實作 - 使用精確尺寸控制與通用分頁框架
    /// </summary>
    public class PurchaseReturnReportService : IPurchaseReturnReportService
    {
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;

        public PurchaseReturnReportService(
            IPurchaseReturnService purchaseReturnService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService)
        {
            _purchaseReturnService = purchaseReturnService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
            _systemParameterService = systemParameterService;
        }

        public async Task<string> GeneratePurchaseReturnReportAsync(int purchaseReturnId, ReportFormat format = ReportFormat.Html)
        {
            return await GeneratePurchaseReturnReportAsync(purchaseReturnId, format, null);
        }

        public async Task<string> GeneratePurchaseReturnReportAsync(
            int purchaseReturnId, 
            ReportFormat format, 
            Data.Entities.ReportPrintConfiguration? reportPrintConfig)
        {
            try
            {
                // 載入資料
                var purchaseReturn = await _purchaseReturnService.GetByIdAsync(purchaseReturnId);
                if (purchaseReturn == null)
                {
                    throw new ArgumentException($"找不到進貨退出單 ID: {purchaseReturnId}");
                }

                var returnDetails = purchaseReturn.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
                
                Supplier? supplier = null;
                if (purchaseReturn.SupplierId > 0)
                {
                    supplier = await _supplierService.GetByIdAsync(purchaseReturn.SupplierId);
                }

                // 取得主要公司（進貨退出單沒有 CompanyId，使用預設主要公司）
                Company? company = await _companyService.GetPrimaryCompanyAsync();

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
                    ReportFormat.Html => GenerateHtmlReport(purchaseReturn, returnDetails, supplier, company, 
                        productDict, taxRate),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成進貨退出單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 進貨退出單明細包裝類別（實作 IReportDetailItem 介面）
        /// </summary>
        private class PurchaseReturnDetailWrapper : IReportDetailItem
        {
            public PurchaseReturnDetail Detail { get; }

            public PurchaseReturnDetailWrapper(PurchaseReturnDetail detail)
            {
                Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            }

            public string GetRemarks()
            {
                return Detail.Remarks ?? string.Empty;
            }

            public decimal GetExtraHeightFactor()
            {
                // 進貨退出單明細目前無額外高度因素
                // 未來若有特殊欄位（如圖片、多行規格）可在此加入
                return 0m;
            }
        }

        private string GenerateHtmlReport(
            PurchaseReturn purchaseReturn,
            List<PurchaseReturnDetail> returnDetails,
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
            html.AppendLine($"    <title>進貨退出單 - {purchaseReturn.PurchaseReturnNumber}</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // 準備明細清單
            var detailsList = returnDetails ?? new List<PurchaseReturnDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<PurchaseReturnDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new PurchaseReturnDetailWrapper(d))
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
                GeneratePage(html, purchaseReturn, pageDetails, supplier, company, 
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
            PurchaseReturn purchaseReturn,
            List<PurchaseReturnDetail> pageDetails,
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
            GenerateHeader(html, purchaseReturn, supplier, company, currentPage, totalPages);

            // 退出資訊區塊（每頁都顯示）
            GenerateInfoSection(html, purchaseReturn, supplier, company);

            // 明細表格
            html.AppendLine("            <div class='print-table-container'>");
            GenerateDetailTable(html, pageDetails, productDict, startRowNum);
            html.AppendLine("            </div>");

            // 統計區域（只在最後一頁顯示）
            if (isLastPage)
            {
                GenerateSummarySection(html, purchaseReturn, taxRate);
                // 簽名區域（只在最後一頁顯示）
                GenerateSignatureSection(html);
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
        }

        private void GenerateHeader(StringBuilder html, PurchaseReturn purchaseReturn, Supplier? supplier, Company? company, int currentPage, int totalPages)
        {
            var headerBuilder = new ReportHeaderBuilder();
            headerBuilder
                .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
                .SetTitle(company?.CompanyName, "進貨退出單")
                .SetPageInfo(currentPage, totalPages);

            html.Append(headerBuilder.Build());
        }

        private void GenerateInfoSection(StringBuilder html, PurchaseReturn purchaseReturn, Supplier? supplier, Company? company)
        {
            var infoBuilder = new ReportInfoSectionBuilder();
            infoBuilder
                .AddField("退回單號", purchaseReturn.PurchaseReturnNumber)
                .AddDateField("退回日期", purchaseReturn.ReturnDate)
                .AddField("廠商名稱", supplier?.CompanyName)
                .AddField("聯絡人", supplier?.ContactPerson)
                .AddField("統一編號", supplier?.TaxNumber)
                .AddField("退回地址", company?.Address, columnSpan: 3);

            html.Append(infoBuilder.Build());
        }

        private void GenerateDetailTable(
            StringBuilder html, 
            List<PurchaseReturnDetail> returnDetails, 
            Dictionary<int, Product> productDict,
            int startRowNum)
        {
            var tableBuilder = new ReportTableBuilder<PurchaseReturnDetail>();
            tableBuilder
                .AddIndexColumn("序號", "5%", startRowNum)
                .AddTextColumn("品名", "25%", detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "", "text-left")
                .AddQuantityColumn("數量", "8%", detail => -detail.ReturnQuantity) // 負數顯示
                .AddTextColumn("單位", "5%", detail => "個", "text-center")
                .AddAmountColumn("單價", "12%", detail => detail.OriginalUnitPrice)
                .AddAmountColumn("小計", "15%", detail => -detail.ReturnSubtotalAmount) // 負數顯示
                .AddTextColumn("備註", "30%", detail => detail.Remarks ?? "", "text-left");

            html.Append(tableBuilder.Build(returnDetails, startRowNum));
        }

        private void GenerateSummarySection(StringBuilder html, PurchaseReturn purchaseReturn, decimal taxRate)
        {
            var summaryBuilder = new ReportSummaryBuilder();
            summaryBuilder
                .SetRemarks(purchaseReturn.Remarks)
                .AddAmountItem("退回金額小計", -purchaseReturn.TotalReturnAmount) // 負數顯示
                .AddSummaryItem($"退回稅額({taxRate:F2}%)", (-purchaseReturn.ReturnTaxAmount).ToString("N2")) // 負數顯示
                .AddAmountItem("退回含稅總計", -purchaseReturn.TotalReturnAmountWithTax); // 負數顯示

            html.Append(summaryBuilder.Build());
        }

        private void GenerateSignatureSection(StringBuilder html)
        {
            var signatureBuilder = new ReportSignatureBuilder();
            signatureBuilder
                .AddSignatures("退回人員", "驗收人員", "核准人員");

            html.Append(signatureBuilder.Build());
        }

        /// <summary>
        /// 批次生成進貨退出單報表（支援多條件篩選）
        /// 設計理念：根據篩選條件查詢多筆進貨退出單，逐一生成報表後合併為單一 HTML，每個單據自動分頁
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <param name="format">輸出格式（目前僅支援 HTML）</param>
        /// <param name="reportPrintConfig">報表列印配置（可選）</param>
        /// <returns>合併後的報表 HTML（包含所有符合條件的進貨退出單）</returns>
        public async Task<string> GenerateBatchReportAsync(
            BatchPrintCriteria criteria,
            ReportFormat format = ReportFormat.Html,
            Data.Entities.ReportPrintConfiguration? reportPrintConfig = null)
        {
            try
            {
                // 驗證篩選條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"批次列印條件驗證失敗：{validation.GetAllErrors()}");
                }

                // 根據條件查詢進貨退出單
                var purchaseReturns = await _purchaseReturnService.GetByBatchCriteriaAsync(criteria);

                if (purchaseReturns == null || !purchaseReturns.Any())
                {
                    // 返回空結果提示頁面
                    return GenerateEmptyResultPage(criteria);
                }

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => await GenerateBatchHtmlReportAsync(purchaseReturns, reportPrintConfig, criteria),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次進貨退出單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成批次 HTML 報表（合併多個進貨退出單）
        /// </summary>
        private async Task<string> GenerateBatchHtmlReportAsync(
            List<PurchaseReturn> purchaseReturns,
            Data.Entities.ReportPrintConfiguration? reportPrintConfig,
            BatchPrintCriteria criteria)
        {
            var html = new StringBuilder();

            // HTML 文件開始（只需一次）
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>進貨退出單批次列印 ({purchaseReturns.Count} 筆)</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // 批次列印資訊頁（可選，顯示篩選條件摘要）
            html.AppendLine(GenerateBatchPrintInfoPage(purchaseReturns, criteria));

            // 載入共用資料（避免每個報表都重複載入）
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

            // 逐一生成每張進貨退出單報表
            for (int i = 0; i < purchaseReturns.Count; i++)
            {
                var purchaseReturn = purchaseReturns[i];

                // 載入該進貨退出單的相關資料
                var returnDetails = purchaseReturn.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
                
                Supplier? supplier = null;
                if (purchaseReturn.SupplierId > 0)
                {
                    supplier = await _supplierService.GetByIdAsync(purchaseReturn.SupplierId);
                }

                // 取得主要公司（進貨退出單沒有 CompanyId，使用預設主要公司）
                Company? company = await _companyService.GetPrimaryCompanyAsync();

                // 生成該進貨退出單的 HTML（重複使用現有的單筆報表邏輯）
                GenerateSingleReportInBatch(html, purchaseReturn, returnDetails, supplier, company, 
                    productDict, taxRate, i + 1, purchaseReturns.Count);
            }

            // 列印腳本（自動列印）
            html.AppendLine(GetPrintScript());

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// 在批次報表中生成單一進貨退出單（使用現有的分頁邏輯）
        /// </summary>
        private void GenerateSingleReportInBatch(
            StringBuilder html,
            PurchaseReturn purchaseReturn,
            List<PurchaseReturnDetail> returnDetails,
            Supplier? supplier,
            Company? company,
            Dictionary<int, Product> productDict,
            decimal taxRate,
            int currentDoc,
            int totalDocs)
        {
            // 準備明細清單
            var detailsList = returnDetails ?? new List<PurchaseReturnDetail>();
            
            // 使用通用分頁計算器
            var layout = ReportPageLayout.ContinuousForm(); // 中一刀格式
            var paginator = new ReportPaginator<PurchaseReturnDetailWrapper>(layout);
            
            // 包裝明細項目
            var wrappedDetails = detailsList
                .Select(d => new PurchaseReturnDetailWrapper(d))
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
                GeneratePage(html, purchaseReturn, pageDetails, supplier, company, 
                    productDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
                
                startRowNum += pageDetails.Count;
            }

            // CSS 的 .print-container { page-break-after: always; } 已經處理分頁
            // 不需要額外加入 div，否則會產生空白頁
        }

        /// <summary>
        /// 生成批次列印資訊頁（顯示篩選條件摘要）
        /// </summary>
        private string GenerateBatchPrintInfoPage(List<PurchaseReturn> purchaseReturns, BatchPrintCriteria criteria)
        {
            var html = new StringBuilder();
            
            html.AppendLine("    <div class='batch-print-info-page' style='display: none;'>"); // 預設隱藏，避免影響列印
            html.AppendLine("        <div class='info-header'>");
            html.AppendLine("            <h2>批次列印資訊</h2>");
            html.AppendLine($"            <p>列印時間：{DateTime.Now:yyyy/MM/dd HH:mm:ss}</p>");
            html.AppendLine($"            <p>共 {purchaseReturns.Count} 筆進貨退出單</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='info-criteria'>");
            html.AppendLine($"            <p>篩選條件：{criteria.GetSummary()}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='info-list'>");
            html.AppendLine("            <h3>單據清單</h3>");
            html.AppendLine("            <ol>");
            
            foreach (var purchaseReturn in purchaseReturns)
            {
                html.AppendLine($"                <li>{purchaseReturn.PurchaseReturnNumber} - {purchaseReturn.Supplier?.CompanyName ?? "未指定廠商"} - {purchaseReturn.ReturnDate:yyyy/MM/dd}</li>");
            }
            
            html.AppendLine("            </ol>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            return html.ToString();
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
        <h1>無符合條件的進貨退出單</h1>
        <p>篩選條件：{criteria.GetSummary()}</p>
        <p>請調整篩選條件後重新查詢。</p>
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
