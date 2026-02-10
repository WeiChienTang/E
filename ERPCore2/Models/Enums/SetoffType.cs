using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 沖款類型列舉
    /// </summary>
    public enum SetoffType
    {
        /// <summary>應收帳款沖款（收款）</summary>
        [Display(Name = "應收帳款沖款")]
        AccountsReceivable = 1,
        
        /// <summary>應付帳款沖款（付款）</summary>
        [Display(Name = "應付帳款沖款")]
        AccountsPayable = 2
    }
}
