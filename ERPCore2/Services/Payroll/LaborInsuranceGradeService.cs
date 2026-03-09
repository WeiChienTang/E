using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    public class LaborInsuranceGradeService : ILaborInsuranceGradeService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<LaborInsuranceGradeService>? _logger;

        public LaborInsuranceGradeService(IDbContextFactory<AppDbContext> contextFactory, ILogger<LaborInsuranceGradeService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<LaborInsuranceGrade>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.LaborInsuranceGrades
                    .OrderByDescending(x => x.EffectiveDate)
                    .ThenBy(x => x.Grade)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<LaborInsuranceGrade>();
            }
        }

        public async Task<List<LaborInsuranceGrade>> GetByEffectiveDateAsync(DateOnly effectiveDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.LaborInsuranceGrades
                    .Where(x => x.EffectiveDate == effectiveDate)
                    .OrderBy(x => x.Grade)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEffectiveDateAsync), GetType(), _logger);
                return new List<LaborInsuranceGrade>();
            }
        }

        public async Task<List<DateOnly>> GetAvailableYearsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.LaborInsuranceGrades
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

        public async Task<LaborInsuranceGrade?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.LaborInsuranceGrades.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<LaborInsuranceGrade?> GetEffectiveGradeAsync(decimal salary, DateOnly date)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 取得該日期適用版本中薪資落在範圍內的等級
                var effectiveDate = await context.LaborInsuranceGrades
                    .Where(x => x.EffectiveDate <= date)
                    .Select(x => x.EffectiveDate)
                    .OrderByDescending(x => x)
                    .FirstOrDefaultAsync();

                return await context.LaborInsuranceGrades
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

        public async Task<ServiceResult<LaborInsuranceGrade>> CreateAsync(LaborInsuranceGrade entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.LaborInsuranceGrades.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<LaborInsuranceGrade>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<LaborInsuranceGrade>.Failure("新增勞保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult<LaborInsuranceGrade>> UpdateAsync(LaborInsuranceGrade entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.LaborInsuranceGrades.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<LaborInsuranceGrade>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<LaborInsuranceGrade>.Failure("更新勞保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.LaborInsuranceGrades.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的勞保分級記錄");
                context.LaborInsuranceGrades.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除勞保分級時發生錯誤");
            }
        }

        public async Task<ServiceResult> CopyYearAsync(DateOnly sourceDate, DateOnly targetDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var source = await context.LaborInsuranceGrades
                    .Where(x => x.EffectiveDate == sourceDate)
                    .ToListAsync();

                if (!source.Any())
                    return ServiceResult.Failure("來源年度無資料");

                bool targetExists = await context.LaborInsuranceGrades
                    .AnyAsync(x => x.EffectiveDate == targetDate);
                if (targetExists)
                    return ServiceResult.Failure("目標年度已有資料，請先刪除後再複製");

                var copies = source.Select(g => new LaborInsuranceGrade
                {
                    Grade = g.Grade,
                    SalaryFrom = g.SalaryFrom,
                    SalaryTo = g.SalaryTo,
                    InsuredSalary = g.InsuredSalary,
                    EffectiveDate = targetDate
                }).ToList();

                context.LaborInsuranceGrades.AddRange(copies);
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
