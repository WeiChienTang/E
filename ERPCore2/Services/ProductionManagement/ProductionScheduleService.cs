using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程服務實作
    /// </summary>
    public class ProductionScheduleService : GenericManagementService<ProductionSchedule>, IProductionScheduleService
    {
        public ProductionScheduleService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionSchedule>> logger) : base(contextFactory, logger)
        {
        }

        public ProductionScheduleService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以包含相關資料
        public override async Task<List<ProductionSchedule>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .OrderByDescending(ps => ps.ScheduleDate)
                    .ThenByDescending(ps => ps.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductionSchedule>();
            }
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionSchedule?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Include(ps => ps.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentProduct)
                    .Include(ps => ps.ScheduleDetails)
                        .ThenInclude(psd => psd.Warehouse)
                    .FirstOrDefaultAsync(ps => ps.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        // 實作必要的抽象方法 - 驗證
        public override async Task<ServiceResult> ValidateAsync(ProductionSchedule entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.ScheduleNumber))
                    errors.Add("排程單號為必填");
                else if (entity.ScheduleNumber.Length > 30)
                    errors.Add("排程單號不可超過30個字元");

                if (entity.ScheduleDate == default)
                    errors.Add("排程日期為必填");

                if (!string.IsNullOrWhiteSpace(entity.SourceDocumentType) && entity.SourceDocumentType.Length > 50)
                    errors.Add("來源單據類型不可超過50個字元");

                // 檢查排程單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.ScheduleNumber) &&
                    await IsScheduleNumberExistsAsync(entity.ScheduleNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("排程單號已存在");

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
                    ScheduleNumber = entity.ScheduleNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作必要的抽象方法 - 搜尋
        public override async Task<List<ProductionSchedule>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Where(ps => ps.ScheduleNumber.Contains(searchTerm) ||
                                (ps.SourceDocumentType != null && ps.SourceDocumentType.Contains(searchTerm)) ||
                                (ps.Customer != null && ps.Customer.CompanyName.Contains(searchTerm)))
                    .OrderByDescending(ps => ps.ScheduleDate)
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
                return new List<ProductionSchedule>();
            }
        }

        // 業務特定方法
        public async Task<bool> IsScheduleNumberExistsAsync(string scheduleNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductionSchedules.Where(ps => ps.ScheduleNumber == scheduleNumber);

                if (excludeId.HasValue)
                    query = query.Where(ps => ps.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsScheduleNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsScheduleNumberExistsAsync),
                    ServiceType = GetType().Name,
                    ScheduleNumber = scheduleNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<ProductionSchedule>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Where(ps => ps.CustomerId == customerId)
                    .OrderByDescending(ps => ps.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<ProductionSchedule>();
            }
        }

        public async Task<List<ProductionSchedule>> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Where(ps => ps.SourceDocumentType == sourceDocumentType &&
                                ps.SourceDocumentId == sourceDocumentId)
                    .OrderByDescending(ps => ps.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySourceDocumentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySourceDocumentAsync),
                    ServiceType = GetType().Name,
                    SourceDocumentType = sourceDocumentType,
                    SourceDocumentId = sourceDocumentId
                });
                return new List<ProductionSchedule>();
            }
        }

        public async Task<List<ProductionSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Where(ps => ps.ScheduleDate >= startDate && ps.ScheduleDate <= endDate)
                    .OrderByDescending(ps => ps.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<ProductionSchedule>();
            }
        }

        public async Task<ProductionSchedule?> GetWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionSchedules
                    .Include(ps => ps.CreatedByEmployee)
                    .Include(ps => ps.Customer)
                    .Include(ps => ps.ScheduleDetails)
                        .ThenInclude(psd => psd.ComponentProduct)
                            .ThenInclude(p => p.ProductCategory)
                    .Include(ps => ps.ScheduleDetails)
                        .ThenInclude(psd => psd.ProductCompositionDetail)
                    .Include(ps => ps.ScheduleDetails)
                        .ThenInclude(psd => psd.Warehouse)
                    .FirstOrDefaultAsync(ps => ps.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        // 覆寫刪除前檢查
        protected override async Task<ServiceResult> CanDeleteAsync(ProductionSchedule entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否有明細
                var hasDetails = await context.ProductionScheduleDetails
                    .AnyAsync(psd => psd.ProductionScheduleId == entity.Id);

                if (hasDetails)
                    return ServiceResult.Failure("無法刪除，此排程已有明細資料");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        // 覆寫刪除方法以加入業務檢查
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("生產排程不存在");

                // 檢查是否可以刪除
                var canDeleteResult = await CanDeleteAsync(entity);
                if (!canDeleteResult.IsSuccess)
                    return canDeleteResult;

                // 呼叫基底類別的刪除方法
                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure("刪除過程發生錯誤");
            }
        }
    }
}
