namespace ERPCore2.Models.Inventory
{
    /// <summary>
    /// 供應商推薦資訊
    /// 用於低庫存警戒時的供應商推薦清單
    /// </summary>
    public class SupplierRecommendationDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string? SupplierItemCode { get; set; }
        
        // 價格資訊
        public decimal? LastPurchasePrice { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int PurchaseCount { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        
        // 推薦資訊
        public bool IsPreferred { get; set; }
        public int Priority { get; set; }
        public string RecommendationSource { get; set; } = string.Empty;  // "Preferred", "History", "Both"
        public int? LeadTimeDays { get; set; }
        public string? Remarks { get; set; }
        
        // UI 輔助屬性
        public string DisplayOrder => IsPreferred ? $"⭐ {Priority}" : "📋";
        
        public string PriceRange
        {
            get
            {
                if (LowestPrice.HasValue && HighestPrice.HasValue && LowestPrice != HighestPrice)
                {
                    return $"${LowestPrice:N0} - ${HighestPrice:N0}";
                }
                
                if (LastPurchasePrice.HasValue)
                {
                    return $"${LastPurchasePrice:N0}";
                }
                
                return "無報價";
            }
        }
    }
}
