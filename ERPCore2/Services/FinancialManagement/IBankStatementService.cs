using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IBankStatementService : IGenericManagementService<BankStatement>
    {
        Task<(List<BankStatement> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<BankStatement>, IQueryable<BankStatement>>? filterFunc,
            int pageNumber,
            int pageSize);

        /// <summary>取得含明細行的對帳單（用於 EditModal）</summary>
        Task<BankStatement?> GetWithLinesAsync(int id);

        /// <summary>儲存對帳單（含明細行 Upsert / 刪除）</summary>
        Task<ServiceResult> SaveWithLinesAsync(BankStatement statement, List<BankStatementLine> lines, string currentUser);

        /// <summary>切換明細行的配對狀態</summary>
        Task<ServiceResult> ToggleLineMatchAsync(int lineId, int? journalEntryLineId, string currentUser);

        /// <summary>取得指定銀行帳號的對帳單清單</summary>
        Task<List<BankStatement>> GetByBankAccountAsync(int companyBankAccountId);
    }
}
