using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備服務實作
    /// </summary>
    public class EquipmentService : GenericManagementService<Equipment>, IEquipmentService
    {
        public EquipmentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<Equipment>> logger) : base(contextFactory, logger)
        {
        }

        public EquipmentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<Equipment> BuildGetAllQuery(AppDbContext context)
        {
            return context.Equipments
                .Include(e => e.EquipmentCategory)
                .Include(e => e.Employee)
                .OrderBy(e => e.Name);
        }

        public override async Task<Equipment?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Include(e => e.Employee)
                    .Include(e => e.EquipmentMaintenances)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Equipment>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lower = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Include(e => e.Employee)
                    .Where(e => (e.Code != null && e.Code.ToLower().Contains(lower)) ||
                                e.Name.ToLower().Contains(lower) ||
                                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(lower)) ||
                                (e.Brand != null && e.Brand.ToLower().Contains(lower)) ||
                                (e.Model != null && e.Model.ToLower().Contains(lower)) ||
                                (e.Location != null && e.Location.ToLower().Contains(lower)))
                    .OrderBy(e => e.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Equipment entity)
        {
            try
            {
                var errors = new List<string>();

                if (!entity.IsDraft)
                {
                    if (string.IsNullOrWhiteSpace(entity.Code))
                        errors.Add("設備編號為必填欄位");
                    else if (await IsEquipmentCodeExistsAsync(entity.Code, entity.Id > 0 ? entity.Id : null))
                        errors.Add("設備編號已存在");

                    if (string.IsNullOrWhiteSpace(entity.Name))
                        errors.Add("設備名稱為必填欄位");
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

        protected override async Task<ServiceResult> CanDeleteAsync(Equipment entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var hasMaintenances = await context.EquipmentMaintenances.AnyAsync(em => em.EquipmentId == entity.Id);
                if (hasMaintenances)
                    return ServiceResult.Failure("此設備已有保養記錄，無法刪除");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { EquipmentId = entity.Id });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        public async Task<(List<Equipment> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Equipment>, IQueryable<Equipment>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<Equipment> query = context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Include(e => e.Employee);

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(e => e.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new { PageNumber = pageNumber, PageSize = pageSize });
                return (new List<Equipment>(), 0);
            }
        }

        public async Task<bool> IsEquipmentCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code)) return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Equipments.Where(e => e.Code == code);
                if (excludeId.HasValue) query = query.Where(e => e.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEquipmentCodeExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<List<Equipment>> GetActiveEquipmentsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Where(e => e.Status == EntityStatus.Active)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveEquipmentsAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<Equipment>> GetByEquipmentCategoryAsync(int categoryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Include(e => e.Employee)
                    .Where(e => e.EquipmentCategoryId == categoryId)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEquipmentCategoryAsync), GetType(), _logger, new { CategoryId = categoryId });
                throw;
            }
        }

        public async Task<List<Equipment>> GetMaintenanceDueAsync(int withinDays = 30)
        {
            try
            {
                var cutoff = DateTime.Today.AddDays(withinDays);
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Equipments
                    .Include(e => e.EquipmentCategory)
                    .Where(e => e.Status == EntityStatus.Active &&
                                e.NextMaintenanceDate.HasValue &&
                                e.NextMaintenanceDate.Value <= cutoff)
                    .OrderBy(e => e.NextMaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetMaintenanceDueAsync), GetType(), _logger);
                throw;
            }
        }
    }
}
