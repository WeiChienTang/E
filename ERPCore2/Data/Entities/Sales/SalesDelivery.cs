using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨/出貨單主檔實體 - 記錄實際出貨資訊
    /// 流程: 報價單 → 銷售訂單 → 出貨單 (本檔) → 銷貨退回
    /// 職責: 管理實際出貨、影響庫存減少、產生應收帳款
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CustomerId), nameof(DeliveryDate))]
    [Index(nameof(SalesOrderId), nameof(DeliveryDate))]
    public class SalesDelivery : BaseEntity
    {
        [Required(ErrorMessage = "出貨日期為必填")]
        [Display(Name = "出貨日期")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [Display(Name = "預計送達日期")]
        public DateTime? ExpectedArrivalDate { get; set; }

        [Display(Name = "實際送達日期")]
        public DateTime? ActualArrivalDate { get; set; }

        [Display(Name = "出貨總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "銷貨稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmountWithTax => TotalAmount + TaxAmount;

        [Display(Name = "折扣金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "付款條件")]
        [MaxLength(200, ErrorMessage = "付款條件不可超過200個字元")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "運送方式")]
        [MaxLength(200, ErrorMessage = "運送方式不可超過200個字元")]
        public string? ShippingMethod { get; set; }

        [Display(Name = "送貨地址")]
        [MaxLength(500, ErrorMessage = "送貨地址不可超過500個字元")]
        public string? DeliveryAddress { get; set; }

        [Display(Name = "是否已出貨")]
        public bool IsShipped { get; set; } = false;

        [Display(Name = "實際出貨日期")]
        public DateTime? ActualShipDate { get; set; }

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

        [Display(Name = "來源銷售訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int? SalesOrderId { get; set; }

        [Display(Name = "業務人員")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        [Display(Name = "出貨倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; } = null!;
        public SalesOrder? SalesOrder { get; set; }
        public Employee? Employee { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public Warehouse? Warehouse { get; set; }
        public ICollection<SalesDeliveryDetail> DeliveryDetails { get; set; } = new List<SalesDeliveryDetail>();
        public ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
    }
}
