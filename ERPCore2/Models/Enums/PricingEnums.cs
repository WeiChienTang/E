using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 定價類型枚舉
    /// </summary>
    public enum PricingType
    {
        [Description("標準價格")]
        Standard = 1,
        
        [Description("促銷價格")]
        Promotional = 2,
        
        [Description("客戶專屬價")]
        CustomerSpecific = 3,
        
        [Description("客戶群組價")]
        CustomerGroup = 4,
        
        [Description("數量折扣價")]
        VolumeDiscount = 5,
        
        [Description("VIP價格")]
        VIP = 6,
        
        [Description("清倉價格")]
        Clearance = 7,
        
        [Description("試用價格")]
        Trial = 8
    }

    /// <summary>
    /// 價格類型枚舉 - 用於價格歷史記錄
    /// </summary>
    public enum PriceType
    {
        [Description("銷售價格")]
        SalesPrice = 1,
        
        [Description("成本價格")]
        CostPrice = 2,
        
        [Description("供應商報價")]
        SupplierPrice = 3,
        
        [Description("標準價格")]
        StandardPrice = 4,
        
        [Description("促銷價格")]
        PromotionalPrice = 5
    }
}
