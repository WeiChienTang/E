using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 倉庫服務介面
    /// </summary>
    public interface IWarehouseService : IGenericManagementService<Warehouse>
    {
        /// <summary>
        /// 檢查倉庫代碼是否存在
        /// </summary>
        Task<bool> IsWarehouseCodeExistsAsync(string warehouseCode, int? excludeId = null);
        
        /// <summary>
        /// 根據倉庫類型取得倉庫
        /// </summary>
        Task<List<Warehouse>> GetByWarehouseTypeAsync(Data.Enums.WarehouseTypeEnum warehouseType);
        
        /// <summary>
        /// 取得預設倉庫
        /// </summary>
        Task<Warehouse?> GetDefaultWarehouseAsync();
        
        /// <summary>
        /// 設定預設倉庫
        /// </summary>
        Task<ServiceResult> SetDefaultWarehouseAsync(int warehouseId);
        
        /// <summary>
        /// 取得倉庫及其庫位
        /// </summary>
        Task<Warehouse?> GetWarehouseWithLocationsAsync(int warehouseId);
    }
}
