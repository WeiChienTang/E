using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨訂單主檔實體 - 記錄銷貨訂單基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(OrderDate))]
    [Index(nameof(QuotationId), nameof(OrderDate))]
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

        [Display(Name = "付款條件")]
        [MaxLength(200, ErrorMessage = "付款條件不可超過200個字元")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "交貨條件")]
        [MaxLength(200, ErrorMessage = "交貨條件不可超過200個字元")]
        public string? DeliveryTerms { get; set; }

        // Foreign Keys
        [Display(Name = "報價單")]
        [ForeignKey(nameof(Quotation))]
        public int? QuotationId { get; set; }  // 可選，支援多訂單模式

        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }  // 客戶直接關聯

        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        // Navigation Properties
        public Quotation? Quotation { get; set; }  // 報價單導覽屬性
        public Customer Customer { get; set; } = null!;
        public Employee? Employee { get; set; }
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
    }
}
