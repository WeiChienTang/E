using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 沖款預收/預付款明細 - 記錄沖款單使用預收/預付款的明細
    /// 支援多對多關係：一筆預收款可分次用於多張沖款單，一張沖款單也可動用多個預收/預付款
    /// </summary>
    [Index(nameof(AccountsReceivableSetoffId))]
    [Index(nameof(AccountsPayableSetoffId))]
    [Index(nameof(PrepaymentId))]
    public class PrepaymentDetail : BaseEntity
    {
        /// <summary>
        /// 應收帳款沖款單ID
        /// </summary>
        [Display(Name = "應收帳款沖款單")]
        [ForeignKey(nameof(AccountsReceivableSetoff))]
        public int? AccountsReceivableSetoffId { get; set; }
        
        /// <summary>
        /// 應付帳款沖款單ID
        /// </summary>
        [Display(Name = "應付帳款沖款單")]
        [ForeignKey(nameof(AccountsPayableSetoff))]
        public int? AccountsPayableSetoffId { get; set; }
        
        /// <summary>
        /// 預收/預付款ID
        /// </summary>
        [Required(ErrorMessage = "預收/預付款為必填")]
        [Display(Name = "預收/預付款")]
        [ForeignKey(nameof(Prepayment))]
        public int PrepaymentId { get; set; }
        
        /// <summary>
        /// 本次使用金額
        /// </summary>
        [Required(ErrorMessage = "使用金額為必填")]
        [Display(Name = "使用金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UseAmount { get; set; } = 0;
        
        // Navigation Properties
        
        /// <summary>
        /// 應收帳款沖款單導航屬性
        /// </summary>
        public AccountsReceivableSetoff? AccountsReceivableSetoff { get; set; }
        
        /// <summary>
        /// 應付帳款沖款單導航屬性
        /// </summary>
        public AccountsPayableSetoff? AccountsPayableSetoff { get; set; }
        
        /// <summary>
        /// 預收/預付款導航屬性
        /// </summary>
        public Prepayment? Prepayment { get; set; }
    }
}
