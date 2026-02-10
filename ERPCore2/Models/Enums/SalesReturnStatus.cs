using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 銷貨退回狀態枚舉
    /// </summary>
    public enum SalesReturnStatus
    {
        [Description("草稿")]
        Draft = 1,
        
        [Description("已送出")]
        Submitted = 2,
        
        [Description("已確認")]
        Confirmed = 3,
        
        [Description("處理中")]
        Processing = 4,
        
        [Description("已完成")]
        Completed = 5,
        
        [Description("已取消")]
        Cancelled = 6,
        
        [Description("已關閉")]
        Closed = 7
    }
}
