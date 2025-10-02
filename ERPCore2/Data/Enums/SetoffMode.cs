using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 沖款模式列舉
    /// </summary>
    public enum SetoffMode
    {
        /// <summary>
        /// 應收帳款 - 向客戶收款
        /// </summary>
        [Display(Name = "應收帳款")]
        Receivable = 1,

        /// <summary>
        /// 應付帳款 - 向供應商付款
        /// </summary>
        [Display(Name = "應付帳款")]
        Payable = 2
    }
}
