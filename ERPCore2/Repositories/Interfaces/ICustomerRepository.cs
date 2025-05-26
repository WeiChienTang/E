using ERPCore2.Data.Entities;

namespace ERPCore2.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        // Basic CRUD Operations
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> AddAsync(Customer entity);
        Task<Customer> UpdateAsync(Customer entity);
        Task DeleteAsync(int id);
        
        // Common Query Patterns
        Task<bool> ExistsAsync(int id);
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        Task<List<Customer>> GetActiveAsync();
        Task<List<Customer>> GetByStatusAsync(EntityStatus status);
        Task<List<Customer>> GetByCustomerTypeAsync(int customerTypeId);
        Task<List<Customer>> GetByIndustryAsync(int industryId);
        
        // Include related data
        Task<Customer?> GetByIdWithDetailsAsync(int id);
        Task<List<Customer>> GetAllWithDetailsAsync();
        
        // Pagination Support
        Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
