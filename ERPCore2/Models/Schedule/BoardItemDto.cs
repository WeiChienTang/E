using ERPCore2.Models.Enums;

namespace ERPCore2.Models.Schedule
{
    /// <summary>
    /// 看板已排程項目 DTO - 顯示在週視圖日欄中的卡片
    /// </summary>
    public class BoardScheduleItemDto
    {
        /// <summary>ProductionScheduleItem.Id</summary>
        public int Id { get; set; }

        /// <summary>所屬批次 ID</summary>
        public int ProductionScheduleId { get; set; }

        /// <summary>所屬批次編號（PS-YYYYMMDD）</summary>
        public string ProductionScheduleCode { get; set; } = string.Empty;

        /// <summary>商品 ID</summary>
        public int ProductId { get; set; }

        /// <summary>商品編號</summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>商品名稱</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>排程數量</summary>
        public decimal ScheduledQuantity { get; set; }

        /// <summary>已完成數量</summary>
        public decimal CompletedQuantity { get; set; }

        /// <summary>待完成數量</summary>
        public decimal PendingQuantity => ScheduledQuantity - CompletedQuantity;

        /// <summary>生產狀態</summary>
        public ProductionItemStatus ProductionItemStatus { get; set; }

        /// <summary>優先順序（數字越小越優先）</summary>
        public int Priority { get; set; }

        /// <summary>計畫生產日</summary>
        public DateTime? PlannedStartDate { get; set; }

        /// <summary>預計完成日期</summary>
        public DateTime? PlannedEndDate { get; set; }

        /// <summary>來源銷售訂單明細 ID</summary>
        public int? SalesOrderDetailId { get; set; }

        /// <summary>來源訂單編號（顯示用）</summary>
        public string? SalesOrderCode { get; set; }

        /// <summary>客戶名稱（顯示用）</summary>
        public string? CustomerName { get; set; }

        /// <summary>預計交貨日（來自銷售訂單明細）</summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>是否已結案</summary>
        public bool IsClosed { get; set; }
    }

    /// <summary>
    /// 看板待排程項目 DTO - 顯示在左側 Sidebar 中等待被拖曳到看板的項目
    /// </summary>
    public class BoardPendingItemDto
    {
        /// <summary>ProductionScheduleItem.Id</summary>
        public int Id { get; set; }

        /// <summary>所屬批次 ID</summary>
        public int ProductionScheduleId { get; set; }

        /// <summary>所屬批次編號</summary>
        public string ProductionScheduleCode { get; set; } = string.Empty;

        /// <summary>商品 ID</summary>
        public int ProductId { get; set; }

        /// <summary>商品編號</summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>商品名稱</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>排程數量</summary>
        public decimal ScheduledQuantity { get; set; }

        /// <summary>已完成數量</summary>
        public decimal CompletedQuantity { get; set; }

        /// <summary>待完成數量</summary>
        public decimal PendingQuantity => ScheduledQuantity - CompletedQuantity;

        /// <summary>生產狀態</summary>
        public ProductionItemStatus ProductionItemStatus { get; set; }

        /// <summary>優先順序</summary>
        public int Priority { get; set; }

        /// <summary>來源銷售訂單明細 ID</summary>
        public int? SalesOrderDetailId { get; set; }

        /// <summary>來源訂單編號（顯示用）</summary>
        public string? SalesOrderCode { get; set; }

        /// <summary>客戶名稱（顯示用）</summary>
        public string? CustomerName { get; set; }

        /// <summary>預計交貨日（來自銷售訂單明細）</summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>是否已結案</summary>
        public bool IsClosed { get; set; }
    }
}
