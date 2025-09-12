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
        Task<ServiceResult> UpdateReceivedAmountAsync(int orderId);
        Task<bool> CanDeleteAsync(int orderId);
        
        // 統計查詢
        Task<List<PurchaseOrder>> GetPendingOrdersAsync();
        Task<List<PurchaseOrder>> GetOverdueOrdersAsync();
        Task<decimal> GetTotalOrderAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
        
        // 明細相關
        Task<List<PurchaseOrderDetail>> GetOrderDetailsAsync(int orderId);
        Task<ServiceResult> AddOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> UpdateOrderDetailAsync(PurchaseOrderDetail detail);
        Task<ServiceResult> DeleteOrderDetailAsync(int detailId);
        
        // 供應商相關進貨明細查詢 - 新增方法
        Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierAsync(int supplierId);
        Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierWithQuantityAsync(int supplierId);
        
        /// <summary>
        /// 取得指定供應商的所有採購明細（支援入庫量超過訂購量，商品持續顯示）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="isEditMode">編輯模式標記（保留用於向後相容性）</param>
        /// <returns>該供應商的所有已核准採購明細</returns>
        Task<List<PurchaseOrderDetail>> GetReceivingDetailsBySupplierAsync(int supplierId, bool isEditMode = false);
        
        // 新增：獲取供應商的未完成採購單（包含完整關聯資料用於判斷完成狀態）
        Task<List<PurchaseOrder>> GetIncompleteOrdersBySupplierAsync(int supplierId);
        
        // 自動產生編號
        Task<string> GenerateOrderNumberAsync();
    }
}
