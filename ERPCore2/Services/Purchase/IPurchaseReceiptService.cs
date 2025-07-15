using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購進貨服務介面
    /// </summary>
    public interface IPurchaseReceiptService : IGenericManagementService<PurchaseReceipt>
    {
        // 基本查詢
        Task<List<PurchaseReceipt>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        Task<List<PurchaseReceipt>> GetByStatusAsync(PurchaseReceiptStatus status);
        Task<List<PurchaseReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PurchaseReceipt?> GetByNumberAsync(string receiptNumber);
        
        // 進貨操作
        Task<ServiceResult> ConfirmReceiptAsync(int receiptId, int confirmedBy);
        Task<ServiceResult> CancelReceiptAsync(int receiptId, string reason);
        Task<ServiceResult> ProcessInventoryAsync(int receiptId);
        
        // 明細相關
        Task<List<PurchaseReceiptDetail>> GetReceiptDetailsAsync(int receiptId);
        Task<ServiceResult> AddReceiptDetailAsync(PurchaseReceiptDetail detail);
        Task<ServiceResult> UpdateReceiptDetailAsync(PurchaseReceiptDetail detail);
        Task<ServiceResult> DeleteReceiptDetailAsync(int detailId);
        
        // 自動產生編號
        Task<string> GenerateReceiptNumberAsync();
        
        // 庫存相關
        Task<ServiceResult> UpdateInventoryStockAsync(int receiptId);
        Task<bool> CanDeleteAsync(int receiptId);
        
        // 統計查詢
        Task<List<PurchaseReceipt>> GetPendingReceiptsAsync();
        Task<decimal> GetTotalReceiptAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
