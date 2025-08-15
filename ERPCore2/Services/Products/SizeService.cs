using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 尺寸服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SizeService : GenericManagementService<Size>, ISizeService
    {
        // 完整建構子
        public SizeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Size>> logger) : base(contextFactory, logger)
        {
        }

        // 簡易建構子
        public SizeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<Size>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<Size?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Size>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted && 
                        ((s.Code != null && s.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (s.SizeName != null && s.SizeName.ToLower().Contains(lowerSearchTerm)) ||
                         (s.Description != null && s.Description.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Size entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("尺寸代碼為必填欄位");
                }
                else
                {
                    // 檢查尺寸代碼唯一性
                    var isDuplicate = await IsSizeCodeExistsAsync(entity.Code, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("尺寸代碼已存在");
                    }
                }

                if (string.IsNullOrWhiteSpace(entity.SizeName))
                {
                    errors.Add("尺寸名稱為必填欄位");
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

        public async Task<Size?> GetBySizeCodeAsync(string sizeCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sizeCode))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Code == sizeCode && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySizeCodeAsync), GetType(), _logger, new { SizeCode = sizeCode });
                throw;
            }
        }

        public async Task<bool> IsSizeCodeExistsAsync(string sizeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sizeCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Sizes.Where(s => s.Code == sizeCode && !s.IsDeleted);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(s => s.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSizeCodeExistsAsync), GetType(), _logger, new { SizeCode = sizeCode, ExcludeId = excludeId });
                return false; // 安全預設值
            }
        }

        public async Task<List<Size>> GetActiveSizesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Where(s => !s.IsDeleted && s.Status == EntityStatus.Active)
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveSizesAsync), GetType(), _logger);
                throw;
            }
        }

        public async Task<List<Size>> GetBySizeNameAsync(string sizeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sizeName))
                    return new List<Size>();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Sizes
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted && s.SizeName.Contains(sizeName))
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySizeNameAsync), GetType(), _logger, new { SizeName = sizeName });
                throw;
            }
        }

        #endregion
    }
}

