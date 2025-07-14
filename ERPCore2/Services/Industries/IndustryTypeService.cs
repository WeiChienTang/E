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
    /// 行業類型服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class IndustryTypeService : GenericManagementService<IndustryType>, IIndustryTypeService
    {
        public IndustryTypeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<IndustryType>> logger) : base(contextFactory, logger)
        {
        }

        public IndustryTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<IndustryType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.IndustryTypes
                    .Where(it => !it.IsDeleted)
                    .OrderBy(it => it.IndustryTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<List<IndustryType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();
                return await context.IndustryTypes
                    .Where(it => !it.IsDeleted &&
                               (it.IndustryTypeName.ToLower().Contains(lowerSearchTerm) ||
                                (it.IndustryTypeCode != null && it.IndustryTypeCode.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(it => it.IndustryTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, 
                    new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(IndustryType entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
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
                    var isDuplicate = await context.IndustryTypes
                        .Where(it => it.IndustryTypeName == entity.IndustryTypeName && !it.IsDeleted)
                        .Where(it => it.Id != entity.Id) // 排除自己
                        .AnyAsync();

                    if (isDuplicate)
                        errors.Add("行業類型名稱已存在");
                }

                // 檢查代碼重複
                if (!string.IsNullOrWhiteSpace(entity.IndustryTypeCode))
                {
                    var isCodeDuplicate = await context.IndustryTypes
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
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id, EntityName = entity.IndustryTypeName });
                throw;
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(IndustryType entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查是否有關聯的客戶
                var hasRelatedCustomers = await context.Customers
                    .AnyAsync(c => c.IndustryTypeId == entity.Id && !c.IsDeleted);

                if (hasRelatedCustomers)
                    return ServiceResult.Failure("無法刪除，此行業類型已被客戶使用");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger,
                    new { EntityId = entity.Id, EntityName = entity.IndustryTypeName });
                throw;
            }
        }        

        #endregion

        #region 業務特定方法

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.IndustryTypes
                    .Where(it => it.IndustryTypeName == name && !it.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(it => it.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger,
                    new { Name = name, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null)
        {
            try
            {
                return await IsNameExistsAsync(industryTypeName, excludeId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsIndustryTypeNameExistsAsync), GetType(), _logger,
                    new { IndustryTypeName = industryTypeName, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<bool> IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(industryTypeCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.IndustryTypes
                    .Where(it => it.IndustryTypeCode == industryTypeCode && !it.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(it => it.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsIndustryTypeCodeExistsAsync), GetType(), _logger,
                    new { IndustryTypeCode = industryTypeCode, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await GetPagedAsync(pageNumber, pageSize, null);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedAsync), GetType(), _logger,
                    new { PageNumber = pageNumber, PageSize = pageSize });
                throw;
            }
        }

        #endregion
    }
}

