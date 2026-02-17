using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 員工名冊表篩選條件
/// </summary>
public class EmployeeRosterCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 指定員工 ID 清單（空表示所有員工）
    /// </summary>
    public List<int> EmployeeIds { get; set; } = new();

    /// <summary>
    /// 指定部門 ID 清單（空表示所有部門）
    /// </summary>
    public List<int> DepartmentIds { get; set; } = new();

    /// <summary>
    /// 指定職位 ID 清單（空表示所有職位）
    /// </summary>
    public List<int> PositionIds { get; set; } = new();

    /// <summary>
    /// 指定在職狀態清單（空表示所有狀態）
    /// </summary>
    public List<EmployeeStatus> EmploymentStatuses { get; set; } = new();

    /// <summary>
    /// 指定權限組 ID 清單（空表示所有權限組）
    /// </summary>
    public List<int> RoleIds { get; set; } = new();

    /// <summary>
    /// 到職日期起始
    /// </summary>
    public DateTime? HireDateStart { get; set; }

    /// <summary>
    /// 到職日期結束
    /// </summary>
    public DateTime? HireDateEnd { get; set; }

    /// <summary>
    /// 離職日期起始
    /// </summary>
    public DateTime? ResignationDateStart { get; set; }

    /// <summary>
    /// 離職日期結束
    /// </summary>
    public DateTime? ResignationDateEnd { get; set; }

    /// <summary>
    /// 生日日期起始
    /// </summary>
    public DateTime? BirthDateStart { get; set; }

    /// <summary>
    /// 生日日期結束
    /// </summary>
    public DateTime? BirthDateEnd { get; set; }

    /// <summary>
    /// 關鍵字搜尋（員工編號、姓名）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否僅顯示在職員工（預設 true）
    /// </summary>
    public bool ActiveOnly { get; set; } = true;

    /// <summary>
    /// 紙張設定
    /// </summary>
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
            ["employeeIds"] = EmployeeIds.Any() ? EmployeeIds : null,
            ["departmentIds"] = DepartmentIds.Any() ? DepartmentIds : null,
            ["positionIds"] = PositionIds.Any() ? PositionIds : null,
            ["employmentStatuses"] = EmploymentStatuses.Any() ? EmploymentStatuses : null,
            ["roleIds"] = RoleIds.Any() ? RoleIds : null,
            ["hireDateStart"] = HireDateStart,
            ["hireDateEnd"] = HireDateEnd,
            ["resignationDateStart"] = ResignationDateStart,
            ["resignationDateEnd"] = ResignationDateEnd,
            ["birthDateStart"] = BirthDateStart,
            ["birthDateEnd"] = BirthDateEnd,
            ["keyword"] = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword,
            ["activeOnly"] = ActiveOnly
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (EmployeeIds.Any())
            summary.Add($"員工：{EmployeeIds.Count} 人");

        if (DepartmentIds.Any())
            summary.Add($"部門：{DepartmentIds.Count} 個");

        if (PositionIds.Any())
            summary.Add($"職位：{PositionIds.Count} 個");

        if (EmploymentStatuses.Any())
            summary.Add($"狀態：{EmploymentStatuses.Count} 種");

        if (RoleIds.Any())
            summary.Add($"權限組：{RoleIds.Count} 個");

        if (HireDateStart.HasValue || HireDateEnd.HasValue)
            summary.Add($"到職：{HireDateStart?.ToString("yyyy/MM/dd") ?? ""}~{HireDateEnd?.ToString("yyyy/MM/dd") ?? ""}");

        if (ResignationDateStart.HasValue || ResignationDateEnd.HasValue)
            summary.Add($"離職：{ResignationDateStart?.ToString("yyyy/MM/dd") ?? ""}~{ResignationDateEnd?.ToString("yyyy/MM/dd") ?? ""}");

        if (BirthDateStart.HasValue || BirthDateEnd.HasValue)
            summary.Add($"生日：{BirthDateStart?.ToString("yyyy/MM/dd") ?? ""}~{BirthDateEnd?.ToString("yyyy/MM/dd") ?? ""}");

        if (!string.IsNullOrEmpty(Keyword))
            summary.Add($"關鍵字：{Keyword}");

        if (ActiveOnly)
            summary.Add("僅在職");

        return summary.Any() ? string.Join(" | ", summary) : "全部";
    }
}
