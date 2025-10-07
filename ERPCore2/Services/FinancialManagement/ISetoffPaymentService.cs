using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款收款記錄服務介面
    /// </summary>
    public interface ISetoffPaymentService : IGenericManagementService<SetoffPayment>
    {
        /// <summary>
        /// 根據沖款單ID取得所有收款記錄
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>收款記錄列表</returns>
        Task<List<SetoffPayment>> GetBySetoffDocumentIdAsync(int setoffDocumentId);

        /// <summary>
        /// 計算指定沖款單的總收款金額
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>總收款金額</returns>
        Task<decimal> GetTotalReceivedAmountAsync(int setoffDocumentId);

        /// <summary>
        /// 計算指定沖款單的總折讓金額
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>總折讓金額</returns>
        Task<decimal> GetTotalAllowanceAmountAsync(int setoffDocumentId);

        /// <summary>
        /// 檢查支票號碼是否已存在
        /// </summary>
        /// <param name="checkNumber">支票號碼</param>
        /// <param name="excludeId">排除的記錄ID（用於編輯時）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCheckNumberExistsAsync(string checkNumber, int? excludeId = null);
    }
}
