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
            return await _dbSet
                .Include(s => s.Products)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SizeName)
                .ToListAsync();
        }

        public override async Task<Size?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public override async Task<List<Size>> SearchAsync(string searchTerm)
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

        public override async Task<ServiceResult> ValidateAsync(Size entity)
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

        #endregion

        #region 業務特定查詢方法

        public async Task<Size?> GetBySizeCodeAsync(string sizeCode)
        {
            if (string.IsNullOrWhiteSpace(sizeCode))
                return null;

            return await _dbSet
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SizeCode == sizeCode && !s.IsDeleted);
        }

        public async Task<bool> IsSizeCodeExistsAsync(string sizeCode, int? excludeId = null)
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

        public async Task<List<Size>> GetActiveSizesAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.SizeName)
                .ToListAsync();
        }

        public async Task<List<Size>> GetBySizeNameAsync(string sizeName)
        {
            if (string.IsNullOrWhiteSpace(sizeName))
                return new List<Size>();

            return await _dbSet
                .Include(s => s.Products)
                .Where(s => !s.IsDeleted && s.SizeName.Contains(sizeName))
                .OrderBy(s => s.SizeName)
                .ToListAsync();
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
                _logger.LogError(ex, "Error updating active status for size {SizeId}", sizeId);
                return ServiceResult.Failure($"更新啟用狀態時發生錯誤: {ex.Message}");
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
                _logger.LogError(ex, "Error batch updating active status for sizes");
                return ServiceResult.Failure($"批次更新啟用狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion
    }
}
