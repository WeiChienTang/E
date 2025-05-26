using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Repositories
{
    public class IndustryRepository : IIndustryRepository
    {
        private readonly AppDbContext _context;
        
        public IndustryRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Industry>> GetAllAsync()
        {
            return await _context.Industries
                .Where(e => e.Status != EntityStatus.Deleted)
                .OrderBy(e => e.IndustryName)
                .ToListAsync();
        }
        
        public async Task<Industry?> GetByIdAsync(int id)
        {
            return await _context.Industries
                .FirstOrDefaultAsync(e => e.IndustryId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<Industry> AddAsync(Industry entity)
        {
            entity.Status = EntityStatus.Active;
            
            _context.Industries.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task<Industry> UpdateAsync(Industry entity)
        {
            _context.Industries.Update(entity);
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
            return await _context.Industries
                .AnyAsync(e => e.IndustryId == id && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<Industry?> GetByNameAsync(string name)
        {
            return await _context.Industries
                .FirstOrDefaultAsync(e => e.IndustryName == name && e.Status != EntityStatus.Deleted);
        }
        
        public async Task<List<Industry>> GetActiveAsync()
        {
            return await _context.Industries
                .Where(e => e.Status == EntityStatus.Active)
                .OrderBy(e => e.IndustryName)
                .ToListAsync();
        }
    }
}
