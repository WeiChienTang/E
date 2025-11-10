using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨/出貨單服務介面
    /// </summary>
    public interface ISalesDeliveryService : IGenericManagementService<SalesDelivery>
    {
        /// <summary>
        /// 檢查銷貨出貨代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        Task<bool> IsSalesDeliveryCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據客戶ID取得銷貨單列表
        /// </summary>
        Task<List<SalesDelivery>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據銷售訂單ID取得銷貨單列表
        /// </summary>
        Task<List<SalesDelivery>> GetBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// 計算銷貨單總金額
        /// </summary>
        Task<ServiceResult<decimal>> CalculateTotalAmountAsync(int deliveryId);

        /// <summary>
        /// 取得銷貨單的統計資訊
        /// </summary>
        Task<SalesDeliveryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? customerId = null);
    }

    /// <summary>
    /// 銷貨單統計資訊
    /// </summary>
    public class SalesDeliveryStatistics
    {
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public int ShippedCount { get; set; }
        public int PendingCount { get; set; }
    }
}
