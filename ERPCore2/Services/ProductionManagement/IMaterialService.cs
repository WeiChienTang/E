using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 材質服務介面
    /// </summary>
    public interface IMaterialService : IGenericManagementService<Material>
    {
        /// <summary>
        /// 檢查材質編號是否已存在
        /// </summary>
        /// <param name="code">材質編號</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據編號取得材質資料
        /// </summary>
        /// <param name="code">材質編號</param>
        /// <returns>材質資料</returns>
        Task<Material?> GetByCodeAsync(string code);

        #region 伺服器端分頁

        /// <summary>
        /// 取得分頁資料（支援動態篩選）
        /// </summary>
        Task<(List<Material> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Material>, IQueryable<Material>>? filterFunc,
            int pageNumber,
            int pageSize);

        #endregion
    }
}
