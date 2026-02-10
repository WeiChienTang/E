namespace ERPCore2.Models.Inventory
{
    /// <summary>
    /// 訂單庫存檢查結果
    /// </summary>
    public class OrderInventoryCheckResult
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 訂單編號
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 檢查項目列表
        /// </summary>
        public List<OrderInventoryCheckItem> Items { get; set; } = new();

        /// <summary>
        /// 整體滿足率 (0-100)
        /// </summary>
        public decimal OverallSatisfactionRate { get; set; }

        /// <summary>
        /// 不足項目數量
        /// </summary>
        public int InsufficientItemCount { get; set; }

        /// <summary>
        /// 警戒項目數量
        /// </summary>
        public int WarningItemCount { get; set; }

        /// <summary>
        /// 是否所有項目都充足
        /// </summary>
        public bool IsFullySatisfied => InsufficientItemCount == 0 && WarningItemCount == 0;

        /// <summary>
        /// 檢查時間
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.Now;
    }
}
