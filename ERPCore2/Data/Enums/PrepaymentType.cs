using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 預先款項類型
    /// </summary>
    public enum PrepaymentType
    {
        /// <summary>
        /// 預收款 - 預先收取客戶的款項
        /// </summary>
        [Display(Name = "預收款")]
        Prepayment = 1,
        
        /// <summary>
        /// 預付款 - 預先支付給供應商的款項
        /// </summary>
        [Display(Name = "預付款")]
        Prepaid = 2,
        
        /// <summary>
        /// 其他 - 其他類型的預先款項
        /// </summary>
        [Display(Name = "其他")]
        Other = 3
    }
}
