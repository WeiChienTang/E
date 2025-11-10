using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Common;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 報價單報表服務實作 - A4 格式報價單
    /// 格式包含：標題區、產品明細區、金額區、說明區、簽章區
    /// </summary>
    public class QuotationReportService : IQuotationReportService
    {
        private readonly IQuotationService _quotationService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;
        private readonly ICompanyService _companyService;
        private readonly ISystemParameterService _systemParameterService;

        public QuotationReportService(
            IQuotationService quotationService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            IProductService productService,
            IUnitService unitService,
            ICompanyService companyService,
            ISystemParameterService systemParameterService)
        {
            _quotationService = quotationService;
            _customerService = customerService;
            _employeeService = employeeService;
            _productService = productService;
            _unitService = unitService;
            _companyService = companyService;
            _systemParameterService = systemParameterService;
        }

        public async Task<string> GenerateQuotationReportAsync(int quotationId, ReportFormat format = ReportFormat.Html)
        {
            return await GenerateQuotationReportAsync(quotationId, format, null);
        }

        public async Task<string> GenerateQuotationReportAsync(
            int quotationId,
            ReportFormat format,
            ReportPrintConfiguration? reportPrintConfig)
        {
            try
            {
                // 載入資料
                var quotation = await _quotationService.GetWithDetailsAsync(quotationId);
                if (quotation == null)
                {
                    throw new ArgumentException($"找不到報價單 ID: {quotationId}");
                }

                var quotationDetails = quotation.QuotationDetails?.ToList() ?? new List<QuotationDetail>();

                Customer? customer = null;
                if (quotation.CustomerId > 0)
                {
                    customer = await _customerService.GetByIdAsync(quotation.CustomerId);
                }

                Employee? employee = null;
                if (quotation.EmployeeId.HasValue && quotation.EmployeeId.Value > 0)
                {
                    employee = await _employeeService.GetByIdAsync(quotation.EmployeeId.Value);
                }

                // 取得主要公司
                Company? company = await _companyService.GetPrimaryCompanyAsync();

                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var allUnits = await _unitService.GetAllAsync();
                var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => GenerateHtmlReport(quotation, quotationDetails, customer, employee, company,
                        productDict, unitDict),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成報價單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        private string GenerateHtmlReport(
            Quotation quotation,
            List<QuotationDetail> quotationDetails,
            Customer? customer,
            Employee? employee,
            Company? company,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            var html = new StringBuilder();

            // HTML 文件開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>報價單 - {quotation.Code}</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("    <style>");
            html.AppendLine(GetQuotationCustomStyles());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // A4 報價單容器
            html.AppendLine("    <div class='print-container quotation-container'>");
            html.AppendLine("        <div class='print-a4-layout'>");

            // 1. 標題區
            GenerateTitleSection(html, company, quotation, customer, employee);

            // 2. 產品明細區
            GenerateProductDetailSection(html, quotationDetails, productDict, unitDict);

            // 3. 金額區 + 說明區（橫向排列）
            GenerateAmountAndDescriptionSection(html, quotation);

            // 4. 簽章區
            GenerateSignatureSection(html);

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            // 列印腳本
            html.AppendLine(GetPrintScript());

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// 1. 標題區 - 包含公司資訊和客戶資訊
        /// </summary>
        private void GenerateTitleSection(
            StringBuilder html,
            Company? company,
            Quotation quotation,
            Customer? customer,
            Employee? employee)
        {
            html.AppendLine("            <!-- 標題區 -->");
            html.AppendLine("            <div class='quotation-title-section'>");
            
            // 第一行：Logo（靠左） + 公司名稱和報價單標題（絕對置中）
            html.AppendLine("                <div class='quotation-header-row-1'>");
            
            // 從 company-logos 資料夾中取得唯一的圖片
            var logoPath = "/Resources/CompanyLOGO.png"; // 預設
            var logoDirectory = Path.Combine("wwwroot", "uploads", "company-logos");
            if (Directory.Exists(logoDirectory))
            {
                var logoFiles = Directory.GetFiles(logoDirectory);
                if (logoFiles.Length > 0)
                {
                    var logoFileName = Path.GetFileName(logoFiles[0]);
                    logoPath = $"/uploads/company-logos/{logoFileName}";
                }
            }
            
            html.AppendLine($"                    <img src='{logoPath}' alt='公司Logo' class='company-logo' />");
            html.AppendLine("                    <div class='quotation-title-wrapper'>");
            html.AppendLine("                        <div class='quotation-company-name'>");
            html.AppendLine($"                            {company?.CompanyName ?? "公司名稱"}");
            html.AppendLine("                        </div>");
            html.AppendLine("                        <div class='quotation-doc-title'>");
            html.AppendLine("                            報價單");
            html.AppendLine("                        </div>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='quotation-date-info'>");
            html.AppendLine($"                        <div>報價日期：{quotation.QuotationDate:yyyy/MM/dd}</div>");
            html.AppendLine($"                        <div>報價單號：{quotation.Code}</div>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            
            // 聯絡資訊（一行顯示完成）
            html.AppendLine("                <div class='quotation-header-row-2'>");
            html.AppendLine($"                    電話：{company?.Phone ?? ""}　傳真：{company?.Fax ?? ""}　E-mail：{company?.Email ?? ""}　地址：{company?.Address ?? ""}");
            html.AppendLine("                </div>");
            
            // 報價單資訊（無框線設計）
            html.AppendLine("                <div class='quotation-doc-info'>");
            html.AppendLine("                    <div class='quotation-info-row'>");
            html.AppendLine($"                        <span class='info-label'>客　　戶：</span><span class='info-value'>{customer?.CompanyName ?? ""}</span>");
            html.AppendLine($"                        <span class='info-label'>統一編號：</span><span class='info-value'>{customer?.TaxNumber ?? ""}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='quotation-info-row'>");
            html.AppendLine($"                        <span class='info-label'>聯 絡 人：</span><span class='info-value'>{customer?.ContactPerson ?? ""}</span>");
            html.AppendLine($"                        <span class='info-label'>信　　箱：</span><span class='info-value'>{customer?.Email ?? ""}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='quotation-info-row'>");
            html.AppendLine($"                        <span class='info-label'>連絡電話：</span><span class='info-value'>{customer?.ContactPhone ?? ""} {customer?.MobilePhone ?? ""}</span>");
            html.AppendLine($"                        <span class='info-label'>傳　　真：</span><span class='info-value'>{customer?.Fax ?? ""}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='quotation-info-row'>");
            html.AppendLine($"                        <span class='info-label'>聯絡地址：</span><span class='info-value info-value-full'>{customer?.ContactAddress ?? ""}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='quotation-info-row'>");
            html.AppendLine($"                        <span class='info-label'>運貨地點：</span><span class='info-value'>{customer?.ShippingAddress ?? ""}</span>");
            html.AppendLine($"                        <span class='info-label'>工程名稱：</span><span class='info-value'>{quotation?.ProjectName ?? ""}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("            </div>");
        }

        /// <summary>
        /// 2. 產品明細區 - 帶黃色標題列的產品明細表格
        /// </summary>
        private void GenerateProductDetailSection(
            StringBuilder html,
            List<QuotationDetail> quotationDetails,
            Dictionary<int, Product> productDict,
            Dictionary<int, Unit> unitDict)
        {
            html.AppendLine("            <!-- 產品明細區 -->");
            html.AppendLine("            <div class='quotation-product-section'>");
            html.AppendLine("                <table class='quotation-product-table'>");
            
            // 明細表頭（黃色背景，參考 Excel）
            html.AppendLine("                    <thead>");
            html.AppendLine("                        <tr class='yellow-header'>");
            html.AppendLine("                            <th width='50px'>項次</th>");
            html.AppendLine("                            <th>項目名稱/規格</th>");
            html.AppendLine("                            <th width='50px'>單位</th>");
            html.AppendLine("                            <th width='70px'>數量</th>");
            html.AppendLine("                            <th width='90px'>單價(元)</th>");
            html.AppendLine("                            <th width='90px'>總價(元)</th>");
            html.AppendLine("                            <th width='150px'>備註</th>");
            html.AppendLine("                        </tr>");
            html.AppendLine("                    </thead>");
            
            // 明細內容
            html.AppendLine("                    <tbody>");        
            
            // 產品明細列
            if (quotationDetails.Any())
            {
                int rowNum = 1;
                foreach (var detail in quotationDetails)
                {
                    var product = productDict.GetValueOrDefault(detail.ProductId);
                    var unit = detail.UnitId.HasValue ? unitDict.GetValueOrDefault(detail.UnitId.Value) : null;
                    
                    html.AppendLine("                        <tr>");
                    html.AppendLine($"                            <td class='text-center'>{rowNum}</td>");
                    html.AppendLine($"                            <td class='text-left'>{product?.Name ?? ""}</td>");
                    html.AppendLine($"                            <td class='text-center'>{unit?.Name ?? ""}</td>");
                    html.AppendLine($"                            <td class='text-right'>{detail.Quantity:N2}</td>");
                    html.AppendLine($"                            <td class='text-right'>{detail.UnitPrice:N2}</td>");
                    html.AppendLine($"                            <td class='text-right'>{detail.SubtotalAmount:N2}</td>");
                    html.AppendLine($"                            <td class='text-left'>{detail.Remarks ?? ""}</td>");
                    html.AppendLine("                        </tr>");
                    
                    rowNum++;
                }
            }
            else
            {
                // 無明細時顯示空行
                html.AppendLine("                        <tr>");
                html.AppendLine("                            <td colspan='7' class='text-center' style='padding: 30px; color: #999;'>尚無報價商品</td>");
                html.AppendLine("                        </tr>");
            }
            
            html.AppendLine("                    </tbody>");
            html.AppendLine("                </table>");            
            html.AppendLine("            </div>");
        }

        /// <summary>
        /// 3. 金額區（合計、稅額、總計金額、訂金）
        /// </summary>
        private void GenerateAmountAndDescriptionSection(
            StringBuilder html,
            Quotation quotation)
        {
            html.AppendLine("            <!-- 金額區 -->");
            html.AppendLine("            <div class='quotation-bottom-section'>");
            html.AppendLine("                <table class='quotation-amount-table'>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <td class='amount-label'>合　　　　計</td>");
            html.AppendLine("                        <td class='amount-value'></td>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <td class='amount-label'>稅　額　5　%</td>");
            html.AppendLine("                        <td class='amount-value'></td>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                    <tr class='total-row'>");
            html.AppendLine("                        <td class='amount-label'>總 計 金 額</td>");
            html.AppendLine($"                        <td class='amount-value'>NT$ {quotation.TotalAmount:N0}</td>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <td class='amount-label'>訂　　　　金</td>");
            html.AppendLine("                        <td class='amount-value'></td>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </table>");
            html.AppendLine("            </div>");
        }

        /// <summary>
        /// 4. 簽章區 - 買方簽章、經辦業務、驗收
        /// </summary>
        private void GenerateSignatureSection(StringBuilder html)
        {
            html.AppendLine("            <!-- 簽章區 -->");
            html.AppendLine("            <div class='quotation-signature-section'>");
            html.AppendLine("                <table class='quotation-signature-table'>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <td width='33%' class='signature-cell'>");
            html.AppendLine("                            <div class='signature-label'>買方簽章：</div>");
            html.AppendLine("                            <div class='signature-space'></div>");
            html.AppendLine("                        </td>");
            html.AppendLine("                        <td width='34%' class='signature-cell'>");
            html.AppendLine("                            <div class='signature-label'>經辦業務：</div>");
            html.AppendLine("                            <div class='signature-space'></div>");
            html.AppendLine("                        </td>");
            html.AppendLine("                        <td width='33%' class='signature-cell'>");
            html.AppendLine("                            <div class='signature-label'>驗　　收：</div>");
            html.AppendLine("                            <div class='signature-space'></div>");
            html.AppendLine("                        </td>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </table>");
            html.AppendLine("            </div>");
        }

        /// <summary>
        /// 報價單專用樣式（符合 Excel 格式設計）
        /// </summary>
        private string GetQuotationCustomStyles()
        {
            return @"
        /* 報價單專用容器 */
        .quotation-container {
            font-family: '標楷體', 'DFKai-SB', 'BiauKai', 'Kaiti TC', serif;
        }

        .print-a4-layout {
            width: 210mm;
            min-height: 297mm;
            padding: 5mm 5mm 5mm 5mm; /* 調整為：上5mm 右5mm 下5mm 左5mm */
            background: white;
            box-sizing: border-box;
        }

        /* 1. 標題區 */
        .quotation-title-section {
            margin-bottom: 0;
            position: relative;
        }

        /* 第一行：Logo（靠左） + 公司名稱和報價單（置中） + 報價資訊（靠右） */
        .quotation-header-row-1 {
            position: relative;
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 5px;
            min-height: 90px;
            gap: 10px; /* 添加間距 */
        }

        .company-logo {
            height: auto;
            width: auto;
            max-height: 80px;
            max-width: 100px;
            object-fit: contain;
            flex-shrink: 0;
            margin-right: auto; /* Logo 靠左 */
        }

        .quotation-title-wrapper {
            position: absolute;
            left: 50%;
            transform: translateX(-50%);
            text-align: center;
        }

        .quotation-date-info {
            text-align: right;
            font-size: 11pt;
            line-height: 1.6;
            white-space: nowrap;
            flex-shrink: 0;
            margin-left: auto; /* 報價資訊靠右 */
        }

        .quotation-company-name {
            font-size: 24pt;
            font-weight: bold;
            text-align: center;
            letter-spacing: 2px;
            margin-bottom: 5px;
        }

        .quotation-doc-title {
            font-size: 20pt;
            font-weight: bold;
            text-align: center;
            letter-spacing: 8px;
        }

        /* 第二行：聯絡資訊（一行） */
        .quotation-header-row-2 {
            text-align: center;
            font-size: 10pt;
            margin-bottom: 1px;
            line-height: 1.5;
        }

        .quotation-doc-info {
            margin-bottom: 5px;
            padding: 3px 0;
        }

        .quotation-info-row {
            display: flex;
            align-items: baseline;
            padding: 1px 0;
            font-size: 11pt;
            line-height: 1.3;
        }

        .info-label {
            font-weight: bold;
            margin-right: 4px;
            white-space: nowrap;
        }

        .info-value {
            flex: 1;
            margin-right: 20px;
        }

        .info-value-full {
            flex: 3;
            margin-right: 0;
        }

        .highlight-field {
            background-color: #ffeb9c;
            font-weight: bold;
        }

        /* 2. 產品明細區 */
        .quotation-product-section {
            margin-bottom: 0;
        }

        .quotation-product-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11pt;
            margin-top: -1px; /* 避免與上方表格邊框重疊 */
        }

        .quotation-product-table th,
        .quotation-product-table td {
            border: 1px solid #000;
            padding: 1px 8px;
        }

        .quotation-product-table thead tr {
            background-color: #ffc000;
            font-weight: bold;
        }

        .yellow-header th {
            background-color: #ffc000 !important;
        }

        .yellow-row td {
            background-color: #ffeb9c;
        }

        .quotation-note {
            text-align: center;
            margin-top: 5px;
            font-size: 10pt;
            font-weight: bold;
        }

        /* 3. 金額區 */
        .quotation-bottom-section {
            margin-bottom: 0;
        }

        .quotation-amount-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11pt;
            margin-top: -1px; /* 避免與上方表格邊框重疊 */
        }

        .quotation-amount-table td {
            border: none;
            padding: 1px 8px;
        }

        .amount-label {
            font-weight: bold;
            text-align: center;
            width: 30%;
        }

        .amount-value {
            text-align: right;
            width: 70%;
        }

        .total-row .amount-label,
        .total-row .amount-value {
            background-color: #ffeb9c;
            font-weight: bold;
        }

        /* 4. 簽章區 */
        .quotation-signature-section {
            margin-top: 0;
        }

        .quotation-signature-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11pt;
            margin-top: -1px; /* 避免與上方表格邊框重疊 */
        }

        .quotation-signature-table td {
            border: none;
            padding: 1px 8px;
            text-align: center;
            height: 60px;
            vertical-align: top;
        }

        .signature-label {
            font-weight: bold;
            text-align: left;
            margin-bottom: 5px;
        }

        .signature-space {
            height: 40px;
        }

        /* 通用樣式 */
        .text-left { text-align: left; }
        .text-center { text-align: center; }
        .text-right { text-align: right; }

        /* 列印樣式 */
        @media print {
            .print-a4-layout {
                margin: 0;
                padding: 5mm 8mm; /* 列印時使用較小的邊距 */
                box-shadow: none;
            }

            @page {
                size: A4 portrait;
                margin: 0; /* 瀏覽器邊距設為0，由 padding 控制內容邊距 */
            }

            body {
                margin: 0;
                padding: 0;
            }
        }
";
        }

        /// <summary>
        /// 批次生成報價單報表
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

                // 根據條件查詢報價單
                var quotations = await _quotationService.GetByBatchCriteriaAsync(criteria);

                if (quotations == null || !quotations.Any())
                {
                    // 返回空結果提示頁面
                    return GenerateEmptyResultPage(criteria);
                }

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => await GenerateBatchHtmlReportAsync(quotations, reportPrintConfig, criteria),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成批次報價單報表時發生錯誤: {ex.Message}", ex);
            }
        }

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
        <h1>無符合條件的報價單</h1>
        <p>篩選條件：{criteria.GetSummary()}</p>
        <p>請調整篩選條件後重新查詢。</p>
    </div>
</body>
</html>";
        }

        private async Task<string> GenerateBatchHtmlReportAsync(
            List<Quotation> quotations,
            ReportPrintConfiguration? reportPrintConfig,
            BatchPrintCriteria criteria)
        {
            var html = new StringBuilder();

            // HTML 文件開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>報價單批次列印 ({quotations.Count} 筆)</title>");
            html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
            html.AppendLine("    <style>");
            html.AppendLine(GetQuotationCustomStyles());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // 載入共用資料
            var allProducts = await _productService.GetAllAsync();
            var productDict = allProducts.ToDictionary(p => p.Id, p => p);

            var allUnits = await _unitService.GetAllAsync();
            var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

            // 逐一生成每張報價單
            for (int i = 0; i < quotations.Count; i++)
            {
                var quotation = quotations[i];
                var quotationDetails = quotation.QuotationDetails?.ToList() ?? new List<QuotationDetail>();

                Customer? customer = null;
                if (quotation.CustomerId > 0)
                {
                    customer = await _customerService.GetByIdAsync(quotation.CustomerId);
                }

                Employee? employee = null;
                if (quotation.EmployeeId.HasValue && quotation.EmployeeId.Value > 0)
                {
                    employee = await _employeeService.GetByIdAsync(quotation.EmployeeId.Value);
                }

                Company? company = await _companyService.GetPrimaryCompanyAsync();

                // 生成單一報價單內容
                html.AppendLine("    <div class='print-container quotation-container'>");
                html.AppendLine("        <div class='print-a4-layout'>");
                GenerateTitleSection(html, company, quotation, customer, employee);
                GenerateProductDetailSection(html, quotationDetails, productDict, unitDict);
                GenerateAmountAndDescriptionSection(html, quotation);
                GenerateSignatureSection(html);
                html.AppendLine("        </div>");
                html.AppendLine("    </div>");
            }

            // 列印腳本
            html.AppendLine(GetPrintScript());

            html.AppendLine("</body>");
            html.AppendLine("</html>");

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
