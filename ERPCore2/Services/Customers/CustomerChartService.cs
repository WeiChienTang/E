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

    /// <summary>依啟用/停用狀態統計</summary>
    public async Task<List<ChartDataItem>> GetCustomersByActiveStatusAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var active = await context.Customers.CountAsync(c => c.Status == EntityStatus.Active);
        var inactive = await context.Customers.CountAsync(c => c.Status == EntityStatus.Inactive);

        return new List<ChartDataItem>
        {
            new() { Label = "啟用中", Value = active },
            new() { Label = "已停用", Value = inactive }
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

    /// <summary>基本統計摘要</summary>
    public async Task<CustomerChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var total = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted);
        var active = await context.Customers.CountAsync(c => c.Status == EntityStatus.Active);
        var inactive = await context.Customers.CountAsync(c => c.Status == EntityStatus.Inactive);
        var thisMonth = await context.Customers.CountAsync(c => c.Status != EntityStatus.Deleted && c.CreatedAt >= firstOfMonth);
        var avgCredit = await context.Customers
            .Where(c => c.Status != EntityStatus.Deleted && c.CreditLimit.HasValue && c.CreditLimit > 0)
            .AverageAsync(c => (decimal?)c.CreditLimit);

        return new CustomerChartSummary
        {
            TotalCustomers = total,
            ActiveCustomers = active,
            InactiveCustomers = inactive,
            CustomersThisMonth = thisMonth,
            AverageCreditLimit = avgCredit.HasValue ? Math.Round(avgCredit.Value, 0) : null
        };
    }
}
