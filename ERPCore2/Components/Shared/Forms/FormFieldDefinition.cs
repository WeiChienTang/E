using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 表單欄位定義
/// </summary>
public class FormFieldDefinition
{
    /// <summary>
    /// 屬性名稱
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 欄位標籤
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 欄位類型
    /// </summary>
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;
    
    /// <summary>
    /// 佔位符文字
    /// </summary>
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// 說明文字
    /// </summary>
    public string? HelpText { get; set; }
    
    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// 是否唯讀
    /// </summary>
    public bool IsReadOnly { get; set; } = false;
    
    /// <summary>
    /// 是否停用
    /// </summary>
    public bool IsDisabled { get; set; } = false;
    
    /// <summary>
    /// 自定義 CSS 類別
    /// </summary>
    public string? CssClass { get; set; }
    
    /// <summary>
    /// 容器 CSS 類別
    /// </summary>
    public string? ContainerCssClass { get; set; }
    
    /// <summary>
    /// 最小值 (數字欄位)
    /// </summary>
    public decimal? Min { get; set; }
    
    /// <summary>
    /// 最大值 (數字欄位)
    /// </summary>
    public decimal? Max { get; set; }
    
    /// <summary>
    /// 步長 (數字欄位)
    /// </summary>
    public decimal? Step { get; set; }
    
    /// <summary>
    /// 最小長度 (文字欄位)
    /// </summary>
    public int? MinLength { get; set; }
    
    /// <summary>
    /// 最大長度 (文字欄位)
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// 文字區域列數
    /// </summary>
    public int? Rows { get; set; }
      /// <summary>
    /// 選項清單 (選擇欄位)
    /// </summary>
    public List<SelectOption>? Options { get; set; }
    
    /// <summary>
    /// 自動完成搜尋函式 (AutoComplete 欄位)
    /// </summary>
    public Func<string, Task<List<SelectOption>>>? SearchFunction { get; set; }
    
    /// <summary>
    /// 自動完成延遲毫秒 (AutoComplete 欄位)
    /// </summary>
    public int AutoCompleteDelayMs { get; set; } = 300;
    
    /// <summary>
    /// 自動完成最小搜尋字符數 (AutoComplete 欄位)
    /// </summary>
    public int MinSearchLength { get; set; } = 1;
    
    /// <summary>
    /// 預設值
    /// </summary>
    public object? DefaultValue { get; set; }
    
    /// <summary>
    /// 驗證規則
    /// </summary>
    public List<ValidationRule>? ValidationRules { get; set; }
    
    /// <summary>
    /// 排序順序
    /// </summary>
    public int Order { get; set; } = 0;
    
    /// <summary>
    /// 分組名稱
    /// </summary>
    public string? GroupName { get; set; }
    
    /// <summary>
    /// 標籤旁邊的操作按鈕
    /// </summary>
    public List<FieldActionButton>? ActionButtons { get; set; }
}

/// <summary>
/// 表單欄位類型
/// </summary>
public enum FormFieldType
{
    /// <summary>
    /// 文字
    /// </summary>
    Text,
    
    /// <summary>
    /// 密碼
    /// </summary>
    Password,
    
    /// <summary>
    /// 電子郵件
    /// </summary>
    Email,
    
    /// <summary>
    /// 數字
    /// </summary>
    Number,
    
    /// <summary>
    /// 日期
    /// </summary>
    Date,
    
    /// <summary>
    /// 日期時間
    /// </summary>
    DateTime,
    
    /// <summary>
    /// 時間
    /// </summary>
    Time,
    
    /// <summary>
    /// 多行文字
    /// </summary>
    TextArea,
    
    /// <summary>
    /// 下拉選擇
    /// </summary>
    Select,
    
    /// <summary>
    /// 多選下拉
    /// </summary>
    MultiSelect,
    
    /// <summary>
    /// 核取方塊
    /// </summary>
    Checkbox,
    
    /// <summary>
    /// 單選按鈕
    /// </summary>
    Radio,
    
    /// <summary>
    /// 檔案上傳
    /// </summary>
    File,
    
    /// <summary>
    /// 隱藏欄位
    /// </summary>
    Hidden,
      /// <summary>
    /// 自動完成
    /// </summary>
    AutoComplete,
    
    /// <summary>
    /// 自定義
    /// </summary>
    Custom
}

/// <summary>
/// 選項類別
/// </summary>
public class SelectOption
{
    /// <summary>
    /// 選項值
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// 顯示文字
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否停用
    /// </summary>
    public bool IsDisabled { get; set; } = false;
    
    /// <summary>
    /// 分組名稱
    /// </summary>
    public string? GroupName { get; set; }
}

/// <summary>
/// 驗證規則類別
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// 規則類型
    /// </summary>
    public ValidationType Type { get; set; }
    
    /// <summary>
    /// 規則值
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// 驗證類型
/// </summary>
public enum ValidationType
{
    /// <summary>
    /// 必填
    /// </summary>
    Required,
    
    /// <summary>
    /// 最小長度
    /// </summary>
    MinLength,
    
    /// <summary>
    /// 最大長度
    /// </summary>
    MaxLength,
    
    /// <summary>
    /// 最小值
    /// </summary>
    Min,
    
    /// <summary>
    /// 最大值
    /// </summary>
    Max,
    
    /// <summary>
    /// 正規表達式
    /// </summary>
    Pattern,
    
    /// <summary>
    /// 電子郵件格式
    /// </summary>
    Email,
    
    /// <summary>
    /// 自定義驗證
    /// </summary>
    Custom
}

/// <summary>
/// 欄位操作按鈕定義
/// </summary>
public class FieldActionButton
{
    /// <summary>
    /// 按鈕文字
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// 按鈕樣式變體
    /// </summary>
    public string Variant { get; set; } = "OutlinePrimary";
    
    /// <summary>
    /// 按鈕大小
    /// </summary>
    public string Size { get; set; } = "Small";
    
    /// <summary>
    /// 按鈕圖示 CSS 類別
    /// </summary>
    public string? IconClass { get; set; }
    
    /// <summary>
    /// 按鈕標題 (用於 tooltip)
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 按鈕點擊事件回調
    /// </summary>
    public Func<Task>? OnClick { get; set; }
    
    /// <summary>
    /// 是否停用
    /// </summary>
    public bool IsDisabled { get; set; } = false;
}
