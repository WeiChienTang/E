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

        /// <summary>品項 ID</summary>
        public int ItemId { get; set; }

        /// <summary>品項編號</summary>
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>品項名稱</summary>
        public string ItemName { get; set; } = string.Empty;

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

        /// <summary>來源銷售訂單 ID（照片 Tab 用）</summary>
        public int? SalesOrderId { get; set; }

        /// <summary>來源訂單編號（顯示用）</summary>
        public string? SalesOrderCode { get; set; }

        /// <summary>客戶名稱（顯示用）</summary>
        public string? CustomerName { get; set; }

        /// <summary>預計交貨日（來自銷售訂單明細）</summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>是否已結案</summary>
        public bool IsClosed { get; set; }

        /// <summary>是否已有任何組件被領料（IssuedQuantity > 0）；為 true 時禁止退回待排清單</summary>
        public bool HasIssuedMaterial { get; set; }
    }

    /// <summary>
    /// 看板待排程項目 DTO - 顯示在左側 Sidebar 中尚未完全排期的銷貨訂單明細
    /// 資料來源為 SalesOrderDetail（非 ProductionScheduleItem），PendingQuantity > 0 才顯示
    /// </summary>
    public class BoardPendingItemDto
    {
        /// <summary>SalesOrderDetail.Id</summary>
        public int SalesOrderDetailId { get; set; }

        /// <summary>所屬銷貨訂單 ID</summary>
        public int SalesOrderId { get; set; }

        /// <summary>訂單編號（顯示用）</summary>
        public string SalesOrderCode { get; set; } = string.Empty;

        /// <summary>訂單日期</summary>
        public DateTime? OrderDate { get; set; }

        /// <summary>預計交貨日</summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>客戶名稱（顯示用）</summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>品項 ID</summary>
        public int ItemId { get; set; }

        /// <summary>品項編號</summary>
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>品項名稱</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>訂單數量</summary>
        public decimal OrderQuantity { get; set; }

        /// <summary>已排程數量（跨所有排程批次的加總）</summary>
        public decimal ScheduledQuantity { get; set; }

        /// <summary>待排數量 = 訂單量 - 已排程量</summary>
        public decimal PendingQuantity => OrderQuantity - ScheduledQuantity;

        /// <summary>訂單是否已審核（false = 待審核，UI 灰化且鎖定拖曳）</summary>
        public bool IsApproved { get; set; } = true;
    }
}
