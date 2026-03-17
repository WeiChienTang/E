using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Inventory;

public static class InventoryChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.InventoryTopByValue,
            Title              = "庫存金額排行 Top 10",
            Category           = ChartCategory.Inventory,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IInventoryChartService>().GetTopProductsByStockValueAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IInventoryChartService>().GetStockDetailsByWarehouseAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.InventoryTopByQuantity,
            Title              = "庫存數量排行 Top 10",
            Category           = ChartCategory.Inventory,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IInventoryChartService>().GetTopProductsByStockQuantityAsync()
            // Top-by-quantity 點擊不做 drill-down（直接看排行即可）
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.InventoryByWarehouse,
            Title              = "依倉庫庫存金額分布",
            Category           = ChartCategory.Inventory,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = false,
            DataFetcher        = sp => sp.GetRequiredService<IInventoryChartService>().GetStockValueByWarehouseAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IInventoryChartService>().GetStockDetailsByWarehouseAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "庫存數量", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.InventoryLowStock,
            Title              = "低庫存預警品項",
            Category           = ChartCategory.Inventory,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IInventoryChartService>().GetLowStockProductsAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IInventoryChartService>().GetLowStockDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "品項名稱",       PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "現有庫存 / 安全量", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "200px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.InventoryTransactionTypes,
            Title              = "庫存異動類型分布（近30天）",
            Category           = ChartCategory.Inventory,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IInventoryChartService>().GetTransactionTypeDistributionAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IInventoryChartService>().GetTransactionDetailsByTypeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "異動日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "單據號碼", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display }
            }
        });
    }
}
