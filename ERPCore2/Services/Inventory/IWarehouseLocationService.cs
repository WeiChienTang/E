using ERPCore2.Data.Entities;
using ERPCore2.Services.Interfaces;

namespace ERPCore2.Services.Inventory
{
    public interface IWarehouseLocationService : IGenericManagementService<WarehouseLocation>
    {
        Task<ServiceResult<IEnumerable<WarehouseLocation>>> GetByWarehouseIdAsync(int warehouseId);
    }
}
