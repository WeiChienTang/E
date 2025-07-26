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
    /// 銷貨出貨服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SalesDeliveryService : GenericManagementService<SalesDelivery>, ISalesDeliveryService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SalesDeliveryService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesDelivery>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SalesDeliveryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<SalesDelivery>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Where(sd => !sd.IsDeleted)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ThenBy(sd => sd.DeliveryNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesDelivery>();
            }
        }

        public override async Task<SalesDelivery?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesDeliveryDetails)
                        .ThenInclude(sdd => sdd.Product)
                    .FirstOrDefaultAsync(sd => sd.Id == id && !sd.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<SalesDelivery>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Where(sd => !sd.IsDeleted && (
                        sd.DeliveryNumber.ToLower().Contains(lowerSearchTerm) ||
                        sd.SalesOrder.SalesOrderNumber.ToLower().Contains(lowerSearchTerm) ||
                        sd.SalesOrder.Customer.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                        (sd.DeliveryPersonnel != null && sd.DeliveryPersonnel.ToLower().Contains(lowerSearchTerm)) ||
                        (sd.TrackingNumber != null && sd.TrackingNumber.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ThenBy(sd => sd.DeliveryNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesDelivery>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesDelivery entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.DeliveryNumber))
                    errors.Add("出貨單號不能為空");

                if (entity.SalesOrderId <= 0)
                    errors.Add("銷貨訂單為必選項目");

                if (entity.DeliveryDate == default)
                    errors.Add("出貨日期不能為空");

                if (!string.IsNullOrWhiteSpace(entity.DeliveryNumber) &&
                    await IsDeliveryNumberExistsAsync(entity.DeliveryNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("出貨單號已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.DeliveryNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自定義方法

        public async Task<bool> IsDeliveryNumberExistsAsync(string deliveryNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesDeliveries.Where(sd => sd.DeliveryNumber == deliveryNumber && !sd.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(sd => sd.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDeliveryNumberExistsAsync), GetType(), _logger, new {
                    Method = nameof(IsDeliveryNumberExistsAsync),
                    ServiceType = GetType().Name,
                    DeliveryNumber = deliveryNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<SalesDelivery>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Where(sd => sd.SalesOrderId == salesOrderId && !sd.IsDeleted)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderIdAsync), GetType(), _logger, new {
                    Method = nameof(GetBySalesOrderIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId
                });
                return new List<SalesDelivery>();
            }
        }

        public async Task<List<SalesDelivery>> GetByDeliveryStatusAsync(SalesDeliveryStatus deliveryStatus)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Where(sd => sd.DeliveryStatus == deliveryStatus && !sd.IsDeleted)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDeliveryStatusAsync), GetType(), _logger, new {
                    Method = nameof(GetByDeliveryStatusAsync),
                    ServiceType = GetType().Name,
                    DeliveryStatus = deliveryStatus
                });
                return new List<SalesDelivery>();
            }
        }

        public async Task<List<SalesDelivery>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Where(sd => sd.DeliveryDate >= startDate && sd.DeliveryDate <= endDate && !sd.IsDeleted)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<SalesDelivery>();
            }
        }

        public async Task<ServiceResult> UpdateDeliveryStatusAsync(int deliveryId, SalesDeliveryStatus newStatus)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var delivery = await context.SalesDeliveries.FindAsync(deliveryId);
                
                if (delivery == null || delivery.IsDeleted)
                    return ServiceResult.Failure("找不到指定的出貨單");

                delivery.DeliveryStatus = newStatus;
                delivery.UpdatedAt = DateTime.Now;

                // 如果狀態改為已送達，更新實際送達日期
                if (newStatus == SalesDeliveryStatus.Received && !delivery.ActualArrivalDate.HasValue)
                {
                    delivery.ActualArrivalDate = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDeliveryStatusAsync), GetType(), _logger, new {
                    Method = nameof(UpdateDeliveryStatusAsync),
                    ServiceType = GetType().Name,
                    DeliveryId = deliveryId,
                    NewStatus = newStatus
                });
                return ServiceResult.Failure("更新出貨狀態時發生錯誤");
            }
        }

        public async Task<SalesDelivery?> GetWithDetailsAsync(int deliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesDeliveryDetails)
                        .ThenInclude(sdd => sdd.Product)
                    .Include(sd => sd.SalesDeliveryDetails)
                        .ThenInclude(sdd => sdd.Unit)
                    .Include(sd => sd.SalesDeliveryDetails)
                        .ThenInclude(sdd => sdd.SalesOrderDetail)
                    .FirstOrDefaultAsync(sd => sd.Id == deliveryId && !sd.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    DeliveryId = deliveryId
                });
                return null;
            }
        }

        #endregion
    }
}
