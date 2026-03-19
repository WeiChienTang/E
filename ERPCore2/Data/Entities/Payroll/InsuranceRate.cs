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

        // ── 加班費倍率（勞基法定義，有效期間可隨法規修訂調整）──────────────
        // 計算引擎讀取時若欄位值為 0，自動使用法定預設值（備援常數）

        /// <summary>平日加班前2小時倍率（勞基法第24條第1項第1款：法定4/3≈1.3333）</summary>
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "平日加班倍率（前2hr）")]
        public decimal OvertimeRate1 { get; set; } = 1.3333m;

        /// <summary>平日加班後2小時倍率（勞基法第24條第1項第2款：法定5/3≈1.6667）</summary>
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "平日加班倍率（後2hr）")]
        public decimal OvertimeRate2 { get; set; } = 1.6667m;

        /// <summary>休息日加班前2小時倍率（勞基法第24條第2項：法定4/3≈1.3333）</summary>
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "休息日加班倍率（前2hr）")]
        public decimal RestDayRate1 { get; set; } = 1.3333m;

        /// <summary>休息日加班超過2小時倍率（勞基法第24條第2項：法定5/3≈1.6667）</summary>
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "休息日加班倍率（後2hr+）")]
        public decimal RestDayRate2 { get; set; } = 1.6667m;

        /// <summary>國定假日額外加給倍率（勞基法第39條：月薪已含假日薪，加給1倍，法定1.0）</summary>
        [Column(TypeName = "decimal(6,4)")]
        [Display(Name = "國定假日加給倍率")]
        public decimal NationalHolidayRate { get; set; } = 1.0000m;

        [MaxLength(200)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
}
