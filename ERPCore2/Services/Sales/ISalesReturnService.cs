using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨退回服務介面
    /// </summary>
    public interface ISalesReturnService : IGenericManagementService<SalesReturn>
    {
        /// <summary>
        /// 檢查退回單號是否已存在
        /// </summary>
        /// <param name="salesReturnNumber">退回單號</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSalesReturnNumberExistsAsync(string salesReturnNumber, int? excludeId = null);

        /// <summary>
        /// 根據客戶取得銷貨退回清單
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據狀態取得銷貨退回清單
        /// </summary>
        /// <param name="status">退回狀態</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetByStatusAsync(SalesReturnStatus status);

        /// <summary>
        /// 根據銷貨訂單取得銷貨退回清單
        /// </summary>
        /// <param name="salesOrderId">銷貨訂單ID</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// 根據銷貨出貨單取得銷貨退回清單
        /// </summary>
        /// <param name="salesDeliveryId">銷貨出貨單ID</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetBySalesDeliveryIdAsync(int salesDeliveryId);

        /// <summary>
        /// 根據日期範圍取得銷貨退回清單
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 更新退回狀態
        /// </summary>
        /// <param name="id">銷貨退回ID</param>
        /// <param name="status">新狀態</param>
        /// <param name="remarks">狀態更新備註</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateStatusAsync(int id, SalesReturnStatus status, string? remarks = null);

        /// <summary>
        /// 計算退回總金額
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>總金額</returns>
        Task<decimal> CalculateTotalReturnAmountAsync(int salesReturnId);

        /// <summary>
        /// 設定退款資訊
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <param name="refundAmount">退款金額</param>
        /// <param name="refundDate">退款日期</param>
        /// <param name="refundRemarks">退款備註</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetRefundInfoAsync(int salesReturnId, decimal refundAmount, DateTime refundDate, string? refundRemarks = null);

        /// <summary>
        /// 產生退回單號
        /// </summary>
        /// <param name="returnDate">退回日期</param>
        /// <returns>退回單號</returns>
        Task<string> GenerateSalesReturnNumberAsync(DateTime returnDate);

        /// <summary>
        /// 取得統計資訊
        /// </summary>
        /// <param name="customerId">客戶ID（選填）</param>
        /// <param name="startDate">開始日期（選填）</param>
        /// <param name="endDate">結束日期（選填）</param>
        /// <returns>統計資訊</returns>
        Task<SalesReturnStatistics> GetStatisticsAsync(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// 銷貨退回統計資訊
    /// </summary>
    public class SalesReturnStatistics
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public int PendingReturns { get; set; }
        public int CompletedReturns { get; set; }
        public int CancelledReturns { get; set; }
        public Dictionary<SalesReturnReason, int> ReturnReasonCounts { get; set; } = new();
        public Dictionary<SalesReturnStatus, int> StatusCounts { get; set; } = new();
    }
}
