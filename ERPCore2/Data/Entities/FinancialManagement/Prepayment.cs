using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預先款項實體 - 統一記錄預收款、預付款和其他預先款項
    /// </summary>
    [Index(nameof(Code))]
    [Index(nameof(PrepaymentType))]
    [Index(nameof(PaymentDate))]
    [Index(nameof(CustomerId))]
    [Index(nameof(SupplierId))]
    [Index(nameof(SetoffId))]
    public class Prepayment : BaseEntity
    {
        /// <summary>
        /// 款項類型（預收/預付/其他）
        /// </summary>
        [Required(ErrorMessage = "款項類型為必填")]
        [Display(Name = "款項類型")]
        public PrepaymentType PrepaymentType { get; set; }
        
        /// <summary>
        /// 款項日期
        /// </summary>
        [Required(ErrorMessage = "款項日期為必填")]
        [Display(Name = "款項日期")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        
        /// <summary>
        /// 款項金額
        /// </summary>
        [Required(ErrorMessage = "款項金額為必填")]
        [Display(Name = "款項金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;
        
        /// <summary>
        /// 客戶ID（預收款時使用）
        /// </summary>
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }
        
        /// <summary>
        /// 供應商ID（預付款時使用）
        /// </summary>
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int? SupplierId { get; set; }
        
        /// <summary>
        /// 應收沖款單ID（預收款時使用）
        /// </summary>
        [Display(Name = "來源沖款單")]
        [ForeignKey(nameof(AccountsReceivableSetoff))]
        public int? SetoffId { get; set; }
        
        // Navigation Properties
        /// <summary>
        /// 客戶導航屬性
        /// </summary>
        public Customer? Customer { get; set; }
        
        /// <summary>
        /// 供應商導航屬性
        /// </summary>
        public Supplier? Supplier { get; set; }
        
        /// <summary>
        /// 應收沖款單導航屬性
        /// </summary>
        public AccountsReceivableSetoff? AccountsReceivableSetoff { get; set; }
    }
}