using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購訂單明細服務介面
    /// </summary>
    public interface IPurchaseOrderDetailService : IGenericManagementService<PurchaseOrderDetail>
    {
        /// <summary>
        /// 根據採購訂單ID取得所有明細
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <returns>採購訂單明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetByPurchaseOrderIdAsync(int purchaseOrderId);

        /// <summary>
        /// 根據商品ID取得所有採購訂單明細
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>採購訂單明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據供應商ID取得所有採購訂單明細
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>採購訂單明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// 取得有待進貨數量的採購訂單明細
        /// </summary>
        /// <param name="supplierId">供應商ID（可選）</param>
        /// <returns>有待進貨數量的採購訂單明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsAsync(int? supplierId = null);

        /// <summary>
        /// 獲取廠商最近一次完整的採購訂單明細（智能下單用）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>最近一次採購的商品明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetLastCompletePurchaseAsync(int supplierId);

        /// <summary>
        /// 根據供應商ID取得有待進貨數量的採購訂單明細
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>有待進貨數量的採購訂單明細清單</returns>
        Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierAsync(int supplierId);

        /// <summary>
        /// 批次更新採購訂單明細
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <param name="details">明細清單</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int purchaseOrderId, List<PurchaseOrderDetail> details);

        /// <summary>
        /// 更新已進貨數量
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <param name="receivedQuantity">進貨數量</param>
        /// <param name="receivedAmount">進貨金額</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateReceivedQuantityAsync(int purchaseOrderDetailId, int receivedQuantity, decimal receivedAmount);

        /// <summary>
        /// 批次更新已進貨數量
        /// </summary>
        /// <param name="updates">更新資料清單（明細ID, 進貨數量, 進貨金額）</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> BatchUpdateReceivedQuantityAsync(List<(int DetailId, int ReceivedQuantity, decimal ReceivedAmount)> updates);

        /// <summary>
        /// 檢查商品在指定採購訂單中是否已存在
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <param name="productId">商品ID</param>
        /// <param name="excludeId">排除的明細ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsProductExistsInOrderAsync(int purchaseOrderId, int productId, int? excludeId = null);

        /// <summary>
        /// 計算採購訂單明細的小計金額
        /// </summary>
        /// <param name="orderQuantity">訂購數量</param>
        /// <param name="unitPrice">單價</param>
        /// <returns>小計金額</returns>
        decimal CalculateSubtotalAmount(int orderQuantity, decimal unitPrice);

        /// <summary>
        /// 計算待進貨數量
        /// </summary>
        /// <param name="orderQuantity">訂購數量</param>
        /// <param name="receivedQuantity">已進貨數量</param>
        /// <returns>待進貨數量</returns>
        int CalculatePendingQuantity(int orderQuantity, int receivedQuantity);

        /// <summary>
        /// 取得採購訂單明細的進貨記錄
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <returns>進貨記錄清單</returns>
        Task<List<PurchaseReceivingDetail>> GetReceivingDetailsAsync(int purchaseOrderDetailId);

        /// <summary>
        /// 檢查採購訂單明細是否已完成進貨
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <returns>是否已完成進貨</returns>
        Task<bool> IsReceivingCompletedAsync(int purchaseOrderDetailId);

        /// <summary>
        /// 取得採購訂單明細的統計資料
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <returns>統計資料（總數量、已進貨數量、待進貨數量、總金額、已進貨金額）</returns>
        Task<(int TotalQuantity, int ReceivedQuantity, int PendingQuantity, decimal TotalAmount, decimal ReceivedAmount)> GetStatisticsAsync(int purchaseOrderId);

        /// <summary>
        /// 重新計算並更新已進貨數量和金額
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> RecalculateReceivedQuantityAsync(int purchaseOrderDetailId);
    }
}
