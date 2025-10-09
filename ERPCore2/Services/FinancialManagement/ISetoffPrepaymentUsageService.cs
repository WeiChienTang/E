using ERPCore2.Data.Entities;
using ERPCore2.Helpers;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項使用記錄服務介面
    /// </summary>
    public interface ISetoffPrepaymentUsageService : IGenericManagementService<SetoffPrepaymentUsage>
    {
        /// <summary>
        /// 根據預收付款項ID取得所有使用記錄
        /// </summary>
        /// <param name="prepaymentId">預收付款項ID</param>
        /// <returns>使用記錄清單</returns>
        Task<List<SetoffPrepaymentUsage>> GetByPrepaymentIdAsync(int prepaymentId);
        
        /// <summary>
        /// 根據沖款單ID取得所有使用記錄
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>使用記錄清單</returns>
        Task<List<SetoffPrepaymentUsage>> GetBySetoffDocumentIdAsync(int setoffDocumentId);
        
        /// <summary>
        /// 計算某預收付款項的總使用金額
        /// </summary>
        /// <param name="prepaymentId">預收付款項ID</param>
        /// <returns>總使用金額</returns>
        Task<decimal> GetTotalUsedAmountAsync(int prepaymentId);
        
        /// <summary>
        /// 驗證使用金額是否超過可用餘額
        /// </summary>
        /// <param name="prepaymentId">預收付款項ID</param>
        /// <param name="usedAmount">欲使用金額</param>
        /// <param name="excludeUsageId">排除的使用記錄ID（編輯時使用）</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult> ValidateUsageAmountAsync(int prepaymentId, decimal usedAmount, int? excludeUsageId = null);
        
        /// <summary>
        /// 刪除沖款單的所有使用記錄（刪除沖款單時調用）
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> DeleteBySetoffDocumentIdAsync(int setoffDocumentId);
    }
}
