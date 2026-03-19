using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項合成明細服務介面 - 管理 BOM 明細
    /// </summary>
    public interface IItemCompositionDetailService : IGenericManagementService<ItemCompositionDetail>
    {
        /// <summary>
        /// 取得指定配方的所有明細
        /// </summary>
        /// <param name="compositionId">配方 ID</param>
        /// <returns>明細列表</returns>
        Task<List<ItemCompositionDetail>> GetDetailsByCompositionIdAsync(int compositionId);

        /// <summary>
        /// 檢查組件是否已存在於配方中
        /// </summary>
        /// <param name="compositionId">配方 ID</param>
        /// <param name="componentItemId">組件品項 ID</param>
        /// <param name="excludeId">排除的明細 ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsComponentExistsInCompositionAsync(int compositionId, int componentItemId, int? excludeId = null);

        /// <summary>
        /// 計算明細的實際用料
        /// </summary>
        /// <param name="detailId">明細 ID</param>
        /// <param name="productionQuantity">生產數量</param>
        /// <returns>實際用料數量</returns>
        Task<decimal> CalculateActualQuantityAsync(int detailId, decimal productionQuantity);

        /// <summary>
        /// 取得使用指定組件的所有配方
        /// </summary>
        /// <param name="componentItemId">組件品項 ID</param>
        /// <returns>配方列表</returns>
        Task<List<ItemComposition>> GetCompositionsUsingComponentAsync(int componentItemId);
    }
}
