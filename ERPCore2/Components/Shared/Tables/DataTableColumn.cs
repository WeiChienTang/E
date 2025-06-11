using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Tables;

/// <summary>
/// 資料表格欄位定義類別
/// </summary>
public class DataTableColumn<TItem>
{
    /// <summary>
    /// 欄位標題
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 對應的屬性名稱，支援巢狀屬性 (例如: "CustomerType.TypeName")
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 資料類型
    /// </summary>
    public DataColumnType DataType { get; set; } = DataColumnType.Text;
    
    /// <summary>
    /// 自定義單元格範本
    /// </summary>
    public RenderFragment<TItem>? CellTemplate { get; set; }
    
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
    /// 標題的圖示 CSS 類別
    /// </summary>
    public string? IconClass { get; set; }
    
    /// <summary>
    /// 當值為 null 時顯示的文字
    /// </summary>
    public string? NullDisplayText { get; set; } = "-";
    
    #region 數值格式化選項
    
    /// <summary>
    /// 數值格式化字串 (例如: "N2", "F1")
    /// </summary>
    public string? NumberFormat { get; set; }
    
    /// <summary>
    /// 貨幣符號 (預設: "NT$")
    /// </summary>
    public string? CurrencySymbol { get; set; } = "NT$";
    
    #endregion
    
    #region 日期格式化選項
    
    /// <summary>
    /// 日期格式化字串 (例如: "yyyy/MM/dd")
    /// </summary>
    public string? DateFormat { get; set; } = "yyyy/MM/dd";
    
    /// <summary>
    /// 日期時間格式化字串 (例如: "yyyy/MM/dd HH:mm")
    /// </summary>
    public string? DateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm";
    
    #endregion
    
    #region 布林值顯示選項
    
    /// <summary>
    /// 布林值為 true 時顯示的文字
    /// </summary>
    public string? TrueText { get; set; } = "是";
    
    /// <summary>
    /// 布林值為 false 時顯示的文字
    /// </summary>
    public string? FalseText { get; set; } = "否";
    
    #endregion
    
    #region 列舉和狀態顯示選項
    
    /// <summary>
    /// 列舉值的顯示文字對應表
    /// </summary>
    public Dictionary<object, string>? EnumDisplayMap { get; set; }
    
    /// <summary>
    /// 狀態值的徽章樣式對應表 (值 -> Bootstrap 徽章 CSS 類別)
    /// </summary>
    public Dictionary<object, string>? StatusBadgeMap { get; set; }
    
    #endregion
    
    /// <summary>
    /// 建立文字欄位
    /// </summary>
    public static DataTableColumn<TItem> Text(string title, string propertyName, string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Text,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立數值欄位
    /// </summary>
    public static DataTableColumn<TItem> Number(string title, string propertyName, string? format = "N2", string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Number,
            NumberFormat = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立貨幣欄位
    /// </summary>
    public static DataTableColumn<TItem> Currency(string title, string propertyName, string? symbol = "NT$", string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Currency,
            CurrencySymbol = symbol,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立日期欄位
    /// </summary>
    public static DataTableColumn<TItem> Date(string title, string propertyName, string? format = "yyyy/MM/dd", string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Date,
            DateFormat = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立日期時間欄位
    /// </summary>
    public static DataTableColumn<TItem> DateTime(string title, string propertyName, string? format = "yyyy/MM/dd HH:mm", string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.DateTime,
            DateTimeFormat = format,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立布林值欄位
    /// </summary>
    public static DataTableColumn<TItem> Boolean(string title, string propertyName, string? trueText = "是", string? falseText = "否", string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Boolean,
            TrueText = trueText,
            FalseText = falseText,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立狀態欄位
    /// </summary>
    public static DataTableColumn<TItem> Status(string title, string propertyName, Dictionary<object, string>? badgeMap = null, string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = propertyName,
            DataType = DataColumnType.Status,
            StatusBadgeMap = badgeMap,
            CellCssClass = cssClass
        };
    }
    
    /// <summary>
    /// 建立自定義範本欄位
    /// </summary>
    public static DataTableColumn<TItem> Template(string title, RenderFragment<TItem> template, string? cssClass = null)
    {
        return new DataTableColumn<TItem>
        {
            Title = title,
            PropertyName = "",
            CellTemplate = template,
            CellCssClass = cssClass
        };
    }
}

/// <summary>
/// 資料欄位類型列舉
/// </summary>
public enum DataColumnType
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
    /// 列舉
    /// </summary>
    Enum,
    
    /// <summary>
    /// 狀態 (會顯示為徽章)
    /// </summary>
    Status,
    
    /// <summary>
    /// HTML 內容
    /// </summary>
    Html
}
