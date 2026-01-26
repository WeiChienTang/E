namespace ERPCore2.Models
{
    /// <summary>
    /// 待排程明細 DTO - 用於明細選擇彈窗
    /// </summary>
    public class PendingScheduleDetailDto
    {
        /// <summary>
        /// 訂單明細 ID
        /// </summary>
        public int SalesOrderDetailId { get; set; }
        
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// 商品編號
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// 訂單數量
        /// </summary>
        public decimal OrderQuantity { get; set; }
        
        /// <summary>
        /// 已排程數量
        /// </summary>
        public decimal ScheduledQuantity { get; set; }
        
        /// <summary>
        /// 待排程數量
        /// </summary>
        public decimal PendingQuantity { get; set; }
        
        /// <summary>
        /// 本次排程數量
        /// </summary>
        public decimal ScheduleQuantity { get; set; }
        
        /// <summary>
        /// 是否可排程（待排程數量 > 0）
        /// </summary>
        public bool CanSchedule => PendingQuantity > 0;
        
        /// <summary>
        /// UI 狀態：是否被選取
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
