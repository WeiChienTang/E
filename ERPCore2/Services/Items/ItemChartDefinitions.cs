using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Items;

public static class ItemChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemByCategory,
            Title              = "依品類分布",
            Category           = ChartCategory.Item,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetItemsByCategoryAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IItemChartService>().GetItemDetailsByCategoryAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemTopSalesByQuantity,
            Title              = "銷售數量排行 Top 10",
            Category           = ChartCategory.Item,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetTopItemsBySalesQuantityAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IItemChartService>().GetTopItemSalesQuantityDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "出貨數量", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemTopSalesByAmount,
            Title              = "銷售金額排行 Top 10",
            Category           = ChartCategory.Item,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetTopItemsBySalesAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IItemChartService>().GetTopItemSalesAmountDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemMonthlySalesTrend,
            Title              = "每月銷售收入趨勢",
            Category           = ChartCategory.Item,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetMonthlySalesTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemBySupplierCount,
            Title              = "依供應商報價品項數",
            Category           = ChartCategory.Item,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetItemsBySupplierCountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IItemChartService>().GetItemDetailsBySupplierAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "最低報價", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ItemByStandardCostRange,
            Title              = "依標準成本分段分布",
            Category           = ChartCategory.Item,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            DataFetcher        = sp => sp.GetRequiredService<IItemChartService>().GetItemsByStandardCostRangeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IItemChartService>().GetItemDetailsByStandardCostRangeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "標準成本", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });
    }
}
