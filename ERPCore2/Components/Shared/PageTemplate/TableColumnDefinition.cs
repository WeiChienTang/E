using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.PageTemplate;

/// <summary>
/// 資料表格欄位定義
/// </summary>
public class TableColumnDefinition
{
    /// <summary>
    /// 欄位標題
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 對應的屬性名稱，支援巢狀屬性 (例如: "Role.RoleName")
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 資料類型
    /// </summary>
    public ColumnDataType DataType { get; set; } = ColumnDataType.Text;
    
    /// <summary>
    /// 自定義單元格範本
    /// </summary>
    public RenderFragment<object>? CustomTemplate { get; set; }
    
    /// <summary>
    /// 是否可排序
    /// </summary>
    public bool IsSortable { get; set; } = false;
    
    /// <summary>
    /// 欄位標題的 CSS 類別
    /// </summary>
    public string? HeaderCssClass { get; set; }
    
    /// <summary>
    /// 欄位標題的內聯樣式
    /// </summary>
    public string? HeaderStyle { get; set; }
    
    /// <summary>
    /// 單元格的 CSS 類別
    /// </summary>
    public string? CellCssClass { get; set; }
    
    /// <summary>
    /// 單元格的內聯樣式
    /// </summary>
    public string? CellStyle { get; set; }
    
    /// <summary>
    /// 欄位寬度 (CSS width 值，例如: "150px", "20%", "auto")
    /// </summary>
    public string? Width { get; set; }
    
    /// <summary>
    /// 標題的圖示 CSS 類別
    /// </summary>
    public string? IconClass { get; set; }
    
    /// <summary>
    /// 當值為 null 時顯示的文字
    /// </summary>
    public string? NullDisplayText { get; set; } = "-";
    
    /// <summary>
    /// 格式化字串 (用於數值、日期等)
    /// </summary>
    public string? Format { get; set; }
    
    /// <summary>
    /// 貨幣符號 (預設: "NT$")
    /// </summary>
    public string? CurrencySymbol { get; set; } = "NT$";
    
    /// <summary>
    /// 布林值為 true 時顯示的文字
    /// </summary>
    public string? TrueText { get; set; } = "是";
    
    /// <summary>
    /// 布林值為 false 時顯示的文字
    /// </summary>
    public string? FalseText { get; set; } = "否";
    
    /// <summary>
    /// 狀態值的徽章樣式對應表 (值 -> Bootstrap 徽章 CSS 類別)
    /// </summary>
    public Dictionary<object, string>? StatusBadgeMap { get; set; }
    
    #region 靜態工廠方法
    
    /// <summary>
    /// 建立文字欄位
    /// </summary>
    public static TableColumnDefinition Text(string title, string propertyName, string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Text,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立數值欄位
    /// </summary>
    public static TableColumnDefinition Number(string title, string propertyName, string? format = "N2", string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Number,
            Format = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立貨幣欄位
    /// </summary>
    public static TableColumnDefinition Currency(string title, string propertyName, string? symbol = "NT$", string? format = "N2", string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Currency,
            CurrencySymbol = symbol,
            Format = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立日期欄位
    /// </summary>
    public static TableColumnDefinition Date(string title, string propertyName, string? format = "yyyy/MM/dd", string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Date,
            Format = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立日期時間欄位
    /// </summary>
    public static TableColumnDefinition DateTime(string title, string propertyName, string? format = "yyyy/MM/dd HH:mm", string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.DateTime,
            Format = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立布林值欄位
    /// </summary>
    public static TableColumnDefinition Boolean(string title, string propertyName, string? trueText = "是", string? falseText = "否", string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Boolean,
            TrueText = trueText,
            FalseText = falseText,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立狀態欄位
    /// </summary>
    public static TableColumnDefinition Status(string title, string propertyName, Dictionary<object, string> statusBadgeMap, string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = propertyName,
            DataType = ColumnDataType.Status,
            StatusBadgeMap = statusBadgeMap,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立自定義範本欄位
    /// </summary>
    public static TableColumnDefinition Template(string title, RenderFragment<object> template, string? cssClass = null)
    {
        return new TableColumnDefinition
        {
            Title = title,
            PropertyName = "",
            CustomTemplate = template,
            CellCssClass = cssClass
        };
    }
    
    #endregion
}

/// <summary>
/// 資料欄位類型列舉
/// </summary>
public enum ColumnDataType
{
    /// <summary>
    /// 文字
    /// </summary>
    Text,
    
    /// <summary>
    /// 數值
    /// </summary>
    Number,
    
    /// <summary>
    /// 貨幣
    /// </summary>
    Currency,
    
    /// <summary>
    /// 日期
    /// </summary>
    Date,
    
    /// <summary>
    /// 日期時間
    /// </summary>
    DateTime,
    
    /// <summary>
    /// 布林值
    /// </summary>
    Boolean,
    
    /// <summary>
    /// 狀態 (會顯示為徽章)
    /// </summary>
    Status,
    
    /// <summary>
    /// HTML 內容
    /// </summary>
    Html
}

/// <summary>
/// 表格大小枚舉
/// </summary>
public enum TableSize
{
    /// <summary>
    /// 小尺寸
    /// </summary>
    Small,
    
    /// <summary>
    /// 正常尺寸
    /// </summary>
    Normal,
    
    /// <summary>
    /// 大尺寸
    /// </summary>
    Large
}
