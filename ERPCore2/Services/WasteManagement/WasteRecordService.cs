using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廢料記錄服務實作
    /// </summary>
    public class WasteRecordService : GenericManagementService<WasteRecord>, IWasteRecordService
    {
        public WasteRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<WasteRecord>> logger) : base(contextFactory, logger)
        {
        }

        public WasteRecordService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<WasteRecord>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .AsQueryable()
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<WasteRecord?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .FirstOrDefaultAsync(wr => wr.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<WasteRecord>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Where(wr => (wr.Code != null && wr.Code.ToLower().Contains(lowerSearchTerm)) ||
                                 wr.Vehicle.LicensePlate.ToLower().Contains(lowerSearchTerm) ||
                                 wr.WasteType.Name.ToLower().Contains(lowerSearchTerm) ||
                                 (wr.Customer != null && wr.Customer.CompanyName != null && wr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WasteRecord entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.VehicleId <= 0)
                    errors.Add("車輛為必填欄位");

                if (entity.WasteTypeId <= 0)
                    errors.Add("廢料類型為必填欄位");

                // 若同時選擇了車輛與客戶，驗證車輛的所屬客戶是否相符
                if (entity.VehicleId > 0 && entity.CustomerId.HasValue)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var vehicle = await context.Vehicles
                        .AsNoTracking()
                        .Select(v => new { v.Id, v.CustomerId })
                        .FirstOrDefaultAsync(v => v.Id == entity.VehicleId);

                    if (vehicle?.CustomerId.HasValue == true && vehicle.CustomerId != entity.CustomerId)
                        errors.Add("所選車輛不屬於此客戶，請確認車輛與客戶是否匹配");
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

        public async Task<bool> IsWasteRecordCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.WasteRecords.Where(wr => wr.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(wr => wr.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsWasteRecordCodeExistsAsync), GetType(), _logger, new { Code = code, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<WasteRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Where(wr => wr.RecordDate >= startDate && wr.RecordDate <= endDate)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { StartDate = startDate, EndDate = endDate });
                throw;
            }
        }

        public async Task<List<WasteRecord>> GetByVehicleAsync(int vehicleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Where(wr => wr.VehicleId == vehicleId)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleAsync), GetType(), _logger, new { VehicleId = vehicleId });
                throw;
            }
        }

        public async Task<List<WasteRecord>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Where(wr => wr.CustomerId == customerId)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerAsync), GetType(), _logger, new { CustomerId = customerId });
                throw;
            }
        }

        #endregion
    }
}
