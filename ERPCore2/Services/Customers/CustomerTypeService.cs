using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶類型服務實作
    /// </summary>
    public class CustomerTypeService : GenericManagementService<CustomerType>, ICustomerTypeService
    {
        /// <summary>
        /// 完整建構子
        /// </summary>
        public CustomerTypeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<CustomerType>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子
        /// </summary>
        public CustomerTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    SearchTerm = searchTerm
                });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerType entity)
        {
            try
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
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    EntityTypeName = entity.TypeName
                });
                return ServiceResult.Failure("驗證客戶類型時發生錯誤");
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(ct => ct.TypeName == name && !ct.IsDeleted);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(ct => ct.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Name = name,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        #endregion

        #region 覆寫基底方法

        public override async Task<List<CustomerType>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Where(ct => !ct.IsDeleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { 
                    Id = id
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTypeNameExistsAsync), GetType(), _logger, new { 
                    TypeName = typeName,
                    ExcludeId = excludeId
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedAsync), GetType(), _logger, new { 
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBatchWithValidationAsync), GetType(), _logger, new { 
                    IdsCount = ids?.Count
                });
                return ServiceResult.Failure("批次刪除客戶類型時發生錯誤");
            }
        }

        #endregion
    }
}

