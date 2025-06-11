using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Tables;

namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 表單配置建構器
/// </summary>
public class FormConfigurationBuilder<TModel>
{
    private readonly List<FormFieldDefinition> _fields = new();

    public List<FormFieldDefinition> Fields => _fields;

    /// <summary>
    /// 添加文字欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddText(string propertyName, string label, 
        string? placeholder = null, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Text,
            Placeholder = placeholder,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加密碼欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddPassword(string propertyName, string label, 
        string? placeholder = null, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Password,
            Placeholder = placeholder,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加電子郵件欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddEmail(string propertyName, string label, 
        string? placeholder = null, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Email,
            Placeholder = placeholder,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加數字欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddNumber(string propertyName, string label, 
        decimal? min = null, decimal? max = null, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Number,
            Min = min,
            Max = max,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加日期欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddDate(string propertyName, string label, 
        bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Date,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加選擇欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddSelect(string propertyName, string label, 
        IEnumerable<SelectOption> options, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Select,
            Options = options.ToList(),
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加多行文字欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddTextArea(string propertyName, string label, 
        int rows = 3, string? placeholder = null, bool isRequired = false)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.TextArea,
            Rows = rows,
            Placeholder = placeholder,
            IsRequired = isRequired
        });
        return this;
    }

    /// <summary>
    /// 添加核取方塊欄位
    /// </summary>
    public FormConfigurationBuilder<TModel> AddCheckbox(string propertyName, string label)
    {
        _fields.Add(new FormFieldDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = FormFieldType.Checkbox
        });
        return this;
    }

    /// <summary>
    /// 自動生成所有可編輯的屬性
    /// </summary>
    public FormConfigurationBuilder<TModel> AutoGenerate()
    {
        var properties = typeof(TModel).GetProperties()
            .Where(p => p.CanWrite && IsEditableProperty(p));

        foreach (var property in properties)
        {
            var field = CreateFieldFromProperty(property);
            if (field != null)
            {
                _fields.Add(field);
            }
        }

        return this;
    }

    private bool IsEditableProperty(PropertyInfo property)
    {
        // 排除常見的不可編輯屬性
        var excludedNames = new[] { "Id", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy" };
        return !excludedNames.Contains(property.Name) && 
               !property.GetCustomAttributes<System.ComponentModel.ReadOnlyAttribute>().Any();
    }

    private FormFieldDefinition? CreateFieldFromProperty(PropertyInfo property)
    {
        var field = new FormFieldDefinition
        {
            PropertyName = property.Name,
            Label = GetDisplayName(property),
            IsRequired = IsRequired(property)
        };

        // 根據屬性類型設定欄位類型
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType == typeof(string))
        {
            if (property.Name.ToLower().Contains("email"))
                field.FieldType = FormFieldType.Email;
            else if (property.Name.ToLower().Contains("password"))
                field.FieldType = FormFieldType.Password;
            else
                field.FieldType = FormFieldType.Text;
        }
        else if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(double))
        {
            field.FieldType = FormFieldType.Number;
        }
        else if (propertyType == typeof(DateTime))
        {
            field.FieldType = FormFieldType.Date;
        }
        else if (propertyType == typeof(bool))
        {
            field.FieldType = FormFieldType.Checkbox;
        }
        else if (propertyType.IsEnum)
        {
            field.FieldType = FormFieldType.Select;
            field.Options = Enum.GetValues(propertyType)
                .Cast<object>()
                .Select(e => new SelectOption { Value = e.ToString()!, Text = e.ToString()! })
                .ToList();
        }
        else
        {
            return null; // 不支援的類型
        }

        return field;
    }

    private string GetDisplayName(PropertyInfo property)
    {
        var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? property.Name;
    }

    private bool IsRequired(PropertyInfo property)
    {
        return property.GetCustomAttribute<RequiredAttribute>() != null ||
               (!property.PropertyType.IsClass && Nullable.GetUnderlyingType(property.PropertyType) == null);
    }

    /// <summary>
    /// 建立配置
    /// </summary>
    public List<FormFieldDefinition> Build() => _fields;

    /// <summary>
    /// 靜態工廠方法
    /// </summary>
    public static FormConfigurationBuilder<TModel> Create()
    {
        return new FormConfigurationBuilder<TModel>();
    }
}

/// <summary>
/// 搜尋篩選配置建構器
/// </summary>
public class SearchFilterConfigurationBuilder<TModel>
{
    private readonly List<SearchFilterDefinition> _filters = new();

    public List<SearchFilterDefinition> Filters => _filters;

    /// <summary>
    /// 添加文字篩選
    /// </summary>
    public SearchFilterConfigurationBuilder<TModel> AddText(string propertyName, string label, 
        string? placeholder = null)
    {
        _filters.Add(new SearchFilterDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FilterType = SearchFilterType.Text,
            Placeholder = placeholder
        });
        return this;
    }

    /// <summary>
    /// 添加選擇篩選
    /// </summary>
    public SearchFilterConfigurationBuilder<TModel> AddSelect(string propertyName, string label, 
        IEnumerable<SelectOption> options)
    {
        _filters.Add(new SearchFilterDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FilterType = SearchFilterType.Select,
            Options = options.ToList()
        });
        return this;
    }

    /// <summary>
    /// 添加日期範圍篩選
    /// </summary>
    public SearchFilterConfigurationBuilder<TModel> AddDateRange(string propertyName, string label)
    {
        _filters.Add(new SearchFilterDefinition
        {
            PropertyName = propertyName,
            Label = label,
            FilterType = SearchFilterType.DateRange
        });
        return this;
    }

    /// <summary>
    /// 建立配置
    /// </summary>
    public List<SearchFilterDefinition> Build() => _filters;

    /// <summary>
    /// 靜態工廠方法
    /// </summary>
    public static SearchFilterConfigurationBuilder<TModel> Create()
    {
        return new SearchFilterConfigurationBuilder<TModel>();
    }
}

/// <summary>
/// 表格配置建構器
/// </summary>
public class TableConfigurationBuilder<TModel>
{
    private readonly List<TableColumnDefinition> _columns = new();

    public List<TableColumnDefinition> Columns => _columns;

    /// <summary>
    /// 添加文字欄位
    /// </summary>
    public TableConfigurationBuilder<TModel> AddText(string propertyName, string title, 
        string? headerCssClass = null, string? cellCssClass = null)
    {
        _columns.Add(new TableColumnDefinition
        {
            PropertyName = propertyName,
            Title = title,
            DataType = ColumnDataType.Text,
            HeaderCssClass = headerCssClass,
            CellCssClass = cellCssClass
        });
        return this;
    }

    /// <summary>
    /// 添加數字欄位
    /// </summary>
    public TableConfigurationBuilder<TModel> AddNumber(string propertyName, string title, 
        string? format = null, string? headerCssClass = null, string? cellCssClass = null)
    {
        _columns.Add(new TableColumnDefinition
        {
            PropertyName = propertyName,
            Title = title,
            DataType = ColumnDataType.Number,
            Format = format,
            HeaderCssClass = headerCssClass,
            CellCssClass = cellCssClass
        });
        return this;
    }

    /// <summary>
    /// 添加日期欄位
    /// </summary>
    public TableConfigurationBuilder<TModel> AddDate(string propertyName, string title, 
        string? format = "yyyy-MM-dd", string? headerCssClass = null, string? cellCssClass = null)
    {
        _columns.Add(new TableColumnDefinition
        {
            PropertyName = propertyName,
            Title = title,
            DataType = ColumnDataType.Date,
            Format = format,
            HeaderCssClass = headerCssClass,
            CellCssClass = cellCssClass
        });
        return this;
    }

    /// <summary>
    /// 添加狀態欄位
    /// </summary>
    public TableConfigurationBuilder<TModel> AddStatus(string propertyName, string title, 
        Dictionary<object, string> statusMapping, string? headerCssClass = null, string? cellCssClass = null)
    {
        _columns.Add(new TableColumnDefinition
        {
            PropertyName = propertyName,
            Title = title,
            DataType = ColumnDataType.Status,
            StatusBadgeMap = statusMapping,
            HeaderCssClass = headerCssClass,
            CellCssClass = cellCssClass
        });
        return this;
    }

    /// <summary>
    /// 添加自定義欄位
    /// </summary>
    public TableConfigurationBuilder<TModel> AddCustomColumn(string propertyName, string title, 
        RenderFragment<object> template, string? headerCssClass = null, string? cellCssClass = null)
    {
        _columns.Add(new TableColumnDefinition
        {
            PropertyName = propertyName,
            Title = title,
            CustomTemplate = template,
            HeaderCssClass = headerCssClass,
            CellCssClass = cellCssClass
        });
        return this;
    }

    /// <summary>
    /// 建立配置
    /// </summary>
    public List<TableColumnDefinition> Build() => _columns;

    /// <summary>
    /// 靜態工廠方法
    /// </summary>
    public static TableConfigurationBuilder<TModel> Create()
    {
        return new TableConfigurationBuilder<TModel>();
    }
}
