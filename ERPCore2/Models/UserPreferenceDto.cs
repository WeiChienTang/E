using ERPCore2.Data.Entities;
using System.Text.Json;

namespace ERPCore2.Models;

/// <summary>
/// 使用者偏好設定 DTO，用於 localStorage 序列化/反序列化。
/// 不含 Language（語言設定保留在 DB，由 ASP.NET Core culture middleware 使用）。
/// </summary>
public class UserPreferenceDto
{
    // ── 顯示設定 ──
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public ContentZoom Zoom { get; set; } = ContentZoom.Medium;
    public bool ShowDisabledModules { get; set; } = false;
    public int DefaultPageSize { get; set; } = 20;
    public bool ShowUnsavedChangesWarning { get; set; } = true;

    // ── 快捷鍵 ──
    public string? ShortcutPageSearch { get; set; }
    public string? ShortcutReportSearch { get; set; }
    public string? ShortcutStickyNotes { get; set; }
    public string? ShortcutCalendar { get; set; }
    public string? ShortcutQuickAction { get; set; }

    // ── 通知設定 ──
    public bool EnableCalendar { get; set; } = true;
    public bool EnableStickyNote { get; set; } = true;
    public bool ShowCalendarBadge { get; set; } = true;
    public bool ShowNoteBadge { get; set; } = true;
    public int DefaultReminderMinutes { get; set; } = 15;

    // ── Toast 顯示時長（毫秒）──
    public int ToastSuccessDurationMs { get; set; } = 2000;
    public int ToastErrorDurationMs { get; set; } = 2000;
    public int ToastWarningDurationMs { get; set; } = 2000;
    public int ToastInfoDurationMs { get; set; } = 2000;

    /// <summary>Schema 版本，供未來向後相容性判斷</summary>
    public int SchemaVersion { get; set; } = 1;

    // ── 序列化設定 ──
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    // ── 轉換方法 ──

    /// <summary>從 EmployeePreference entity 建立 DTO（不含 Language）</summary>
    public static UserPreferenceDto FromEntity(EmployeePreference p) => new()
    {
        Theme                    = p.Theme,
        Zoom                     = p.Zoom,
        ShowDisabledModules      = p.ShowDisabledModules,
        DefaultPageSize          = p.DefaultPageSize,
        ShowUnsavedChangesWarning = p.ShowUnsavedChangesWarning,
        ShortcutPageSearch       = p.ShortcutPageSearch,
        ShortcutReportSearch     = p.ShortcutReportSearch,
        ShortcutStickyNotes      = p.ShortcutStickyNotes,
        ShortcutCalendar         = p.ShortcutCalendar,
        ShortcutQuickAction      = p.ShortcutQuickAction,
        EnableCalendar           = p.EnableCalendar,
        EnableStickyNote         = p.EnableStickyNote,
        ShowCalendarBadge        = p.ShowCalendarBadge,
        ShowNoteBadge            = p.ShowNoteBadge,
        DefaultReminderMinutes   = p.DefaultReminderMinutes,
        ToastSuccessDurationMs   = p.ToastSuccessDurationMs,
        ToastErrorDurationMs     = p.ToastErrorDurationMs,
        ToastWarningDurationMs   = p.ToastWarningDurationMs,
        ToastInfoDurationMs      = p.ToastInfoDurationMs,
    };

    /// <summary>將 DTO 的值寫回 EmployeePreference（Language 欄位不變）</summary>
    public void ApplyTo(EmployeePreference p)
    {
        p.Theme                    = Theme;
        p.Zoom                     = Zoom;
        p.ShowDisabledModules      = ShowDisabledModules;
        p.DefaultPageSize          = DefaultPageSize;
        p.ShowUnsavedChangesWarning = ShowUnsavedChangesWarning;
        p.ShortcutPageSearch       = ShortcutPageSearch;
        p.ShortcutReportSearch     = ShortcutReportSearch;
        p.ShortcutStickyNotes      = ShortcutStickyNotes;
        p.ShortcutCalendar         = ShortcutCalendar;
        p.ShortcutQuickAction      = ShortcutQuickAction;
        p.EnableCalendar           = EnableCalendar;
        p.EnableStickyNote         = EnableStickyNote;
        p.ShowCalendarBadge        = ShowCalendarBadge;
        p.ShowNoteBadge            = ShowNoteBadge;
        p.DefaultReminderMinutes   = DefaultReminderMinutes;
        p.ToastSuccessDurationMs   = ToastSuccessDurationMs;
        p.ToastErrorDurationMs     = ToastErrorDurationMs;
        p.ToastWarningDurationMs   = ToastWarningDurationMs;
        p.ToastInfoDurationMs      = ToastInfoDurationMs;
    }

    /// <summary>序列化為 JSON 字串</summary>
    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);

    /// <summary>從 JSON 字串反序列化；失敗時回傳預設值</summary>
    public static UserPreferenceDto FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<UserPreferenceDto>(json, _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }
}
