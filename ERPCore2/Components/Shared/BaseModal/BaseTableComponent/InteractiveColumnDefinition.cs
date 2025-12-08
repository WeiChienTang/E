using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.GenericComponent.Button;

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
        /// 是否禁用（靜態設定）
        /// </summary>
        public bool IsDisabled { get; set; } = false;
        
        /// <summary>
        /// 動態判斷是否禁用的函數（優先於 IsDisabled）
        /// </summary>
        public Func<object, bool>? IsDisabledFunc { get; set; }
        
        /// <summary>
        /// 佔位符文字
        /// </summary>
        public string? Placeholder { get; set; }
        
        /// <summary>
        /// 工具提示
        /// </summary>
        public string? Tooltip { get; set; }
        
        /// <summary>
        /// 動態工具提示函數（用於根據項目狀態顯示不同提示）
        /// </summary>
        public Func<object, string?>? TooltipFunc { get; set; }
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
        public ButtonVariant ButtonVariant { get; set; } = ButtonVariant.DarkBlue;
        
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
        /// 參數為整個資料列物件，可以訪問該物件的所有屬性和導航屬性
        /// 返回要顯示的 HTML 字串（支援 HTML 標籤）
        /// 範例：item => { var entity = item as MyEntity; return $"[{entity.Code}] {entity.Name}"; }
        /// </summary>
        public Func<object?, string>? DisplayFormatter { get; set; }
        
        /// <summary>
        /// 空值顯示文字
        /// </summary>
        public string? NullDisplayText { get; set; }
        
        /// <summary>
        /// 文字對齊方式 (left, center, right)，預設為 left
        /// </summary>
        public string TextAlign { get; set; } = "left";
        
        /// <summary>
        /// 數字格式化字串（例如："N0" 整數千分位、"N2" 兩位小數、"C" 貨幣、"P" 百分比等）
        /// 參考：https://docs.microsoft.com/zh-tw/dotnet/standard/base-types/standard-numeric-format-strings
        /// </summary>
        public string? NumberFormat { get; set; }
        
        /// <summary>
        /// 是否智能顯示小數點（整數不顯示小數點，有小數才顯示）
        /// 當設為 true 時：
        /// - 整數（如 100）顯示為 "100"
        /// - 小數（如 100.50）顯示為 "100.50" 或根據 NumberFormat 設定顯示
        /// 優先級：DisplayFormatter > SmartDecimalDisplay > NumberFormat > 預設顯示
        /// </summary>
        public bool SmartDecimalDisplay { get; set; } = false;
        #endregion

        #region SearchableSelect 專用屬性
        /// <summary>
        /// 搜尋值屬性名稱（用於可搜尋下拉選單）
        /// </summary>
        public string? SearchValuePropertyName { get; set; }
        
        /// <summary>
        /// 選中項目屬性名稱（用於可搜尋下拉選單）
        /// </summary>
        public string? SelectedItemPropertyName { get; set; }
        
        /// <summary>
        /// 過濾項目列表屬性名稱（用於可搜尋下拉選單）
        /// </summary>
        public string? FilteredItemsPropertyName { get; set; }
        
        /// <summary>
        /// 顯示下拉選單屬性名稱（用於可搜尋下拉選單）
        /// </summary>
        public string? ShowDropdownPropertyName { get; set; }
        
        /// <summary>
        /// 選中索引屬性名稱（用於可搜尋下拉選單）
        /// </summary>
        public string? SelectedIndexPropertyName { get; set; }
        
        /// <summary>
        /// 所有可用項目的資料來源函數
        /// </summary>
        public Func<IEnumerable<object>>? AvailableItemsProvider { get; set; }
        
        /// <summary>
        /// 項目顯示格式化函數（用於顯示 Code - Name 格式）
        /// </summary>
        public Func<object, string>? ItemDisplayFormatter { get; set; }
        
        /// <summary>
        /// 搜尋過濾函數
        /// </summary>
        public Func<object, string, bool>? SearchFilter { get; set; }
        
        /// <summary>
        /// 搜尋輸入變更事件
        /// </summary>
        public EventCallback<(object item, string? searchValue)>? OnSearchInputChanged { get; set; }
        
        /// <summary>
        /// 項目選擇事件
        /// </summary>
        public EventCallback<(object item, object? selectedItem)>? OnItemSelected { get; set; }
        
        /// <summary>
        /// 輸入框焦點事件
        /// </summary>
        public EventCallback<object>? OnInputFocus { get; set; }
        
        /// <summary>
        /// 輸入框失焦事件
        /// </summary>
        public EventCallback<object>? OnInputBlur { get; set; }
        
        /// <summary>
        /// 項目滑鼠移入事件
        /// </summary>
        public EventCallback<(object item, int index)>? OnItemMouseEnter { get; set; }
        
        /// <summary>
        /// 最大顯示項目數量（預設20）
        /// </summary>
        public int MaxDisplayItems { get; set; } = 20;
        
        /// <summary>
        /// 下拉選單最大高度（CSS值，預設 200px）
        /// </summary>
        public string DropdownMaxHeight { get; set; } = "200px";
        
        /// <summary>
        /// 下拉選單最小寬度（CSS值，預設 300px）
        /// </summary>
        public string DropdownMinWidth { get; set; } = "300px";
        
        /// <summary>
        /// 下拉選單最大寬度（CSS值，預設 500px）
        /// </summary>
        public string DropdownMaxWidth { get; set; } = "500px";
        #endregion

        #region Custom 專用屬性
        /// <summary>
        /// 自訂模板
        /// </summary>
        public RenderFragment<object>? CustomTemplate { get; set; }
        #endregion

        #region 自動空行檢測專用屬性
        /// <summary>
        /// 用於空行檢測的屬性名稱
        /// 當 ColumnType 為 Custom 且 PropertyName 為空時，指定要檢查的實際屬性
        /// 例如：商品選擇欄位的 PropertyName 是空的，但實際要檢查 "SelectedProduct"
        /// </summary>
        public string? EmptyCheckPropertyName { get; set; }
        
        /// <summary>
        /// 是否從空行檢測中排除此欄位
        /// 設為 true 表示此欄位不參與空行判斷（例如：備註、唯讀欄位）
        /// </summary>
        public bool ExcludeFromEmptyCheck { get; set; } = false;
        
        /// <summary>
        /// 當此欄位從空值變為有值時，自動觸發新增空行
        /// 設為 true 表示這是「關鍵欄位」，填入值後就應該準備下一個空行
        /// 例如：商品選擇欄位，選了商品就算一筆，應該立即新增空行
        /// </summary>
        public bool TriggerEmptyRowOnFilled { get; set; } = false;
        #endregion

        #region 鍵盤導航專用屬性
        /// <summary>
        /// 是否啟用鍵盤導航（適用於下拉選單相關欄位）
        /// </summary>
        public bool EnableKeyboardNavigation { get; set; } = false;
        
        /// <summary>
        /// 下拉選單項目列表（用於鍵盤導航）
        /// </summary>
        public Func<object, IEnumerable<object>>? GetDropdownItems { get; set; }
        
        /// <summary>
        /// 下拉選單顯示格式化函數
        /// </summary>
        public Func<object, string>? DropdownDisplayFormatter { get; set; }
        
        /// <summary>
        /// 下拉選單項目選擇事件
        /// </summary>
        public EventCallback<(object item, object? selectedItem)>? OnDropdownItemSelected { get; set; }
        
        /// <summary>
        /// 取得當前選中索引的函數
        /// </summary>
        public Func<object, int>? GetSelectedIndex { get; set; }
        
        /// <summary>
        /// 設定選中索引的函數
        /// </summary>
        public Action<object, int>? SetSelectedIndex { get; set; }
        
        /// <summary>
        /// 判斷是否顯示下拉選單的函數
        /// </summary>
        public Func<object, bool>? GetShowDropdown { get; set; }
        
        /// <summary>
        /// 設定下拉選單顯示狀態的函數
        /// </summary>
        public Action<object, bool>? SetShowDropdown { get; set; }
        
        /// <summary>
        /// 下拉選單容器ID格式（用於定位和捲動）
        /// </summary>
        public string? DropdownContainerIdFormat { get; set; }
        #endregion
        
        #region 原始 Callback 存儲 - 用於 SearchableSelectHelper
        /// <summary>
        /// 原始搜尋輸入變更回調
        /// </summary>
        internal EventCallback<(object item, string? searchValue)>? SearchInputChangedCallback { get; set; }
        
        /// <summary>
        /// 原始項目選擇回調
        /// </summary>
        internal EventCallback<(object item, object? selectedItem)>? ItemSelectedCallback { get; set; }
        
        /// <summary>
        /// 原始輸入框焦點回調
        /// </summary>
        internal EventCallback<object>? InputFocusCallback { get; set; }
        
        /// <summary>
        /// 原始輸入框失焦回調
        /// </summary>
        internal EventCallback<object>? InputBlurCallback { get; set; }
        
        /// <summary>
        /// 原始項目滑鼠移入回調
        /// </summary>
        internal EventCallback<(object item, int index)>? ItemMouseEnterCallback { get; set; }
        #endregion
        
        #region Action 委託 - 用於 SearchableSelectHelper 類型轉換
        /// <summary>
        /// 搜尋輸入變更委託
        /// </summary>
        internal Action<object, string?>? SearchInputChangedAction { get; set; }
        
        /// <summary>
        /// 項目選擇委託
        /// </summary>
        internal Action<object, object?>? ItemSelectedAction { get; set; }
        
        /// <summary>
        /// 輸入框焦點委託
        /// </summary>
        internal Action<object>? InputFocusAction { get; set; }
        
        /// <summary>
        /// 輸入框失焦委託
        /// </summary>
        internal Action<object>? InputBlurAction { get; set; }
        
        /// <summary>
        /// 項目滑鼠移入委託
        /// </summary>
        internal Action<object, int>? ItemMouseEnterAction { get; set; }
        #endregion
    }
}
