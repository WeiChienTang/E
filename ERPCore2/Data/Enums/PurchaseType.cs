using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 採購類型枚舉
    /// </summary>
    public enum PurchaseType
    {
        [Description("一般採購")]
        Normal = 1,
        
        [Description("緊急採購")]
        Urgent = 2,
        
        [Description("補貨採購")]
        Replenishment = 3,
        
        [Description("專案採購")]
        Project = 4
    }
}
