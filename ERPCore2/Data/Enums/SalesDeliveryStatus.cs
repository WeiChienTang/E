using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 銷貨出貨狀態枚舉
    /// </summary>
    public enum SalesDeliveryStatus
    {
        [Description("待出貨")]
        Pending = 1,
        
        [Description("備貨中")]
        Preparing = 2,
        
        [Description("已出貨")]
        Delivered = 3,
        
        [Description("已送達")]
        Received = 4,
        
        [Description("已取消")]
        Cancelled = 5
    }
}
