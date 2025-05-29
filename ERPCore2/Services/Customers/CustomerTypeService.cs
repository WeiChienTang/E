using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶類型服務實作 - 使用 DbContextFactory 避免並發問題
    /// </summary>
    public class CustomerTypeService : ICustomerTypeService, IGenericManagementService<CustomerType>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<CustomerTypeService> _logger;

        public CustomerTypeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CustomerTypeService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<CustomerType>> GetAllAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerTypes
                    .Where(ct => ct.Status != EntityStatus.Deleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customer types");
                throw;
            }
        }

        public async Task<List<CustomerType>> GetActiveAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerTypes
                    .Where(ct => ct.Status == EntityStatus.Active)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active customer types");
                throw;
            }
        }

        public async Task<CustomerType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerTypes
                    .Where(ct => ct.CustomerTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer type by id {CustomerTypeId}", id);
                throw;
            }
        }

        public async Task<ServiceResult<CustomerType>> CreateAsync(CustomerType customerType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateCustomerType(customerType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 檢查重複名稱
                var isDuplicate = await context.CustomerTypes
                    .AnyAsync(ct => ct.TypeName == customerType.TypeName && ct.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<CustomerType>.Failure("客戶類型名稱已存在");

                // 設定稽核欄位
                customerType.CreatedDate = DateTime.Now;
                customerType.CreatedBy = "System"; // TODO: 從認證取得使用者
                customerType.Status = EntityStatus.Active;

                context.CustomerTypes.Add(customerType);
                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully created customer type {TypeName} with ID {CustomerTypeId}", 
                    customerType.TypeName, customerType.CustomerTypeId);

                return ServiceResult<CustomerType>.Success(customerType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer type {TypeName}", customerType.TypeName);
                return ServiceResult<CustomerType>.Failure("建立客戶類型時發生錯誤");
            }
        }

        public async Task<ServiceResult<CustomerType>> UpdateAsync(CustomerType customerType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateCustomerType(customerType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 取得現有資料
                var existingCustomerType = await context.CustomerTypes
                    .Where(ct => ct.CustomerTypeId == customerType.CustomerTypeId && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (existingCustomerType == null)
                    return ServiceResult<CustomerType>.Failure("客戶類型不存在");

                // 檢查重複名稱（排除自己）
                var isDuplicate = await context.CustomerTypes
                    .AnyAsync(ct => ct.TypeName == customerType.TypeName && 
                                  ct.CustomerTypeId != customerType.CustomerTypeId && 
                                  ct.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<CustomerType>.Failure("客戶類型名稱已存在");

                // 更新屬性
                existingCustomerType.TypeName = customerType.TypeName;
                existingCustomerType.Description = customerType.Description;
                existingCustomerType.ModifiedDate = DateTime.Now;
                existingCustomerType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated customer type {CustomerTypeId}", customerType.CustomerTypeId);

                return ServiceResult<CustomerType>.Success(existingCustomerType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer type {CustomerTypeId}", customerType.CustomerTypeId);
                return ServiceResult<CustomerType>.Failure("更新客戶類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var customerType = await context.CustomerTypes
                    .Where(ct => ct.CustomerTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (customerType == null)
                    return ServiceResult.Failure("客戶類型不存在");

                // 檢查是否有關聯的客戶
                var hasRelatedCustomers = await context.Customers
                    .AnyAsync(c => c.CustomerTypeId == id && c.Status != EntityStatus.Deleted);

                if (hasRelatedCustomers)
                    return ServiceResult.Failure("無法刪除，此客戶類型已被客戶使用");

                // 軟刪除
                customerType.Status = EntityStatus.Deleted;
                customerType.ModifiedDate = DateTime.Now;
                customerType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted customer type {CustomerTypeId}", id);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer type {CustomerTypeId}", id);
                return ServiceResult.Failure("刪除客戶類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id, EntityStatus newStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var customerType = await context.CustomerTypes
                    .Where(ct => ct.CustomerTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (customerType == null)
                    return ServiceResult.Failure("客戶類型不存在");

                customerType.Status = newStatus;
                customerType.ModifiedDate = DateTime.Now;
                customerType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for customer type {CustomerTypeId} to {Status}", 
                    id, newStatus);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for customer type {CustomerTypeId}", id);
                return ServiceResult.Failure("變更客戶類型狀態時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var customerType = await context.CustomerTypes
                    .Where(ct => ct.CustomerTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (customerType == null)
                    return ServiceResult.Failure("客戶類型不存在");

                // 切換狀態（Active <-> Inactive）
                customerType.Status = customerType.Status == EntityStatus.Active 
                    ? EntityStatus.Inactive 
                    : EntityStatus.Active;
                
                customerType.ModifiedDate = DateTime.Now;
                customerType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully toggled status for customer type {CustomerTypeId} to {Status}", 
                    id, customerType.Status);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for customer type {CustomerTypeId}", id);
                return ServiceResult.Failure("變更客戶類型狀態時發生錯誤");
            }
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.CustomerTypes
                    .Where(ct => ct.TypeName == name && ct.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                    query = query.Where(ct => ct.CustomerTypeId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer type name exists {TypeName}", name);
                throw;
            }
        }

        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(typeName, excludeId);
        }

        public async Task<(List<CustomerType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.CustomerTypes
                    .Where(ct => ct.Status != EntityStatus.Deleted);

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
                _logger.LogError(ex, "Error getting paged customer types for page {PageNumber}, size {PageSize}", 
                    pageNumber, pageSize);
                throw;
            }
        }

        private ServiceResult ValidateCustomerType(CustomerType customerType)
        {
            if (string.IsNullOrWhiteSpace(customerType.TypeName))
                return ServiceResult.Failure("客戶類型名稱為必填");

            if (customerType.TypeName.Length > 20)
                return ServiceResult.Failure("客戶類型名稱不可超過20個字元");

            if (!string.IsNullOrEmpty(customerType.Description) && customerType.Description.Length > 100)
                return ServiceResult.Failure("描述不可超過100個字元");

            return ServiceResult.Success();
        }
    }
}
