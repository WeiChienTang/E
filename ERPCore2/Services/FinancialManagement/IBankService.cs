using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IBankService : IGenericManagementService<Bank>
    {
        Task<(List<Bank> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Bank>, IQueryable<Bank>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsBankCodeExistsAsync(string code, int? excludeId = null);
        Task<bool> IsBankNameExistsAsync(string bankName, int? excludeId = null);
        Task<Bank?> GetBySwiftCodeAsync(string swiftCode);
    }
}
