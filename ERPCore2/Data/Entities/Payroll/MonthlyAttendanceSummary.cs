using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Entities;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 月度出勤彙總 — 每員工每月一筆，作為薪資計算的出勤資料來源
    /// Year 使用民國年（114 = 2025）
    /// </summary>
    public class MonthlyAttendanceSummary
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        /// <summary>民國年（例：114 = 2025）</summary>
        [Required]
        public int Year { get; set; }

        /// <summary>月份（1–12）</summary>
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        // ── 出勤天數 ─────────────────────────────────────────────────

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "應出勤天數")]
        public decimal ScheduledWorkDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "實際出勤天數")]
        public decimal ActualWorkDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "曠職天數")]
        public decimal AbsentDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "病假天數（半薪）")]
        public decimal SickLeaveDays { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Display(Name = "事假天數（無薪）")]
        public decimal PersonalLeaveDays { get; set; }

        // ── 加班時數 ─────────────────────────────────────────────────

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "平日加班-前2hr")]
        public decimal OvertimeHours1 { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "平日加班-後2hr")]
        public decimal OvertimeHours2 { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "休息日加班時數")]
        public decimal HolidayOvertimeHours { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "國定假日加班時數")]
        public decimal NationalHolidayHours { get; set; }

        // ── 時薪專用 ──────────────────────────────────────────────────

        /// <summary>
        /// 時薪員工實際工時總計（月薪員工留 0）
        /// 由逐日出勤記錄彙總而來，或手動輸入
        /// </summary>
        [Column(TypeName = "decimal(7,2)")]
        [Display(Name = "總工時（時薪）")]
        public decimal TotalWorkHours { get; set; }

        // ── 狀態 ─────────────────────────────────────────────────────

        /// <summary>鎖定後不允許修改（薪資計算後自動鎖定）</summary>
        [Display(Name = "已鎖定")]
        public bool IsLocked { get; set; } = false;

        [MaxLength(500)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        // ── 稽核欄位 ──────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
