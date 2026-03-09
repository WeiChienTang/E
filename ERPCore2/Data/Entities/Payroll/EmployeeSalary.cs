using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 員工薪資設定 — 含生效日期，調薪時新增紀錄（歷史保留）
    /// 同一員工在同一時間點，ExpiryDate = null 的那筆為目前有效設定
    /// </summary>
    public class EmployeeSalary : BaseEntity
    {
        // BaseEntity 已提供：Id, Code, Status(EntityStatus), CreatedAt, CreatedBy,
        //                    UpdatedAt, UpdatedBy, Remarks

        /// <summary>員工 ID</summary>
        [Required]
        [Display(Name = "員工")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // ── 薪資制度 ──────────────────────────────────────────
        /// <summary>薪資制度（月薪 / 時薪）</summary>
        [Display(Name = "薪資制度")]
        public SalaryType SalaryType { get; set; } = SalaryType.Monthly;

        /// <summary>本薪（月薪制：元/月；時薪制：元/時）</summary>
        [Required]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "本薪")]
        [Range(0, 9_999_999)]
        public decimal BaseSalary { get; set; }

        /// <summary>職務加給</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "職務加給")]
        public decimal PositionAllowance { get; set; } = 0;

        /// <summary>餐飲補助（每月固定）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "餐飲補助")]
        public decimal MealAllowance { get; set; } = 0;

        /// <summary>交通津貼（每月固定）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "交通津貼")]
        public decimal TransportAllowance { get; set; } = 0;

        // ── 勞健保設定 ────────────────────────────────────────
        /// <summary>勞保投保薪資（依分級表選取，非實際薪資）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "勞保投保薪資")]
        public decimal LaborInsuredSalary { get; set; }

        /// <summary>健保投保金額（依分級表選取）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "健保投保金額")]
        public decimal HealthInsuredAmount { get; set; }

        /// <summary>健保眷屬人數（不含本人）</summary>
        [Display(Name = "眷屬人數")]
        [Range(0, 10)]
        public int DependentCount { get; set; } = 0;

        // ── 所得稅設定 ────────────────────────────────────────
        /// <summary>扣繳類型</summary>
        [Display(Name = "扣繳類型")]
        public TaxWithholdingType TaxType { get; set; } = TaxWithholdingType.Standard;

        // ── 勞退提繳 ──────────────────────────────────────────
        /// <summary>員工自願提繳率（0～6%，以 0.01 為單位）</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "自願提繳率(%)")]
        [Range(0, 6)]
        public decimal VoluntaryRetirementRate { get; set; } = 0;

        // ── 生效區間 ──────────────────────────────────────────
        /// <summary>生效日期</summary>
        [Required]
        [Display(Name = "生效日期")]
        public DateOnly EffectiveDate { get; set; }

        /// <summary>失效日期（null = 目前有效）</summary>
        [Display(Name = "失效日期")]
        public DateOnly? ExpiryDate { get; set; }
    }
}
