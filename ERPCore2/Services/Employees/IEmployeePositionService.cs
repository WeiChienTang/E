using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IEmployeePositionService : IGenericManagementService<EmployeePosition>
    {
        Task<bool> IsEmployeePositionCodeExistsAsync(string code, int? excludeId = null);
    }
}
