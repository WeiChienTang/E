using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 供應商推薦服務介面
    /// 用於低庫存警戒時推薦合適的供應商
    /// </summary>
    public interface ISupplierRecommendationService
    {
        /// <summary>
        /// 取得商品的供應商推薦清單（混合綁定資料與歷史資料）
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>供應商推薦列表</returns>
        Task<List<SupplierRecommendation>> GetRecommendedSuppliersAsync(int productId);
        
        /// <summary>
        /// 取得指定商品和供應商的最近採購資訊
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="productId">商品ID</param>
        /// <returns>最近採購資訊（價格和日期）</returns>
        Task<(decimal Price, DateTime PurchaseDate)?> GetLastPurchasePriceAsync(int supplierId, int productId);
    }
}
