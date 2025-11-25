using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 稅率計算方式
    /// </summary>
    public enum TaxCalculationMethod
    {
        /// <summary>
        /// 外加稅 - 稅額 = 金額 × 稅率，總計 = 金額 + 稅額
        /// </summary>
        [Display(Name = "外加稅")]
        TaxExclusive = 1,

        /// <summary>
        /// 內含稅 - 總計已含稅，稅額 = 總計 / (1 + 稅率) × 稅率
        /// </summary>
        [Display(Name = "內含稅")]
        TaxInclusive = 2,

        /// <summary>
        /// 不含稅 - 不計算稅額，稅額為 0
        /// </summary>
        [Display(Name = "不含稅")]
        NoTax = 3
    }
}
