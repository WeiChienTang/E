using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ERPCore2.Helpers;
using ERPCore2.Helpers.EditModal;

namespace ERPCore2.Components.Shared.UI.Form;

/// <summary>
/// 通用表單組件 - 基於配置驅動的動態表單
/// Code-behind 檔案
/// </summary>
public partial class GenericFormComponent<TModel> : ComponentBase, IDisposable
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    protected ILogger<GenericFormComponent<TModel>> Logger { get; set; } = default!;

    #region Parameters

    [Parameter] 
    public TModel Model { get; set; } = default!;

    [Parameter] 
    public List<FormFieldDefinition>? FieldDefinitions { get; set; }

    [Parameter] 
    public Dictionary<string, string>? FieldSections { get; set; }

    [Parameter] 
    public EventCallback<TModel> OnFormSubmit { get; set; }

    [Parameter] 
    public EventCallback OnCancel { get; set; }

    [Parameter] 
    public EventCallback<(string PropertyName, object? Value)> OnFieldChanged { get; set; }

    [Parameter] 
    public bool IsSubmitting { get; set; } = false;

    [Parameter] 
    public bool ShowValidationSummary { get; set; } = true;

    [Parameter] 
    public string FieldContainerCssClass { get; set; } = "col-md-2";

    /// <summary>
    /// Tab 頁籤定義清單 - 定義每個 Tab 包含哪些 Section
    /// 有提供時啟用 Tab 佈局；為 null 時使用現有的水平並排 column 佈局
    /// </summary>
    [Parameter]
    public List<FormTabDefinition>? TabDefinitions { get; set; }

    #endregion

    #region Private Fields

    protected EditForm? editForm;
    
    // Tab 相關狀態
    protected int _activeTabIndex = 0;
    private TModel? _previousModel;

    // AutoComplete 狀態管理器（取代原本的多個 Dictionary）
    protected readonly AutoCompleteStateManager _autoCompleteStates = new();

    // 追蹤已初始化的數字輸入欄位
    protected HashSet<string> initializedNumberInputs = new();

    #endregion

    #region Lifecycle

    protected override void OnParametersSet()
    {
        // Model 變更時重置 Tab 索引（例如上下筆切換）
        if (_previousModel != null && !ReferenceEquals(_previousModel, Model))
        {
            _activeTabIndex = 0;
        }
        _previousModel = Model;

        InitializeAutoCompleteDisplayValues();
        ApplyDefaultValues();
        base.OnParametersSet();
    }

    /// <summary>
    /// 初始化 AutoComplete 欄位的顯示值
    /// </summary>
    private void InitializeAutoCompleteDisplayValues()
    {
        if (FieldDefinitions == null || Model == null) return;

        foreach (var field in FieldDefinitions.Where(f => f.FieldType == FormFieldType.AutoComplete))
        {
            var fieldId = field.PropertyName;
            var state = _autoCompleteStates.GetOrCreate(fieldId);
            var value = GetPropertyValue(Model, field.PropertyName);

            if (value != null && string.IsNullOrEmpty(state.DisplayValue))
            {
                // 對於新加載的資料，先設置空字串
                state.DisplayValue = "";

                // 非同步查找顯示文字
                _ = LoadAutoCompleteDisplayValueAsync(field, fieldId, value.ToString());
            }
        }
    }

    /// <summary>
    /// 非同步載入 AutoComplete 顯示值
    /// </summary>
    private async Task LoadAutoCompleteDisplayValueAsync(FormFieldDefinition field, string fieldId, string? valueString)
    {
        if (field.SearchFunction == null || string.IsNullOrEmpty(valueString)) return;

        try
        {
            var options = await field.SearchFunction("");
            var matchingOption = options.FirstOrDefault(o => o.Value == valueString);
            if (matchingOption != null)
            {
                await InvokeAsync(() =>
                {
                    var state = _autoCompleteStates.GetOrCreate(fieldId);
                    state.DisplayValue = matchingOption.Text;
                    StateHasChanged();
                });
            }
        }
        catch
        {
            // 靜默處理，保持原來的顯示值
        }
    }

    /// <summary>
    /// 以程式方式設定 AutoComplete 欄位的值與顯示文字
    /// 供外部組件（如 EditModal）呼叫以實現跨欄位自動帶入
    /// </summary>
    public async Task SetAutoCompleteValueAsync(string propertyName, string value, string displayText)
    {
        var state = _autoCompleteStates.GetOrCreate(propertyName);
        state.DisplayValue = displayText;
        SetPropertyValueInternal(Model, propertyName, value);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// 套用欄位預設值
    /// </summary>
    private void ApplyDefaultValues()
    {
        if (FieldDefinitions == null || Model == null) return;

        foreach (var field in FieldDefinitions.Where(f => f.DefaultValue != null))
        {
            var currentValue = GetPropertyValue(Model, field.PropertyName);
            if (ShouldApplyDefaultValue(currentValue, field))
            {
                SetPropertyValueInternal(Model, field.PropertyName, field.DefaultValue);
            }
        }
    }

    /// <summary>
    /// 判斷是否應該應用預設值
    /// </summary>
    protected bool ShouldApplyDefaultValue(object? currentValue, FormFieldDefinition field)
    {
        if (currentValue == null) return true;
        if (currentValue is string str && string.IsNullOrEmpty(str)) return true;
        return false;
    }

    /// <summary>
    /// 初始化 Bootstrap Popover 以支援標籤問號說明
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        // 初始化 Popover (每次渲染都需要，因為可能有新的 Popover 元素)
        try
        {
            await JSRuntime.InvokeVoidAsync("popoverHelpers.initializeAll");
        }
        catch (JSDisconnectedException)
        {
            // 靜默處理斷線錯誤
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Popover initialization skipped: {Message}", ex.Message);
        }
    }

    public void Dispose()
    {
        _autoCompleteStates.Dispose();
    }

    #endregion

    #region Tab Management

    /// <summary>
    /// 按照 FieldDefinitions 中的欄位順序來排列區段
    /// </summary>
    protected IEnumerable<IGrouping<string, KeyValuePair<string, string>>> GetOrderedSections()
    {
        if (FieldDefinitions == null || FieldSections == null)
            return Enumerable.Empty<IGrouping<string, KeyValuePair<string, string>>>();

        var sectionOrderMap = new Dictionary<string, int>();
        var sectionOrder = 0;

        foreach (var field in FieldDefinitions)
        {
            if (FieldSections.TryGetValue(field.PropertyName, out var sectionName))
            {
                if (!sectionOrderMap.ContainsKey(sectionName))
                {
                    sectionOrderMap[sectionName] = sectionOrder++;
                }
            }
        }

        return FieldSections
            .GroupBy(kvp => kvp.Value)
            .OrderBy(g => sectionOrderMap.GetValueOrDefault(g.Key, int.MaxValue));
    }

    /// <summary>
    /// 設定當前啟用的 Tab 索引
    /// </summary>
    protected void SetActiveTab(int index)
    {
        _activeTabIndex = index;
        StateHasChanged();
    }

    #endregion

    #region Field Rendering Helpers

    /// <summary>
    /// 判斷欄位類型是否應該包裝 ActionButtons
    /// </summary>
    protected bool ShouldWrapWithActionButtons(FormFieldType fieldType)
    {
        return fieldType switch
        {
            FormFieldType.Text => true,
            FormFieldType.Email => true,
            FormFieldType.Number => true,
            FormFieldType.AutoComplete => true,
            FormFieldType.Select => true,
            FormFieldType.MobilePhone => true,
            _ => false
        };
    }

    /// <summary>
    /// 根據按鈕文字返回對應的圖示
    /// </summary>
    protected string GetActionButtonIcon(string buttonText)
    {
        return FormConstants.GetIconForButtonText(buttonText);
    }

    /// <summary>
    /// 根據 Variant 返回顏色 class
    /// </summary>
    protected string GetActionButtonColorClass(string variant)
    {
        return FormConstants.GetColorClassForVariant(variant);
    }

    protected string GetInputType(FormFieldType fieldType)
    {
        return fieldType switch
        {
            FormFieldType.Email => "email",
            FormFieldType.Password => "password",
            FormFieldType.Number => "number",
            FormFieldType.Date => "date",
            FormFieldType.DateTime => "datetime-local",
            FormFieldType.Time => "time",
            FormFieldType.MobilePhone => "tel",
            _ => "text"
        };
    }

    protected string GetInputCssClass(FormFieldDefinition field)
    {
        var classes = new List<string> { FormConstants.CssClasses.FormControl };
        if (!string.IsNullOrEmpty(field.CssClass))
            classes.Add(field.CssClass);
        return string.Join(" ", classes);
    }

    protected string GetAutocompleteValue(FormFieldDefinition field)
    {
        return !string.IsNullOrEmpty(field.AutoCompleteAttribute)
            ? field.AutoCompleteAttribute
            : (field.FieldType == FormFieldType.Password 
                ? FormConstants.AutoCompleteAttributes.NewPassword 
                : FormConstants.AutoCompleteAttributes.Off);
    }

    #endregion

    #region AutoComplete Event Handlers

    /// <summary>
    /// 處理 AutoComplete 輸入事件
    /// </summary>
    protected void HandleAutoCompleteInputEvent(FormFieldDefinition field, string inputValue)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        state.DisplayValue = inputValue;
        _ = HandleAutoCompleteInputAsync(field, inputValue);
    }

    /// <summary>
    /// 處理 AutoComplete 獲得焦點
    /// </summary>
    protected void HandleAutoCompleteFocus(FormFieldDefinition field)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        state.HasBlurred = false;

        var inputValue = state.DisplayValue;
        if (field.MinSearchLength == 0 || (!string.IsNullOrEmpty(inputValue) && inputValue.Length >= field.MinSearchLength))
        {
            _ = HandleAutoCompleteInputAsync(field, inputValue);
        }
    }

    /// <summary>
    /// 處理 AutoComplete 失去焦點
    /// </summary>
    protected async Task HandleAutoCompleteBlur(FormFieldDefinition field)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        state.HasBlurred = true;
        state.ClearTimer();
        state.IsLoading = false;

        await TrySmartMatchAsync(field);

        // 延遲隱藏下拉選單，讓點擊事件有時間觸發
        Timer? timer = null;
        timer = new Timer(_ =>
        {
            state.IsVisible = false;
            InvokeAsync(StateHasChanged);
            timer?.Dispose();
        }, null, 300, Timeout.Infinite);
    }

    /// <summary>
    /// 處理 AutoComplete 輸入（帶延遲搜尋）
    /// </summary>
    protected async Task HandleAutoCompleteInputAsync(FormFieldDefinition field, string inputValue)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);

        // 清除之前的計時器和載入狀態
        state.ClearTimer();
        state.IsLoading = false;

        // 如果欄位已經失去焦點，不要進行搜尋
        if (state.HasBlurred) return;

        // 如果輸入為空，清空實體值
        if (string.IsNullOrEmpty(inputValue))
        {
            var currentValue = GetPropertyValue(Model, field.PropertyName);
            if (currentValue != null)
            {
                SetPropertyValueInternal(Model, field.PropertyName, null);
                await NotifyFieldChanged(field.PropertyName, null);
            }

            if (field.MinSearchLength > 0)
            {
                state.IsVisible = false;
                StateHasChanged();
                return;
            }
        }

        // 如果輸入長度不足，隱藏下拉選單
        if (inputValue.Length < field.MinSearchLength)
        {
            state.IsVisible = false;
            StateHasChanged();
            return;
        }

        // 設置載入狀態
        state.IsLoading = true;
        StateHasChanged();

        // 設置延遲搜尋計時器
        state.SearchTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                try
                {
                    if (field.SearchFunction != null)
                    {
                        var results = await field.SearchFunction(inputValue);
                        state.Options = results;
                        state.IsVisible = results.Any();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "AutoComplete search error for field {FieldId}", fieldId);
                    state.Options = new List<SelectOption>();
                    state.IsVisible = false;
                }
                finally
                {
                    state.IsLoading = false;
                    StateHasChanged();
                }
            });
        }, null, field.AutoCompleteDelayMs, Timeout.Infinite);
    }

    /// <summary>
    /// 選擇 AutoComplete 選項
    /// </summary>
    protected async Task SelectAutoCompleteOption(FormFieldDefinition field, SelectOption option)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);

        state.DisplayValue = option.Text;
        SetPropertyValueInternal(Model, field.PropertyName, option.Value);
        state.HideDropdown();

        await NotifyFieldChanged(field.PropertyName, option.Value);
        StateHasChanged();
    }

    /// <summary>
    /// 處理鍵盤導航
    /// </summary>
    protected async Task HandleKeyDown(FormFieldDefinition field, KeyboardEventArgs args)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        var options = state.Options;

        switch (args.Key)
        {
            case "ArrowDown":
                if (!state.IsVisible)
                {
                    var inputValue = state.DisplayValue;
                    if (field.MinSearchLength == 0 || (!string.IsNullOrEmpty(inputValue) && inputValue.Length >= field.MinSearchLength))
                    {
                        await HandleAutoCompleteInputAsync(field, inputValue);
                    }
                }
                else if (options.Any())
                {
                    MoveHighlight(state, options, 1);
                }
                break;

            case "ArrowUp":
                if (state.IsVisible && options.Any())
                {
                    MoveHighlight(state, options, -1);
                }
                break;

            case "Enter":
                if (state.IsVisible && options.Any())
                {
                    if (options.Count == 1)
                    {
                        await SelectSingleOption(field, state, options[0]);
                    }
                    else
                    {
                        await SelectHighlightedOption(field, state, options);
                    }
                }
                break;

            case "Escape":
                state.HideDropdown();
                break;

            case "Tab":
                if (state.IsVisible && options.Count == 1)
                {
                    await SelectSingleOption(field, state, options[0]);
                }
                else
                {
                    await TrySmartMatchAsync(field);
                    state.HideDropdown();
                }
                break;
        }

        StateHasChanged();
    }

    private async Task SelectSingleOption(FormFieldDefinition field, AutoCompleteFieldState state, SelectOption option)
    {
        state.DisplayValue = option.Text;
        SetPropertyValueInternal(Model, field.PropertyName, option.Value);
        state.HideDropdown();
        await NotifyFieldChanged(field.PropertyName, option.Value);
    }

    private void MoveHighlight(AutoCompleteFieldState state, List<SelectOption> options, int direction)
    {
        if (!options.Any()) return;

        var newIndex = state.HighlightedIndex + direction;

        // 循環導航
        if (newIndex < 0)
            newIndex = options.Count - 1;
        else if (newIndex >= options.Count)
            newIndex = 0;

        state.HighlightedIndex = newIndex;
        state.IsKeyboardNavigating = true;

        // 滾動到可見區域
        _ = ScrollToHighlightedOptionAsync(state, newIndex);
    }

    private async Task ScrollToHighlightedOptionAsync(AutoCompleteFieldState state, int optionIndex)
    {
        try
        {
            // 需要 fieldId 來構建 element ID，這裡簡化處理
            await JSRuntime.InvokeVoidAsync("scrollToElement", $"option_{optionIndex}");
        }
        catch
        {
            // 靜默處理滾動錯誤
        }
    }

    private async Task SelectHighlightedOption(FormFieldDefinition field, AutoCompleteFieldState state, List<SelectOption> options)
    {
        if (state.HighlightedIndex >= 0 && state.HighlightedIndex < options.Count)
        {
            var selectedOption = options[state.HighlightedIndex];
            await SelectSingleOption(field, state, selectedOption);
        }
    }

    /// <summary>
    /// 嘗試智能匹配 AutoComplete 輸入值
    /// </summary>
    protected async Task TrySmartMatchAsync(FormFieldDefinition field)
    {
        var fieldId = field.PropertyName;
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        var inputValue = state.DisplayValue.Trim();

        if (string.IsNullOrEmpty(inputValue))
        {
            var currentValue = GetPropertyValue(Model, field.PropertyName);
            if (currentValue != null)
            {
                SetPropertyValueInternal(Model, field.PropertyName, null);
                await NotifyFieldChanged(field.PropertyName, null);
            }
            return;
        }

        try
        {
            if (field.SearchFunction != null)
            {
                var results = await field.SearchFunction(inputValue);

                // 精確匹配優先
                var exactMatch = results.FirstOrDefault(r =>
                    string.Equals(r.Text, inputValue, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    state.DisplayValue = exactMatch.Text;
                    SetPropertyValueInternal(Model, field.PropertyName, exactMatch.Value);
                    await NotifyFieldChanged(field.PropertyName, exactMatch.Value);
                    return;
                }

                // 如果只有一個結果，也自動選擇
                if (results.Count == 1)
                {
                    var singleResult = results[0];
                    state.DisplayValue = singleResult.Text;
                    SetPropertyValueInternal(Model, field.PropertyName, singleResult.Value);
                    await NotifyFieldChanged(field.PropertyName, singleResult.Value);
                    return;
                }

                // 如果有輸入但找不到匹配，清空值
                var currentValue = GetPropertyValue(Model, field.PropertyName);
                if (currentValue != null)
                {
                    SetPropertyValueInternal(Model, field.PropertyName, null);
                    await NotifyFieldChanged(field.PropertyName, null);
                }
            }
        }
        catch
        {
            // 靜默處理錯誤
        }
    }

    /// <summary>
    /// 取得 AutoComplete 欄位的顯示值
    /// </summary>
    protected string GetAutoCompleteDisplayValue(string fieldId)
    {
        return _autoCompleteStates.TryGet(fieldId, out var state) 
            ? state?.DisplayValue ?? "" 
            : "";
    }

    /// <summary>
    /// 取得 AutoComplete 欄位是否正在載入
    /// </summary>
    protected bool GetAutoCompleteIsLoading(string fieldId)
    {
        return _autoCompleteStates.TryGet(fieldId, out var state) && (state?.IsLoading ?? false);
    }

    /// <summary>
    /// 取得 AutoComplete 欄位下拉選單是否可見
    /// </summary>
    protected bool GetAutoCompleteIsVisible(string fieldId)
    {
        return _autoCompleteStates.TryGet(fieldId, out var state) && (state?.IsVisible ?? false);
    }

    /// <summary>
    /// 取得 AutoComplete 欄位的選項
    /// </summary>
    protected List<SelectOption> GetAutoCompleteOptions(string fieldId)
    {
        return _autoCompleteStates.TryGet(fieldId, out var state) 
            ? state?.Options ?? new List<SelectOption>() 
            : new List<SelectOption>();
    }

    /// <summary>
    /// 取得 AutoComplete 欄位的高亮索引
    /// </summary>
    protected int GetAutoCompleteHighlightedIndex(string fieldId)
    {
        return _autoCompleteStates.TryGet(fieldId, out var state) 
            ? state?.HighlightedIndex ?? -1 
            : -1;
    }

    /// <summary>
    /// 設定 AutoComplete 欄位的高亮索引
    /// </summary>
    protected void SetAutoCompleteHighlightedIndex(string fieldId, int index)
    {
        var state = _autoCompleteStates.GetOrCreate(fieldId);
        state.HighlightedIndex = index;
        state.IsKeyboardNavigating = false;
    }

    #endregion

    #region Mobile Phone Handlers

    /// <summary>
    /// 處理台灣手機號碼輸入，使用 JS 互操作過濾非數字並自動格式化
    /// </summary>
    protected async Task HandleMobilePhoneInputWithJS(string elementId, string propertyName)
    {
        try
        {
            var digitsOnly = await JSRuntime.InvokeAsync<string>("formatMobilePhoneInput",
                await JSRuntime.InvokeAsync<IJSObjectReference>("eval", $"document.getElementById('{elementId}')"));

            SetPropertyValueInternal(Model, propertyName, string.IsNullOrEmpty(digitsOnly) ? null : digitsOnly);
        }
        catch
        {
            // JS 調用失敗時的回退處理
        }
    }

    /// <summary>
    /// 格式化手機號碼顯示為 0912-345-678 格式
    /// </summary>
    protected string FormatMobilePhoneDisplay(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(digitsOnly))
            return string.Empty;

        return digitsOnly.Length switch
        {
            <= 4 => digitsOnly,
            <= 7 => $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4)}",
            _ => $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, Math.Min(3, digitsOnly.Length - 7))}"
        };
    }

    #endregion

    #region Number Field Handlers

    /// <summary>
    /// 初始化數字輸入欄位的即時範圍限制
    /// </summary>
    protected async Task InitNumberInputRangeLimit(string elementId, decimal? min, decimal? max)
    {
        try
        {
            if (initializedNumberInputs.Contains(elementId))
                return;

            if (min.HasValue || max.HasValue)
            {
                await JSRuntime.InvokeVoidAsync("NumberInputHelper.initRangeLimitById",
                    elementId,
                    min.HasValue ? (object)(double)min.Value : null,
                    max.HasValue ? (object)(double)max.Value : null);

                initializedNumberInputs.Add(elementId);
            }
        }
        catch
        {
            // 靜默處理 JS 互操作錯誤
        }
    }

    /// <summary>
    /// 處理數字欄位的值設定，包含 Min/Max 範圍驗證
    /// </summary>
    protected void SetNumericPropertyValue(TModel model, FormFieldDefinition field, string? value)
    {
        if (model == null || field == null || string.IsNullOrEmpty(field.PropertyName))
            return;

        if (string.IsNullOrWhiteSpace(value))
        {
            SetPropertyValueInternal(model, field.PropertyName, null);
            return;
        }

        if (decimal.TryParse(value, out decimal numericValue))
        {
            if (field.Min.HasValue && numericValue < field.Min.Value)
                numericValue = field.Min.Value;
            if (field.Max.HasValue && numericValue > field.Max.Value)
                numericValue = field.Max.Value;

            SetPropertyValueInternal(model, field.PropertyName, numericValue.ToString());
            StateHasChanged();
        }
        else
        {
            SetPropertyValueInternal(model, field.PropertyName, null);
        }
    }

    /// <summary>
    /// 格式化數字欄位用於輸入框顯示
    /// </summary>
    protected string FormatNumberForInput(object? value)
    {
        if (value == null)
            return string.Empty;

        if (decimal.TryParse(value.ToString(), out decimal decimalValue))
        {
            return NumberFormatHelper.FormatForInput(decimalValue);
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 格式化數字欄位用於唯讀顯示（帶千分位）
    /// </summary>
    protected string FormatNumberForDisplay(object? value, int decimalPlaces = 0, bool useThousandsSeparator = true)
    {
        if (value == null)
            return string.Empty;

        if (decimal.TryParse(value.ToString(), out decimal decimalValue))
        {
            return NumberFormatHelper.FormatSmart(decimalValue, decimalPlaces, useThousandsSeparator);
        }

        return value.ToString() ?? string.Empty;
    }

    #endregion

    #region Property Value Accessors

    /// <summary>
    /// 取得屬性值（支援巢狀屬性）
    /// </summary>
    protected object? GetPropertyValue(TModel model, string propertyName)
    {
        if (model == null || string.IsNullOrEmpty(propertyName))
            return null;

        var parts = propertyName.Split('.');
        object? currentValue = model;

        foreach (var part in parts)
        {
            if (currentValue == null) return null;

            var currentType = currentValue.GetType();
            var property = currentType.GetProperty(part);

            if (property == null) return null;

            currentValue = property.GetValue(currentValue);
        }

        return currentValue;
    }

    /// <summary>
    /// 設定屬性值（內部使用，同步版本）
    /// </summary>
    private void SetPropertyValueInternal(TModel model, string propertyName, object? value)
    {
        if (model == null || string.IsNullOrEmpty(propertyName))
            return;

        var parts = propertyName.Split('.');
        object? currentValue = model;

        // 導航到最後一個屬性的父物件
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (currentValue == null) return;

            var currentType = currentValue.GetType();
            var property = currentType.GetProperty(parts[i]);

            if (property == null) return;

            currentValue = property.GetValue(currentValue);
        }

        // 設定最後一個屬性的值
        if (currentValue != null)
        {
            var finalType = currentValue.GetType();
            var finalProperty = finalType.GetProperty(parts[^1]);

            if (finalProperty != null && finalProperty.CanWrite)
            {
                var convertedValue = ConvertValue(value, finalProperty.PropertyType);
                finalProperty.SetValue(currentValue, convertedValue);
            }
        }
    }

    /// <summary>
    /// 設定屬性值並觸發變更通知（公開方法，用於 UI 綁定）
    /// </summary>
    protected void SetPropertyValue(TModel model, string propertyName, object? value)
    {
        SetPropertyValueInternal(model, propertyName, value);
        
        // 使用 fire-and-forget 模式觸發事件，避免 async void
        _ = NotifyFieldChanged(propertyName, value);
    }

    /// <summary>
    /// 通知欄位變更
    /// </summary>
    private Task NotifyFieldChanged(string propertyName, object? value)
    {
        if (OnFieldChanged.HasDelegate)
        {
            return OnFieldChanged.InvokeAsync((propertyName, value));
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 轉換值到目標類型
    /// </summary>
    protected object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        // 處理空字串
        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
                return null;
            if (targetType == typeof(string))
                return stringValue;
            return null;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            // 特別處理枚舉類型
            if (underlyingType.IsEnum)
            {
                if (value is string enumStringValue)
                {
                    if (int.TryParse(enumStringValue, out var intValue))
                    {
                        return Enum.ToObject(underlyingType, intValue);
                    }
                    return Enum.Parse(underlyingType, enumStringValue, true);
                }
                else if (value is int intValue)
                {
                    return Enum.ToObject(underlyingType, intValue);
                }
            }

            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Button Helpers

    protected ERPCore2.Components.Shared.UI.Button.ButtonVariant GetButtonVariant(string variant)
    {
        return variant switch
        {
            FormConstants.ButtonVariants.Primary => ERPCore2.Components.Shared.UI.Button.ButtonVariant.DarkBlue,
            FormConstants.ButtonVariants.Secondary => ERPCore2.Components.Shared.UI.Button.ButtonVariant.Gray,
            FormConstants.ButtonVariants.Success => ERPCore2.Components.Shared.UI.Button.ButtonVariant.Green,
            FormConstants.ButtonVariants.Danger => ERPCore2.Components.Shared.UI.Button.ButtonVariant.Red,
            FormConstants.ButtonVariants.Warning => ERPCore2.Components.Shared.UI.Button.ButtonVariant.Yellow,
            FormConstants.ButtonVariants.Info => ERPCore2.Components.Shared.UI.Button.ButtonVariant.Blue,
            FormConstants.ButtonVariants.OutlinePrimary => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineDarkBlue,
            FormConstants.ButtonVariants.OutlineSecondary => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineGray,
            FormConstants.ButtonVariants.OutlineSuccess => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineGreen,
            FormConstants.ButtonVariants.OutlineDanger => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineRed,
            FormConstants.ButtonVariants.OutlineWarning => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineYellow,
            FormConstants.ButtonVariants.OutlineInfo => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineBlue,
            _ => ERPCore2.Components.Shared.UI.Button.ButtonVariant.OutlineDarkBlue
        };
    }

    protected ERPCore2.Components.Shared.UI.Button.ButtonSize GetButtonSize(string size)
    {
        return size switch
        {
            FormConstants.ButtonSizes.Large => ERPCore2.Components.Shared.UI.Button.ButtonSize.Large,
            FormConstants.ButtonSizes.Normal => ERPCore2.Components.Shared.UI.Button.ButtonSize.Normal,
            FormConstants.ButtonSizes.Small => ERPCore2.Components.Shared.UI.Button.ButtonSize.Small,
            _ => ERPCore2.Components.Shared.UI.Button.ButtonSize.Small
        };
    }

    #endregion
}
