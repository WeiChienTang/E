using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址類型服務實作
    /// </summary>
    public class AddressTypeService : IAddressTypeService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AddressTypeService> _logger;

        public AddressTypeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AddressTypeService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<AddressType>> GetAllAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.Status != EntityStatus.Deleted)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all address types");
                throw;
            }
        }

        public async Task<List<AddressType>> GetActiveAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.Status == EntityStatus.Active)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active address types");
                throw;
            }
        }

        public async Task<AddressType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.AddressTypeId == id && at.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address type by id {AddressTypeId}", id);
                throw;
            }
        }

        public async Task<ServiceResult<AddressType>> CreateAsync(AddressType addressType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateAddressType(addressType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<AddressType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 檢查重複名稱
                var isDuplicate = await context.AddressTypes
                    .AnyAsync(at => at.TypeName == addressType.TypeName && at.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<AddressType>.Failure("地址類型名稱已存在");

                // 設定稽核欄位
                addressType.CreatedDate = DateTime.Now;
                addressType.CreatedBy = "System"; // TODO: 從認證取得使用者
                addressType.Status = EntityStatus.Active;

                context.AddressTypes.Add(addressType);
                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully created address type {TypeName} with ID {AddressTypeId}", 
                    addressType.TypeName, addressType.AddressTypeId);

                return ServiceResult<AddressType>.Success(addressType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address type {TypeName}", addressType.TypeName);
                return ServiceResult<AddressType>.Failure("建立地址類型時發生錯誤");
            }
        }

        public async Task<ServiceResult<AddressType>> UpdateAsync(AddressType addressType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateAddressType(addressType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<AddressType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 取得現有資料
                var existingAddressType = await context.AddressTypes
                    .Where(at => at.AddressTypeId == addressType.AddressTypeId && at.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (existingAddressType == null)
                    return ServiceResult<AddressType>.Failure("地址類型不存在");

                // 檢查重複名稱（排除自己）
                var isDuplicate = await context.AddressTypes
                    .AnyAsync(at => at.TypeName == addressType.TypeName && 
                                  at.AddressTypeId != addressType.AddressTypeId && 
                                  at.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<AddressType>.Failure("地址類型名稱已存在");

                // 更新屬性
                existingAddressType.TypeName = addressType.TypeName;
                existingAddressType.Description = addressType.Description;
                existingAddressType.ModifiedDate = DateTime.Now;
                existingAddressType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated address type {AddressTypeId}", addressType.AddressTypeId);

                return ServiceResult<AddressType>.Success(existingAddressType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address type {AddressTypeId}", addressType.AddressTypeId);
                return ServiceResult<AddressType>.Failure("更新地址類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var addressType = await context.AddressTypes
                    .Where(at => at.AddressTypeId == id && at.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (addressType == null)
                    return ServiceResult.Failure("地址類型不存在");

                // 檢查是否有關聯的客戶地址
                var hasRelatedAddresses = await context.CustomerAddresses
                    .AnyAsync(ca => ca.AddressTypeId == id && ca.Status != EntityStatus.Deleted);

                if (hasRelatedAddresses)
                    return ServiceResult.Failure("無法刪除，此地址類型已被客戶地址使用");

                // 軟刪除
                addressType.Status = EntityStatus.Deleted;
                addressType.ModifiedDate = DateTime.Now;
                addressType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted address type {AddressTypeId}", id);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address type {AddressTypeId}", id);
                return ServiceResult.Failure("刪除地址類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id, EntityStatus newStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var addressType = await context.AddressTypes
                    .Where(at => at.AddressTypeId == id && at.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (addressType == null)
                    return ServiceResult.Failure("地址類型不存在");

                addressType.Status = newStatus;
                addressType.ModifiedDate = DateTime.Now;
                addressType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for address type {AddressTypeId} to {Status}", 
                    id, newStatus);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for address type {AddressTypeId}", id);
                return ServiceResult.Failure("變更地址類型狀態時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var addressType = await context.AddressTypes
                    .Where(at => at.AddressTypeId == id && at.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (addressType == null)
                    return ServiceResult.Failure("地址類型不存在");

                // 切換狀態（Active <-> Inactive）
                addressType.Status = addressType.Status == EntityStatus.Active 
                    ? EntityStatus.Inactive 
                    : EntityStatus.Active;
                
                addressType.ModifiedDate = DateTime.Now;
                addressType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully toggled status for address type {AddressTypeId} to {Status}", 
                    id, addressType.Status);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for address type {AddressTypeId}", id);
                return ServiceResult.Failure("變更地址類型狀態時發生錯誤");
            }
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.AddressTypes
                    .Where(at => at.TypeName == name && at.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                    query = query.Where(at => at.AddressTypeId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if address type name exists {TypeName}", name);
                throw;
            }
        }

        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(typeName, excludeId);
        }

        public async Task<(List<AddressType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.AddressTypes
                    .Where(at => at.Status != EntityStatus.Deleted);

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
                _logger.LogError(ex, "Error getting paged address types for page {PageNumber}, size {PageSize}", 
                    pageNumber, pageSize);
                throw;
            }
        }

        private ServiceResult ValidateAddressType(AddressType addressType)
        {
            if (string.IsNullOrWhiteSpace(addressType.TypeName))
                return ServiceResult.Failure("地址類型名稱為必填");

            if (addressType.TypeName.Length > 20)
                return ServiceResult.Failure("地址類型名稱不可超過20個字元");

            if (!string.IsNullOrEmpty(addressType.Description) && addressType.Description.Length > 100)
                return ServiceResult.Failure("描述不可超過100個字元");

            return ServiceResult.Success();
        }
    }
}