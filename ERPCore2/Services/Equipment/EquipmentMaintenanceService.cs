using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備保養維修記錄服務實作
    /// </summary>
    public class EquipmentMaintenanceService : GenericManagementService<EquipmentMaintenance>, IEquipmentMaintenanceService
    {
        public EquipmentMaintenanceService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EquipmentMaintenance>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public EquipmentMaintenanceService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<EquipmentMaintenance> BuildGetAllQuery(AppDbContext context)
        {
            return context.EquipmentMaintenances
                .Include(em => em.Equipment)
                .Include(em => em.Employee)
                .OrderByDescending(em => em.MaintenanceDate);
        }

        public override async Task<EquipmentMaintenance?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                        .ThenInclude(e => e!.EquipmentCategory)
                    .Include(em => em.Employee)
                    .FirstOrDefaultAsync(em => em.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<EquipmentMaintenance>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lower = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                    .Include(em => em.Employee)
                    .Where(em => (em.Code != null && em.Code.ToLower().Contains(lower)) ||
                                 (em.Equipment != null && em.Equipment.Name.ToLower().Contains(lower)) ||
                                 (em.ServiceProvider != null && em.ServiceProvider.ToLower().Contains(lower)) ||
                                 (em.Description != null && em.Description.ToLower().Contains(lower)))
                    .OrderByDescending(em => em.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EquipmentMaintenance entity)
        {
            try
            {
                var errors = new List<string>();

                if (!entity.IsDraft)
                {
                    if (string.IsNullOrWhiteSpace(entity.Code))
                        errors.Add("記錄編號為必填欄位");
                    else if (await IsEquipmentMaintenanceCodeExistsAsync(entity.Code, entity.Id > 0 ? entity.Id : null))
                        errors.Add("記錄編號已存在");

                    if (!entity.EquipmentId.HasValue)
                        errors.Add("所屬設備為必填欄位");

                    if (entity.MaintenanceDate == default)
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

        public async Task<(List<EquipmentMaintenance> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EquipmentMaintenance>, IQueryable<EquipmentMaintenance>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<EquipmentMaintenance> query = context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                    .Include(em => em.Employee);

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(em => em.MaintenanceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new { PageNumber = pageNumber, PageSize = pageSize });
                return (new List<EquipmentMaintenance>(), 0);
            }
        }

        public async Task<bool> IsEquipmentMaintenanceCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code)) return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EquipmentMaintenances.Where(em => em.Code == code);
                if (excludeId.HasValue) query = query.Where(em => em.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEquipmentMaintenanceCodeExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<List<EquipmentMaintenance>> GetByEquipmentAsync(int equipmentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                    .Include(em => em.Employee)
                    .Where(em => em.EquipmentId == equipmentId)
                    .OrderByDescending(em => em.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEquipmentAsync), GetType(), _logger, new { EquipmentId = equipmentId });
                throw;
            }
        }

        public async Task<List<EquipmentMaintenance>> GetByMaintenanceTypeAsync(EquipmentMaintenanceType maintenanceType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                    .Include(em => em.Employee)
                    .Where(em => em.MaintenanceType == maintenanceType)
                    .OrderByDescending(em => em.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByMaintenanceTypeAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<EquipmentMaintenance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentMaintenances
                    .Include(em => em.Equipment)
                    .Include(em => em.Employee)
                    .Where(em => em.MaintenanceDate >= startDate && em.MaintenanceDate <= endDate)
                    .OrderByDescending(em => em.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger);
                throw;
            }
        }
    }
}
