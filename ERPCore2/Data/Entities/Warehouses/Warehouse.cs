using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 倉庫實體 - 定義倉庫基本資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Warehouse : BaseEntity
    {        
        [Required(ErrorMessage = "倉庫名稱為必填")]
        [MaxLength(50, ErrorMessage = "倉庫名稱不可超過50個字元")]
        [Display(Name = "倉庫名稱")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }
        
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(20, ErrorMessage = "聯絡電話不可超過20個字元")]
        [Display(Name = "聯絡電話")]
        public string? Phone { get; set; }
        
        // Navigation Properties
        public ICollection<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();
        public ICollection<InventoryStockDetail> InventoryStockDetails { get; set; } = new List<InventoryStockDetail>();
    }
}
