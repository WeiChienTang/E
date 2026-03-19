using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 逐日出勤記錄 — 每員工每天一筆
    /// 月薪員工：記錄出勤狀態即可，WorkHours 留 0
    /// 時薪員工：需填寫 WorkHours（實際上班時數，可超過8小時）
    /// Year 使用民國年（114 = 2025）
    /// </summary>
    public class AttendanceDailyRecord
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        /// <summary>出勤日期</summary>
        [Required]
        public DateOnly Date { get; set; }

        /// <summary>出勤狀態</summary>
        [Display(Name = "出勤狀態")]
        public DailyAttendanceStatus Status { get; set; } = DailyAttendanceStatus.Present;

        /// <summary>
        /// 實際工時（時薪員工必填；月薪員工留 0）
        /// 允許超過 8 小時（例：15 小時）
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "工時")]
        [Range(0, 24)]
        public decimal WorkHours { get; set; }

        /// <summary>平日加班-前2hr（當天）</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "平日加班前2hr")]
        public decimal OvertimeHours1 { get; set; }

        /// <summary>平日加班-後2hr（當天）</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "平日加班後2hr")]
        public decimal OvertimeHours2 { get; set; }

        /// <summary>休息日加班時數（當天）</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "休息日加班")]
        public decimal HolidayOvertimeHours { get; set; }

        /// <summary>國定假日加班時數（當天）</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "國定假日加班")]
        public decimal NationalHolidayHours { get; set; }

        [MaxLength(200)]
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
