using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Customers;

public class CustomerChartService : ICustomerChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CustomerChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依業務負責人統計客戶數量（左外連接 Employee）</summary>
    public async Task<List<ChartDataItem>> GetCustomersByAccountManagerAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from c in context.Customers
            where c.Status != EntityStatus.Deleted
            join e in context.Employees on c.EmployeeId equals e.Id into eg
            from emp in eg.DefaultIfEmpty()
            select new { ManagerName = emp != null && emp.Name != null ? emp.Name : "未分配" }
        )
        .GroupBy(x => x.ManagerName)
        .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>依付款方式統計客戶數量</summary>
    public async Task<List<ChartDataItem>> GetCustomersByPaymentMethodAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from c in context.Customers
            where c.Status != EntityStatus.Deleted
            join pm in context.PaymentMethods on c.PaymentMethodId equals pm.Id into pmg
            from payMethod in pmg.DefaultIfEmpty()
            select new { MethodName = payMethod != null ? payMethod.Name : "未設定" }
        )
        .GroupBy(x => x.MethodName)
        .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>取得近 N 個月每月新增客戶趨勢</summary>
    public async Task<List<ChartDataItem>> GetCustomersByMonthAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var raw = await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted && c.CreatedAt >= startDate)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = (decimal)g.Count() })
            .ToListAsync();

        // 補齊缺少的月份
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

    /// <summary>依客戶狀態統計（正常往來/停用/黑名單）</summary>
    public async Task<List<ChartDataItem>> GetCustomersByCustomerStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var active      = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Active);
        var inactive    = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Inactive);
        var blacklisted = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Blacklisted);

        return new List<ChartDataItem>
        {
            new() { Label = "正常往來", Value = active },
            new() { Label = "停用",    Value = inactive },
            new() { Label = "黑名單",  Value = blacklisted }
        };
    }

    /// <summary>依信用額度分段統計</summary>
    public async Task<List<ChartDataItem>> GetCustomersByCreditLimitRangeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var limits = await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted)
            .Select(c => c.CreditLimit)
            .ToListAsync();

        int noLimit = 0, low = 0, mid = 0, high = 0;
        foreach (var credit in limits)
        {
            if (!credit.HasValue || credit == 0)
                noLimit++;
            else if (credit <= 100_000)
                low++;
            else if (credit <= 500_000)
                mid++;
            else
                high++;
        }

        return new List<ChartDataItem>
        {
            new() { Label = "無設定", Value = noLimit },
            new() { Label = "1~10萬", Value = low },
            new() { Label = "10~50萬", Value = mid },
            new() { Label = "50萬以上", Value = high }
        };
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetCustomerDetailsByPaymentMethodAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Customer> query;
        if (label == "未設定")
        {
            query = context.Customers
                .Where(c => c.Status != EntityStatus.Deleted && c.PaymentMethodId == null);
        }
        else
        {
            query = from c in context.Customers
                    where c.Status != EntityStatus.Deleted
                    join pm in context.PaymentMethods on c.PaymentMethodId equals pm.Id
                    where pm.Name == label
                    select c;
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new ChartDetailItem
            {
                Id = c.Id,
                Name = c.CompanyName ?? c.Code ?? $"ID:{c.Id}",
                SubLabel = c.Code
            })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetCustomerDetailsByAccountManagerAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Customer> query;
        if (label == "未分配")
        {
            query = context.Customers
                .Where(c => c.Status != EntityStatus.Deleted && c.EmployeeId == null);
        }
        else
        {
            query = from c in context.Customers
                    where c.Status != EntityStatus.Deleted
                    join e in context.Employees on c.EmployeeId equals e.Id
                    where e.Name == label
                    select c;
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new ChartDetailItem
            {
                Id = c.Id,
                Name = c.CompanyName ?? c.Code ?? $"ID:{c.Id}",
                SubLabel = c.Code
            })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetCustomerDetailsByStatusAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var customerStatus = label switch
        {
            "正常往來" => CustomerStatus.Active,
            "停用"    => CustomerStatus.Inactive,
            "黑名單"  => CustomerStatus.Blacklisted,
            _         => (CustomerStatus?)null
        };

        if (customerStatus == null) return new();

        return await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == customerStatus.Value)
            .OrderBy(c => c.Code)
            .Select(c => new ChartDetailItem
            {
                Id = c.Id,
                Name = c.CompanyName ?? c.Code ?? $"ID:{c.Id}",
                SubLabel = c.Code
            })
            .ToListAsync();
    }

    public async Task<List<ChartDetailItem>> GetCustomerDetailsByCreditLimitRangeAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Customer> query = label switch
        {
            "無設定"  => context.Customers.Where(c => c.Status != EntityStatus.Deleted && (!c.CreditLimit.HasValue || c.CreditLimit == 0)),
            "1~10萬" => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CreditLimit > 0 && c.CreditLimit <= 100_000),
            "10~50萬" => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CreditLimit > 100_000 && c.CreditLimit <= 500_000),
            "50萬以上" => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CreditLimit > 500_000),
            _ => context.Customers.Where(c => false)
        };

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new ChartDetailItem
            {
                Id = c.Id,
                Name = c.CompanyName ?? c.Code ?? $"ID:{c.Id}",
                SubLabel = c.Code
            })
            .ToListAsync();
    }

    // ===== 金錢數據圖表 =====

    /// <summary>客戶銷售金額排行 Top N（依含稅總額）</summary>
    public async Task<List<ChartDataItem>> GetTopCustomersBySalesAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        // Group key 不能含字串插補（EF Core 無法翻譯），改為分開欄位後記憶體格式化
        var result = await (
            from so in context.SalesOrders
            where so.Status != EntityStatus.Deleted
            join c in context.Customers on so.CustomerId equals c.Id
            group so by new { c.Id, c.CompanyName, c.Code } into g
            select new { g.Key.Id, g.Key.CompanyName, g.Key.Code, Total = g.Sum(o => o.TotalAmount + o.SalesTaxAmount) }
        )
        .OrderByDescending(x => x.Total)
        .Take(top)
        .ToListAsync();

        return result.Select(x => new ChartDataItem
        {
            Label = x.CompanyName ?? x.Code ?? $"ID:{x.Id}",
            Value = x.Total
        }).ToList();
    }

    /// <summary>每月銷售收入趨勢（含稅，近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        // 先取出必要欄位，再於記憶體中分組加總，避免 EF Core 對 GroupBy+Sum(算術) 的翻譯問題
        var rows = await context.SalesOrders
            .Where(so => so.Status != EntityStatus.Deleted && so.OrderDate >= startDate)
            .Select(so => new { so.OrderDate, so.TotalAmount, so.SalesTaxAmount })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.TotalAmount + x.SalesTaxAmount));

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

    /// <summary>依目前應收餘額分段統計客戶數量</summary>
    public async Task<List<ChartDataItem>> GetCustomersByCurrentBalanceRangeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var balances = await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted)
            .Select(c => c.CurrentBalance)
            .ToListAsync();

        int noBalance = 0, low = 0, mid = 0, high = 0, veryHigh = 0;
        foreach (var b in balances)
        {
            if (b == 0)               noBalance++;
            else if (b <= 100_000)    low++;
            else if (b <= 500_000)    mid++;
            else if (b <= 1_000_000)  high++;
            else                      veryHigh++;
        }

        return new List<ChartDataItem>
        {
            new() { Label = "無應收",    Value = noBalance },
            new() { Label = "1~10萬",   Value = low },
            new() { Label = "10~50萬",  Value = mid },
            new() { Label = "50~100萬", Value = high },
            new() { Label = "100萬以上", Value = veryHigh }
        };
    }

    /// <summary>客戶退貨金額排行 Top N（依含稅退回總額）</summary>
    public async Task<List<ChartDataItem>> GetTopCustomersByReturnAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from sr in context.SalesReturns
            where sr.Status != EntityStatus.Deleted
            join c in context.Customers on sr.CustomerId equals c.Id
            group sr by new { c.Id, c.CompanyName, c.Code } into g
            select new { g.Key.Id, g.Key.CompanyName, g.Key.Code, Total = g.Sum(r => r.TotalReturnAmount + r.ReturnTaxAmount) }
        )
        .OrderByDescending(x => x.Total)
        .Take(top)
        .ToListAsync();

        return result.Select(x => new ChartDataItem
        {
            Label = x.CompanyName ?? x.Code ?? $"ID:{x.Id}",
            Value = x.Total
        }).ToList();
    }

    // ===== 金錢數據 Drill-down 明細 =====

    /// <summary>點擊銷售排行客戶 → 顯示該客戶的訂單明細（最近 20 筆）</summary>
    public async Task<List<ChartDetailItem>> GetTopCustomerSalesOrderDetailsAsync(string customerLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        // WHERE 條件不能使用字串插補，改為先比 CompanyName，若為 null 再比 Code
        var raw = await (
            from so in context.SalesOrders
            where so.Status != EntityStatus.Deleted
            join c in context.Customers on so.CustomerId equals c.Id
            where c.CompanyName == customerLabel || (c.CompanyName == null && c.Code == customerLabel)
            orderby so.OrderDate descending
            select new { so.Id, so.OrderDate, Amount = so.TotalAmount + so.SalesTaxAmount }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.OrderDate.ToString("yyyy/MM/dd"),
            SubLabel = $"NT${x.Amount:N0}"
        }).ToList();
    }

    /// <summary>點擊應收餘額分布區間 → 顯示該區間客戶清單</summary>
    public async Task<List<ChartDetailItem>> GetCustomersByCurrentBalanceRangeDetailsAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Customer> query = label switch
        {
            "無應收"    => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CurrentBalance == 0),
            "1~10萬"   => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CurrentBalance > 0 && c.CurrentBalance <= 100_000),
            "10~50萬"  => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CurrentBalance > 100_000 && c.CurrentBalance <= 500_000),
            "50~100萬" => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CurrentBalance > 500_000 && c.CurrentBalance <= 1_000_000),
            "100萬以上" => context.Customers.Where(c => c.Status != EntityStatus.Deleted && c.CurrentBalance > 1_000_000),
            _           => context.Customers.Where(c => false)
        };

        var raw = await query
            .OrderByDescending(c => c.CurrentBalance)
            .Select(c => new { c.Id, c.CompanyName, c.Code, c.CurrentBalance })
            .ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.CompanyName ?? x.Code ?? $"ID:{x.Id}",
            SubLabel = $"NT${x.CurrentBalance:N0}"
        }).ToList();
    }

    /// <summary>點擊退貨排行客戶 → 顯示該客戶的退貨明細（最近 20 筆）</summary>
    public async Task<List<ChartDetailItem>> GetTopCustomerReturnDetailsAsync(string customerLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sr in context.SalesReturns
            where sr.Status != EntityStatus.Deleted
            join c in context.Customers on sr.CustomerId equals c.Id
            where c.CompanyName == customerLabel || (c.CompanyName == null && c.Code == customerLabel)
            orderby sr.ReturnDate descending
            select new { sr.Id, sr.ReturnDate, Amount = sr.TotalReturnAmount + sr.ReturnTaxAmount }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ReturnDate.ToString("yyyy/MM/dd"),
            SubLabel = $"NT${x.Amount:N0}"
        }).ToList();
    }

    /// <summary>基本統計摘要</summary>
    public async Task<CustomerChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var total = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted);
        var active      = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Active);
        var inactive    = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Inactive);
        var blacklisted = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CustomerStatus == CustomerStatus.Blacklisted);
        var thisMonth = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CreatedAt >= firstOfMonth);
        var avgCredit = await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted && c.CreditLimit.HasValue && c.CreditLimit > 0)
            .AverageAsync(c => (decimal?)c.CreditLimit);

        return new CustomerChartSummary
        {
            TotalCustomers = total,
            ActiveCustomers = active,
            InactiveCustomers = inactive,
            BlacklistedCustomers = blacklisted,
            CustomersThisMonth = thisMonth,
            AverageCreditLimit = avgCredit.HasValue ? Math.Round(avgCredit.Value, 0) : null
        };
    }
}
