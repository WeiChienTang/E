using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 材質服務實作
    /// </summary>
    public class MaterialService : GenericManagementService<Material>, IMaterialService
    {
        public MaterialService(IDbContextFactory<AppDbContext> contextFactory, ILogger<GenericManagementService<Material>> logger) : base(contextFactory, logger)
        {
        }

        public MaterialService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料方法，包含供應商關聯資料並加入排序
        /// </summary>
        public override async Task<List<Material>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Materials
                    .AsQueryable()
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Material>();
            }
        }

        /// <summary>
        /// 覆寫取得單一資料方法，包含供應商關聯資料
        /// </summary>
        public override async Task<Material?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Materials
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        /// <summary>
        /// 覆寫搜尋方法，實作材質特定的搜尋邏輯
        /// </summary>
        public override async Task<List<Material>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Materials
                    .Where(m => ((m.Name != null && m.Name.Contains(searchTerm)) ||
                                (m.Code != null && m.Code.Contains(searchTerm)) ||
                                (m.Description != null && m.Description.Contains(searchTerm))))
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<Material>();
            }
        }

        /// <summary>
        /// 覆寫驗證方法，添加材質特定的驗證規則
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Material entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("材質名稱為必填");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("材質編號為必填");

                // 檢查編號是否重複
                if (await IsCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("材質編號已存在");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.Name, entity.Id))
                    return ServiceResult.Failure("材質名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.Name,
                    EntityCode = entity.Code
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
                var query = context.Materials.Where(m => m.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(m => m.Id != excludeId.Value);

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
        /// 檢查材質編號是否已存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Materials.Where(m => m.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(m => m.Id != excludeId.Value);

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
        /// 根據編號取得材質資料
        /// </summary>
        public async Task<Material?> GetByCodeAsync(string code)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Materials
                    .Where(m => m.Code == code)
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

