using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services.GovernmentAgencies
{
    /// <summary>
    /// 公家機關服務介面 - 繼承通用管理服務
    /// </summary>
    public interface IGovernmentAgencyService : IGenericManagementService<GovernmentAgency>
    {
        #region 業務特定查詢方法

        /// <summary>
        /// 根據機關編號取得公家機關
        /// </summary>
        Task<GovernmentAgency?> GetByAgencyCodeAsync(string code);

        /// <summary>
        /// 檢查機關編號是否存在
        /// </summary>
        Task<bool> IsGovernmentAgencyCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 檢查機關名稱是否存在
        /// </summary>
        Task<bool> IsAgencyNameExistsAsync(string agencyName, int? excludeId = null);

        #endregion

        #region 輔助方法

        /// <summary>
        /// 初始化新公家機關
        /// </summary>
        void InitializeNewAgency(GovernmentAgency agency);

        /// <summary>
        /// 取得基本必填欄位數量
        /// </summary>
        int GetBasicRequiredFieldsCount();

        /// <summary>
        /// 取得基本完成欄位數量
        /// </summary>
        int GetBasicCompletedFieldsCount(GovernmentAgency agency);

        #endregion

        #region 伺服器端分頁

        /// <summary>
        /// 取得分頁資料（支援動態篩選）
        /// </summary>
        Task<(List<GovernmentAgency> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<GovernmentAgency>, IQueryable<GovernmentAgency>>? filterFunc,
            int pageNumber,
            int pageSize);

        #endregion
    }
}
