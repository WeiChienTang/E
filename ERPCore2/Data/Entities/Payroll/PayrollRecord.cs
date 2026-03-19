using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>薪資單主檔 — 每人每月一筆</summary>
    public class PayrollRecord : BaseEntity
    {
        // BaseEntity 已提供：Id, Code, Status(EntityStatus), CreatedAt, CreatedBy,
        //                    UpdatedAt, UpdatedBy, Remarks
        // 薪資單流程狀態命名為 RecordStatus，以區別 BaseEntity.Status（EntityStatus）

        [Required] public int PayrollPeriodId { get; set; }
        public PayrollPeriod Period { get; set; } = null!;

        [Required] public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // ── 出勤彙總快照（從出勤模組或手動輸入取得）────────────
        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "應出勤天數")] public decimal ScheduledWorkDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "實際出勤天數")] public decimal ActualWorkDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "曠職天數")] public decimal AbsentDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "病假天數（半薪）")] public decimal SickLeaveDays { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "平日加班-前2hr")] public decimal OvertimeHours1 { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "平日加班-後2hr")] public decimal OvertimeHours2 { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "休息日加班時數")] public decimal HolidayOvertimeHours { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "國定假日加班時數")] public decimal NationalHolidayHours { get; set; }

        // ── 時薪專用 ──────────────────────────────────────────
        /// <summary>時薪員工實際工時總計（月薪員工為 0）</summary>
        [Column(TypeName = "decimal(7,2)")]
        [Display(Name = "總工時（時薪）")] public decimal TotalWorkHours { get; set; }

        // ── 金額彙總 ──────────────────────────────────────────
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "應發薪資")] public decimal GrossIncome { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "扣除合計")] public decimal TotalDeduction { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "實發薪資")] public decimal NetPay { get; set; }

        // ── 投保快照（鎖定計算當月使用值）───────────────────────
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "勞保投保薪資")] public decimal LaborInsuranceSalary { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "健保投保金額")] public decimal HealthInsuranceAmount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "課稅所得")] public decimal TaxableIncome { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "代扣所得稅")] public decimal WithholdingTax { get; set; }

        // ── 雇主負擔快照（用於會計傳票，不顯示於員工薪資單）──────
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "雇主勞保費")] public decimal EmployerLaborInsurance { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "雇主健保費")] public decimal EmployerHealthInsurance { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "雇主勞退提繳")] public decimal EmployerRetirement { get; set; }

        // ── 薪資單流程狀態（不同於 BaseEntity.Status 的啟用/停用）──
        [Display(Name = "薪資單狀態")]
        public PayrollRecordStatus RecordStatus { get; set; } = PayrollRecordStatus.Draft;

        // ── 計算稽核欄位（CreatedAt = 記錄建立時間；CalculatedAt = 計算引擎執行時間）
        [Display(Name = "計算時間")] public DateTime? CalculatedAt { get; set; }

        [MaxLength(50)]
        [Display(Name = "計算人員")] public string? CalculatedBy { get; set; }

        // 導航屬性
        public ICollection<PayrollRecordDetail> Details { get; set; } = new List<PayrollRecordDetail>();
    }
}
