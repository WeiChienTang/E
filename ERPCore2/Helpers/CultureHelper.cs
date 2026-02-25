using ERPCore2.Data.Entities;

namespace ERPCore2.Helpers;

/// <summary>
/// UILanguage 列舉與 .NET culture code 互換工具
/// </summary>
public static class CultureHelper
{
    /// <summary>
    /// 將 UILanguage 轉換為 culture code（例如 "zh-TW"、"en-US"）
    /// </summary>
    public static string ToCultureCode(UILanguage language) => language switch
    {
        UILanguage.ZhTW => "zh-TW",
        UILanguage.EnUS => "en-US",
        UILanguage.JaJP => "ja-JP",
        UILanguage.ZhCN => "zh-CN",
        UILanguage.FilPH => "fil",
        _ => "zh-TW"
    };

    /// <summary>
    /// 應用程式支援的 culture code 清單
    /// </summary>
    public static readonly string[] SupportedCultures = ["zh-TW", "en-US", "ja-JP", "zh-CN", "fil"];
}
