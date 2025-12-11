using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品-供應商關聯服務介面
    /// </summary>
    public interface IProductSupplierService : IGenericManagementService<ProductSupplier>
    {
        #region 查詢方法
        
        /// <summary>
        /// 依商品ID取得所有綁定的供應商
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>供應商綁定列表</returns>
        Task<List<ProductSupplier>> GetByProductIdAsync(int productId);
        
        /// <summary>
        /// 依供應商ID取得所有綁定的商品
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>商品綁定列表</returns>
        Task<List<ProductSupplier>> GetBySupplierIdAsync(int supplierId);
        
        /// <summary>
        /// 取得指定商品的常用供應商列表（依優先順序排序）
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>常用供應商列表</returns>
        Task<List<ProductSupplier>> GetPreferredSuppliersByProductIdAsync(int productId);
        
        /// <summary>
        /// 檢查商品-供應商綁定是否存在
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsBindingExistsAsync(int productId, int supplierId, int? excludeId = null);
        
        #endregion
        
        #region 批次操作
        
        /// <summary>
        /// 從採購歷史自動建立供應商-商品綁定
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>匯入的綁定數量</returns>
        Task<int> ImportFromPurchaseHistoryAsync(int supplierId);
        
        /// <summary>
        /// 批次更新最近採購價格（從採購單資料）
        /// </summary>
        /// <param name="supplierId">供應商ID（可選）</param>
        /// <returns>更新的數量</returns>
        Task<int> BatchUpdateLastPurchasePriceAsync(int? supplierId = null);
        
        #endregion
        
        #region 輔助方法
        
        /// <summary>
        /// 更新指定綁定的最近採購價格和日期
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="price">價格</param>
        /// <param name="date">日期</param>
        Task UpdateLastPurchaseInfoAsync(int productId, int supplierId, decimal price, DateTime date);
        
        #endregion
    }
}
