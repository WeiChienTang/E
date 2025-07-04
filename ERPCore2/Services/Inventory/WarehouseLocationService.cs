using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class WarehouseLocationService : GenericManagementService<WarehouseLocation>, IWarehouseLocationService
    {
        public WarehouseLocationService(
            AppDbContext context, 
            ILogger<GenericManagementService<WarehouseLocation>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        public override async Task<List<WarehouseLocation>> GetAllAsync()
        {
            try
            {
                return await _context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .Where(wl => !wl.IsDeleted)
                    .OrderBy(wl => wl.Warehouse.WarehouseName)
                    .ThenBy(wl => wl.LocationCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all warehouse locations");
                throw;
            }
        }

        public override async Task<WarehouseLocation?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .FirstOrDefaultAsync(wl => wl.Id == id && !wl.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByIdAsync),
                    Id = id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting warehouse location by id {Id}", id);
                throw;
            }
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByWarehouseIdAsync),
                    WarehouseId = warehouseId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting warehouse locations by warehouse {WarehouseId}", warehouseId);
                return ServiceResult<IEnumerable<WarehouseLocation>>.Failure("取得倉庫庫位列表時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WarehouseLocation entity)
        {
            try
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
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    WarehouseId = entity.WarehouseId,
                    LocationCode = entity.LocationCode,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating warehouse location entity {EntityId}", entity.Id);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<WarehouseLocation>> SearchAsync(string searchTerm)
        {
            try
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
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching warehouse locations with term {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}
