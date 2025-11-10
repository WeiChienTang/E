using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
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
        /// 更新採購進貨單的庫存（差異更新模式）
        /// 功能：比較編輯前後的明細差異，只更新變更的部分，避免重複累加
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="updatedBy">更新人員ID</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0);

        /// <summary>
        /// 生成進貨單號
        /// </summary>
        /// <returns>進貨單號</returns>
        Task<string> GenerateReceiptNumberAsync();

        /// <summary>
        /// 檢查進貨代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">進貨代碼</param>
        /// <param name="excludeId">排除的ID（用於編輯模式）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPurchaseReceivingCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 取得指定產品最近一次進貨的倉庫和位置資訊
        /// </summary>
        /// <param name="productId">產品ID</param>
        /// <returns>倉庫ID和倉庫位置ID的元組，如無歷史記錄則回傳null</returns>
        Task<(int? WarehouseId, int? WarehouseLocationId)> GetLastReceivingLocationAsync(int productId);

        /// <summary>
        /// 取得指定廠商的可退貨明細（已進貨但尚未全部退貨）
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <returns>可退貨的進貨明細清單</returns>
        Task<List<PurchaseReceivingDetail>> GetReturnableDetailsBySupplierAsync(int supplierId);

        /// <summary>
        /// 根據批次列印條件查詢進貨單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的進貨單列表</returns>
        Task<List<PurchaseReceiving>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
    }
}