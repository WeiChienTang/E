using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class WarehouseLocationService : GenericManagementService<WarehouseLocation>, IWarehouseLocationService
    {
        private readonly IErrorLogService _errorLogService;

        // 完整建構子
        public WarehouseLocationService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<WarehouseLocation>> logger, 
            IErrorLogService errorLogService) : base(contextFactory, logger)
        {
            _errorLogService = errorLogService;
        }

        // 簡易建構子
        public WarehouseLocationService(
            IDbContextFactory<AppDbContext> contextFactory, 
            IErrorLogService errorLogService) : base(contextFactory)
        {
            _errorLogService = errorLogService;
        }

        public override async Task<List<WarehouseLocation>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .Where(wl => !wl.IsDeleted)
                    .OrderBy(wl => wl.Warehouse.Name)
                    .ThenBy(wl => wl.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetAllAsync), 
                    GetType(), 
                    _logger,
                    new { ServiceType = GetType().Name }
                );
                throw;
            }
        }

        public override async Task<WarehouseLocation?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .FirstOrDefaultAsync(wl => wl.Id == id && !wl.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetByIdAsync), 
                    GetType(), 
                    _logger,
                    new { Id = id, ServiceType = GetType().Name }
                );
                throw;
            }
        }

        public async Task<ServiceResult<IEnumerable<WarehouseLocation>>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entities = await context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .Where(wl => wl.WarehouseId == warehouseId && !wl.IsDeleted)
                    .OrderBy(wl => wl.Code)
                    .ToListAsync();

                return ServiceResult<IEnumerable<WarehouseLocation>>.Success(entities);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(GetByWarehouseIdAsync), 
                    GetType(), 
                    _logger,
                    new { WarehouseId = warehouseId, ServiceType = GetType().Name }
                );
                return ServiceResult<IEnumerable<WarehouseLocation>>.Failure("取得倉庫庫位列表時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WarehouseLocation entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查在同一倉庫中庫位代碼是否重複
                var exists = await context.WarehouseLocations
                    .AnyAsync(wl => wl.WarehouseId == entity.WarehouseId && 
                                   wl.Code == entity.Code && 
                                   wl.Id != entity.Id && 
                                   !wl.IsDeleted);

                if (exists)
                {
                    return ServiceResult.Failure($"在倉庫中已存在庫位代碼 '{entity.Code}'");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(ValidateAsync), 
                    GetType(), 
                    _logger,
                    new { 
                        EntityId = entity.Id,
                        WarehouseId = entity.WarehouseId,
                        LocationCode = entity.Code,
                        ServiceType = GetType().Name 
                    }
                );
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<WarehouseLocation>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WarehouseLocations
                    .Include(wl => wl.Warehouse)
                    .Where(wl => !wl.IsDeleted && 
                                ((wl.Code != null && wl.Code.Contains(searchTerm)) || 
                                 (wl.Name != null && wl.Name.Contains(searchTerm)) ||
                                 (wl.Warehouse.Name != null && wl.Warehouse.Name.Contains(searchTerm))))
                    .OrderBy(wl => wl.Warehouse.Name)
                    .ThenBy(wl => wl.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(SearchAsync), 
                    GetType(), 
                    _logger,
                    new { SearchTerm = searchTerm, ServiceType = GetType().Name }
                );
                throw;
            }
        }

        public async Task<bool> IsWarehouseLocationCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.WarehouseLocations.Where(wl => wl.Code == code && !wl.IsDeleted);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(wl => wl.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, 
                    nameof(IsWarehouseLocationCodeExistsAsync), 
                    GetType(), 
                    _logger,
                    new { Code = code, ExcludeId = excludeId, ServiceType = GetType().Name }
                );
                throw;
            }
        }
    }
}

