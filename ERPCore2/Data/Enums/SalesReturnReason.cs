using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 銷貨退回原因枚舉
    /// </summary>
    public enum SalesReturnReason
    {
        [Description("品質不良")]
        QualityIssue = 1,
        
        [Description("規格不符")]
        SpecificationMismatch = 2,
        
        [Description("數量錯誤")]
        QuantityError = 3,
        
        [Description("客戶要求")]
        CustomerRequest = 4,
        
        [Description("過期商品")]
        ExpiredProduct = 5,
        
        [Description("運送損壞")]
        ShippingDamage = 6,
        
        [Description("其他")]
        Other = 99
    }
}
