using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class HealthInsuranceGradeService : IHealthInsuranceGradeService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<HealthInsuranceGradeService>? _logger;

        public HealthInsuranceGradeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<HealthInsuranceGradeService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<HealthInsuranceGrade>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.HealthInsuranceGrades
                    .OrderByDescending(x => x.EffectiveDate)
                    .ThenBy(x => x.Grade)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<HealthInsuranceGrade>();
            }
        }

        public async Task<List<HealthInsuranceGrade>> GetByEffectiveDateAsync(DateOnly effectiveDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.HealthInsuranceGrades
                    .Where(x => x.EffectiveDate == effectiveDate)
                    .OrderBy(x => x.Grade)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEffectiveDateAsync), GetType(), _logger);
                return new List<HealthInsuranceGrade>();
            }
        }

        public async Task<List<DateOnly>> GetAvailableYearsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.HealthInsuranceGrades
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

        public async Task<HealthInsuranceGrade?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.HealthInsuranceGrades.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<HealthInsuranceGrade?> GetEffectiveGradeAsync(decimal salary, DateOnly date)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var effectiveDate = await context.HealthInsuranceGrades
                    .Where(x => x.EffectiveDate <= date)
                    .Select(x => x.EffectiveDate)
                    .OrderByDescending(x => x)
                    .FirstOrDefaultAsync();

                return await context.HealthInsuranceGrades
                    .Where(x => x.EffectiveDate == effectiveDate
                             && x.SalaryFrom <= salary
                             && (x.SalaryTo == null || x.SalaryTo >= salary))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEffectiveGradeAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult<HealthInsuranceGrade>> CreateAsync(HealthInsuranceGrade entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.HealthInsuranceGrades.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<HealthInsuranceGrade>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<HealthInsuranceGrade>.Failure("新增健保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult<HealthInsuranceGrade>> UpdateAsync(HealthInsuranceGrade entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.HealthInsuranceGrades.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<HealthInsuranceGrade>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<HealthInsuranceGrade>.Failure("更新健保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.HealthInsuranceGrades.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的健保分級記錄");
                context.HealthInsuranceGrades.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除健保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var source = await context.HealthInsuranceGrades
                    .Where(x => x.EffectiveDate == sourceDate)
                    .ToListAsync();

                if (!source.Any())
                    return ServiceResult.Failure("來源年度無資料");

                bool targetExists = await context.HealthInsuranceGrades
                    .AnyAsync(x => x.EffectiveDate == targetDate);
                if (targetExists)
                    return ServiceResult.Failure("目標年度已有資料，請先刪除後再複製");

                var copies = source.Select(g => new HealthInsuranceGrade
                {
                    Grade = g.Grade,
                    SalaryFrom = g.SalaryFrom,
                    SalaryTo = g.SalaryTo,
                    InsuredAmount = g.InsuredAmount,
                    EffectiveDate = targetDate
                }).ToList();

                context.HealthInsuranceGrades.AddRange(copies);
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
