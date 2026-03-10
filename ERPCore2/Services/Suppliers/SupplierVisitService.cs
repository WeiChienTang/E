using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商拜訪紀錄服務實作
    /// </summary>
    public class SupplierVisitService : GenericManagementService<SupplierVisit>, ISupplierVisitService
    {
        public SupplierVisitService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SupplierVisit>> logger) : base(contextFactory, logger)
        {
        }

        public SupplierVisitService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        protected override IQueryable<SupplierVisit> BuildGetAllQuery(AppDbContext context)
        {
            return context.SupplierVisits
                .Include(v => v.Supplier)
                .Include(v => v.Employee)
                .OrderByDescending(v => v.VisitDate);
        }

        public override async Task<SupplierVisit?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierVisits
                    .Include(v => v.Supplier)
                    .Include(v => v.Employee)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<SupplierVisit>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierVisits
                    .Include(v => v.Supplier)
                    .Include(v => v.Employee)
                    .Where(v => (v.Purpose != null && v.Purpose.ToLower().Contains(lowerSearchTerm)) ||
                                (v.Content != null && v.Content.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SupplierVisit entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.SupplierId <= 0)
                    errors.Add("請選擇廠商");

                if (entity.VisitDate == default)
                    errors.Add("請輸入拜訪日期");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<List<SupplierVisit>> GetBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierVisits
                    .Include(v => v.Employee)
                    .Where(v => v.SupplierId == supplierId)
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierAsync), GetType(), _logger, new { SupplierId = supplierId });
                throw;
            }
        }
    }
}
