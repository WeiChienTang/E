using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>薪資週期 — 每月開帳一筆，關帳後鎖定</summary>
    public class PayrollPeriod : BaseEntity
    {
        // BaseEntity 已提供：Id, Code, Status(EntityStatus), CreatedAt, CreatedBy,
        //                    UpdatedAt, UpdatedBy, Remarks

        /// <summary>民國年（如 114）</summary>
        [Required]
        [Display(Name = "年份")]
        [Range(100, 200)]
        public int Year { get; set; }

        /// <summary>月份（1-12）</summary>
        [Required]
        [Display(Name = "月份")]
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// 週期流程狀態（不同於 BaseEntity.Status 的啟用/停用）
        /// 命名為 PeriodStatus 以區別
        /// </summary>
        [Display(Name = "週期狀態")]
        public PayrollPeriodStatus PeriodStatus { get; set; } = PayrollPeriodStatus.Draft;

        /// <summary>關帳時間</summary>
        [Display(Name = "關帳時間")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>關帳人員</summary>
        [MaxLength(50)]
        [Display(Name = "關帳人員")]
        public string? ClosedBy { get; set; }

        // 導航屬性
        public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
    }
}
