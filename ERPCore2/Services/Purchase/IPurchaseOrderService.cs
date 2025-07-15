using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status);
        Task<List<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber);
        
        // 訂單操作
        Task<ServiceResult> SubmitOrderAsync(int orderId);
        Task<ServiceResult> ApproveOrderAsync(int orderId, int approvedBy);
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
        
        // 自動產生編號
        Task<string> GenerateOrderNumberAsync();
    }
}
