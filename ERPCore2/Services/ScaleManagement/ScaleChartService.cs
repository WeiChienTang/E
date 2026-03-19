using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.ScaleManagement;

public class ScaleChartService : IScaleChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ScaleChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依品項過磅淨重排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetNetWeightByItemAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from sr in context.ScaleRecords
            where sr.Status != EntityStatus.Deleted && sr.ItemId != null && sr.NetWeight.HasValue
            join item in context.Items on sr.ItemId equals item.Id
            select new { ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}", sr.NetWeight }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.ItemName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.NetWeight ?? 0)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>每月過磅淨重趨勢（近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyWeightTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.ScaleRecords
            .Where(x => x.Status != EntityStatus.Deleted && x.RecordDate >= startDate && x.NetWeight.HasValue)
            .Select(x => new { x.RecordDate, x.NetWeight })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.RecordDate.Year, x.RecordDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.NetWeight ?? 0));

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var total);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = total });
        }
        return result;
    }

    /// <summary>依客戶淨收益排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetNetAmountByCustomerAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from sr in context.ScaleRecords
            where sr.Status != EntityStatus.Deleted && sr.CustomerId != null && sr.NetAmount.HasValue
            join c in context.Customers on sr.CustomerId equals c.Id
            select new { CustomerName = c.CompanyName ?? c.Code ?? $"ID:{c.Id}", sr.NetAmount }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.CustomerName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.NetAmount ?? 0)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>每月淨收益趨勢（近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyRevenueTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.ScaleRecords
            .Where(x => x.Status != EntityStatus.Deleted && x.RecordDate >= startDate && x.NetAmount.HasValue)
            .Select(x => new { x.RecordDate, x.NetAmount })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.RecordDate.Year, x.RecordDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.NetAmount ?? 0));

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var total);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = total });
        }
        return result;
    }

    /// <summary>每月過磅筆數趨勢（近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyRecordCountAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.ScaleRecords
            .Where(x => x.Status != EntityStatus.Deleted && x.RecordDate >= startDate)
            .Select(x => new { x.RecordDate })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.RecordDate.Year, x.RecordDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => (decimal)g.Count());

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var count);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = count });
        }
        return result;
    }

    /// <summary>取得磅秤管理統計摘要</summary>
    public async Task<ScaleChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var thisMonth = await context.ScaleRecords
            .Where(x => x.Status != EntityStatus.Deleted && x.RecordDate >= firstOfMonth)
            .Select(x => new { x.NetWeight, x.NetAmount, x.CustomerId, x.ItemId })
            .ToListAsync();

        return new ScaleChartSummary
        {
            TotalRecordsThisMonth    = thisMonth.Count,
            TotalNetWeightThisMonth  = thisMonth.Sum(x => x.NetWeight ?? 0),
            TotalNetAmountThisMonth  = thisMonth.Sum(x => x.NetAmount ?? 0),
            UniqueCustomersThisMonth = thisMonth.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId).Distinct().Count(),
            UniqueItemsThisMonth     = thisMonth.Where(x => x.ItemId.HasValue).Select(x => x.ItemId).Distinct().Count()
        };
    }

    // ===== Drill-down 明細查詢 =====

    /// <summary>依品項 Drill-down：顯示該品項最近過磅記錄</summary>
    public async Task<List<ChartDetailItem>> GetRecordDetailsByItemAsync(string itemLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sr in context.ScaleRecords
            where sr.Status != EntityStatus.Deleted && sr.ItemId != null
            join item in context.Items on sr.ItemId equals item.Id
            where (item.Name ?? item.Code ?? $"ID:{item.Id}") == itemLabel
            orderby sr.RecordDate descending
            select new { sr.Id, sr.RecordDate, sr.NetWeight }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.RecordDate.ToString("yyyy/MM/dd"),
            SubLabel = $"{x.NetWeight:N2} kg"
        }).ToList();
    }

    /// <summary>依客戶 Drill-down：顯示該客戶最近過磅記錄</summary>
    public async Task<List<ChartDetailItem>> GetRecordDetailsByCustomerAsync(string customerLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sr in context.ScaleRecords
            where sr.Status != EntityStatus.Deleted && sr.CustomerId != null
            join c in context.Customers on sr.CustomerId equals c.Id
            where (c.CompanyName ?? c.Code ?? $"ID:{c.Id}") == customerLabel
            orderby sr.RecordDate descending
            select new { sr.Id, sr.RecordDate, sr.NetAmount }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.RecordDate.ToString("yyyy/MM/dd"),
            SubLabel = $"NT${x.NetAmount:N0}"
        }).ToList();
    }
}
