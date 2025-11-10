using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨/出貨單明細服務介面
    /// </summary>
    public interface ISalesDeliveryDetailService : IGenericManagementService<SalesDeliveryDetail>
    {
        /// <summary>
        /// 根據銷貨單ID取得明細列表
        /// </summary>
        Task<List<SalesDeliveryDetail>> GetByDeliveryIdAsync(int deliveryId);

        /// <summary>
        /// 根據銷售訂單明細ID取得出貨明細列表
        /// </summary>
        Task<List<SalesDeliveryDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);

        /// <summary>
        /// 取得銷貨明細的統計資訊
        /// </summary>
        Task<SalesDeliveryDetailStatistics> GetStatisticsAsync(int? deliveryId = null, int? productId = null);
    }

    /// <summary>
    /// 銷貨明細統計資訊
    /// </summary>
    public class SalesDeliveryDetailStatistics
    {
        public int TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
