using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 廢料記錄報表篩選條件（WL001）
/// </summary>
public class WasteRecordCriteria : IReportFilterCriteria
{
    /// <summary>廢料類型 ID 清單（空表示全部）</summary>
    [FilterFK(typeof(IWasteTypeService),
        Group = FilterGroup.Basic,
        Label = "廢料類型",
        Placeholder = "搜尋廢料類型...",
        EmptyMessage = "未選擇（查詢全部）",
        Order = 1)]
    public List<int> WasteTypeIds { get; set; } = new();

    /// <summary>車輛 ID 清單（空表示全部）</summary>
    [FilterFK(typeof(IVehicleService),
        Group = FilterGroup.Basic,
        Label = "車輛",
        Placeholder = "搜尋車牌號碼...",
        EmptyMessage = "未選擇（查詢全部）",
        Order = 2)]
    public List<int> VehicleIds { get; set; } = new();

    /// <summary>客戶 ID 清單（空表示全部）</summary>
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇（查詢全部）",
        Order = 3)]
    public List<int> CustomerIds { get; set; } = new();

    /// <summary>入庫倉庫 ID 清單（空表示全部）</summary>
    [FilterFK(typeof(IWarehouseService),
        Group = FilterGroup.Basic,
        Label = "入庫倉庫",
        Placeholder = "搜尋倉庫...",
        EmptyMessage = "未選擇（查詢全部）",
        Order = 4)]
    public List<int> WarehouseIds { get; set; } = new();

    /// <summary>記錄日期起始</summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "記錄日期", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>記錄日期結束（自動與 StartDate 配對）</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>關鍵字搜尋（廢料單號、車牌號碼）</summary>
    [FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字",
        Placeholder = "搜尋廢料單號、車牌號碼...", Order = 1)]
    public string? Keyword { get; set; }

    /// <summary>僅顯示啟用記錄</summary>
    [FilterToggle(Group = FilterGroup.Quick, Label = "狀態篩選",
        CheckboxLabel = "僅顯示啟用記錄", DefaultValue = true, Order = 2)]
    public bool ActiveOnly { get; set; } = true;

    /// <summary>紙張設定</summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errorMessage = "起始日期不可大於結束日期";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["wasteTypeIds"] = WasteTypeIds.Any() ? WasteTypeIds : null,
            ["vehicleIds"] = VehicleIds.Any() ? VehicleIds : null,
            ["customerIds"] = CustomerIds.Any() ? CustomerIds : null,
            ["warehouseIds"] = WarehouseIds.Any() ? WarehouseIds : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["keyword"] = Keyword,
            ["activeOnly"] = ActiveOnly
        };
    }

    public string GetSummary()
    {
        var summary = new List<string>();

        if (StartDate.HasValue)
            summary.Add($"起始：{StartDate:yyyy/MM/dd}");

        if (EndDate.HasValue)
            summary.Add($"結束：{EndDate:yyyy/MM/dd}");

        if (WasteTypeIds.Any())
            summary.Add($"廢料類型：{WasteTypeIds.Count} 個");

        if (VehicleIds.Any())
            summary.Add($"車輛：{VehicleIds.Count} 台");

        if (CustomerIds.Any())
            summary.Add($"客戶：{CustomerIds.Count} 個");

        if (WarehouseIds.Any())
            summary.Add($"倉庫：{WarehouseIds.Count} 個");

        if (!string.IsNullOrWhiteSpace(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅啟用記錄");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
