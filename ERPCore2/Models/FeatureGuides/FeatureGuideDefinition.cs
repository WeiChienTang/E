namespace ERPCore2.Models.FeatureGuides;

/// <summary>
/// 功能說明定義（數據驅動，由 FeatureGuideRenderer 自動渲染）
/// </summary>
public class FeatureGuideDefinition
{
    /// <summary>說明章節列表</summary>
    public List<GuideSection> Sections { get; set; } = new();

    /// <summary>定義檔來源路徑（SuperAdmin Debug 用，自動填入）</summary>
    public string? SourceFile { get; set; }
}

/// <summary>
/// 章節類型 — 決定渲染模板
/// </summary>
public enum GuideSectionType
{
    /// <summary>段落描述文字</summary>
    Description,
    /// <summary>有序步驟列表（自動編號圓圈）</summary>
    Steps,
    /// <summary>欄位說明列表（dt/dd 格式）</summary>
    FieldList,
    /// <summary>提示與警告框</summary>
    Tips,
    /// <summary>常見問題（問答對）</summary>
    FAQ
}

/// <summary>
/// 內容項目的樣式
/// </summary>
public enum GuideItemStyle
{
    /// <summary>一般文字</summary>
    Normal,
    /// <summary>藍色提示框</summary>
    Tip,
    /// <summary>黃色警告框</summary>
    Warning
}

/// <summary>
/// 單一章節定義
/// </summary>
public class GuideSection
{
    /// <summary>HTML anchor id（用於書籤滾動）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>章節標題的 resx key</summary>
    public string TitleKey { get; set; } = string.Empty;

    /// <summary>Bootstrap Icons CSS 類別（例如 "bi-info-circle"）</summary>
    public string Icon { get; set; } = "bi-info-circle";

    /// <summary>左側書籤標籤文字</summary>
    public string BookmarkLabel { get; set; } = string.Empty;

    /// <summary>書籤顏色（HEX，例如 "#3B82F6"）</summary>
    public string BookmarkColor { get; set; } = "#3B82F6";

    /// <summary>章節類型（決定渲染方式）</summary>
    public GuideSectionType Type { get; set; } = GuideSectionType.Description;

    /// <summary>章節內容項目</summary>
    public List<GuideItem> Items { get; set; } = new();
}

/// <summary>
/// 內容項目定義
/// </summary>
public class GuideItem
{
    /// <summary>主要文字的 resx key（步驟文字、描述、答案等）</summary>
    public string TextKey { get; set; } = string.Empty;

    /// <summary>標籤文字的 resx key（欄位名稱、問題標題，FieldList/FAQ 用）</summary>
    public string? LabelKey { get; set; }

    /// <summary>項目樣式（Normal / Tip / Warning）</summary>
    public GuideItemStyle Style { get; set; } = GuideItemStyle.Normal;

    /// <summary>建立一般項目</summary>
    public GuideItem() { }

    /// <summary>建立一般項目（快捷建構）</summary>
    public GuideItem(string textKey)
    {
        TextKey = textKey;
    }

    /// <summary>建立含標籤的項目（FieldList/FAQ 用）</summary>
    public GuideItem(string labelKey, string textKey)
    {
        LabelKey = labelKey;
        TextKey = textKey;
    }

    /// <summary>建立提示/警告項目</summary>
    public GuideItem(string textKey, GuideItemStyle style)
    {
        TextKey = textKey;
        Style = style;
    }
}
