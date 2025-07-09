using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 單位服務介面
    /// </summary>
    public interface IUnitService : IGenericManagementService<Unit>
    {
        /// <summary>
        /// 檢查單位代碼是否存在
        /// </summary>
        Task<bool> IsUnitCodeExistsAsync(string unitCode, int? excludeId = null);
        
        /// <summary>
        /// 取得基本單位
        /// </summary>
        Task<List<Unit>> GetBaseUnitsAsync();
        
        /// <summary>
        /// 取得單位及其轉換關係
        /// </summary>
        Task<Unit?> GetUnitWithConversionsAsync(int unitId);
        
        /// <summary>
        /// 計算單位轉換
        /// </summary>
        Task<decimal?> ConvertUnitsAsync(int fromUnitId, int toUnitId, decimal quantity);
        
        /// <summary>
        /// 取得可轉換的單位列表
        /// </summary>
        Task<List<Unit>> GetConvertibleUnitsAsync(int unitId);
    }
}
