using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動服務接口（主/明細結構）
    /// </summary>
    public interface IInventoryTransactionService : IGenericManagementService<InventoryTransaction>
    {
        #region 基本查詢

        /// <summary>
        /// 根據商品ID查詢異動記錄（透過明細查詢）
        /// </summary>
        Task<List<InventoryTransaction>> GetByProductIdAsync(int productId);
        
        Task<List<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<InventoryTransaction>> GetByTransactionNumberAsync(string transactionNumber);
        Task<List<InventoryTransaction>> GetByTypeAsync(InventoryTransactionTypeEnum transactionType);
        Task<List<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// 根據商品和日期範圍查詢異動記錄（透過明細查詢）
        /// </summary>
        Task<List<InventoryTransaction>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);
        Task<List<InventoryTransaction>> GetByWarehouseAndDateRangeAsync(int warehouseId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// 根據ID取得完整異動記錄（包含明細）
        /// </summary>
        Task<InventoryTransaction?> GetByIdWithDetailsAsync(int id);
        
        /// <summary>
        /// 根據來源單據查詢異動記錄
        /// </summary>
        Task<List<InventoryTransaction>> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId);

        #endregion

        #region 統計查詢

        /// <summary>
        /// 取得商品總入庫量（透過明細彙總）
        /// </summary>
        Task<decimal> GetTotalInboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// 取得商品總出庫量（透過明細彙總）
        /// </summary>
        Task<decimal> GetTotalOutboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        
        Task<Dictionary<InventoryTransactionTypeEnum, int>> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region 庫存異動記錄（已過時，請使用 IInventoryStockService）

        [Obsolete("請使用 IInventoryStockService.AddStockAsync")]
        Task<ServiceResult> CreateInboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            decimal? unitCost = null, int? locationId = null, string? remarks = null, int? employeeId = null);

        [Obsolete("請使用 IInventoryStockService.ReduceStockAsync")]
        Task<ServiceResult> CreateOutboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null);

        [Obsolete("請使用 IInventoryStockService.AdjustStockAsync")]
        Task<ServiceResult> CreateAdjustmentTransactionAsync(int productId, int warehouseId, 
            decimal originalQuantity, decimal adjustedQuantity, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null);

        [Obsolete("請使用 IInventoryStockService.TransferStockAsync")]
        Task<ServiceResult> CreateTransferTransactionAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null, int? employeeId = null);

        #endregion

        #region 庫存流水追蹤

        /// <summary>
        /// 取得商品的異動歷史（主檔層級）
        /// </summary>
        Task<List<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, int? warehouseId = null);
        
        /// <summary>
        /// 取得商品的異動歷史明細
        /// </summary>
        Task<List<InventoryTransactionDetail>> GetProductMovementHistoryDetailsAsync(int productId, int? warehouseId = null);

        /// <summary>
        /// 取得關聯的庫存異動記錄（原始交易 + 調整記錄）
        /// 用於顯示一張單據相關的所有庫存異動
        /// </summary>
        /// <param name="baseTransactionNumber">基礎交易編號（不含 _ADJ、_DEL 等後綴）</param>
        /// <returns>包含原始交易和所有調整記錄的 RelatedDocument 列表</returns>
        Task<List<ERPCore2.Models.RelatedDocument>> GetRelatedTransactionsAsync(string baseTransactionNumber);

        [Obsolete("沖銷功能需重新設計以支援主/明細結構")]
        Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reason, int? employeeId = null);

        #endregion

        #region 驗證方法

        Task<ServiceResult> ValidateTransactionAsync(InventoryTransaction transaction);
        Task<bool> IsTransactionNumberUniqueAsync(string transactionNumber, int? excludeId = null);

        #endregion
    }
}

