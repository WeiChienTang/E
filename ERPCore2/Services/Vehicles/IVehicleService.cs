using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車輛服務介面
    /// </summary>
    public interface IVehicleService : IGenericManagementService<Vehicle>
    {
        Task<Vehicle?> GetByVehicleCodeAsync(string vehicleCode);
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
        Task<bool> IsVehicleCodeExistsAsync(string vehicleCode, int? excludeId = null);
        Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null);
        Task<List<Vehicle>> GetActiveVehiclesAsync();
        Task<List<Vehicle>> GetByVehicleTypeAsync(int vehicleTypeId);
        Task<List<Vehicle>> GetByOwnershipTypeAsync(VehicleOwnershipType ownershipType);
        Task<List<Vehicle>> GetByCustomerAsync(int customerId);
        Task<List<Vehicle>> GetByEmployeeAsync(int employeeId);
    }
}
