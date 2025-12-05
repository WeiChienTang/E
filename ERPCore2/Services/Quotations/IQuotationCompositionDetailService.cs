using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單組合明細服務介面 - 管理報價單專屬的 BOM 組成
    /// </summary>
    public interface IQuotationCompositionDetailService : IGenericManagementService<QuotationCompositionDetail>
    {
        /// <summary>
        /// 取得指定報價單明細的所有組合明細
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        /// <returns>組合明細列表</returns>
        Task<List<QuotationCompositionDetail>> GetByQuotationDetailIdAsync(int quotationDetailId);

        /// <summary>
        /// 從產品合成表複製 BOM 到報價單明細（使用最新的配方）
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        /// <param name="productId">商品 ID</param>
        /// <returns>複製的組合明細列表</returns>
        Task<List<QuotationCompositionDetail>> CopyFromProductCompositionAsync(int quotationDetailId, int productId);
        
        /// <summary>
        /// 從指定的產品配方複製 BOM 到報價單明細
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        /// <param name="compositionId">產品配方 ID</param>
        /// <returns>複製的組合明細列表</returns>
        Task<List<QuotationCompositionDetail>> CopyFromCompositionAsync(int quotationDetailId, int compositionId);

        /// <summary>
        /// 檢查組件是否已存在於報價單明細的組合中
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        /// <param name="componentProductId">組件產品 ID</param>
        /// <param name="excludeId">排除的組合明細 ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsComponentExistsAsync(int quotationDetailId, int componentProductId, int? excludeId = null);

        /// <summary>
        /// 批次儲存報價單組合明細
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        /// <param name="compositionDetails">組合明細列表</param>
        /// <returns>儲存的組合明細列表</returns>
        Task<List<QuotationCompositionDetail>> SaveBatchAsync(int quotationDetailId, List<QuotationCompositionDetail> compositionDetails);

        /// <summary>
        /// 刪除指定報價單明細的所有組合明細
        /// </summary>
        /// <param name="quotationDetailId">報價單明細 ID</param>
        Task DeleteByQuotationDetailIdAsync(int quotationDetailId);
    }
}
