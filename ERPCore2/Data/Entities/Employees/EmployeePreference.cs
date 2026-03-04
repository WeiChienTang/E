using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工個人化設定 - 每位員工的偏好設定（1-to-1）
    /// 記錄不存在代表使用系統預設值
    /// </summary>
    [Index(nameof(EmployeeId), IsUnique = true)]
    public class EmployeePreference : BaseEntity
    {
        /// <summary>
        /// 關聯員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 介面語言
        /// </summary>
        [Display(Name = "介面語言")]
        public UILanguage Language { get; set; } = UILanguage.ZhTW;

        /// <summary>
        /// 字型縮放級別
        /// </summary>
        [Display(Name = "字型大小")]
        public ContentZoom Zoom { get; set; } = ContentZoom.Medium;

        /// <summary>
        /// 介面主題
        /// </summary>
        [Display(Name = "主題")]
        public AppTheme Theme { get; set; } = AppTheme.Light;

        // ===== 快捷鍵自訂 =====
        // null = 使用系統預設值；預設值定義於 ShortcutDefaults

        /// <summary>頁面搜尋快捷鍵（預設 Alt+S）</summary>
        [MaxLength(20)]
        public string? ShortcutPageSearch { get; set; }

        /// <summary>報表搜尋快捷鍵（預設 Alt+R）</summary>
        [MaxLength(20)]
        public string? ShortcutReportSearch { get; set; }

        /// <summary>便條貼快捷鍵（預設 Alt+N）</summary>
        [MaxLength(20)]
        public string? ShortcutStickyNotes { get; set; }

        /// <summary>行事曆快捷鍵（預設 Alt+C）</summary>
        [MaxLength(20)]
        public string? ShortcutCalendar { get; set; }

        /// <summary>快速功能表快捷鍵（預設 Alt+Q）</summary>
        [MaxLength(20)]
        public string? ShortcutQuickAction { get; set; }

        // ===== 通知設定 =====

        /// <summary>是否啟用行事曆功能（預設開啟）</summary>
        [Display(Name = "啟用行事曆")]
        public bool EnableCalendar { get; set; } = true;

        /// <summary>是否啟用便條貼功能（預設開啟）</summary>
        [Display(Name = "啟用便條貼")]
        public bool EnableStickyNote { get; set; } = true;

        /// <summary>是否在 QuickAction 按鈕顯示行事曆提醒徽章（預設開啟）</summary>
        [Display(Name = "顯示行事曆提醒徽章")]
        public bool ShowCalendarBadge { get; set; } = true;

        /// <summary>是否在 QuickAction 按鈕顯示便條貼計數徽章（預設開啟）</summary>
        [Display(Name = "顯示便條貼計數徽章")]
        public bool ShowNoteBadge { get; set; } = true;

        /// <summary>行事曆事項的預設提醒時間（分鐘）。0 = 不提醒</summary>
        [Display(Name = "預設提醒時間（分鐘）")]
        public int DefaultReminderMinutes { get; set; } = 15;

        // ===== 系統訊息顯示時長 (毫秒) =====

        /// <summary>成功訊息顯示時長（毫秒），預設 2000</summary>
        [Display(Name = "成功訊息顯示時長")]
        public int ToastSuccessDurationMs { get; set; } = 2000;

        /// <summary>錯誤訊息顯示時長（毫秒），預設 2000</summary>
        [Display(Name = "錯誤訊息顯示時長")]
        public int ToastErrorDurationMs { get; set; } = 2000;

        /// <summary>警告訊息顯示時長（毫秒），預設 2000</summary>
        [Display(Name = "警告訊息顯示時長")]
        public int ToastWarningDurationMs { get; set; } = 2000;

        /// <summary>資訊訊息顯示時長（毫秒），預設 2000</summary>
        [Display(Name = "資訊訊息顯示時長")]
        public int ToastInfoDurationMs { get; set; } = 2000;

        // ===== 導覽列顯示設定 =====

        /// <summary>是否在側邊選單中顯示已停用的模組（灰色鎖定狀態）。false = 完全隱藏</summary>
        [Display(Name = "顯示停用模組")]
        public bool ShowDisabledModules { get; set; } = false;

        // 導航屬性
        public Employee? Employee { get; set; }
    }

    /// <summary>
    /// 快捷鍵預設值常數
    /// </summary>
    public static class ShortcutDefaults
    {
        public const string PageSearch   = "Alt+S";
        public const string ReportSearch = "Alt+R";
        public const string StickyNotes  = "Alt+N";
        public const string Calendar     = "Alt+C";
        public const string QuickAction  = "Alt+Q";
    }

    /// <summary>
    /// 字型縮放級別列舉
    /// </summary>
    public enum ContentZoom
    {
        [Display(Name = "75%")]
        XSmall = 1,

        [Display(Name = "90%")]
        Small = 2,

        [Display(Name = "100%")]
        Medium = 3,

        [Display(Name = "110%")]
        Large = 4,

        [Display(Name = "125%")]
        XLarge = 5,

        [Display(Name = "150%")]
        XXLarge = 6
    }

    /// <summary>
    /// 介面主題列舉
    /// </summary>
    public enum AppTheme
    {
        [Display(Name = "淺色")]
        Light = 1,

        [Display(Name = "深色")]
        Dark = 2
    }

    /// <summary>
    /// 介面語言列舉
    /// </summary>
    public enum UILanguage
    {
        [Display(Name = "繁體中文")]
        ZhTW = 1,

        [Display(Name = "English")]
        EnUS = 2,

        [Display(Name = "日本語")]
        JaJP = 3,

        [Display(Name = "简体中文")]
        ZhCN = 4,

        [Display(Name = "Filipino")]
        FilPH = 5
    }
}
