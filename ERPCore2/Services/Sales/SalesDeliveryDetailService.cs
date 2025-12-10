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
        private readonly ISalesOrderDetailService _salesOrderDetailService;

        public SalesDeliveryDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesDeliveryDetail>> logger,
            ISalesOrderDetailService salesOrderDetailService) 
            : base(contextFactory, logger)
        {
            _salesOrderDetailService = salesOrderDetailService;
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

        /// <summary>
        /// 取得指定客戶的可退貨出貨明細（已出貨但未完全退貨）
        /// </summary>
        public async Task<List<SalesDeliveryDetail>> GetReturnableDetailsByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該客戶的所有出貨明細，並計算已退貨數量
                var deliveryDetails = await context.SalesDeliveryDetails
                    .Include(dd => dd.SalesDelivery)
                        .ThenInclude(sd => sd.Customer)
                    .Include(dd => dd.Product)
                    .Include(dd => dd.Unit)
                    .Include(dd => dd.SalesOrderDetail)
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Where(dd => dd.SalesDelivery.CustomerId == customerId)
                    .ToListAsync();
                
                // 篩選出還有可退貨數量的明細
                var returnableDetails = new List<SalesDeliveryDetail>();
                
                foreach (var detail in deliveryDetails)
                {
                    // 計算已退貨數量
                    var returnedQty = await context.SalesReturnDetails
                        .Where(srd => srd.SalesDeliveryDetailId == detail.Id)
                        .SumAsync(srd => srd.ReturnQuantity);
                    
                    // 如果還有可退貨數量，加入列表
                    if (detail.DeliveryQuantity > returnedQty)
                    {
                        returnableDetails.Add(detail);
                    }
                }
                
                return returnableDetails
                    .OrderByDescending(dd => dd.SalesDelivery.DeliveryDate)
                    .ThenBy(dd => dd.Product.Code)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnableDetailsByCustomerAsync), GetType(), _logger, new {
                    Method = nameof(GetReturnableDetailsByCustomerAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        /// <summary>
        /// 取得指定客戶最近一次完整的銷貨出貨明細（用於智能下單）
        /// </summary>
        public async Task<List<SalesDeliveryDetail>> GetLastCompleteDeliveryAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該客戶最近一次的銷貨出貨單
                var lastSalesDelivery = await context.SalesDeliveries
                    .Include(sd => sd.DeliveryDetails)
                        .ThenInclude(sdd => sdd.Product)
                    .Include(sd => sd.DeliveryDetails)
                        .ThenInclude(sdd => sdd.Warehouse)
                    .Where(sd => sd.CustomerId == customerId 
                              && sd.DeliveryDetails.Any())
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ThenByDescending(sd => sd.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastSalesDelivery == null)
                    return new List<SalesDeliveryDetail>();

                // 返回該銷貨出貨單的所有明細
                return lastSalesDelivery.DeliveryDetails
                    .Where(sdd => sdd.Product != null)
                    .OrderBy(sdd => sdd.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastCompleteDeliveryAsync), GetType(), _logger, new { 
                    CustomerId = customerId
                });
                return new List<SalesDeliveryDetail>();
            }
        }

        #endregion

        #region 刪除覆寫方法

        /// <summary>
        /// 覆寫永久刪除方法,加入銷貨訂單明細的已出貨數量回寫邏輯
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                // 查詢要刪除的明細,包含相關資訊
                var entity = await context.SalesDeliveryDetails
                    .Include(d => d.Product)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (entity == null)
                {
                    return ServiceResult.Failure($"找不到ID為 {id} 的銷貨出貨明細");
                }

                // 回寫銷貨訂單明細的已出貨數量
                if (entity.SalesOrderDetailId.HasValue && entity.SalesOrderDetailId.Value > 0)
                {
                    var recalculateResult = await _salesOrderDetailService
                        .RecalculateDeliveredQuantityAsync(entity.SalesOrderDetailId.Value);

                    if (!recalculateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure($"回寫銷貨訂單明細失敗: {recalculateResult.ErrorMessage}");
                    }
                }

                // 執行實際刪除
                context.SalesDeliveryDetails.Remove(entity);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new {
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure($"永久刪除銷貨出貨明細時發生錯誤: {ex.Message}");
            }
        }

        #endregion
    }
}
