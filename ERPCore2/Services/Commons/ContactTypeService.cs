using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡類型服務實作
    /// </summary>
    public class ContactTypeService : GenericManagementService<ContactType>, IContactTypeService
    {
        public ContactTypeService(AppDbContext context) : base(context)
        {
        }

        // 覆寫 GetAllAsync 以提供自訂排序
        public override async Task<List<ContactType>> GetAllAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();
        }        // 覆寫 GetActiveAsync 以提供自訂排序
        public override async Task<List<ContactType>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();
        }

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(ContactType entity)
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

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<ContactType>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(x => !x.IsDeleted && 
                           (x.TypeName.Contains(searchTerm) || 
                            (x.Description != null && x.Description.Contains(searchTerm))))
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();
        }

        // 覆寫 IsNameExistsAsync 以使用正確的屬性名稱
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(x => !x.IsDeleted && x.TypeName == name);
            
            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);
                
            return await query.AnyAsync();
        }

        // 覆寫 DeleteAsync 以檢查關聯資料
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            // 檢查是否有關聯的客戶聯絡資料
            var hasRelatedContacts = await _context.CustomerContacts
                .AnyAsync(cc => cc.ContactTypeId == id && !cc.IsDeleted);

            if (hasRelatedContacts)
                return ServiceResult.Failure("無法刪除，此聯絡類型已被客戶聯絡資料使用");

            // 使用基底類別的刪除邏輯
            return await base.DeleteAsync(id);
        }

        // 實作 IContactTypeService 特定方法
        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(typeName, excludeId);
        }

        public async Task<(List<ContactType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(x => !x.IsDeleted);
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderBy(ct => ct.TypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
