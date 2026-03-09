using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class InsuranceRateService : IInsuranceRateService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<InsuranceRateService>? _logger;

        public InsuranceRateService(IDbContextFactory<AppDbContext> contextFactory, ILogger<InsuranceRateService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<InsuranceRate>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InsuranceRates
                    .OrderByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<InsuranceRate>();
            }
        }

        public async Task<InsuranceRate?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InsuranceRates.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<InsuranceRate?> GetEffectiveAsync(DateOnly date)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InsuranceRates
                    .Where(x => x.EffectiveDate <= date)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEffectiveAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult<InsuranceRate>> CreateAsync(InsuranceRate entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.InsuranceRates.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<InsuranceRate>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<InsuranceRate>.Failure("新增保費費率時發生錯誤");
            }
        }

        public async Task<ServiceResult<InsuranceRate>> UpdateAsync(InsuranceRate entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.InsuranceRates.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<InsuranceRate>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<InsuranceRate>.Failure("更新保費費率時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.InsuranceRates.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的保費費率記錄");
                context.InsuranceRates.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除保費費率時發生錯誤");
            }
        }
    }
}
