using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 行業類型服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class IndustryTypeService : GenericManagementService<IndustryType>, IIndustryTypeService
    {
        private readonly ILogger<IndustryTypeService> _logger;

        public IndustryTypeService(AppDbContext context, ILogger<IndustryTypeService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<IndustryType>> GetAllAsync()
        {
            return await _dbSet
                .Where(it => !it.IsDeleted)
                .OrderBy(it => it.IndustryTypeName)
                .ToListAsync();
        }

        public override async Task<List<IndustryType>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet
                .Where(it => !it.IsDeleted &&
                           (it.IndustryTypeName.ToLower().Contains(lowerSearchTerm) ||
                            (it.IndustryTypeCode != null && it.IndustryTypeCode.ToLower().Contains(lowerSearchTerm))))
                .OrderBy(it => it.IndustryTypeName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> ValidateAsync(IndustryType entity)
        {
            var errors = new List<string>();

            // 檢查必要欄位
            if (string.IsNullOrWhiteSpace(entity.IndustryTypeName))
                errors.Add("行業類型名稱為必填");

            // 檢查長度限制
            if (entity.IndustryTypeName?.Length > 100)
                errors.Add("行業類型名稱不可超過100個字元");

            if (!string.IsNullOrEmpty(entity.IndustryTypeCode) && entity.IndustryTypeCode.Length > 10)
                errors.Add("行業類型代碼不可超過10個字元");

            // 檢查名稱重複
            if (!string.IsNullOrWhiteSpace(entity.IndustryTypeName))
            {
                var isDuplicate = await _dbSet
                    .Where(it => it.IndustryTypeName == entity.IndustryTypeName && !it.IsDeleted)
                    .Where(it => it.Id != entity.Id) // 排除自己
                    .AnyAsync();

                if (isDuplicate)
                    errors.Add("行業類型名稱已存在");
            }

            // 檢查代碼重複
            if (!string.IsNullOrWhiteSpace(entity.IndustryTypeCode))
            {
                var isCodeDuplicate = await _dbSet
                    .Where(it => it.IndustryTypeCode == entity.IndustryTypeCode && !it.IsDeleted)
                    .Where(it => it.Id != entity.Id) // 排除自己
                    .AnyAsync();

                if (isCodeDuplicate)
                    errors.Add("行業類型代碼已存在");
            }

            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));

            return ServiceResult.Success();
        }

        protected override async Task<ServiceResult> CanDeleteAsync(IndustryType entity)
        {
            // 檢查是否有關聯的客戶
            var hasRelatedCustomers = await _context.Customers
                .AnyAsync(c => c.IndustryTypeId == entity.Id && !c.IsDeleted);

            if (hasRelatedCustomers)
                return ServiceResult.Failure("無法刪除，此行業類型已被客戶使用");

            return ServiceResult.Success();
        }        // 移除重複的 DeleteAsync 覆寫，使用基底類別的實作
        // 基底類別已提供完整的刪除功能，包含 CanDeleteAsync 的檢查

        #endregion

        #region 業務特定方法

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet
                    .Where(it => it.IndustryTypeName == name && !it.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(it => it.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if industry type name exists {IndustryTypeName}", name);
                throw;
            }
        }        public async Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null)
        {
            return await IsNameExistsAsync(industryTypeName, excludeId);
        }

        public async Task<bool> IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(industryTypeCode))
                    return false;

                var query = _dbSet
                    .Where(it => it.IndustryTypeCode == industryTypeCode && !it.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(it => it.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if industry type code exists {IndustryTypeCode}", industryTypeCode);
                throw;
            }
        }        public async Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await GetPagedAsync(pageNumber, pageSize, null);
        }

        #endregion
    }
}
