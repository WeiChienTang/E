using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IProductService : IGenericManagementService<Product>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據商品代碼取得商品
        /// </summary>
        Task<Product?> GetByProductCodeAsync(string productCode);
        
        /// <summary>
        /// 檢查商品代碼是否存在
        /// </summary>
        Task<bool> IsProductCodeExistsAsync(string productCode, int? excludeId = null);
        
        /// <summary>
        /// 根據商品分類取得商品列表
        /// </summary>
        Task<List<Product>> GetByProductCategoryAsync(int productCategoryId);
        
        /// <summary>
        /// 根據主要供應商取得商品列表
        /// </summary>
        Task<List<Product>> GetByPrimarySupplierAsync(int supplierId);
        
        /// <summary>
        /// 取得啟用的商品列表
        /// </summary>
        Task<List<Product>> GetActiveProductsAsync();
        
        #endregion

        #region 輔助資料查詢
        
        /// <summary>
        /// 取得所有商品分類
        /// </summary>
        Task<List<ProductCategory>> GetProductCategoriesAsync();
        
        /// <summary>
        /// 取得所有供應商
        /// </summary>
        Task<List<Supplier>> GetSuppliersAsync();
        
        /// <summary>
        /// 取得所有單位
        /// </summary>
        Task<List<Unit>> GetUnitsAsync();
        
        #endregion

        #region 供應商管理
        
        /// <summary>
        /// 取得商品的供應商列表
        /// </summary>
        Task<List<ProductSupplier>> GetProductSuppliersAsync(int productId);
        
        /// <summary>
        /// 更新商品供應商關聯
        /// </summary>
        Task<ServiceResult> UpdateProductSuppliersAsync(int productId, List<ProductSupplier> productSuppliers);
        
        /// <summary>
        /// 設定主要供應商
        /// </summary>
        Task<ServiceResult> SetPrimarySupplierAsync(int productId, int supplierId);
        
        #endregion

        #region 價格管理
        
        /// <summary>
        /// 更新商品價格
        /// </summary>
        Task<ServiceResult> UpdatePricesAsync(int productId, decimal? unitPrice, decimal? costPrice);
        
        /// <summary>
        /// 批次更新價格
        /// </summary>
        Task<ServiceResult> BatchUpdatePricesAsync(List<int> productIds, decimal? priceAdjustment, bool isPercentage);
        
        #endregion

        #region 狀態管理
        
        /// <summary>
        /// 切換商品啟用狀態
        /// </summary>
        Task<ServiceResult> ToggleActiveStatusAsync(int productId);
        
        /// <summary>
        /// 批次設定商品狀態
        /// </summary>
        Task<ServiceResult> BatchSetActiveStatusAsync(List<int> productIds, bool isActive);
        
        #endregion

        #region 輔助方法
        
        /// <summary>
        /// 初始化新商品
        /// </summary>
        void InitializeNewProduct(Product product);
        
        /// <summary>
        /// 取得基本必填欄位數量
        /// </summary>
        int GetBasicRequiredFieldsCount();
        
        /// <summary>
        /// 取得基本完成欄位數量
        /// </summary>
        int GetBasicCompletedFieldsCount(Product product);
        
        #endregion
    }
}

