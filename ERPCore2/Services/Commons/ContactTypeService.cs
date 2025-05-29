using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡類型服務實作
    /// </summary>
    public class ContactTypeService : IContactTypeService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ContactTypeService> _logger;

        public ContactTypeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ContactTypeService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<ContactType>> GetAllAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ContactTypes
                    .Where(ct => ct.Status != EntityStatus.Deleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contact types");
                throw;
            }
        }

        public async Task<List<ContactType>> GetActiveAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ContactTypes
                    .Where(ct => ct.Status == EntityStatus.Active)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contact types");
                throw;
            }
        }

        public async Task<ContactType?> GetByIdAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ContactTypes
                    .Where(ct => ct.ContactTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact type by id {ContactTypeId}", id);
                throw;
            }
        }

        public async Task<ServiceResult<ContactType>> CreateAsync(ContactType contactType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateContactType(contactType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<ContactType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 檢查重複名稱
                var isDuplicate = await context.ContactTypes
                    .AnyAsync(ct => ct.TypeName == contactType.TypeName && ct.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<ContactType>.Failure("聯絡類型名稱已存在");

                // 設定稽核欄位
                contactType.CreatedDate = DateTime.Now;
                contactType.CreatedBy = "System"; // TODO: 從認證取得使用者
                contactType.Status = EntityStatus.Active;

                context.ContactTypes.Add(contactType);
                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully created contact type {TypeName} with ID {ContactTypeId}", 
                    contactType.TypeName, contactType.ContactTypeId);

                return ServiceResult<ContactType>.Success(contactType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact type {TypeName}", contactType.TypeName);
                return ServiceResult<ContactType>.Failure("建立聯絡類型時發生錯誤");
            }
        }

        public async Task<ServiceResult<ContactType>> UpdateAsync(ContactType contactType)
        {
            try
            {
                // 驗證
                var validationResult = ValidateContactType(contactType);
                if (!validationResult.IsSuccess)
                    return ServiceResult<ContactType>.Failure(validationResult.ErrorMessage!);

                using var context = _contextFactory.CreateDbContext();
                
                // 取得現有資料
                var existingContactType = await context.ContactTypes
                    .Where(ct => ct.ContactTypeId == contactType.ContactTypeId && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (existingContactType == null)
                    return ServiceResult<ContactType>.Failure("聯絡類型不存在");

                // 檢查重複名稱（排除自己）
                var isDuplicate = await context.ContactTypes
                    .AnyAsync(ct => ct.TypeName == contactType.TypeName && 
                                  ct.ContactTypeId != contactType.ContactTypeId && 
                                  ct.Status != EntityStatus.Deleted);
                if (isDuplicate)
                    return ServiceResult<ContactType>.Failure("聯絡類型名稱已存在");

                // 更新屬性
                existingContactType.TypeName = contactType.TypeName;
                existingContactType.Description = contactType.Description;
                existingContactType.ModifiedDate = DateTime.Now;
                existingContactType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated contact type {ContactTypeId}", contactType.ContactTypeId);

                return ServiceResult<ContactType>.Success(existingContactType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact type {ContactTypeId}", contactType.ContactTypeId);
                return ServiceResult<ContactType>.Failure("更新聯絡類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var contactType = await context.ContactTypes
                    .Where(ct => ct.ContactTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (contactType == null)
                    return ServiceResult.Failure("聯絡類型不存在");

                // 檢查是否有關聯的客戶聯絡資料
                var hasRelatedContacts = await context.CustomerContacts
                    .AnyAsync(cc => cc.ContactTypeId == id && cc.Status != EntityStatus.Deleted);

                if (hasRelatedContacts)
                    return ServiceResult.Failure("無法刪除，此聯絡類型已被客戶聯絡資料使用");

                // 軟刪除
                contactType.Status = EntityStatus.Deleted;
                contactType.ModifiedDate = DateTime.Now;
                contactType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted contact type {ContactTypeId}", id);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact type {ContactTypeId}", id);
                return ServiceResult.Failure("刪除聯絡類型時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id, EntityStatus newStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var contactType = await context.ContactTypes
                    .Where(ct => ct.ContactTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (contactType == null)
                    return ServiceResult.Failure("聯絡類型不存在");

                contactType.Status = newStatus;
                contactType.ModifiedDate = DateTime.Now;
                contactType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for contact type {ContactTypeId} to {Status}", 
                    id, newStatus);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for contact type {ContactTypeId}", id);
                return ServiceResult.Failure("變更聯絡類型狀態時發生錯誤");
            }
        }

        public async Task<ServiceResult> ToggleStatusAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var contactType = await context.ContactTypes
                    .Where(ct => ct.ContactTypeId == id && ct.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();
                    
                if (contactType == null)
                    return ServiceResult.Failure("聯絡類型不存在");

                // 切換狀態（Active <-> Inactive）
                contactType.Status = contactType.Status == EntityStatus.Active 
                    ? EntityStatus.Inactive 
                    : EntityStatus.Active;
                
                contactType.ModifiedDate = DateTime.Now;
                contactType.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully toggled status for contact type {ContactTypeId} to {Status}", 
                    id, contactType.Status);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for contact type {ContactTypeId}", id);
                return ServiceResult.Failure("變更聯絡類型狀態時發生錯誤");
            }
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.ContactTypes
                    .Where(ct => ct.TypeName == name && ct.Status != EntityStatus.Deleted);

                if (excludeId.HasValue)
                    query = query.Where(ct => ct.ContactTypeId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if contact type name exists {TypeName}", name);
                throw;
            }
        }

        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(typeName, excludeId);
        }

        public async Task<(List<ContactType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var query = context.ContactTypes
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
                _logger.LogError(ex, "Error getting paged contact types for page {PageNumber}, size {PageSize}", 
                    pageNumber, pageSize);
                throw;
            }
        }

        private ServiceResult ValidateContactType(ContactType contactType)
        {
            if (string.IsNullOrWhiteSpace(contactType.TypeName))
                return ServiceResult.Failure("聯絡類型名稱為必填");

            if (contactType.TypeName.Length > 20)
                return ServiceResult.Failure("聯絡類型名稱不可超過20個字元");

            if (!string.IsNullOrEmpty(contactType.Description) && contactType.Description.Length > 100)
                return ServiceResult.Failure("描述不可超過100個字元");

            return ServiceResult.Success();
        }
    }
}
