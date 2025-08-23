using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 供應商定價服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierPricingService : IGenericManagementService<SupplierPricing>
    {
        /// <summary>
        /// 根據商品ID取得供應商定價
        /// </summary>
        Task<List<SupplierPricing>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據供應商ID取得定價
        /// </summary>
        Task<List<SupplierPricing>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// 根據商品ID和供應商ID取得定價
        /// </summary>
        Task<List<SupplierPricing>> GetByProductIdAndSupplierIdAsync(int productId, int supplierId);

        /// <summary>
        /// 取得有效的供應商定價（未過期）
        /// </summary>
        Task<List<SupplierPricing>> GetEffectivePricingAsync(int productId, DateTime? asOfDate = null);

        /// <summary>
        /// 檢查供應商商品代碼是否已存在
        /// </summary>
        Task<bool> IsSupplierProductCodeExistsAsync(int supplierId, string supplierProductCode, int? excludeId = null);

        /// <summary>
        /// 根據供應商商品代碼查詢
        /// </summary>
        Task<List<SupplierPricing>> GetBySupplierProductCodeAsync(int supplierId, string supplierProductCode);

        /// <summary>
        /// 取得即將到期的定價（指定天數內到期）
        /// </summary>
        Task<List<SupplierPricing>> GetExpiringPricingAsync(int daysFromNow = 30);
    }
}
