using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存預留服務接口
    /// </summary>
    public interface IInventoryReservationService : IGenericManagementService<InventoryReservation>
    {
        // 基本查詢
        Task<List<InventoryReservation>> GetByProductIdAsync(int productId);
        Task<List<InventoryReservation>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<InventoryReservation>> GetByReferenceNumberAsync(string referenceNumber);
        Task<List<InventoryReservation>> GetByTypeAsync(InventoryReservationType reservationType);
        Task<List<InventoryReservation>> GetActiveReservationsAsync();
        Task<List<InventoryReservation>> GetExpiredReservationsAsync();
        
        // 特定查詢
        Task<List<InventoryReservation>> GetActiveByProductAsync(int productId, int? warehouseId = null);
        Task<List<InventoryReservation>> GetActiveByWarehouseAsync(int warehouseId);
        Task<InventoryReservation?> GetByIdWithDetailsAsync(int id);
        Task<List<InventoryReservation>> GetExpiringReservationsAsync(DateTime beforeDate);
        
        // 統計查詢
        Task<decimal> GetTotalReservedQuantityAsync(int productId, int warehouseId, int? locationId = null);
        Task<decimal> GetAvailableQuantityForReservationAsync(int productId, int warehouseId, int? locationId = null);
        Task<Dictionary<InventoryReservationType, int>> GetReservationSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        // 預留操作
        Task<ServiceResult> CreateReservationAsync(int productId, int warehouseId, int quantity,
            InventoryReservationType reservationType, string referenceNumber,
            DateTime? expiryDate = null, int? locationId = null, string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> ReleaseReservationAsync(int reservationId, int? releaseQuantity = null, 
            string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> CancelReservationAsync(int reservationId, string? reason = null, int? employeeId = null);
        
        Task<ServiceResult> ExtendReservationAsync(int reservationId, DateTime newExpiryDate, 
            string? remarks = null, int? employeeId = null);
        
        Task<ServiceResult> PartialReleaseAsync(int reservationId, int releaseQuantity, 
            string? remarks = null, int? employeeId = null);
        
        // 批次操作
        Task<ServiceResult> ReleaseExpiredReservationsAsync(int? employeeId = null);
        Task<ServiceResult> ReleaseReservationsByReferenceAsync(string referenceNumber, 
            string? remarks = null, int? employeeId = null);
        
        // 驗證方法
        Task<ServiceResult> ValidateReservationAsync(InventoryReservation reservation);
        Task<bool> CanReserveQuantityAsync(int productId, int warehouseId, int quantity, int? locationId = null);
        Task<bool> IsReferenceNumberUniqueAsync(string referenceNumber, InventoryReservationType reservationType, int? excludeId = null);
        
        // 業務邏輯方法
        Task<ServiceResult> ConvertReservationToSaleAsync(int reservationId, string saleOrderNumber, 
            int? employeeId = null);
        Task<List<InventoryReservation>> GetReservationsForStockCheckAsync(int productId, int warehouseId);
    }
}

