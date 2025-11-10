using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 產品合成服務介面 - 管理 BOM 主檔
    /// </summary>
    public interface IProductCompositionService : IGenericManagementService<ProductComposition>
    {
        /// <summary>
        /// 檢查產品合成代碼是否已存在
        /// </summary>
        /// <param name="code">產品合成代碼</param>
        /// <param name="excludeId">排除的產品合成ID</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsProductCompositionCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 取得指定產品的所有配方
        /// </summary>
        /// <param name="productId">產品 ID</param>
        /// <returns>配方列表</returns>
        Task<List<ProductComposition>> GetCompositionsByProductIdAsync(int productId);

        /// <summary>
        /// 計算配方總成本
        /// </summary>
        /// <param name="compositionId">配方 ID</param>
        /// <returns>總成本</returns>
        Task<decimal> CalculateTotalCostAsync(int compositionId);

        /// <summary>
        /// 取得 BOM 樹狀結構（包含多層展開）
        /// </summary>
        /// <param name="compositionId">配方 ID</param>
        /// <param name="maxLevel">最大展開層級（預設 10）</param>
        /// <returns>BOM 樹狀結構資料</returns>
        Task<object> GetBomTreeAsync(int compositionId, int maxLevel = 10);
    }
}
