using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 單位實體 - 定義商品計量單位
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Unit : BaseEntity
    {
        
        [Required(ErrorMessage = "單位名稱為必填")]
        [MaxLength(20, ErrorMessage = "單位名稱不可超過20個字元")]
        [Display(Name = "單位名稱")]
        public string UnitName { get; set; } = string.Empty;    
        
        // 保留但不使用
        [MaxLength(5, ErrorMessage = "符號不可超過5個字元")]
        [Display(Name = "單位符號")]
        public string? Symbol { get; set; }
        
        // 保留但不使用
        [Display(Name = "是否為基本單位")]
        public bool IsBaseUnit { get; set; } = true;
        
        // Navigation Properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<UnitConversion> FromUnitConversions { get; set; } = new List<UnitConversion>();
        public ICollection<UnitConversion> ToUnitConversions { get; set; } = new List<UnitConversion>();
    }
}