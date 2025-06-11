using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 搜尋篩選字段配置
/// </summary>
public class SearchFilterDefinition
{
    /// <summary>
    /// 字段名稱（對應屬性名稱）
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 顯示標籤
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 輸入提示文字
    /// </summary>
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// 篩選器類型
    /// </summary>
    public FilterType FilterType { get; set; } = FilterType.Text;
    
    /// <summary>
    /// 欄位寬度（Bootstrap 格線系統）
    /// </summary>
    public int ColumnWidth { get; set; } = 4;
    
    /// <summary>
    /// 排序順序
    /// </summary>
    public int Order { get; set; } = 0;
    
    /// <summary>
    /// 下拉選項資料源
    /// </summary>
    public IEnumerable<SelectOption>? SelectOptions { get; set; }
    
    /// <summary>
    /// 自定義範本
    /// </summary>
    public RenderFragment<object>? CustomTemplate { get; set; }
    
    /// <summary>
    /// CSS 類別
    /// </summary>
    public string? CssClass { get; set; }
    
    /// <summary>
    /// 是否預設顯示
    /// </summary>
    public bool IsDefaultVisible { get; set; } = true;
    
    /// <summary>
    /// 預設值
    /// </summary>
    public object? DefaultValue { get; set; }
    
    /// <summary>
    /// 最小值（數字和日期類型使用）
    /// </summary>
    public object? MinValue { get; set; }
    
    /// <summary>
    /// 最大值（數字和日期類型使用）
    /// </summary>
    public object? MaxValue { get; set; }
    
    /// <summary>
    /// 是否啟用清除按鈕
    /// </summary>
    public bool ShowClearButton { get; set; } = true;
    
    /// <summary>
    /// 說明文字
    /// </summary>
    public string? HelpText { get; set; }
}

/// <summary>
/// 篩選器類型
/// </summary>
public enum FilterType
{
    /// <summary>
    /// 文字搜尋
    /// </summary>
    Text,
    
    /// <summary>
    /// 下拉選單
    /// </summary>
    Select,
    
    /// <summary>
    /// 多選下拉
    /// </summary>
    MultiSelect,
    
    /// <summary>
    /// 日期範圍
    /// </summary>
    DateRange,
    
    /// <summary>
    /// 數字範圍
    /// </summary>
    NumberRange,
    
    /// <summary>
    /// 核取方塊
    /// </summary>
    Checkbox,
    
    /// <summary>
    /// 自定義範本
    /// </summary>
    Custom
}

/// <summary>
/// 搜尋結果模型
/// </summary>
public class SearchFilterModel
{
    public Dictionary<string, object?> Filters { get; set; } = new();
    
    /// <summary>
    /// 設定篩選值
    /// </summary>
    public void SetFilter(string propertyName, object? value)
    {
        Filters[propertyName] = value;
    }
    
    /// <summary>
    /// 取得篩選值
    /// </summary>
    public T? GetFilter<T>(string propertyName, T? defaultValue = default)
    {
        if (Filters.TryGetValue(propertyName, out var value) && value != null)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 清除所有篩選
    /// </summary>
    public void Clear()
    {
        Filters.Clear();
    }
    
    /// <summary>
    /// 清除特定篩選
    /// </summary>
    public void ClearFilter(string propertyName)
    {
        Filters.Remove(propertyName);
    }
}
