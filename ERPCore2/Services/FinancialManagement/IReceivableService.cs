using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收沖款服務介面 - 統一處理銷貨訂單和採購退回的應收款項
    /// </summary>
    public interface IReceivableService
    {
        /// <summary>
        /// 取得所有應收款項
        /// </summary>
        /// <returns>應收款項清單</returns>
        Task<ServiceResult<List<ReceivableViewModel>>> GetAllReceivablesAsync();

        /// <summary>
        /// 取得未結清的應收款項
        /// </summary>
        /// <returns>未結清應收款項清單</returns>
        Task<ServiceResult<List<ReceivableViewModel>>> GetUnSettledReceivablesAsync();

        /// <summary>
        /// 取得逾期的應收款項
        /// </summary>
        /// <returns>逾期應收款項清單</returns>
        Task<ServiceResult<List<ReceivableViewModel>>> GetOverdueReceivablesAsync();

        /// <summary>
        /// 根據條件搜尋應收款項
        /// </summary>
        /// <param name="documentType">單據類型 (可選)</param>
        /// <param name="isSettled">是否結清 (可選)</param>
        /// <param name="customerOrSupplier">往來對象名稱 (可選)</param>
        /// <param name="startDate">起始日期 (可選)</param>
        /// <param name="endDate">結束日期 (可選)</param>
        /// <returns>符合條件的應收款項清單</returns>
        Task<ServiceResult<List<ReceivableViewModel>>> SearchReceivablesAsync(
            string? documentType = null,
            bool? isSettled = null,
            string? customerOrSupplier = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 更新收款金額
        /// </summary>
        /// <param name="id">明細 ID</param>
        /// <param name="documentType">單據類型</param>
        /// <param name="receivedAmount">本次收款金額</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult<bool>> UpdateReceivedAmountAsync(int id, string documentType, decimal receivedAmount);

        /// <summary>
        /// 結清應收款項
        /// </summary>
        /// <param name="id">明細 ID</param>
        /// <param name="documentType">單據類型</param>
        /// <returns>結清結果</returns>
        Task<ServiceResult<bool>> SettleReceivableAsync(int id, string documentType);

        /// <summary>
        /// 批次更新收款金額
        /// </summary>
        /// <param name="receivableUpdates">收款更新資料</param>
        /// <returns>批次更新結果</returns>
        Task<ServiceResult<int>> BatchUpdateReceivedAmountAsync(List<ReceivableUpdateModel> receivableUpdates);

        /// <summary>
        /// 批次結清應收款項
        /// </summary>
        /// <param name="receivableIds">要結清的應收款項 ID 和單據類型清單</param>
        /// <returns>批次結清結果</returns>
        Task<ServiceResult<int>> BatchSettleReceivablesAsync(List<ReceivableIdentifier> receivableIds);

        /// <summary>
        /// 取得應收款項統計資訊
        /// </summary>
        /// <returns>統計資訊</returns>
        Task<ServiceResult<ReceivableStatistics>> GetReceivableStatisticsAsync();

        /// <summary>
        /// 取得單一應收款項詳細資訊
        /// </summary>
        /// <param name="id">明細 ID</param>
        /// <param name="documentType">單據類型</param>
        /// <returns>應收款項詳細資訊</returns>
        Task<ServiceResult<ReceivableViewModel?>> GetReceivableByIdAsync(int id, string documentType);

        /// <summary>
        /// 驗證收款金額是否有效
        /// </summary>
        /// <param name="id">明細 ID</param>
        /// <param name="documentType">單據類型</param>
        /// <param name="receivedAmount">擬收款金額</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult<bool>> ValidateReceivedAmountAsync(int id, string documentType, decimal receivedAmount);
    }

    /// <summary>
    /// 應收款項更新模型
    /// </summary>
    public class ReceivableUpdateModel
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public decimal ReceivedAmount { get; set; }
    }

    /// <summary>
    /// 應收款項識別模型
    /// </summary>
    public class ReceivableIdentifier
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 應收款項統計資訊
    /// </summary>
    public class ReceivableStatistics
    {
        /// <summary>
        /// 總應收款項數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 未結清款項數量
        /// </summary>
        public int UnSettledCount { get; set; }

        /// <summary>
        /// 逾期款項數量
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// 總應收金額
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 已收款金額
        /// </summary>
        public decimal TotalReceivedAmount { get; set; }

        /// <summary>
        /// 餘額
        /// </summary>
        public decimal BalanceAmount { get; set; }

        /// <summary>
        /// 收款完成率
        /// </summary>
        public decimal CompletionRate => TotalAmount > 0 ? (TotalReceivedAmount / TotalAmount) * 100 : 0;
    }
}