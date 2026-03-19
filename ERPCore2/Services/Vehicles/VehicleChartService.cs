using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Vehicles;

public class VehicleChartService : IVehicleChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public VehicleChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依保養類型費用分布</summary>
    public async Task<List<ChartDataItem>> GetMaintenanceByTypeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await context.VehicleMaintenances
            .Where(x => x.Status != EntityStatus.Deleted && x.Cost.HasValue)
            .Select(x => new { x.MaintenanceType, x.Cost })
            .ToListAsync();

        var typeLabels = new Dictionary<MaintenanceType, string>
        {
            [MaintenanceType.RegularService] = "定期保養",
            [MaintenanceType.Repair]         = "維修",
            [MaintenanceType.TireChange]     = "輪胎更換",
            [MaintenanceType.OilChange]      = "換機油",
            [MaintenanceType.Insurance]      = "保險",
            [MaintenanceType.Inspection]     = "驗車",
            [MaintenanceType.Other]          = "其他"
        };

        return rows
            .GroupBy(x => x.MaintenanceType)
            .Select(g => new ChartDataItem
            {
                Label = typeLabels.GetValueOrDefault(g.Key, g.Key.ToString()),
                Value = g.Sum(x => x.Cost ?? 0)
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>每月保養費用趨勢（近 N 個月）</summary>
    public async Task<List<ChartDataItem>> GetMonthlyCostTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await context.VehicleMaintenances
            .Where(x => x.Status != EntityStatus.Deleted && x.Cost.HasValue && x.MaintenanceDate >= startDate)
            .Select(x => new { x.MaintenanceDate, x.Cost })
            .ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.MaintenanceDate.Year, x.MaintenanceDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.Cost ?? 0));

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var total);
            result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = total });
        }
        return result;
    }

    /// <summary>依車輛保養費用排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetCostByVehicleAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await (
            from vm in context.VehicleMaintenances
            where vm.Status != EntityStatus.Deleted && vm.Cost.HasValue && vm.VehicleId.HasValue
            join v in context.Vehicles on vm.VehicleId equals v.Id
            select new { VehicleLabel = v.LicensePlate + " " + v.VehicleName, vm.Cost }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.VehicleLabel.Trim())
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.Cost ?? 0)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>依車輛歸屬類型分布（公司/客戶）</summary>
    public async Task<List<ChartDataItem>> GetVehiclesByOwnershipTypeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var rows = await context.Vehicles
            .Where(x => x.Status != EntityStatus.Deleted)
            .Select(x => new { x.OwnershipType })
            .ToListAsync();

        var typeLabels = new Dictionary<VehicleOwnershipType, string>
        {
            [VehicleOwnershipType.Company]  = "公司",
            [VehicleOwnershipType.Customer] = "客戶"
        };

        return rows
            .GroupBy(x => x.OwnershipType)
            .Select(g => new ChartDataItem
            {
                Label = typeLabels.GetValueOrDefault(g.Key, g.Key.ToString()),
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>保險到期分布（依距到期日分段）</summary>
    public async Task<List<ChartDataItem>> GetInsuranceExpiryDistributionAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var vehicles = await context.Vehicles
            .Where(x => x.Status != EntityStatus.Deleted)
            .Select(x => new { x.InsuranceExpiryDate })
            .ToListAsync();

        int expired = 0, within30 = 0, within90 = 0, over90 = 0, notSet = 0;
        foreach (var v in vehicles)
        {
            if (!v.InsuranceExpiryDate.HasValue)
                notSet++;
            else
            {
                var days = (v.InsuranceExpiryDate.Value - now).TotalDays;
                if (days < 0)        expired++;
                else if (days <= 30) within30++;
                else if (days <= 90) within90++;
                else                 over90++;
            }
        }

        return new List<ChartDataItem>
        {
            new() { Label = "已過期",     Value = expired },
            new() { Label = "30天內到期", Value = within30 },
            new() { Label = "31~90天",    Value = within90 },
            new() { Label = "90天以上",   Value = over90 },
            new() { Label = "未設定",     Value = notSet }
        };
    }

    /// <summary>取得車輛管理統計摘要</summary>
    public async Task<VehicleChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var in30Days = now.AddDays(30);

        var totalVehicles = await context.Vehicles.CountAsync(x => x.Status != EntityStatus.Deleted);

        var maintenanceThisMonth = await context.VehicleMaintenances
            .Where(x => x.Status != EntityStatus.Deleted && x.MaintenanceDate >= firstOfMonth)
            .Select(x => new { x.Cost })
            .ToListAsync();

        var insuranceExpiring = await context.Vehicles
            .CountAsync(x => x.Status != EntityStatus.Deleted
                          && x.InsuranceExpiryDate.HasValue
                          && x.InsuranceExpiryDate.Value >= now
                          && x.InsuranceExpiryDate.Value <= in30Days);

        var inspectionExpiring = await context.Vehicles
            .CountAsync(x => x.Status != EntityStatus.Deleted
                          && x.InspectionExpiryDate.HasValue
                          && x.InspectionExpiryDate.Value >= now
                          && x.InspectionExpiryDate.Value <= in30Days);

        return new VehicleChartSummary
        {
            TotalVehicles                    = totalVehicles,
            MaintenancesThisMonth            = maintenanceThisMonth.Count,
            TotalMaintenanceCostThisMonth    = maintenanceThisMonth.Sum(x => x.Cost ?? 0),
            VehiclesInsuranceExpiringSoon    = insuranceExpiring,
            VehiclesInspectionExpiringSoon   = inspectionExpiring
        };
    }

    // ===== Drill-down 明細查詢 =====

    /// <summary>依保養類型 Drill-down：顯示該類型的保養記錄</summary>
    public async Task<List<ChartDetailItem>> GetMaintenanceDetailsByTypeAsync(string typeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var typeMap = new Dictionary<string, MaintenanceType>
        {
            ["定期保養"] = MaintenanceType.RegularService,
            ["維修"]     = MaintenanceType.Repair,
            ["輪胎更換"] = MaintenanceType.TireChange,
            ["換機油"]   = MaintenanceType.OilChange,
            ["保險"]     = MaintenanceType.Insurance,
            ["驗車"]     = MaintenanceType.Inspection,
            ["其他"]     = MaintenanceType.Other
        };

        if (!typeMap.TryGetValue(typeLabel, out var mType))
            return new List<ChartDetailItem>();

        var raw = await (
            from vm in context.VehicleMaintenances
            where vm.Status != EntityStatus.Deleted && vm.MaintenanceType == mType
            join v in context.Vehicles on vm.VehicleId equals v.Id into vg
            from vehicle in vg.DefaultIfEmpty()
            orderby vm.MaintenanceDate descending
            select new { vm.Id, VehicleLabel = vehicle != null ? vehicle.LicensePlate + " " + vehicle.VehicleName : "未指定", vm.MaintenanceDate, vm.Cost }
        ).Take(30).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.VehicleLabel.Trim(),
            SubLabel = x.Cost.HasValue ? $"NT${x.Cost.Value:N0}" : x.MaintenanceDate.ToString("yyyy/MM/dd")
        }).ToList();
    }

    /// <summary>依車輛保養費用 Drill-down：顯示該車輛的保養記錄</summary>
    public async Task<List<ChartDetailItem>> GetMaintenanceDetailsByVehicleAsync(string vehicleLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from vm in context.VehicleMaintenances
            where vm.Status != EntityStatus.Deleted && vm.VehicleId.HasValue
            join v in context.Vehicles on vm.VehicleId equals v.Id
            where (v.LicensePlate + " " + v.VehicleName).Trim() == vehicleLabel
            orderby vm.MaintenanceDate descending
            select new { vm.Id, vm.MaintenanceDate, vm.MaintenanceType, vm.Cost }
        ).Take(30).ToListAsync();

        var typeLabels = new Dictionary<MaintenanceType, string>
        {
            [MaintenanceType.RegularService] = "定期保養",
            [MaintenanceType.Repair]         = "維修",
            [MaintenanceType.TireChange]     = "輪胎更換",
            [MaintenanceType.OilChange]      = "換機油",
            [MaintenanceType.Insurance]      = "保險",
            [MaintenanceType.Inspection]     = "驗車",
            [MaintenanceType.Other]          = "其他"
        };

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = $"{x.MaintenanceDate:yyyy/MM/dd} {typeLabels.GetValueOrDefault(x.MaintenanceType, x.MaintenanceType.ToString())}",
            SubLabel = x.Cost.HasValue ? $"NT${x.Cost.Value:N0}" : "-"
        }).ToList();
    }
}
