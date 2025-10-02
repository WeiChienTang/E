using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預收款實體 - 記錄預先收取的款項
    /// </summary>
    [Index(nameof(Code))]
    [Index(nameof(SetoffId))]
    [Index(nameof(PrepaymentDate))]
    public class Prepayment : BaseEntity
    {
        /// <summary>
        /// 預收款日期
        /// </summary>
        [Required(ErrorMessage = "預收款日期為必填")]
        [Display(Name = "預收款日期")]
        public DateTime PrepaymentDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 預收款金額
        /// </summary>
        [Required(ErrorMessage = "預收款金額為必填")]
        [Display(Name = "預收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrepaymentAmount { get; set; } = 0;

        /// <summary>
        /// 應收沖款單ID - 記錄來源於哪一筆沖款單
        /// </summary>
        [Required(ErrorMessage = "來源沖款單為必填")]
        [Display(Name = "來源沖款單")]
        [ForeignKey(nameof(AccountsReceivableSetoff))]
        public int SetoffId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 應收沖款單導航屬性
        /// </summary>
        public AccountsReceivableSetoff AccountsReceivableSetoff { get; set; } = null!;
    }
}