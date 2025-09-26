using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface ICurrencyService : IGenericManagementService<Currency>
    {
        Task<Currency?> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
    }
}
