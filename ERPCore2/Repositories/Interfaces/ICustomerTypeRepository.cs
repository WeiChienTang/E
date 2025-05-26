using ERPCore2.Data.Entities;

namespace ERPCore2.Repositories.Interfaces
{
    public interface ICustomerTypeRepository
    {
        Task<List<CustomerType>> GetAllAsync();
        Task<CustomerType?> GetByIdAsync(int id);
        Task<CustomerType> AddAsync(CustomerType entity);
        Task<CustomerType> UpdateAsync(CustomerType entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<CustomerType?> GetByNameAsync(string name);
        Task<List<CustomerType>> GetActiveAsync();
    }
}
