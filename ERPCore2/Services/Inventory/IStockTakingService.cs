using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存盤點服務接口
    /// </summary>
    public interface IStockTakingService : IGenericManagementService<StockTaking>
    {
        // 基本查詢
        Task<List<StockTaking>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<StockTaking>> GetByStatusAsync(StockTakingStatusEnum status);
        Task<List<StockTaking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<StockTaking?> GetByTakingNumberAsync(string takingNumber);
        Task<StockTaking?> GetWithDetailsAsync(int id);

        // 盤點管理
        Task<ServiceResult> CreateStockTakingAsync(StockTaking stockTaking);
        Task<ServiceResult> GenerateStockTakingListAsync(int warehouseId, int? warehouseLocationId = null, 
            StockTakingTypeEnum takingType = StockTakingTypeEnum.Full, List<int>? specificProductIds = null);
        Task<ServiceResult> StartStockTakingAsync(int stockTakingId);
        Task<ServiceResult> CompleteStockTakingAsync(int stockTakingId);
        Task<ServiceResult> ApproveStockTakingAsync(int stockTakingId, int approvedBy, string? remarks = null);
        Task<ServiceResult> CancelStockTakingAsync(int stockTakingId, string? reason = null);

        // 盤點明細管理
        Task<List<StockTakingDetail>> GetStockTakingDetailsAsync(int stockTakingId);
        Task<ServiceResult> UpdateStockTakingDetailAsync(int detailId, decimal actualStock, string? personnel = null, string? remarks = null);
        Task<ServiceResult> BatchUpdateStockTakingDetailsAsync(List<StockTakingDetailUpdateModel> updates);
        
        /// <summary>
        /// 儲存盤點單連同明細（新增或更新模式都適用）
        /// </summary>
        /// <param name="stockTaking">盤點主檔</param>
        /// <param name="details">盤點明細清單</param>
        /// <returns>儲存結果</returns>
        Task<ServiceResult<StockTaking>> SaveWithDetailsAsync(StockTaking stockTaking, List<StockTakingDetail> details);

        // 差異處理
        Task<List<StockTakingDetail>> GetDifferenceItemsAsync(int stockTakingId);
        Task<ServiceResult> GenerateAdjustmentTransactionsAsync(int stockTakingId);
        Task<StockTakingReportModel> GenerateStockTakingReportAsync(int stockTakingId);

        // 統計查詢
        Task<int> GetPendingStockTakingsCountAsync();
        Task<List<StockTakingStatisticsModel>> GetStockTakingStatisticsAsync(DateTime startDate, DateTime endDate);

        // 驗證
        Task<ServiceResult> ValidateStockTakingAsync(StockTaking stockTaking);
        Task<bool> IsStockTakingNumberUniqueAsync(string takingNumber, int? excludeId = null);
    }

    /// <summary>
    /// 盤點明細更新模型
    /// </summary>
    public class StockTakingDetailUpdateModel
    {
        public int DetailId { get; set; }
        public decimal ActualStock { get; set; }
        public string? Personnel { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 盤點報告模型
    /// </summary>
    public class StockTakingReportModel
    {
        public StockTaking StockTaking { get; set; } = null!;
        public List<StockTakingDetail> AllDetails { get; set; } = new();
        public List<StockTakingDetail> DifferenceItems { get; set; } = new();
        public decimal TotalDifferenceAmount { get; set; }
        public int TotalCountedItems { get; set; }
        public int TotalDifferenceItems { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AccuracyRate { get; set; }
    }

    /// <summary>
    /// 盤點統計模型
    /// </summary>
    public class StockTakingStatisticsModel
    {
        public DateTime Date { get; set; }
        public int TotalStockTakings { get; set; }
        public int CompletedStockTakings { get; set; }
        public int PendingStockTakings { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public decimal AverageAccuracyRate { get; set; }
        public decimal TotalDifferenceAmount { get; set; }
    }
}
