using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 供應商定價表 - 管理從供應商的採購價格
    /// </summary>
    [Index(nameof(ProductId), nameof(SupplierId), nameof(EffectiveDate))]
    [Index(nameof(SupplierId), nameof(EffectiveDate))]
    public class SupplierPricing : BaseEntity
    {
        // 基本資訊
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        // 價格資訊
        [Required(ErrorMessage = "採購價格為必填")]
        [Display(Name = "採購價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [MaxLength(3, ErrorMessage = "貨幣編號不可超過3個字元")]
        [Display(Name = "貨幣")]
        public string Currency { get; set; } = "TWD";

        [MaxLength(50, ErrorMessage = "供應商商品編號不可超過50個字元")]
        [Display(Name = "供應商商品編號")]
        public string? SupplierProductCode { get; set; }

        // 採購條件
        [Display(Name = "最小訂購量")]
        public int? MinOrderQuantity { get; set; }

        [Display(Name = "交期天數")]
        public int? LeadTimeDays { get; set; }

        // 時效性
        [Required(ErrorMessage = "生效日期為必填")]
        [Display(Name = "生效日期")]
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        [Display(Name = "失效日期")]
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(200, ErrorMessage = "採購備註不可超過200個字元")]
        [Display(Name = "採購備註")]
        public string? PurchaseRemarks { get; set; }

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
    }
}
