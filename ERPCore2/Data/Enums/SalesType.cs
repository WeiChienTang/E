using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 銷貨類型枚舉
    /// </summary>
    public enum SalesType
    {
        [Description("一般銷貨")]
        Normal = 1,
        
        [Description("緊急銷貨")]
        Urgent = 2,
        
        [Description("預售")]
        PreSale = 3,
        
        [Description("專案銷貨")]
        Project = 4,
        
        [Description("試用銷貨")]
        Trial = 5
    }
}
