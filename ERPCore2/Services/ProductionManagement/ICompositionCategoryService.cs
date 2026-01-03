using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 物料清單類型服務介面
    /// </summary>
    public interface ICompositionCategoryService : IGenericManagementService<CompositionCategory>
    {
        /// <summary>
        /// 根據名稱模糊搜尋物料清單類型
        /// </summary>
        Task<List<CompositionCategory>> SearchByNameAsync(string keyword);

        /// <summary>
        /// 檢查物料清單類型編號是否已存在
        /// </summary>
        Task<bool> IsCompositionCategoryCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 檢查物料清單類型名稱是否已存在
        /// </summary>
        Task<bool> IsCompositionCategoryNameExistsAsync(string name, int? excludeId = null);
    }
}
