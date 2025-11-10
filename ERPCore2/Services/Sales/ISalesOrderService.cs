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
        /// 檢查銷貨訂單代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
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
        /// 驗證指定倉庫的銷貨訂單明細庫存是否足夠
        /// </summary>
        /// <param name="salesOrderDetails">銷貨訂單明細清單</param>
        /// <returns>驗證結果，包含庫存不足的詳細訊息</returns>
        Task<ServiceResult> ValidateWarehouseInventoryStockAsync(List<SalesOrderDetail> salesOrderDetails);

        /// <summary>
        /// 根據批次列印條件查詢銷貨訂單（批次列印專用）
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的銷貨訂單列表</returns>
        Task<List<SalesOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
    }
}
