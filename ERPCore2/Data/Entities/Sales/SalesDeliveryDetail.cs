using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨/出貨單明細實體 - 記錄實際出貨的商品明細
    /// 職責: 記錄出貨商品、數量、價格,並關聯來源訂單明細
    /// </summary>
    [Index(nameof(SalesDeliveryId), nameof(ProductId))]
    [Index(nameof(SalesOrderDetailId))]
    public class SalesDeliveryDetail : BaseEntity
    {
        [Required(ErrorMessage = "出貨數量為必填")]
        [Display(Name = "出貨數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal DeliveryQuantity { get; set; } = 0;

        [Required(ErrorMessage = "單價為必填")]
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Display(Name = "折扣")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount => Math.Round(DeliveryQuantity * UnitPrice * (1 - DiscountPercentage / 100), 2);

        [Required(ErrorMessage = "是否結清為必填")]
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; } = false;

        [Display(Name = "累計收款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceivedAmount { get; set; } = 0;

        // Foreign Keys
        [Required(ErrorMessage = "銷貨單為必填")]
        [Display(Name = "銷貨單")]
        [ForeignKey(nameof(SalesDelivery))]
        public int SalesDeliveryId { get; set; }

        [Display(Name = "來源訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int? SalesOrderDetailId { get; set; }

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
        public SalesDelivery SalesDelivery { get; set; } = null!;
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        public Product Product { get; set; } = null!;
        public Unit? Unit { get; set; }
        public Warehouse? Warehouse { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
        public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; } = new List<SalesReturnDetail>();
    }
}
