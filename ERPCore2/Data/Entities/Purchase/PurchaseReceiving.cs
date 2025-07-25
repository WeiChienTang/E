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

        [MaxLength(100, ErrorMessage = "驗收人員不可超過100個字元")]
        [Display(Name = "驗收人員")]
        public string? InspectionPersonnel { get; set; }

        [MaxLength(500, ErrorMessage = "進貨備註不可超過500個字元")]
        [Display(Name = "進貨備註")]
        public string? ReceiptRemarks { get; set; }

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
        [Required(ErrorMessage = "採購訂單為必填")]
        [Display(Name = "採購訂單")]
        [ForeignKey(nameof(PurchaseOrder))]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        // Navigation Properties
        public PurchaseOrder PurchaseOrder { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public Employee? ConfirmedByUser { get; set; }
        public ICollection<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; } = new List<PurchaseReceivingDetail>();
    }
}
