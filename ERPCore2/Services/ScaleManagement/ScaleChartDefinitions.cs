using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.ScaleManagement;

public static class ScaleChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ScaleWeightByItem,
            Title              = "依品項過磅重量排行 Top 10",
            Category           = ChartCategory.ScaleManagement,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IScaleChartService>().GetNetWeightByItemAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IScaleChartService>().GetRecordDetailsByItemAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "過磅日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "淨重(kg)", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ScaleMonthlyWeightTrend,
            Title              = "每月過磅淨重趨勢",
            Category           = ChartCategory.ScaleManagement,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ScaleNetAmountByCustomer,
            Title              = "依客戶淨收益排行 Top 10",
            Category           = ChartCategory.ScaleManagement,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IScaleChartService>().GetNetAmountByCustomerAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IScaleChartService>().GetRecordDetailsByCustomerAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "過磅日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "淨收益",   PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ScaleMonthlyRevenueTrend,
            Title              = "每月淨收益趨勢",
            Category           = ChartCategory.ScaleManagement,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IScaleChartService>().GetMonthlyRevenueTrendAsync()
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ScaleRecordCountByMonth,
            Title              = "每月過磅筆數趨勢",
            Category           = ChartCategory.ScaleManagement,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            DataFetcher        = sp => sp.GetRequiredService<IScaleChartService>().GetMonthlyRecordCountAsync()
        });
    }
}
