using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶類型服務實作
    /// </summary>
    public class CustomerTypeService : GenericManagementService<CustomerType>, ICustomerTypeService
    {
        private readonly ILogger<CustomerTypeService> _logger;
        private readonly IErrorLogService _errorLogService;

        public CustomerTypeService(AppDbContext context, ILogger<CustomerTypeService> logger, IErrorLogService errorLogService)
            : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }

        #region 覆寫基底抽象方法

        public override async Task<List<CustomerType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Where(ct => !ct.IsDeleted &&
                               (ct.TypeName.Contains(searchTerm) ||
                                (ct.Description != null && ct.Description.Contains(searchTerm))))
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching customer types with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerType entity)
        {
            var errors = new List<string>();

            // 驗證名稱
            if (string.IsNullOrWhiteSpace(entity.TypeName))
            {
                errors.Add("客戶類型名稱不能為空");
            }
            else if (entity.TypeName.Length > 100)
            {
                errors.Add("客戶類型名稱長度不能超過100個字元");
            }
            else
            {
                // 檢查名稱是否重複
                var isDuplicate = await IsNameExistsAsync(entity.TypeName, entity.Id > 0 ? entity.Id : null);
                if (isDuplicate)
                {
                    errors.Add("客戶類型名稱已存在");
                }
            }

            // 驗證描述
            if (!string.IsNullOrWhiteSpace(entity.Description) && entity.Description.Length > 500)
            {
                errors.Add("描述長度不能超過500個字元");
            }

            return errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(ct => ct.TypeName == name && !ct.IsDeleted);
            
            if (excludeId.HasValue)
            {
                query = query.Where(ct => ct.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #endregion

        #region 覆寫基底方法

        public override async Task<List<CustomerType>> GetAllAsync()
        {
            return await _dbSet
                .Where(ct => !ct.IsDeleted)
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到要刪除的客戶類型");
                }                // 檢查是否有客戶使用此類型
                var hasCustomers = await _context.Customers
                    .AnyAsync(c => c.CustomerTypeId == id && !c.IsDeleted);

                if (hasCustomers)
                {
                    return ServiceResult.Failure("此客戶類型正在被使用，無法刪除");
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer type with ID {Id}", id);
                return ServiceResult.Failure("刪除客戶類型時發生錯誤");
            }
        }

        #endregion

        #region ICustomerTypeService 特定方法

        /// <summary>
        /// 檢查客戶類型名稱是否存在
        /// </summary>
        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(ct => ct.TypeName == typeName && !ct.IsDeleted);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(ct => ct.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer type name exists: {TypeName}", typeName);
                return false;
            }
        }        /// <summary>
        /// 取得分頁的客戶類型資料
        /// </summary>
        public async Task<(List<CustomerType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _dbSet.Where(ct => !ct.IsDeleted);

                var totalCount = await query.CountAsync();
                
                var items = await query
                    .OrderBy(ct => ct.TypeName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged customer types. Page: {PageNumber}, Size: {PageSize}", 
                    pageNumber, pageSize);
                return (new List<CustomerType>(), 0);
            }
        }

        /// <summary>
        /// 批次刪除客戶類型（檢查是否有關聯的客戶）
        /// </summary>
        public async Task<ServiceResult> DeleteBatchWithValidationAsync(List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return ServiceResult.Failure("沒有指定要刪除的客戶類型");
                }                // 檢查是否有客戶使用這些類型
                var usedTypeIds = await _context.Customers
                    .Where(c => c.CustomerTypeId.HasValue && ids.Contains(c.CustomerTypeId.Value) && !c.IsDeleted)
                    .Select(c => c.CustomerTypeId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (usedTypeIds.Any())
                {
                    var usedTypeNames = await _context.CustomerTypes
                        .Where(ct => usedTypeIds.Contains(ct.Id))
                        .Select(ct => ct.TypeName)
                        .ToListAsync();

                    return ServiceResult.Failure($"以下客戶類型正在被使用，無法刪除: {string.Join(", ", usedTypeNames)}");
                }

                // 執行批次刪除
                return await DeleteBatchAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch deleting customer types");
                return ServiceResult.Failure("批次刪除客戶類型時發生錯誤");
            }
        }

        #endregion
    }
}
