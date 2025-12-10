using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單明細服務介面
    /// </summary>
    public interface ISalesOrderDetailService : IGenericManagementService<SalesOrderDetail>
    {
        /// <summary>
        /// 根據銷貨訂單ID取得明細清單
        /// </summary>
        Task<List<SalesOrderDetail>> GetBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// 根據商品ID取得銷貨訂單明細
        /// </summary>
        Task<List<SalesOrderDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 計算明細小計
        /// </summary>
        Task<ServiceResult> CalculateSubtotalAsync(int detailId);

        /// <summary>
        /// 批次更新明細資料
        /// </summary>
        Task<ServiceResult> UpdateDetailsAsync(List<SalesOrderDetail> details);

        /// <summary>
        /// 刪除指定銷貨訂單的所有明細
        /// </summary>
        Task<ServiceResult> DeleteBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// 檢查商品在訂單中是否已存在
        /// </summary>
        Task<bool> IsProductExistsInOrderAsync(int salesOrderId, int productId, int? excludeDetailId = null);

        /// <summary>
        /// 取得銷貨訂單明細包含關聯資料
        /// </summary>
        Task<SalesOrderDetail?> GetWithIncludesAsync(int detailId);

        /// <summary>
        /// 根據銷貨訂單ID取得明細包含關聯資料
        /// </summary>
        Task<List<SalesOrderDetail>> GetBySalesOrderIdWithIncludesAsync(int salesOrderId);

        /// <summary>
        /// 更新銷貨明細並處理庫存回滾/重新分配
        /// </summary>
        Task<ServiceResult> UpdateDetailsWithInventoryAsync(
            int salesOrderId, 
            List<SalesOrderDetail> newDetails, 
            List<SalesOrderDetail> originalDetails);

        /// <summary>
        /// 根據客戶取得可退貨的銷售訂單明細
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>可退貨的銷售訂單明細清單</returns>
        Task<List<SalesOrderDetail>> GetReturnableDetailsByCustomerAsync(int customerId);

        /// <summary>
        /// 獲取客戶最近一次完整的銷貨訂單明細（智能下單用）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>最近一次銷貨訂單的明細清單</returns>
        Task<List<SalesOrderDetail>> GetLastCompleteSalesAsync(int customerId);

        /// <summary>
        /// 重新計算並更新已出貨數量和金額
        /// 用於在銷貨出貨單儲存後，回寫已出貨數量到銷貨訂單明細
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <returns>執行結果</returns>
        Task<ServiceResult> RecalculateDeliveredQuantityAsync(int salesOrderDetailId);
        
        /// <summary>
        /// 重新計算並更新已出貨數量和金額（支援外部 DbContext）
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <param name="context">外部 DbContext（用於 Transaction 內）</param>
        /// <returns>執行結果</returns>
        Task<ServiceResult> RecalculateDeliveredQuantityAsync(int salesOrderDetailId, Data.Context.AppDbContext context);
    }
}
