using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單服務介面
    /// </summary>
    public interface ISalesOrderService : IGenericManagementService<SalesOrder>
    {
        /// <summary>
        /// 檢查銷貨單號是否已存在
        /// </summary>
        Task<bool> IsSalesOrderNumberExistsAsync(string salesOrderNumber, int? excludeId = null);

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
        /// 驗證銷貨訂單明細的庫存是否足夠
        /// </summary>
        /// <param name="salesOrderDetails">銷貨訂單明細清單</param>
        /// <returns>驗證結果，包含庫存不足的詳細訊息</returns>
        Task<ServiceResult> ValidateInventoryStockAsync(List<SalesOrderDetail> salesOrderDetails);
    }
}
