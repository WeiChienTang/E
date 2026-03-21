using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備類別服務介面
    /// </summary>
    public interface IEquipmentCategoryService : IGenericManagementService<EquipmentCategory>
    {
        Task<(List<EquipmentCategory> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EquipmentCategory>, IQueryable<EquipmentCategory>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsEquipmentCategoryCodeExistsAsync(string code, int? excludeId = null);
        Task<bool> IsEquipmentCategoryNameExistsAsync(string name, int? excludeId = null);
        Task<List<EquipmentCategory>> GetActiveEquipmentCategoriesAsync();
    }
}
