namespace ERPCore2.Models
{
    /// <summary>
    /// 商品條碼批次列印條件
    /// </summary>
    public class ProductBarcodePrintCriteria
    {
        /// <summary>
        /// 商品ID列表（空列表表示列印所有商品）
        /// </summary>
        public List<int> ProductIds { get; set; } = new();

        /// <summary>
        /// 商品分類ID列表（可用於篩選特定分類的商品）
        /// </summary>
        public List<int> CategoryIds { get; set; } = new();

        /// <summary>
        /// 是否只列印有條碼的商品
        /// </summary>
        public bool OnlyWithBarcode { get; set; } = true;

        /// <summary>
        /// 條碼尺寸
        /// </summary>
        public BarcodeSize BarcodeSize { get; set; } = BarcodeSize.Medium;

        /// <summary>
        /// 每頁條碼數量
        /// </summary>
        public int BarcodesPerPage { get; set; } = 20;

        /// <summary>
        /// 是否顯示商品名稱
        /// </summary>
        public bool ShowProductName { get; set; } = true;

        /// <summary>
        /// 是否顯示商品編號
        /// </summary>
        public bool ShowProductCode { get; set; } = true;

        /// <summary>
        /// 每個商品的列印數量字典 (ProductId -> PrintQuantity)
        /// </summary>
        public Dictionary<int, int> PrintQuantities { get; set; } = new();

        /// <summary>
        /// 驗證條件是否有效
        /// </summary>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            if (BarcodesPerPage <= 0)
            {
                errors.Add("每頁條碼數必須大於0");
            }

            if (BarcodesPerPage > 100)
            {
                errors.Add("每頁條碼數不能超過100");
            }

            // 驗證列印數量
            foreach (var kvp in PrintQuantities)
            {
                if (kvp.Value <= 0)
                {
                    errors.Add($"商品ID {kvp.Key} 的列印數量必須大於0");
                }
                if (kvp.Value > 100)
                {
                    errors.Add($"商品ID {kvp.Key} 的列印數量不能超過100");
                }
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// 取得篩選條件摘要
        /// </summary>
        public string GetSummary()
        {
            var summary = new List<string>();

            if (ProductIds.Any())
            {
                summary.Add($"選擇 {ProductIds.Count} 個商品");
            }
            else
            {
                summary.Add("所有商品");
            }

            if (CategoryIds.Any())
            {
                summary.Add($"{CategoryIds.Count} 個分類");
            }

            var totalQuantity = PrintQuantities.Values.Sum();
            summary.Add($"總列印數量：{totalQuantity} 張");

            summary.Add($"尺寸：{BarcodeSize}");

            return string.Join(" | ", summary);
        }
    }
}
