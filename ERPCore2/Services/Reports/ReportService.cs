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
                
                // 報表內容
                html.AppendLine("    <div class='report-container'>");
                
                // 公司標頭
                html.AppendLine(GenerateCompanyHeader(configuration));
                
                // 報表標題
                html.AppendLine(GenerateReportTitle(configuration));
                
                // 頁首區段
                html.AppendLine(GenerateHeaderSections(configuration, reportData));
                
                // 明細表格（如果有）
                if (reportData.DetailEntities != null && reportData.DetailEntities.Any())
                {
                    html.AppendLine(GenerateDetailTable<TMainEntity, TDetailEntity>(configuration, reportData));
                }
                
                // 頁尾區段
                html.AppendLine(GenerateFooterSections(configuration, reportData));
                
                // 頁尾資訊
                html.AppendLine(GeneratePageFooter(configuration));
                
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
        
        public async Task<byte[]> GeneratePdfReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class
        {
            // TODO: 實作 PDF 生成功能
            // 可以使用 iTextSharp、PuppeteerSharp 等套件
            await Task.Delay(1);
            throw new NotImplementedException("PDF 報表功能尚未實作");
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
            var pageSize = configuration.PageSize.ToString().ToLower();
            
            return $@"
    <style>
        @media print {{
            @page {{
                size: {pageSize} {orientation};
                margin: 1cm;
            }}
            body {{ 
                -webkit-print-color-adjust: exact; 
                print-color-adjust: exact; 
            }}
        }}
        
        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            margin: 0;
            padding: 20px;
            font-size: 12px;
            line-height: 1.4;
            color: #333;
        }}
        
        .report-container {{
            max-width: 800px;
            margin: 0 auto;
            background: white;
        }}
        
        .company-header {{
            text-align: center;
            border-bottom: 2px solid #333;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }}
        
        .company-name {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 5px;
        }}
        
        .company-info {{
            font-size: 10px;
            color: #666;
        }}
        
        .report-title {{
            text-align: center;
            font-size: 16px;
            font-weight: bold;
            margin: 20px 0;
            text-decoration: underline;
        }}
        
        .report-section {{
            margin-bottom: 20px;
        }}
        
        .section-title {{
            font-weight: bold;
            font-size: 13px;
            margin-bottom: 10px;
            padding-bottom: 3px;
            border-bottom: 1px solid #ddd;
        }}
        
        .field-row {{
            display: flex;
            margin-bottom: 8px;
        }}
        
        .field-item {{
            flex: 1;
            margin-right: 20px;
        }}
        
        .field-item:last-child {{
            margin-right: 0;
        }}
        
        .field-label {{
            font-weight: bold;
            display: inline-block;
            min-width: 80px;
        }}
        
        .field-value {{
            display: inline-block;
        }}
        
        .detail-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        
        .detail-table th,
        .detail-table td {{
            border: 1px solid #333;
            padding: 6px;
            text-align: left;
        }}
        
        .detail-table th {{
            background-color: #f5f5f5;
            font-weight: bold;
            text-align: center;
        }}
        
        .text-center {{ text-align: center; }}
        .text-right {{ text-align: right; }}
        .text-left {{ text-align: left; }}
        
        .font-bold {{ font-weight: bold; }}
        
        .page-footer {{
            margin-top: 30px;
            text-align: center;
            font-size: 10px;
            color: #666;
            border-top: 1px solid #ddd;
            padding-top: 10px;
        }}
        
        .statistics-section {{
            border: 1px solid #333;
            padding: 10px;
            background-color: #f9f9f9;
        }}
    </style>";
        }
        
        /// <summary>
        /// 生成公司標頭
        /// </summary>
        private string GenerateCompanyHeader(ReportConfiguration configuration)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='company-header'>");
            html.AppendLine($"            <div class='company-name'>{configuration.CompanyName}</div>");
            
            if (!string.IsNullOrEmpty(configuration.CompanyAddress) || !string.IsNullOrEmpty(configuration.CompanyPhone))
            {
                html.AppendLine("            <div class='company-info'>");
                if (!string.IsNullOrEmpty(configuration.CompanyAddress))
                    html.AppendLine($"                地址：{configuration.CompanyAddress}");
                if (!string.IsNullOrEmpty(configuration.CompanyPhone))
                    html.AppendLine($"                電話：{configuration.CompanyPhone}");
                html.AppendLine("            </div>");
            }
            
            html.AppendLine("        </div>");
            return html.ToString();
        }
        
        /// <summary>
        /// 生成報表標題
        /// </summary>
        private string GenerateReportTitle(ReportConfiguration configuration)
        {
            var html = new StringBuilder();
            html.AppendLine($"        <div class='report-title'>{configuration.Title}</div>");
            
            if (!string.IsNullOrEmpty(configuration.Subtitle))
            {
                html.AppendLine($"        <div class='text-center' style='margin-bottom: 20px;'>{configuration.Subtitle}</div>");
            }
            
            return html.ToString();
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
                html.AppendLine("        <div class='report-section'>");
                
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
            
            if (configuration.ShowPageNumbers)
            {
                html.AppendLine("        <div class='page-footer'>");
                html.AppendLine($"            報表生成時間：{configuration.GeneratedAt:yyyy/MM/dd HH:mm:ss}");
                html.AppendLine("        </div>");
            }
            
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
        /// 取得表格欄位值（舊版本，保持向下相容）
        /// </summary>
        private string GetColumnValue<TDetailEntity>(ReportColumnDefinition column, TDetailEntity detail)
        {
            return GetColumnValue(column, detail, new Dictionary<string, object>(), 0);
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
    }
}
