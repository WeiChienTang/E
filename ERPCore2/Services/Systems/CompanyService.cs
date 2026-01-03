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
                    .Where(c => (c.Code!.Contains(searchTerm) ||
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
                    .Where(c => c.Status == EntityStatus.Active)
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
                    .Where(c => c.Code == code)
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

        public async Task<bool> IsCompanyCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Companies.Where(c => c.Code == code);

                if (excludeId.HasValue)
                {
                    query = query.Where(c => c.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCompanyCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsCompanyCodeExistsAsync),
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
                    .Where(c => c.Status == EntityStatus.Active)
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
            return Regex.IsMatch(taxId, @"^\d{8}$");
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
                    return ServiceResult.Failure("公司編號為必填欄位");
                }

                // 檢查編號是否重複（排除自己）
                if (await IsCompanyCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                {
                    return ServiceResult.Failure("公司編號已存在");
                }

                // 驗證統一編號
                if (!ValidateTaxId(entity.TaxId))
                {
                    return ServiceResult.Failure("統一編號格式不正確，正確格式為8位數字");
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

        /// <summary>
        /// 更新公司 LOGO 路徑
        /// </summary>
        public async Task<ServiceResult> UpdateLogoPathAsync(int companyId, string logoPath)
        {
            try
            {
                if (companyId <= 0)
                {
                    return ServiceResult.Failure("公司 ID 無效");
                }

                if (string.IsNullOrWhiteSpace(logoPath))
                {
                    return ServiceResult.Failure("LOGO 路徑不能為空");
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var company = await context.Companies.FindAsync(companyId);
                
                if (company == null)
                {
                    return ServiceResult.Failure("找不到指定的公司");
                }

                // 更新 LOGO 路徑
                company.LogoPath = logoPath;
                company.UpdatedAt = DateTime.Now;
                company.UpdatedBy = "System";

                await context.SaveChangesAsync();
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateLogoPathAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateLogoPathAsync),
                    ServiceType = GetType().Name,
                    CompanyId = companyId,
                    LogoPath = logoPath
                });
                return ServiceResult.Failure("更新 LOGO 路徑時發生錯誤");
            }
        }
    }
}

