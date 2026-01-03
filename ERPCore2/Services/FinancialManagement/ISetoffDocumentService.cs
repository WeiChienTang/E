using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款單服務介面
    /// </summary>
    public interface ISetoffDocumentService : IGenericManagementService<SetoffDocument>
    {
        /// <summary>
        /// 檢查沖款單號是否已存在
        /// </summary>
        /// <param name="setoffNumber">沖款單號</param>
        /// <param name="excludeId">要排除的ID（更新時使用）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null);

        /// <summary>
        /// 檢查沖款單編號是否已存在（別名方法，供 EntityCodeGenerationHelper 使用）
        /// </summary>
        /// <param name="code">沖款單編號</param>
        /// <param name="excludeId">要排除的ID（更新時使用）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSetoffDocumentCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據沖款類型取得沖款單列表
        /// </summary>
        /// <param name="setoffType">沖款類型</param>
        /// <returns>沖款單列表</returns>
        Task<List<SetoffDocument>> GetBySetoffTypeAsync(SetoffType setoffType);

        /// <summary>
        /// 根據關聯方ID取得沖款單列表
        /// </summary>
        /// <param name="relatedPartyId">關聯方ID</param>
        /// <param name="setoffType">沖款類型（可選）</param>
        /// <returns>沖款單列表</returns>
        Task<List<SetoffDocument>> GetByRelatedPartyIdAsync(int relatedPartyId, SetoffType? setoffType = null);

        /// <summary>
        /// 根據公司ID取得沖款單列表
        /// </summary>
        /// <param name="companyId">公司ID</param>
        /// <returns>沖款單列表</returns>
        Task<List<SetoffDocument>> GetByCompanyIdAsync(int companyId);

        /// <summary>
        /// 根據日期區間取得沖款單列表
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>沖款單列表</returns>
        Task<List<SetoffDocument>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 重建所有來源明細的快取金額（修復工具）
        /// </summary>
        /// <param name="sourceDetailType">來源明細類型（null 表示全部）</param>
        /// <returns>重建結果</returns>
        Task<ServiceResult> RebuildCacheAsync(SetoffDetailType? sourceDetailType = null);
    }
}
