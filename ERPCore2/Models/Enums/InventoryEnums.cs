using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 庫存交易類型枚舉
    /// </summary>
    public enum InventoryTransactionTypeEnum
    {
        [Description("期初庫存")]
        OpeningBalance = 1,
        
        [Description("進貨")]
        Purchase = 2,
        
        [Description("銷貨")]
        Sale = 3,
        
        [Description("進貨退出")]
        Return = 4,
        
        [Description("銷貨退回")]
        SalesReturn = 12,
        
        [Description("調整")]
        Adjustment = 5,
        
        [Description("轉倉")]
        Transfer = 6,
        
        [Description("盤點")]
        StockTaking = 7,
        
        [Description("生產投料")]
        ProductionConsumption = 8,
        
        [Description("生產完工")]
        ProductionCompletion = 9,
        
        [Description("報廢")]
        Scrap = 10,
        
        [Description("領料")]
        MaterialIssue = 11,
        
        [Description("領料退回")]
        MaterialReturn = 13,

        [Description("廢料收料")]
        WasteReceiving = 14
    }

    /// <summary>
    /// 庫存預留類型
    /// </summary>
    public enum InventoryReservationType
    {
        [Description("銷售訂單")]
        SalesOrder = 1,
        
        [Description("生產訂單")]
        ProductionOrder = 2,
        
        [Description("轉倉單")]
        TransferOrder = 3,
        
        [Description("其他")]
        Other = 4
    }

    /// <summary>
    /// 庫存預留狀態
    /// </summary>
    public enum InventoryReservationStatus
    {
        [Description("預留中")]
        Reserved = 1,
        
        [Description("部分釋放")]
        PartiallyReleased = 2,
        
        [Description("已釋放")]
        Released = 3,
        
        [Description("已取消")]
        Cancelled = 4
    }

    /// <summary>
    /// 倉庫位置類型
    /// </summary>
    public enum WarehouseLocationTypeEnum
    {
        [Description("普通儲位")]
        Normal = 1,
        
        [Description("冷藏儲位")]
        Refrigerated = 2,
        
        [Description("冷凍儲位")]
        Frozen = 3,
        
        [Description("危險品儲位")]
        Hazardous = 4,
        
        [Description("收貨區")]
        Receiving = 5,
        
        [Description("出貨區")]
        Shipping = 6,
        
        [Description("品檢區")]
        QualityControl = 7,
        
        [Description("隔離區")]
        Quarantine = 8
    }

    /// <summary>
    /// 庫存盤點類型
    /// </summary>
    public enum StockTakingTypeEnum
    {
        [Description("全盤")]
        Full = 1,
        
        [Description("循環盤點")]
        Cycle = 2,
        
        [Description("抽樣盤點")]
        Sample = 3,
        
        [Description("特定商品盤點")]
        Specific = 4,
        
        [Description("特定位置盤點")]
        Location = 5
    }

    /// <summary>
    /// 庫存盤點狀態
    /// </summary>
    public enum StockTakingStatusEnum
    {
        [Description("草稿")]
        Draft = 1,
        
        [Description("進行中")]
        InProgress = 2,
        
        [Description("已完成")]
        Completed = 3,
        
        [Description("待審核")]
        PendingApproval = 4,
        
        [Description("已審核")]
        Approved = 5,
        
        [Description("已取消")]
        Cancelled = 6
    }

    /// <summary>
    /// 庫存盤點明細狀態
    /// </summary>
    public enum StockTakingDetailStatusEnum
    {
        [Description("待盤點")]
        Pending = 1,
        
        [Description("已盤點")]
        Counted = 2,
        
        [Description("重新盤點")]
        Recounted = 3,
        
        [Description("確認無誤")]
        Confirmed = 4,
        
        [Description("跳過")]
        Skipped = 5
    }

    /// <summary>
    /// 庫存異動操作類型
    /// 用於標識每筆異動明細的操作類型（新增、調整、刪除回退）
    /// </summary>
    public enum InventoryOperationTypeEnum
    {
        /// <summary>
        /// 首次入庫/出庫（來源單據確認時）
        /// </summary>
        [Description("初始")]
        Initial = 1,

        /// <summary>
        /// 編輯調整（來源單據編輯時產生的差異）
        /// </summary>
        [Description("調整")]
        Adjust = 2,

        /// <summary>
        /// 刪除回退（來源單據刪除時的庫存回退）
        /// </summary>
        [Description("刪除回退")]
        Delete = 3

        // 🔑 已移除 PriceAdjust：價格調整不應產生庫存異動記錄
        // 價格變更時直接調整 InventoryStockDetail.AverageCost
    }
}
