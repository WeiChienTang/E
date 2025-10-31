using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 報價單主檔實體 - 記錄報價單基本資訊
    /// </summary>
    [Index(nameof(QuotationNumber), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(QuotationDate))]
    public class Quotation : BaseEntity
    {
        [Required(ErrorMessage = "報價單號為必填")]
        [MaxLength(30, ErrorMessage = "報價單號不可超過30個字元")]
        [Display(Name = "報價單號")]
        public string QuotationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "報價日期為必填")]
        [Display(Name = "報價日期")]
        public DateTime QuotationDate { get; set; } = DateTime.Today;

        [Display(Name = "有效期限")]
        public DateTime? ValidUntilDate { get; set; }

        [Display(Name = "報價總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "含稅總金額")]
        [NotMapped]
        public decimal TotalAmountWithTax => TotalAmount + TaxAmount;

        [Display(Name = "折扣金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "付款方式")]
        [MaxLength(200, ErrorMessage = "付款方式不可超過200個字元")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "交貨方式")]
        [MaxLength(200, ErrorMessage = "交貨方式不可超過200個字元")]
        public string? DeliveryTerms { get; set; }

        [Display(Name = "報價說明")]
        [MaxLength(500, ErrorMessage = "報價說明不可超過500個字元")]
        public string? Description { get; set; }

        [Display(Name = "是否已轉單")]
        public bool IsConverted { get; set; } = false;

        [Display(Name = "轉單日期")]
        public DateTime? ConvertedDate { get; set; }

        [Display(Name = "報價狀態")]
        public QuotationStatus QuotationStatus { get; set; } = QuotationStatus.Draft;

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
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Display(Name = "業務人員")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        [Display(Name = "轉換成銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int? ConvertedToSalesOrderId { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public Employee? Employee { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();
    }
}