using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 倉庫服務實作
    /// </summary>
    public class WarehouseService : GenericManagementService<Warehouse>, IWarehouseService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public WarehouseService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Warehouse>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public WarehouseService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含相關資料
        /// </summary>
        public override async Task<List<Warehouse>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Warehouses
                    .Include(w => w.WarehouseLocations)
                    .OrderBy(w => w.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<Warehouse>();
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

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Warehouses
                    .Include(w => w.WarehouseLocations)
                    .Where(w => ((w.Name != null && w.Name.Contains(searchTerm)) ||
                                (w.Code != null && w.Code.Contains(searchTerm))))
                    .OrderBy(w => w.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<Warehouse>();
            }
        }

        /// <summary>
        /// 檢查倉庫代碼是否存在
        /// </summary>
        public async Task<bool> IsWarehouseCodeExistsAsync(string warehouseCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Warehouses.Where(w => w.Code == warehouseCode);

                if (excludeId.HasValue)
                    query = query.Where(w => w.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsWarehouseCodeExistsAsync), GetType(), _logger, new { WarehouseCode = warehouseCode, ExcludeId = excludeId });
                return false;
            }
        }

        /// <summary>
        /// 檢查倉庫名稱是否存在
        /// </summary>
        public async Task<bool> IsWarehouseNameExistsAsync(string warehouseName, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Warehouses.Where(w => w.Name == warehouseName);

                if (excludeId.HasValue)
                    query = query.Where(w => w.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsWarehouseNameExistsAsync), GetType(), _logger, new { WarehouseName = warehouseName, ExcludeId = excludeId });
                return false;
            }
        }

        /// <summary>
        /// 取得倉庫及其庫位
        /// </summary>
        public async Task<Warehouse?> GetWarehouseWithLocationsAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Warehouses
                    .Include(w => w.WarehouseLocations.AsQueryable())
                    .FirstOrDefaultAsync(w => w.Id == warehouseId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWarehouseWithLocationsAsync), GetType(), _logger, new { WarehouseId = warehouseId });
                return null;
            }
        }

        /// <summary>
        /// 覆寫驗證方法
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Warehouse entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("倉庫代碼為必填");
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("倉庫名稱為必填");

                // 檢查倉庫代碼是否重複
                if (await IsWarehouseCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("倉庫代碼已存在");

                // 檢查倉庫名稱是否重複
                if (await IsWarehouseNameExistsAsync(entity.Name, entity.Id))
                    return ServiceResult.Failure("倉庫名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查（使用倉庫名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Warehouses.Where(w => w.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(w => w.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { Name = name, ExcludeId = excludeId });
                return false;
            }
        }
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作倉庫特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Warehouse entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckWarehouseDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("倉庫"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = entity.Id 
                });
                return ServiceResult.Failure("檢查倉庫刪除條件時發生錯誤");
            }
        }
    }
}

