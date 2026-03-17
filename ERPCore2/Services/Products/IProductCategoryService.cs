using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項分類服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IProductCategoryService : IGenericManagementService<ProductCategory>
    {
        Task<(List<ProductCategory> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ProductCategory>, IQueryable<ProductCategory>>? filterFunc,
            int pageNumber,
            int pageSize);

        #region 業務特定方法
        
        /// <summary>
        /// 檢查品項分類名稱是否存在
        /// </summary>
        Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeId = null);
        
        /// <summary>
        /// 檢查品項分類編號是否存在
        /// </summary>
        Task<bool> IsProductCategoryCodeExistsAsync(string categoryCode, int? excludeId = null);
        
        /// <summary>
        /// 根據分類名稱取得品項分類
        /// </summary>
        Task<ProductCategory?> GetByCategoryNameAsync(string categoryName);
        
        /// <summary>
        /// 根據分類編號取得品項分類
        /// </summary>
        Task<ProductCategory?> GetByCategoryCodeAsync(string categoryCode);
        
        /// <summary>
        /// 檢查是否可以刪除分類（檢查是否有品項使用此分類）
        /// </summary>
        Task<bool> CanDeleteCategoryAsync(int categoryId);
        
        #endregion
    }
}

