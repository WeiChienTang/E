using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 傳票分錄 - 傳票的借貸明細行，每一行記錄一個科目的借或貸金額
    /// 一張傳票（JournalEntry）必須包含至少一借一貸，且借貸總額相等
    /// </summary>
    [Index(nameof(JournalEntryId))]
    [Index(nameof(AccountItemId))]
    [Index(nameof(AccountItemId), nameof(Direction))]
    public class JournalEntryLine : BaseEntity
    {
        /// <summary>
        /// 傳票主檔ID
        /// </summary>
        [Required(ErrorMessage = "傳票為必填")]
        [Display(Name = "傳票")]
        [ForeignKey(nameof(JournalEntry))]
        public int JournalEntryId { get; set; }

        /// <summary>
        /// 行號 - 傳票內的順序編號，從 1 開始
        /// </summary>
        [Required(ErrorMessage = "行號為必填")]
        [Display(Name = "行號")]
        public int LineNumber { get; set; }

        /// <summary>
        /// 會計科目ID - 只允許使用 IsDetailAccount = true 的明細科目
        /// </summary>
        [Required(ErrorMessage = "會計科目為必填")]
        [Display(Name = "會計科目")]
        [ForeignKey(nameof(AccountItem))]
        public int AccountItemId { get; set; }

        /// <summary>
        /// 借貸方向（借方/貸方）
        /// </summary>
        [Required(ErrorMessage = "借貸方向為必填")]
        [Display(Name = "借貸方向")]
        public AccountDirection Direction { get; set; }

        /// <summary>
        /// 金額 - 恆為正數，方向由 Direction 決定
        /// </summary>
        [Required(ErrorMessage = "金額為必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
        [Display(Name = "金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 分錄說明 - 此行分錄的補充說明
        /// </summary>
        [MaxLength(200, ErrorMessage = "分錄說明不可超過200個字元")]
        [Display(Name = "分錄說明")]
        public string? LineDescription { get; set; }

        // Navigation Properties

        /// <summary>
        /// 傳票導航屬性
        /// </summary>
        public JournalEntry JournalEntry { get; set; } = null!;

        /// <summary>
        /// 會計科目導航屬性
        /// </summary>
        public AccountItem AccountItem { get; set; } = null!;

        // Computed Properties (NotMapped)

        /// <summary>
        /// 是否為借方分錄
        /// </summary>
        [NotMapped]
        public bool IsDebit => Direction == AccountDirection.Debit;

        /// <summary>
        /// 是否為貸方分錄
        /// </summary>
        [NotMapped]
        public bool IsCredit => Direction == AccountDirection.Credit;
    }
}
