using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 沖款收款記錄 - 管理收款、折讓等財務資訊
    /// </summary>
    public class SetoffPayment : BaseEntity
    {
        /// <summary>
        /// 沖款單ID
        /// </summary>
        [Required(ErrorMessage = "沖款單為必填")]
        public int SetoffDocumentId { get; set; }
        
        /// <summary>
        /// 沖款單導航屬性
        /// </summary>
        public SetoffDocument SetoffDocument { get; set; } = null!;

        /// <summary>
        /// 銀行別
        /// </summary>
        public int? BankId { get; set; }
        
        /// <summary>
        /// 銀行導航屬性
        /// </summary>
        public Bank? Bank { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public int? PaymentMethodId { get; set; }
        
        /// <summary>
        /// 付款方式導航屬性
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// 收款金額
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "收款金額")]
        public decimal ReceivedAmount { get; set; } = 0;

        /// <summary>
        /// 折讓金額
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "折讓金額")]
        public decimal AllowanceAmount { get; set; } = 0;

        /// <summary>
        /// 支票號碼
        /// </summary>
        [MaxLength(50, ErrorMessage = "支票號碼不可超過50個字元")]
        [Display(Name = "支票號碼")]
        public string? CheckNumber { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>
        [Display(Name = "到期日")]
        public DateTime? DueDate { get; set; }
    }
}
