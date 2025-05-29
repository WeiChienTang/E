using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 客戶地址管理服務介面 - 統一處理所有客戶地址相關操作
    /// </summary>
    public interface ICustomerAddressService : IGenericManagementService<CustomerAddress>
    {
        #region 資料查詢
        
        /// <summary>
        /// 取得客戶的所有地址
        /// </summary>
        Task<List<CustomerAddress>> GetAddressesByCustomerIdAsync(int customerId);
        
        /// <summary>
        /// 取得客戶的主要地址
        /// </summary>
        Task<CustomerAddress?> GetPrimaryAddressAsync(int customerId);
        
        /// <summary>
        /// 根據地址ID取得地址
        /// </summary>
        Task<CustomerAddress?> GetByIdAsync(int addressId);
        
        #endregion

        #region CRUD 操作
        
        /// <summary>
        /// 建立新地址
        /// </summary>
        Task<ServiceResult<CustomerAddress>> CreateAddressAsync(CustomerAddress address);
        
        /// <summary>
        /// 更新地址
        /// </summary>
        Task<ServiceResult<CustomerAddress>> UpdateAddressAsync(CustomerAddress address);
        
        /// <summary>
        /// 刪除地址（軟刪除）
        /// </summary>
        Task<ServiceResult<bool>> DeleteAddressAsync(int addressId);
        
        /// <summary>
        /// 批量更新客戶地址
        /// </summary>
        Task<ServiceResult<bool>> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses);
        
        #endregion

        #region 業務邏輯操作
        
        /// <summary>
        /// 設定主要地址
        /// </summary>
        Task<ServiceResult<bool>> SetPrimaryAddressAsync(int addressId);
        
        /// <summary>
        /// 複製地址到另一個客戶
        /// </summary>
        Task<ServiceResult<CustomerAddress>> CopyAddressToCustomerAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null);
        
        /// <summary>
        /// 確保客戶有主要地址
        /// </summary>
        Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId);
        
        #endregion

        #region 記憶體操作方法（用於UI編輯）
        
        /// <summary>
        /// 建立新的地址物件（記憶體操作）
        /// </summary>
        CustomerAddress CreateNewAddress(int customerId, int addressCount);
        
        /// <summary>
        /// 初始化預設地址清單（記憶體操作）
        /// </summary>
        void InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes);
        
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
        ServiceResult ValidateAddresses(List<CustomerAddress> addresses);
        
        /// <summary>
        /// 驗證單一地址
        /// </summary>
        Task<ServiceResult> ValidateAddressAsync(CustomerAddress address);
        
        /// <summary>
        /// 確保主要地址存在（記憶體操作）
        /// </summary>
        ServiceResult EnsurePrimaryAddressExists(List<CustomerAddress> addresses);
        
        /// <summary>
        /// 計算已完成的地址欄位數量
        /// </summary>
        int GetAddressCompletedFieldsCount(List<CustomerAddress> addresses);
        
        /// <summary>
        /// 取得預設地址類型ID
        /// </summary>
        int? GetDefaultAddressTypeId(int addressIndex, List<AddressType> addressTypes);
        
        /// <summary>
        /// 根據名稱取得預設地址類型ID
        /// </summary>
        Task<int?> GetDefaultAddressTypeIdAsync(string addressTypeName);
        
        /// <summary>
        /// 取得地址（包含預設地址初始化）
        /// </summary>
        Task<List<CustomerAddress>> GetAddressesWithDefaultAsync(int customerId, List<AddressType> addressTypes);
        
        #endregion
    }
}