using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 磅秤類型服務實作
    /// </summary>
    public class ScaleTypeService : GenericManagementService<ScaleType>, IScaleTypeService
    {
        public ScaleTypeService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ScaleType>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public ScaleTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<ScaleType> BuildGetAllQuery(AppDbContext context)
        {
            return context.ScaleTypes
                .OrderBy(st => st.Name);
        }

        public override async Task<ScaleType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleTypes
                    .FirstOrDefaultAsync(st => st.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<ScaleType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleTypes
                    .Where(st => (st.Code != null && st.Code.ToLower().Contains(lowerSearchTerm)) ||
                                 st.Name.ToLower().Contains(lowerSearchTerm) ||
                                 (st.Description != null && st.Description.ToLower().Contains(lowerSearchTerm)))
                    .OrderBy(st => st.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ScaleType entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("磅秤類型編號為必填欄位");
                }
                else
                {
                    if (await IsScaleTypeCodeExistsAsync(entity.Code, entity.Id))
                        errors.Add("磅秤類型編號已存在");
                }

                if (!await IsFieldRelaxedByEbcAsync(nameof(entity.Name))
                    && string.IsNullOrWhiteSpace(entity.Name))
                {
                    errors.Add("磅秤類型名稱為必填欄位");
                }
                else if (!string.IsNullOrWhiteSpace(entity.Name))
                {
                    if (await IsNameExistsAsync(entity.Name, entity.Id))
                        errors.Add("磅秤類型名稱已存在");
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

        protected override async Task<ServiceResult> CanDeleteAsync(ScaleType entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // ScaleTypeId 已從 ScaleRecord 移除，允許刪除
                _ = context;

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { ScaleTypeId = entity.Id });
                return ServiceResult.Failure("檢查磅秤類型刪除條件時發生錯誤");
            }
        }

        #endregion

        public async Task<(List<ScaleType> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ScaleType>, IQueryable<ScaleType>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<ScaleType> query = context.ScaleTypes;

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(st => st.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new {
                    Method = nameof(GetPagedWithFiltersAsync),
                    ServiceType = GetType().Name,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
                return (new List<ScaleType>(), 0);
            }
        }

        #region 業務特定查詢方法

        public async Task<ScaleType?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleTypes
                    .FirstOrDefaultAsync(st => st.Code == code);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new { Code = code });
                throw;
            }
        }

        public async Task<bool> IsScaleTypeCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ScaleTypes.Where(st => st.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsScaleTypeCodeExistsAsync), GetType(), _logger, new { Code = code, ExcludeId = excludeId });
                return false;
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ScaleTypes.Where(st => st.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { Name = name, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<ScaleType>> GetActiveScaleTypesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleTypes
                    .Where(st => st.Status == EntityStatus.Active)
                    .OrderBy(st => st.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveScaleTypesAsync), GetType(), _logger);
                throw;
            }
        }

        #endregion
    }
}
