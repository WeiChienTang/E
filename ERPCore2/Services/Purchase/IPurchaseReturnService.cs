using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
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
        Task<List<PurchaseReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<PurchaseReturn>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);

        Task<List<PurchaseReturn>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId);
        Task<PurchaseReturn?> GetWithDetailsAsync(int id);
        Task<bool> IsPurchaseReturnNumberExistsAsync(string purchaseReturnNumber, int? excludeId = null);

        // 業務邏輯
        Task<ServiceResult> CalculateTotalsAsync(int id);

        Task<ServiceResult> CreateFromPurchaseReceivingAsync(int purchaseReceivingId, List<PurchaseReturnDetail> details);

        // 儲存和明細管理
        Task<ServiceResult<PurchaseReturn>> SaveWithDetailsAsync(PurchaseReturn purchaseReturn, List<PurchaseReturnDetail> details);
        Task<ServiceResult> UpdateDetailsAsync(int purchaseReturnId, List<PurchaseReturnDetail> details);

        // 報表和統計
        Task<PurchaseReturnStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalReturnAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// 採購退回統計資料
    /// </summary>
    public class PurchaseReturnStatistics
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public Dictionary<int, decimal> SupplierReturnAmounts { get; set; } = new();
    }
}
