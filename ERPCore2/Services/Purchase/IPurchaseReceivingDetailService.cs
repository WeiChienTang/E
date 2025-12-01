using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購入庫明細服務介面
    /// </summary>
    public interface IPurchaseReceivingDetailService : IGenericManagementService<PurchaseReceivingDetail>
    {
        /// <summary>
        /// 根據採購入庫單ID取得所有明細
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫單ID</param>
        /// <returns>採購入庫明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId);

        /// <summary>
        /// 根據採購訂單明細ID取得所有入庫明細
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <returns>採購入庫明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetByPurchaseOrderDetailIdAsync(int purchaseOrderDetailId);

        /// <summary>
        /// 根據商品ID取得所有入庫明細
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>採購入庫明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據倉庫ID取得所有入庫明細
        /// </summary>
        /// <param name="warehouseId">倉庫ID</param>
        /// <returns>採購入庫明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// 根據廠商ID取得可退回的入庫明細
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <returns>可退回的採購入庫明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetReturnableDetailsBySupplierAsync(int supplierId);

        /// <summary>
        /// 取得指定廠商的最後一次完整入庫明細（智能載入用）
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <returns>最後一次完整入庫的明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetLastCompleteReceivingAsync(int supplierId);

        /// <summary>
        /// 批次更新採購入庫明細
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫單ID</param>
        /// <param name="details">明細清單</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details);

        /// <summary>
        /// 批次更新採購入庫明細 - 支援外部交易
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫單ID</param>
        /// <param name="details">明細清單</param>
        /// <param name="externalTransaction">外部交易</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? externalTransaction);

        /// <summary>
        /// 檢查商品在指定採購入庫單中是否已存在
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫單ID</param>
        /// <param name="productId">商品ID</param>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <param name="excludeId">排除的明細ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsProductExistsInReceivingAsync(int purchaseReceivingId, int productId, int purchaseOrderDetailId, int? excludeId = null);

        /// <summary>
        /// 檢查商品在指定採購入庫單的特定倉庫位置是否已存在
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫單ID</param>
        /// <param name="productId">商品ID</param>
        /// <param name="warehouseId">倉庫ID</param>
        /// <param name="warehouseLocationId">庫位ID</param>
        /// <param name="excludeId">排除的明細ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsProductWarehouseLocationExistsInReceivingAsync(int purchaseReceivingId, int productId, int warehouseId, int? warehouseLocationId, int? excludeId = null);

        /// <summary>
        /// 計算採購入庫明細的小計金額
        /// </summary>
        /// <param name="receivedQuantity">入庫數量</param>
        /// <param name="unitPrice">單價</param>
        /// <returns>小計金額</returns>
        decimal CalculateSubtotalAmount(int receivedQuantity, decimal unitPrice);

        /// <summary>
        /// 驗證入庫數量是否超過訂購數量
        /// </summary>
        /// <param name="purchaseOrderDetailId">採購訂單明細ID</param>
        /// <param name="newReceivedQuantity">新增入庫數量</param>
        /// <param name="excludeDetailId">排除的入庫明細ID（編輯時使用）</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> ValidateReceivingQuantityAsync(int purchaseOrderDetailId, int newReceivedQuantity, int? excludeDetailId = null);

        /// <summary>
        /// 取得採購入庫明細的庫存異動記錄
        /// </summary>
        /// <param name="purchaseReceivingDetailId">採購入庫明細ID</param>
        /// <returns>庫存異動記錄清單</returns>
        Task<List<InventoryTransaction>> GetRelatedInventoryTransactionsAsync(int purchaseReceivingDetailId);
    }
}