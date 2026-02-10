using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 價格歷史服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IPriceHistoryService : IGenericManagementService<PriceHistory>
    {
        /// <summary>
        /// 根據商品ID取得價格歷史
        /// </summary>
        Task<List<PriceHistory>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據商品ID和價格類型取得價格歷史
        /// </summary>
        Task<List<PriceHistory>> GetByProductIdAndPriceTypeAsync(int productId, PriceType priceType);

        /// <summary>
        /// 根據日期範圍取得價格歷史
        /// </summary>
        Task<List<PriceHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 根據變更人員取得價格歷史
        /// </summary>
        Task<List<PriceHistory>> GetByChangedByUserIdAsync(int userId);

        /// <summary>
        /// 取得商品的最新價格歷史記錄
        /// </summary>
        Task<PriceHistory?> GetLatestByProductIdAndPriceTypeAsync(int productId, PriceType priceType);

        /// <summary>
        /// 記錄價格變更
        /// </summary>
        Task<ServiceResult> RecordPriceChangeAsync(int productId, PriceType priceType, decimal oldPrice, decimal newPrice, string changeReason, int changedByUserId, string? changedByUserName = null, int? relatedCustomerId = null, int? relatedSupplierId = null, string? changeDetails = null);
    }
}
