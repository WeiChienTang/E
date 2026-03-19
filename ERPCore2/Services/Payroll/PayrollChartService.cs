using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Payroll;

public class PayrollChartService : IPayrollChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PayrollChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>每月薪資總支出趨勢（近 N 個月，以 PayrollPeriod.Year/Month 計）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyPayrollTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        // PayrollPeriod.Year 為民國年，轉 AD: year + 1911
        var now = DateTime.Now;
        var rocYear = now.Year - 1911;
        var startRocYear = rocYear - (months / 12) - 1;

        var rows = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join pp in context.PayrollPeriods on pr.PayrollPeriodId equals pp.Id
            where pp.Year >= startRocYear
            select new { pp.Year, pp.Month, pr.NetPay }
        ).ToListAsync();

        // 建立近 N 個月的 AD 年月清單
        var adMonths = new List<(int Year, int Month)>();
        for (int i = months - 1; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            adMonths.Add((d.Year, d.Month));
        }

        var grouped = rows
            .GroupBy(x => (RocYear: x.Year, x.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.NetPay));

        return adMonths.Select(ad =>
        {
            var rocY = ad.Year - 1911;
            grouped.TryGetValue((rocY, ad.Month), out var total);
            return new ChartDataItem { Label = $"{ad.Year:D4}/{ad.Month:D2}", Value = total };
        }).ToList();
    }

    /// <summary>依部門應發薪資分布</summary>
    public async Task<List<ChartDataItem>> GetGrossIncomeByDepartmentAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            join dept in context.Departments on emp.DepartmentId equals dept.Id into dg
            from d in dg.DefaultIfEmpty()
            select new { DepartmentName = d != null ? d.Name : "未分配部門", pr.GrossIncome }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.DepartmentName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.GrossIncome)
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>員工實發薪資排行 Top N（彙總所有期間）</summary>
    public async Task<List<ChartDataItem>> GetTopEarnersAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            select new { EmployeeName = emp.Name ?? emp.Code ?? $"ID:{emp.Id}", pr.NetPay }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.EmployeeName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.NetPay)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>薪資單狀態分布</summary>
    public async Task<List<ChartDataItem>> GetRecordStatusDistributionAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await context.PayrollRecords
            .Where(x => x.Status != EntityStatus.Deleted)
            .Select(x => new { x.RecordStatus })
            .ToListAsync();

        var statusLabels = new Dictionary<PayrollRecordStatus, string>
        {
            [PayrollRecordStatus.Draft]     = "試算中",
            [PayrollRecordStatus.Confirmed] = "已確認"
        };

        return rows
            .GroupBy(x => x.RecordStatus)
            .Select(g => new ChartDataItem
            {
                Label = statusLabels.GetValueOrDefault(g.Key, g.Key.ToString()),
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>員工加班時數排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetTopOvertimeByEmployeeAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            select new
            {
                EmployeeName = emp.Name ?? emp.Code ?? $"ID:{emp.Id}",
                pr.OvertimeHours1,
                pr.OvertimeHours2,
                pr.HolidayOvertimeHours,
                pr.NationalHolidayHours
            }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.EmployeeName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = (decimal)g.Sum(x => x.OvertimeHours1 + x.OvertimeHours2 + x.HolidayOvertimeHours + x.NationalHolidayHours)
            })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>取得薪資管理統計摘要</summary>
    public async Task<PayrollChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var rocYear = now.Year - 1911;
        var rocMonth = now.Month;

        var thisMonthRecords = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join pp in context.PayrollPeriods on pr.PayrollPeriodId equals pp.Id
            where pp.Year == rocYear && pp.Month == rocMonth
            select new { pr.GrossIncome, pr.NetPay, pr.RecordStatus }
        ).ToListAsync();

        var draftCount = await context.PayrollRecords
            .CountAsync(x => x.Status != EntityStatus.Deleted && x.RecordStatus == PayrollRecordStatus.Draft);

        return new PayrollChartSummary
        {
            TotalRecordsThisMonth    = thisMonthRecords.Count,
            TotalGrossIncomeThisMonth = thisMonthRecords.Sum(x => x.GrossIncome),
            TotalNetPayThisMonth     = thisMonthRecords.Sum(x => x.NetPay),
            ApprovedRecordsCount     = thisMonthRecords.Count(x => x.RecordStatus == PayrollRecordStatus.Confirmed),
            DraftRecordsCount        = draftCount
        };
    }

    // ===== Drill-down 明細查詢 =====

    /// <summary>依部門 Drill-down：顯示該部門員工薪資清單</summary>
    public async Task<List<ChartDetailItem>> GetEmployeeDetailsByDepartmentAsync(string departmentLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            join dept in context.Departments on emp.DepartmentId equals dept.Id into dg
            from d in dg.DefaultIfEmpty()
            where (d != null ? d.Name : "未分配部門") == departmentLabel
            select new { emp.Id, EmployeeName = emp.Name ?? emp.Code ?? $"ID:{emp.Id}", pr.NetPay }
        ).ToListAsync();

        return rows
            .GroupBy(x => new { x.Id, x.EmployeeName })
            .Select(g => new ChartDetailItem
            {
                Id       = g.Key.Id,
                Name     = g.Key.EmployeeName,
                SubLabel = $"NT${g.Sum(x => x.NetPay):N0}"
            })
            .OrderByDescending(x => x.SubLabel)
            .ToList();
    }

    /// <summary>薪資排行 Drill-down：顯示該員工的薪資期間記錄</summary>
    public async Task<List<ChartDetailItem>> GetPayrollDetailsByEmployeeAsync(string employeeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            where (emp.Name ?? emp.Code ?? $"ID:{emp.Id}") == employeeLabel
            join pp in context.PayrollPeriods on pr.PayrollPeriodId equals pp.Id
            orderby pp.Year descending, pp.Month descending
            select new { pr.Id, pp.Year, pp.Month, pr.NetPay }
        ).Take(24).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = $"{x.Year + 1911:D4}/{x.Month:D2}",
            SubLabel = $"NT${x.NetPay:N0}"
        }).ToList();
    }

    /// <summary>依薪資單狀態 Drill-down</summary>
    public async Task<List<ChartDetailItem>> GetRecordDetailsByStatusAsync(string statusLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var statusMap = new Dictionary<string, PayrollRecordStatus>
        {
            ["試算中"] = PayrollRecordStatus.Draft,
            ["已確認"] = PayrollRecordStatus.Confirmed
        };

        if (!statusMap.TryGetValue(statusLabel, out var status))
            return new List<ChartDetailItem>();

        var raw = await (
            from pr in context.PayrollRecords
            where pr.Status != EntityStatus.Deleted && pr.RecordStatus == status
            join emp in context.Employees on pr.EmployeeId equals emp.Id
            join pp in context.PayrollPeriods on pr.PayrollPeriodId equals pp.Id
            orderby pp.Year descending, pp.Month descending
            select new { pr.Id, EmployeeName = emp.Name ?? emp.Code ?? $"ID:{emp.Id}", pp.Year, pp.Month }
        ).Take(30).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.EmployeeName,
            SubLabel = $"{x.Year + 1911:D4}/{x.Month:D2}"
        }).ToList();
    }
}
