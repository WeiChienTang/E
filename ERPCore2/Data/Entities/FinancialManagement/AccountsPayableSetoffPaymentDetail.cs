using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 應付帳款沖款付款明細 - 記錄每筆沖款使用的付款方式明細
    /// </summary>
    [Index(nameof(SetoffId))]
    [Index(nameof(PaymentMethodId))]
    [Index(nameof(BankId))]
    public class AccountsPayableSetoffPaymentDetail : BaseEntity
    {
        /// <summary>
        /// 沖款單ID
        /// </summary>
        [Required(ErrorMessage = "沖款單為必填")]
        [Display(Name = "沖款單")]
        [ForeignKey(nameof(Setoff))]
        public int SetoffId { get; set; }

        /// <summary>
        /// 付款方式ID
        /// </summary>
        [Required(ErrorMessage = "付款方式為必填")]
        [Display(Name = "付款方式")]
        [ForeignKey(nameof(PaymentMethod))]
        public int PaymentMethodId { get; set; }

        /// <summary>
        /// 銀行ID (選填,僅銀行相關付款方式需要)
        /// </summary>
        [Display(Name = "銀行")]
        [ForeignKey(nameof(Bank))]
        public int? BankId { get; set; }

        /// <summary>
        /// 付款金額
        /// </summary>
        [Required(ErrorMessage = "付款金額為必填")]
        [Display(Name = "付款金額")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "付款金額必須大於 0")]
        public decimal Amount { get; set; } = 0;

        /// <summary>
        /// 帳號/票號/參考號碼
        /// </summary>
        [MaxLength(100, ErrorMessage = "帳號不可超過100個字元")]
        [Display(Name = "帳號/票號")]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// 匯款單號/交易參考號
        /// </summary>
        [MaxLength(100, ErrorMessage = "交易參考號不可超過100個字元")]
        [Display(Name = "交易參考號")]
        public string? TransactionReference { get; set; }

        /// <summary>
        /// 付款日期 (可能與沖款日期不同,例如支票兌現日)
        /// </summary>
        [Display(Name = "付款日期")]
        public DateTime? PaymentDate { get; set; }

        // Navigation Properties
        /// <summary>
        /// 沖款單導航屬性
        /// </summary>
        public AccountsPayableSetoff Setoff { get; set; } = null!;

        /// <summary>
        /// 付款方式導航屬性
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = null!;

        /// <summary>
        /// 銀行導航屬性
        /// </summary>
        public Bank? Bank { get; set; }
    }
}
