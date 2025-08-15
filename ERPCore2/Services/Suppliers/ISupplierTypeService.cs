using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商類型服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierTypeService : IGenericManagementService<SupplierType>
    {
        #region 業務特定方法
        
        /// <summary>
        /// 檢查廠商類型名稱是否存在
        /// </summary>
        Task<bool> IsSupplierTypeNameExistsAsync(string typeName, int? excludeId = null);
        
        /// <summary>
        /// 檢查廠商類型代碼是否存在
        /// </summary>
        Task<bool> IsSupplierTypeCodeExistsAsync(string typeCode, int? excludeId = null);
        
        /// <summary>
        /// 根據類型名稱取得廠商類型
        /// </summary>
        Task<SupplierType?> GetByTypeNameAsync(string typeName);
        
        /// <summary>
        /// 檢查是否可以刪除廠商類型（檢查是否有廠商使用此類型）
        /// </summary>
        Task<bool> CanDeleteSupplierTypeAsync(int supplierTypeId);
        
        #endregion
    }
}

