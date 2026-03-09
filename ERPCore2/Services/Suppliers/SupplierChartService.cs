using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Suppliers;

public class SupplierChartService : ISupplierChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SupplierChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依廠商狀態統計（正常往來/停用/暫停）</summary>
    public async Task<List<ChartDataItem>> GetSuppliersByStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var active    = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Active);
        var inactive  = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Inactive);
        var suspended = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Suspended);

        return new List<ChartDataItem>
        {
            new() { Label = "正常往來", Value = active },
            new() { Label = "停用",    Value = inactive },
            new() { Label = "暫停往來", Value = suspended }
        };
    }

    /// <summary>依廠商類型統計（製造商/貿易商/代理商/服務商/未設定）</summary>
    public async Task<List<ChartDataItem>> GetSuppliersByTypeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var suppliers = await context.Suppliers
            .Where(s => s.Status != EntityStatus.Deleted)
            .Select(s => s.SupplierType)
            .ToListAsync();

        var groups = suppliers
            .GroupBy(t => t)
            .Select(g => new ChartDataItem
            {
                Label = g.Key switch
                {
                    SupplierType.Manufacturer   => "製造商",
                    SupplierType.Trader         => "貿易商",
                    SupplierType.Agent          => "代理商",
                    SupplierType.ServiceProvider => "服務商",
                    _                           => "未設定"
                },
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        return groups;
    }

    /// <summary>依付款方式統計廠商數量</summary>
    public async Task<List<ChartDataItem>> GetSuppliersByPaymentMethodAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from s in context.Suppliers
            where s.Status != EntityStatus.Deleted
            join pm in context.PaymentMethods on s.PaymentMethodId equals pm.Id into pmg
            from payMethod in pmg.DefaultIfEmpty()
            select new { MethodName = payMethod != null ? payMethod.Name : "未設定" }
        )
        .GroupBy(x => x.MethodName)
        .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>取得近 N 個月每月新增廠商趨勢</summary>
    public async Task<List<ChartDataItem>> GetSuppliersByMonthAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.Suppliers
            .Where(s => s.Status != EntityStatus.Deleted && s.CreatedAt >= startDate)
            .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = (decimal)g.Count() })
            .ToListAsync();

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var found = raw.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
            result.Add(new ChartDataItem
            {
                Label = date.ToString("yyyy/MM"),
                Value = found?.Count ?? 0
            });
        }

        return result;
    }

    /// <summary>廠商進貨金額排行 Top N（含稅，使用進貨單）</summary>
    public async Task<List<ChartDataItem>> GetTopSuppliersByPurchaseAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PurchaseReceivings
            where pr.Status != EntityStatus.Deleted
            join s in context.Suppliers on pr.SupplierId equals s.Id
            select new { s.Id, s.CompanyName, pr.TotalAmount, pr.PurchaseReceivingTaxAmount }
        ).ToListAsync();

        return rows
            .GroupBy(x => new { x.Id, x.CompanyName })
            .Select(g => new ChartDataItem
            {
                Label = g.Key.CompanyName ?? $"ID:{g.Key.Id}",
                Value = g.Sum(x => x.TotalAmount + x.PurchaseReceivingTaxAmount)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>每月進貨金額趨勢（含稅，近 N 個月，依進貨日期）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyPurchaseTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.PurchaseReceivings
            .Where(pr => pr.Status != EntityStatus.Deleted && pr.ReceiptDate >= startDate)
            .Select(pr => new { pr.ReceiptDate, pr.TotalAmount, pr.PurchaseReceivingTaxAmount })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.ReceiptDate.Year, x.ReceiptDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.TotalAmount + x.PurchaseReceivingTaxAmount));

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var total);
            result.Add(new ChartDataItem
            {
                Label = date.ToString("yyyy/MM"),
                Value = total
            });
        }

        return result;
    }

    /// <summary>依目前應付餘額分段統計廠商數量</summary>
    public async Task<List<ChartDataItem>> GetSuppliersByCurrentPayableRangeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var payables = await context.Suppliers
            .Where(s => s.Status != EntityStatus.Deleted)
            .Select(s => s.CurrentPayable)
            .ToListAsync();

        int noPayable = 0, low = 0, mid = 0, high = 0, veryHigh = 0;
        foreach (var p in payables)
        {
            if (p == 0)              noPayable++;
            else if (p <= 100_000)   low++;
            else if (p <= 500_000)   mid++;
            else if (p <= 1_000_000) high++;
            else                     veryHigh++;
        }

        return new List<ChartDataItem>
        {
            new() { Label = "無應付",    Value = noPayable },
            new() { Label = "1~10萬",   Value = low },
            new() { Label = "10~50萬",  Value = mid },
            new() { Label = "50~100萬", Value = high },
            new() { Label = "100萬以上", Value = veryHigh }
        };
    }

    /// <summary>廠商退回金額排行 Top N（含稅，採購退出）</summary>
    public async Task<List<ChartDataItem>> GetTopSuppliersByReturnAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from pr in context.PurchaseReturns
            where pr.Status != EntityStatus.Deleted
            join s in context.Suppliers on pr.SupplierId equals s.Id
            select new { s.Id, s.CompanyName, pr.TotalReturnAmount, pr.ReturnTaxAmount }
        ).ToListAsync();

        return rows
            .GroupBy(x => new { x.Id, x.CompanyName })
            .Select(g => new ChartDataItem
            {
                Label = g.Key.CompanyName ?? $"ID:{g.Key.Id}",
                Value = g.Sum(x => x.TotalReturnAmount + x.ReturnTaxAmount)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetSupplierDetailsByStatusAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var supplierStatus = label switch
        {
            "正常往來"  => SupplierStatus.Active,
            "停用"     => SupplierStatus.Inactive,
            "暫停往來"  => SupplierStatus.Suspended,
            _          => (SupplierStatus?)null
        };

        if (supplierStatus == null) return new();

        return await context.Suppliers
            .Where(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == supplierStatus.Value)
            .OrderBy(s => s.CompanyName)
            .Select(s => new ChartDetailItem
            {
                Id       = s.Id,
                Name     = s.CompanyName!,
                SubLabel = s.Code
            })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetSupplierDetailsByTypeAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        SupplierType? supplierType = label switch
        {
            "製造商" => SupplierType.Manufacturer,
            "貿易商" => SupplierType.Trader,
            "代理商" => SupplierType.Agent,
            "服務商" => SupplierType.ServiceProvider,
            _       => null
        };

        IQueryable<ERPCore2.Data.Entities.Supplier> query;
        if (label == "未設定")
        {
            query = context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.SupplierType == null);
        }
        else if (supplierType.HasValue)
        {
            query = context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.SupplierType == supplierType.Value);
        }
        else
        {
            return new();
        }

        return await query
            .OrderBy(s => s.CompanyName)
            .Select(s => new ChartDetailItem
            {
                Id       = s.Id,
                Name     = s.CompanyName!,
                SubLabel = s.Code
            })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetSupplierDetailsByPaymentMethodAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Supplier> query;
        if (label == "未設定")
        {
            query = context.Suppliers
                .Where(s => s.Status != EntityStatus.Deleted && s.PaymentMethodId == null);
        }
        else
        {
            query = from s in context.Suppliers
                    where s.Status != EntityStatus.Deleted
                    join pm in context.PaymentMethods on s.PaymentMethodId equals pm.Id
                    where pm.Name == label
                    select s;
        }

        return await query
            .OrderBy(s => s.CompanyName)
            .Select(s => new ChartDetailItem
            {
                Id       = s.Id,
                Name     = s.CompanyName!,
                SubLabel = s.Code
            })
            .ToListAsync();
    }

    /// <summary>點擊進貨排行廠商 → 顯示該廠商的進貨明細（最近 20 筆）</summary>
    public async Task<List<ChartDetailItem>> GetTopSupplierPurchaseReceivingDetailsAsync(string supplierLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from pr in context.PurchaseReceivings
            where pr.Status != EntityStatus.Deleted
            join s in context.Suppliers on pr.SupplierId equals s.Id
            where s.CompanyName == supplierLabel
            orderby pr.ReceiptDate descending
            select new { pr.Id, pr.ReceiptDate, Amount = pr.TotalAmount + pr.PurchaseReceivingTaxAmount }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ReceiptDate.ToString("yyyy/MM/dd"),
            SubLabel = $"NT${x.Amount:N0}"
        }).ToList();
    }

    /// <summary>點擊應付餘額分布區間 → 顯示該區間廠商清單</summary>
    public async Task<List<ChartDetailItem>> GetSuppliersByCurrentPayableRangeDetailsAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Supplier> query = label switch
        {
            "無應付"    => context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.CurrentPayable == 0),
            "1~10萬"   => context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.CurrentPayable > 0 && s.CurrentPayable <= 100_000),
            "10~50萬"  => context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.CurrentPayable > 100_000 && s.CurrentPayable <= 500_000),
            "50~100萬" => context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.CurrentPayable > 500_000 && s.CurrentPayable <= 1_000_000),
            "100萬以上" => context.Suppliers.Where(s => s.Status != EntityStatus.Deleted && s.CurrentPayable > 1_000_000),
            _           => context.Suppliers.Where(s => false)
        };

        var raw = await query
            .OrderByDescending(s => s.CurrentPayable)
            .Select(s => new { s.Id, s.CompanyName, s.CurrentPayable })
            .ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.CompanyName!,
            SubLabel = $"NT${x.CurrentPayable:N0}"
        }).ToList();
    }

    /// <summary>點擊退回排行廠商 → 顯示該廠商的退回明細（最近 20 筆）</summary>
    public async Task<List<ChartDetailItem>> GetTopSupplierReturnDetailsAsync(string supplierLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from pr in context.PurchaseReturns
            where pr.Status != EntityStatus.Deleted
            join s in context.Suppliers on pr.SupplierId equals s.Id
            where s.CompanyName == supplierLabel
            orderby pr.ReturnDate descending
            select new { pr.Id, pr.ReturnDate, Amount = pr.TotalReturnAmount + pr.ReturnTaxAmount }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ReturnDate.ToString("yyyy/MM/dd"),
            SubLabel = $"NT${x.Amount:N0}"
        }).ToList();
    }

    /// <summary>基本統計摘要</summary>
    public async Task<SupplierChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var total     = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted);
        var active    = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Active);
        var inactive  = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Inactive);
        var suspended = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.SupplierStatus == SupplierStatus.Suspended);
        var thisMonth = await context.Suppliers.CountAsync(s => s.Status != EntityStatus.Deleted && s.CreatedAt >= firstOfMonth);
        var totalPayable = await context.Suppliers
            .Where(s => s.Status != EntityStatus.Deleted)
            .SumAsync(s => s.CurrentPayable);

        return new SupplierChartSummary
        {
            TotalSuppliers    = total,
            ActiveSuppliers   = active,
            InactiveSuppliers = inactive,
            SuspendedSuppliers = suspended,
            SuppliersThisMonth = thisMonth,
            TotalPayable      = totalPayable
        };
    }
}
