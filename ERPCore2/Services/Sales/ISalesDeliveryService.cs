using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨出貨服務介面
    /// </summary>
    public interface ISalesDeliveryService : IGenericManagementService<SalesDelivery>
    {
        /// <summary>
        /// 檢查出貨單號是否已存在
        /// </summary>
        Task<bool> IsDeliveryNumberExistsAsync(string deliveryNumber, int? excludeId = null);

        /// <summary>
        /// 根據銷貨訂單ID取得出貨單
        /// </summary>
        Task<List<SalesDelivery>> GetBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// 根據出貨狀態取得出貨單
        /// </summary>
        Task<List<SalesDelivery>> GetByDeliveryStatusAsync(SalesDeliveryStatus deliveryStatus);

        /// <summary>
        /// 根據日期範圍取得出貨單
        /// </summary>
        Task<List<SalesDelivery>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 更新出貨狀態
        /// </summary>
        Task<ServiceResult> UpdateDeliveryStatusAsync(int deliveryId, SalesDeliveryStatus newStatus);

        /// <summary>
        /// 取得出貨單包含明細
        /// </summary>
        Task<SalesDelivery?> GetWithDetailsAsync(int deliveryId);
    }
}
