using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 生產排程項目狀態
    /// </summary>
    public enum ProductionItemStatus
    {
        /// <summary>
        /// 待生產 - 排程已建立，尚未開始生產
        /// </summary>
        [Description("待生產")]
        Pending = 0,
        
        /// <summary>
        /// 生產中 - 已開始生產，組件已領料
        /// </summary>
        [Description("生產中")]
        InProgress = 1,
        
        /// <summary>
        /// 已完成 - 生產完成，成品已入庫
        /// </summary>
        [Description("已完成")]
        Completed = 2,
        
        /// <summary>
        /// 已停產（舊值，保留 DB 相容）
        /// </summary>
        [Description("已停產")]
        Discontinued = 3,

        /// <summary>
        /// 等待領料 - 排入看板但 BOM 物料尚未完成領料
        /// </summary>
        [Description("等待領料")]
        WaitingMaterial = 4
    }
}
