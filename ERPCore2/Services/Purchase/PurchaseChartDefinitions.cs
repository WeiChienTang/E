using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Purchase;

public static class PurchaseChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PurchaseTopItemByAmount,
            Title              = "依品項採購金額排行 Top 10",
            Category           = ChartCategory.Purchase,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPurchaseChartService>().GetTopItemsByReceivingAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPurchaseChartService>().GetReceivingDetailsByItemAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "進貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PurchaseMonthlyTrend,
            Title              = "每月進貨金額趨勢",
            Category           = ChartCategory.Purchase,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPurchaseChartService>().GetMonthlyReceivingTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PurchaseOrderApprovalStatus,
            Title              = "採購訂單核准狀態分布",
            Category           = ChartCategory.Purchase,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IPurchaseChartService>().GetOrderApprovalStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPurchaseChartService>().GetOrderDetailsByApprovalStatusAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "採購單號", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "訂單日期", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PurchaseReturnByReason,
            Title              = "依退回原因分布",
            Category           = ChartCategory.Purchase,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            DataFetcher        = sp => sp.GetRequiredService<IPurchaseChartService>().GetReturnsByReasonAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPurchaseChartService>().GetReturnDetailsByReasonAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "退回日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PurchaseMonthlyReturnTrend,
            Title              = "每月採購退回金額趨勢",
            Category           = ChartCategory.Purchase,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPurchaseChartService>().GetMonthlyReturnTrendAsync()
            // 月趨勢圖不支援 drill-down
        });
    }
}
