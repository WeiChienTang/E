using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 單位轉換實體 - 定義不同單位間的轉換關係
    /// </summary>
    [Index(nameof(FromUnitId), nameof(ToUnitId), IsUnique = true)]
    public class UnitConversion : BaseEntity
    {
        // Required Properties
        [Required(ErrorMessage = "轉換比例為必填")]
        [Display(Name = "轉換比例")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ConversionRate { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        // Foreign Keys
        [Required(ErrorMessage = "來源單位為必填")]
        [Display(Name = "來源單位")]
        [ForeignKey(nameof(FromUnit))]
        public int FromUnitId { get; set; }
        
        [Required(ErrorMessage = "目標單位為必填")]
        [Display(Name = "目標單位")]
        [ForeignKey(nameof(ToUnit))]
        public int ToUnitId { get; set; }
        
        // Navigation Properties
        public Unit FromUnit { get; set; } = null!;
        public Unit ToUnit { get; set; } = null!;
    }
}
