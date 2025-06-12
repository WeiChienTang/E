namespace ERPCore2.Components.Shared.Details;

/// <summary>
/// 詳細檢視配置類別 - 描述整個詳細檢視的結構
/// </summary>
public class DetailViewConfiguration
{
    /// <summary>
    /// 整體標題
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 副標題
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 標題圖示
    /// </summary>
    public string TitleIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// 區塊/分頁列表
    /// </summary>
    public List<DetailSection> Sections { get; set; } = new();
    
    /// <summary>
    /// 是否使用分頁模式（預設為 true，false 為摺疊模式）
    /// </summary>
    public bool UseTabs { get; set; } = true;
    
    /// <summary>
    /// 載入狀態
    /// </summary>
    public bool IsLoading { get; set; } = false;
    
    /// <summary>
    /// 載入文字
    /// </summary>
    public string LoadingText { get; set; } = "載入中...";
    
    /// <summary>
    /// CSS 類別
    /// </summary>
    public string CssClass { get; set; } = string.Empty;
}

/// <summary>
/// 詳細區塊類別 - 描述每個邏輯區塊
/// </summary>
public class DetailSection
{
    /// <summary>
    /// 區塊標題
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 區塊圖示
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    
    /// <summary>
    /// 區塊 ID（用於分頁識別）
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否需要延遲載入
    /// </summary>
    public bool LazyLoad { get; set; } = false;
    
    /// <summary>
    /// 載入狀態
    /// </summary>
    public bool IsLoading { get; set; } = false;
    
    /// <summary>
    /// 欄位項目列表
    /// </summary>
    public List<DetailItem> Items { get; set; } = new();
    
    /// <summary>
    /// 是否預設展開（摺疊模式下使用）
    /// </summary>
    public bool IsExpanded { get; set; } = true;
    
    /// <summary>
    /// 自訂內容範本
    /// </summary>
    public object? CustomData { get; set; }
    
    /// <summary>
    /// CSS 類別
    /// </summary>
    public string CssClass { get; set; } = string.Empty;
}

/// <summary>
/// 詳細項目類別 - 描述每個具體欄位
/// </summary>
public class DetailItem
{
    /// <summary>
    /// 標籤名稱
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 實際數值
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// 顯示類型
    /// </summary>
    public DetailDisplayType DisplayType { get; set; } = DetailDisplayType.Text;
    
    /// <summary>
    /// 格式設定
    /// </summary>
    public DetailItemFormat? Format { get; set; }
    
    /// <summary>
    /// 是否重點標示
    /// </summary>
    public bool IsHighlight { get; set; } = false;
    
    /// <summary>
    /// 容器 CSS 類別
    /// </summary>
    public string ContainerCssClass { get; set; } = "col-md-6";
    
    /// <summary>
    /// 標籤 CSS 類別
    /// </summary>
    public string LabelCssClass { get; set; } = string.Empty;
    
    /// <summary>
    /// 值 CSS 類別
    /// </summary>
    public string ValueCssClass { get; set; } = string.Empty;
    
    /// <summary>
    /// 點擊事件處理器鍵值（用於事件回調）
    /// </summary>
    public string? ClickHandler { get; set; }
    
    /// <summary>
    /// 是否可點擊
    /// </summary>
    public bool IsClickable { get; set; } = false;
    
    /// <summary>
    /// 連結 URL（當 DisplayType 為 Link 時）
    /// </summary>
    public string? LinkUrl { get; set; }
    
    /// <summary>
    /// 連結目標（_blank, _self 等）
    /// </summary>
    public string LinkTarget { get; set; } = "_self";
}

/// <summary>
/// 詳細項目格式設定
/// </summary>
public class DetailItemFormat
{
    /// <summary>
    /// 日期格式
    /// </summary>
    public string DateFormat { get; set; } = "yyyy/MM/dd";
    
    /// <summary>
    /// 日期時間格式
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm";
    
    /// <summary>
    /// 數字小數位數
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;
    
    /// <summary>
    /// 貨幣符號
    /// </summary>
    public string CurrencySymbol { get; set; } = "NT$";
    
    /// <summary>
    /// 是否顯示千分位
    /// </summary>
    public bool ShowThousandsSeparator { get; set; } = true;
    
    /// <summary>
    /// 狀態顏色對應（狀態值 -> CSS 類別）
    /// </summary>
    public Dictionary<string, string> StatusColors { get; set; } = new();
    
    /// <summary>
    /// 清單分隔符號
    /// </summary>
    public string ListSeparator { get; set; } = ", ";
    
    /// <summary>
    /// 清單項目範本
    /// </summary>
    public string? ListItemTemplate { get; set; }
}

/// <summary>
/// 顯示類型枚舉
/// </summary>
public enum DetailDisplayType
{
    /// <summary>
    /// 純文字
    /// </summary>
    Text,
    
    /// <summary>
    /// 數字
    /// </summary>
    Number,
    
    /// <summary>
    /// 金額
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
    /// 狀態標籤
    /// </summary>
    Status,
    
    /// <summary>
    /// 連結
    /// </summary>
    Link,
    
    /// <summary>
    /// 清單
    /// </summary>
    List,
    
    /// <summary>
    /// 布林值（是/否）
    /// </summary>
    Boolean,
    
    /// <summary>
    /// 百分比
    /// </summary>
    Percentage,
    
    /// <summary>
    /// 電子郵件
    /// </summary>
    Email,
    
    /// <summary>
    /// 電話號碼
    /// </summary>
    Phone,
    
    /// <summary>
    /// 自訂範本
    /// </summary>
    Custom
}
