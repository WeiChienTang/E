using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 業績目標主檔 - 設定業務員或公司整體的月度/年度銷售目標
    /// </summary>
    [Index(nameof(Year), nameof(Month), nameof(SalespersonId), IsUnique = true)]
    public class SalesTarget : BaseEntity
    {
        /// <summary>年度（例如 2026）</summary>
        [Required(ErrorMessage = "年度為必填")]
        [Display(Name = "年度")]
        [Range(2000, 2100, ErrorMessage = "年度範圍不正確")]
        public int Year { get; set; } = DateTime.Today.Year;

        /// <summary>月份（1–12，null 代表年度整體目標）</summary>
        [Display(Name = "月份")]
        [Range(1, 12, ErrorMessage = "月份需為 1–12")]
        public int? Month { get; set; }

        /// <summary>目標金額（含稅）</summary>
        [Required(ErrorMessage = "目標金額為必填")]
        [Display(Name = "目標金額")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "目標金額不可為負數")]
        public decimal TargetAmount { get; set; }

        // Foreign Keys
        /// <summary>
        /// 業務員 ID（null = 公司整體目標，不隸屬於特定業務員）
        /// </summary>
        [Display(Name = "業務員")]
        [ForeignKey(nameof(Salesperson))]
        public int? SalespersonId { get; set; }

        // Navigation Properties
        public Employee? Salesperson { get; set; }

        // Computed (NotMapped)
        [NotMapped]
        public string PeriodLabel => Month.HasValue
            ? $"{Year}/{Month:D2}"
            : $"{Year}（全年）";

        [NotMapped]
        public string SalespersonLabel => Salesperson?.Name ?? "公司整體";
    }
}
