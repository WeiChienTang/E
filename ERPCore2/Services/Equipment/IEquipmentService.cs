using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備服務介面
    /// </summary>
    public interface IEquipmentService : IGenericManagementService<Equipment>
    {
        Task<(List<Equipment> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Equipment>, IQueryable<Equipment>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsEquipmentCodeExistsAsync(string code, int? excludeId = null);
        Task<List<Equipment>> GetActiveEquipmentsAsync();
        Task<List<Equipment>> GetByEquipmentCategoryAsync(int categoryId);
        Task<List<Equipment>> GetMaintenanceDueAsync(int withinDays = 30);
    }
}
