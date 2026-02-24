using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

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
        [Required(ErrorMessage = "商品單位為必填")]
        [Display(Name = "採購單位")]
        [ForeignKey(nameof(Unit))]
        public int UnitId { get; set; }

        [Display(Name = "製程單位")]
        [ForeignKey(nameof(ProductionUnit))]
        public int? ProductionUnitId { get; set; }

        /// <summary>
        /// 製程單位換算率：1 基本單位（商品單位）= N 製程單位
        /// 例如：基本單位為「包」、製程單位為「公斤」，1 包 = 30 公斤，則此值為 30
        /// </summary>
        [Display(Name = "製程單位換算率")]
        [Column(TypeName = "decimal(18,6)")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "換算率必須大於 0")]
        public decimal? ProductionUnitConversionRate { get; set; }

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

        [Display(Name = "標準成本")]
        [Column(TypeName = "decimal(18,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "標準成本必須大於或等於 0")]
        public decimal? StandardCost { get; set; }

        // Navigation Properties
        public Unit? Unit { get; set; }
        public Unit? ProductionUnit { get; set; }
        public Size? Size { get; set; }
        public ProductCategory? ProductCategory { get; set; }
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
