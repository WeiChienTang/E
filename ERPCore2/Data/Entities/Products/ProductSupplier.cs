using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品供應商關聯實體 - 多對多關聯表，定義商品與供應商的關係
    /// </summary>
    public class ProductSupplier : BaseEntity
    {
        // Foreign Keys
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        
        // Optional Properties
        [MaxLength(50, ErrorMessage = "供應商商品代碼不可超過50個字元")]
        [Display(Name = "供應商商品代碼")]
        public string? SupplierProductCode { get; set; }
        
        [Display(Name = "供應商報價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SupplierPrice { get; set; }
        
        [Display(Name = "交期天數")]
        public int? LeadTime { get; set; }
        
        [Display(Name = "最小訂購量")]
        public int? MinOrderQuantity { get; set; }
        
        [Display(Name = "是否為主要供應商")]
        public bool IsPrimarySupplier { get; set; } = false;
        
        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
    }
}