using ERPCore2.Data.Entities;
using ERPCore2.Services.Interfaces;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 商品供應商關聯服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IProductSupplierService : IGenericManagementService<ProductSupplier>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據商品ID取得供應商關聯列表
        /// </summary>
        Task<List<ProductSupplier>> GetByProductIdAsync(int productId);
        
        /// <summary>
        /// 根據供應商ID取得商品關聯列表
        /// </summary>
        Task<List<ProductSupplier>> GetBySupplierId(int supplierId);
        
        /// <summary>
        /// 取得商品的主要供應商關聯
        /// </summary>
        Task<ProductSupplier?> GetPrimarySupplierAsync(int productId);
        
        /// <summary>
        /// 取得指定商品和供應商的關聯
        /// </summary>
        Task<ProductSupplier?> GetByProductAndSupplierAsync(int productId, int supplierId);
        
        /// <summary>
        /// 取得所有主要供應商關聯
        /// </summary>
        Task<List<ProductSupplier>> GetPrimarySuppliersAsync();
        
        #endregion

        #region 業務邏輯操作
        
        /// <summary>
        /// 設定主要供應商
        /// </summary>
        Task<ServiceResult> SetPrimarySupplierAsync(int productSupplierId);
        
        /// <summary>
        /// 批次設定商品的供應商
        /// </summary>
        Task<ServiceResult> BatchSetProductSuppliersAsync(int productId, List<int> supplierIds);
        
        /// <summary>
        /// 批次設定供應商的商品
        /// </summary>
        Task<ServiceResult> BatchSetSupplierProductsAsync(int supplierId, List<int> productIds);
        
        /// <summary>
        /// 更新供應商價格資訊
        /// </summary>
        Task<ServiceResult> UpdateSupplierPriceAsync(int productSupplierId, decimal? supplierPrice, string? supplierProductCode = null);
        
        /// <summary>
        /// 更新交期和最小訂購量
        /// </summary>
        Task<ServiceResult> UpdateDeliveryInfoAsync(int productSupplierId, int? leadTime, int? minOrderQuantity);
        
        /// <summary>
        /// 檢查商品是否有供應商
        /// </summary>
        Task<bool> HasSuppliersAsync(int productId);
        
        /// <summary>
        /// 檢查供應商是否有商品
        /// </summary>
        Task<bool> HasProductsAsync(int supplierId);
        
        /// <summary>
        /// 取得供應商的最佳價格商品
        /// </summary>
        Task<List<ProductSupplier>> GetBestPriceProductsAsync(int supplierId);
        
        /// <summary>
        /// 取得商品的最佳價格供應商
        /// </summary>
        Task<List<ProductSupplier>> GetBestPriceSuppliersAsync(int productId);
        
        #endregion

        #region 統計分析
        
        /// <summary>
        /// 取得商品的供應商數量
        /// </summary>
        Task<int> GetSupplierCountAsync(int productId);
        
        /// <summary>
        /// 取得供應商的商品數量
        /// </summary>
        Task<int> GetProductCountAsync(int supplierId);
        
        /// <summary>
        /// 取得平均交期
        /// </summary>
        Task<double> GetAverageLeadTimeAsync(int productId);
        
        /// <summary>
        /// 取得價格範圍
        /// </summary>
        Task<(decimal? MinPrice, decimal? MaxPrice)> GetPriceRangeAsync(int productId);
        
        #endregion
    }
}
