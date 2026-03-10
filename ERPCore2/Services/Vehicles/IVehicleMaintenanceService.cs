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

        #region 伺服器端分頁

        /// <summary>
        /// 取得分頁資料（支援動態篩選）
        /// </summary>
        Task<(List<VehicleMaintenance> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<VehicleMaintenance>, IQueryable<VehicleMaintenance>>? filterFunc,
            int pageNumber,
            int pageSize);

        #endregion
    }
}
