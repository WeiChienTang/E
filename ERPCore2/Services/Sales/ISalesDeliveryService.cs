using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨/出貨單服務介面
    /// </summary>
    public interface ISalesDeliveryService : IGenericManagementService<SalesDelivery>
    {
        /// <summary>
        /// 檢查銷貨出貨編號是否已存在（符合 EntityCodeGenerationHelper 約定）
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

        /// <summary>
        /// 確認銷貨出貨單並更新庫存（首次新增時使用）
        /// 功能：執行出貨確認流程，將出貨數量從庫存扣除
        /// 使用原始單號作為 TransactionNumber，不帶 _ADJ 後綴
        /// </summary>
        Task<ServiceResult> ConfirmDeliveryAsync(int id, int confirmedBy = 0);

        /// <summary>
        /// 更新銷貨出貨單的庫存（編輯時使用）
        /// 比較編輯前後的明細差異，使用淨值計算方式確保庫存準確性
        /// 使用 Code_ADJ 作為 TransactionNumber
        /// </summary>
        Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0);
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
