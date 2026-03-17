using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Purchase;

public class PurchaseChartService : IPurchaseChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PurchaseChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依品項統計進貨金額排行 Top N（PurchaseReceivingDetails，含稅計算）</summary>
    public async Task<List<ChartDataItem>> GetTopProductsByReceivingAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.PurchaseReceivingDetails
            where d.Status != EntityStatus.Deleted
                  && d.PurchaseReceiving != null
                  && d.PurchaseReceiving.Status != EntityStatus.Deleted
            join p in context.Products on d.ProductId equals p.Id
            select new
            {
                ProductName = p.Name ?? "未知品項",
                Amount = d.UnitPrice * d.ReceivedQuantity,
                TaxRate = d.TaxRate ?? 0m
            }
        ).ToListAsync();

        var grouped = raw
            .GroupBy(x => x.ProductName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.Amount * (1 + x.TaxRate / 100))
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();

        return grouped;
    }

    /// <summary>近 N 個月每月進貨金額趨勢（含稅）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyReceivingTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.PurchaseReceivings
            .Where(r => r.Status != EntityStatus.Deleted && r.ReceiptDate >= startDate)
            .ToListAsync();

        var grouped = raw
            .GroupBy(r => new { r.ReceiptDate.Year, r.ReceiptDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(r => r.TotalAmount + r.PurchaseReceivingTaxAmount)
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

    /// <summary>採購訂單核准狀態分布</summary>
    public async Task<List<ChartDataItem>> GetOrderApprovalStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.PurchaseOrders
            .Where(o => o.Status != EntityStatus.Deleted)
            .ToListAsync();

        var approved  = raw.Count(o => o.IsApproved);
        var rejected  = raw.Count(o => !o.IsApproved && !string.IsNullOrEmpty(o.RejectReason));
        var pending   = raw.Count(o => !o.IsApproved && string.IsNullOrEmpty(o.RejectReason));

        return new List<ChartDataItem>
        {
            new() { Label = "待審核", Value = pending },
            new() { Label = "已核准", Value = approved },
            new() { Label = "已拒絕", Value = rejected }
        }.Where(x => x.Value > 0).ToList();
    }

    /// <summary>依退回原因統計採購退回次數</summary>
    public async Task<List<ChartDataItem>> GetReturnsByReasonAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from r in context.PurchaseReturns
            where r.Status != EntityStatus.Deleted
            join reason in context.PurchaseReturnReasons on r.ReturnReasonId equals reason.Id into rg
            from returnReason in rg.DefaultIfEmpty()
            select new { ReasonName = returnReason != null ? returnReason.Name : "未分類" }
        ).ToListAsync();

        return raw
            .GroupBy(x => x.ReasonName)
            .Select(g => new ChartDataItem { Label = g.Key ?? "未分類", Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>近 N 個月每月採購退回金額趨勢（含稅）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyReturnTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.PurchaseReturns
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

    /// <summary>取得採購統計摘要</summary>
    public async Task<PurchaseChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var allOrders = await context.PurchaseOrders
            .Where(o => o.Status != EntityStatus.Deleted)
            .ToListAsync();

        var thisMonthReceivings = await context.PurchaseReceivings
            .Where(r => r.Status != EntityStatus.Deleted && r.ReceiptDate >= firstOfMonth)
            .ToListAsync();

        var thisMonthReturns = await context.PurchaseReturns
            .Where(r => r.Status != EntityStatus.Deleted && r.ReturnDate >= firstOfMonth)
            .ToListAsync();

        return new PurchaseChartSummary
        {
            TotalOrdersThisMonth      = allOrders.Count(o => o.OrderDate >= firstOfMonth),
            PendingApprovalOrders     = allOrders.Count(o => !o.IsApproved && string.IsNullOrEmpty(o.RejectReason)),
            ApprovedOrders            = allOrders.Count(o => o.IsApproved),
            ThisMonthReceivingAmount  = thisMonthReceivings.Sum(r => r.TotalAmount + r.PurchaseReceivingTaxAmount),
            ThisMonthReturnAmount     = thisMonthReturns.Sum(r => r.TotalReturnAmount + r.ReturnTaxAmount),
            TotalReceivingsThisMonth  = thisMonthReceivings.Count
        };
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetReceivingDetailsByProductAsync(string productLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.PurchaseReceivingDetails
            where d.Status != EntityStatus.Deleted
            join p in context.Products on d.ProductId equals p.Id
            where (p.Name ?? "") == productLabel
            join r in context.PurchaseReceivings on d.PurchaseReceivingId equals r.Id
            where r.Status != EntityStatus.Deleted
            orderby r.ReceiptDate descending
            select new
            {
                r.Id,
                r.Code,
                r.ReceiptDate,
                Amount = (d.UnitPrice * d.ReceivedQuantity) * (1 + (d.TaxRate ?? 0m) / 100)
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ReceiptDate.ToString("yyyy/MM/dd"),
            SubLabel = x.Amount.ToString("N0")
        }).ToList();
    }

    public async Task<List<ChartDetailItem>> GetOrderDetailsByApprovalStatusAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await context.PurchaseOrders
            .Where(o => o.Status != EntityStatus.Deleted)
            .ToListAsync();

        var filtered = label switch
        {
            "待審核" => raw.Where(o => !o.IsApproved && string.IsNullOrEmpty(o.RejectReason)),
            "已核准" => raw.Where(o => o.IsApproved),
            "已拒絕" => raw.Where(o => !o.IsApproved && !string.IsNullOrEmpty(o.RejectReason)),
            _        => Enumerable.Empty<Data.Entities.PurchaseOrder>()
        };

        return filtered
            .OrderByDescending(o => o.OrderDate)
            .Take(50)
            .Select(o => new ChartDetailItem
            {
                Id       = o.Id,
                Name     = o.Code ?? "",
                SubLabel = o.OrderDate.ToString("yyyy/MM/dd")
            })
            .ToList();
    }

    public async Task<List<ChartDetailItem>> GetReturnDetailsByReasonAsync(string reasonLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from r in context.PurchaseReturns
            where r.Status != EntityStatus.Deleted
            join reason in context.PurchaseReturnReasons on r.ReturnReasonId equals reason.Id into rg
            from returnReason in rg.DefaultIfEmpty()
            where (returnReason != null ? returnReason.Name : "未分類") == reasonLabel
            orderby r.ReturnDate descending
            select new
            {
                r.Id,
                r.Code,
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
}
