using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Sales;

public static class SalesChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SalesTopProductByAmount,
            Title              = "依商品銷售金額排行 Top 10",
            Category           = ChartCategory.Sales,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISalesChartService>().GetTopProductsBySalesAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISalesChartService>().GetDeliveryDetailsByProductAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SalesTopEmployeeByAmount,
            Title              = "依業務員業績排行 Top 10",
            Category           = ChartCategory.Sales,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISalesChartService>().GetTopEmployeesBySalesAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISalesChartService>().GetDeliveryDetailsByEmployeeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SalesMonthlyTrend,
            Title              = "每月出貨金額趨勢",
            Category           = ChartCategory.Sales,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISalesChartService>().GetMonthlyDeliveryTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SalesReturnByReason,
            Title              = "依退回原因分布",
            Category           = ChartCategory.Sales,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            DataFetcher        = sp => sp.GetRequiredService<ISalesChartService>().GetReturnsByReasonAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISalesChartService>().GetReturnDetailsByReasonAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "退回日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SalesMonthlyReturnTrend,
            Title              = "每月銷貨退回金額趨勢",
            Category           = ChartCategory.Sales,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<ISalesChartService>().GetMonthlyReturnTrendAsync()
            // 月趨勢圖不支援 drill-down
        });
    }
}
