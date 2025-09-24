using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 系統參數服務實作
    /// </summary>
    public class SystemParameterService : GenericManagementService<SystemParameter>, ISystemParameterService
    {
        public SystemParameterService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SystemParameter>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<SystemParameter>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemParameters
                    .Where(sp => (sp.TaxRate.ToString().Contains(searchTerm) ||
                         (sp.Remarks != null && sp.Remarks.Contains(searchTerm))))
                    .OrderBy(sp => sp.Id)
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
                return new List<SystemParameter>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SystemParameter entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.TaxRate < 0 || entity.TaxRate > 100)
                    errors.Add("稅率範圍必須在 0% 到 100% 之間");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    TaxRate = entity.TaxRate
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<decimal> GetTaxRateAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var systemParameter = await context.SystemParameters
                    .Where(sp => sp.Status == EntityStatus.Active)
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();

                return systemParameter?.TaxRate ?? 5.00m; // 預設 5% 稅率
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTaxRateAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTaxRateAsync),
                    ServiceType = GetType().Name
                });
                return 5.00m; // 發生錯誤時返回預設稅率
            }
        }

        public async Task<bool> SetTaxRateAsync(decimal taxRate)
        {
            try
            {
                if (taxRate < 0 || taxRate > 100)
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var systemParameter = await context.SystemParameters
                    .AsQueryable()
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();

                if (systemParameter == null)
                {
                    // 如果不存在系統參數，創建一個新的
                    systemParameter = new SystemParameter
                    {
                        TaxRate = taxRate,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        Remarks = "系統自動創建的稅率設定"
                    };
                    await context.SystemParameters.AddAsync(systemParameter);
                }
                else
                {
                    systemParameter.TaxRate = taxRate;
                    systemParameter.UpdatedAt = DateTime.UtcNow;
                    systemParameter.UpdatedBy = "System";
                    context.SystemParameters.Update(systemParameter);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetTaxRateAsync), GetType(), _logger, new
                {
                    Method = nameof(SetTaxRateAsync),
                    ServiceType = GetType().Name,
                    TaxRate = taxRate
                });
                return false;
            }
        }

        public async Task<SystemParameter?> GetSystemParameterAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemParameters
                    .Where(sp => sp.Status == EntityStatus.Active)
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSystemParameterAsync), GetType(), _logger, new
                {
                    Method = nameof(GetSystemParameterAsync),
                    ServiceType = GetType().Name
                });
                return null;
            }
        }
    }
}
