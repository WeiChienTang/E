using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶服務介面 - 直接使用 Entity，無需 DTO
    /// </summary>
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<ServiceResult<Customer>> CreateAsync(Customer customer);
        Task<ServiceResult<Customer>> UpdateAsync(Customer customer);
        Task<ServiceResult> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        Task<List<Customer>> GetByCompanyNameAsync(string companyName);
        Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<List<Customer>> GetActiveCustomersAsync();
          // 新增方法用於支持下拉列表
        Task<List<CustomerType>> GetCustomerTypesAsync();
        Task<List<IndustryType>> GetIndustryTypesAsync();
        Task<List<ContactType>> GetContactTypesAsync();
        Task<List<AddressType>> GetAddressTypesAsync();
          // 聯絡資料管理方法
        Task<List<CustomerContact>> GetCustomerContactsAsync(int customerId);
        Task<ServiceResult> UpdateCustomerContactsAsync(int customerId, List<CustomerContact> contacts);
        
        // 客戶初始化和完成度計算方法
        void InitializeNewCustomer(Customer customer);
        int GetBasicRequiredFieldsCount();
        int GetBasicCompletedFieldsCount(Customer customer);
    }
}
