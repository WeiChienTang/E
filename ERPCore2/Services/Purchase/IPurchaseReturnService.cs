using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購退回服務介面
    /// </summary>
    public interface IPurchaseReturnService : IGenericManagementService<PurchaseReturn>
    {
        // 查詢方法
        Task<List<PurchaseReturn>> GetBySupplierIdAsync(int supplierId);
        Task<List<PurchaseReturn>> GetByStatusAsync(PurchaseReturnStatus status);
        Task<List<PurchaseReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<PurchaseReturn>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        Task<List<PurchaseReturn>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId);
        Task<PurchaseReturn?> GetWithDetailsAsync(int id);
        Task<bool> IsPurchaseReturnNumberExistsAsync(string purchaseReturnNumber, int? excludeId = null);

        // 狀態管理
        Task<ServiceResult> SubmitAsync(int id);
        Task<ServiceResult> ConfirmAsync(int id, int confirmedBy);
        Task<ServiceResult> StartProcessingAsync(int id);
        Task<ServiceResult> CompleteAsync(int id);
        Task<ServiceResult> CancelAsync(int id, string? remarks = null);
        Task<ServiceResult> UpdateStatusAsync(int id, PurchaseReturnStatus status, string? remarks = null);

        // 業務邏輯
        Task<ServiceResult> CalculateTotalsAsync(int id);
        Task<ServiceResult> RefundProcessAsync(int id, decimal refundAmount, string? remarks = null);
        Task<ServiceResult> CreateFromPurchaseOrderAsync(int purchaseOrderId, List<PurchaseReturnDetail> details);
        Task<ServiceResult> CreateFromPurchaseReceivingAsync(int purchaseReceivingId, List<PurchaseReturnDetail> details);

        // 報表和統計
        Task<PurchaseReturnStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<PurchaseReturn>> GetPendingReturnsAsync();
        Task<List<PurchaseReturn>> GetOverdueReturnsAsync();
        Task<decimal> GetTotalReturnAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// 採購退回統計資料
    /// </summary>
    public class PurchaseReturnStatistics
    {
        public int TotalReturns { get; set; }
        public int PendingReturns { get; set; }
        public int ProcessingReturns { get; set; }
        public int CompletedReturns { get; set; }
        public int CancelledReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public Dictionary<PurchaseReturnReason, int> ReturnReasonCounts { get; set; } = new();
        public Dictionary<PurchaseReturnStatus, int> StatusCounts { get; set; } = new();
        public Dictionary<int, decimal> SupplierReturnAmounts { get; set; } = new();
    }
}
