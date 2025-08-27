using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 採購進貨狀態枚舉
    /// </summary>
    public enum PurchaseReceivingStatus
    {
        [Description("草稿")]
        Draft = 1,
        
        [Description("已核准")]
        Approved = 2,
        
        [Description("已執行")]
        Executed = 3,
        
        [Description("已作廢")]
        Voided = 4
    }
}
