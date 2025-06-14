using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 單位實體 - 定義商品計量單位
    /// </summary>
    [Index(nameof(UnitCode), IsUnique = true)]
    public class Unit : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "單位代碼為必填")]
        [MaxLength(10, ErrorMessage = "單位代碼不可超過10個字元")]
        [Display(Name = "單位代碼")]
        public string UnitCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "單位名稱為必填")]
        [MaxLength(20, ErrorMessage = "單位名稱不可超過20個字元")]
        [Display(Name = "單位名稱")]
        public string UnitName { get; set; } = string.Empty;    
        
        [MaxLength(5, ErrorMessage = "符號不可超過5個字元")]
        [Display(Name = "單位符號")]
        public string? Symbol { get; set; }
        
        [Display(Name = "是否為基本單位")]
        public bool IsBaseUnit { get; set; } = false;
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<UnitConversion> FromUnitConversions { get; set; } = new List<UnitConversion>();
        public ICollection<UnitConversion> ToUnitConversions { get; set; } = new List<UnitConversion>();
    }
}