using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 採購進貨狀態枚舉
    /// </summary>
    public enum PurchaseReceiptStatus
    {
        [Description("草稿")]
        Draft = 1,
        
        [Description("已確認")]
        Confirmed = 2,
        
        [Description("已入庫")]
        Received = 3,
        
        [Description("已取消")]
        Cancelled = 4
    }
}
