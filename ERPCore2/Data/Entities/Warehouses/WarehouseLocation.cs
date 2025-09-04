using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫位實體 - 定義倉庫內具體存放位置
    /// </summary>
    [Index(nameof(WarehouseId), nameof(Code), IsUnique = true)]
    public class WarehouseLocation : BaseEntity
    {        
        [Required(ErrorMessage = "庫位名稱為必填")]
        [MaxLength(50, ErrorMessage = "庫位名稱不可超過50個字元")]
        [Display(Name = "庫位名稱")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(10, ErrorMessage = "區域不可超過10個字元")]
        [Display(Name = "區域")]
        public string? Zone { get; set; }
        
        [MaxLength(10, ErrorMessage = "排號不可超過10個字元")]
        [Display(Name = "排號")]
        public string? Aisle { get; set; }
        
        [MaxLength(10, ErrorMessage = "層號不可超過10個字元")]
        [Display(Name = "層號")]
        public string? Level { get; set; }
        
        [MaxLength(10, ErrorMessage = "位號不可超過10個字元")]
        [Display(Name = "位號")]
        public string? Position { get; set; }
        
        [Display(Name = "最大容量")]
        public int? MaxCapacity { get; set; }
        
        // Foreign Keys
        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        
        // Navigation Properties
        public Warehouse Warehouse { get; set; } = null!;
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    }
}