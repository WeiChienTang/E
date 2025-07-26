using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 採購退回原因枚舉
    /// </summary>
    public enum PurchaseReturnReason
    {
        [Description("品質不良")]
        QualityIssue = 1,
        
        [Description("規格不符")]
        SpecificationMismatch = 2,
        
        [Description("數量錯誤")]
        QuantityError = 3,
        
        [Description("過期商品")]
        ExpiredProduct = 4,
        
        [Description("運送損壞")]
        ShippingDamage = 5,
        
        [Description("廠商要求")]
        SupplierRequest = 6,
        
        [Description("其他")]
        Other = 99
    }
}
