using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品定價服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IProductPricingService : IGenericManagementService<ProductPricing>
    {
        /// <summary>
        /// 根據商品ID取得商品定價
        /// </summary>
        Task<List<ProductPricing>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據客戶ID取得定價
        /// </summary>
        Task<List<ProductPricing>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據商品ID和客戶ID取得定價
        /// </summary>
        Task<List<ProductPricing>> GetByProductIdAndCustomerIdAsync(int productId, int customerId);

        /// <summary>
        /// 根據定價類型取得定價
        /// </summary>
        Task<List<ProductPricing>> GetByPricingTypeAsync(PricingType pricingType);

        /// <summary>
        /// 取得有效的商品定價（未過期）
        /// </summary>
        Task<List<ProductPricing>> GetEffectivePricingAsync(int productId, DateTime? asOfDate = null);

        /// <summary>
        /// 取得商品對特定客戶的最佳價格
        /// </summary>
        Task<ProductPricing?> GetBestPriceForCustomerAsync(int productId, int? customerId, int quantity = 1, DateTime? asOfDate = null);

        /// <summary>
        /// 根據商品ID和數量取得適用的定價
        /// </summary>
        Task<List<ProductPricing>> GetApplicablePricingAsync(int productId, int quantity, DateTime? asOfDate = null);

        /// <summary>
        /// 取得商品的標準價格
        /// </summary>
        Task<ProductPricing?> GetStandardPricingAsync(int productId, DateTime? asOfDate = null);

        /// <summary>
        /// 取得即將到期的定價（指定天數內到期）
        /// </summary>
        Task<List<ProductPricing>> GetExpiringPricingAsync(int daysFromNow = 30);

        /// <summary>
        /// 根據優先順序取得定價
        /// </summary>
        Task<List<ProductPricing>> GetByPriorityAsync(int productId, int? customerId = null, DateTime? asOfDate = null);
    }
}
