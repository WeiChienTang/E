using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 審核類型列舉
    /// </summary>
    public enum ApprovalType
    {
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

        // ===== 審核流程開關查詢 =====

        /// <summary>
        /// 統一的審核開關查詢（泛型版本，避免重複程式碼）
        /// </summary>
        /// <param name="approvalType">審核類型</param>
        /// <returns>是否啟用審核</returns>
        Task<bool> IsApprovalEnabledAsync(ApprovalType approvalType);

        /// <summary>
        /// 檢查採購單是否需要審核
        /// </summary>
        Task<bool> IsPurchaseOrderApprovalEnabledAsync();

        /// <summary>
        /// 檢查進貨單是否需要審核
        /// </summary>
        Task<bool> IsPurchaseReceivingApprovalEnabledAsync();

        /// <summary>
        /// 檢查進貨退回是否需要審核
        /// </summary>
        Task<bool> IsPurchaseReturnApprovalEnabledAsync();

        /// <summary>
        /// 檢查銷貨單是否需要審核
        /// </summary>
        Task<bool> IsSalesOrderApprovalEnabledAsync();

        /// <summary>
        /// 檢查銷貨退回是否需要審核
        /// </summary>
        Task<bool> IsSalesReturnApprovalEnabledAsync();

        /// <summary>
        /// 檢查庫存調撥是否需要審核
        /// </summary>
        Task<bool> IsInventoryTransferApprovalEnabledAsync();

        /// <summary>
        /// 清除審核配置快取（當系統參數更新時使用）
        /// </summary>
        void ClearApprovalConfigCache();
    }
}