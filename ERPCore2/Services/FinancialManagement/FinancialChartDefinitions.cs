using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.FinancialManagement;

public static class FinancialChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialSetoffByType,
            Title              = "依沖款類型分布",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetSetoffByTypeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IFinancialChartService>().GetSetoffDetailsByTypeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "沖款日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "沖款金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialMonthlySetoffTrend,
            Title              = "每月沖款金額趨勢",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetMonthlySetoffTrendAsync()
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialTopSetoffByAmount,
            Title              = "依關聯方沖款金額排行 Top 10",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetTopSetoffByAmountAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IFinancialChartService>().GetSetoffDetailsByPartyAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "沖款日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "沖款金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialJournalByType,
            Title              = "依傳票類型分布",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Donut, SeriesType.Pie, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetJournalByTypeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IFinancialChartService>().GetJournalDetailsByTypeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "傳票日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "借方金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialMonthlyJournalTrend,
            Title              = "每月過帳金額趨勢",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetMonthlyJournalTrendAsync()
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.FinancialJournalByStatus,
            Title              = "依傳票狀態分布",
            Category           = ChartCategory.FinancialManagement,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IFinancialChartService>().GetJournalByStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IFinancialChartService>().GetJournalDetailsByStatusAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "傳票單號", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "借方金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });
    }
}
