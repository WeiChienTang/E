using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單明細服務介面
    /// </summary>
    public interface IQuotationDetailService : IGenericManagementService<QuotationDetail>
    {
        /// <summary>
        /// 根據報價單ID取得所有明細
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <returns>報價單明細列表</returns>
        Task<List<QuotationDetail>> GetByQuotationIdAsync(int quotationId);

        /// <summary>
        /// 根據報價單ID刪除所有明細
        /// </summary>
        /// <param name="quotationId">報價單ID</param>
        /// <returns>刪除結果</returns>
        Task<ServiceResult> DeleteByQuotationIdAsync(int quotationId);

        /// <summary>
        /// 根據產品ID取得報價單明細列表
        /// </summary>
        /// <param name="productId">產品ID</param>
        /// <returns>報價單明細列表</returns>
        Task<List<QuotationDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 獲取客戶最近一次完整的報價單明細（智能下單用）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>報價單明細列表</returns>
        Task<List<QuotationDetail>> GetLastCompleteQuotationAsync(int customerId);
    }
}
