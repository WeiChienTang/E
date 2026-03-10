using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車型服務介面
    /// </summary>
    public interface IVehicleTypeService : IGenericManagementService<VehicleType>
    {
        Task<(List<VehicleType> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<VehicleType>, IQueryable<VehicleType>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<VehicleType?> GetByVehicleTypeCodeAsync(string vehicleTypeCode);
        Task<bool> IsVehicleTypeCodeExistsAsync(string vehicleTypeCode, int? excludeId = null);
        Task<bool> IsVehicleTypeNameExistsAsync(string vehicleTypeName, int? excludeId = null);
        Task<List<VehicleType>> GetActiveVehicleTypesAsync();
        Task<List<VehicleType>> GetByVehicleTypeNameAsync(string vehicleTypeName);
    }
}
