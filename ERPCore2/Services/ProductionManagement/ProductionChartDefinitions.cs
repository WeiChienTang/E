using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.ProductionManagement;

public static class ProductionChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionByStatus,
            Title              = "依生產狀態分布",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IProductionChartService>().GetOrdersByStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductionChartService>().GetOrderDetailsByStatusAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "製令單號", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "150px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionTopItemsByQuantity,
            Title              = "品項排程數量 Top 10",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IProductionChartService>().GetTopItemsByScheduledQuantityAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductionChartService>().GetOrderDetailsByItemAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "製令單號", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "完成/排程", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "160px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionMonthlyTrend,
            Title              = "每月製令開單趨勢",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line }
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionByEmployee,
            Title              = "依負責人員分布",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Donut, SeriesType.Pie, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IProductionChartService>().GetOrdersByEmployeeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductionChartService>().GetOrderDetailsByEmployeeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "製令單號", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "150px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionCompletionRateByItem,
            Title              = "品項完成率排行 Top 10 (%)",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar }
            // 完成率圖不支援 drill-down（百分比值，無明細意義）
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductionComponentUsage,
            Title              = "組件用料排行 Top 10",
            Category           = ChartCategory.ProductionManagement,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IProductionChartService>().GetTopComponentsByUsageAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductionChartService>().GetComponentDetailsByItemAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "成品名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "消耗量",   PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });
    }
}
