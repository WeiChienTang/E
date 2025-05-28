using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 客戶地址管理服務介面 - 專門處理客戶地址相關的業務邏輯
    /// </summary>
    public interface ICustomerAddressService
    {
        // 地址初始化和配置
        CustomerAddress CreateNewAddress(int customerId, int addressCount);
        void InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes);
        int? GetDefaultAddressTypeId(int addressIndex, List<AddressType> addressTypes);
        
        // 地址操作業務邏輯
        ServiceResult AddAddress(List<CustomerAddress> addressList, CustomerAddress newAddress);
        ServiceResult RemoveAddress(List<CustomerAddress> addressList, int index);
        ServiceResult SetPrimaryAddress(List<CustomerAddress> addressList, int index);
        ServiceResult CopyAddressFromFirst(List<CustomerAddress> addressList, int targetIndex);
        
        // 地址資料更新
        ServiceResult UpdateAddressType(List<CustomerAddress> addressList, int index, int? addressTypeId);
        ServiceResult UpdatePostalCode(List<CustomerAddress> addressList, int index, string? postalCode);
        ServiceResult UpdateCity(List<CustomerAddress> addressList, int index, string? city);
        ServiceResult UpdateDistrict(List<CustomerAddress> addressList, int index, string? district);
        ServiceResult UpdateAddress(List<CustomerAddress> addressList, int index, string? address);
        
        // 地址驗證和計算
        ServiceResult ValidateAddresses(List<CustomerAddress> addresses);
        int GetAddressCompletedFieldsCount(List<CustomerAddress> addresses);
        ServiceResult EnsurePrimaryAddressExists(List<CustomerAddress> addresses);
    }
}
