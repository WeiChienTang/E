using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 薪資保費費率表
    /// 儲存勞保、健保、勞退費率及免稅上限，每次調整新增一筆（含生效日）
    /// Phase 1 使用常數；Phase 2 改從此表讀取
    /// </summary>
    public class InsuranceRate
    {
        public int Id { get; set; }

        /// <summary>生效日期（取 EffectiveDate ≤ 目標日期 最新一筆）</summary>
        [Required]
        [Display(Name = "生效日期")]
        public DateOnly EffectiveDate { get; set; }

        // ── 勞保費率 ──────────────────────────────────────────────────
        /// <summary>勞保員工負擔費率（例：0.02 = 2%）</summary>
        [Required]
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "勞保員工費率")]
        public decimal LaborInsuranceEmployeeRate { get; set; }

        /// <summary>勞保雇主負擔費率（例：0.10 = 10%）</summary>
        [Required]
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "勞保雇主費率")]
        public decimal LaborInsuranceEmployerRate { get; set; }

        // ── 健保費率 ──────────────────────────────────────────────────
        /// <summary>健保員工負擔費率（例：0.0235 = 2.35%）</summary>
        [Required]
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "健保員工費率")]
        public decimal HealthInsuranceEmployeeRate { get; set; }

        /// <summary>健保雇主負擔費率（例：0.0611 = 6.11%）</summary>
        [Required]
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "健保雇主費率")]
        public decimal HealthInsuranceEmployerRate { get; set; }

        // ── 勞退費率 ──────────────────────────────────────────────────
        /// <summary>勞退雇主強制提繳費率（例：0.06 = 6%）</summary>
        [Required]
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "勞退雇主費率")]
        public decimal RetirementEmployerRate { get; set; }

        // ── 免稅上限 ──────────────────────────────────────────────────
        /// <summary>餐費每月免稅上限（元）</summary>
        [Required]
        [Column(TypeName = "decimal(10,0)")]
        [Display(Name = "餐費免稅上限")]
        public decimal MealTaxFreeLimit { get; set; }

        /// <summary>交通費每月免稅上限（元）</summary>
        [Required]
        [Column(TypeName = "decimal(10,0)")]
        [Display(Name = "交通費免稅上限")]
        public decimal TransportTaxFreeLimit { get; set; }

        [MaxLength(200)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
}
