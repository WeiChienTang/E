using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Buttons;

namespace ERPCore2.Components.Shared.SubCollections
{
    /// <summary>
    /// 互動表格下拉選單選項模型
    /// </summary>
    public class InteractiveSelectOption
    {
        public object Value { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public bool IsDisabled { get; set; } = false;
    }

    /// <summary>
    /// 互動表格欄位定義
    /// </summary>
    public class InteractiveColumnDefinition
    {
        #region 基本屬性
        /// <summary>
        /// 欄位標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 資料屬性名稱
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;
        
        /// <summary>
        /// 控件類型
        /// </summary>
        public InteractiveColumnType ColumnType { get; set; } = InteractiveColumnType.Display;
        
        /// <summary>
        /// 欄位寬度 (CSS width 值)
        /// </summary>
        public string? Width { get; set; }
        
        /// <summary>
        /// 標題圖示 CSS 類別
        /// </summary>
        public string? IconClass { get; set; }
        
        /// <summary>
        /// 欄位標題 CSS 類別
        /// </summary>
        public string? HeaderCssClass { get; set; }
        
        /// <summary>
        /// 欄位內容 CSS 類別
        /// </summary>
        public string? CellCssClass { get; set; }
        
        /// <summary>
        /// 是否在手機版隱藏
        /// </summary>
        public bool HideOnMobile { get; set; } = false;
        #endregion

        #region 控件共用屬性
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; } = false;
        
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; } = false;
        
        /// <summary>
        /// 佔位符文字
        /// </summary>
        public string? Placeholder { get; set; }
        
        /// <summary>
        /// 工具提示
        /// </summary>
        public string? Tooltip { get; set; }
        #endregion

        #region Input/Number 專用屬性
        /// <summary>
        /// 輸入值變更事件
        /// </summary>
        public EventCallback<(object item, string? value)>? OnInputChanged { get; set; }
        
        /// <summary>
        /// 驗證規則 (正規表達式)
        /// </summary>
        public string? ValidationPattern { get; set; }
        
        /// <summary>
        /// 最小值 (數字類型)
        /// </summary>
        public decimal? MinValue { get; set; }
        
        /// <summary>
        /// 最大值 (數字類型)
        /// </summary>
        public decimal? MaxValue { get; set; }
        
        /// <summary>
        /// 步進值 (數字類型)
        /// </summary>
        public decimal? Step { get; set; }
        
        /// <summary>
        /// 是否唯讀
        /// </summary>
        public bool IsReadOnly { get; set; } = false;
        #endregion

        #region Select 專用屬性
        /// <summary>
        /// 下拉選項列表
        /// </summary>
        public List<InteractiveSelectOption>? Options { get; set; }
        
        /// <summary>
        /// 選項變更事件
        /// </summary>
        public EventCallback<(object item, object? value)>? OnSelectionChanged { get; set; }
        
        /// <summary>
        /// 是否支援多選
        /// </summary>
        public bool IsMultiSelect { get; set; } = false;
        #endregion

        #region Checkbox 專用屬性
        /// <summary>
        /// 勾選狀態變更事件
        /// </summary>
        public EventCallback<(object item, bool isChecked)>? OnCheckboxChanged { get; set; }
        
        /// <summary>
        /// 勾選時顯示文字
        /// </summary>
        public string? CheckedText { get; set; }
        
        /// <summary>
        /// 未勾選時顯示文字
        /// </summary>
        public string? UncheckedText { get; set; }
        #endregion

        #region Button 專用屬性
        /// <summary>
        /// 按鈕文字
        /// </summary>
        public string? ButtonText { get; set; }
        
        /// <summary>
        /// 按鈕圖示 CSS 類別
        /// </summary>
        public string? ButtonIcon { get; set; }
        
        /// <summary>
        /// 按鈕樣式
        /// </summary>
        public ButtonVariant ButtonVariant { get; set; } = ButtonVariant.Primary;
        
        /// <summary>
        /// 按鈕尺寸
        /// </summary>
        public ButtonSize ButtonSize { get; set; } = ButtonSize.Small;
        
        /// <summary>
        /// 按鈕點擊事件
        /// </summary>
        public EventCallback<object>? OnButtonClick { get; set; }
        
        /// <summary>
        /// 按鈕是否禁用的判斷函數
        /// </summary>
        public Func<object, bool>? IsButtonDisabled { get; set; }
        #endregion

        #region Display 專用屬性
        /// <summary>
        /// 顯示格式化函數
        /// </summary>
        public Func<object?, string>? DisplayFormatter { get; set; }
        
        /// <summary>
        /// 空值顯示文字
        /// </summary>
        public string? NullDisplayText { get; set; }
        #endregion

        #region Custom 專用屬性
        /// <summary>
        /// 自訂模板
        /// </summary>
        public RenderFragment<object>? CustomTemplate { get; set; }
        #endregion
    }
}
