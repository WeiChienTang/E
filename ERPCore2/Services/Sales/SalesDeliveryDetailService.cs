using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨/出貨單明細服務實作
    /// </summary>
    public class SalesDeliveryDetailService : GenericManagementService<SalesDeliveryDetail>, ISalesDeliveryDetailService
    {
        public SalesDeliveryDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesDeliveryDetail>> logger) 
            : base(contextFactory, logger)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<SalesDeliveryDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveryDetails
                    .Include(d => d.SalesDelivery)
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Include(d => d.SalesOrderDetail)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        public override async Task<SalesDeliveryDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveryDetails
                    .Include(d => d.SalesDelivery)
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Include(d => d.SalesOrderDetail)
                    .FirstOrDefaultAsync(d => d.Id == id);
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

        public override async Task<List<SalesDeliveryDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveryDetails
                    .Include(d => d.Product)
                    .Include(d => d.SalesDelivery)
                    .Where(d =>
                        (d.Product.Name != null && d.Product.Name.Contains(searchTerm)) ||
                        (d.Product.Barcode != null && d.Product.Barcode.Contains(searchTerm)) ||
                        (d.SalesDelivery.Code != null && d.SalesDelivery.Code.Contains(searchTerm)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesDeliveryDetail entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.SalesDeliveryId <= 0)
                    errors.Add("銷貨單為必選項目");

                if (entity.ProductId <= 0)
                    errors.Add("商品為必選項目");

                if (entity.DeliveryQuantity <= 0)
                    errors.Add("出貨數量必須大於零");

                if (entity.UnitPrice < 0)
                    errors.Add("單價不能為負數");

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
                    DeliveryId = entity.SalesDeliveryId,
                    ProductId = entity.ProductId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自訂方法

        public async Task<List<SalesDeliveryDetail>> GetByDeliveryIdAsync(int deliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveryDetails
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Include(d => d.SalesOrderDetail)
                    .Where(d => d.SalesDeliveryId == deliveryId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDeliveryIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByDeliveryIdAsync),
                    DeliveryId = deliveryId
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        public async Task<List<SalesDeliveryDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveryDetails
                    .Include(d => d.SalesDelivery)
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderDetailIdAsync), GetType(), _logger, new {
                    Method = nameof(GetBySalesOrderDetailIdAsync),
                    SalesOrderDetailId = salesOrderDetailId
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        public async Task<SalesDeliveryDetailStatistics> GetStatisticsAsync(int? deliveryId = null, int? productId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.SalesDeliveryDetails.AsQueryable();

                if (deliveryId.HasValue)
                    query = query.Where(d => d.SalesDeliveryId == deliveryId.Value);

                if (productId.HasValue)
                    query = query.Where(d => d.ProductId == productId.Value);

                var statistics = new SalesDeliveryDetailStatistics
                {
                    TotalItems = await query.CountAsync(),
                    TotalQuantity = await query.SumAsync(d => d.DeliveryQuantity),
                    TotalAmount = await query.SumAsync(d => d.SubtotalAmount)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new {
                    Method = nameof(GetStatisticsAsync),
                    DeliveryId = deliveryId,
                    ProductId = productId
                });
                return new SalesDeliveryDetailStatistics();
            }
        }

        #endregion
    }
}
