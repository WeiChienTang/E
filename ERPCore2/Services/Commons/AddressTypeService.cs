using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址類型服務實作
    /// </summary>
    public class AddressTypeService : GenericManagementService<AddressType>, IAddressTypeService
    {
        public AddressTypeService(AppDbContext context) : base(context)
        {
        }

        // 覆寫 GetAllAsync 以提供自訂排序
        public override async Task<List<AddressType>> GetAllAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .OrderBy(at => at.TypeName)
                .ToListAsync();
        }

        // 覆寫 GetActiveAsync 以提供自訂排序
        public override async Task<List<AddressType>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
                .OrderBy(at => at.TypeName)
                .ToListAsync();
        }

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(AddressType entity)
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

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<AddressType>> SearchAsync(string searchTerm)
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

        // 覆寫名稱檢查方法以實作實際邏輯
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(at => at.TypeName == name && !at.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(at => at.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        // 業務特定方法
        public async Task<bool> IsTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(typeName, excludeId);
        }

        // 分頁查詢
        public async Task<(List<AddressType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
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

        // 覆寫刪除前檢查以確保沒有相關地址
        protected override async Task<ServiceResult> CanDeleteAsync(AddressType entity)
        {
            var hasRelatedAddresses = await _context.CustomerAddresses
                .AnyAsync(ca => ca.AddressTypeId == entity.Id && !ca.IsDeleted);

            if (hasRelatedAddresses)
                return ServiceResult.Failure("無法刪除，此地址類型已被客戶地址使用");

            return ServiceResult.Success();
        }

        // 覆寫刪除方法以加入業務檢查
        public override async Task<ServiceResult> DeleteAsync(int id)
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
    }
}