using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 尺寸服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SizeService : GenericManagementService<Size>, ISizeService
    {
        private readonly ILogger<SizeService> _logger;
        private readonly IErrorLogService _errorLogService;

        public SizeService(AppDbContext context, ILogger<SizeService> logger, IErrorLogService errorLogService) : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }

        #region 覆寫基底方法

        public override async Task<List<Size>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all sizes");
                throw;
            }
        }

        public override async Task<Size?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByIdAsync),
                    Id = id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting size by id {Id}", id);
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
                
                return await _dbSet
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted && 
                        (s.SizeCode.ToLower().Contains(lowerSearchTerm) ||
                         s.SizeName.ToLower().Contains(lowerSearchTerm) ||
                         (s.Description != null && s.Description.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching sizes with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Size entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(entity.SizeCode))
                {
                    errors.Add("尺寸代碼為必填欄位");
                }
                else
                {
                    // 檢查尺寸代碼唯一性
                    var isDuplicate = await IsSizeCodeExistsAsync(entity.SizeCode, entity.Id);
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
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating size entity {EntityId}", entity.Id);
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

                return await _dbSet
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.SizeCode == sizeCode && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetBySizeCodeAsync),
                    SizeCode = sizeCode,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting size by code {SizeCode}", sizeCode);
                throw;
            }
        }

        public async Task<bool> IsSizeCodeExistsAsync(string sizeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sizeCode))
                    return false;

                var query = _dbSet.Where(s => s.SizeCode == sizeCode && !s.IsDeleted);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(s => s.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsSizeCodeExistsAsync),
                    SizeCode = sizeCode,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking size code exists {SizeCode}", sizeCode);
                return false; // 安全預設值
            }
        }

        public async Task<List<Size>> GetActiveSizesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetActiveSizesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting active sizes");
                throw;
            }
        }

        public async Task<List<Size>> GetBySizeNameAsync(string sizeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sizeName))
                    return new List<Size>();

                return await _dbSet
                    .Include(s => s.Products)
                    .Where(s => !s.IsDeleted && s.SizeName.Contains(sizeName))
                    .OrderBy(s => s.SizeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetBySizeNameAsync),
                    SizeName = sizeName,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting sizes by name {SizeName}", sizeName);
                throw;
            }
        }

        #endregion

        #region 狀態管理

        public async Task<ServiceResult> UpdateActiveStatusAsync(int sizeId, bool isActive)
        {
            try
            {
                var size = await GetByIdAsync(sizeId);
                if (size == null)
                {
                    return ServiceResult.Failure("找不到指定的尺寸");
                }

                size.IsActive = isActive;
                size.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Size {SizeId} active status updated to {IsActive}", sizeId, isActive);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(UpdateActiveStatusAsync),
                    SizeId = sizeId,
                    IsActive = isActive,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error updating active status for size {SizeId}", sizeId);
                return ServiceResult.Failure("更新啟用狀態時發生錯誤");
            }
        }

        public async Task<ServiceResult> BatchUpdateActiveStatusAsync(List<int> sizeIds, bool isActive)
        {
            try
            {
                var sizes = await _dbSet
                    .Where(s => sizeIds.Contains(s.Id) && !s.IsDeleted)
                    .ToListAsync();

                if (!sizes.Any())
                {
                    return ServiceResult.Failure("找不到要更新的尺寸");
                }

                foreach (var size in sizes)
                {
                    size.IsActive = isActive;
                    size.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Batch updated active status for {Count} sizes to {IsActive}", sizes.Count, isActive);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(BatchUpdateActiveStatusAsync),
                    SizeIds = sizeIds,
                    IsActive = isActive,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error batch updating active status for sizes");
                return ServiceResult.Failure("批次更新啟用狀態時發生錯誤");
            }
        }

        #endregion
    }
}
