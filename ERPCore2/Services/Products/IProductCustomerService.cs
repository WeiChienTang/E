using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 商品-客戶關聯服務介面
    /// </summary>
    public interface IProductCustomerService : IGenericManagementService<ProductCustomer>
    {
        #region 查詢方法

        /// <summary>
        /// 依商品ID取得所有綁定的客戶
        /// </summary>
        Task<List<ProductCustomer>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 依客戶ID取得所有綁定的商品
        /// </summary>
        Task<List<ProductCustomer>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 取得指定商品的常用客戶列表（依優先順序排序）
        /// </summary>
        Task<List<ProductCustomer>> GetPreferredCustomersByProductIdAsync(int productId);

        /// <summary>
        /// 檢查商品-客戶綁定是否存在
        /// </summary>
        Task<bool> IsBindingExistsAsync(int productId, int customerId, int? excludeId = null);

        #endregion

        #region 輔助方法

        /// <summary>
        /// 更新指定綁定的最近銷售價格和日期
        /// </summary>
        Task UpdateLastSaleInfoAsync(int productId, int customerId, decimal price, DateTime date);

        #endregion
    }
}
