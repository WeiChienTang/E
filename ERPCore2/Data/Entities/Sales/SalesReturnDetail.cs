using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨退回明細實體 - 記錄銷貨退回的商品明細
    /// </summary>
    [Index(nameof(SalesReturnId), nameof(ProductId))]
    public class SalesReturnDetail : BaseEntity
    {
        [Required(ErrorMessage = "退回數量為必填")]
        [Display(Name = "退回數量")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal ReturnQuantity { get; set; } = 0;

        [Required(ErrorMessage = "原始單價為必填")]
        [Display(Name = "原始單價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalUnitPrice { get; set; } = 0;

        [Display(Name = "折扣")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "稅率(%)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "稅率必須介於 0 到 100 之間")]
        public decimal? TaxRate { get; set; }

        [Display(Name = "退回小計")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnSubtotalAmount => Math.Round(ReturnQuantity * OriginalUnitPrice * (1 - DiscountPercentage / 100), 2);

        [Required(ErrorMessage = "是否結清為必填")]
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; } = false;

        [Display(Name = "累計付款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaidAmount { get; set; } = 0;

        // Foreign Keys
        [Required(ErrorMessage = "銷貨退回為必填")]
        [Display(Name = "銷貨退回")]
        [ForeignKey(nameof(SalesReturn))]
        public int SalesReturnId { get; set; }

        [Required(ErrorMessage = "產品為必填")]
        [Display(Name = "產品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "原始銷貨訂單明細")]
        [ForeignKey(nameof(SalesOrderDetail))]
        public int? SalesOrderDetailId { get; set; }

        [Display(Name = "原始銷貨/出貨明細")]
        [ForeignKey(nameof(SalesDeliveryDetail))]
        public int? SalesDeliveryDetailId { get; set; }


        // Navigation Properties
        public SalesReturn SalesReturn { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        public SalesDeliveryDetail? SalesDeliveryDetail { get; set; }
    }
}
