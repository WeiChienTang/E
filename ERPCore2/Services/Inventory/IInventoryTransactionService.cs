using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動服務接口
    /// </summary>
    public interface IInventoryTransactionService : IGenericManagementService<InventoryTransaction>
    {
        // 基本查詢
        Task<List<InventoryTransaction>> GetByProductIdAsync(int productId);
        Task<List<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<InventoryTransaction>> GetByTransactionNumberAsync(string transactionNumber);
        Task<List<InventoryTransaction>> GetByTypeAsync(InventoryTransactionTypeEnum transactionType);
        Task<List<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // 特定查詢
        Task<List<InventoryTransaction>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);
        Task<List<InventoryTransaction>> GetByWarehouseAndDateRangeAsync(int warehouseId, DateTime startDate, DateTime endDate);
        Task<InventoryTransaction?> GetByIdWithDetailsAsync(int id);
        
        // 統計查詢
        Task<decimal> GetTotalInboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalOutboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<InventoryTransactionTypeEnum, int>> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate);
        
        // 庫存異動記錄
        Task<ServiceResult> CreateInboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            decimal? unitCost = null, int? locationId = null, string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> CreateOutboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> CreateAdjustmentTransactionAsync(int productId, int warehouseId, 
            int originalQuantity, int adjustedQuantity, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> CreateTransferTransactionAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null, int? employeeId = null);
        
        // 庫存流水追蹤
        Task<List<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, int? warehouseId = null);
        Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reason, int? employeeId = null);
        
        // 驗證方法
        Task<ServiceResult> ValidateTransactionAsync(InventoryTransaction transaction);
        Task<bool> IsTransactionNumberUniqueAsync(string transactionNumber, int? excludeId = null);
    }
}

