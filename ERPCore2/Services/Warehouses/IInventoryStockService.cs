using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存管理服務接口
    /// </summary>
    public interface IInventoryStockService : IGenericManagementService<InventoryStock>
    {
        // 基本查詢
        Task<List<InventoryStock>> GetByProductIdAsync(int productId);
        Task<List<InventoryStock>> GetByWarehouseIdAsync(int warehouseId);
        Task<InventoryStock?> GetByProductWarehouseAsync(int productId, int warehouseId, int? locationId = null);
        Task<InventoryStock?> GetByProductWarehouseAsync(int productId, int? warehouseId = null, int? locationId = null);
        Task<List<InventoryStock>> GetLowStockItemsAsync();
        Task<decimal> GetAvailableStockAsync(int productId, int warehouseId, int? locationId = null);
        Task<InventoryStockDetail?> GetStockDetailAsync(int productId, int warehouseId, int? locationId = null);
        Task<decimal> GetTotalAvailableStockByWarehouseAsync(int productId, int warehouseId);
        
        // 庫存總覽查詢
        Task<List<InventoryStock>> GetInventoryOverviewAsync(int? warehouseId = null, int? categoryId = null, int? locationId = null);
        Task<List<InventoryStock>> GetLowStockOverviewAsync();
        Task<Dictionary<string, object>> GetInventoryStatisticsAsync();
        
        // 庫存異動
        Task<ServiceResult> RevertStockToOriginalAsync(int inventoryStockDetailId, decimal quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, string? remarks = null);
            
        Task<ServiceResult> AddStockAsync(int productId, int warehouseId, decimal quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, 
            decimal? unitCost = null, int? locationId = null, string? remarks = null,
            string? batchNumber = null, DateTime? batchDate = null, DateTime? expiryDate = null,
            string? sourceDocumentType = null, int? sourceDocumentId = null, int? sourceDetailId = null);
        
        Task<ServiceResult> ReduceStockAsync(int productId, int warehouseId, decimal quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null,
            string? sourceDocumentType = null, int? sourceDocumentId = null, int? sourceDetailId = null);
        
        Task<ServiceResult> TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId,
            decimal quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null);
        
        Task<ServiceResult> AdjustStockAsync(int productId, int warehouseId, decimal newQuantity,
            string transactionNumber, string? remarks = null, int? locationId = null,
            string? sourceDocumentType = null, int? sourceDocumentId = null);
        
        // 庫存預留
        Task<ServiceResult> ReserveStockAsync(int productId, int warehouseId, decimal quantity,
            InventoryReservationType reservationType, string referenceNumber,
            DateTime? expiryDate = null, int? locationId = null, string? remarks = null);
        
        Task<ServiceResult> ReleaseReservationAsync(int reservationId, decimal? releaseQuantity = null);
        Task<ServiceResult> CancelReservationAsync(int reservationId);
        Task<List<InventoryReservation>> GetActiveReservationsAsync(int productId, int warehouseId);
        
        // 庫存驗證
        Task<bool> IsStockAvailableAsync(int productId, int warehouseId, decimal requiredQuantity, int? locationId = null);
        Task<ServiceResult> ValidateStockOperationAsync(int productId, int warehouseId, decimal quantity, bool isReduce);
        
        // 庫存交易查詢
        Task<List<InventoryTransaction>> GetInventoryTransactionsBySalesOrderAsync(int salesOrderId);
        
        // FIFO 庫存扣減
        Task<ServiceResult> ReduceStockWithFIFOAsync(int productId, int warehouseId, decimal quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null, int? salesOrderDetailId = null);
        
        // 警戒線管理
        Task<List<InventoryStockDetail>> GetStockDetailsWithoutAlertAsync();
        Task<ServiceResult> BatchUpdateStockLevelAlertsAsync(List<(int detailId, decimal? minLevel, decimal? maxLevel)> updates);
        Task<List<InventoryStockDetail>> GetLowStockDetailsAsync();
        Task<List<InventoryStockDetail>> GetOverStockDetailsAsync();
        
        // 取得商品的可用倉庫位置清單
        Task<List<InventoryStockDetail>> GetAvailableWarehouseLocationsByProductAsync(int productId);
    }
}

