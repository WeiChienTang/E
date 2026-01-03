using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class WeatherService : GenericManagementService<Weather>, IWeatherService
    {
        public WeatherService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Weather>> logger) : base(contextFactory, logger)
        {
        }

        public WeatherService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料方法，加入排序
        /// </summary>
        public override async Task<List<Weather>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Weathers
                    .AsQueryable()
                    .OrderBy(w => w.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Weather>();
            }
        }

        /// <summary>
        /// 覆寫搜尋方法，實作天氣特定的搜尋邏輯
        /// </summary>
        public override async Task<List<Weather>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Weathers
                    .Where(w => ((w.Name != null && w.Name.Contains(searchTerm)) ||
                                (w.Code != null && w.Code.Contains(searchTerm)) ||
                                (w.Description != null && w.Description.Contains(searchTerm))))
                    .OrderBy(w => w.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                return new List<Weather>();
            }
        }        /// <summary>
        /// 覆寫驗證方法，添加天氣特定的驗證規則
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Weather entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("天氣名稱為必填");

                // 檢查編號
                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("天氣編號為必填");

                // 檢查編號是否重複
                if (await IsCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("天氣編號已存在");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.Name, entity.Id))
                    return ServiceResult.Failure("天氣名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    EntityName = entity.Name,
                    ServiceType = GetType().Name 
                });
                return ServiceResult.Failure("天氣驗證過程中發生錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Weathers.Where(w => w.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(w => w.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false;
            }
        }

        /// <summary>
        /// 檢查天氣編號是否已存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Weathers.Where(w => w.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(w => w.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCodeExistsAsync),
                    Code = code,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false;
            }
        }

        /// <summary>
        /// 根據編號取得天氣資料
        /// </summary>
        public async Task<Weather?> GetByCodeAsync(string code)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Weathers
                    .Where(w => w.Code == code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByCodeAsync),
                    Code = code,
                    ServiceType = GetType().Name 
                });
                return null;
            }
        }
    }
}

