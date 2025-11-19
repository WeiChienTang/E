using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 報價單主檔實體 - 記錄報價單基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(QuotationDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "QT",
        DateFieldName = nameof(QuotationDate),
        SequenceDigits = 4
    )]
    public class Quotation : BaseEntity
    {
        [Required(ErrorMessage = "報價日期為必填")]
        [Display(Name = "報價日期")]
        public DateTime QuotationDate { get; set; } = DateTime.Today;

        [Display(Name = "報價總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "折扣金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "付款方式")]
        [MaxLength(200, ErrorMessage = "付款方式不可超過200個字元")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "交貨方式")]
        [MaxLength(200, ErrorMessage = "交貨方式不可超過200個字元")]
        public string? DeliveryTerms { get; set; }

        [Display(Name = "工程名稱")]
        [MaxLength(200, ErrorMessage = "工程名稱不可超過200個字元")]
        public string? ProjectName { get; set; }

        [Display(Name = "核准人員")]
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedBy { get; set; }

        [Display(Name = "核准時間")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "是否核准")]
        public bool IsApproved { get; set; } = false;

        [MaxLength(200, ErrorMessage = "駁回原因不可超過200個字元")]
        [Display(Name = "駁回原因")]
        public string? RejectReason { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Display(Name = "業務人員")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        // Navigation Properties
        public Company Company { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public Employee? Employee { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();
        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}