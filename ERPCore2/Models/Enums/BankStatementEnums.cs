using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 銀行對帳單狀態
    /// </summary>
    public enum BankStatementStatus
    {
        [Description("草稿")]
        [Display(Name = "草稿")]
        Draft = 1,

        [Description("對帳中")]
        [Display(Name = "對帳中")]
        InProgress = 2,

        [Description("已完成")]
        [Display(Name = "已完成")]
        Completed = 3
    }
}
