using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品實體 - 定義商品基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Product : BaseEntity
    {        
        [Required(ErrorMessage = "商品名稱為必填")]
        [MaxLength(100, ErrorMessage = "商品名稱不可超過100個字元")]
        [Display(Name = "商品名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "條碼編號不可超過50個字元")]
        [Display(Name = "條碼編號")]
        public string? Barcode { get; set; }

        // Foreign Keys
        [Display(Name = "基本單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }
        
        [Display(Name = "採購單位")]
        [ForeignKey(nameof(PurchaseUnit))]
        public int? PurchaseUnitId { get; set; }
        
        [Display(Name = "製程單位")]
        [ForeignKey(nameof(ProductionUnit))]
        public int? ProductionUnitId { get; set; }
        
        [Display(Name = "尺寸")]
        [ForeignKey(nameof(Size))]
        public int? SizeId { get; set; }
        
        [Display(Name = "商品分類")]
        [ForeignKey(nameof(ProductCategory))]
        public int? ProductCategoryId { get; set; }

        [Display(Name = "規格說明")]
        [MaxLength(100, ErrorMessage = "規格說明不可超過100個字元")]
        public string? Specification { get; set; }
        
        [Display(Name = "稅率")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "稅率必須介於0到100之間")]
        public decimal? TaxRate { get; set; }
        
        /// <summary>
        /// 供應商 ID(直接關聯,不使用中間表)
        /// </summary>
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int? SupplierId { get; set; }
        
        /// <summary>
        /// 採購/製造類型 - 決定商品的取得方式
        /// </summary>
        [Display(Name = "採購類型")]
        public ProcurementType ProcurementType { get; set; } = ProcurementType.Purchased;
        
        // Navigation Properties
        public Unit? Unit { get; set; }
        public Unit? PurchaseUnit { get; set; }
        public Unit? ProductionUnit { get; set; }
        public Size? Size { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
        
        // Product Composition (BOM) Navigation Properties
        /// <summary>
        /// 此商品作為成品的所有 BOM 配方
        /// </summary>
        public ICollection<ProductComposition> ProductCompositions { get; set; } = new List<ProductComposition>();
        
        /// <summary>
        /// 此商品作為組件被使用在哪些 BOM 中
        /// </summary>
        public ICollection<ProductCompositionDetail> ComponentInCompositions { get; set; } = new List<ProductCompositionDetail>();
        
        /// <summary>
        /// 供應商關聯列表（商品-供應商綁定）
        /// </summary>
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    }
}