using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 會計期間（Fiscal Period）
    /// 用於控制傳票可開立的期間範圍，每年最多 12 個期間（月份）
    /// </summary>
    [Index(nameof(FiscalYear))]
    [Index(nameof(CompanyId))]
    [Index(nameof(FiscalYear), nameof(PeriodNumber), nameof(CompanyId), IsUnique = true)]
    public class FiscalPeriod : BaseEntity
    {
        /// <summary>
        /// 會計年度（西元年，如 2024）
        /// </summary>
        [Required(ErrorMessage = "會計年度為必填")]
        [Range(1900, 2100, ErrorMessage = "會計年度必須介於 1900 至 2100 之間")]
        [Display(Name = "會計年度")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// 期間編號（1-12，對應月份）
        /// </summary>
        [Required(ErrorMessage = "期間編號為必填")]
        [Range(1, 12, ErrorMessage = "期間編號必須介於 1 至 12 之間")]
        [Display(Name = "期間編號")]
        public int PeriodNumber { get; set; }

        /// <summary>
        /// 期間開始日期
        /// </summary>
        [Required(ErrorMessage = "開始日期為必填")]
        [Display(Name = "開始日期")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 期間結束日期
        /// </summary>
        [Required(ErrorMessage = "結束日期為必填")]
        [Display(Name = "結束日期")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 期間狀態（Open=開放中、Closed=已關帳、Locked=已鎖定）
        /// </summary>
        [Required(ErrorMessage = "期間狀態為必填")]
        [Display(Name = "期間狀態")]
        public FiscalPeriodStatus PeriodStatus { get; set; } = FiscalPeriodStatus.Open;

        /// <summary>
        /// 公司 ID（多公司支援）
        /// </summary>
        [Required(ErrorMessage = "公司為必填")]
        [Display(Name = "公司")]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        /// <summary>
        /// 關帳時間（關帳時填入）
        /// </summary>
        [Display(Name = "關帳時間")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 關帳人員 ID（關帳時填入）
        /// </summary>
        [Display(Name = "關帳人員")]
        public int? ClosedByEmployeeId { get; set; }

        /// <summary>
        /// 重新開放原因（重開時填入）
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "重開原因")]
        public string? ReopenReason { get; set; }

        /// <summary>
        /// 最後重新開放時間
        /// </summary>
        [Display(Name = "最後重開時間")]
        public DateTime? ReopenedAt { get; set; }

        /// <summary>
        /// 公司導航屬性
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// 期間顯示名稱（不持久化）
        /// </summary>
        [NotMapped]
        public string DisplayName => $"{FiscalYear}/{PeriodNumber:D2}";

        /// <summary>
        /// 是否開放中（不持久化）
        /// </summary>
        [NotMapped]
        public bool IsOpen => PeriodStatus == FiscalPeriodStatus.Open;

        /// <summary>
        /// 是否曾被重開（有重開紀錄，UI 顯示警示 badge）
        /// </summary>
        [NotMapped]
        public bool WasReopened => ReopenedAt.HasValue;
    }
}
