using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 審核類型列舉
    /// </summary>
    public enum ApprovalType
    {
        /// <summary>報價單</summary>
        Quotation,
        /// <summary>採購單</summary>
        PurchaseOrder,
        /// <summary>進貨單</summary>
        PurchaseReceiving,
        /// <summary>進貨退回</summary>
        PurchaseReturn,
        /// <summary>銷貨單</summary>
        SalesOrder,
        /// <summary>銷貨退回</summary>
        SalesReturn,
        /// <summary>銷貨出貨</summary>
        SalesDelivery,
        /// <summary>庫存調撥</summary>
        InventoryTransfer
    }

    /// <summary>
    /// 系統參數服務介面
    /// </summary>
    public interface ISystemParameterService : IGenericManagementService<SystemParameter>
    {
        /// <summary>
        /// 取得系統稅率
        /// </summary>
        /// <returns>當前系統稅率</returns>
        Task<decimal> GetTaxRateAsync();

        /// <summary>
        /// 設定系統稅率
        /// </summary>
        /// <param name="taxRate">稅率值</param>
        /// <returns>是否設定成功</returns>
        Task<bool> SetTaxRateAsync(decimal taxRate);

        /// <summary>
        /// 取得第一個系統參數設定
        /// </summary>
        /// <returns>系統參數物件</returns>
        Task<SystemParameter?> GetSystemParameterAsync();

        // ===== 審核模式查詢（false=系統自動審核，true=人工審核）=====

        /// <summary>
        /// 統一的審核模式查詢（泛型版本，避免重複程式碼）
        /// </summary>
        /// <param name="approvalType">審核類型</param>
        /// <returns>是否使用人工審核（false=自動審核）</returns>
        Task<bool> IsManualApprovalAsync(ApprovalType approvalType);

        /// <summary>
        /// 報價單是否使用人工審核
        /// </summary>
        Task<bool> IsQuotationManualApprovalAsync();

        /// <summary>
        /// 採購訂單是否使用人工審核
        /// </summary>
        Task<bool> IsPurchaseOrderManualApprovalAsync();

        /// <summary>
        /// 進貨單是否使用人工審核
        /// </summary>
        Task<bool> IsPurchaseReceivingManualApprovalAsync();

        /// <summary>
        /// 進貨退回是否使用人工審核
        /// </summary>
        Task<bool> IsPurchaseReturnManualApprovalAsync();

        /// <summary>
        /// 銷貨訂單是否使用人工審核
        /// </summary>
        Task<bool> IsSalesOrderManualApprovalAsync();

        /// <summary>
        /// 銷貨退回是否使用人工審核
        /// </summary>
        Task<bool> IsSalesReturnManualApprovalAsync();

        /// <summary>
        /// 出貨單是否使用人工審核
        /// </summary>
        Task<bool> IsSalesDeliveryManualApprovalAsync();

        /// <summary>
        /// 庫存調撥是否使用人工審核
        /// </summary>
        Task<bool> IsInventoryTransferManualApprovalAsync();

        /// <summary>
        /// 是否隱藏所有模組的審核資訊欄位（false=顯示，true=隱藏）
        /// </summary>
        Task<bool> IsApprovalInfoHiddenAsync();

        /// <summary>
        /// 清除審核配置快取（當系統參數更新時使用）
        /// </summary>
        void ClearApprovalConfigCache();

        /// <summary>
        /// 將系統參數重置為預設值
        /// </summary>
        /// <returns>操作結果</returns>
        Task<ServiceResult<SystemParameter>> ResetToDefaultAsync();
    }
}
