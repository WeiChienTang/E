using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Suppliers;

public static class SupplierChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierByStatus,
            Title              = "廠商狀態分布",
            Category           = ChartCategory.Supplier,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.RadialBar, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetSupplierDetailsByStatusAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierByType,
            Title              = "依廠商類型分布",
            Category           = ChartCategory.Supplier,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByTypeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetSupplierDetailsByTypeAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierByPaymentMethod,
            Title              = "依付款方式分布",
            Category           = ChartCategory.Supplier,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByPaymentMethodAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetSupplierDetailsByPaymentMethodAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierMonthlyTrend,
            Title              = "每月新增趨勢",
            Category           = ChartCategory.Supplier,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByMonthAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierTopPurchaseByAmount,
            Title              = "廠商進貨金額排行 Top 10",
            Category           = ChartCategory.Supplier,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetTopSuppliersByPurchaseAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetTopSupplierPurchaseReceivingDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "進貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierMonthlyPurchaseTrend,
            Title              = "每月進貨金額趨勢",
            Category           = ChartCategory.Supplier,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetMonthlyPurchaseTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierByCurrentPayable,
            Title              = "應付餘額分布",
            Category           = ChartCategory.Supplier,
            SortOrder          = 7,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByCurrentPayableRangeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByCurrentPayableRangeDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "公司名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "應付餘額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierTopReturnsByAmount,
            Title              = "廠商退回金額排行 Top 10",
            Category           = ChartCategory.Supplier,
            SortOrder          = 8,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetTopSuppliersByReturnAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetTopSupplierReturnDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "退回日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });
    }
}
