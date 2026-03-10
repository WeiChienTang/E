using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface ICurrencyService : IGenericManagementService<Currency>
    {
        Task<(List<Currency> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Currency>, IQueryable<Currency>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<Currency?> GetByCodeAsync(string code);
        Task<bool> IsCurrencyCodeExistsAsync(string code, int? excludeId = null);
    }
}
