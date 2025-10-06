using ERPCore2.Data.Entities;
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
        
        // 訂單明細管理
        Task<List<PurchaseOrderDetail>> GetOrderDetailsAsync(int purchaseOrderId);
        Task<ServiceResult> AddOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> UpdateOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> DeleteOrderDetailAsync(int detailId);
        
        // 進貨相關查詢
        Task<List<PurchaseOrderDetail>> GetReceivingDetailsBySupplierAsync(int supplierId, bool includeCompleted);
        Task<List<PurchaseOrder>> GetIncompleteOrdersBySupplierAsync(int supplierId);
        
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
