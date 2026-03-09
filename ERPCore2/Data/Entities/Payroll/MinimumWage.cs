using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>基本工資 — 每年由勞動部公告，需保留歷史</summary>
    public class MinimumWage
    {
        public int Id { get; set; }

        /// <summary>月基本工資</summary>
        [Required]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "月基本工資")] public decimal MonthlyAmount { get; set; }

        /// <summary>時基本工資</summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "時基本工資")] public decimal HourlyAmount { get; set; }

        /// <summary>生效日期</summary>
        [Required]
        [Display(Name = "生效日期")] public DateOnly EffectiveDate { get; set; }

        [MaxLength(200)]
        [Display(Name = "備註")] public string? Remarks { get; set; }
    }
}
