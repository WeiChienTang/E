using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services.EmployeePosition
{
    public interface IEmployeePositionService : IGenericManagementService<Data.Entities.EmployeePosition>
    {
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
        Task<List<Data.Entities.EmployeePosition>> GetByLevelAsync(int level);
        Task<List<Data.Entities.EmployeePosition>> GetOrderedByLevelAsync();
    }
}
