using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車輛服務實作
    /// </summary>
    public class VehicleService : GenericManagementService<Vehicle>, IVehicleService
    {
        public VehicleService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<Vehicle>> logger) : base(contextFactory, logger)
        {
        }

        public VehicleService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<Vehicle>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Include(v => v.Company)
                    .AsQueryable()
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<Vehicle?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Include(v => v.Company)
                    .Include(v => v.VehicleMaintenances)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Vehicle>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Include(v => v.Company)
                    .Where(v => ((v.Code != null && v.Code.ToLower().Contains(lowerSearchTerm)) ||
                         v.LicensePlate.ToLower().Contains(lowerSearchTerm) ||
                         v.VehicleName.ToLower().Contains(lowerSearchTerm) ||
                         (v.Brand != null && v.Brand.ToLower().Contains(lowerSearchTerm)) ||
                         (v.Model != null && v.Model.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Vehicle entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("車輛編號為必填欄位");
                }
                else
                {
                    var isDuplicate = await IsVehicleCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("車輛編號已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.LicensePlate))
                {
                    errors.Add("車牌號碼為必填欄位");
                }
                else
                {
                    var isLicensePlateDuplicate = await IsLicensePlateExistsAsync(entity.LicensePlate, entity.Id);
                    if (isLicensePlateDuplicate)
                    {
                        errors.Add("車牌號碼已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.VehicleName))
                {
                    errors.Add("車輛名稱為必填欄位");
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

        protected override async Task<ServiceResult> CanDeleteAsync(Vehicle entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var hasMaintenances = await context.VehicleMaintenances.AnyAsync(vm => vm.VehicleId == entity.Id);
                if (hasMaintenances)
                {
                    return ServiceResult.Failure("此車輛已有保養紀錄，無法刪除");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { VehicleId = entity.Id });
                return ServiceResult.Failure("檢查車輛刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Vehicle?> GetByVehicleCodeAsync(string vehicleCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Include(v => v.Company)
                    .FirstOrDefaultAsync(v => v.Code == vehicleCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleCodeAsync), GetType(), _logger, new { VehicleCode = vehicleCode });
                throw;
            }
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licensePlate))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Include(v => v.Company)
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByLicensePlateAsync), GetType(), _logger, new { LicensePlate = licensePlate });
                throw;
            }
        }

        public async Task<bool> IsVehicleCodeExistsAsync(string vehicleCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Vehicles.Where(v => v.Code == vehicleCode);

                if (excludeId.HasValue)
                {
                    query = query.Where(v => v.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsVehicleCodeExistsAsync), GetType(), _logger, new { VehicleCode = vehicleCode, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licensePlate))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Vehicles.Where(v => v.LicensePlate == licensePlate);

                if (excludeId.HasValue)
                {
                    query = query.Where(v => v.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsLicensePlateExistsAsync), GetType(), _logger, new { LicensePlate = licensePlate, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<Vehicle>> GetActiveVehiclesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Where(v => v.Status == EntityStatus.Active)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveVehiclesAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<Vehicle>> GetByVehicleTypeAsync(int vehicleTypeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Where(v => v.VehicleTypeId == vehicleTypeId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleTypeAsync), GetType(), _logger, new { VehicleTypeId = vehicleTypeId });
                throw;
            }
        }

        public async Task<List<Vehicle>> GetByOwnershipTypeAsync(VehicleOwnershipType ownershipType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Include(v => v.Employee)
                    .Include(v => v.Customer)
                    .Where(v => v.OwnershipType == ownershipType)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByOwnershipTypeAsync), GetType(), _logger, new { OwnershipType = ownershipType });
                throw;
            }
        }

        public async Task<List<Vehicle>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Where(v => v.CustomerId == customerId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerAsync), GetType(), _logger, new { CustomerId = customerId });
                throw;
            }
        }

        public async Task<List<Vehicle>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Vehicles
                    .Include(v => v.VehicleType)
                    .Where(v => v.EmployeeId == employeeId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new { EmployeeId = employeeId });
                throw;
            }
        }

        #endregion
    }
}
