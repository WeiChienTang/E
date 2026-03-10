using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    public interface IAccountItemService : IGenericManagementService<AccountItem>
    {
        Task<bool> IsAccountItemCodeExistsAsync(string code, int? excludeId = null);
        Task<AccountItem?> GetByCodeAsync(string code);
        Task<List<AccountItem>> GetByAccountTypeAsync(AccountType accountType);
        Task<List<AccountItem>> GetByLevelAsync(int level);
        Task<List<AccountItem>> GetDetailAccountsAsync();
        Task<List<AccountItem>> GetAllWithParentAsync();

        /// <summary>
        /// 伺服器端分頁查詢（不載入 Include，僅取列表所需欄位）。
        /// filterFunc 由呼叫端提供，直接作用在 EF Core IQueryable，確保過濾在 DB 層執行。
        /// </summary>
        Task<(List<AccountItem> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<AccountItem>, IQueryable<AccountItem>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
