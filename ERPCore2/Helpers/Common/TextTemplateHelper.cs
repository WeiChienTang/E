using ERPCore2.Models;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 通用文字範本處理輔助類別
    /// 提供變數替換、明細格式化等功能
    /// 可套用到採購單、銷貨單、報價單、Email 範本等場景
    /// </summary>
    public static class TextTemplateHelper
    {
        /// <summary>
        /// 替換範本中的變數
        /// 支援變數：{supplierName}, {customerName}, {orderCode}, {orderDate}, {companyName} 等
        /// </summary>
        /// <param name="template">範本文字</param>
        /// <param name="variables">變數字典 (key: 變數名, value: 值)</param>
        /// <returns>替換後的文字</returns>
        public static string ReplaceVariables(string template, Dictionary<string, string?> variables)
        {
            if (string.IsNullOrEmpty(template)) 
                return string.Empty;

            var result = template;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value ?? string.Empty);
            }
            return result;
        }

        /// <summary>
        /// 格式化明細項目列表
        /// </summary>
        /// <typeparam name="TDetail">明細類型</typeparam>
        /// <param name="details">明細列表</param>
        /// <param name="config">格式設定</param>
        /// <param name="lineFormatter">行格式化函數</param>
        /// <returns>格式化後的明細文字</returns>
        public static string FormatDetailLines<TDetail>(
            IEnumerable<TDetail> details,
            DetailFormatConfig config,
            Func<TDetail, int, DetailFormatConfig, string> lineFormatter)
        {
            if (details == null || !details.Any())
                return string.Empty;

            var lines = details
                .Select((detail, index) => lineFormatter(detail, index + 1, config))
                .Where(line => !string.IsNullOrWhiteSpace(line));

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 組合完整訊息（問候語 + 明細 + 結語）
        /// </summary>
        /// <param name="header">問候語（第一段）</param>
        /// <param name="details">明細內容（第二段）</param>
        /// <param name="footer">結語（第三段）</param>
        /// <returns>完整訊息</returns>
        public static string BuildFullMessage(string header, string details, string footer)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(header))
                parts.Add(header.Trim());

            if (!string.IsNullOrWhiteSpace(details))
                parts.Add(details.Trim());

            if (!string.IsNullOrWhiteSpace(footer))
                parts.Add(footer.Trim());

            return string.Join("\n\n", parts);
        }

        /// <summary>
        /// 建立常用變數字典
        /// </summary>
        /// <param name="supplierName">供應商名稱</param>
        /// <param name="customerName">客戶名稱</param>
        /// <param name="orderCode">單據編號</param>
        /// <param name="orderDate">單據日期</param>
        /// <param name="companyName">本公司名稱</param>
        /// <returns>變數字典</returns>
        public static Dictionary<string, string?> CreateVariables(
            string? supplierName = null,
            string? customerName = null,
            string? orderCode = null,
            DateTime? orderDate = null,
            string? companyName = null)
        {
            return new Dictionary<string, string?>
            {
                { "supplierName", supplierName },
                { "customerName", customerName },
                { "orderCode", orderCode },
                { "orderDate", orderDate?.ToString("yyyy/MM/dd") },
                { "companyName", companyName }
            };
        }

        /// <summary>
        /// 格式化採購單明細行
        /// </summary>
        /// <param name="productCode">商品編號</param>
        /// <param name="productName">商品名稱</param>
        /// <param name="quantity">數量</param>
        /// <param name="unit">單位</param>
        /// <param name="unitPrice">單價</param>
        /// <param name="subtotal">小計</param>
        /// <param name="remark">備註</param>
        /// <param name="index">序號</param>
        /// <param name="config">格式設定</param>
        /// <returns>格式化後的行文字</returns>
        public static string FormatDetailLine(
            string? productCode,
            string? productName,
            decimal? quantity,
            string? unit,
            decimal? unitPrice,
            decimal? subtotal,
            string? remark,
            int index,
            DetailFormatConfig config)
        {
            var parts = new List<string>();

            // 序號
            parts.Add($"{index}.");

            // 商品編號
            if (config.ShowProductCode && !string.IsNullOrWhiteSpace(productCode))
            {
                parts.Add($"[{productCode}]");
            }

            // 商品名稱
            if (config.ShowProductName && !string.IsNullOrWhiteSpace(productName))
            {
                parts.Add(productName);
            }

            // 數量和單位
            if (config.ShowQuantity && quantity.HasValue)
            {
                var quantityStr = $"x {quantity.Value:G29}";
                if (config.ShowUnit && !string.IsNullOrWhiteSpace(unit))
                {
                    quantityStr += $" {unit}";
                }
                parts.Add(quantityStr);
            }

            // 單價
            if (config.ShowUnitPrice && unitPrice.HasValue)
            {
                parts.Add($"@ ${unitPrice.Value:N2}");
            }

            // 小計
            if (config.ShowSubtotal && subtotal.HasValue)
            {
                parts.Add($"= ${subtotal.Value:N2}");
            }

            // 備註
            if (config.ShowRemark && !string.IsNullOrWhiteSpace(remark))
            {
                parts.Add($"({remark})");
            }

            return string.Join(" ", parts);
        }
    }
}
