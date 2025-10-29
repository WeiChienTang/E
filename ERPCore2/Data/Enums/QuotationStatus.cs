using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 報價單狀態
    /// </summary>
    public enum QuotationStatus
    {
        [Display(Name = "草稿")]
        Draft = 1,

        [Display(Name = "已提交")]
        Submitted = 2,

        [Display(Name = "已接受")]
        Accepted = 3,

        [Display(Name = "已拒絕")]
        Rejected = 4,

        [Display(Name = "已過期")]
        Expired = 5,

        [Display(Name = "已轉單")]
        Converted = 6,

        [Display(Name = "已取消")]
        Cancelled = 7
    }
}
