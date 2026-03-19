using ERPCore2.Models.Reports;

namespace ERPCore2.Models.Barcode
{
    /// <summary>
    /// 品項條碼批次列印條件
    /// </summary>
    public class ItemBarcodePrintCriteria
    {
        /// <summary>
        /// 品項ID列表（空列表表示列印所有品項）
        /// </summary>
        public List<int> ItemIds { get; set; } = new();

        /// <summary>
        /// 品項分類ID列表（可用於篩選特定分類的品項）
        /// </summary>
        public List<int> CategoryIds { get; set; } = new();

        /// <summary>
        /// 是否只列印有條碼的品項
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
        /// 是否顯示品項名稱
        /// </summary>
        public bool ShowItemName { get; set; } = true;

        /// <summary>
        /// 是否顯示品項編號
        /// </summary>
        public bool ShowItemCode { get; set; } = true;

        /// <summary>
        /// 每個品項的列印數量字典 (ItemId -> PrintQuantity)
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
                    errors.Add($"品項ID {kvp.Key} 的列印數量必須大於0");
                }
                if (kvp.Value > 100)
                {
                    errors.Add($"品項ID {kvp.Key} 的列印數量不能超過100");
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

            if (ItemIds.Any())
            {
                summary.Add($"選擇 {ItemIds.Count} 個品項");
            }
            else
            {
                summary.Add("所有品項");
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
