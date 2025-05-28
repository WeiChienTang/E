using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址管理服務介面 - 直接使用 Entity，無需 DTO
    /// </summary>
    public interface IAddressService
    {
        // 取得地址相關資料
        Task<List<AddressType>> GetAddressTypesAsync();
        Task<List<CustomerAddress>> GetAddressesByCustomerIdAsync(int customerId);
        Task<CustomerAddress?> GetPrimaryAddressAsync(int customerId);
          // 地址業務邏輯操作
        Task<ServiceResult<CustomerAddress>> CreateAddressAsync(CustomerAddress address);
        Task<ServiceResult<CustomerAddress>> UpdateAddressAsync(CustomerAddress address);
        Task<ServiceResult> DeleteAddressAsync(int addressId);
        Task<ServiceResult> SetPrimaryAddressAsync(int customerId, int addressId);
        Task<ServiceResult> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses);
        
        // 地址驗證和業務規則
        Task<ServiceResult> ValidateAddressAsync(CustomerAddress address);
        Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId);
          // 地址操作輔助方法
        Task<ServiceResult<CustomerAddress>> CopyFromAddressAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null);
        Task<int?> GetDefaultAddressTypeIdAsync(string addressTypeName);
        
        // 地址載入方法，包含預設地址初始化
        Task<List<CustomerAddress>> GetAddressesWithDefaultAsync(int customerId, List<AddressType> addressTypes);
    }
}
