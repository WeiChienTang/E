using ERPCore2.Models.Enums;

namespace ERPCore2.Models.Schedule
{
    /// <summary>
    /// 排程項目 DTO - 用於 UI 暫存和傳輸
    /// </summary>
    public class ScheduleItemDto
    {
        /// <summary>
        /// 排程項目 ID（編輯現有項目時有值）
        /// </summary>
        public int? Id { get; set; }
        
        /// <summary>
        /// 來源銷售訂單明細 ID
        /// </summary>
        public int SalesOrderDetailId { get; set; }
        
        /// <summary>
        /// 來源銷售訂單 ID（顯示用）
        /// </summary>
        public int SalesOrderId { get; set; }
        
        /// <summary>
        /// 來源銷售訂單編號（顯示用）
        /// </summary>
        public string SalesOrderCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// 商品編號（顯示用）
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品名稱（顯示用）
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// 訂單數量
        /// </summary>
        public decimal OrderQuantity { get; set; }
        
        /// <summary>
        /// 已排程數量（此明細在其他排程中已排的數量）
        /// </summary>
        public decimal AlreadyScheduledQuantity { get; set; }
        
        /// <summary>
        /// 待排程數量 = 訂單數量 - 已排程數量
        /// </summary>
        public decimal PendingScheduleQuantity { get; set; }
        
        /// <summary>
        /// 本次排程數量
        /// </summary>
        public decimal ScheduleQuantity { get; set; }
        
        /// <summary>
        /// 已完成數量（編輯模式時顯示）
        /// </summary>
        public decimal CompletedQuantity { get; set; }
        
        /// <summary>
        /// 生產狀態
        /// </summary>
        public ProductionItemStatus Status { get; set; } = ProductionItemStatus.Pending;
        
        /// <summary>
        /// 是否為新項目（尚未儲存到資料庫）
        /// </summary>
        public bool IsNew { get; set; } = true;
        
        /// <summary>
        /// 排序順序（用於排程項目的拖曳排序）
        /// </summary>
        public int SortOrder { get; set; } = 0;
        
        /// <summary>
        /// 是否可刪除（已有完成數量的不可刪除）
        /// </summary>
        public bool CanDelete { get; set; } = true;
        
        /// <summary>
        /// 是否已結案 - 手動標記不再追蹤生產進度
        /// </summary>
        public bool IsClosed { get; set; } = false;
        
        /// <summary>
        /// 客戶名稱（顯示用）
        /// </summary>
        public string? CustomerName { get; set; }
    }
}
