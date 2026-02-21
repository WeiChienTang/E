using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 車輛保養表篩選條件
/// </summary>
public class VehicleMaintenanceCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定車輛 ID 清單（空表示所有車輛）
    /// </summary>
    [FilterFK(typeof(IVehicleService),
        Group = FilterGroup.Basic,
        Label = "指定車輛",
        Placeholder = "搜尋車輛...",
        EmptyMessage = "未選擇車輛（查詢全部車輛）",
        Order = 1)]
    public List<int> VehicleIds { get; set; } = new();

    /// <summary>
    /// 保養類型篩選（空表示所有類型）
    /// </summary>
    public List<MaintenanceType> MaintenanceTypes { get; set; } = new();

    /// <summary>
    /// 起始日期
    /// </summary>
    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 關鍵字搜尋（車牌號碼、維修廠、保養描述）
    /// </summary>
    [FilterKeyword(Group = FilterGroup.Basic, Label = "關鍵字", Placeholder = "搜尋車牌號碼、維修廠、保養描述...", Order = 2)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 紙張設定
    /// </summary>
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
            ["vehicleIds"] = VehicleIds.Any() ? VehicleIds : null,
            ["maintenanceTypes"] = MaintenanceTypes.Any() ? MaintenanceTypes : null,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (VehicleIds.Any())
            summary.Add($"車輛：{VehicleIds.Count} 台");

        if (MaintenanceTypes.Any())
            summary.Add($"保養類型：{MaintenanceTypes.Count} 種");

        if (StartDate.HasValue)
            summary.Add($"起始：{StartDate:yyyy/MM/dd}");

        if (EndDate.HasValue)
            summary.Add($"結束：{EndDate:yyyy/MM/dd}");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
