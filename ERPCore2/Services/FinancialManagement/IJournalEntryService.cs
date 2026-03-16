using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 傳票服務介面
    /// </summary>
    public interface IJournalEntryService : IGenericManagementService<JournalEntry>
    {
        /// <summary>
        /// 檢查傳票號碼是否已存在（供 EntityCodeGenerationHelper 使用）
        /// </summary>
        Task<bool> IsJournalEntryCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 依會計年度與期間查詢傳票（含分錄明細）
        /// </summary>
        Task<List<JournalEntry>> GetByFiscalPeriodAsync(int fiscalYear, int fiscalPeriod);

        /// <summary>
        /// 依來源單據查詢已產生的傳票
        /// </summary>
        Task<JournalEntry?> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId);

        /// <summary>
        /// 取得尚未過帳的草稿傳票清單
        /// </summary>
        Task<List<JournalEntry>> GetDraftEntriesAsync();

        /// <summary>
        /// 取得含分錄明細的傳票（供 EditModal 使用）
        /// </summary>
        Task<JournalEntry?> GetWithLinesAsync(int id);

        /// <summary>
        /// 作廢草稿：將草稿傳票標記為已取消（不可逆）
        /// 注意：自動產生的傳票作廢後，呼叫端需自行重置來源業務單據的 IsJournalized
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CancelDraftEntryAsync(int id, string updatedBy);

        /// <summary>
        /// 過帳：將傳票狀態從草稿改為已過帳
        /// 前提：傳票必須借貸平衡且至少有一借一貸
        /// </summary>
        Task<(bool Success, string ErrorMessage)> PostEntryAsync(int id, string updatedBy);

        /// <summary>
        /// 沖銷：建立反向傳票並將原傳票標記為已沖銷
        /// </summary>
        Task<(bool Success, string ErrorMessage, JournalEntry? ReversalEntry)> ReverseEntryAsync(int id, DateTime reversalDate, string updatedBy);

        /// <summary>
        /// 儲存傳票及其分錄（含新增/更新/刪除分錄行）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> SaveWithLinesAsync(JournalEntry journalEntry, string savedBy);

        /// <summary>
        /// 伺服器端分頁查詢（不載入 Lines，僅取列表所需欄位）。
        /// </summary>
        Task<(List<JournalEntry> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<JournalEntry>, IQueryable<JournalEntry>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
