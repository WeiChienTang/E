using ERPCore2.Data.Entities.Payroll;

namespace ERPCore2.Services.Payroll
{
    public interface IPayrollItemService : IGenericManagementService<PayrollItem>
    {
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
    }
}
