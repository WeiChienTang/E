using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備保養維修記錄服務介面
    /// </summary>
    public interface IEquipmentMaintenanceService : IGenericManagementService<EquipmentMaintenance>
    {
        Task<(List<EquipmentMaintenance> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EquipmentMaintenance>, IQueryable<EquipmentMaintenance>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsEquipmentMaintenanceCodeExistsAsync(string code, int? excludeId = null);
        Task<List<EquipmentMaintenance>> GetByEquipmentAsync(int equipmentId);
        Task<List<EquipmentMaintenance>> GetByMaintenanceTypeAsync(EquipmentMaintenanceType maintenanceType);
        Task<List<EquipmentMaintenance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
