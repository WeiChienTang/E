using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 設備保養維修記錄篩選條件
/// </summary>
public class EquipmentMaintenanceCriteria : IReportFilterCriteria
{
    [FilterFK(typeof(IEquipmentService),
        Group = FilterGroup.Basic,
        Label = "設備",
        EmptyMessage = "全部設備",
        Order = 0)]
    public int? EquipmentId { get; set; }

    [FilterEnum(typeof(Models.Enums.EquipmentMaintenanceType),
        Group = FilterGroup.Basic,
        Label = "維修類型",
        Order = 1)]
    public int? MaintenanceType { get; set; }

    [FilterDateRange(Group = FilterGroup.Date, Label = "保養日期", Order = 2)]
    public DateTime? MaintenanceDateStart { get; set; }

    public DateTime? MaintenanceDateEnd { get; set; }

    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage)
    {
        errorMessage = null;
        return true;
    }

    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["EquipmentId"] = EquipmentId,
            ["MaintenanceType"] = MaintenanceType,
            ["MaintenanceDateStart"] = MaintenanceDateStart,
            ["MaintenanceDateEnd"] = MaintenanceDateEnd
        };
    }

    public string GetSummary()
    {
        var parts = new List<string>();
        if (MaintenanceDateStart.HasValue && MaintenanceDateEnd.HasValue)
            parts.Add($"保養日期：{MaintenanceDateStart:yyyy/MM/dd} ~ {MaintenanceDateEnd:yyyy/MM/dd}");
        return parts.Any() ? string.Join("，", parts) : "全部記錄";
    }
}
