using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 倉庫服務介面
    /// </summary>
    public interface IWarehouseService : IGenericManagementService<Warehouse>
    {
        /// <summary>
        /// 檢查倉庫編號是否存在
        /// </summary>
        Task<bool> IsWarehouseCodeExistsAsync(string warehouseCode, int? excludeId = null);
        
        /// <summary>
        /// 檢查倉庫名稱是否存在
        /// </summary>
        Task<bool> IsWarehouseNameExistsAsync(string warehouseName, int? excludeId = null);
        
        /// <summary>
        /// 取得倉庫及其庫位
        /// </summary>
        Task<Warehouse?> GetWarehouseWithLocationsAsync(int warehouseId);
    }
}
