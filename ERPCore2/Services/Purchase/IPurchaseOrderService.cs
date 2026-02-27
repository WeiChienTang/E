using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購訂單服務介面
    /// </summary>
    public interface IPurchaseOrderService : IGenericManagementService<PurchaseOrder>
    {
        // 基本查詢
        Task<List<PurchaseOrder>> GetBySupplierIdAsync(int supplierId);
        Task<List<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber);
        
        /// <summary>
        /// 根據批次列印條件查詢採購單（批次列印專用）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的採購單列表</returns>
        Task<List<PurchaseOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
        
        // 訂單操作
        Task<ServiceResult> SubmitOrderAsync(int orderId);
        Task<ServiceResult> ApproveOrderAsync(int orderId, int approvedBy);
        Task<ServiceResult> RejectOrderAsync(int orderId, int rejectedBy, string reason);
        Task<ServiceResult> CancelOrderAsync(int orderId, string reason);
        Task<ServiceResult> CloseOrderAsync(int orderId);
        
        // 進貨相關
        // 注釋：UpdateReceivedAmountAsync 已移除，因為 ReceivedAmount 欄位已不存在
        Task<bool> CanDeleteAsync(int orderId);
        
        // 統計查詢
        Task<List<PurchaseOrder>> GetPendingOrdersAsync();
        Task<List<PurchaseOrder>> GetOverdueOrdersAsync();
        Task<decimal> GetTotalOrderAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
        
        // 自動產生編號
        Task<string> GenerateOrderNumberAsync();
        
        /// <summary>
        /// 檢查採購單編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">採購單編號</param>
        /// <param name="excludeId">排除的ID（用於編輯模式）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPurchaseOrderCodeExistsAsync(string code, int? excludeId = null);
        
        // 訂單明細管理
        Task<List<PurchaseOrderDetail>> GetOrderDetailsAsync(int purchaseOrderId);
        Task<ServiceResult> AddOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> UpdateOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> DeleteOrderDetailAsync(int detailId);
        
        // 進貨相關查詢
        /// <summary>
        /// 獲取廠商的可進貨採購明細（含審核過濾）
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <param name="includeCompleted">是否包含已完成的明細</param>
        /// <param name="checkApproval">是否檢查審核狀態（true=只載入已審核，false=不檢查審核）</param>
        Task<List<PurchaseOrderDetail>> GetReceivingDetailsBySupplierAsync(int supplierId, bool includeCompleted, bool checkApproval = true);
        Task<List<PurchaseOrder>> GetIncompleteOrdersBySupplierAsync(int supplierId);

        /// <summary>
        /// 依採購進貨單 ID 取得來源採購訂單（跨進貨明細查詢）
        /// </summary>
        Task<List<PurchaseOrder>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId);
        
        // 稅額計算
        /// <summary>
        /// 計算並更新採購單的稅額
        /// </summary>
        /// <param name="purchaseOrderId">採購單ID</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> CalculateAndUpdateTaxAmountAsync(int purchaseOrderId);
        
        /// <summary>
        /// 計算稅額(不儲存到資料庫)
        /// </summary>
        /// <param name="totalAmount">總金額</param>
        /// <returns>計算出的稅額</returns>
        Task<decimal> CalculateTaxAmountAsync(decimal totalAmount);
    }
}
