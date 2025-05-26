using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Repositories
{
    public class CustomerTypeRepository : ICustomerTypeRepository
    {
        private readonly AppDbContext _context;
        
        public CustomerTypeRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<CustomerType>> GetAllAsync()
        {
            return await _context.CustomerTypes
                .Where(e => e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.TypeName)
                .ToListAsync();
        }
        
        public async Task<CustomerType?> GetByIdAsync(int id)
        {
            return await _context.CustomerTypes
                .FirstOrDefaultAsync(e => e.CustomerTypeId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<CustomerType> AddAsync(CustomerType entity)
        {
            entity.Status = EntityStatus.Active;
            
            _context.CustomerTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task<CustomerType> UpdateAsync(CustomerType entity)
        {
            _context.CustomerTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.Status = EntityStatus.Deleted;
                await UpdateAsync(entity);
            }
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CustomerTypes
                .AnyAsync(e => e.CustomerTypeId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<CustomerType?> GetByNameAsync(string name)
        {
            return await _context.CustomerTypes
                .FirstOrDefaultAsync(e => e.TypeName == name && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<List<CustomerType>> GetActiveAsync()
        {
            return await _context.CustomerTypes
                .Where(e => e.Status == EntityStatus.Active)
                .OrderBy(e => e.TypeName)
                .ToListAsync();
        }
    }
}
