using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 合成表類型服務介面
    /// </summary>
    public interface ICompositionCategoryService : IGenericManagementService<CompositionCategory>
    {
        /// <summary>
        /// 根據名稱模糊搜尋合成表類型
        /// </summary>
        Task<List<CompositionCategory>> SearchByNameAsync(string keyword);

        /// <summary>
        /// 檢查合成表類型代碼是否已存在
        /// </summary>
        Task<bool> IsCompositionCategoryCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 檢查合成表類型名稱是否已存在
        /// </summary>
        Task<bool> IsCompositionCategoryNameExistsAsync(string name, int? excludeId = null);
    }
}
