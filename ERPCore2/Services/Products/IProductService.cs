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
        /// 根據商品編號取得商品
        /// </summary>
        Task<Product?> GetByProductCodeAsync(string productCode);
        
        /// <summary>
        /// 根據條碼取得商品
        /// </summary>
        Task<Product?> GetByBarcodeAsync(string barcode);
        
        /// <summary>
        /// 檢查商品編號是否存在
        /// </summary>
        Task<bool> IsProductCodeExistsAsync(string productCode, int? excludeId = null);
        
        /// <summary>
        /// 檢查條碼編號是否存在
        /// </summary>
        Task<bool> IsBarcodeExistsAsync(string barcode, int? excludeId = null);
        
        /// <summary>
        /// 根據商品分類取得商品列表
        /// </summary>
        Task<List<Product>> GetByProductCategoryAsync(int productCategoryId);
        
        /// <summary>
        /// 根據供應商取得商品列表（包括主要供應商和關聯供應商）
        /// </summary>
        Task<List<Product>> GetBySupplierAsync(int supplierId);
        
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

