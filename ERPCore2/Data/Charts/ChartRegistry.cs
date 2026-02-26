using ApexCharts;
using ERPCore2.Models.Charts;
using ERPCore2.Services.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Data.Charts;

/// <summary>全域圖表登記表，類比 ReportRegistry</summary>
public static class ChartRegistry
{
    private static readonly List<ChartDefinition> _definitions = new();
    private static bool _initialized = false;
    private static readonly object _lock = new();

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            Initialize();
            _initialized = true;
        }
    }

    private static void Initialize()
    {
        // ===== 客戶模組圖表 =====

        _definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByAccountManager,
            Title              = "依業務負責人分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByAccountManagerAsync()
        });

        _definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByPaymentMethod,
            Title              = "依付款方式分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByPaymentMethodAsync()
        });

        _definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerMonthlyTrend,
            Title              = "每月新增趨勢",
            Category           = ChartCategory.Customer,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByMonthAsync()
        });

        _definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByStatus,
            Title              = "啟用/停用狀態",
            Category           = ChartCategory.Customer,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.RadialBar, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByActiveStatusAsync()
        });

        _definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByCreditLimit,
            Title              = "信用額度分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByCreditLimitRangeAsync()
        });
    }

    /// <summary>取得指定分類的圖表定義（依 SortOrder 排序）</summary>
    public static List<ChartDefinition> GetByCategory(string category)
    {
        EnsureInitialized();
        return _definitions
            .Where(d => d.Category == category)
            .OrderBy(d => d.SortOrder)
            .ToList();
    }
}
