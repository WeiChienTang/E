namespace ERPCore2.Components.Shared.Table;

/// <summary>
/// 右鍵選單項目定義 — 供 GenericTableComponent / GenericInteractiveTableComponent 使用
/// </summary>
public class ContextMenuItem<TItem>
{
    /// <summary>顯示文字</summary>
    public string Label { get; set; } = "";

    /// <summary>Font Awesome / Bootstrap Icons class，例如 "fas fa-trash"</summary>
    public string? IconClass { get; set; }

    /// <summary>額外 CSS class，例如 "text-danger"</summary>
    public string CssClass { get; set; } = "";

    /// <summary>是否為分隔線（IsDivider=true 時 Label/IconClass/OnClick 均無效）</summary>
    public bool IsDivider { get; set; }

    /// <summary>點擊事件；null 時點擊無反應</summary>
    public Func<TItem, Task>? OnClick { get; set; }

    /// <summary>動態控制是否顯示；null 代表永遠顯示</summary>
    public Func<TItem, bool>? IsVisible { get; set; }

    /// <summary>動態控制是否停用；null 代表永遠啟用</summary>
    public Func<TItem, bool>? IsDisabled { get; set; }
}
