using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 報價單明細實體 - 記錄報價單的商品明細
    /// </summary>
    [Index(nameof(QuotationId), nameof(ProductId))]
    public class QuotationDetail : BaseEntity
    {
        [Required(ErrorMessage = "報價數量為必填")]
        [Display(Name = "報價數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; } = 0;

        [Required(ErrorMessage = "單價為必填")]
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Display(Name = "折扣")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount => Math.Round(Quantity * UnitPrice * (1 - DiscountPercentage / 100), 2);

        // Foreign Keys
        [Required(ErrorMessage = "報價單為必填")]
        [Display(Name = "報價單")]
        [ForeignKey(nameof(Quotation))]
        public int QuotationId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }

        // Navigation Properties
        public Quotation Quotation { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Unit? Unit { get; set; }
    }
}