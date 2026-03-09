using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class MinimumWageService : IMinimumWageService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<MinimumWageService>? _logger;

        public MinimumWageService(IDbContextFactory<AppDbContext> contextFactory, ILogger<MinimumWageService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<MinimumWage>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages
                    .OrderByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<MinimumWage>();
            }
        }

        public async Task<MinimumWage?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<MinimumWage?> GetEffectiveAsync(DateOnly date)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages
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

        public async Task<ServiceResult<MinimumWage>> CreateAsync(MinimumWage entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.MinimumWages.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<MinimumWage>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<MinimumWage>.Failure("新增基本工資時發生錯誤");
            }
        }

        public async Task<ServiceResult<MinimumWage>> UpdateAsync(MinimumWage entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.MinimumWages.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<MinimumWage>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<MinimumWage>.Failure("更新基本工資時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.MinimumWages.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的基本工資記錄");
                context.MinimumWages.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除基本工資時發生錯誤");
            }
        }
    }
}
