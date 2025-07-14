using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IEmployeePositionService : IGenericManagementService<EmployeePosition>
    {
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
        Task<List<EmployeePosition>> GetByLevelAsync(int level);
        Task<List<EmployeePosition>> GetOrderedByLevelAsync();
    }
}
