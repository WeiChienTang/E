using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 車型服務實作
    /// </summary>
    public class VehicleTypeService : GenericManagementService<VehicleType>, IVehicleTypeService
    {
        public VehicleTypeService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<VehicleType>> logger) : base(contextFactory, logger)
        {
        }

        public VehicleTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<VehicleType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Include(vt => vt.Vehicles)
                    .AsQueryable()
                    .OrderBy(vt => vt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<VehicleType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Include(vt => vt.Vehicles)
                    .FirstOrDefaultAsync(vt => vt.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<VehicleType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Include(vt => vt.Vehicles)
                    .Where(vt => ((vt.Code != null && vt.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (vt.Name != null && vt.Name.ToLower().Contains(lowerSearchTerm)) ||
                         (vt.Description != null && vt.Description.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(vt => vt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(VehicleType entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("車型編號為必填欄位");
                }
                else
                {
                    var isDuplicate = await IsVehicleTypeCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("車型編號已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    errors.Add("車型名稱為必填欄位");
                }
                else
                {
                    var isNameDuplicate = await IsVehicleTypeNameExistsAsync(entity.Name, entity.Id);
                    if (isNameDuplicate)
                    {
                        errors.Add("車型名稱已存在");
                    }
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

        protected override async Task<ServiceResult> CanDeleteAsync(VehicleType entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var hasVehicles = await context.Vehicles.AnyAsync(v => v.VehicleTypeId == entity.Id);
                if (hasVehicles)
                {
                    return ServiceResult.Failure("此車型已被車輛使用，無法刪除");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { VehicleTypeId = entity.Id });
                return ServiceResult.Failure("檢查車型刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<VehicleType?> GetByVehicleTypeCodeAsync(string vehicleTypeCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleTypeCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Include(vt => vt.Vehicles)
                    .FirstOrDefaultAsync(vt => vt.Code == vehicleTypeCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleTypeCodeAsync), GetType(), _logger, new { VehicleTypeCode = vehicleTypeCode });
                throw;
            }
        }

        public async Task<bool> IsVehicleTypeCodeExistsAsync(string vehicleTypeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleTypeCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.VehicleTypes.Where(vt => vt.Code == vehicleTypeCode);

                if (excludeId.HasValue)
                {
                    query = query.Where(vt => vt.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsVehicleTypeCodeExistsAsync), GetType(), _logger, new { VehicleTypeCode = vehicleTypeCode, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<bool> IsVehicleTypeNameExistsAsync(string vehicleTypeName, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleTypeName))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.VehicleTypes.Where(vt => vt.Name == vehicleTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(vt => vt.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsVehicleTypeNameExistsAsync), GetType(), _logger, new { VehicleTypeName = vehicleTypeName, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<VehicleType>> GetActiveVehicleTypesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Where(vt => vt.Status == EntityStatus.Active)
                    .OrderBy(vt => vt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveVehicleTypesAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<VehicleType>> GetByVehicleTypeNameAsync(string vehicleTypeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleTypeName))
                    return new List<VehicleType>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.VehicleTypes
                    .Include(vt => vt.Vehicles)
                    .Where(vt => vt.Name.Contains(vehicleTypeName))
                    .OrderBy(vt => vt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleTypeNameAsync), GetType(), _logger, new { VehicleTypeName = vehicleTypeName });
                throw;
            }
        }

        #endregion
    }
}
