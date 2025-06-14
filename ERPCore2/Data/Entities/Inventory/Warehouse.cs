using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 倉庫實體 - 定義倉庫基本資訊
    /// </summary>
    [Index(nameof(WarehouseCode), IsUnique = true)]
    public class Warehouse : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "倉庫代碼為必填")]
        [MaxLength(20, ErrorMessage = "倉庫代碼不可超過20個字元")]
        [Display(Name = "倉庫代碼")]
        public string WarehouseCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "倉庫名稱為必填")]
        [MaxLength(50, ErrorMessage = "倉庫名稱不可超過50個字元")]
        [Display(Name = "倉庫名稱")]
        public string WarehouseName { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "地址不可超過200個字元")]
        [Display(Name = "地址")]
        public string? Address { get; set; }
        
        [MaxLength(50, ErrorMessage = "聯絡人不可超過50個字元")]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; }
        
        [MaxLength(20, ErrorMessage = "聯絡電話不可超過20個字元")]
        [Display(Name = "聯絡電話")]
        public string? Phone { get; set; }
        
        [Required(ErrorMessage = "倉庫類型為必填")]
        [Display(Name = "倉庫類型")]
        public WarehouseTypeEnum WarehouseType { get; set; } = WarehouseTypeEnum.Main;
        
        [Display(Name = "是否為預設倉庫")]
        public bool IsDefault { get; set; } = false;
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();
    }
}