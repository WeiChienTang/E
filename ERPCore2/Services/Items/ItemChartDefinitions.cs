using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Products;

public static class ProductChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductByCategory,
            Title              = "依品類分布",
            Category           = ChartCategory.Product,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetProductsByCategoryAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductChartService>().GetProductDetailsByCategoryAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductTopSalesByQuantity,
            Title              = "銷售數量排行 Top 10",
            Category           = ChartCategory.Product,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetTopProductsBySalesQuantityAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductChartService>().GetTopProductSalesQuantityDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "出貨數量", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductTopSalesByAmount,
            Title              = "銷售金額排行 Top 10",
            Category           = ChartCategory.Product,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetTopProductsBySalesAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductChartService>().GetTopProductSalesAmountDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductMonthlySalesTrend,
            Title              = "每月銷售收入趨勢",
            Category           = ChartCategory.Product,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetMonthlySalesTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductBySupplierCount,
            Title              = "依供應商報價品項數",
            Category           = ChartCategory.Product,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetProductsBySupplierCountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductChartService>().GetProductDetailsBySupplierAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "最低報價", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.ProductByStandardCostRange,
            Title              = "依標準成本分段分布",
            Category           = ChartCategory.Product,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            DataFetcher        = sp => sp.GetRequiredService<IProductChartService>().GetProductsByStandardCostRangeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IProductChartService>().GetProductDetailsByStandardCostRangeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "標準成本", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });
    }
}
