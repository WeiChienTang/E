using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨出貨明細實體 - 記錄銷貨出貨的商品明細
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

        [Display(Name = "小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } = 0;

        [MaxLength(200, ErrorMessage = "明細備註不可超過200個字元")]
        [Display(Name = "明細備註")]
        public string? DetailRemarks { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "銷貨出貨為必填")]
        [Display(Name = "銷貨出貨")]
        [ForeignKey(nameof(SalesDelivery))]
        public int SalesDeliveryId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "銷貨訂單明細為必填")]
        [Display(Name = "銷貨訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int SalesOrderDetailId { get; set; }

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        // Navigation Properties
        public SalesDelivery SalesDelivery { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public SalesOrderDetail SalesOrderDetail { get; set; } = null!;
        public Unit? Unit { get; set; }
    }
}
