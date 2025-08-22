using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購訂單主檔實體 - 記錄採購訂單基本資訊
    /// </summary>
    [Index(nameof(PurchaseOrderNumber), IsUnique = true)]
    [Index(nameof(SupplierId), nameof(OrderDate))]
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

        [Display(Name = "是否核准")]
        public bool IsApproved { get; set; } = false;

        [MaxLength(100, ErrorMessage = "駁回原因不可超過100個字元")]
        [Display(Name = "駁回原因")]
        public string? RejectReason { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "採購公司為必填")]
        [Display(Name = "採購公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        // Navigation Properties
        public Supplier Supplier { get; set; } = null!;
        public Company Company { get; set; } = null!;
        public Warehouse? Warehouse { get; set; }
        public Employee? ApprovedByUser { get; set; }
        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
        public ICollection<PurchaseReceiving> PurchaseReceivings { get; set; } = new List<PurchaseReceiving>();
    }
}
