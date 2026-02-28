using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Customers;

public static class CustomerChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByAccountManager,
            Title              = "依業務負責人分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByAccountManagerAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByAccountManagerAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByPaymentMethod,
            Title              = "依付款方式分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByPaymentMethodAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByPaymentMethodAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerMonthlyTrend,
            Title              = "每月新增趨勢",
            Category           = ChartCategory.Customer,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByMonthAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByStatus,
            Title              = "客戶狀態分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.RadialBar, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByCustomerStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByStatusAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByCreditLimit,
            Title              = "信用額度分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByCreditLimitRangeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByCreditLimitRangeAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerTopSalesByAmount,
            Title              = "客戶銷售金額排行 Top 10",
            Category           = ChartCategory.Customer,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetTopCustomersBySalesAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetTopCustomerSalesOrderDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerMonthlySalesTrend,
            Title              = "每月銷售收入趨勢",
            Category           = ChartCategory.Customer,
            SortOrder          = 7,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetMonthlySalesTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerByCurrentBalance,
            Title              = "應收餘額分布",
            Category           = ChartCategory.Customer,
            SortOrder          = 8,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByCurrentBalanceRangeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomersByCurrentBalanceRangeDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "公司名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "應收餘額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.CustomerTopReturnsByAmount,
            Title              = "客戶退貨金額排行 Top 10",
            Category           = ChartCategory.Customer,
            SortOrder          = 9,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetTopCustomersByReturnAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetTopCustomerReturnDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "退回日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });
    }
}
