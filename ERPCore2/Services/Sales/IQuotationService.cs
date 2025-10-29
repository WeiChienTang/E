using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單服務介面
    /// </summary>
    public interface IQuotationService : IGenericManagementService<Quotation>
    {
        /// <summary>
        /// 檢查報價單號是否已存在
        /// </summary>
        /// <param name="quotationNumber">報價單號</param>
        /// <param name="excludeId">排除的ID（用於編輯時排除自己）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsQuotationNumberExistsAsync(string quotationNumber, int? excludeId = null);

        /// <summary>
        /// 根據客戶ID取得報價單列表
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>報價單列表</returns>
        Task<List<Quotation>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據日期範圍取得報價單列表
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>報價單列表</returns>
        Task<List<Quotation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得報價單（含完整明細）
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <returns>報價單（含明細）</returns>
        Task<Quotation?> GetWithDetailsAsync(int quotationId);

        /// <summary>
        /// 計算報價單總金額
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <returns>計算結果</returns>
        Task<ServiceResult> CalculateTotalAmountAsync(int quotationId);

        /// <summary>
        /// 取得未轉單的報價單列表
        /// </summary>
        /// <returns>未轉單的報價單列表</returns>
        Task<List<Quotation>> GetUnconvertedQuotationsAsync();

        /// <summary>
        /// 取得已核准且未轉單的報價單列表
        /// </summary>
        /// <returns>已核准且未轉單的報價單列表</returns>
        Task<List<Quotation>> GetApprovedUnconvertedQuotationsAsync();

        /// <summary>
        /// 根據批次列印條件查詢報價單
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的報價單列表</returns>
        Task<List<Quotation>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
    }
}
