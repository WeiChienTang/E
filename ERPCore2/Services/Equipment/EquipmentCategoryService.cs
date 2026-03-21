using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 設備類別服務實作
    /// </summary>
    public class EquipmentCategoryService : GenericManagementService<EquipmentCategory>, IEquipmentCategoryService
    {
        public EquipmentCategoryService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EquipmentCategory>> logger) : base(contextFactory, logger)
        {
        }

        public EquipmentCategoryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<EquipmentCategory> BuildGetAllQuery(AppDbContext context)
        {
            return context.EquipmentCategories
                .OrderBy(ec => ec.Name);
        }

        public override async Task<EquipmentCategory?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentCategories
                    .Include(ec => ec.Equipments)
                    .FirstOrDefaultAsync(ec => ec.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<EquipmentCategory>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lower = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentCategories
                    .Where(ec => (ec.Code != null && ec.Code.ToLower().Contains(lower)) ||
                                 (ec.Name != null && ec.Name.ToLower().Contains(lower)) ||
                                 (ec.Description != null && ec.Description.ToLower().Contains(lower)))
                    .OrderBy(ec => ec.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EquipmentCategory entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("類別編號為必填欄位");
                else if (await IsEquipmentCategoryCodeExistsAsync(entity.Code, entity.Id > 0 ? entity.Id : null))
                    errors.Add("類別編號已存在");

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("類別名稱為必填欄位");
                else if (await IsEquipmentCategoryNameExistsAsync(entity.Name, entity.Id > 0 ? entity.Id : null))
                    errors.Add("類別名稱已存在");

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

        protected override async Task<ServiceResult> CanDeleteAsync(EquipmentCategory entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var hasEquipments = await context.Equipments.AnyAsync(e => e.EquipmentCategoryId == entity.Id);
                if (hasEquipments)
                    return ServiceResult.Failure("此設備類別已被設備使用，無法刪除");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { EquipmentCategoryId = entity.Id });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        public async Task<(List<EquipmentCategory> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<EquipmentCategory>, IQueryable<EquipmentCategory>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<EquipmentCategory> query = context.EquipmentCategories;

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(ec => ec.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new { PageNumber = pageNumber, PageSize = pageSize });
                return (new List<EquipmentCategory>(), 0);
            }
        }

        public async Task<bool> IsEquipmentCategoryCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code)) return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EquipmentCategories.Where(ec => ec.Code == code);
                if (excludeId.HasValue) query = query.Where(ec => ec.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEquipmentCategoryCodeExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<bool> IsEquipmentCategoryNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EquipmentCategories.Where(ec => ec.Name == name);
                if (excludeId.HasValue) query = query.Where(ec => ec.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEquipmentCategoryNameExistsAsync), GetType(), _logger);
                return false;
            }
        }

        public async Task<List<EquipmentCategory>> GetActiveEquipmentCategoriesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EquipmentCategories
                    .Where(ec => ec.Status == EntityStatus.Active)
                    .OrderBy(ec => ec.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveEquipmentCategoriesAsync), GetType(), _logger);
                throw;
            }
        }
    }
}
