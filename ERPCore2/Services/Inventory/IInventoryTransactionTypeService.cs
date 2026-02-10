using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動類型服務介面
    /// </summary>
    public interface IInventoryTransactionTypeService : IGenericManagementService<InventoryTransactionType>
    {
        /// <summary>
        /// 檢查類型編號是否存在
        /// </summary>
        Task<bool> IsTypeCodeExistsAsync(string typeCode, int? excludeId = null);
        
        /// <summary>
        /// 根據異動類型取得類型設定
        /// </summary>
        Task<List<InventoryTransactionType>> GetByTransactionTypeAsync(InventoryTransactionTypeEnum transactionType);
        
        /// <summary>
        /// 取得需要審核的異動類型
        /// </summary>
        Task<List<InventoryTransactionType>> GetRequiresApprovalTypesAsync();
        
        /// <summary>
        /// 取得影響成本的異動類型
        /// </summary>
        Task<List<InventoryTransactionType>> GetAffectsCostTypesAsync();
        
        /// <summary>
        /// 產生下一個異動單號
        /// </summary>
        Task<string> GenerateNextNumberAsync(int typeId);
    }
}
