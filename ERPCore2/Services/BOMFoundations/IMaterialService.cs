using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 材質服務介面
    /// </summary>
    public interface IMaterialService : IGenericManagementService<Material>
    {
        /// <summary>
        /// 檢查材質代碼是否已存在
        /// </summary>
        /// <param name="code">材質代碼</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據代碼取得材質資料
        /// </summary>
        /// <param name="code">材質代碼</param>
        /// <returns>材質資料</returns>
        Task<Material?> GetByCodeAsync(string code);

        /// <summary>
        /// 根據材質類別取得材質清單
        /// </summary>
        /// <param name="category">材質類別</param>
        /// <returns>材質清單</returns>
        Task<List<Material>> GetByCategoryAsync(string category);

        /// <summary>
        /// 取得所有材質類別
        /// </summary>
        /// <returns>材質類別清單</returns>
        Task<List<string>> GetCategoriesAsync();

        /// <summary>
        /// 根據供應商ID取得材質清單
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>材質清單</returns>
        Task<List<Material>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// 取得環保材質清單
        /// </summary>
        /// <returns>環保材質清單</returns>
        Task<List<Material>> GetEcoFriendlyMaterialsAsync();

        /// <summary>
        /// 根據密度範圍取得材質清單
        /// </summary>
        /// <param name="minDensity">最小密度</param>
        /// <param name="maxDensity">最大密度</param>
        /// <returns>材質清單</returns>
        Task<List<Material>> GetByDensityRangeAsync(decimal? minDensity = null, decimal? maxDensity = null);

        /// <summary>
        /// 根據熔點範圍取得材質清單
        /// </summary>
        /// <param name="minMeltingPoint">最小熔點</param>
        /// <param name="maxMeltingPoint">最大熔點</param>
        /// <returns>材質清單</returns>
        Task<List<Material>> GetByMeltingPointRangeAsync(decimal? minMeltingPoint = null, decimal? maxMeltingPoint = null);
    }
}