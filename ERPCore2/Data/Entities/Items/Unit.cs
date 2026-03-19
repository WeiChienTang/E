using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 單位實體 - 定義品項計量單位
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Unit : BaseEntity
    {
        
        [MaxLength(20, ErrorMessage = "單位名稱不可超過20個字元")]
        [Display(Name = "單位名稱")]
        public string? Name { get; set; }

        [MaxLength(50, ErrorMessage = "英文名稱不可超過50個字元")]
        [Display(Name = "英文名稱")]
        public string? EnglishName { get; set; }
        
        // Navigation Properties
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<UnitConversion> FromUnitConversions { get; set; } = new List<UnitConversion>();
        public ICollection<UnitConversion> ToUnitConversions { get; set; } = new List<UnitConversion>();
    }
}
