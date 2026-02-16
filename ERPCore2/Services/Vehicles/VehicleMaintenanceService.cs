using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車輛保養紀錄服務實作
    /// </summary>
    public class VehicleMaintenanceService : GenericManagementService<VehicleMaintenance>, IVehicleMaintenanceService
    {
        public VehicleMaintenanceService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<VehicleMaintenance>> logger) : base(contextFactory, logger)
        {
        }

        public VehicleMaintenanceService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<VehicleMaintenance>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .AsQueryable()
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<VehicleMaintenance?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .FirstOrDefaultAsync(vm => vm.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<VehicleMaintenance>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .Where(vm => ((vm.Code != null && vm.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (vm.Description != null && vm.Description.ToLower().Contains(lowerSearchTerm)) ||
                         (vm.ServiceProvider != null && vm.ServiceProvider.ToLower().Contains(lowerSearchTerm)) ||
                         vm.Vehicle.LicensePlate.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(VehicleMaintenance entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("保養紀錄編號為必填欄位");
                }
                else
                {
                    var isDuplicate = await IsVehicleMaintenanceCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("保養紀錄編號已存在");
                    }
                }

                if (entity.VehicleId <= 0)
                {
                    errors.Add("所屬車輛為必填欄位");
                }

                if (entity.MaintenanceDate == default)
                {
                    errors.Add("保養日期為必填欄位");
                }

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<VehicleMaintenance?> GetByMaintenanceCodeAsync(string maintenanceCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maintenanceCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .FirstOrDefaultAsync(vm => vm.Code == maintenanceCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByMaintenanceCodeAsync), GetType(), _logger, new { MaintenanceCode = maintenanceCode });
                throw;
            }
        }

        public async Task<bool> IsVehicleMaintenanceCodeExistsAsync(string maintenanceCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maintenanceCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.VehicleMaintenances.Where(vm => vm.Code == maintenanceCode);

                if (excludeId.HasValue)
                {
                    query = query.Where(vm => vm.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsVehicleMaintenanceCodeExistsAsync), GetType(), _logger, new { MaintenanceCode = maintenanceCode, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<VehicleMaintenance>> GetByVehicleAsync(int vehicleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .Where(vm => vm.VehicleId == vehicleId)
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleAsync), GetType(), _logger, new { VehicleId = vehicleId });
                throw;
            }
        }

        public async Task<List<VehicleMaintenance>> GetByMaintenanceTypeAsync(MaintenanceType maintenanceType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .Where(vm => vm.MaintenanceType == maintenanceType)
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByMaintenanceTypeAsync), GetType(), _logger, new { MaintenanceType = maintenanceType });
                throw;
            }
        }

        public async Task<List<VehicleMaintenance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .Where(vm => vm.MaintenanceDate >= startDate && vm.MaintenanceDate <= endDate)
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { StartDate = startDate, EndDate = endDate });
                throw;
            }
        }

        public async Task<List<VehicleMaintenance>> GetActiveMaintenancesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleMaintenances
                    .Include(vm => vm.Vehicle)
                    .Include(vm => vm.Employee)
                    .Where(vm => vm.Status == EntityStatus.Active)
                    .OrderByDescending(vm => vm.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveMaintenancesAsync), GetType(), _logger);
                throw;
            }
        }

        #endregion
    }
}
