using ERPCore2.Data.Entities;
using ERPCore2.Services.Interfaces;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 廠商地址服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierAddressService : IGenericManagementService<SupplierAddress>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據廠商ID取得地址列表
        /// </summary>
        Task<List<SupplierAddress>> GetBySupplierIdAsync(int supplierId);
        
        /// <summary>
        /// 根據地址類型取得地址列表
        /// </summary>
        Task<List<SupplierAddress>> GetByAddressTypeAsync(int addressTypeId);
        
        /// <summary>
        /// 取得廠商的主要地址
        /// </summary>
        Task<SupplierAddress?> GetPrimaryAddressAsync(int supplierId);
        
        /// <summary>
        /// 取得廠商的特定類型地址
        /// </summary>
        Task<SupplierAddress?> GetAddressByTypeAsync(int supplierId, int addressTypeId);
        
        #endregion

        #region 業務邏輯操作
        
        /// <summary>
        /// 設定主要地址
        /// </summary>
        Task<ServiceResult> SetPrimaryAddressAsync(int addressId);
        
        /// <summary>
        /// 複製地址到其他廠商
        /// </summary>
        Task<ServiceResult<SupplierAddress>> CopyAddressToSupplierAsync(SupplierAddress sourceAddress, int targetSupplierId, int? targetAddressTypeId = null);
        
        /// <summary>
        /// 確保廠商有主要地址
        /// </summary>
        Task<ServiceResult> EnsureSupplierHasPrimaryAddressAsync(int supplierId);
        
        /// <summary>
        /// 取得廠商地址及預設類型
        /// </summary>
        Task<List<SupplierAddress>> GetAddressesWithDefaultAsync(int supplierId, List<AddressType> addressTypes);
        
        /// <summary>
        /// 更新廠商地址
        /// </summary>
        Task<ServiceResult> UpdateSupplierAddressesAsync(int supplierId, List<SupplierAddress> addresses);
        
        #endregion

        #region 記憶體操作方法（用於UI編輯）
        
        /// <summary>
        /// 建立新的地址
        /// </summary>
        SupplierAddress CreateNewAddress(int supplierId, int addressCount);
        
        /// <summary>
        /// 初始化預設地址
        /// </summary>
        void InitializeDefaultAddresses(List<SupplierAddress> addressList, List<AddressType> addressTypes);
        
        /// <summary>
        /// 取得預設地址類型ID
        /// </summary>
        int? GetDefaultAddressTypeId(int addressCount, List<AddressType> addressTypes);
        
        /// <summary>
        /// 確保主要地址存在
        /// </summary>
        ServiceResult EnsurePrimaryAddressExists(List<SupplierAddress> addresses);
        
        /// <summary>
        /// 取得完整地址數量
        /// </summary>
        int GetCompletedAddressCount(List<SupplierAddress> addresses);
        
        /// <summary>
        /// 取得地址完成欄位數量
        /// </summary>
        int GetAddressCompletedFieldsCount(List<SupplierAddress> addresses);
        
        #endregion
    }
}
