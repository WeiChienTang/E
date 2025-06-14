using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品分類服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IProductCategoryService : IGenericManagementService<ProductCategory>
    {
        #region 業務特定方法
        
        /// <summary>
        /// 檢查商品分類名稱是否存在
        /// </summary>
        Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeId = null);
        
        /// <summary>
        /// 檢查商品分類代碼是否存在
        /// </summary>
        Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeId = null);
        
        /// <summary>
        /// 根據分類名稱取得商品分類
        /// </summary>
        Task<ProductCategory?> GetByCategoryNameAsync(string categoryName);
        
        /// <summary>
        /// 根據分類代碼取得商品分類
        /// </summary>
        Task<ProductCategory?> GetByCategoryCodeAsync(string categoryCode);
        
        /// <summary>
        /// 取得頂層分類（無父分類）
        /// </summary>
        Task<List<ProductCategory>> GetTopLevelCategoriesAsync();
        
        /// <summary>
        /// 取得指定分類的子分類
        /// </summary>
        Task<List<ProductCategory>> GetChildCategoriesAsync(int parentCategoryId);
        
        /// <summary>
        /// 取得分類階層樹狀結構
        /// </summary>
        Task<List<ProductCategory>> GetCategoryTreeAsync();
        
        /// <summary>
        /// 檢查是否可以刪除分類（檢查是否有商品或子分類使用此分類）
        /// </summary>
        Task<bool> CanDeleteCategoryAsync(int categoryId);
        
        /// <summary>
        /// 檢查是否可以設定為父分類（避免循環參考）
        /// </summary>
        Task<bool> CanSetAsParentAsync(int categoryId, int parentCategoryId);
        
        #endregion
    }
}
