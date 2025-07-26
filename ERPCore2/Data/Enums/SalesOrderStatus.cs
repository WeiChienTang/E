using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 銷貨訂單狀態枚舉
    /// </summary>
    public enum SalesOrderStatus
    {
        [Description("草稿")]
        Draft = 1,
        
        [Description("已送出")]
        Submitted = 2,
        
        [Description("已確認")]
        Confirmed = 3,
        
        [Description("部分出貨")]
        PartialDelivered = 4,
        
        [Description("已完成")]
        Completed = 5,
        
        [Description("已取消")]
        Cancelled = 6,
        
        [Description("已關閉")]
        Closed = 7
    }
}
