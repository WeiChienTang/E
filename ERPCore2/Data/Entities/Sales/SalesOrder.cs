using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨訂單主檔實體 - 記錄銷貨訂單基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(OrderDate))]
    [Index(nameof(CompanyId), nameof(OrderDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "SO",
        DateFieldName = nameof(OrderDate),
        SequenceDigits = 4
    )]
    public class SalesOrder : BaseEntity
    {
        [Required(ErrorMessage = "訂單日期為必填")]
        [Display(Name = "訂單日期")]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "預計交貨日期")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Display(Name = "訂單總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "銷貨稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesTaxAmount { get; set; } = 0;

        [Display(Name = "含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmountWithTax => TotalAmount + SalesTaxAmount;

        [Display(Name = "折扣金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required(ErrorMessage = "稅別為必填")]
        [Display(Name = "稅別")]
        public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;

        [Display(Name = "付款條件")]
        [MaxLength(200, ErrorMessage = "付款條件不可超過200個字元")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "交貨條件")]
        [MaxLength(200, ErrorMessage = "交貨條件不可超過200個字元")]
        public string? DeliveryTerms { get; set; }

        // ===== 審核欄位 =====
        [Display(Name = "是否核准")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "核准人員")]
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedBy { get; set; }

        [Display(Name = "核准時間")]
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100, ErrorMessage = "駁回原因不可超過100個字元")]
        [Display(Name = "駁回原因")]
        public string? RejectReason { get; set; }

        [NotMapped]
        public string ApprovalStatusText =>
            IsApproved ? "已核准" :
            !string.IsNullOrEmpty(RejectReason) ? "已駁回" : "待審核";

        [NotMapped]
        public string? ApprovedAtText => ApprovedAt?.ToString("yyyy-MM-dd HH:mm");

        [NotMapped]
        public string ApprovedByDisplayName =>
            IsApproved ? (ApprovedByUser?.Name ?? "系統自動審核") :
            !string.IsNullOrEmpty(RejectReason) ? (ApprovedByUser?.Name ?? "") : "";

        // Foreign Keys
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int? CompanyId { get; set; }

        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }  // 客戶直接關聯

        [Display(Name = "製表者")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        [Display(Name = "業務人員")]
        [ForeignKey(nameof(Salesperson))]
        public int? SalespersonId { get; set; }

        // Navigation Properties
        public Company? Company { get; set; }
        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
        public Employee? Salesperson { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
    }
}
