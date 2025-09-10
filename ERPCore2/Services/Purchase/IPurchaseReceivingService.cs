using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購進貨服務介面
    /// </summary>
    public interface IPurchaseReceivingService : IGenericManagementService<PurchaseReceiving>
    {
        /// <summary>
        /// 獲取指定日期範圍內的進貨單
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>進貨單列表</returns>
        Task<List<PurchaseReceiving>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 獲取指定採購訂單的進貨單
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <returns>進貨單列表</returns>
        Task<List<PurchaseReceiving>> GetByPurchaseOrderAsync(int purchaseOrderId);

        /// <summary>
        /// 確認進貨單
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="confirmedBy">確認人員ID</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> ConfirmReceiptAsync(int id, int confirmedBy);

        /// <summary>
        /// 取消進貨單
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> CancelReceiptAsync(int id);

        /// <summary>
        /// 生成進貨單號
        /// </summary>
        /// <returns>進貨單號</returns>
        Task<string> GenerateReceiptNumberAsync();

        /// <summary>
        /// 檢查入庫單號是否已存在
        /// </summary>
        /// <param name="receiptNumber">入庫單號</param>
        /// <param name="excludeId">排除的ID（用於編輯模式）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsReceiptNumberExistsAsync(string receiptNumber, int? excludeId = null);

        /// <summary>
        /// 儲存採購入庫連同明細
        /// </summary>
        /// <param name="purchaseReceiving">採購入庫主檔</param>
        /// <param name="details">入庫明細清單</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult<PurchaseReceiving>> SaveWithDetailsAsync(PurchaseReceiving purchaseReceiving, List<PurchaseReceivingDetail> details);

        /// <summary>
        /// 更新採購入庫明細
        /// </summary>
        /// <param name="purchaseReceivingId">採購入庫ID</param>
        /// <param name="details">明細清單</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details);

        /// <summary>
        /// 取得指定產品最近一次進貨的倉庫和位置資訊
        /// </summary>
        /// <param name="productId">產品ID</param>
        /// <returns>倉庫ID和倉庫位置ID的元組，如無歷史記錄則回傳null</returns>
        Task<(int? WarehouseId, int? WarehouseLocationId)> GetLastReceivingLocationAsync(int productId);
    }
}
