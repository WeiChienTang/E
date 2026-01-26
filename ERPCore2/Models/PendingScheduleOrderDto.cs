namespace ERPCore2.Models
{
    /// <summary>
    /// 待排程訂單 DTO - 用於左側表格顯示
    /// </summary>
    public class PendingScheduleOrderDto
    {
        /// <summary>
        /// 訂單 ID
        /// </summary>
        public int SalesOrderId { get; set; }
        
        /// <summary>
        /// 訂單編號
        /// </summary>
        public string OrderCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 訂單日期
        /// </summary>
        public DateTime OrderDate { get; set; }
        
        /// <summary>
        /// 客戶 ID
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
        
        /// <summary>
        /// 待排程總數量
        /// </summary>
        public decimal PendingScheduleQuantity { get; set; }
        
        /// <summary>
        /// 待排程項目數量
        /// </summary>
        public int PendingItemCount { get; set; }
        
        /// <summary>
        /// 訂單明細列表（展開時顯示）
        /// </summary>
        public List<PendingScheduleDetailDto> Details { get; set; } = new();
    }
}
