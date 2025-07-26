using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購進貨明細實體 - 記錄採購進貨商品明細
    /// </summary>
    [Index(nameof(PurchaseReceivingId), nameof(ProductId))]
    [Index(nameof(PurchaseOrderDetailId))]
    [Index(nameof(ProductId))]
    public class PurchaseReceivingDetail : BaseEntity
    {
        [Required(ErrorMessage = "採購進貨單為必填")]
        [Display(Name = "採購進貨單")]
        [ForeignKey(nameof(PurchaseReceiving))]
        public int PurchaseReceivingId { get; set; }

        [Required(ErrorMessage = "採購訂單明細為必填")]
        [Display(Name = "採購訂單明細")]
        [ForeignKey(nameof(PurchaseOrderDetail))]
        public int PurchaseOrderDetailId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "進貨數量為必填")]
        [Display(Name = "進貨數量")]
        public int ReceivedQuantity { get; set; }

        [Required(ErrorMessage = "單價為必填")]
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "小計金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount => ReceivedQuantity * UnitPrice;

        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }

        [MaxLength(200, ErrorMessage = "驗收備註不可超過200個字元")]
        [Display(Name = "驗收備註")]
        public string? InspectionRemarks { get; set; }

        [Display(Name = "品質檢驗結果")]
        public bool? QualityInspectionPassed { get; set; }

        [MaxLength(200, ErrorMessage = "品質備註不可超過200個字元")]
        [Display(Name = "品質備註")]
        public string? QualityRemarks { get; set; }

        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }

        [Display(Name = "到期日期")]
        public DateTime? ExpiryDate { get; set; }

        // Navigation Properties
        public PurchaseReceiving PurchaseReceiving { get; set; } = null!;
        public PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
