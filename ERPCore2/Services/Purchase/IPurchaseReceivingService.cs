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
        /// 根據狀態獲取進貨單
        /// </summary>
        /// <param name="status">進貨狀態</param>
        /// <returns>進貨單列表</returns>
        Task<List<PurchaseReceiving>> GetByStatusAsync(PurchaseReceivingStatus status);

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
    }
}
