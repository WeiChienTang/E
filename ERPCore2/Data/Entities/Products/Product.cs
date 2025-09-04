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

        [Display(Name = "最低庫存警戒線")]
        public int MinStockLevel { get; set; } = 0;
        
        [Display(Name = "最高庫存警戒線")]
        public int MaxStockLevel { get; set; } = 0;

        // Foreign Keys
        [Display(Name = "單位")]
        [ForeignKey(nameof(Unit))]
        public int? UnitId { get; set; }
        
        [Display(Name = "尺寸")]
        [ForeignKey(nameof(Size))]
        public int? SizeId { get; set; }
        
        [Display(Name = "商品分類")]
        [ForeignKey(nameof(ProductCategory))]
        public int? ProductCategoryId { get; set; }
        
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }
        
        [Display(Name = "倉庫位置")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        // Navigation Properties
        public Unit? Unit { get; set; }
        public Size? Size { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        public Warehouse? Warehouse { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    }
}