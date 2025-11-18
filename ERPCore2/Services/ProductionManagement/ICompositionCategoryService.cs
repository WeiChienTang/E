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
    }
}
