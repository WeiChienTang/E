using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購進貨單主檔實體 - 記錄採購進貨作業
    /// </summary>
    [Index(nameof(ReceiptNumber), IsUnique = true)]
    [Index(nameof(PurchaseOrderId), nameof(ReceiptDate))]
    [Index(nameof(ReceiptStatus), nameof(ReceiptDate))]
    public class PurchaseReceiving : BaseEntity
    {
        [Required(ErrorMessage = "進貨單號為必填")]
        [MaxLength(30, ErrorMessage = "進貨單號不可超過30個字元")]
        [Display(Name = "進貨單號")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "進貨日期為必填")]
        [Display(Name = "進貨日期")]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "進貨狀態為必填")]
        [Display(Name = "進貨狀態")]
        public PurchaseReceivingStatus ReceiptStatus { get; set; } = PurchaseReceivingStatus.Draft;

        [Display(Name = "進貨總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "確認時間")]
        public DateTime? ConfirmedAt { get; set; }

        [Display(Name = "確認人員")]
        [ForeignKey(nameof(ConfirmedByUser))]
        public int? ConfirmedBy { get; set; }

        // Foreign Keys
        [Display(Name = "採購訂單")]
        [ForeignKey(nameof(PurchaseOrder))]
        public int? PurchaseOrderId { get; set; }  // 改為可選，支援多採購單模式

        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }  // 新增供應商直接關聯

        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        // Navigation Properties
        public PurchaseOrder? PurchaseOrder { get; set; }  // 改為可選
        public Supplier Supplier { get; set; } = null!;  // 新增供應商導覽屬性
        public Warehouse Warehouse { get; set; } = null!;
        public Employee? ConfirmedByUser { get; set; }
        public ICollection<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; } = new List<PurchaseReceivingDetail>();
    }
}
