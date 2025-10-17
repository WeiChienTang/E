using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 採購單報表服務實作 - 新版（使用精確尺寸控制）
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
            html.AppendLine("    <div class='print-container'>");
            html.AppendLine("        <div class='print-single-layout'>");

            // 公司標頭
            GenerateHeader(html, purchaseOrder, supplier, company);

            // 採購資訊區塊
            GenerateInfoSection(html, purchaseOrder, supplier, company);

            // 明細表格
            GenerateDetailTable(html, orderDetails, productDict);

            // 統計區域
            GenerateSummarySection(html, purchaseOrder, taxRate);

            // 簽名區域
            GenerateSignatureSection(html);

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // 列印腳本
            html.AppendLine(GetPrintScript());
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void GenerateHeader(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company)
        {
            html.AppendLine("            <div class='print-header'>");
            html.AppendLine("                <div class='print-company-header'>");
            
            // 左側：公司資訊
            html.AppendLine("                    <div class='print-company-left'>");
            html.AppendLine($"                        <div class='print-info-row'><strong>統一編號：</strong>{company?.TaxId ?? ""}</div>");
            html.AppendLine($"                        <div class='print-info-row'><strong>聯絡電話：</strong>{company?.Phone ?? ""}</div>");
            html.AppendLine($"                        <div class='print-info-row'><strong>傳　　真：</strong>{company?.Fax ?? ""}</div>");
            html.AppendLine("                    </div>");
            
            // 中間：公司名稱與報表標題
            html.AppendLine("                    <div class='print-company-center'>");
            html.AppendLine($"                        <div class='print-company-name'>{company?.CompanyName ?? "公司名稱"}</div>");
            html.AppendLine("                        <div class='print-report-title'>採購單</div>");
            html.AppendLine("                    </div>");
            
            // 右側：頁次
            html.AppendLine("                    <div class='print-company-right'>");
            html.AppendLine("                        <div class='print-info-row'>第 1 頁</div>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
        }

        private void GenerateInfoSection(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company)
        {
            html.AppendLine("            <div class='print-info-section'>");
            html.AppendLine("                <div class='print-info-grid'>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>採購單號：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{purchaseOrder.PurchaseOrderNumber}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>採購日期：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{purchaseOrder.OrderDate:yyyy/MM/dd}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>交貨日期：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{purchaseOrder.ExpectedDeliveryDate:yyyy/MM/dd}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>廠商名稱：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{supplier?.CompanyName ?? ""}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>聯絡人：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{supplier?.ContactPerson ?? ""}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>統一編號：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{supplier?.TaxNumber ?? ""}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='print-info-grid-2col mt-2'>");
            
            html.AppendLine("                    <div class='print-info-item'>");
            html.AppendLine("                        <span class='print-info-label'>送貨地址：</span>");
            html.AppendLine($"                        <span class='print-info-value'>{company?.Address ?? ""}</span>");
            html.AppendLine("                    </div>");
            
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
        }

        private void GenerateDetailTable(StringBuilder html, List<PurchaseOrderDetail> orderDetails, Dictionary<int, Product> productDict)
        {
            html.AppendLine("            <table class='print-table'>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th style='width: 5%;'>序號</th>");
            html.AppendLine("                        <th style='width: 30%;'>品名</th>");
            html.AppendLine("                        <th style='width: 10%;'>數量</th>");
            html.AppendLine("                        <th style='width: 8%;'>單位</th>");
            html.AppendLine("                        <th style='width: 12%;'>單價</th>");
            html.AppendLine("                        <th style='width: 15%;'>小計</th>");
            html.AppendLine("                        <th style='width: 20%;'>備註</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            int rowNum = 1;
            if (orderDetails != null && orderDetails.Any())
            {
                foreach (var detail in orderDetails)
                {
                    productDict.TryGetValue(detail.ProductId, out var product);
                    
                    html.AppendLine("                    <tr>");
                    html.AppendLine($"                        <td class='text-center'>{rowNum}</td>");
                    html.AppendLine($"                        <td class='text-left'>{product?.Name ?? ""}</td>");
                    html.AppendLine($"                        <td class='text-right'>{detail.OrderQuantity:N0}</td>");
                    html.AppendLine("                        <td class='text-center'>個</td>");
                    html.AppendLine($"                        <td class='text-right'>{detail.UnitPrice:N2}</td>");
                    html.AppendLine($"                        <td class='text-right'>{detail.SubtotalAmount:N2}</td>");
                    html.AppendLine($"                        <td class='text-left'>{detail.Remarks ?? ""}</td>");
                    html.AppendLine("                    </tr>");
                    rowNum++;
                }
            }

            // 填充空白行
            for (int i = rowNum; i <= 8; i++)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td class='text-center'>{i}</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                        <td>&nbsp;</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
        }

        private void GenerateSummarySection(StringBuilder html, PurchaseOrder purchaseOrder, decimal taxRate)
        {
            html.AppendLine("            <div class='print-summary'>");
            html.AppendLine("                <div class='print-summary-left'>");
            html.AppendLine("                    <div class='print-remarks'>");
            html.AppendLine("                        <div class='print-remarks-label'>備註：</div>");
            html.AppendLine($"                        <div class='print-remarks-content'>{purchaseOrder.Remarks ?? ""}</div>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='print-summary-right'>");
            html.AppendLine("                    <div class='print-summary-row'>");
            html.AppendLine("                        <span class='print-summary-label'>金額小計：</span>");
            html.AppendLine($"                        <span class='print-summary-value'>{purchaseOrder.TotalAmount:N2}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='print-summary-row'>");
            html.AppendLine($"                        <span class='print-summary-label'>稅額({taxRate:F2}%)：</span>");
            html.AppendLine($"                        <span class='print-summary-value'>{purchaseOrder.PurchaseTaxAmount:N2}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class='print-summary-row'>");
            html.AppendLine("                        <span class='print-summary-label'>含稅總計：</span>");
            html.AppendLine($"                        <span class='print-summary-value font-bold'>{purchaseOrder.PurchaseTotalAmountIncludingTax:N2}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
        }

        private void GenerateSignatureSection(StringBuilder html)
        {
            html.AppendLine("            <div class='print-signature-section'>");
            html.AppendLine("                <div class='print-signature-item'>");
            html.AppendLine("                    <div class='print-signature-label'>採購人員</div>");
            html.AppendLine("                    <div class='print-signature-line'></div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='print-signature-item'>");
            html.AppendLine("                    <div class='print-signature-label'>核准人員</div>");
            html.AppendLine("                    <div class='print-signature-line'></div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='print-signature-item'>");
            html.AppendLine("                    <div class='print-signature-label'>收貨確認</div>");
            html.AppendLine("                    <div class='print-signature-line'></div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
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
