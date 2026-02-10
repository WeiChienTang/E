using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 商品採購/製造類型
    /// </summary>
    public enum ProcurementType
    {
        /// <summary>
        /// 外購 - 直接向供應商採購
        /// </summary>
        [Display(Name = "外購")]
        Purchased = 0,
        
        /// <summary>
        /// 自製 - 內部生產製造(需要排程)
        /// </summary>
        [Display(Name = "自製")]
        Manufactured = 1,
        
        /// <summary>
        /// 委外 - 委外加工(未來擴展用)
        /// </summary>
        [Display(Name = "委外")]
        Outsourced = 2
    }
}
