using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車輛保養紀錄服務介面
    /// </summary>
    public interface IVehicleMaintenanceService : IGenericManagementService<VehicleMaintenance>
    {
        Task<VehicleMaintenance?> GetByMaintenanceCodeAsync(string maintenanceCode);
        Task<bool> IsVehicleMaintenanceCodeExistsAsync(string maintenanceCode, int? excludeId = null);
        Task<List<VehicleMaintenance>> GetByVehicleAsync(int vehicleId);
        Task<List<VehicleMaintenance>> GetByMaintenanceTypeAsync(MaintenanceType maintenanceType);
        Task<List<VehicleMaintenance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<VehicleMaintenance>> GetActiveMaintenancesAsync();
    }
}
