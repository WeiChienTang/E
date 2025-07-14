using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡類型服務實作
    /// </summary>
    public class ContactTypeService : GenericManagementService<ContactType>, IContactTypeService
    {
        public ContactTypeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<ContactType>> logger) : base(contextFactory, logger)
        {
        }

        public ContactTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以提供自訂排序
        public override async Task<List<ContactType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ContactTypes
                    .Where(x => !x.IsDeleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        // 覆寫 GetActiveAsync 以提供自訂排序
        public override async Task<List<ContactType>> GetActiveAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ContactTypes
                    .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveAsync), GetType(), _logger, new { 
                    Method = nameof(GetActiveAsync),
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(ContactType entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity.TypeName))
                    return ServiceResult.Failure("聯絡類型名稱為必填");

                if (entity.TypeName.Length > 20)
                    return ServiceResult.Failure("聯絡類型名稱不可超過20個字元");

                if (!string.IsNullOrEmpty(entity.Description) && entity.Description.Length > 100)
                    return ServiceResult.Failure("描述不可超過100個字元");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.TypeName, entity.Id))
                    return ServiceResult.Failure("聯絡類型名稱已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<ContactType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ContactTypes
                    .Where(x => !x.IsDeleted && 
                               (x.TypeName.Contains(searchTerm) || 
                                (x.Description != null && x.Description.Contains(searchTerm))))
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        // 覆寫 IsNameExistsAsync 以使用正確的屬性名稱
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ContactTypes.Where(x => !x.IsDeleted && x.TypeName == name);
                
                if (excludeId.HasValue)
                    query = query.Where(x => x.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false; // 安全預設值
            }
        }

        // 覆寫 DeleteAsync 以檢查關聯資料
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查是否有關聯的客戶聯絡資料
                var hasRelatedContacts = await context.CustomerContacts
                    .AnyAsync(cc => cc.ContactTypeId == id && !cc.IsDeleted);

                if (hasRelatedContacts)
                    return ServiceResult.Failure("無法刪除，此聯絡類型已被客戶聯絡資料使用");

                // 使用基底類別的刪除邏輯
                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteAsync),
                    Id = id,
                    ServiceType = GetType().Name 
                });
                return ServiceResult.Failure("刪除過程發生錯誤");
            }
        }

        // 實作 IContactTypeService 特定方法
        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            try
            {
                return await IsNameExistsAsync(typeName, excludeId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTypeNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsTypeNameExistsAsync),
                    TypeName = typeName,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false; // 安全預設值
            }
        }

        public async Task<(List<ContactType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ContactTypes.Where(x => !x.IsDeleted);
                
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
                    Method = nameof(GetPagedAsync),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ServiceType = GetType().Name 
                });
                return (new List<ContactType>(), 0); // 安全預設值
            }
        }
    }
}

