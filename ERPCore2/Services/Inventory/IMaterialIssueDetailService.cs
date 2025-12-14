using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨明細服務介面
    /// </summary>
    public interface IMaterialIssueDetailService : IGenericManagementService<MaterialIssueDetail>
    {
        /// <summary>
        /// 根據領貨主檔ID取得所有明細
        /// </summary>
        /// <param name="materialIssueId">領貨主檔ID</param>
        /// <returns>領貨明細清單</returns>
        Task<List<MaterialIssueDetail>> GetByMaterialIssueIdAsync(int materialIssueId);

        /// <summary>
        /// 根據商品ID取得所有領貨明細
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>領貨明細清單</returns>
        Task<List<MaterialIssueDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據倉庫ID取得所有領貨明細
        /// </summary>
        /// <param name="warehouseId">倉庫ID</param>
        /// <returns>領貨明細清單</returns>
        Task<List<MaterialIssueDetail>> GetByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// 根據庫位ID取得所有領貨明細
        /// </summary>
        /// <param name="warehouseLocationId">庫位ID</param>
        /// <returns>領貨明細清單</returns>
        Task<List<MaterialIssueDetail>> GetByWarehouseLocationIdAsync(int warehouseLocationId);

        /// <summary>
        /// 批次更新領貨明細
        /// </summary>
        /// <param name="materialIssueId">領貨主檔ID</param>
        /// <param name="details">明細清單</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int materialIssueId, List<MaterialIssueDetail> details);

        /// <summary>
        /// 批次更新領貨明細（支援外部 context 和 transaction）
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="materialIssueId">領貨主檔ID</param>
        /// <param name="details">明細清單</param>
        /// <param name="externalTransaction">外部交易</param>
        /// <returns>服務結果</returns>
        Task<ServiceResult> UpdateDetailsInContextAsync(
            Data.Context.AppDbContext context,
            int materialIssueId, 
            List<MaterialIssueDetail> details,
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? externalTransaction = null);

        /// <summary>
        /// 檢查商品在指定領貨單中是否已存在
        /// </summary>
        /// <param name="materialIssueId">領貨主檔ID</param>
        /// <param name="productId">商品ID</param>
        /// <param name="warehouseId">倉庫ID</param>
        /// <param name="warehouseLocationId">庫位ID（可選）</param>
        /// <param name="excludeId">排除的明細ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsProductExistsInIssueAsync(int materialIssueId, int productId, int warehouseId, int? warehouseLocationId = null, int? excludeId = null);

        /// <summary>
        /// 驗證領貨明細的庫存是否充足
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="warehouseId">倉庫ID</param>
        /// <param name="warehouseLocationId">庫位ID（可選）</param>
        /// <param name="issueQuantity">領貨數量</param>
        /// <returns>驗證結果（是否充足，可用數量）</returns>
        Task<(bool IsValid, int AvailableQuantity, string ErrorMessage)> ValidateStockAvailabilityAsync(
            int productId, 
            int warehouseId, 
            int? warehouseLocationId, 
            int issueQuantity);

        /// <summary>
        /// 取得領貨明細的統計資料
        /// </summary>
        /// <param name="materialIssueId">領貨主檔ID</param>
        /// <returns>統計資料（總數量、總成本）</returns>
        Task<(int TotalQuantity, decimal TotalCost)> GetStatisticsAsync(int materialIssueId);

        /// <summary>
        /// 計算領貨明細的總成本
        /// </summary>
        /// <param name="issueQuantity">領貨數量</param>
        /// <param name="unitCost">單位成本</param>
        /// <returns>總成本</returns>
        decimal CalculateTotalCost(int issueQuantity, decimal? unitCost);
    }
}
