using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 倉庫服務實作
    /// </summary>
    public class WarehouseService : GenericManagementService<Warehouse>, IWarehouseService
    {
        public WarehouseService(
            AppDbContext context, 
            ILogger<GenericManagementService<Warehouse>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含相關資料
        /// </summary>
        public override async Task<List<Warehouse>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(w => w.WarehouseLocations)
                    .Where(w => !w.IsDeleted)
                    .OrderBy(w => w.WarehouseCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all warehouses");
                throw;
            }
        }

        /// <summary>
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<Warehouse>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(w => w.WarehouseLocations)
                    .Where(w => !w.IsDeleted &&
                               (w.WarehouseName.Contains(searchTerm) ||
                                w.WarehouseCode.Contains(searchTerm)))
                    .OrderBy(w => w.WarehouseCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching warehouses with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// 檢查倉庫代碼是否存在
        /// </summary>
        public async Task<bool> IsWarehouseCodeExistsAsync(string warehouseCode, int? excludeId = null)
        {
            var query = _dbSet.Where(w => w.WarehouseCode == warehouseCode && !w.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// 根據倉庫類型取得倉庫
        /// </summary>
        public async Task<List<Warehouse>> GetByWarehouseTypeAsync(WarehouseTypeEnum warehouseType)
        {
            return await _dbSet
                .Include(w => w.WarehouseLocations)
                .Where(w => !w.IsDeleted && w.WarehouseType == warehouseType)
                .OrderBy(w => w.WarehouseCode)
                .ToListAsync();
        }

        /// <summary>
        /// 取得預設倉庫
        /// </summary>
        public async Task<Warehouse?> GetDefaultWarehouseAsync()
        {
            return await _dbSet
                .Include(w => w.WarehouseLocations)
                .FirstOrDefaultAsync(w => !w.IsDeleted && w.IsDefault);
        }

        /// <summary>
        /// 設定預設倉庫
        /// </summary>
        public async Task<ServiceResult> SetDefaultWarehouseAsync(int warehouseId)
        {
            try
            {
                // 先取消所有倉庫的預設設定
                var allWarehouses = await _dbSet.Where(w => !w.IsDeleted).ToListAsync();
                foreach (var warehouse in allWarehouses)
                {
                    warehouse.IsDefault = warehouse.Id == warehouseId;
                    warehouse.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"設定預設倉庫失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得倉庫及其庫位
        /// </summary>
        public async Task<Warehouse?> GetWarehouseWithLocationsAsync(int warehouseId)
        {
            return await _dbSet
                .Include(w => w.WarehouseLocations.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(w => w.Id == warehouseId && !w.IsDeleted);
        }

        /// <summary>
        /// 覆寫驗證方法
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Warehouse entity)
        {
            // 基本驗證
            if (string.IsNullOrWhiteSpace(entity.WarehouseCode))
                return ServiceResult.Failure("倉庫代碼為必填");
            
            if (string.IsNullOrWhiteSpace(entity.WarehouseName))
                return ServiceResult.Failure("倉庫名稱為必填");

            // 檢查倉庫代碼是否重複
            if (await IsWarehouseCodeExistsAsync(entity.WarehouseCode, entity.Id))
                return ServiceResult.Failure("倉庫代碼已存在");

            return ServiceResult.Success();
        }

        /// <summary>
        /// 覆寫名稱存在檢查（使用倉庫名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(w => w.WarehouseName == name && !w.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}