using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Helpers.EditModal;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨退回主檔實體 - 記錄銷貨退回基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(ReturnDate))]
    [Index(nameof(SalesOrderId), nameof(ReturnDate))]
    [CodeGenerationStrategy(
        CodeGenerationStrategy.TimestampWithSequence,
        Prefix = "SR",
        DateFieldName = nameof(ReturnDate),
        SequenceDigits = 4
    )]
    public class SalesReturn : BaseEntity
    {
        [Required(ErrorMessage = "退回日期為必填")]
        [Display(Name = "退回日期")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Display(Name = "退回總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmount { get; set; } = 0;

        [Display(Name = "退回稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnTaxAmount { get; set; } = 0;

        [Display(Name = "退回含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReturnAmountWithTax => TotalReturnAmount + ReturnTaxAmount;

        [Display(Name = "是否已退款")]
        public bool IsRefunded { get; set; } = false;

        [Display(Name = "退款日期")]
        public DateTime? RefundDate { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "客戶為必填")]
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Display(Name = "原始銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int? SalesOrderId { get; set; }

        [Display(Name = "原始銷貨/出貨單")]
        [ForeignKey(nameof(SalesDelivery))]
        public int? SalesDeliveryId { get; set; }

        [Display(Name = "處理員工")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        [Display(Name = "退回原因")]
        [ForeignKey(nameof(ReturnReason))]
        public int? ReturnReasonId { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public SalesOrder? SalesOrder { get; set; }
        public SalesDelivery? SalesDelivery { get; set; }
        public Employee? Employee { get; set; }
        public EntitySalesReturnReason? ReturnReason { get; set; }
        public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; } = new List<SalesReturnDetail>();
    }
}
