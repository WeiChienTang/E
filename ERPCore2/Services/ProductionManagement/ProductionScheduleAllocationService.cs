using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程分配服務實作
    /// 用於追蹤生產數量分配到哪些銷售訂單
    /// </summary>
    public class ProductionScheduleAllocationService : GenericManagementService<ProductionScheduleAllocation>, IProductionScheduleAllocationService
    {
        public ProductionScheduleAllocationService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public ProductionScheduleAllocationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ProductionScheduleAllocation>> logger) : base(contextFactory, logger)
        {
        }

        // 覆寫 GetAllAsync 以包含相關資料
        public override async Task<List<ProductionScheduleAllocation>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psa => psa.SalesOrderDetail)
                        .ThenInclude(sod => sod.SalesOrder)
                    .OrderByDescending(psa => psa.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ProductionScheduleAllocation>();
            }
        }

        // 覆寫 GetByIdAsync 以包含相關資料
        public override async Task<ProductionScheduleAllocation?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Include(psa => psa.SalesOrderDetail)
                        .ThenInclude(sod => sod.SalesOrder)
                            .ThenInclude(so => so.Customer)
                    .FirstOrDefaultAsync(psa => psa.Id == id);
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

        // 實作 ValidateAsync
        public override async Task<ServiceResult> ValidateAsync(ProductionScheduleAllocation entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ProductionScheduleItemId <= 0)
                    errors.Add("生產排程項目為必填");

                if (entity.SalesOrderDetailId <= 0)
                    errors.Add("銷售訂單明細為必填");

                if (entity.AllocatedQuantity <= 0)
                    errors.Add("分配數量必須大於0");

                // 檢查排程項目是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var item = await context.ProductionScheduleItems
                    .FirstOrDefaultAsync(psi => psi.Id == entity.ProductionScheduleItemId);

                if (item == null)
                    errors.Add("生產排程項目不存在");

                // 檢查銷售訂單明細是否存在
                var salesOrderDetailExists = await context.SalesOrderDetails
                    .AnyAsync(sod => sod.Id == entity.SalesOrderDetailId);

                if (!salesOrderDetailExists)
                    errors.Add("銷售訂單明細不存在");

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
                    ScheduleItemId = entity.ProductionScheduleItemId,
                    SalesOrderDetailId = entity.SalesOrderDetailId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 實作 SearchAsync
        public override async Task<List<ProductionScheduleAllocation>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psa => psa.SalesOrderDetail)
                        .ThenInclude(sod => sod.SalesOrder)
                    .Where(psa => psa.ProductionScheduleItem.Product.Name.Contains(searchTerm) ||
                                 (psa.ProductionScheduleItem.Product.Code != null && 
                                  psa.ProductionScheduleItem.Product.Code.Contains(searchTerm)) ||
                                 (psa.SalesOrderDetail.SalesOrder.Code != null && 
                                  psa.SalesOrderDetail.SalesOrder.Code.Contains(searchTerm)))
                    .OrderByDescending(psa => psa.CreatedAt)
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
                return new List<ProductionScheduleAllocation>();
            }
        }

        // === 業務特定方法 ===

        public async Task<List<ProductionScheduleAllocation>> GetByScheduleItemIdAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Include(psa => psa.SalesOrderDetail)
                        .ThenInclude(sod => sod.SalesOrder)
                            .ThenInclude(so => so.Customer)
                    .Where(psa => psa.ProductionScheduleItemId == scheduleItemId)
                    .OrderBy(psa => psa.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByScheduleItemIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByScheduleItemIdAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId
                });
                return new List<ProductionScheduleAllocation>();
            }
        }

        public async Task<List<ProductionScheduleAllocation>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Product)
                    .Include(psa => psa.ProductionScheduleItem)
                        .ThenInclude(psi => psi.ProductionSchedule)
                    .Where(psa => psa.SalesOrderDetailId == salesOrderDetailId)
                    .OrderByDescending(psa => psa.ProductionScheduleItem.ProductionSchedule.ScheduleDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderDetailIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId
                });
                return new List<ProductionScheduleAllocation>();
            }
        }

        public async Task<decimal> GetTotalAllocatedQuantityAsync(int scheduleItemId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ProductionScheduleAllocations
                    .Where(psa => psa.ProductionScheduleItemId == scheduleItemId)
                    .SumAsync(psa => psa.AllocatedQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalAllocatedQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTotalAllocatedQuantityAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId
                });
                return 0;
            }
        }

        public async Task<ServiceResult> CreateAllocationsAsync(int scheduleItemId, List<ProductionScheduleAllocation> allocations)
        {
            try
            {
                if (allocations == null || !allocations.Any())
                    return ServiceResult.Failure("分配列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程項目是否存在
                var itemExists = await context.ProductionScheduleItems.AnyAsync(psi => psi.Id == scheduleItemId);
                if (!itemExists)
                    return ServiceResult.Failure("生產排程項目不存在");

                // 設定排程項目ID與初始值
                foreach (var allocation in allocations)
                {
                    allocation.ProductionScheduleItemId = scheduleItemId;
                    allocation.CreatedAt = DateTime.Now;
                    allocation.Status = EntityStatus.Active;
                }

                // 驗證所有分配
                foreach (var allocation in allocations)
                {
                    var validationResult = await ValidateAsync(allocation);
                    if (!validationResult.IsSuccess)
                        return validationResult;
                }

                await context.ProductionScheduleAllocations.AddRangeAsync(allocations);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAllocationsAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateAllocationsAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId,
                    AllocationCount = allocations?.Count ?? 0
                });
                return ServiceResult.Failure("建立分配紀錄過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateAllocationsAsync(int scheduleItemId, List<ProductionScheduleAllocation> allocations)
        {
            try
            {
                if (allocations == null || !allocations.Any())
                    return ServiceResult.Failure("分配列表不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查排程項目是否存在
                var itemExists = await context.ProductionScheduleItems.AnyAsync(psi => psi.Id == scheduleItemId);
                if (!itemExists)
                    return ServiceResult.Failure("生產排程項目不存在");

                // 刪除舊分配
                var oldAllocations = await context.ProductionScheduleAllocations
                    .Where(psa => psa.ProductionScheduleItemId == scheduleItemId)
                    .ToListAsync();
                
                context.ProductionScheduleAllocations.RemoveRange(oldAllocations);

                // 新增新分配
                foreach (var allocation in allocations)
                {
                    allocation.ProductionScheduleItemId = scheduleItemId;
                    allocation.CreatedAt = DateTime.Now;
                    allocation.Status = EntityStatus.Active;
                }

                // 驗證所有分配
                foreach (var allocation in allocations)
                {
                    var validationResult = await ValidateAsync(allocation);
                    if (!validationResult.IsSuccess)
                        return validationResult;
                }

                await context.ProductionScheduleAllocations.AddRangeAsync(allocations);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAllocationsAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateAllocationsAsync),
                    ServiceType = GetType().Name,
                    ScheduleItemId = scheduleItemId,
                    AllocationCount = allocations?.Count ?? 0
                });
                return ServiceResult.Failure("更新分配紀錄過程發生錯誤");
            }
        }
    }
}
