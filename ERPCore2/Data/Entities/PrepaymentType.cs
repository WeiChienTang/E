using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預收付款項類型實體 - 定義預收付款項的分類
    /// </summary>
    public class PrepaymentType : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "名稱為必填")]
        [MaxLength(50, ErrorMessage = "名稱不可超過50個字元")]
        [Display(Name = "名稱")]
        public string Name { get; set; } = string.Empty;
        
        // Navigation Properties
        /// <summary>
        /// 預收付款項集合
        /// </summary>
        public ICollection<SetoffPrepayment> SetoffPrepayments { get; set; } = new List<SetoffPrepayment>();
    }
}
