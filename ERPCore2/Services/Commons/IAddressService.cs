using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 統一地址服務介面 - 管理所有實體的地址資訊
    /// </summary>
    public interface IAddressService
    {
        // 查詢方法
        Task<List<Address>> GetAddressesByOwnerAsync(string ownerType, int ownerId);
        Task<Address?> GetAddressByIdAsync(int addressId);
        Task<Address?> GetPrimaryAddressAsync(string ownerType, int ownerId);
        
        // 客戶地址特定方法
        Task<List<Address>> GetCustomerAddressesAsync(int customerId);
        Task<Address?> GetCustomerPrimaryAddressAsync(int customerId);
        
        // 廠商地址特定方法
        Task<List<Address>> GetSupplierAddressesAsync(int supplierId);
        Task<Address?> GetSupplierPrimaryAddressAsync(int supplierId);
        
        // 員工地址特定方法
        Task<List<Address>> GetEmployeeAddressesAsync(int employeeId);
        Task<Address?> GetEmployeePrimaryAddressAsync(int employeeId);
        
        // 修改方法
        Task<Address> CreateAddressAsync(string ownerType, int ownerId, Address address);
        Task<Address> UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(int addressId);
        Task SetPrimaryAddressAsync(string ownerType, int ownerId, int addressId);
        
        // 驗證方法
        Task<bool> ValidateOwnerExistsAsync(string ownerType, int ownerId);
        bool IsValidOwnerType(string ownerType);
    }
}
