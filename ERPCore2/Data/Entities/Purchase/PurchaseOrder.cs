using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購訂單主檔實體 - 記錄採購訂單基本資訊
    /// </summary>
    [Index(nameof(PurchaseOrderNumber), IsUnique = true)]
    [Index(nameof(SupplierId), nameof(OrderDate))]
    [Index(nameof(OrderStatus), nameof(OrderDate))]
    public class PurchaseOrder : BaseEntity
    {
        [Required(ErrorMessage = "採購單號為必填")]
        [MaxLength(30, ErrorMessage = "採購單號不可超過30個字元")]
        [Display(Name = "採購單號")]
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "訂單日期為必填")]
        [Display(Name = "訂單日期")]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "預計到貨日期")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required(ErrorMessage = "訂單狀態為必填")]
        [Display(Name = "訂單狀態")]
        public PurchaseOrderStatus OrderStatus { get; set; } = PurchaseOrderStatus.Draft;

        [Required(ErrorMessage = "採購類型為必填")]
        [Display(Name = "採購類型")]
        public PurchaseType PurchaseType { get; set; } = PurchaseType.Normal;

        [MaxLength(100, ErrorMessage = "採購人員不可超過100個字元")]
        [Display(Name = "採購人員")]
        public string? PurchasePersonnel { get; set; }

        [MaxLength(500, ErrorMessage = "訂單備註不可超過500個字元")]
        [Display(Name = "訂單備註")]
        public string? OrderRemarks { get; set; }

        [Display(Name = "訂單總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "已進貨金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedAmount { get; set; } = 0;

        [Display(Name = "核准人員")]
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedBy { get; set; }

        [Display(Name = "核准時間")]
        public DateTime? ApprovedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public Warehouse? Warehouse { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
        public ICollection<PurchaseReceiving> PurchaseReceivings { get; set; } = new List<PurchaseReceiving>();
    }
}
