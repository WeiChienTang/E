using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Inventory
{
    public class WarehouseLocationService : GenericManagementService<WarehouseLocation>, IWarehouseLocationService
    {
        public WarehouseLocationService(AppDbContext context) : base(context)
        {
        }

        public override async Task<List<WarehouseLocation>> GetAllAsync()
        {
            return await _context.WarehouseLocations
                .Include(wl => wl.Warehouse)
                .Where(wl => !wl.IsDeleted)
                .OrderBy(wl => wl.Warehouse.WarehouseName)
                .ThenBy(wl => wl.LocationCode)
                .ToListAsync();
        }

        public override async Task<WarehouseLocation?> GetByIdAsync(int id)
        {
            return await _context.WarehouseLocations
                .Include(wl => wl.Warehouse)
                .FirstOrDefaultAsync(wl => wl.Id == id && !wl.IsDeleted);
        }

        public async Task<ServiceResult<IEnumerable<WarehouseLocation>>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                var entities = await _context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .Where(wl => wl.WarehouseId == warehouseId && !wl.IsDeleted)
                    .OrderBy(wl => wl.LocationCode)
                    .ToListAsync();

                return ServiceResult<IEnumerable<WarehouseLocation>>.Success(entities);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<WarehouseLocation>>.Failure($"取得倉庫庫位列表失敗: {ex.Message}");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WarehouseLocation entity)
        {
            // 檢查在同一倉庫中庫位代碼是否重複
            var exists = await _context.WarehouseLocations
                .AnyAsync(wl => wl.WarehouseId == entity.WarehouseId && 
                               wl.LocationCode == entity.LocationCode && 
                               wl.Id != entity.Id && 
                               !wl.IsDeleted);

            if (exists)
            {
                return ServiceResult.Failure($"在倉庫中已存在庫位代碼 '{entity.LocationCode}'");
            }

            return ServiceResult.Success();
        }

        public override async Task<List<WarehouseLocation>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.WarehouseLocations
                .Include(wl => wl.Warehouse)
                .Where(wl => !wl.IsDeleted && 
                            (wl.LocationCode.Contains(searchTerm) || 
                             wl.LocationName.Contains(searchTerm) ||
                             wl.Warehouse.WarehouseName.Contains(searchTerm)))
                .OrderBy(wl => wl.Warehouse.WarehouseName)
                .ThenBy(wl => wl.LocationCode)
                .ToListAsync();
        }
    }
}
