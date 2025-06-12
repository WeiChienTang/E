using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 搜尋篩選類型枚舉
/// </summary>
public enum SearchFilterType
{
    Text,           // 文字篩選
    Number,         // 數字篩選
    NumberRange,    // 數字範圍篩選
    Date,           // 日期篩選
    DateRange,      // 日期範圍篩選
    DateTime,       // 日期時間篩選
    DateTimeRange,  // 日期時間範圍篩選
    Select,         // 單選下拉篩選
    MultiSelect,    // 多選篩選
    Boolean,        // 布林篩選
    Custom          // 自定義篩選
}

/// <summary>
/// 搜尋篩選定義
/// </summary>
public class SearchFilterDefinition
{
    /// <summary>
    /// 篩選器名稱（對應屬性名稱）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 屬性名稱（Name 的別名，用於向後兼容）
    /// </summary>
    public string PropertyName
    {
        get => Name;
        set => Name = value;
    }

    /// <summary>
    /// 顯示標籤
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 篩選類型
    /// </summary>
    public SearchFilterType Type { get; set; } = SearchFilterType.Text;

    /// <summary>
    /// 篩選類型（Type 的別名，用於向後兼容）
    /// </summary>
    public SearchFilterType FilterType
    {
        get => Type;
        set => Type = value;
    }

    /// <summary>
    /// 佔位符文字
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// 是否為進階篩選（預設隱藏）
    /// </summary>
    public bool IsAdvanced { get; set; } = false;

    /// <summary>
    /// 選項清單（用於 Select 和 MultiSelect）
    /// </summary>
    public List<SelectOption> Options { get; set; } = new();

    /// <summary>
    /// 空選項文字
    /// </summary>
    public string? EmptyOptionText { get; set; }

    /// <summary>
    /// 容器 CSS 類別
    /// </summary>
    public string? ContainerCssClass { get; set; }

    /// <summary>
    /// 預設值
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// CSS 類別
    /// </summary>
    public string CssClass { get; set; } = "col-md-6";

    /// <summary>
    /// 驗證規則
    /// </summary>
    public List<ValidationRule> ValidationRules { get; set; } = new();

    /// <summary>
    /// 自定義屬性
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();
}

/// <summary>
/// 搜尋篩選模型基類
/// </summary>
public class SearchFilterModel
{
    /// <summary>
    /// 文字篩選值
    /// </summary>
    public Dictionary<string, string?> TextFilters { get; set; } = new();

    /// <summary>
    /// 數字篩選值
    /// </summary>
    public Dictionary<string, decimal?> NumberFilters { get; set; } = new();

    /// <summary>
    /// 數字範圍篩選值
    /// </summary>
    public Dictionary<string, NumberRange?> NumberRangeFilters { get; set; } = new();

    /// <summary>
    /// 日期篩選值
    /// </summary>
    public Dictionary<string, DateTime?> DateFilters { get; set; } = new();

    /// <summary>
    /// 日期範圍篩選值
    /// </summary>
    public Dictionary<string, DateRange?> DateRangeFilters { get; set; } = new();

    /// <summary>
    /// 日期時間篩選值
    /// </summary>
    public Dictionary<string, DateTime?> DateTimeFilters { get; set; } = new();

    /// <summary>
    /// 日期時間範圍篩選值
    /// </summary>
    public Dictionary<string, DateTimeRange?> DateTimeRangeFilters { get; set; } = new();

    /// <summary>
    /// 選擇篩選值
    /// </summary>
    public Dictionary<string, string?> SelectFilters { get; set; } = new();

    /// <summary>
    /// 多選篩選值
    /// </summary>
    public Dictionary<string, List<string>> MultiSelectFilters { get; set; } = new();

    /// <summary>
    /// 布林篩選值
    /// </summary>
    public Dictionary<string, bool?> BooleanFilters { get; set; } = new();

    /// <summary>
    /// 自定義篩選值
    /// </summary>
    public Dictionary<string, object?> CustomFilters { get; set; } = new();

    /// <summary>
    /// 獲取篩選值
    /// </summary>
    public object? GetFilterValue(string name)
    {
        if (TextFilters.TryGetValue(name, out var textValue))
            return textValue;
        
        if (NumberFilters.TryGetValue(name, out var numberValue))
            return numberValue;
        
        if (NumberRangeFilters.TryGetValue(name, out var numberRangeValue))
            return numberRangeValue;
        
        if (DateFilters.TryGetValue(name, out var dateValue))
            return dateValue;
        
        if (DateRangeFilters.TryGetValue(name, out var dateRangeValue))
            return dateRangeValue;
        
        if (DateTimeFilters.TryGetValue(name, out var dateTimeValue))
            return dateTimeValue;
        
        if (DateTimeRangeFilters.TryGetValue(name, out var dateTimeRangeValue))
            return dateTimeRangeValue;
        
        if (SelectFilters.TryGetValue(name, out var selectValue))
            return selectValue;
        
        if (MultiSelectFilters.TryGetValue(name, out var multiSelectValue))
            return multiSelectValue;
        
        if (BooleanFilters.TryGetValue(name, out var booleanValue))
            return booleanValue;
        
        if (CustomFilters.TryGetValue(name, out var customValue))
            return customValue;
        
        return null;
    }

    /// <summary>
    /// 設定篩選值
    /// </summary>
    public void SetFilterValue(string name, object? value)
    {
        if (value == null)
        {
            // 清除所有可能的值
            TextFilters.Remove(name);
            NumberFilters.Remove(name);
            NumberRangeFilters.Remove(name);
            DateFilters.Remove(name);
            DateRangeFilters.Remove(name);
            DateTimeFilters.Remove(name);
            DateTimeRangeFilters.Remove(name);
            SelectFilters.Remove(name);
            MultiSelectFilters.Remove(name);
            BooleanFilters.Remove(name);
            CustomFilters.Remove(name);
            return;
        }

        switch (value)
        {
            case string strValue:
                TextFilters[name] = strValue;
                SelectFilters[name] = strValue;
                break;
            
            case decimal decValue:
                NumberFilters[name] = decValue;
                break;
            
            case NumberRange numberRange:
                NumberRangeFilters[name] = numberRange;
                break;
            
            case DateTime dateTime:
                DateFilters[name] = dateTime;
                DateTimeFilters[name] = dateTime;
                break;
            
            case DateRange dateRange:
                DateRangeFilters[name] = dateRange;
                break;
            
            case DateTimeRange dateTimeRange:
                DateTimeRangeFilters[name] = dateTimeRange;
                break;
            
            case List<string> stringList:
                MultiSelectFilters[name] = stringList;
                break;
            
            case bool boolValue:
                BooleanFilters[name] = boolValue;
                break;
            
            default:
                CustomFilters[name] = value;
                break;
        }
    }

    /// <summary>
    /// 清除所有篩選
    /// </summary>
    public void Clear()
    {
        TextFilters.Clear();
        NumberFilters.Clear();
        NumberRangeFilters.Clear();
        DateFilters.Clear();
        DateRangeFilters.Clear();
        DateTimeFilters.Clear();
        DateTimeRangeFilters.Clear();
        SelectFilters.Clear();
        MultiSelectFilters.Clear();
        BooleanFilters.Clear();
        CustomFilters.Clear();
    }

    /// <summary>
    /// 檢查是否有任何篩選條件
    /// </summary>
    public bool HasAnyFilter()
    {
        return TextFilters.Values.Any(v => !string.IsNullOrWhiteSpace(v)) ||
               NumberFilters.Values.Any(v => v.HasValue) ||
               NumberRangeFilters.Values.Any(v => v != null && (v.Min.HasValue || v.Max.HasValue)) ||
               DateFilters.Values.Any(v => v.HasValue) ||
               DateRangeFilters.Values.Any(v => v != null && (v.StartDate.HasValue || v.EndDate.HasValue)) ||
               DateTimeFilters.Values.Any(v => v.HasValue) ||
               DateTimeRangeFilters.Values.Any(v => v != null && (v.StartDateTime.HasValue || v.EndDateTime.HasValue)) ||
               SelectFilters.Values.Any(v => !string.IsNullOrWhiteSpace(v)) ||
               MultiSelectFilters.Values.Any(v => v != null && v.Count > 0) ||
               BooleanFilters.Values.Any(v => v.HasValue) ||
               CustomFilters.Values.Any(v => v != null);
    }
}

/// <summary>
/// 數字範圍
/// </summary>
public class NumberRange
{
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }

    public bool IsValid()
    {
        return Min.HasValue || Max.HasValue;
    }

    public bool Contains(decimal value)
    {
        if (!IsValid()) return true;
        
        var minCheck = !Min.HasValue || value >= Min.Value;
        var maxCheck = !Max.HasValue || value <= Max.Value;
        
        return minCheck && maxCheck;
    }
}

/// <summary>
/// 日期範圍
/// </summary>
public class DateRange
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsValid()
    {
        return StartDate.HasValue || EndDate.HasValue;
    }

    public bool Contains(DateTime date)
    {
        if (!IsValid()) return true;
        
        var startCheck = !StartDate.HasValue || date.Date >= StartDate.Value.Date;
        var endCheck = !EndDate.HasValue || date.Date <= EndDate.Value.Date;
        
        return startCheck && endCheck;
    }
}

/// <summary>
/// 日期時間範圍
/// </summary>
public class DateTimeRange
{
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public bool IsValid()
    {
        return StartDateTime.HasValue || EndDateTime.HasValue;
    }

    public bool Contains(DateTime dateTime)
    {
        if (!IsValid()) return true;
        
        var startCheck = !StartDateTime.HasValue || dateTime >= StartDateTime.Value;
        var endCheck = !EndDateTime.HasValue || dateTime <= EndDateTime.Value;
        
        return startCheck && endCheck;
    }
}

/// <summary>
/// 搜尋篩選配置建構器
/// </summary>
public class SearchFilterBuilder<TModel>
{
    private readonly List<SearchFilterDefinition> _filters = new();

    /// <summary>
    /// 添加文字篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddText(string name, string label, string? placeholder = null, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.Text,
            Placeholder = placeholder,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加數字篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddNumber(string name, string label, string? placeholder = null, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.Number,
            Placeholder = placeholder,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加數字範圍篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddNumberRange(string name, string label, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.NumberRange,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加日期篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddDate(string name, string label, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.Date,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加日期範圍篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddDateRange(string name, string label, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.DateRange,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加選擇篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddSelect(string name, string label, List<SelectOption> options, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.Select,
            Options = options,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加多選篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddMultiSelect(string name, string label, List<SelectOption> options, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.MultiSelect,
            Options = options,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 添加布林篩選
    /// </summary>
    public SearchFilterBuilder<TModel> AddBoolean(string name, string label, bool isAdvanced = false)
    {
        _filters.Add(new SearchFilterDefinition
        {
            Name = name,
            Label = label,
            Type = SearchFilterType.Boolean,
            IsAdvanced = isAdvanced
        });
        return this;
    }

    /// <summary>
    /// 建構篩選定義清單
    /// </summary>
    public List<SearchFilterDefinition> Build()
    {
        return _filters;
    }
}
