using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶地址管理服務介面 - 統一處理所有客戶地址相關操作
    /// </summary>
    public interface ICustomerAddressService : IGenericManagementService<CustomerAddress>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據客戶ID取得地址清單
        /// </summary>
        Task<List<CustomerAddress>> GetByCustomerIdAsync(int customerId);
        
        /// <summary>
        /// 取得客戶的主要地址
        /// </summary>
        Task<CustomerAddress?> GetPrimaryAddressAsync(int customerId);
        
        /// <summary>
        /// 根據地址類型取得地址清單
        /// </summary>
        Task<List<CustomerAddress>> GetByAddressTypeAsync(int addressTypeId);
        
        #endregion

        #region 業務邏輯操作
          /// <summary>
        /// 設定主要地址
        /// </summary>
        Task<ServiceResult> SetPrimaryAddressAsync(int addressId);
        
        /// <summary>
        /// 複製地址到其他客戶
        /// </summary>
        Task<ServiceResult<CustomerAddress>> CopyAddressToCustomerAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null);
        
        /// <summary>
        /// 確保客戶至少有一個主要地址
        /// </summary>
        Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId);
        
        /// <summary>
        /// 取得客戶地址清單並初始化預設地址（如果需要）
        /// </summary>
        Task<List<CustomerAddress>> GetAddressesWithDefaultAsync(int customerId, List<AddressType> addressTypes);
        
        /// <summary>
        /// 更新客戶的所有地址資料
        /// </summary>
        Task<ServiceResult> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses);
        
        #endregion

        #region 記憶體操作方法（用於UI編輯）

        /// <summary>
        /// 建立新地址物件（記憶體操作）
        /// </summary>
        CustomerAddress CreateNewAddress(int customerId, int addressCount);
        
        /// <summary>
        /// 初始化預設地址清單（記憶體操作）
        /// </summary>
        void InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes);
        
        /// <summary>
        /// 取得預設地址類型ID（記憶體操作）
        /// </summary>
        int? GetDefaultAddressTypeId(int addressCount, List<AddressType> addressTypes);
        
        /// <summary>
        /// 新增地址到清單（記憶體操作）
        /// </summary>
        ServiceResult AddAddress(List<CustomerAddress> addressList, CustomerAddress newAddress);
        
        /// <summary>
        /// 從清單移除地址（記憶體操作）
        /// </summary>
        ServiceResult RemoveAddress(List<CustomerAddress> addressList, int index);
        
        /// <summary>
        /// 設定主要地址（記憶體操作）
        /// </summary>
        ServiceResult SetPrimaryAddress(List<CustomerAddress> addressList, int index);
        
        /// <summary>
        /// 複製第一個地址到指定位置（記憶體操作）
        /// </summary>
        ServiceResult CopyAddressFromFirst(List<CustomerAddress> addressList, int targetIndex);
        
        #endregion

        #region 記憶體欄位更新方法
        
        /// <summary>
        /// 更新地址類型（記憶體操作）
        /// </summary>
        ServiceResult UpdateAddressType(List<CustomerAddress> addressList, int index, int? addressTypeId);
        
        /// <summary>
        /// 更新郵遞區號（記憶體操作）
        /// </summary>
        ServiceResult UpdatePostalCode(List<CustomerAddress> addressList, int index, string? postalCode);
        
        /// <summary>
        /// 更新城市（記憶體操作）
        /// </summary>
        ServiceResult UpdateCity(List<CustomerAddress> addressList, int index, string? city);
        
        /// <summary>
        /// 更新行政區（記憶體操作）
        /// </summary>
        ServiceResult UpdateDistrict(List<CustomerAddress> addressList, int index, string? district);
        
        /// <summary>
        /// 更新地址（記憶體操作）
        /// </summary>
        ServiceResult UpdateAddress(List<CustomerAddress> addressList, int index, string? address);
        
        #endregion

        #region 驗證和輔助方法
        
        /// <summary>
        /// 驗證地址清單
        /// </summary>
        ServiceResult ValidateAddressList(List<CustomerAddress> addresses);
        
        /// <summary>
        /// 確保地址清單中有主要地址（記憶體操作）
        /// </summary>
        ServiceResult EnsurePrimaryAddressExists(List<CustomerAddress> addresses);
          /// <summary>
        /// 取得地址完成度統計
        /// </summary>
        int GetCompletedAddressCount(List<CustomerAddress> addresses);
        
        /// <summary>
        /// 取得地址已完成欄位數量
        /// </summary>
        int GetAddressCompletedFieldsCount(List<CustomerAddress> addresses);
        
        #endregion
    }
}
