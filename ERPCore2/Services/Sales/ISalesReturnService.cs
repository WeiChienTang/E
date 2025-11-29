using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
using ERPCore2.Services;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨退回服務介面
    /// </summary>
    public interface ISalesReturnService : IGenericManagementService<SalesReturn>
    {
        /// <summary>
        /// 檢查銷貨退回代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        Task<bool> IsSalesReturnCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據客戶取得銷貨退回清單
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據銷貨出貨單取得銷貨退回清單
        /// </summary>
        /// <param name="salesDeliveryId">銷貨出貨單ID</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetBySalesDeliveryIdAsync(int salesDeliveryId);



        /// <summary>
        /// 根據日期範圍取得銷貨退回清單
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>銷貨退回清單</returns>
        Task<List<SalesReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 計算退回總金額
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>總金額</returns>
        Task<decimal> CalculateTotalReturnAmountAsync(int salesReturnId);

        /// <summary>
        /// 產生退回單號
        /// </summary>
        /// <param name="returnDate">退回日期</param>
        /// <returns>退回單號</returns>
        Task<string> GenerateSalesReturnNumberAsync(DateTime returnDate);

        /// <summary>
        /// 儲存銷貨退回連同明細
        /// </summary>
        /// <param name="salesReturn">銷貨退回主檔</param>
        /// <param name="details">銷貨退回明細清單</param>
        /// <returns>儲存結果</returns>
        Task<ServiceResult<SalesReturn>> SaveWithDetailsAsync(SalesReturn salesReturn, List<SalesReturnDetail> details);

        /// <summary>
        /// 更新銷貨退回明細
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <param name="details">銷貨退回明細清單</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(int salesReturnId, List<SalesReturnDetail> details);

        /// <summary>
        /// 取得統計資訊
        /// </summary>
        /// <param name="customerId">客戶ID（選填）</param>
        /// <param name="startDate">開始日期（選填）</param>
        /// <param name="endDate">結束日期（選填）</param>
        /// <returns>統計資訊</returns>
        Task<SalesReturnStatistics> GetStatisticsAsync(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// 根據批次列印條件查詢銷貨退回單（支援多條件組合篩選）
        /// 設計理念：靈活組合日期、客戶、狀態等多種篩選條件，適用於批次列印場景
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的銷貨退回單列表（包含完整關聯資料）</returns>
        Task<List<SalesReturn>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
    }

    /// <summary>
    /// 銷貨退回統計資訊
    /// </summary>
    public class SalesReturnStatistics
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public Dictionary<EntitySalesReturnReason, int> ReturnReasonCounts { get; set; } = new();
    }
}
