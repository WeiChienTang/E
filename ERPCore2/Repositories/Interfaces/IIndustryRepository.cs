using ERPCore2.Data.Entities;

namespace ERPCore2.Repositories.Interfaces
{
    public interface IIndustryRepository
    {
        Task<List<Industry>> GetAllAsync();
        Task<Industry?> GetByIdAsync(int id);
        Task<Industry> AddAsync(Industry entity);
        Task<Industry> UpdateAsync(Industry entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Industry?> GetByNameAsync(string name);
        Task<List<Industry>> GetActiveAsync();
    }
}
