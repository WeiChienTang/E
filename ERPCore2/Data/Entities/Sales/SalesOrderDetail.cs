using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨訂單明細實體 - 記錄銷貨訂單的商品明細
    /// </summary>
    [Index(nameof(SalesOrderId), nameof(ProductId))]
    public class SalesOrderDetail : BaseEntity
    {
        [Required(ErrorMessage = "訂單數量為必填")]
        [Display(Name = "訂單數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal OrderQuantity { get; set; } = 0;

        [Required(ErrorMessage = "單價為必填")]
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Display(Name = "折扣")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount => Math.Round(OrderQuantity * UnitPrice * (1 - DiscountPercentage / 100), 2);

        [Required(ErrorMessage = "是否結清為必填")]
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; } = false;

        [Display(Name = "累計收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceivedAmount { get; set; } = 0;

        // Foreign Keys
        [Required(ErrorMessage = "銷貨訂單為必填")]
        [Display(Name = "銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int SalesOrderId { get; set; }

        [Display(Name = "來源報價單明細")]
        [ForeignKey(nameof(QuotationDetail))]
        public int? QuotationDetailId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }

        // Navigation Properties
        public SalesOrder SalesOrder { get; set; } = null!;
        public QuotationDetail? QuotationDetail { get; set; }
        public Product Product { get; set; } = null!;
        public Unit? Unit { get; set; }
        public Warehouse? Warehouse { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
