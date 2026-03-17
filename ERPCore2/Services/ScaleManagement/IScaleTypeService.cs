using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 磅秤類型服務介面
    /// </summary>
    public interface IScaleTypeService : IGenericManagementService<ScaleType>
    {
        Task<(List<ScaleType> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ScaleType>, IQueryable<ScaleType>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<ScaleType?> GetByCodeAsync(string code);
        Task<bool> IsScaleTypeCodeExistsAsync(string code, int? excludeId = null);
        Task<List<ScaleType>> GetActiveScaleTypesAsync();
    }
}
