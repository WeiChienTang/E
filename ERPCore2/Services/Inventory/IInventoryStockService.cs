using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services.Inventory
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
        Task<List<InventoryStock>> GetLowStockItemsAsync();
        Task<int> GetAvailableStockAsync(int productId, int warehouseId, int? locationId = null);
        
        // 庫存異動
        Task<ServiceResult> AddStockAsync(int productId, int warehouseId, int quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, 
            decimal? unitCost = null, int? locationId = null, string? remarks = null);
        
        Task<ServiceResult> ReduceStockAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null);
        
        Task<ServiceResult> TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null);
        
        Task<ServiceResult> AdjustStockAsync(int productId, int warehouseId, int newQuantity,
            string transactionNumber, string? remarks = null, int? locationId = null);
        
        // 庫存預留
        Task<ServiceResult> ReserveStockAsync(int productId, int warehouseId, int quantity,
            InventoryReservationType reservationType, string referenceNumber,
            DateTime? expiryDate = null, int? locationId = null, string? remarks = null);
        
        Task<ServiceResult> ReleaseReservationAsync(int reservationId, int? releaseQuantity = null);
        Task<ServiceResult> CancelReservationAsync(int reservationId);
        Task<List<InventoryReservation>> GetActiveReservationsAsync(int productId, int warehouseId);
        
        // 庫存驗證
        Task<bool> IsStockAvailableAsync(int productId, int warehouseId, int requiredQuantity, int? locationId = null);
        Task<ServiceResult> ValidateStockOperationAsync(int productId, int warehouseId, int quantity, bool isReduce);
    }
}
