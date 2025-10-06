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
    public class PurchaseReceiving : BaseEntity
    {
        [Required(ErrorMessage = "進貨單號為必填")]
        [MaxLength(30, ErrorMessage = "進貨單號不可超過30個字元")]
        [Display(Name = "進貨單號")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "進貨日期為必填")]
        [Display(Name = "進貨日期")]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Display(Name = "進貨總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "進貨稅額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseReceivingTaxAmount { get; set; } = 0;

        [Display(Name = "進貨含稅總金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseReceivingTotalAmountIncludingTax => TotalAmount + PurchaseReceivingTaxAmount;

        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }

        // Foreign Keys
        [Display(Name = "採購訂單")]
        [ForeignKey(nameof(PurchaseOrder))]
        public int? PurchaseOrderId { get; set; }  // 改為可選，支援多採購單模式

        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }  // 新增供應商直接關聯

        // Navigation Properties
        public PurchaseOrder? PurchaseOrder { get; set; }  // 改為可選
        public Supplier Supplier { get; set; } = null!;  // 新增供應商導覽屬性
        public ICollection<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; } = new List<PurchaseReceivingDetail>();
    }
}
