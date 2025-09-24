using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class ColorService : GenericManagementService<Color>, IColorService
    {
        public ColorService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Color>> logger) : base(contextFactory, logger)
        {
        }

        public ColorService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料方法，加入排序
        /// </summary>
        public override async Task<List<Color>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Colors
                    .AsQueryable()
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Color>();
            }
        }

        /// <summary>
        /// 覆寫搜尋方法，實作顏色特定的搜尋邏輯
        /// </summary>
        public override async Task<List<Color>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Colors
                    .Where(c => ((c.Name != null && c.Name.Contains(searchTerm)) ||
                                (c.Code != null && c.Code.Contains(searchTerm)) ||
                                (c.Description != null && c.Description.Contains(searchTerm))))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<Color>();
            }
        }

        /// <summary>
        /// 覆寫驗證方法，添加顏色特定的驗證規則
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Color entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("顏色名稱為必填");

                // 檢查代碼是否重複（只有在提供代碼時才檢查）
                if (!string.IsNullOrWhiteSpace(entity.Code) && await IsCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("顏色代碼已存在");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.Name, entity.Id))
                    return ServiceResult.Failure("顏色名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.Name 
                });
                return ServiceResult.Failure("驗證過程中發生錯誤");
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
                var query = context.Colors.Where(c => c.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(c => c.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = name,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 檢查顏色代碼是否已存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Colors.Where(c => c.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(c => c.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 根據代碼取得顏色資料
        /// </summary>
        public async Task<Color?> GetByCodeAsync(string code)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Colors
                    .Where(c => c.Code == code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByCodeAsync),
                    ServiceType = GetType().Name,
                    Code = code 
                });
                return null;
            }
        }

    }
}

