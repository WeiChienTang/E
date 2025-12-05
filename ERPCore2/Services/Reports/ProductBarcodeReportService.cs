using System.Text;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 商品條碼報表服務實作
    /// </summary>
    public class ProductBarcodeReportService : IProductBarcodeReportService
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductBarcodeReportService> _logger;

        public ProductBarcodeReportService(
            IProductService productService,
            ILogger<ProductBarcodeReportService> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// 生成條碼批次列印報表
        /// </summary>
        public async Task<string> GenerateBarcodeReportAsync(ProductBarcodePrintCriteria criteria)
        {
            try
            {
                // 驗證條件
                var validation = criteria.Validate();
                if (!validation.IsValid)
                {
                    return GenerateErrorPage($"條件驗證失敗：{validation.GetAllErrors()}");
                }

                // 載入商品資料
                var products = await LoadProductsAsync(criteria);

                if (products == null || !products.Any())
                {
                    return GenerateEmptyResultPage();
                }

                // 生成 HTML 報表
                return GenerateHtmlReport(products, criteria);
            }
            catch (Exception ex)
            {
                return GenerateErrorPage($"生成報表時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 載入商品資料
        /// </summary>
        private async Task<List<Product>> LoadProductsAsync(ProductBarcodePrintCriteria criteria)
        {
            var allProducts = await _productService.GetAllAsync();

            // 篩選條件
            var query = allProducts.AsQueryable();

            // 只列印有條碼的商品
            if (criteria.OnlyWithBarcode)
            {
                query = query.Where(p => !string.IsNullOrWhiteSpace(p.Barcode));
            }

            // 篩選特定商品
            if (criteria.ProductIds.Any())
            {
                query = query.Where(p => criteria.ProductIds.Contains(p.Id));
            }

            // 篩選特定分類
            if (criteria.CategoryIds.Any())
            {
                query = query.Where(p => p.ProductCategoryId.HasValue && 
                                        criteria.CategoryIds.Contains(p.ProductCategoryId.Value));
            }

            return query.OrderBy(p => p.Code).ToList();
        }

        /// <summary>
        /// 生成 HTML 報表
        /// </summary>
        private string GenerateHtmlReport(List<Product> products, ProductBarcodePrintCriteria criteria)
        {
            var html = new StringBuilder();

            // HTML 開始
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='zh-TW'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>商品條碼列印</title>");
            
            // 引入 JsBarcode 套件
            html.AppendLine("    <script src='https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js'></script>");
            
            // 列印樣式
            html.AppendLine(GeneratePrintStyles(criteria));
            
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // 條碼內容
            html.AppendLine("    <div class='barcode-print-container'>");

            foreach (var product in products)
            {
                // 取得該商品的列印數量
                int quantity = 1;
                if (criteria.PrintQuantities.ContainsKey(product.Id))
                {
                    quantity = criteria.PrintQuantities[product.Id];
                }

                // 生成多份條碼
                for (int i = 0; i < quantity; i++)
                {
                    html.AppendLine($"        <div class='barcode-item barcode-{criteria.BarcodeSize.ToString().ToLower()}'>");
                    
                    if (criteria.ShowProductCode)
                    {
                        html.AppendLine($"            <div class='barcode-code'>{product.Code}</div>");
                    }
                    
                    html.AppendLine($"            <svg class='barcode-svg' id='barcode-{product.Id}-{i}'></svg>");
                    
                    if (criteria.ShowProductName)
                    {
                        html.AppendLine($"            <div class='barcode-name'>{product.Name}</div>");
                    }
                    
                    html.AppendLine("        </div>");
                }
            }

            html.AppendLine("    </div>");

            // 生成條碼的 JavaScript
            html.AppendLine("    <script>");
            html.AppendLine("        window.onload = function() {");
            
            foreach (var product in products)
            {
                int quantity = criteria.PrintQuantities.ContainsKey(product.Id) 
                    ? criteria.PrintQuantities[product.Id] : 1;
                
                for (int i = 0; i < quantity; i++)
                {
                    var (width, height) = GetBarcodeDimensions(criteria.BarcodeSize);
                    html.AppendLine($"            JsBarcode('#barcode-{product.Id}-{i}', '{product.Barcode}', {{");
                    html.AppendLine($"                format: 'CODE128',");
                    html.AppendLine($"                width: {width},");
                    html.AppendLine($"                height: {height},");
                    html.AppendLine($"                displayValue: false,");
                    html.AppendLine($"                margin: 2");
                    html.AppendLine($"            }});");
                }
            }
            
            // 自動列印
            html.AppendLine("            setTimeout(function() {");
            html.AppendLine("                window.print();");
            html.AppendLine("            }, 500);");
            
            html.AppendLine("        };");
            html.AppendLine("    </script>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// 生成列印樣式
        /// </summary>
        private string GeneratePrintStyles(ProductBarcodePrintCriteria criteria)
        {
            var (itemWidth, itemHeight) = GetBarcodeItemSize(criteria.BarcodeSize);
            
            return $@"
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            background: white;
        }}

        .barcode-print-container {{
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 5mm;
            padding: 5mm;
            width: 100%;
        }}

        .barcode-item {{
            border: 1px solid #ccc;
            padding: 3mm;
            text-align: center;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            page-break-inside: avoid;
            overflow: visible;
        }}

        .barcode-item.barcode-small {{
            width: {itemWidth}mm;
            height: {itemHeight}mm;
        }}

        .barcode-item.barcode-medium {{
            width: {itemWidth}mm;
            height: {itemHeight}mm;
        }}

        .barcode-item.barcode-large {{
            width: {itemWidth}mm;
            height: {itemHeight}mm;
        }}

        .barcode-code {{
            font-size: 12pt;
            font-weight: bold;
            margin-bottom: 2mm;
            color: #000;
            line-height: 1.3;
        }}

        .barcode-svg {{
            max-width: 100%;
            height: auto;
        }}

        .barcode-name {{
            font-size: 11pt;
            margin-top: 2mm;
            padding-bottom: 1mm;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            max-width: 100%;
            color: #000;
            line-height: 1.3;
        }}

        @media print {{
            @page {{
                size: A4;
                margin: 10mm;
            }}

            body {{
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }}

            .barcode-print-container {{
                gap: 5mm;
            }}

            .barcode-item {{
                page-break-inside: avoid;
            }}
        }}

        @media screen {{
            body {{
                background: #f5f5f5;
                padding: 20px;
            }}

            .barcode-item {{
                background: white;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            }}
        }}
    </style>";
        }

        /// <summary>
        /// 取得條碼項目尺寸
        /// </summary>
        private (int width, int height) GetBarcodeItemSize(BarcodeSize size)
        {
            return size switch
            {
                BarcodeSize.Small => (40, 20),
                BarcodeSize.Medium => (50, 25),
                BarcodeSize.Large => (70, 35),
                _ => (50, 25)
            };
        }

        /// <summary>
        /// 取得條碼圖片尺寸參數
        /// </summary>
        private (int width, int height) GetBarcodeDimensions(BarcodeSize size)
        {
            return size switch
            {
                BarcodeSize.Small => (1, 30),
                BarcodeSize.Medium => (2, 40),
                BarcodeSize.Large => (3, 70),
                _ => (2, 40)
            };
        }

        /// <summary>
        /// 生成空結果頁面
        /// </summary>
        private string GenerateEmptyResultPage()
        {
            return @"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>條碼列印 - 無資料</title>
    <style>
        body {
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f5f5f5;
        }
        .message {
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .message h1 {
            color: #666;
            font-size: 24px;
            margin-bottom: 10px;
        }
        .message p {
            color: #999;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class='message'>
        <h1>📭 沒有可列印的條碼</h1>
        <p>請確認商品是否已設定條碼號碼</p>
    </div>
    <script>
        setTimeout(function() { window.close(); }, 3000);
    </script>
</body>
</html>";
        }

        /// <summary>
        /// 生成錯誤頁面
        /// </summary>
        private string GenerateErrorPage(string errorMessage)
        {
            return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>條碼列印 - 錯誤</title>
    <style>
        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f5f5f5;
        }}
        .error {{
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            max-width: 500px;
        }}
        .error h1 {{
            color: #dc3545;
            font-size: 24px;
            margin-bottom: 10px;
        }}
        .error p {{
            color: #666;
            font-size: 14px;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='error'>
        <h1>❌ 列印失敗</h1>
        <p>{errorMessage}</p>
    </div>
    <script>
        setTimeout(function() {{ window.close(); }}, 5000);
    </script>
</body>
</html>";
        }
    }
}
