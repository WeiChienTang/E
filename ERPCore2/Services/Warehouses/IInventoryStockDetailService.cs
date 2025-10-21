using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存明細管理服務接口
    /// </summary>
    public interface IInventoryStockDetailService : IGenericManagementService<InventoryStockDetail>
    {
        /// <summary>
        /// 根據庫存主檔ID取得所有明細
        /// </summary>
        Task<List<InventoryStockDetail>> GetByInventoryStockIdAsync(int inventoryStockId);
        
        /// <summary>
        /// 根據倉庫ID取得所有明細
        /// </summary>
        Task<List<InventoryStockDetail>> GetByWarehouseIdAsync(int warehouseId);
        
        /// <summary>
        /// 根據倉庫位置ID取得所有明細
        /// </summary>
        Task<List<InventoryStockDetail>> GetByWarehouseLocationIdAsync(int warehouseLocationId);
        
        /// <summary>
        /// 取得特定商品在特定倉庫位置的明細
        /// </summary>
        Task<InventoryStockDetail?> GetByInventoryWarehouseLocationAsync(int inventoryStockId, int warehouseId, int? warehouseLocationId = null);
        
        /// <summary>
        /// 檢查明細是否存在
        /// </summary>
        Task<bool> ExistsAsync(int inventoryStockId, int warehouseId, int? warehouseLocationId = null);
        
    }
}
