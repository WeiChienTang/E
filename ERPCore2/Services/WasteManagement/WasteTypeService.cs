using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廢料類型服務實作
    /// </summary>
    public class WasteTypeService : GenericManagementService<WasteType>, IWasteTypeService
    {
        public WasteTypeService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<WasteType>> logger) : base(contextFactory, logger)
        {
        }

        public WasteTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<WasteType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteTypes
                    .AsQueryable()
                    .OrderBy(wt => wt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<WasteType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteTypes
                    .FirstOrDefaultAsync(wt => wt.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<WasteType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteTypes
                    .Where(wt => (wt.Code != null && wt.Code.ToLower().Contains(lowerSearchTerm)) ||
                                 wt.Name.ToLower().Contains(lowerSearchTerm) ||
                                 (wt.Description != null && wt.Description.ToLower().Contains(lowerSearchTerm)))
                    .OrderBy(wt => wt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WasteType entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("廢料類型編號為必填欄位");
                }
                else
                {
                    if (await IsWasteTypeCodeExistsAsync(entity.Code, entity.Id))
                        errors.Add("廢料類型編號已存在");
                }

                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    errors.Add("廢料類型名稱為必填欄位");
                }
                else
                {
                    if (await IsNameExistsAsync(entity.Name, entity.Id))
                        errors.Add("廢料類型名稱已存在");
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

        protected override async Task<ServiceResult> CanDeleteAsync(WasteType entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var hasRecords = await context.WasteRecords.AnyAsync(wr => wr.WasteTypeId == entity.Id);
                if (hasRecords)
                    return ServiceResult.Failure("此廢料類型已有廢料記錄使用，無法刪除");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { WasteTypeId = entity.Id });
                return ServiceResult.Failure("檢查廢料類型刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<WasteType?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteTypes
                    .FirstOrDefaultAsync(wt => wt.Code == code);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new { Code = code });
                throw;
            }
        }

        public async Task<bool> IsWasteTypeCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.WasteTypes.Where(wt => wt.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(wt => wt.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsWasteTypeCodeExistsAsync), GetType(), _logger, new { Code = code, ExcludeId = excludeId });
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
                var query = context.WasteTypes.Where(wt => wt.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(wt => wt.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { Name = name, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<WasteType>> GetActiveWasteTypesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteTypes
                    .Where(wt => wt.Status == EntityStatus.Active)
                    .OrderBy(wt => wt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveWasteTypesAsync), GetType(), _logger);
                throw;
            }
        }

        #endregion
    }
}
