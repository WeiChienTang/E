namespace ERPCore2.Models
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

    /// <summary>
    /// 訂單庫存檢查項目
    /// </summary>
    public class OrderInventoryCheckItem
    {
        /// <summary>
        /// 層級 (1=明細, 2=組成)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 父層明細ID (如果是組成元件)
        /// </summary>
        public int? ParentDetailId { get; set; }

        /// <summary>
        /// 明細ID
        /// </summary>
        public int? DetailId { get; set; }

        /// <summary>
        /// 產品ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 產品編號
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// 產品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 產品規格
        /// </summary>
        public string? ProductSpecification { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// 需求數量
        /// </summary>
        public decimal RequiredQuantity { get; set; }

        /// <summary>
        /// 可用庫存
        /// </summary>
        public decimal AvailableStock { get; set; }

        /// <summary>
        /// 缺口數量 (需求 - 庫存，如果為負則為0)
        /// </summary>
        public decimal ShortageQuantity => RequiredQuantity > AvailableStock 
            ? RequiredQuantity - AvailableStock 
            : 0;

        /// <summary>
        /// 庫存狀態
        /// </summary>
        public InventoryStatus Status { get; set; }

        /// <summary>
        /// 組成倍數 (如果是組成元件，表示每組需要的數量)
        /// </summary>
        public decimal? CompositionMultiplier { get; set; }

        /// <summary>
        /// 是否為組合產品
        /// </summary>
        public bool IsComposition { get; set; }

        /// <summary>
        /// 子項目 (組合產品的元件)
        /// </summary>
        public List<OrderInventoryCheckItem> Children { get; set; } = new();

        /// <summary>
        /// 是否展開顯示子項目
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// 取得狀態圖示
        /// </summary>
        public string StatusIcon => Status switch
        {
            InventoryStatus.Sufficient => "🟢",
            InventoryStatus.Warning => "🟡",
            InventoryStatus.Insufficient => "🔴",
            _ => "⚪"
        };

        /// <summary>
        /// 取得狀態文字
        /// </summary>
        public string StatusText => Status switch
        {
            InventoryStatus.Sufficient => "充足",
            InventoryStatus.Warning => "警戒",
            InventoryStatus.Insufficient => "不足",
            _ => "未知"
        };

        /// <summary>
        /// 取得狀態 CSS 類別
        /// </summary>
        public string StatusClass => Status switch
        {
            InventoryStatus.Sufficient => "text-success",
            InventoryStatus.Warning => "text-warning",
            InventoryStatus.Insufficient => "text-danger",
            _ => ""
        };
    }

    /// <summary>
    /// 庫存狀態枚舉
    /// </summary>
    public enum InventoryStatus
    {
        /// <summary>
        /// 充足 - 庫存充足
        /// </summary>
        Sufficient = 0,

        /// <summary>
        /// 警戒 - 庫存低於安全庫存但仍足夠
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 不足 - 庫存不足以滿足需求
        /// </summary>
        Insufficient = 2
    }
}
