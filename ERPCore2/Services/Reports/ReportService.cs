using System.Reflection;
using System.Text;
using ERPCore2.Models;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 泛型報表服務實作
    /// </summary>
    public class ReportService : IReportService
    {
        public async Task<string> GenerateHtmlReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            try
            {
                var html = new StringBuilder();
                
                // 開始 HTML 文件
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang='zh-TW'>");
                html.AppendLine("<head>");
                html.AppendLine("    <meta charset='UTF-8'>");
                html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                html.AppendLine($"    <title>{configuration.Title}</title>");
                html.AppendLine(GetReportStyles(configuration));
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                
                // 生成完整報表內容
                var reportContent = GenerateReportContent(configuration, reportData);
                
                // 判斷是否需要第二部分
                var needsSecondPart = RequiresSecondPart(configuration, reportData);
                
                html.AppendLine("    <div class='report-container'>");
                
                if (needsSecondPart)
                {
                    // 需要兩部分時：分割內容到上下兩部分
                    var (firstPart, secondPart) = SplitReportContent(reportContent, configuration, reportData);
                    
                    // 第一部分（上半部）
                    html.AppendLine("    <div class='upper-section'>");
                    html.AppendLine(firstPart);
                    html.AppendLine("    </div>");
                    
                    // 中間撕裂線
                    html.AppendLine("    <div class='tear-line'>");
                    html.AppendLine("        <div class='tear-perforations'></div>");
                    html.AppendLine("    </div>");
                    
                    // 第二部分（下半部）
                    html.AppendLine("    <div class='lower-section'>");
                    html.AppendLine(secondPart);
                    html.AppendLine("    </div>");
                }
                else
                {
                    // 內容少時：只使用一個部分，置中顯示
                    html.AppendLine("    <div class='single-section'>");
                    html.AppendLine(reportContent);
                    html.AppendLine("    </div>");
                }
                
                // 頁尾資訊
                html.AppendLine(GeneratePageFooter(configuration));
                
                // 添加列印優化的 JavaScript
                var (jsWidth, jsHeight) = GetPageDimensions(configuration.PageSize);
                html.AppendLine("    <script>");
                html.AppendLine($"        var pageWidth = '{jsWidth}';");
                html.AppendLine($"        var pageHeight = '{jsHeight}';");
                html.AppendLine(@"        
        function getPageDimensions() {
            return {
                width: pageWidth,
                height: pageHeight
            };
        }
        
        function setupPrintOptimization() {
            var pageInfo = getPageDimensions();
            var style = document.createElement('style');
            style.textContent = 
                '@media print {' +
                '  @page { margin: 0; size: ' + pageInfo.width + ' ' + pageInfo.height + '; }' +
                '  html, body { margin: 0 !important; padding: 0 !important; overflow: hidden !important; }' +
                '  .report-container { width: ' + pageInfo.width + ' !important; height: ' + pageInfo.height + ' !important; page-break-inside: avoid !important; page-break-after: avoid !important; }' +
                '  .single-section { page-break-inside: avoid !important; page-break-after: avoid !important; }' +
                '}';
            document.head.appendChild(style);
        }
        
        function handleAutoPrint() {
            if (window.location.search.indexOf('autoprint=true') !== -1) {
                setTimeout(function() {
                    window.print();
                }, 1000);
            }
        }
        
        function optimizedPrint() {
            var printStyle = document.createElement('style');
            printStyle.media = 'print';
            printStyle.textContent = '@page { margin: 0 !important; } body { margin: 0 !important; padding: 2mm !important; }';
            document.head.appendChild(printStyle);
            
            setTimeout(function() {
                window.print();
                document.head.removeChild(printStyle);
            }, 100);
        }
        
        window.addEventListener('load', function() {
            setupPrintOptimization();
            handleAutoPrint();
        });
        
        document.addEventListener('keydown', function(e) {
            if (e.ctrlKey && e.key === 'p') {
                e.preventDefault();
                optimizedPrint();
            }
        });
        
        window.optimizedPrint = optimizedPrint;");
                html.AppendLine("    </script>");
                
                html.AppendLine("    </div>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");
                
                return await Task.FromResult(html.ToString());
            }
            catch (Exception)
            {
                throw new InvalidOperationException("生成 HTML 報表時發生錯誤");
            }
        }
        
        public async Task<string> GeneratePrintableReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            // 生成適合列印的 HTML，包含列印樣式和 JavaScript
            var html = await GenerateHtmlReportAsync(configuration, reportData);
            
            // 加入列印相關的 JavaScript
            var printScript = @"
                <script>
                    window.onload = function() {
                        window.print();
                        // 列印完成後可以關閉視窗（可選）
                        window.onafterprint = function() {
                            // window.close();
                        };
                    };
                </script>
            ";
            
            html = html.Replace("</body>", printScript + "</body>");
            
            return html;
        }
        
        /// <summary>
        /// 生成報表樣式
        /// </summary>
        private string GetReportStyles(ReportConfiguration configuration)
        {
            var orientation = configuration.Orientation == PageOrientation.Landscape ? "landscape" : "portrait";
            
            // 根據頁面大小類型設定實際尺寸
            var (pageWidth, pageHeight) = GetPageDimensions(configuration.PageSize);
            
            return $@"
    <style>
        @media print {{
            @page {{
                size: {pageWidth} {pageHeight} {orientation};
                margin: 0;
            }}
            body {{ 
                -webkit-print-color-adjust: exact; 
                print-color-adjust: exact;
                margin: 0 !important;
                padding: 0 !important;
            }}
            
            /* 強力隱藏瀏覽器預設頁首頁尾 */
            html {{
                margin: 0 !important;
                padding: 0 !important;
            }}
        }}
        
        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            margin: 0;
            padding: 2mm;
            font-size: 13px;
            line-height: 1.3;
            color: #333;
        }}
        
        .report-container {{
            width: {pageWidth};
            height: {pageHeight};
            margin: 0 auto;
            background: white;
            position: relative;
        }}
        
        .single-section {{
            width: {pageWidth};
            height: {pageHeight};
            padding: 4mm 3mm 3mm 3mm;
            box-sizing: border-box;
            margin: 0 auto;
            display: flex;
            flex-direction: column;
        }}
        
        .upper-section {{
            height: calc({pageHeight} / 2);
            padding: 4mm 3mm 3mm 3mm;
            box-sizing: border-box;
            display: flex;
            flex-direction: column;
        }}
        
        .lower-section {{
            height: calc({pageHeight} / 2);
            padding: 4mm 3mm 3mm 3mm;
            box-sizing: border-box;
            display: flex;
            flex-direction: column;
        }}
        
        .content-area {{
            flex: 1;
            overflow: hidden;
        }}
        
        .footer-area {{
            margin-top: auto;
            padding-top: 3mm;
        }}
        
        .tear-line {{
            height: 2mm;
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            border-top: 1px dashed #999;
        }}
        
        .tear-perforations {{
            width: 100%;
            height: 1px;
            position: relative;
        }}
        
        .section-header {{
            text-align: center;
            font-size: 10px;
            color: #666;
            margin-bottom: 3mm;
            font-weight: bold;
        }}
        
        .company-header {{
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            padding-bottom: 3mm;
            margin-bottom: 3mm;
        }}
        
        .company-left-info {{
            flex: 1;
            font-size: 12px;
        }}
        
        .company-left-info .info-row {{
            margin-bottom: 1mm;
        }}
        
        .company-center-section {{
            flex: 1;
            text-align: center;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }}
        
        .company-right-section {{
            flex: 1;
            text-align: center;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }}
        
        .company-name {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 2mm;
        }}
        
        .report-title-inline {{
            font-size: 16px;
            font-weight: bold;
            text-decoration: underline;
        }}
        
        .company-logo {{
            max-height: 30mm;
            max-width: 100%;
        }}
        
        .company-info {{
            font-size: 12px;
            color: #666;
            text-align: right;
        }}
        
        .report-section {{
            margin-bottom: 1.5mm;
        }}
        
        .report-section-bordered {{
            margin-bottom: 1.5mm;
            border: 1px solid #333;
            padding: 3mm;
        }}
        
        .section-title {{
            font-weight: bold;
            font-size: 13px;
            margin-bottom: 3mm;
            padding-bottom: 1mm;
            border-bottom: 1px solid #ddd;
        }}
        
        .field-row {{
            display: flex;
            margin-bottom: 0.5mm;
            line-height: 1.2;
        }}
        
        .field-item {{
            flex: 1;
            margin-right: 5mm;
        }}
        
        .field-item:last-child {{
            margin-right: 0;
        }}
        
        .field-label {{
            font-weight: bold;
            display: inline-block;
            min-width: 20mm;
        }}
        
        .field-value {{
            display: inline-block;
        }}
        
        .detail-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 3mm 0;
            font-size: 13px;
            line-height: 1.1;
        }}
        
        .detail-table th,
        .detail-table td {{
            border: none;
            padding: 1mm 0.5mm;
            vertical-align: middle;
        }}
        
        .detail-table th {{
            font-weight: bold;
            text-align: center;
            border-bottom: 1px solid #333;
            padding-bottom: 1.5mm;
        }}
        
        .detail-table td {{
            text-align: left;
        }}
        
        .detail-table td.text-center {{ text-align: center !important; }}
        .detail-table td.text-right {{ text-align: right !important; }}
        .detail-table td.text-left {{ text-align: left !important; }}
        
        .text-center {{ text-align: center; }}
        .text-right {{ text-align: right; }}
        .text-left {{ text-align: left; }}
        
        .font-bold {{ font-weight: bold; }}
        
        .page-footer {{
            position: absolute;
            bottom: 2mm;
            left: 3mm;
            right: 3mm;
            text-align: center;
            font-size: 9px;
            color: #666;
            border-top: 1px solid #ddd;
            padding-top: 2mm;
        }}
        
        .statistics-section {{
            border: 1px solid #333;
            padding: 3mm;
            margin-top: 2mm;
        }}
    </style>";
        }
        
        /// <summary>
        /// 生成公司標頭
        /// </summary>
        private string GenerateCompanyHeader<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='company-header'>");
            
            // 左側：公司資訊（統一編號、聯絡電話、傳真）
            html.AppendLine("            <div class='company-left-info'>");
            
            // 統一編號
            var taxId = GetFieldValueFromAdditionalData("CompanyTaxId", reportData.AdditionalData);
            html.AppendLine($"                <div class='info-row'><strong>統一編號：</strong>{taxId}</div>");
            
            // 聯絡電話
            var phone = GetFieldValueFromAdditionalData("CompanyPhone", reportData.AdditionalData);
            html.AppendLine($"                <div class='info-row'><strong>聯絡電話：</strong>{phone}</div>");
            
            // 傳真
            var fax = GetFieldValueFromAdditionalData("CompanyFax", reportData.AdditionalData);
            html.AppendLine($"                <div class='info-row'><strong>傳　　真：</strong>{fax}</div>");
            
            html.AppendLine("            </div>");
            
            // 中間：公司名稱和報表標題（置中顯示）
            html.AppendLine("            <div class='company-center-section'>");
            html.AppendLine($"                <div class='company-name'>{configuration.CompanyName}</div>");
            html.AppendLine($"                <div class='report-title-inline'>{configuration.Title}</div>");
            html.AppendLine("            </div>");
            
            // 右側：預留給公司Logo（目前為空）
            html.AppendLine("            <div class='company-right-section'>");
            // 未來可以在這裡添加公司Logo
            // html.AppendLine($"                <img src='{logoUrl}' alt='Company Logo' class='company-logo'>");
            html.AppendLine("            </div>");
            
            html.AppendLine("        </div>");
            return html.ToString();
        }
        
        /// <summary>
        /// 從額外資料中取得欄位值
        /// </summary>
        private string GetFieldValueFromAdditionalData(string key, Dictionary<string, object> additionalData)
        {
            if (additionalData.ContainsKey(key))
            {
                return additionalData[key]?.ToString() ?? "";
            }
            return "";
        }
        
        /// <summary>
        /// 生成頁首區段
        /// </summary>
        private string GenerateHeaderSections<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            var html = new StringBuilder();
            
            foreach (var section in configuration.HeaderSections.OrderBy(s => s.Order))
            {
                var sectionClass = section.HasBorder ? "report-section-bordered" : "report-section";
                html.AppendLine($"        <div class='{sectionClass}'>");
                
                if (!string.IsNullOrEmpty(section.Title))
                {
                    html.AppendLine($"            <div class='section-title'>{section.Title}</div>");
                }
                
                // 將欄位分組顯示
                var fieldGroups = ChunkFields(section.Fields, section.FieldsPerRow);
                
                foreach (var fieldGroup in fieldGroups)
                {
                    html.AppendLine("            <div class='field-row'>");
                    
                    foreach (var field in fieldGroup)
                    {
                        var value = GetFieldValue(field, reportData.MainEntity, reportData.AdditionalData);
                        var boldClass = field.IsBold ? " font-bold" : "";
                        
                        html.AppendLine($"                <div class='field-item{boldClass}'>");
                        html.AppendLine($"                    <span class='field-label'>{field.Label}：</span>");
                        html.AppendLine($"                    <span class='field-value'>{value}</span>");
                        html.AppendLine("                </div>");
                    }
                    
                    html.AppendLine("            </div>");
                }
                
                html.AppendLine("        </div>");
            }
            
            return html.ToString();
        }
        
        /// <summary>
        /// 生成明細表格
        /// </summary>
        private string GenerateDetailTable<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            var html = new StringBuilder();
            
            if (!configuration.Columns.Any() || reportData.DetailEntities == null || !reportData.DetailEntities.Any()) 
                return html.ToString();
            
            html.AppendLine("        <table class='detail-table'>");
            
            // 表頭
            html.AppendLine("            <thead>");
            html.AppendLine("                <tr>");
            
            foreach (var column in configuration.Columns.Where(c => c.IsVisible).OrderBy(c => c.Order))
            {
                var width = !string.IsNullOrEmpty(column.Width) ? $" style='width: {column.Width}'" : "";
                html.AppendLine($"                    <th{width}>{column.Header}</th>");
            }
            
            html.AppendLine("                </tr>");
            html.AppendLine("            </thead>");
            
            // 表身
            html.AppendLine("            <tbody>");
            
            int rowIndex = 1;
            foreach (var detail in reportData.DetailEntities)
            {
                html.AppendLine("                <tr>");
                
                foreach (var column in configuration.Columns.Where(c => c.IsVisible).OrderBy(c => c.Order))
                {
                    var value = GetColumnValue(column, detail, reportData.AdditionalData, rowIndex);
                    var alignmentClass = GetAlignmentClass(column.Alignment);
                    
                    html.AppendLine($"                    <td class='{alignmentClass}'>{value}</td>");
                }
                
                html.AppendLine("                </tr>");
                rowIndex++;
            }
            
            html.AppendLine("            </tbody>");
            html.AppendLine("        </table>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// 生成頁尾區段
        /// </summary>
        private string GenerateFooterSections<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            var html = new StringBuilder();
            
            foreach (var section in configuration.FooterSections.OrderBy(s => s.Order))
            {
                var sectionClass = section.IsStatisticsSection ? "report-section statistics-section" : "report-section";
                
                html.AppendLine($"        <div class='{sectionClass}'>");
                
                if (!string.IsNullOrEmpty(section.Title))
                {
                    html.AppendLine($"            <div class='section-title'>{section.Title}</div>");
                }
                
                var fieldGroups = ChunkFields(section.Fields, section.FieldsPerRow);
                
                foreach (var fieldGroup in fieldGroups)
                {
                    html.AppendLine("            <div class='field-row'>");
                    
                    foreach (var field in fieldGroup)
                    {
                        var value = GetFieldValue(field, reportData.MainEntity, reportData.AdditionalData);
                        var boldClass = field.IsBold ? " font-bold" : "";
                        
                        html.AppendLine($"                <div class='field-item{boldClass}'>");
                        html.AppendLine($"                    <span class='field-label'>{field.Label}：</span>");
                        html.AppendLine($"                    <span class='field-value'>{value}</span>");
                        html.AppendLine("                </div>");
                    }
                    
                    html.AppendLine("            </div>");
                }
                
                html.AppendLine("        </div>");
            }
            
            return html.ToString();
        }
        
        /// <summary>
        /// 生成頁面頁尾
        /// </summary>
        private string GeneratePageFooter(ReportConfiguration configuration)
        {
            var html = new StringBuilder();
            
            // 移除報表生成時間的顯示
            // 保留空的頁尾結構以備將來擴展
            
            return html.ToString();
        }
        
        /// <summary>
        /// 將欄位分組
        /// </summary>
        private List<List<ReportField>> ChunkFields(List<ReportField> fields, int fieldsPerRow)
        {
            var result = new List<List<ReportField>>();
            
            for (int i = 0; i < fields.Count; i += fieldsPerRow)
            {
                result.Add(fields.Skip(i).Take(fieldsPerRow).ToList());
            }
            
            return result;
        }
        
        /// <summary>
        /// 取得欄位值
        /// </summary>
        private string GetFieldValue(ReportField field, object mainEntity, Dictionary<string, object> additionalData)
        {
            try
            {
                // 如果直接指定了值，使用該值
                if (!string.IsNullOrEmpty(field.Value))
                {
                    return field.Value;
                }
                
                // 嘗試從主實體取得屬性值
                var value = GetPropertyValue(mainEntity, field.PropertyName);
                
                // 如果主實體沒有該屬性，嘗試從額外資料取得
                if (value == null && additionalData.ContainsKey(field.PropertyName))
                {
                    value = additionalData[field.PropertyName];
                }
                
                return FormatValue(value, field.Format);
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 取得表格欄位值
        /// </summary>
        private string GetColumnValue<TDetailEntity>(
            ReportColumnDefinition column, 
            TDetailEntity detail, 
            Dictionary<string, object> additionalData, 
            int rowIndex)
        {
            try
            {
                // 序號欄位特殊處理
                if (column.Header == "序號" && string.IsNullOrEmpty(column.PropertyName))
                {
                    return rowIndex.ToString();
                }
                
                // 如果有自訂值產生器，使用它
                if (column.ValueGenerator != null)
                {
                    return column.ValueGenerator(detail!);
                }
                
                // 商品名稱特殊處理（從 ProductDict 查詢）
                if (column.PropertyName == nameof(PurchaseOrderDetail.ProductId) && 
                    detail is PurchaseOrderDetail orderDetail && 
                    additionalData.ContainsKey("ProductDict"))
                {
                    if (additionalData["ProductDict"] is Dictionary<int, Product> productDict &&
                        productDict.ContainsKey(orderDetail.ProductId))
                    {
                        return productDict[orderDetail.ProductId].Name;
                    }
                    return $"商品ID: {orderDetail.ProductId}";
                }
                
                var value = GetPropertyValue(detail!, column.PropertyName);
                return FormatValue(value, column.Format);
            }
            catch
            {
                return "";
            }
        }    
        
        /// <summary>
        /// 使用反射取得屬性值
        /// </summary>
        private object? GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                return property?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 格式化值
        /// </summary>
        private string FormatValue(object? value, string? format)
        {
            if (value == null) return "";
            
            if (!string.IsNullOrEmpty(format))
            {
                try
                {
                    if (value is DateTime dateTime)
                        return dateTime.ToString(format);
                    if (value is decimal decimalValue)
                        return decimalValue.ToString(format);
                    if (value is double doubleValue)
                        return doubleValue.ToString(format);
                    if (value is int intValue)
                        return intValue.ToString(format);
                }
                catch
                {
                    // 格式化失敗時回傳原始值
                }
            }
            
            return value.ToString() ?? "";
        }
        
        /// <summary>
        /// 取得對齊樣式類別
        /// </summary>
        private string GetAlignmentClass(TextAlignment alignment)
        {
            return alignment switch
            {
                TextAlignment.Center => "text-center",
                TextAlignment.Right => "text-right",
                _ => "text-left"
            };
        }
        
        /// <summary>
        /// 生成完整報表內容
        /// </summary>
        private string GenerateReportContent<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            var html = new StringBuilder();
            
            // 內容區域（公司標頭、頁首區段、明細表格）
            html.AppendLine("            <div class='content-area'>");
            
            // 公司標頭（現在包含報表標題）
            html.AppendLine(GenerateCompanyHeader(configuration, reportData));
            
            // 頁首區段
            html.AppendLine(GenerateHeaderSections(configuration, reportData));
            
            // 明細表格（如果有）
            if (reportData.DetailEntities != null && reportData.DetailEntities.Any())
            {
                html.AppendLine(GenerateDetailTable<TMainEntity, TDetailEntity>(configuration, reportData));
            }
            
            html.AppendLine("            </div>");
            
            // 尾段區域（固定在底部）
            html.AppendLine("            <div class='footer-area'>");
            html.AppendLine(GenerateFooterSections(configuration, reportData));
            html.AppendLine("            </div>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// 判斷是否需要第二部分
        /// </summary>
        private bool RequiresSecondPart<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            // 對於中一刀報表紙 (9.5" x 5.5")，通常不需要分割
            if (configuration.PageSize == PageSize.ContinuousForm)
            {
                return false; // 強制使用單一區段，避免產生額外空白頁
            }

            // 估算內容高度來判斷是否需要分割
            var estimatedHeight = EstimateContentHeight(configuration, reportData);
            
            // 5.5英寸約等於140mm，保留一些邊距
            var maxHeight = 130; // mm
            
            return estimatedHeight > maxHeight;
        }
        
        /// <summary>
        /// 估算內容高度（以mm為單位）
        /// </summary>
        private double EstimateContentHeight<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            double height = 0;
            
            // 公司標頭：約15mm
            height += 15;
            
            // 報表標題：約10mm
            height += 10;
            
            // 頁首區段：每個區段約8mm
            height += configuration.HeaderSections.Count * 8;
            
            // 明細表格：表頭5mm + 每行4mm
            if (reportData.DetailEntities != null && reportData.DetailEntities.Any())
            {
                height += 5; // 表頭
                height += reportData.DetailEntities.Count() * 4; // 每行
            }
            
            // 頁尾區段：每個區段約8mm
            height += configuration.FooterSections.Count * 8;
            
            // 邊距和間距：約10mm
            height += 10;
            
            return height;
        }
        
        /// <summary>
        /// 分割報表內容到兩部分
        /// </summary>
        private (string firstPart, string secondPart) SplitReportContent<TMainEntity, TDetailEntity>(
            string fullContent,
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            // 簡單實作：如果需要分割，第一部分包含標頭和部分明細，第二部分包含剩餘明細和頁尾
            
            var firstPart = new StringBuilder();
            var secondPart = new StringBuilder();
            
            // 第一部分：標頭（使用新的結構）
            firstPart.AppendLine("            <div class='content-area'>");
            firstPart.AppendLine(GenerateCompanyHeader(configuration, reportData));
            firstPart.AppendLine(GenerateHeaderSections(configuration, reportData));
            firstPart.AppendLine("            </div>");
            firstPart.AppendLine("            <div class='footer-area'>");
            // 第一部分的尾段可以為空或包含部分資訊
            firstPart.AppendLine("            </div>");
            
            // 第二部分：完整內容（使用新的結構）
            secondPart.AppendLine("            <div class='content-area'>");
            secondPart.AppendLine(GenerateCompanyHeader(configuration, reportData));
            secondPart.AppendLine(GenerateHeaderSections(configuration, reportData));
            
            if (reportData.DetailEntities != null && reportData.DetailEntities.Any())
            {
                secondPart.AppendLine(GenerateDetailTable<TMainEntity, TDetailEntity>(configuration, reportData));
            }
            
            secondPart.AppendLine("            </div>");
            secondPart.AppendLine("            <div class='footer-area'>");
            secondPart.AppendLine(GenerateFooterSections(configuration, reportData));
            secondPart.AppendLine("            </div>");
            
            return (firstPart.ToString(), secondPart.ToString());
        }

        /// <summary>
        /// 根據頁面大小設定取得實際尺寸
        /// </summary>
        private (string width, string height) GetPageDimensions(PageSize pageSize)
        {
            return pageSize switch
            {
                PageSize.ContinuousForm => ("8.46in", "5.5in"), // 中一刀報表紙
                PageSize.A4 => ("8.27in", "11.69in"), // A4 紙張
                PageSize.Letter => ("8.5in", "11in"), // Letter 紙張
                _ => ("8.46in", "5.5in") // 預設為中一刀報表紙
            };
        }
    }
}
