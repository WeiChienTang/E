using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Employees;

public static class EmployeeChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeByDepartment,
            Title              = "依部門分布",
            Category           = ChartCategory.Employee,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeesByDepartmentAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeDetailsByDepartmentAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeByType,
            Title              = "依員工類別分布",
            Category           = ChartCategory.Employee,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.PolarArea },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeesByTypeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeDetailsByTypeAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeByStatus,
            Title              = "依在職狀態分布",
            Category           = ChartCategory.Employee,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Pie,
            AllowedSeriesTypes = new() { SeriesType.Pie, SeriesType.Donut, SeriesType.Bar, SeriesType.RadialBar },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeesByStatusAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeDetailsByStatusAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeByGender,
            Title              = "依性別分布",
            Category           = ChartCategory.Employee,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeesByGenderAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeDetailsByGenderAsync(label)
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeMonthlyHireTrend,
            Title              = "每月入職趨勢",
            Category           = ChartCategory.Employee,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Line,
            AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeMonthlyHireTrendAsync()
            // 月趨勢圖不支援 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeBySeniority,
            Title              = "年資分布",
            Category           = ChartCategory.Employee,
            SortOrder          = 6,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetEmployeesBySeniorityAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetEmployeeDetailsBySeniorityAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "姓名",     PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "到職日期", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.EmployeeTopTrainingHours,
            Title              = "培訓時數排行 Top 10",
            Category           = ChartCategory.Employee,
            SortOrder          = 7,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            DataFetcher        = sp => sp.GetRequiredService<IEmployeeChartService>().GetTopEmployeesByTrainingHoursAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IEmployeeChartService>().GetTopEmployeeTrainingDetailsAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "課程名稱", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "時數",     PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "80px", TextAlign = "right" }
            }
        });
    }
}
