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

        // ===== 以下欄位由 localStorage 管理，不寫入 DB =====
        // 由 MainLayout.LoadPreferenceAsync 從 localStorage 讀取後寫入此 entity，作為記憶體載體

        [NotMapped] public ContentZoom Zoom { get; set; } = ContentZoom.Medium;
        [NotMapped] public AppTheme Theme { get; set; } = AppTheme.Light;

        // 快捷鍵（null = 使用 ShortcutDefaults 預設值）
        [NotMapped] public string? ShortcutPageSearch { get; set; }
        [NotMapped] public string? ShortcutReportSearch { get; set; }
        [NotMapped] public string? ShortcutStickyNotes { get; set; }
        [NotMapped] public string? ShortcutCalendar { get; set; }
        [NotMapped] public string? ShortcutQuickAction { get; set; }

        // 通知設定
        [NotMapped] public bool EnableCalendar { get; set; } = true;
        [NotMapped] public bool EnableStickyNote { get; set; } = true;
        [NotMapped] public bool ShowCalendarBadge { get; set; } = true;
        [NotMapped] public bool ShowNoteBadge { get; set; } = true;
        [NotMapped] public int DefaultReminderMinutes { get; set; } = 15;

        // Toast 顯示時長（毫秒）
        [NotMapped] public int ToastSuccessDurationMs { get; set; } = 2000;
        [NotMapped] public int ToastErrorDurationMs { get; set; } = 2000;
        [NotMapped] public int ToastWarningDurationMs { get; set; } = 2000;
        [NotMapped] public int ToastInfoDurationMs { get; set; } = 2000;

        // 導覽列與操作偏好
        [NotMapped] public bool ShowDisabledModules { get; set; } = false;
        [NotMapped] public int DefaultPageSize { get; set; } = 20;
        [NotMapped] public bool ShowUnsavedChangesWarning { get; set; } = true;

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
