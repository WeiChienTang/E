using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 業績目標服務實作
    /// </summary>
    public class SalesTargetService : GenericManagementService<SalesTarget>, ISalesTargetService
    {
        public SalesTargetService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesTarget>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<SalesTarget> BuildGetAllQuery(AppDbContext context)
        {
            return context.SalesTargets
                .Include(t => t.Salesperson)
                .Where(t => t.Status != EntityStatus.Deleted)
                .OrderByDescending(t => t.Year)
                .ThenBy(t => t.Month)
                .ThenBy(t => t.Salesperson != null ? t.Salesperson.Name : "");
        }

        public override async Task<SalesTarget?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesTargets
                    .Include(t => t.Salesperson)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger,
                    new { Id = id });
                return null;
            }
        }

        public override async Task<List<SalesTarget>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesTargets
                    .Include(t => t.Salesperson)
                    .Where(t => t.Status != EntityStatus.Deleted
                             && (t.Year.ToString().Contains(searchTerm)
                             || (t.Salesperson != null && t.Salesperson.Name != null && t.Salesperson.Name.Contains(searchTerm))))
                    .OrderByDescending(t => t.Year)
                    .ThenBy(t => t.Month)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<SalesTarget>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesTarget entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.Year < 2000 || entity.Year > 2100)
                    errors.Add("年度範圍不正確（2000–2100）");

                if (entity.Month.HasValue && (entity.Month < 1 || entity.Month > 12))
                    errors.Add("月份需為 1–12");

                if (entity.TargetAmount < 0)
                    errors.Add("目標金額不可為負數");

                // 防止重複（同年月+同業務員）
                var exists = await IsTargetExistsAsync(entity.Year, entity.Month, entity.SalespersonId,
                    entity.Id > 0 ? entity.Id : null);
                if (exists)
                    errors.Add("相同年月與業務員的目標已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("\n", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        public async Task<List<SalesTarget>> GetByYearAsync(int year)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SalesTargets
                .Include(t => t.Salesperson)
                .Where(t => t.Status != EntityStatus.Deleted && t.Year == year)
                .OrderBy(t => t.Month)
                .ThenBy(t => t.Salesperson != null ? t.Salesperson.Name : "")
                .ToListAsync();
        }

        public async Task<List<SalesTarget>> GetByYearMonthAsync(int year, int? month)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SalesTargets
                .Include(t => t.Salesperson)
                .Where(t => t.Status != EntityStatus.Deleted && t.Year == year && t.Month == month)
                .OrderBy(t => t.Salesperson != null ? t.Salesperson.Name : "")
                .ToListAsync();
        }

        public async Task<List<SalesTarget>> GetBySalespersonAsync(int salespersonId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SalesTargets
                .Include(t => t.Salesperson)
                .Where(t => t.Status != EntityStatus.Deleted && t.SalespersonId == salespersonId)
                .OrderByDescending(t => t.Year)
                .ThenBy(t => t.Month)
                .ToListAsync();
        }

        public async Task<List<SalesAchievementItem>> GetAchievementAsync(int year, int? month = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var targets = await context.SalesTargets
                .Include(t => t.Salesperson)
                .Where(t => t.Status != EntityStatus.Deleted
                         && t.Year == year
                         && (month == null || t.Month == month))
                .ToListAsync();

            DateTime startDate, endDate;
            if (month.HasValue)
            {
                startDate = new DateTime(year, month.Value, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year, 12, 31);
            }

            var deliveries = await context.SalesDeliveries
                .Where(d => d.Status != EntityStatus.Deleted
                         && d.DeliveryDate >= startDate
                         && d.DeliveryDate <= endDate)
                .ToListAsync();

            var actualBySalesperson = deliveries
                .GroupBy(d => d.SalespersonId ?? 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(d => d.TotalAmount + d.TaxAmount));

            return targets.Select(t => new SalesAchievementItem
            {
                SalespersonId   = t.SalespersonId,
                SalespersonName = t.SalespersonLabel,
                TargetAmount    = t.TargetAmount,
                ActualAmount    = actualBySalesperson.GetValueOrDefault(t.SalespersonId ?? 0, 0m)
            })
            .OrderBy(x => x.IsCompanyTotal ? 0 : 1)
            .ThenBy(x => x.SalespersonName)
            .ToList();
        }

        public async Task<bool> IsTargetExistsAsync(int year, int? month, int? salespersonId, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SalesTargets
                .Where(t => t.Status != EntityStatus.Deleted
                         && t.Year == year
                         && t.Month == month
                         && t.SalespersonId == salespersonId
                         && (excludeId == null || t.Id != excludeId))
                .AnyAsync();
        }

        public async Task<List<SalesTargetMatrixRow>> GetMatrixByYearAsync(int year, List<Employee> employees)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.SalesTargets
                    .Where(t => t.Status != EntityStatus.Deleted && t.Year == year)
                    .ToListAsync();

                var rows = new List<SalesTargetMatrixRow>();

                // 公司整體目標列（SalespersonId == null）
                var companyRow = new SalesTargetMatrixRow { SalespersonId = null, SalespersonName = "公司整體" };
                foreach (var target in existing.Where(t => t.SalespersonId == null))
                {
                    if (target.Month.HasValue)
                        companyRow.SetMonth(target.Month.Value, target.TargetAmount);
                    else
                        companyRow.Annual = target.TargetAmount;
                }
                rows.Add(companyRow);

                // 每位業務員一列
                foreach (var emp in employees.Where(e => !e.IsSuperAdmin).OrderBy(e => e.Name))
                {
                    var row = new SalesTargetMatrixRow
                    {
                        SalespersonId = emp.Id,
                        SalespersonName = emp.Name ?? ""
                    };
                    foreach (var target in existing.Where(t => t.SalespersonId == emp.Id))
                    {
                        if (target.Month.HasValue)
                            row.SetMonth(target.Month.Value, target.TargetAmount);
                        else
                            row.Annual = target.TargetAmount;
                    }
                    rows.Add(row);
                }

                return rows;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetMatrixByYearAsync), GetType(), _logger,
                    new { Year = year });
                return new List<SalesTargetMatrixRow>();
            }
        }

        public async Task BatchUpsertAsync(int year, List<SalesTargetMatrixRow> rows)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.SalesTargets
                    .Where(t => t.Status != EntityStatus.Deleted && t.Year == year)
                    .ToListAsync();

                foreach (var row in rows)
                {
                    // 各月目標
                    for (int m = 1; m <= 12; m++)
                    {
                        var amount = row.GetMonth(m);
                        var record = existing.FirstOrDefault(t => t.SalespersonId == row.SalespersonId && t.Month == m);

                        if (amount > 0)
                        {
                            if (record == null)
                            {
                                context.SalesTargets.Add(new SalesTarget
                                {
                                    Year = year,
                                    Month = m,
                                    SalespersonId = row.SalespersonId,
                                    TargetAmount = amount,
                                    Status = EntityStatus.Active
                                });
                            }
                            else
                            {
                                record.TargetAmount = amount;
                                record.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        else if (record != null)
                        {
                            record.Status = EntityStatus.Deleted;
                        }
                    }

                    // 全年整體目標（Month == null）
                    var annualRecord = existing.FirstOrDefault(t => t.SalespersonId == row.SalespersonId && t.Month == null);
                    if (row.Annual > 0)
                    {
                        if (annualRecord == null)
                        {
                            context.SalesTargets.Add(new SalesTarget
                            {
                                Year = year,
                                Month = null,
                                SalespersonId = row.SalespersonId,
                                TargetAmount = row.Annual,
                                Status = EntityStatus.Active
                            });
                        }
                        else
                        {
                            annualRecord.TargetAmount = row.Annual;
                            annualRecord.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else if (annualRecord != null)
                    {
                        annualRecord.Status = EntityStatus.Deleted;
                    }
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpsertAsync), GetType(), _logger,
                    new { Year = year, RowCount = rows.Count });
                throw;
            }
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var yearsFromDb = await context.SalesTargets
                    .Where(t => t.Status != EntityStatus.Deleted)
                    .Select(t => t.Year)
                    .Distinct()
                    .ToListAsync();

                var currentYear = DateTime.Today.Year;
                if (!yearsFromDb.Contains(currentYear))
                    yearsFromDb.Add(currentYear);

                return yearsFromDb.OrderByDescending(y => y).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableYearsAsync), GetType(), _logger, null);
                return new List<int> { DateTime.Today.Year };
            }
        }

        public async Task<(List<SalesTarget> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<SalesTarget>, IQueryable<SalesTarget>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<SalesTarget> query = context.SalesTargets
                    .Include(t => t.Salesperson)
                    .Where(t => t.Status != EntityStatus.Deleted);

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(t => t.Year)
                    .ThenByDescending(t => t.Month)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new
                {
                    Method = nameof(GetPagedWithFiltersAsync),
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
                return (new List<SalesTarget>(), 0);
            }
        }
    }
}
