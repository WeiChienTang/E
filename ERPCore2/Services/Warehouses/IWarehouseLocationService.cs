using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IWarehouseLocationService : IGenericManagementService<WarehouseLocation>
    {
        Task<(List<WarehouseLocation> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<WarehouseLocation>, IQueryable<WarehouseLocation>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<ServiceResult<IEnumerable<WarehouseLocation>>> GetByWarehouseIdAsync(int warehouseId);
        Task<bool> IsWarehouseLocationCodeExistsAsync(string code, int? excludeId = null);
    }
}

