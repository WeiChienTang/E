using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購退回明細服務介面
    /// </summary>
    public interface IPurchaseReturnDetailService : IGenericManagementService<PurchaseReturnDetail>
    {
        // 查詢方法
        Task<List<PurchaseReturnDetail>> GetByPurchaseReturnIdAsync(int purchaseReturnId);
        Task<List<PurchaseReturnDetail>> GetByProductIdAsync(int productId);
        Task<List<PurchaseReturnDetail>> GetByPurchaseOrderDetailIdAsync(int purchaseOrderDetailId);
        Task<List<PurchaseReturnDetail>> GetByPurchaseReceivingDetailIdAsync(int purchaseReceivingDetailId);
        Task<PurchaseReturnDetail?> GetWithNavigationAsync(int id);

        // 業務邏輯
        Task<ServiceResult> ValidateReturnQuantityAsync(int id, int returnQuantity);
        Task<ServiceResult> CalculateSubtotalAsync(int id);

        // 統計和報表
        Task<decimal> GetTotalReturnAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<int, int>> GetReturnQuantityByProductAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<PurchaseReturnDetail>> GetHighValueReturnsAsync(decimal minAmount);
        
        /// <summary>
        /// 取得指定進貨明細的已退貨數量
        /// </summary>
        /// <param name="purchaseReceivingDetailId">進貨明細ID</param>
        /// <returns>已退貨數量</returns>
        Task<int> GetReturnedQuantityByReceivingDetailAsync(int purchaseReceivingDetailId);
    }
}
