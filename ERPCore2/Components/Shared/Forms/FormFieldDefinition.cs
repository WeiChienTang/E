using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 表單字段配置定義
/// </summary>
public class FormFieldDefinition
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
    /// 字段類型
    /// </summary>
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;
    
    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// 是否唯讀
    /// </summary>
    public bool IsReadOnly { get; set; } = false;
    
    /// <summary>
    /// 是否隱藏
    /// </summary>
    public bool IsHidden { get; set; } = false;
    
    /// <summary>
    /// CSS 類別
    /// </summary>
    public string? CssClass { get; set; }
    
    /// <summary>
    /// 欄位寬度（Bootstrap 格線系統）
    /// </summary>
    public int ColumnWidth { get; set; } = 12;
    
    /// <summary>
    /// 排序順序
    /// </summary>
    public int Order { get; set; } = 0;
    
    /// <summary>
    /// 下拉選項資料源（用於 Select 類型）
    /// </summary>
    public IEnumerable<SelectOption>? SelectOptions { get; set; }
    
    /// <summary>
    /// 自定義範本
    /// </summary>
    public RenderFragment<object>? CustomTemplate { get; set; }
    
    /// <summary>
    /// 驗證規則
    /// </summary>
    public List<ValidationAttribute>? ValidationRules { get; set; }
    
    /// <summary>
    /// 最小值（數字類型使用）
    /// </summary>
    public double? MinValue { get; set; }
    
    /// <summary>
    /// 最大值（數字類型使用）
    /// </summary>
    public double? MaxValue { get; set; }
    
    /// <summary>
    /// 最大長度（文字類型使用）
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// 文字區域的行數
    /// </summary>
    public int TextAreaRows { get; set; } = 3;
    
    /// <summary>
    /// 說明文字
    /// </summary>
    public string? HelpText { get; set; }
    
    /// <summary>
    /// 是否顯示在同一行
    /// </summary>
    public bool InlinePrevious { get; set; } = false;
}

/// <summary>
/// 表單字段類型
/// </summary>
public enum FormFieldType
{
    /// <summary>
    /// 文字輸入
    /// </summary>
    Text,
    
    /// <summary>
    /// 密碼輸入
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
    /// 下拉選單
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
    /// 單選按鈕群組
    /// </summary>
    Radio,
    
    /// <summary>
    /// 檔案上傳
    /// </summary>
    File,
    
    /// <summary>
    /// 隱藏字段
    /// </summary>
    Hidden,
    
    /// <summary>
    /// 自定義範本
    /// </summary>
    Custom
}

/// <summary>
/// 選項資料
/// </summary>
public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
    public bool IsDisabled { get; set; } = false;
    public object? Data { get; set; }
}
