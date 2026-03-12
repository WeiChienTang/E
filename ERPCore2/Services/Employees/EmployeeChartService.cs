using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Employees;

public class EmployeeChartService : IEmployeeChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EmployeeChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依部門統計員工人數（左外連接 Department）</summary>
    public async Task<List<ChartDataItem>> GetEmployeesByDepartmentAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from e in context.Employees
            where e.Status != EntityStatus.Deleted
                  && e.EmploymentStatus != EmployeeStatus.Resigned
                  && e.EmploymentStatus != EmployeeStatus.Inactive
            join d in context.Departments on e.DepartmentId equals d.Id into dg
            from dept in dg.DefaultIfEmpty()
            select new { DeptName = dept != null && dept.Name != null ? dept.Name : "未分配" }
        )
        .GroupBy(x => x.DeptName)
        .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>依員工類別統計員工人數</summary>
    public async Task<List<ChartDataItem>> GetEmployeesByTypeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive)
            .ToListAsync();

        var grouped = raw
            .GroupBy(e => e.EmployeeType)
            .Select(g => new ChartDataItem
            {
                Label = g.Key switch
                {
                    EmployeeType.FullTime  => "正職",
                    EmployeeType.PartTime  => "兼職",
                    EmployeeType.Contract  => "約聘",
                    EmployeeType.Intern    => "實習",
                    EmployeeType.Dispatch  => "派遣",
                    _                      => g.Key.ToString()
                },
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        return grouped;
    }

    /// <summary>依在職狀態統計員工人數</summary>
    public async Task<List<ChartDataItem>> GetEmployeesByStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var probation      = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Probation);
        var active         = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Active);
        var leaveOfAbsence = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.LeaveOfAbsence);
        var resigned       = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Resigned);
        var inactive       = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Inactive);

        return new List<ChartDataItem>
        {
            new() { Label = "試用期",     Value = probation },
            new() { Label = "在職",       Value = active },
            new() { Label = "留職停薪",   Value = leaveOfAbsence },
            new() { Label = "已離職",     Value = resigned },
            new() { Label = "停用",       Value = inactive }
        }.Where(x => x.Value > 0).ToList();
    }

    /// <summary>依性別統計員工人數（排除已離職、停用）</summary>
    public async Task<List<ChartDataItem>> GetEmployeesByGenderAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive)
            .ToListAsync();

        var grouped = raw
            .GroupBy(e => e.Gender)
            .Select(g => new ChartDataItem
            {
                Label = g.Key switch
                {
                    Gender.Male   => "男性",
                    Gender.Female => "女性",
                    Gender.Other  => "其他",
                    null          => "未設定",
                    _             => g.Key.ToString()!
                },
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        return grouped;
    }

    /// <summary>取得近 N 個月每月入職趨勢</summary>
    public async Task<List<ChartDataItem>> GetEmployeeMonthlyHireTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.HireDate.HasValue
                        && e.HireDate.Value >= startDate)
            .ToListAsync();

        var grouped = raw
            .GroupBy(e => new { e.HireDate!.Value.Year, e.HireDate.Value.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => (decimal)g.Count());

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var count);
            result.Add(new ChartDataItem
            {
                Label = date.ToString("yyyy/MM"),
                Value = count
            });
        }

        return result;
    }

    /// <summary>依年資分段統計員工人數（以 HireDate 計算）</summary>
    public async Task<List<ChartDataItem>> GetEmployeesBySeniorityAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var today = DateTime.Today;
        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.HireDate.HasValue
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive)
            .Select(e => e.HireDate!.Value)
            .ToListAsync();

        var buckets = new Dictionary<string, int>
        {
            ["未滿1年"]  = 0,
            ["1-3年"]    = 0,
            ["3-5年"]    = 0,
            ["5-10年"]   = 0,
            ["10年以上"] = 0
        };

        foreach (var hireDate in raw)
        {
            var years = (today - hireDate).TotalDays / 365.25;
            if (years < 1)       buckets["未滿1年"]++;
            else if (years < 3)  buckets["1-3年"]++;
            else if (years < 5)  buckets["3-5年"]++;
            else if (years < 10) buckets["5-10年"]++;
            else                 buckets["10年以上"]++;
        }

        return buckets
            .Select(kv => new ChartDataItem { Label = kv.Key, Value = kv.Value })
            .ToList();
    }

    /// <summary>訓練時數排行 Top N（在職員工）</summary>
    public async Task<List<ChartDataItem>> GetTopEmployeesByTrainingHoursAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.EmployeeTrainingRecords
            .Where(t => t.Status != EntityStatus.Deleted
                        && t.Employee != null
                        && t.Employee.Status != EntityStatus.Deleted
                        && t.Employee.EmploymentStatus != EmployeeStatus.Resigned
                        && t.Employee.EmploymentStatus != EmployeeStatus.Inactive)
            .ToListAsync();

        var grouped = raw
            .GroupBy(t => new { t.EmployeeId, Name = t.Employee?.Name ?? "未知" })
            .Select(g => new ChartDataItem
            {
                Label = g.Key.Name,
                Value = g.Sum(t => t.TrainingHours ?? 0)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();

        return grouped;
    }

    /// <summary>取得員工基本統計摘要</summary>
    public async Task<EmployeeChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var today = DateTime.Today;
        var expiryThreshold = today.AddDays(30);

        var total       = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted);
        var active      = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Active);
        var probation   = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == EmployeeStatus.Probation);
        var hiredThis   = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.HireDate.HasValue && e.HireDate.Value >= firstOfMonth);
        var resignedThis = await context.Employees.CountAsync(e => e.Status != EntityStatus.Deleted && e.ResignationDate.HasValue && e.ResignationDate.Value >= firstOfMonth);
        var expiring    = await context.EmployeeLicenses.CountAsync(l => l.Status != EntityStatus.Deleted && l.ExpiryDate.HasValue && l.ExpiryDate.Value >= today && l.ExpiryDate.Value <= expiryThreshold);

        return new EmployeeChartSummary
        {
            TotalEmployees    = total,
            ActiveEmployees   = active,
            ProbationEmployees = probation,
            HiredThisMonth    = hiredThis,
            ResignedThisMonth = resignedThis,
            ExpiringLicenses  = expiring
        };
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetEmployeeDetailsByDepartmentAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        if (label == "未分配")
        {
            return await context.Employees
                .Where(e => e.Status != EntityStatus.Deleted
                            && e.EmploymentStatus != EmployeeStatus.Resigned
                            && e.EmploymentStatus != EmployeeStatus.Inactive
                            && e.DepartmentId == null)
                .OrderBy(e => e.Name)
                .Select(e => new ChartDetailItem { Id = e.Id, Name = e.Name ?? "", SubLabel = e.Code ?? "" })
                .ToListAsync();
        }

        return await (
            from e in context.Employees
            where e.Status != EntityStatus.Deleted
                  && e.EmploymentStatus != EmployeeStatus.Resigned
                  && e.EmploymentStatus != EmployeeStatus.Inactive
            join d in context.Departments on e.DepartmentId equals d.Id
            where d.Name == label
            orderby e.Name
            select new ChartDetailItem { Id = e.Id, Name = e.Name ?? "", SubLabel = e.Code ?? "" }
        ).ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetEmployeeDetailsByTypeAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var typeMap = new Dictionary<string, EmployeeType>
        {
            ["正職"] = EmployeeType.FullTime,
            ["兼職"] = EmployeeType.PartTime,
            ["約聘"] = EmployeeType.Contract,
            ["實習"] = EmployeeType.Intern,
            ["派遣"] = EmployeeType.Dispatch
        };

        if (!typeMap.TryGetValue(label, out var empType))
            return new List<ChartDetailItem>();

        return await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive
                        && e.EmployeeType == empType)
            .OrderBy(e => e.Name)
            .Select(e => new ChartDetailItem { Id = e.Id, Name = e.Name ?? "", SubLabel = e.Code ?? "" })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetEmployeeDetailsByStatusAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var statusMap = new Dictionary<string, EmployeeStatus>
        {
            ["試用期"]   = EmployeeStatus.Probation,
            ["在職"]     = EmployeeStatus.Active,
            ["留職停薪"] = EmployeeStatus.LeaveOfAbsence,
            ["已離職"]   = EmployeeStatus.Resigned,
            ["停用"]     = EmployeeStatus.Inactive
        };

        if (!statusMap.TryGetValue(label, out var empStatus))
            return new List<ChartDetailItem>();

        return await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted && e.EmploymentStatus == empStatus)
            .OrderBy(e => e.Name)
            .Select(e => new ChartDetailItem { Id = e.Id, Name = e.Name ?? "", SubLabel = e.Code ?? "" })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetEmployeeDetailsByGenderAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive)
            .ToListAsync();

        Gender? targetGender = label switch
        {
            "男性" => Gender.Male,
            "女性" => Gender.Female,
            "其他" => Gender.Other,
            "未設定" => null,
            _ => (Gender?)(-1)
        };

        if ((int?)targetGender == -1)
            return new List<ChartDetailItem>();

        var filtered = label == "未設定"
            ? raw.Where(e => e.Gender == null)
            : raw.Where(e => e.Gender == targetGender);

        return filtered
            .OrderBy(e => e.Name)
            .Select(e => new ChartDetailItem { Id = e.Id, Name = e.Name ?? "", SubLabel = e.Code ?? "" })
            .ToList();
    }

    public async Task<List<ChartDetailItem>> GetEmployeeDetailsBySeniorityAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var today = DateTime.Today;
        var raw = await context.Employees
            .Where(e => e.Status != EntityStatus.Deleted
                        && e.HireDate.HasValue
                        && e.EmploymentStatus != EmployeeStatus.Resigned
                        && e.EmploymentStatus != EmployeeStatus.Inactive)
            .ToListAsync();

        var filtered = raw.Where(e =>
        {
            var years = (today - e.HireDate!.Value).TotalDays / 365.25;
            return label switch
            {
                "未滿1年"  => years < 1,
                "1-3年"    => years >= 1 && years < 3,
                "3-5年"    => years >= 3 && years < 5,
                "5-10年"   => years >= 5 && years < 10,
                "10年以上" => years >= 10,
                _          => false
            };
        });

        return filtered
            .OrderBy(e => e.HireDate)
            .Select(e => new ChartDetailItem
            {
                Id       = e.Id,
                Name     = e.Name ?? "",
                SubLabel = e.HireDate.HasValue ? e.HireDate.Value.ToString("yyyy/MM/dd") : ""
            })
            .ToList();
    }

    public async Task<List<ChartDetailItem>> GetTopEmployeeTrainingDetailsAsync(string employeeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.EmployeeTrainingRecords
            .Where(t => t.Status != EntityStatus.Deleted
                        && t.Employee != null
                        && (t.Employee.Name ?? "") == employeeLabel)
            .ToListAsync();

        return raw
            .OrderByDescending(t => t.TrainingDate)
            .Select(t => new ChartDetailItem
            {
                Id       = t.Id,
                Name     = t.CourseName,
                SubLabel = $"{t.TrainingHours?.ToString("0.#") ?? "0"} 時"
            })
            .ToList();
    }
}
