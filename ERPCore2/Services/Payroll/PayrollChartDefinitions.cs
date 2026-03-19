using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Payroll;

public static class PayrollChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PayrollMonthlyTrend,
            Title              = "每月薪資總支出趨勢",
            Category           = ChartCategory.Payroll,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPayrollChartService>().GetMonthlyPayrollTrendAsync()
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PayrollGrossByDepartment,
            Title              = "依部門應發薪資分布",
            Category           = ChartCategory.Payroll,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Donut, SeriesType.Pie, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPayrollChartService>().GetGrossIncomeByDepartmentAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPayrollChartService>().GetEmployeeDetailsByDepartmentAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "員工姓名", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "實發薪資", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PayrollTopEarners,
            Title              = "員工薪資排行 Top 10",
            Category           = ChartCategory.Payroll,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IPayrollChartService>().GetTopEarnersAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPayrollChartService>().GetPayrollDetailsByEmployeeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "薪資年月", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "實發薪資", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PayrollRecordStatus,
            Title              = "薪資單狀態分布",
            Category           = ChartCategory.Payroll,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IPayrollChartService>().GetRecordStatusDistributionAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IPayrollChartService>().GetRecordDetailsByStatusAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "員工姓名", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "薪資年月", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "110px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.PayrollOvertimeByEmployee,
            Title              = "員工加班時數排行 Top 10",
            Category           = ChartCategory.Payroll,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap }
            // 加班時數直接顯示排行，無需 drill-down
        });
    }
}
