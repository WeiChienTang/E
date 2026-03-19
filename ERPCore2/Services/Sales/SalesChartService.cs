using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Sales;

public class SalesChartService : ISalesChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SalesChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依品項統計銷售金額排行 Top N（SalesDeliveryDetails，含稅計算）</summary>
    public async Task<List<ChartDataItem>> GetTopItemsBySalesAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.SalesDeliveryDetails
            where d.Status != EntityStatus.Deleted
                  && d.SalesDelivery != null
                  && d.SalesDelivery.Status != EntityStatus.Deleted
            join p in context.Items on d.ItemId equals p.Id
            select new
            {
                ItemName = p.Name ?? "未知品項",
                SubtotalAmount = d.SubtotalAmount,
                TaxRate = d.TaxRate ?? 0m
            }
        ).ToListAsync();

        var grouped = raw
            .GroupBy(x => x.ItemName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.SubtotalAmount * (1 + x.TaxRate / 100))
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();

        return grouped;
    }

    /// <summary>依業務員統計出貨金額排行 Top N（SalesDeliveries，含稅）</summary>
    public async Task<List<ChartDataItem>> GetTopEmployeesBySalesAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.SalesDeliveries
            where d.Status != EntityStatus.Deleted
            join e in context.Employees on d.SalespersonId equals e.Id into eg
            from emp in eg.DefaultIfEmpty()
            select new
            {
                EmployeeName = emp != null && emp.Name != null ? emp.Name : "其他",
                Amount = d.TotalAmount + d.TaxAmount
            }
        ).ToListAsync();

        var grouped = raw
            .GroupBy(x => x.EmployeeName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();

        return grouped;
    }

    /// <summary>近 N 個月每月出貨金額趨勢（含稅）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyDeliveryTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.SalesDeliveries
            .Where(d => d.Status != EntityStatus.Deleted && d.DeliveryDate >= startDate)
            .ToListAsync();

        var grouped = raw
            .GroupBy(d => new { d.DeliveryDate.Year, d.DeliveryDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(d => d.TotalAmount + d.TaxAmount)
            );

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var amount);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = amount });
        }

        return result;
    }

    /// <summary>依退回原因統計銷貨退回次數</summary>
    public async Task<List<ChartDataItem>> GetReturnsByReasonAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from r in context.SalesReturns
            where r.Status != EntityStatus.Deleted
            join reason in context.SalesReturnReasons on r.ReturnReasonId equals reason.Id into rg
            from returnReason in rg.DefaultIfEmpty()
            select new { ReasonName = returnReason != null ? returnReason.Name : "未分類" }
        ).ToListAsync();

        return raw
            .GroupBy(x => x.ReasonName)
            .Select(g => new ChartDataItem { Label = g.Key ?? "未分類", Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>近 N 個月每月銷貨退回金額趨勢（含稅）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyReturnTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.SalesReturns
            .Where(r => r.Status != EntityStatus.Deleted && r.ReturnDate >= startDate)
            .ToListAsync();

        var grouped = raw
            .GroupBy(r => new { r.ReturnDate.Year, r.ReturnDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(r => r.TotalReturnAmount + r.ReturnTaxAmount)
            );

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var amount);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = amount });
        }

        return result;
    }

    /// <summary>取得銷貨統計摘要</summary>
    public async Task<SalesChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstOfYear  = new DateTime(now.Year, 1, 1);

        var deliveries = await context.SalesDeliveries
            .Where(d => d.Status != EntityStatus.Deleted)
            .ToListAsync();

        var thisMonthReturns = await context.SalesReturns
            .Where(r => r.Status != EntityStatus.Deleted && r.ReturnDate >= firstOfMonth)
            .ToListAsync();

        var thisMonthOrders = await context.SalesOrders
            .Where(o => o.Status != EntityStatus.Deleted && o.OrderDate >= firstOfMonth)
            .CountAsync();

        return new SalesChartSummary
        {
            TotalDeliveriesThisMonth   = deliveries.Count(d => d.DeliveryDate >= firstOfMonth),
            ThisMonthDeliveryAmount    = deliveries.Where(d => d.DeliveryDate >= firstOfMonth).Sum(d => d.TotalAmount + d.TaxAmount),
            ThisMonthReturnAmount      = thisMonthReturns.Sum(r => r.TotalReturnAmount + r.ReturnTaxAmount),
            TotalOrdersThisMonth       = thisMonthOrders,
            PendingApprovalDeliveries  = deliveries.Count(d => !d.IsApproved && string.IsNullOrEmpty(d.RejectReason)),
            YearToDateDeliveryAmount   = deliveries.Where(d => d.DeliveryDate >= firstOfYear).Sum(d => d.TotalAmount + d.TaxAmount)
        };
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetDeliveryDetailsByItemAsync(string productLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.SalesDeliveryDetails
            where d.Status != EntityStatus.Deleted
            join p in context.Items on d.ItemId equals p.Id
            where (p.Name ?? "") == productLabel
            join sd in context.SalesDeliveries on d.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted
            orderby sd.DeliveryDate descending
            select new
            {
                sd.Id,
                sd.DeliveryDate,
                Amount = d.SubtotalAmount * (1 + (d.TaxRate ?? 0m) / 100)
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.DeliveryDate.ToString("yyyy/MM/dd"),
            SubLabel = x.Amount.ToString("N0")
        }).ToList();
    }

    public async Task<List<ChartDetailItem>> GetDeliveryDetailsByEmployeeAsync(string employeeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        if (employeeLabel == "其他")
        {
            var unassigned = await context.SalesDeliveries
                .Where(d => d.Status != EntityStatus.Deleted && d.SalespersonId == null)
                .OrderByDescending(d => d.DeliveryDate)
                .Take(50)
                .Select(d => new ChartDetailItem
                {
                    Id       = d.Id,
                    Name     = d.DeliveryDate.ToString("yyyy/MM/dd"),
                    SubLabel = (d.TotalAmount + d.TaxAmount).ToString("N0")
                })
                .ToListAsync();
            return unassigned;
        }

        var raw = await (
            from d in context.SalesDeliveries
            where d.Status != EntityStatus.Deleted
            join e in context.Employees on d.SalespersonId equals e.Id
            where (e.Name ?? "") == employeeLabel
            orderby d.DeliveryDate descending
            select new
            {
                d.Id,
                d.DeliveryDate,
                Amount = d.TotalAmount + d.TaxAmount
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.DeliveryDate.ToString("yyyy/MM/dd"),
            SubLabel = x.Amount.ToString("N0")
        }).ToList();
    }

    public async Task<List<ChartDetailItem>> GetReturnDetailsByReasonAsync(string reasonLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from r in context.SalesReturns
            where r.Status != EntityStatus.Deleted
            join reason in context.SalesReturnReasons on r.ReturnReasonId equals reason.Id into rg
            from returnReason in rg.DefaultIfEmpty()
            where (returnReason != null ? returnReason.Name : "未分類") == reasonLabel
            orderby r.ReturnDate descending
            select new
            {
                r.Id,
                r.ReturnDate,
                Amount = r.TotalReturnAmount + r.ReturnTaxAmount
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ReturnDate.ToString("yyyy/MM/dd"),
            SubLabel = x.Amount.ToString("N0")
        }).ToList();
    }

    /// <summary>本月業績達成率（%）按業務員</summary>
    public async Task<List<ChartDataItem>> GetMonthlyAchievementRateAsync()
    {
        using var context = await _factory.CreateDbContextAsync();
        var now = DateTime.Today;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // 取得本月目標
        var targets = await context.SalesTargets
            .Include(t => t.Salesperson)
            .Where(t => t.Status != EntityStatus.Deleted && t.Year == now.Year && t.Month == now.Month)
            .ToListAsync();

        if (!targets.Any())
            return new List<ChartDataItem>();

        // 取得本月實際出貨金額
        var deliveries = await context.SalesDeliveries
            .Where(d => d.Status != EntityStatus.Deleted
                     && d.DeliveryDate >= startDate && d.DeliveryDate <= endDate)
            .ToListAsync();

        var actualBySalesperson = deliveries
            .GroupBy(d => d.SalespersonId ?? 0)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.TotalAmount + d.TaxAmount));

        return targets
            .Where(t => t.TargetAmount > 0)
            .Select(t => new ChartDataItem
            {
                Label = t.SalespersonId.HasValue
                    ? (t.Salesperson?.Name ?? "其他")
                    : "公司整體",
                Value = t.TargetAmount > 0
                    ? Math.Round(actualBySalesperson.GetValueOrDefault(t.SalespersonId ?? 0, 0m) / t.TargetAmount * 100, 1)
                    : 0
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>本年度目標金額排行（按業務員）</summary>
    public async Task<List<ChartDataItem>> GetAnnualTargetByPersonAsync()
    {
        using var context = await _factory.CreateDbContextAsync();
        var year = DateTime.Today.Year;

        var targets = await context.SalesTargets
            .Include(t => t.Salesperson)
            .Where(t => t.Status != EntityStatus.Deleted && t.Year == year && t.Month == null)
            .ToListAsync();

        if (!targets.Any())
            return new List<ChartDataItem>();

        return targets
            .Select(t => new ChartDataItem
            {
                Label = t.SalespersonId.HasValue
                    ? (t.Salesperson?.Name ?? "其他")
                    : "公司整體",
                Value = t.TargetAmount
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }
}
