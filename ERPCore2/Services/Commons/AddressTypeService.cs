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
    /// 地址類型服務實作
    /// </summary>
    public class AddressTypeService : GenericManagementService<AddressType>, IAddressTypeService
    {
        public AddressTypeService(
            AppDbContext context, 
            ILogger<GenericManagementService<AddressType>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        // 覆寫 GetAllAsync 以提供自訂排序
        public override async Task<List<AddressType>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Where(x => !x.IsDeleted)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all address types");
                throw;
            }
        }

        // 覆寫 GetActiveAsync 以提供自訂排序
        public override async Task<List<AddressType>> GetActiveAsync()
        {
            try
            {
                return await _dbSet
                    .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetActiveAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting active address types");
                throw;
            }
        }

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(AddressType entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity.TypeName))
                    return ServiceResult.Failure("地址類型名稱為必填");

                if (entity.TypeName.Length > 20)
                    return ServiceResult.Failure("地址類型名稱不可超過20個字元");

                if (!string.IsNullOrEmpty(entity.Description) && entity.Description.Length > 100)
                    return ServiceResult.Failure("描述不可超過100個字元");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.TypeName, entity.Id))
                    return ServiceResult.Failure("地址類型名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating address type entity {EntityId}", entity.Id);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<AddressType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Where(at => !at.IsDeleted && 
                               (at.TypeName.Contains(searchTerm) || 
                                (at.Description != null && at.Description.Contains(searchTerm))))
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching address types with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        // 覆寫名稱檢查方法以實作實際邏輯
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(at => at.TypeName == name && !at.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(at => at.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if address type name exists {Name}", name);
                return false; // 安全預設值
            }
        }

        // 業務特定方法
        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            try
            {
                return await IsNameExistsAsync(typeName, excludeId);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsTypeNameExistsAsync),
                    TypeName = typeName,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if address type name exists {TypeName}", typeName);
                return false; // 安全預設值
            }
        }

        // 分頁查詢
        public async Task<(List<AddressType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _dbSet.Where(at => !at.IsDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(at => at.TypeName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetPagedAsync),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting paged address types page {PageNumber} size {PageSize}", pageNumber, pageSize);
                return (new List<AddressType>(), 0); // 安全預設值
            }
        }

        // 覆寫刪除前檢查以確保沒有相關地址
        protected override async Task<ServiceResult> CanDeleteAsync(AddressType entity)
        {
            try
            {
                var hasRelatedAddresses = await _context.CustomerAddresses
                    .AnyAsync(ca => ca.AddressTypeId == entity.Id && !ca.IsDeleted);

                if (hasRelatedAddresses)
                    return ServiceResult.Failure("無法刪除，此地址類型已被客戶地址使用");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if address type can be deleted {EntityId}", entity.Id);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        // 覆寫刪除方法以加入業務檢查
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("地址類型不存在");

                // 檢查是否可以刪除
                var canDeleteResult = await CanDeleteAsync(entity);
                if (!canDeleteResult.IsSuccess)
                    return canDeleteResult;

                // 呼叫基底類別的刪除方法
                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(DeleteAsync),
                    Id = id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error deleting address type {Id}", id);
                return ServiceResult.Failure("刪除過程發生錯誤");
            }
        }
    }
}