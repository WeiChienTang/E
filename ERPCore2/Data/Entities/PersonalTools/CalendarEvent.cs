using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 個人行事曆事項
    /// Phase 1：個人手動新增事項
    /// Phase 2（預留）：ERP 系統自動建立事項（如採購到期、交貨日）
    /// </summary>
    public class CalendarEvent : BaseEntity
    {
        /// <summary>
        /// 所屬員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 事項名稱
        /// </summary>
        [Required]
        [Display(Name = "事項名稱")]
        [MaxLength(200, ErrorMessage = "事項名稱不可超過 200 字元")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 詳細說明（預留）
        /// </summary>
        [Display(Name = "說明")]
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// 事項日期
        /// </summary>
        [Required]
        [Display(Name = "日期")]
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// 事項時間（null = 全天事項）
        /// </summary>
        [Display(Name = "時間")]
        public TimeOnly? EventTime { get; set; }

        /// <summary>
        /// 顯示顏色
        /// </summary>
        [Display(Name = "顏色")]
        public CalendarEventColor Color { get; set; } = CalendarEventColor.Blue;

        /// <summary>
        /// 事項來源類型
        /// </summary>
        [Display(Name = "類型")]
        public CalendarEventType EventType { get; set; } = CalendarEventType.Personal;

        /// <summary>
        /// 來源模組（Phase 2 預留：例如 "PurchaseOrder"、"SalesOrder"）
        /// </summary>
        [Display(Name = "來源模組")]
        [MaxLength(50)]
        public string? SourceModule { get; set; }

        /// <summary>
        /// 來源資料 ID（Phase 2 預留：對應模組記錄的 PK）
        /// </summary>
        [Display(Name = "來源 ID")]
        public int? SourceId { get; set; }

        /// <summary>
        /// 提醒時間（分鐘）。null = 使用個人偏好設定的預設值，0 = 不提醒
        /// </summary>
        [Display(Name = "提醒")]
        public int? ReminderMinutes { get; set; }

        // 導航屬性
        public Employee? Employee { get; set; }
    }

    /// <summary>
    /// 行事曆事項顏色
    /// </summary>
    public enum CalendarEventColor
    {
        [Display(Name = "黃色")]
        Yellow = 1,

        [Display(Name = "綠色")]
        Green = 2,

        [Display(Name = "藍色")]
        Blue = 3,

        [Display(Name = "紅色")]
        Red = 4
    }

    /// <summary>
    /// 行事曆事項類型
    /// </summary>
    public enum CalendarEventType
    {
        [Display(Name = "個人事項")]
        Personal = 1,

        [Display(Name = "系統事項")]
        System = 2
    }
}
