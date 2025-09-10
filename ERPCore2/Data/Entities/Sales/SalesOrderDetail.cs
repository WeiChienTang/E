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

        [Display(Name = "折扣金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } = 0;

        [Display(Name = "已出貨數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal DeliveredQuantity { get; set; } = 0;

        [Display(Name = "待出貨數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal PendingQuantity { get; set; } = 0;

        [MaxLength(200, ErrorMessage = "明細備註不可超過200個字元")]
        [Display(Name = "明細備註")]
        public string? DetailRemarks { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "銷貨訂單為必填")]
        [Display(Name = "銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int SalesOrderId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        // Navigation Properties
        public SalesOrder SalesOrder { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Unit? Unit { get; set; }
    }
}
