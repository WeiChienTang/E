using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨退回明細服務介面
    /// </summary>
    public interface ISalesReturnDetailService : IGenericManagementService<SalesReturnDetail>
    {
        /// <summary>
        /// 根據銷貨退回ID取得明細清單
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetBySalesReturnIdAsync(int salesReturnId);

        /// <summary>
        /// 根據產品ID取得退回明細清單
        /// </summary>
        /// <param name="productId">產品ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據銷貨出貨明細ID取得退回明細清單
        /// </summary>
        /// <param name="salesDeliveryDetailId">銷貨出貨明細ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetBySalesDeliveryDetailIdAsync(int salesDeliveryDetailId);



        /// <summary>
        /// 批量更新明細
        /// </summary>
        /// <param name="details">明細清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(List<SalesReturnDetail> details);

        /// <summary>
        /// 檢查明細是否可以刪除
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <returns>是否可以刪除</returns>
        Task<bool> CanDeleteDetailAsync(int detailId);

        /// <summary>
        /// 取得明細統計資訊
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>統計資訊</returns>
        Task<SalesReturnDetailStatistics> GetDetailStatisticsAsync(int salesReturnId);

        /// <summary>
        /// 驗證退回數量
        /// </summary>
        /// <param name="detail">明細實體</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult> ValidateReturnQuantityAsync(SalesReturnDetail detail);

        /// <summary>
        /// 取得指定銷售出貨明細的已退貨數量
        /// </summary>
        /// <param name="salesDeliveryDetailId">銷售出貨明細ID</param>
        /// <returns>已退貨數量</returns>
        Task<decimal> GetReturnedQuantityByDeliveryDetailAsync(int salesDeliveryDetailId);
    }

    /// <summary>
    /// 銷貨退回明細統計資訊
    /// </summary>
    public class SalesReturnDetailStatistics
    {
        public int TotalDetails { get; set; }
        public decimal TotalReturnQuantity { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public int ProductCount { get; set; }
        public Dictionary<int, decimal> ProductReturnQuantities { get; set; } = new();
    }
}
