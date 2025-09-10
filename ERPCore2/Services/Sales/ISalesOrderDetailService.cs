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
    }
}
