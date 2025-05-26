using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Where(e => e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(e => e.CustomerId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<Customer> AddAsync(Customer entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.Status = EntityStatus.Active;
            
            _context.Customers.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task<Customer> UpdateAsync(Customer entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            
            _context.Customers.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.Status = EntityStatus.Deleted;
                entity.ModifiedDate = DateTime.UtcNow;
                await UpdateAsync(entity);
            }
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers
                .AnyAsync(e => e.CustomerId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(e => e.CustomerCode == customerCode && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<List<Customer>> GetActiveAsync()
        {
            return await _context.Customers
                .Where(e => e.Status == EntityStatus.Active)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<List<Customer>> GetByStatusAsync(EntityStatus status)
        {
            return await _context.Customers
                .Where(e => e.Status == status)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<List<Customer>> GetByCustomerTypeAsync(int customerTypeId)
        {
            return await _context.Customers
                .Where(e => e.CustomerTypeId == customerTypeId && e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<List<Customer>> GetByIndustryAsync(int industryId)
        {
            return await _context.Customers
                .Where(e => e.IndustryId == industryId && e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<Customer?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.CustomerType)
                .Include(c => c.Industry)
                .Include(c => c.CustomerContacts.Where(cc => cc.Status != EntityStatus.Deleted))
                    .ThenInclude(cc => cc.ContactType)
                .Include(c => c.CustomerAddresses.Where(ca => ca.Status != EntityStatus.Deleted))
                    .ThenInclude(ca => ca.AddressType)
                .FirstOrDefaultAsync(e => e.CustomerId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<List<Customer>> GetAllWithDetailsAsync()
        {
            return await _context.Customers
                .Include(c => c.CustomerType)
                .Include(c => c.Industry)
                .Where(e => e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.CompanyName)
                .ToListAsync();
        }
        
        public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Customers
                .Where(e => e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.CompanyName);
                
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }
    }
}
