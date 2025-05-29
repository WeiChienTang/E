using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 行業類型服務實作 - 使用 DbContextFactory 避免並發問題
    /// </summary>
    public class IndustryTypeService : IIndustryTypeService, IGenericManagementService<IndustryType>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<IndustryTypeService> _logger;

        public IndustryTypeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<IndustryTypeService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<IndustryType>> GetAllAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.IndustryTypes
                    .Where(it => it.Status != EntityStatus.Deleted)
                    .OrderBy(it => it.IndustryTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all industry types");
                throw;
            }
        }

        public async Task<List<IndustryType>> GetActiveAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.IndustryTypes
                    .Where(it => it.Status == EntityStatus.Active)
                    .OrderBy(it => it.IndustryTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active industry types");
                throw;
            }
        }

        public async Task<IndustryType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.IndustryTypes
                    .Where(it => it.IndustryTypeId == id && it.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting industry type by id {IndustryTypeId}", id);
                throw;
            }
        }

        public async Task<ServiceResult<IndustryType>> CreateAsync(IndustryType industryType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateIndustryType(industryType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<IndustryType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 檢查重複名稱
                var isDuplicate = await context.IndustryTypes
                    .AnyAsync(it => it.IndustryTypeName == industryType.IndustryTypeName && it.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<IndustryType>.Failure("行業類型名稱已存在");

                // 設定稽核欄位
                industryType.CreatedDate = DateTime.Now;
                industryType.CreatedBy = "System"; // TODO: 從認證取得使用者
                industryType.Status = EntityStatus.Active;

                context.IndustryTypes.Add(industryType);
                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully created industry type {IndustryTypeName} with ID {IndustryTypeId}", 
                    industryType.IndustryTypeName, industryType.IndustryTypeId);

                return ServiceResult<IndustryType>.Success(industryType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating industry type {IndustryTypeName}", industryType.IndustryTypeName);
                return ServiceResult<IndustryType>.Failure("建立行業類型時發生錯誤");
            }
        }

        public async Task<ServiceResult<IndustryType>> UpdateAsync(IndustryType industryType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateIndustryType(industryType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<IndustryType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 取得現有資料
                var existingIndustryType = await context.IndustryTypes
                    .Where(it => it.IndustryTypeId == industryType.IndustryTypeId && it.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (existingIndustryType == null)
                    return ServiceResult<IndustryType>.Failure("行業類型不存在");

                // 檢查重複名稱（排除自己）
                var isDuplicate = await context.IndustryTypes
                    .AnyAsync(it => it.IndustryTypeName == industryType.IndustryTypeName && 
                                  it.IndustryTypeId != industryType.IndustryTypeId && 
                                  it.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<IndustryType>.Failure("行業類型名稱已存在");

                // 更新屬性
                existingIndustryType.IndustryTypeName = industryType.IndustryTypeName;
                existingIndustryType.IndustryTypeCode = industryType.IndustryTypeCode;
                existingIndustryType.ModifiedDate = DateTime.Now;
                existingIndustryType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated industry type {IndustryTypeId}", industryType.IndustryTypeId);

                return ServiceResult<IndustryType>.Success(existingIndustryType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating industry type {IndustryTypeId}", industryType.IndustryTypeId);
                return ServiceResult<IndustryType>.Failure("更新行業類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var industryType = await context.IndustryTypes
                    .Where(it => it.IndustryTypeId == id && it.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (industryType == null)
                    return ServiceResult.Failure("行業類型不存在");

                // 檢查是否有關聯的客戶
                var hasRelatedCustomers = await context.Customers
                    .AnyAsync(c => c.IndustryTypeId == id && c.Status != EntityStatus.Deleted);

                if (hasRelatedCustomers)
                    return ServiceResult.Failure("無法刪除，此行業類型已被客戶使用");

                // 軟刪除
                industryType.Status = EntityStatus.Deleted;
                industryType.ModifiedDate = DateTime.Now;
                industryType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted industry type {IndustryTypeId}", id);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting industry type {IndustryTypeId}", id);
                return ServiceResult.Failure("刪除行業類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id, EntityStatus newStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var industryType = await context.IndustryTypes
                    .Where(it => it.IndustryTypeId == id && it.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (industryType == null)
                    return ServiceResult.Failure("行業類型不存在");

                industryType.Status = newStatus;
                industryType.ModifiedDate = DateTime.Now;
                industryType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for industry type {IndustryTypeId} to {Status}", 
                    id, newStatus);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for industry type {IndustryTypeId}", id);
                return ServiceResult.Failure("變更行業類型狀態時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var industryType = await context.IndustryTypes
                    .Where(it => it.IndustryTypeId == id && it.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (industryType == null)
                    return ServiceResult.Failure("行業類型不存在");

                // 切換狀態（Active <-> Inactive）
                industryType.Status = industryType.Status == EntityStatus.Active 
                    ? EntityStatus.Inactive 
                    : EntityStatus.Active;
                
                industryType.ModifiedDate = DateTime.Now;
                industryType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully toggled status for industry type {IndustryTypeId} to {Status}", 
                    id, industryType.Status);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for industry type {IndustryTypeId}", id);
                return ServiceResult.Failure("變更行業類型狀態時發生錯誤");
            }
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.IndustryTypes
                    .Where(it => it.IndustryTypeName == name && it.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                    query = query.Where(it => it.IndustryTypeId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if industry type name exists {IndustryTypeName}", name);
                throw;
            }
        }

        public async Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(industryTypeName, excludeId);
        }

        public async Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.IndustryTypes
                    .Where(it => it.Status != EntityStatus.Deleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(it => it.IndustryTypeName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged industry types for page {PageNumber}, size {PageSize}", 
                    pageNumber, pageSize);
                throw;
            }
        }

        private ServiceResult ValidateIndustryType(IndustryType industryType)
        {
            if (string.IsNullOrWhiteSpace(industryType.IndustryTypeName))
                return ServiceResult.Failure("行業類型名稱為必填");

            if (industryType.IndustryTypeName.Length > 30)
                return ServiceResult.Failure("行業類型名稱不可超過30個字元");

            if (!string.IsNullOrEmpty(industryType.IndustryTypeCode) && industryType.IndustryTypeCode.Length > 10)
                return ServiceResult.Failure("行業類型代碼不可超過10個字元");

            return ServiceResult.Success();
        }
    }
}
