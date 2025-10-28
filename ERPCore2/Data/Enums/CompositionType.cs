using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 產品合成類型
    /// </summary>
    public enum CompositionType
    {
        /// <summary>
        /// 標準配方
        /// </summary>
        [Display(Name = "標準配方")]
        Standard = 1,

        /// <summary>
        /// 替代配方
        /// </summary>
        [Display(Name = "替代配方")]
        Alternative = 2,

        /// <summary>
        /// 簡化配方
        /// </summary>
        [Display(Name = "簡化配方")]
        Simplified = 3,

        /// <summary>
        /// 客製配方
        /// </summary>
        [Display(Name = "客製配方")]
        Custom = 4
    }
}
