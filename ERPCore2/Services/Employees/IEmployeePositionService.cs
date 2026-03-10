using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IEmployeePositionService : IGenericManagementService<EmployeePosition>
    {
        Task<(List<EmployeePosition> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EmployeePosition>, IQueryable<EmployeePosition>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsEmployeePositionCodeExistsAsync(string code, int? excludeId = null);
    }
}
