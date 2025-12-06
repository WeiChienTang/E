using System.ComponentModel;

namespace ERPCore2.Data.Enums
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
        Completed = 2
    }
}
