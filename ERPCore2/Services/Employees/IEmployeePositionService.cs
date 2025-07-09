using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;

namespace ERPCore2.Services.Employees
{
    public interface IEmployeePositionService : IGenericManagementService<EmployeePosition>
    {
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
        Task<List<EmployeePosition>> GetByLevelAsync(int level);
        Task<List<EmployeePosition>> GetOrderedByLevelAsync();
    }
}
