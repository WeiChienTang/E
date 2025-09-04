using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IWarehouseLocationService : IGenericManagementService<WarehouseLocation>
    {
        Task<ServiceResult<IEnumerable<WarehouseLocation>>> GetByWarehouseIdAsync(int warehouseId);
        Task<bool> IsWarehouseLocationCodeExistsAsync(string code, int? excludeId = null);
    }
}

