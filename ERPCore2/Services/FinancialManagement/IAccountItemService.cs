using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    public interface IAccountItemService : IGenericManagementService<AccountItem>
    {
        Task<bool> IsAccountItemCodeExistsAsync(string code, int? excludeId = null);
        Task<List<AccountItem>> GetByAccountTypeAsync(AccountType accountType);
        Task<List<AccountItem>> GetByLevelAsync(int level);
        Task<List<AccountItem>> GetDetailAccountsAsync();
        Task<List<AccountItem>> GetAllWithParentAsync();
    }
}
