using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 公司服務實作
    /// </summary>
    public class CompanyService : GenericManagementService<Company>, ICompanyService
    {
        public CompanyService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<Company>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<Company>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Companies
                    .Where(c => !c.IsDeleted &&
                        (c.Code!.Contains(searchTerm) ||
                         c.CompanyName.Contains(searchTerm) ||
                         (c.CompanyNameEn != null && c.CompanyNameEn.Contains(searchTerm)) ||
                         (c.TaxId != null && c.TaxId.Contains(searchTerm)) ||
                         (c.Representative != null && c.Representative.Contains(searchTerm))))
                    .OrderBy(c => c.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<Company>();
            }
        }

        public async Task<Company?> GetPrimaryCompanyAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 返回第一個啟用的公司
                return await context.Companies
                    .Where(c => !c.IsDeleted && c.Status == EntityStatus.Active)
                    .OrderBy(c => c.Code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryCompanyAsync), GetType(), _logger, new
                {
                    Method = nameof(GetPrimaryCompanyAsync),
                    ServiceType = GetType().Name
                });
                return null;
            }
        }

        public async Task<Company?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Companies
                    .Where(c => c.Code == code && !c.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCodeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCodeAsync),
                    ServiceType = GetType().Name,
                    Code = code
                });
                return null;
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Companies.Where(c => c.Code == code && !c.IsDeleted);

                if (excludeId.HasValue)
                {
                    query = query.Where(c => c.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<Company>> GetActiveCompaniesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Companies
                    .Where(c => !c.IsDeleted && c.Status == EntityStatus.Active)
                    .OrderBy(c => c.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveCompaniesAsync), GetType(), _logger, new
                {
                    Method = nameof(GetActiveCompaniesAsync),
                    ServiceType = GetType().Name
                });
                return new List<Company>();
            }
        }

        public bool ValidateTaxId(string? taxId)
        {
            if (string.IsNullOrWhiteSpace(taxId))
                return true; // 統一編號為選填

            // 台灣統一編號驗證：8位數字
            if (!Regex.IsMatch(taxId, @"^\d{8}$"))
                return false;

            // 統一編號檢查碼驗證演算法
            var weights = new int[] { 1, 2, 1, 2, 1, 2, 4, 1 };
            var sum = 0;

            for (int i = 0; i < 8; i++)
            {
                var digit = int.Parse(taxId[i].ToString());
                var product = digit * weights[i];
                
                // 如果乘積大於等於10，將十位數和個位數相加
                if (product >= 10)
                {
                    sum += (product / 10) + (product % 10);
                }
                else
                {
                    sum += product;
                }
            }

            return sum % 10 == 0;
        }

        public override async Task<ServiceResult> ValidateAsync(Company entity)
        {
            try
            {
                // 驗證必要欄位
                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                    return ServiceResult.Failure("公司名稱為必填欄位");
                }

                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    return ServiceResult.Failure("公司代碼為必填欄位");
                }

                // 檢查代碼是否重複（排除自己）
                if (await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                {
                    return ServiceResult.Failure("公司代碼已存在");
                }

                // 驗證統一編號
                if (!ValidateTaxId(entity.TaxId))
                {
                    return ServiceResult.Failure("統一編號格式不正確");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    Entity = entity
                });
                return ServiceResult.Failure("驗證公司資料時發生錯誤");
            }
        }

        public override async Task<ServiceResult<Company>> CreateAsync(Company entity)
        {
            try
            {
                return await base.CreateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    Entity = entity
                });
                return ServiceResult<Company>.Failure("建立公司時發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作公司特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Company entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckCompanyDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("公司"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    CompanyId = entity.Id 
                });
                return ServiceResult.Failure("檢查公司刪除條件時發生錯誤");
            }
        }
    }
}
