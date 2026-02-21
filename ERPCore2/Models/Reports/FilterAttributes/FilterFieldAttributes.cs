namespace ERPCore2.Models.Reports.FilterAttributes;

/// <summary>
/// 篩選欄位群組（對應 FilterSectionColumn 的欄位）
/// </summary>
public enum FilterGroup
{
    Basic = 1,  // 基本篩選
    Date = 2,   // 日期範圍
    Quick = 3   // 快速條件
}

/// <summary>
/// FK 多選篩選的顯示格式
/// </summary>
public enum FilterDisplayFormat
{
    /// <summary>僅顯示 Name 屬性（預設）</summary>
    NameOnly = 0,
    /// <summary>顯示「Code - Name」格式</summary>
    CodeDashName = 1,
    /// <summary>僅顯示 Code 屬性</summary>
    CodeOnly = 2
}

/// <summary>
/// 動態篩選器內部使用的顯示項目
/// </summary>
public class FilterDisplayItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// 標記 Criteria 的 List&lt;int&gt; 屬性對應 FK 多選篩選
/// DynamicFilterTemplate 會自動注入對應 Service 並載入選項
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterFKAttribute : Attribute
{
    public FilterFKAttribute(Type serviceType)
    {
        ServiceType = serviceType;
    }

    /// <summary>用於載入選項的 Service 介面型別（例如 typeof(IDepartmentService)）</summary>
    public Type ServiceType { get; }

    /// <summary>顯示在哪個欄位群組</summary>
    public FilterGroup Group { get; set; } = FilterGroup.Basic;

    /// <summary>FilterFieldRow 的標籤文字</summary>
    public string Label { get; set; } = "";

    /// <summary>搜尋框提示文字</summary>
    public string Placeholder { get; set; } = "搜尋...";

    /// <summary>未選擇時的提示訊息</summary>
    public string EmptyMessage { get; set; } = "未選擇（查詢全部）";

    /// <summary>顯示名稱格式</summary>
    public FilterDisplayFormat DisplayFormat { get; set; } = FilterDisplayFormat.NameOnly;

    /// <summary>
    /// Entity 上的 bool 屬性名稱，若該屬性為 true 則排除此筆資料
    /// 例如：ExcludeProperty = "IsSuperAdmin" 會過濾掉超級管理員
    /// </summary>
    public string? ExcludeProperty { get; set; }

    /// <summary>群組內的排列順序（數字越小越前面）</summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// 標記 Criteria 的 List&lt;TEnum&gt; 屬性對應 Enum 多選篩選
/// 會自動讀取 Enum 的 [Display(Name)] 屬性產生選項
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterEnumAttribute : Attribute
{
    public FilterEnumAttribute(Type enumType)
    {
        EnumType = enumType;
    }

    /// <summary>對應的 Enum 型別</summary>
    public Type EnumType { get; }

    /// <summary>顯示在哪個欄位群組</summary>
    public FilterGroup Group { get; set; } = FilterGroup.Basic;

    /// <summary>FilterFieldRow 的標籤文字</summary>
    public string Label { get; set; } = "";

    /// <summary>群組內的排列順序</summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// 標記 Criteria 的 DateTime? Start 屬性對應日期範圍篩選
/// 必須貼在 XxxStart 屬性上，End 屬性（XxxEnd）會自動配對
/// 命名規則：HireDateStart → 自動推導 HireDateEnd
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterDateRangeAttribute : Attribute
{
    /// <summary>顯示在哪個欄位群組</summary>
    public FilterGroup Group { get; set; } = FilterGroup.Date;

    /// <summary>FilterFieldRow 的標籤文字</summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// 手動指定 End 屬性名稱（可選，不填則自動將 Start 替換為 End）
    /// 自動配對規則：屬性名稱結尾 "Start" → 替換為 "End"
    /// </summary>
    public string? EndPropertyName { get; set; }

    /// <summary>群組內的排列順序</summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// 標記 Criteria 的 string? 屬性對應關鍵字文字搜尋
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterKeywordAttribute : Attribute
{
    /// <summary>顯示在哪個欄位群組</summary>
    public FilterGroup Group { get; set; } = FilterGroup.Quick;

    /// <summary>FilterFieldRow 的標籤文字</summary>
    public string Label { get; set; } = "關鍵字";

    /// <summary>輸入框提示文字</summary>
    public string Placeholder { get; set; } = "搜尋...";

    /// <summary>群組內的排列順序</summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// 標記 Criteria 的 bool 屬性對應 Checkbox 切換篩選
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterToggleAttribute : Attribute
{
    /// <summary>顯示在哪個欄位群組</summary>
    public FilterGroup Group { get; set; } = FilterGroup.Quick;

    /// <summary>FilterFieldRow 的標籤文字</summary>
    public string Label { get; set; } = "";

    /// <summary>Checkbox 旁邊的說明文字</summary>
    public string CheckboxLabel { get; set; } = "";

    /// <summary>預設值</summary>
    public bool DefaultValue { get; set; } = false;

    /// <summary>群組內的排列順序</summary>
    public int Order { get; set; } = 0;
}
