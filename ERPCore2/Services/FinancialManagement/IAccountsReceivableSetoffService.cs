using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款單服務介面
    /// </summary>
    public interface IAccountsReceivableSetoffService : IGenericManagementService<AccountsReceivableSetoff>
    {
        /// <summary>
        /// 檢查沖款單號是否已存在
        /// </summary>
        /// <param name="setoffNumber">沖款單號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null);

        /// <summary>
        /// 依據客戶ID取得沖款單列表
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>沖款單列表</returns>
        Task<List<AccountsReceivableSetoff>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 依據日期範圍取得沖款單列表
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>沖款單列表</returns>
        Task<List<AccountsReceivableSetoff>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得包含明細的沖款單
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>包含明細的沖款單</returns>
        Task<AccountsReceivableSetoff?> GetWithDetailsAsync(int id);

        /// <summary>
        /// 完成沖款單（設定完成狀態）
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CompleteSetoffAsync(int id);

        /// <summary>
        /// 取消完成沖款單
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UncompleteSetoffAsync(int id);

        /// <summary>
        /// 計算沖款單的總金額
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>總金額</returns>
        Task<decimal> CalculateTotalAmountAsync(int id);

        /// <summary>
        /// 取得未完成的沖款單列表
        /// </summary>
        /// <returns>未完成的沖款單列表</returns>
        Task<List<AccountsReceivableSetoff>> GetIncompleteSetoffsAsync();
    }
}