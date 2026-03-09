using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class WithholdingTaxTableService : IWithholdingTaxTableService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<WithholdingTaxTableService>? _logger;

        public WithholdingTaxTableService(IDbContextFactory<AppDbContext> contextFactory, ILogger<WithholdingTaxTableService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<WithholdingTaxTable>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WithholdingTaxTables
                    .OrderByDescending(x => x.EffectiveDate)
                    .ThenBy(x => x.DependentCount)
                    .ThenBy(x => x.SalaryFrom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<WithholdingTaxTable>();
            }
        }

        public async Task<List<WithholdingTaxTable>> GetByEffectiveDateAsync(DateOnly effectiveDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WithholdingTaxTables
                    .Where(x => x.EffectiveDate == effectiveDate)
                    .OrderBy(x => x.DependentCount)
                    .ThenBy(x => x.SalaryFrom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEffectiveDateAsync), GetType(), _logger);
                return new List<WithholdingTaxTable>();
            }
        }

        public async Task<List<DateOnly>> GetAvailableYearsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WithholdingTaxTables
                    .Select(x => x.EffectiveDate)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableYearsAsync), GetType(), _logger);
                return new List<DateOnly>();
            }
        }

        public async Task<WithholdingTaxTable?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WithholdingTaxTables.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult<WithholdingTaxTable>> CreateAsync(WithholdingTaxTable entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.WithholdingTaxTables.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<WithholdingTaxTable>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<WithholdingTaxTable>.Failure("新增扣繳稅額記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult<WithholdingTaxTable>> UpdateAsync(WithholdingTaxTable entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.WithholdingTaxTables.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<WithholdingTaxTable>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<WithholdingTaxTable>.Failure("更新扣繳稅額記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.WithholdingTaxTables.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的扣繳稅額記錄");
                context.WithholdingTaxTables.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除扣繳稅額記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var source = await context.WithholdingTaxTables
                    .Where(x => x.EffectiveDate == sourceDate)
                    .ToListAsync();

                if (!source.Any())
                    return ServiceResult.Failure("來源年度無資料");

                bool targetExists = await context.WithholdingTaxTables
                    .AnyAsync(x => x.EffectiveDate == targetDate);
                if (targetExists)
                    return ServiceResult.Failure("目標年度已有資料，請先刪除後再複製");

                var copies = source.Select(t => new WithholdingTaxTable
                {
                    SalaryFrom = t.SalaryFrom,
                    SalaryTo = t.SalaryTo,
                    DependentCount = t.DependentCount,
                    TaxAmount = t.TaxAmount,
                    EffectiveDate = targetDate
                }).ToList();

                context.WithholdingTaxTables.AddRange(copies);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyYearAsync), GetType(), _logger);
                return ServiceResult.Failure("複製年度資料時發生錯誤");
            }
        }
    }
}
