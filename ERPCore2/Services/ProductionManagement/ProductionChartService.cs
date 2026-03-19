using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.ProductionManagement;

public class ProductionChartService : IProductionChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProductionChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依生產狀態分布</summary>
    public async Task<List<ChartDataItem>> GetOrdersByStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await context.ProductionScheduleItems
            .Where(x => x.Status != EntityStatus.Deleted)
            .Select(x => new { x.ProductionItemStatus })
            .ToListAsync();

        var statusLabels = new Dictionary<ProductionItemStatus, string>
        {
            [ProductionItemStatus.Pending]   = "待生產",
            [ProductionItemStatus.InProgress] = "生產中",
            [ProductionItemStatus.Completed] = "已完成",
            [ProductionItemStatus.Closed]    = "已結案"
        };

        return rows
            .GroupBy(x => x.ProductionItemStatus)
            .Select(g => new ChartDataItem
            {
                Label = statusLabels.GetValueOrDefault(g.Key, g.Key.ToString()),
                Value = g.Count()
            })
            .OrderBy(x => Array.IndexOf(new[] { "待生產", "生產中", "已完成", "已結案" }, x.Label))
            .ToList();
    }

    /// <summary>品項排程數量排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetTopItemsByScheduledQuantityAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted
            join item in context.Items on psi.ItemId equals item.Id
            select new { ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}", psi.ScheduledQuantity }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.ItemName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.ScheduledQuantity)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>每月製令開單趨勢（近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyOrderTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.ProductionScheduleItems
            .Where(x => x.Status != EntityStatus.Deleted && x.CreatedAt >= startDate)
            .Select(x => new { x.CreatedAt })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
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

    /// <summary>依負責人員分布（製令數量）</summary>
    public async Task<List<ChartDataItem>> GetOrdersByEmployeeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted
            join emp in context.Employees on psi.ResponsibleEmployeeId equals emp.Id into eg
            from e in eg.DefaultIfEmpty()
            select new { EmployeeName = e != null ? (e.Name ?? e.Code ?? $"ID:{e.Id}") : "未指派" }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.EmployeeName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>品項完成率排行 Top N（%）</summary>
    public async Task<List<ChartDataItem>> GetCompletionRateByItemAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted && psi.ScheduledQuantity > 0
            join item in context.Items on psi.ItemId equals item.Id
            select new
            {
                ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}",
                psi.ScheduledQuantity,
                psi.CompletedQuantity
            }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.ItemName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = Math.Round(g.Sum(x => x.CompletedQuantity) / g.Sum(x => x.ScheduledQuantity) * 100, 1)
            })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>組件用料排行 Top N（依實際消耗量）</summary>
    public async Task<List<ChartDataItem>> GetTopComponentsByUsageAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from psd in context.ProductionScheduleDetails
            where psd.Status != EntityStatus.Deleted && psd.ActualUsedQty > 0
            join item in context.Items on psd.ComponentItemId equals item.Id
            select new { ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}", psd.ActualUsedQty }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.ItemName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.ActualUsedQty)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>取得生產管理統計摘要</summary>
    public async Task<ProductionChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var allItems = await context.ProductionScheduleItems
            .Where(x => x.Status != EntityStatus.Deleted)
            .Select(x => new { x.ProductionItemStatus, x.CreatedAt, x.PlannedEndDate })
            .ToListAsync();

        return new ProductionChartSummary
        {
            TotalOrdersThisMonth = allItems.Count(x => x.CreatedAt >= firstOfMonth),
            PendingOrders        = allItems.Count(x => x.ProductionItemStatus == ProductionItemStatus.Pending),
            InProgressOrders     = allItems.Count(x => x.ProductionItemStatus == ProductionItemStatus.InProgress),
            CompletedThisMonth   = allItems.Count(x => x.ProductionItemStatus == ProductionItemStatus.Completed && x.CreatedAt >= firstOfMonth),
            OverdueOrders        = allItems.Count(x =>
                x.ProductionItemStatus != ProductionItemStatus.Completed &&
                x.ProductionItemStatus != ProductionItemStatus.Closed &&
                x.PlannedEndDate.HasValue && x.PlannedEndDate.Value < now)
        };
    }

    // ===== Drill-down 明細查詢 =====

    /// <summary>依生產狀態 Drill-down：顯示該狀態下的製令清單</summary>
    public async Task<List<ChartDetailItem>> GetOrderDetailsByStatusAsync(string statusLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var statusMap = new Dictionary<string, ProductionItemStatus>
        {
            ["待生產"] = ProductionItemStatus.Pending,
            ["生產中"] = ProductionItemStatus.InProgress,
            ["已完成"] = ProductionItemStatus.Completed,
            ["已結案"] = ProductionItemStatus.Closed
        };

        if (!statusMap.TryGetValue(statusLabel, out var status))
            return new List<ChartDetailItem>();

        var raw = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted && psi.ProductionItemStatus == status
            join item in context.Items on psi.ItemId equals item.Id
            orderby psi.CreatedAt descending
            select new { psi.Id, psi.Code, ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}" }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ItemName,
            SubLabel = x.Code
        }).ToList();
    }

    /// <summary>品項排程數量 Drill-down：顯示該品項最近的製令清單</summary>
    public async Task<List<ChartDetailItem>> GetOrderDetailsByItemAsync(string itemLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted
            join item in context.Items on psi.ItemId equals item.Id
            where (item.Name ?? item.Code ?? $"ID:{item.Id}") == itemLabel
            orderby psi.CreatedAt descending
            select new { psi.Id, psi.Code, psi.ScheduledQuantity, psi.CompletedQuantity }
        ).Take(30).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.Code ?? $"ID:{x.Id}",
            SubLabel = $"{x.CompletedQuantity:N2} / {x.ScheduledQuantity:N2}"
        }).ToList();
    }

    /// <summary>依負責人員 Drill-down：顯示該人員負責的製令清單</summary>
    public async Task<List<ChartDetailItem>> GetOrderDetailsByEmployeeAsync(string employeeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        if (employeeLabel == "未指派")
        {
            var unassigned = await (
                from psi in context.ProductionScheduleItems
                where psi.Status != EntityStatus.Deleted && psi.ResponsibleEmployeeId == null
                join item in context.Items on psi.ItemId equals item.Id
                orderby psi.CreatedAt descending
                select new { psi.Id, psi.Code, ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}" }
            ).Take(30).ToListAsync();
            return unassigned.Select(x => new ChartDetailItem { Id = x.Id, Name = x.ItemName, SubLabel = x.Code }).ToList();
        }

        var raw = await (
            from psi in context.ProductionScheduleItems
            where psi.Status != EntityStatus.Deleted
            join emp in context.Employees on psi.ResponsibleEmployeeId equals emp.Id
            where (emp.Name ?? emp.Code ?? $"ID:{emp.Id}") == employeeLabel
            join item in context.Items on psi.ItemId equals item.Id
            orderby psi.CreatedAt descending
            select new { psi.Id, psi.Code, ItemName = item.Name ?? item.Code ?? $"ID:{item.Id}" }
        ).Take(30).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ItemName,
            SubLabel = x.Code
        }).ToList();
    }

    /// <summary>組件用料 Drill-down：顯示該組件的領用明細</summary>
    public async Task<List<ChartDetailItem>> GetComponentDetailsByItemAsync(string componentLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from psd in context.ProductionScheduleDetails
            where psd.Status != EntityStatus.Deleted
            join comp in context.Items on psd.ComponentItemId equals comp.Id
            where (comp.Name ?? comp.Code ?? $"ID:{comp.Id}") == componentLabel
            join psi in context.ProductionScheduleItems on psd.ProductionScheduleItemId equals psi.Id
            join item in context.Items on psi.ItemId equals item.Id
            orderby psd.ActualUsedQty descending
            select new { psd.Id, FinishedItem = item.Name ?? item.Code ?? $"ID:{item.Id}", psd.ActualUsedQty }
        ).Take(30).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.FinishedItem,
            SubLabel = $"{x.ActualUsedQty:N4}"
        }).ToList();
    }
}
