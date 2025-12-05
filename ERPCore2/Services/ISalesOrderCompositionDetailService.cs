using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單組成明細服務介面
    /// </summary>
    public interface ISalesOrderCompositionDetailService : IGenericManagementService<SalesOrderCompositionDetail>
    {
        /// <summary>
        /// 取得指定銷貨訂單明細的組合明細
        /// </summary>
        Task<List<SalesOrderCompositionDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);
        
        /// <summary>
        /// 從商品合成表複製 BOM 資料到銷貨訂單（使用最新的配方）
        /// </summary>
        Task<List<SalesOrderCompositionDetail>> CopyFromProductCompositionAsync(
            int salesOrderDetailId, 
            int productId);
        
        /// <summary>
        /// 從指定的商品配方複製 BOM 資料到銷貨訂單
        /// </summary>
        Task<List<SalesOrderCompositionDetail>> CopyFromCompositionAsync(
            int salesOrderDetailId, 
            int compositionId);
        
        /// <summary>
        /// 批次儲存組合明細（新增、更新、刪除）
        /// </summary>
        Task SaveBatchAsync(
            int salesOrderDetailId, 
            List<SalesOrderCompositionDetail> compositionDetails);
        
        /// <summary>
        /// 刪除指定銷貨訂單明細的所有組合明細
        /// </summary>
        Task DeleteBySalesOrderDetailIdAsync(int salesOrderDetailId);
    }
}
