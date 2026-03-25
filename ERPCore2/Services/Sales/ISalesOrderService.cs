using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單服務介面
    /// </summary>
    public interface ISalesOrderService : IGenericManagementService<SalesOrder>
    {
        /// <summary>
        /// 檢查銷貨訂單編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        Task<bool> IsSalesOrderCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據客戶ID取得銷貨訂單
        /// </summary>
        Task<List<SalesOrder>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據日期範圍取得銷貨訂單
        /// </summary>
        Task<List<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 計算訂單總金額
        /// </summary>
        Task<ServiceResult> CalculateOrderTotalAsync(int orderId);

        /// <summary>
        /// 取得銷貨訂單包含明細
        /// </summary>
        Task<SalesOrder?> GetWithDetailsAsync(int orderId);

        /// <summary>
        /// 根據批次列印條件查詢銷貨訂單（批次列印專用）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的銷貨訂單列表</returns>
        Task<List<SalesOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);

        /// <summary>
        /// 取得客戶的出貨明細（可篩選是否包含已完成和是否檢查審核）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="includeCompleted">是否包含已完成的明細</param>
        /// <param name="checkApproval">是否檢查審核狀態（true=只載入已審核，false=不檢查審核）</param>
        /// <returns>符合條件的銷貨訂單明細列表</returns>
        Task<List<SalesOrderDetail>> GetDeliveryDetailsByCustomerAsync(int customerId, bool includeCompleted, bool checkApproval = true);

        /// <summary>
        /// 取得訂單庫存檢查結果
        /// </summary>
        /// <param name="salesOrderId">銷貨訂單ID</param>
        /// <returns>庫存檢查結果</returns>
        Task<OrderInventoryCheckResult?> GetOrderInventoryCheckAsync(int salesOrderId);

        /// <summary>
        /// 依報價單 ID 取得轉換而來的銷貨訂單（跨訂單明細查詢）
        /// </summary>
        Task<List<SalesOrder>> GetByQuotationIdAsync(int quotationId);

        /// <summary>
        /// 依銷貨出貨單 ID 取得來源銷貨訂單（跨出貨明細查詢）
        /// </summary>
        Task<List<SalesOrder>> GetBySalesDeliveryIdAsync(int salesDeliveryId);

        /// <summary>
        /// 核准銷貨訂單
        /// </summary>
        Task<ServiceResult> ApproveAsync(int id, int? approvedBy);

        /// <summary>
        /// 駁回銷貨訂單
        /// </summary>
        Task<ServiceResult> RejectAsync(int id, int rejectedBy, string reason);

        /// <summary>
        /// 伺服器端分頁查詢（不載入 Details，僅取列表所需欄位）。
        /// </summary>
        Task<(List<SalesOrder> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<SalesOrder>, IQueryable<SalesOrder>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
