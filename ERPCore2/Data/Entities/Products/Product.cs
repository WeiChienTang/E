using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品實體 - 定義商品基本資訊
    /// </summary>
    [Index(nameof(ProductCode), IsUnique = true)]
    public class Product : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "商品代碼為必填")]
        [MaxLength(30, ErrorMessage = "商品代碼不可超過30個字元")]
        [Display(Name = "商品代碼")]
        public string ProductCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "商品名稱為必填")]
        [MaxLength(100, ErrorMessage = "商品名稱不可超過100個字元")]
        [Display(Name = "商品名稱")]
        public string ProductName { get; set; } = string.Empty;
        
        // Optional Properties
        [MaxLength(500, ErrorMessage = "商品描述不可超過500個字元")]
        [Display(Name = "商品描述")]
        public string? Description { get; set; }
        
        [MaxLength(200, ErrorMessage = "規格說明不可超過200個字元")]
        [Display(Name = "規格說明")]
        public string? Specification { get; set; }

        // Foreign Keys
        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }
        
        [Display(Name = "尺寸")]
        [ForeignKey(nameof(Size))]
        public int? SizeId { get; set; }
        
        [Display(Name = "單價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }
        
        [Display(Name = "成本價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        // Foreign Keys
        [Display(Name = "商品分類")]
        [ForeignKey(nameof(ProductCategory))]
        public int? ProductCategoryId { get; set; }
        
        [Display(Name = "主要供應商")]
        [ForeignKey(nameof(PrimarySupplier))]
        public int? PrimarySupplierId { get; set; }
        
        // Navigation Properties
        public Unit? Unit { get; set; }
        public Size? Size { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        public Supplier? PrimarySupplier { get; set; }
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    }
}