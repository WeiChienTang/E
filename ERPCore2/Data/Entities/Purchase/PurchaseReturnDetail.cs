using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 採購退回明細實體 - 記錄採購退回的商品明細
    /// </summary>
    [Index(nameof(PurchaseReturnId), nameof(ProductId))]
    [Index(nameof(ProductId))]
    public class PurchaseReturnDetail : BaseEntity
    {
        [Required(ErrorMessage = "退回數量為必填")]
        [Display(Name = "退回數量")]
        public int ReturnQuantity { get; set; } = 0;

        [Required(ErrorMessage = "原始單價為必填")]
        [Display(Name = "原始單價")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal OriginalUnitPrice { get; set; } = 0;

        [Display(Name = "退回小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnSubtotalAmount => ReturnQuantity * OriginalUnitPrice;

        [Display(Name = "批號")]
        [MaxLength(50, ErrorMessage = "批號不可超過50個字元")]
        public string? BatchNumber { get; set; }

        [Required(ErrorMessage = "是否結清為必填")]
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; } = false;
        
        [Display(Name = "累計收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceivedAmount { get; set; } = 0;

        // Foreign Keys
        [Required(ErrorMessage = "採購退回為必填")]
        [Display(Name = "採購退回")]
        [ForeignKey(nameof(PurchaseReturn))]
        public int PurchaseReturnId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "原始採購訂單明細")]
        [ForeignKey(nameof(PurchaseOrderDetail))]
        public int? PurchaseOrderDetailId { get; set; }

        [Display(Name = "原始採購進貨明細")]
        [ForeignKey(nameof(PurchaseReceivingDetail))]
        public int? PurchaseReceivingDetailId { get; set; }

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }


        // Navigation Properties
        public PurchaseReturn PurchaseReturn { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public PurchaseOrderDetail? PurchaseOrderDetail { get; set; }
        public PurchaseReceivingDetail? PurchaseReceivingDetail { get; set; }
        public Unit? Unit { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
