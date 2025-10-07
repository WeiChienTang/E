using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 預收付款項類型
    /// </summary>
    public enum PrepaymentType
    {
        /// <summary>
        /// 應收 - 預先收取客戶的款項（預收款）
        /// </summary>
        [Display(Name = "應收")]
        AccountsReceivable = 1,
        
        /// <summary>
        /// 應收轉沖款 - 應收款項轉為沖款使用
        /// </summary>
        [Display(Name = "應收轉沖款")]
        AccountsReceivableToSetoff = 2,
        
        /// <summary>
        /// 預付 - 預先支付給供應商的款項
        /// </summary>
        [Display(Name = "預付")]
        Prepaid = 3,
        
        /// <summary>
        /// 預付轉沖款 - 預付款項轉為沖款使用
        /// </summary>
        [Display(Name = "預付轉沖款")]
        PrepaidToSetoff = 4,
        
        /// <summary>
        /// 其他 - 其他類型的預收付款項
        /// </summary>
        [Display(Name = "其他")]
        Other = 5
    }
}
