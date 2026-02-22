using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 會計科目表（Chart of Accounts）
    /// 採用自我參照樹狀結構，支援多層級科目階層
    /// </summary>
    public class AccountItem : BaseEntity
    {
        /// <summary>
        /// 會計項目名稱（中文）
        /// </summary>
        [Required(ErrorMessage = "會計項目名稱為必填")]
        [MaxLength(100, ErrorMessage = "會計項目名稱不可超過100個字元")]
        [Display(Name = "會計項目名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 英文名稱
        /// </summary>
        [MaxLength(200, ErrorMessage = "英文名稱不可超過200個字元")]
        [Display(Name = "英文名稱")]
        public string? EnglishName { get; set; }

        /// <summary>
        /// 項目說明（中文）
        /// </summary>
        [MaxLength(500, ErrorMessage = "項目說明不可超過500個字元")]
        [Display(Name = "項目說明")]
        public string? Description { get; set; }

        /// <summary>
        /// 英文說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "英文說明不可超過500個字元")]
        [Display(Name = "英文說明")]
        public string? EnglishDescription { get; set; }

        /// <summary>
        /// 科目層級（1=大分類, 2=子分類, 3=中分類, 4=明細科目）
        /// </summary>
        [Required(ErrorMessage = "科目層級為必填")]
        [Display(Name = "科目層級")]
        public int AccountLevel { get; set; }

        /// <summary>
        /// 父層科目 ID（自我參照外鍵，一級科目為 null）
        /// </summary>
        [Display(Name = "父層科目")]
        public int? ParentId { get; set; }

        /// <summary>
        /// 父層科目導航屬性
        /// </summary>
        public AccountItem? Parent { get; set; }

        /// <summary>
        /// 子科目集合
        /// </summary>
        public ICollection<AccountItem> Children { get; set; } = new List<AccountItem>();

        /// <summary>
        /// 科目大類（資產、負債、權益、收入、成本、費用等）
        /// </summary>
        [Required(ErrorMessage = "科目大類為必填")]
        [Display(Name = "科目大類")]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// 借貸方向
        /// </summary>
        [Required(ErrorMessage = "借貸方向為必填")]
        [Display(Name = "借貸方向")]
        public AccountDirection Direction { get; set; }

        /// <summary>
        /// 是否為明細科目（僅明細科目可用於記帳分錄）
        /// </summary>
        [Display(Name = "是否為明細科目")]
        public bool IsDetailAccount { get; set; }

        /// <summary>
        /// 排序順序（控制同層級科目的顯示位置）
        /// </summary>
        [Display(Name = "排序順序")]
        public int SortOrder { get; set; }
    }
}
