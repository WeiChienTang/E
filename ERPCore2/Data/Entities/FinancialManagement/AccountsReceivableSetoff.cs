using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 應收帳款沖款單實體 - 記錄收款作業的主檔資料
    /// </summary>
    [Index(nameof(SetoffNumber))]
    [Index(nameof(CustomerId))]
    [Index(nameof(CompanyId))]
    [Index(nameof(SetoffDate))]
    public class AccountsReceivableSetoff : BaseEntity
    {
        /// <summary>
        /// 沖款單號
        /// </summary>
        [Required(ErrorMessage = "沖款單號為必填")]
        [MaxLength(50, ErrorMessage = "沖款單號不可超過50個字元")]
        [Display(Name = "沖款單號")]
        public string SetoffNumber { get; set; } = string.Empty;

        /// <summary>
        /// 沖款日期
        /// </summary>
        [Required(ErrorMessage = "沖款日期為必填")]
        [Display(Name = "沖款日期")]
        public DateTime SetoffDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 客戶ID
        /// </summary>
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        /// <summary>
        /// 總沖款金額
        /// </summary>
        [Display(Name = "總沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSetoffAmount { get; set; } = 0;

        /// <summary>
        /// 收款方式ID (可選)
        /// </summary>
        [Display(Name = "收款方式")]
        [ForeignKey(nameof(PaymentMethod))]
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// 收款帳戶/現金
        /// </summary>
        [MaxLength(100, ErrorMessage = "收款帳戶不可超過100個字元")]
        [Display(Name = "收款帳戶")]
        public string? PaymentAccount { get; set; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        [Display(Name = "是否已完成")]
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// 完成日期
        /// </summary>
        [Display(Name = "完成日期")]
        public DateTime? CompletedDate { get; set; }

        // Navigation Properties
        /// <summary>
        /// 客戶導航屬性
        /// </summary>
        public Customer Customer { get; set; } = null!;

        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// 收款方式導航屬性
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// 沖款明細導航屬性
        /// </summary>
        public ICollection<AccountsReceivableSetoffDetail> SetoffDetails { get; set; } = new List<AccountsReceivableSetoffDetail>();

        /// <summary>
        /// 付款明細導航屬性
        /// </summary>
        public ICollection<AccountsReceivableSetoffPaymentDetail> PaymentDetails { get; set; } = new List<AccountsReceivableSetoffPaymentDetail>();

        /// <summary>
        /// 預收款明細導航屬性
        /// </summary>
        public ICollection<PrepaymentDetail> PrepaymentDetails { get; set; } = new List<PrepaymentDetail>();
    }
}