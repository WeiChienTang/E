using ApexCharts;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Vehicles;

public static class VehicleChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.VehicleMaintenanceByType,
            Title              = "依保養類型費用分布",
            Category           = ChartCategory.VehicleManagement,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Donut, SeriesType.Pie, SeriesType.PolarArea },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IVehicleChartService>().GetMaintenanceByTypeAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IVehicleChartService>().GetMaintenanceDetailsByTypeAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "車輛",   PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "費用",   PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.VehicleMonthlyCostTrend,
            Title              = "每月保養費用趨勢",
            Category           = ChartCategory.VehicleManagement,
            SortOrder          = 2,
            DefaultSeriesType  = SeriesType.Area,
            AllowedSeriesTypes = new() { SeriesType.Area, SeriesType.Line },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IVehicleChartService>().GetMonthlyCostTrendAsync()
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.VehicleCostByVehicle,
            Title              = "依車輛保養費用排行 Top 10",
            Category           = ChartCategory.VehicleManagement,
            SortOrder          = 3,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
            IsMoneyChart       = true,
            DataFetcher        = sp => sp.GetRequiredService<IVehicleChartService>().GetCostByVehicleAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<IVehicleChartService>().GetMaintenanceDetailsByVehicleAsync(label),
            DetailColumns      = new()
            {
                new() { Title = "保養日期/類型", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
                new() { Title = "費用",          PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "120px", TextAlign = "right" }
            }
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.VehicleByOwnershipType,
            Title              = "依歸屬類型分布",
            Category           = ChartCategory.VehicleManagement,
            SortOrder          = 4,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar }
            // 歸屬類型僅兩類，不需 drill-down
        });

        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.VehicleInsuranceExpiry,
            Title              = "保險到期狀態分布",
            Category           = ChartCategory.VehicleManagement,
            SortOrder          = 5,
            DefaultSeriesType  = SeriesType.Bar,
            AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Donut, SeriesType.Pie }
            // 到期分布為靜態分段，無 drill-down
        });
    }
}
