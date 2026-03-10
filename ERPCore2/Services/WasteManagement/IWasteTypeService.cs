using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 磅秤類型服務介面
    /// </summary>
    public interface IWasteTypeService : IGenericManagementService<WasteType>
    {
        Task<(List<WasteType> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<WasteType>, IQueryable<WasteType>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<WasteType?> GetByCodeAsync(string code);
        Task<bool> IsWasteTypeCodeExistsAsync(string code, int? excludeId = null);
        Task<List<WasteType>> GetActiveWasteTypesAsync();
    }
}
