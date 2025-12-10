using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨/出貨單服務實作
    /// </summary>
    public class SalesDeliveryService : GenericManagementService<SalesDelivery>, ISalesDeliveryService
    {
        private readonly ISalesDeliveryDetailService? _detailService;
        private readonly IInventoryStockService _inventoryStockService;
        private readonly ISalesOrderDetailService? _salesOrderDetailService;

        public SalesDeliveryService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesDelivery>> logger,
            IInventoryStockService inventoryStockService,
            ISalesDeliveryDetailService? detailService = null,
            ISalesOrderDetailService? salesOrderDetailService = null) 
            : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _detailService = detailService;
            _salesOrderDetailService = salesOrderDetailService;
        }

        #region 覆寫基底方法

        public override async Task<List<SalesDelivery>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesOrder)
                    .Include(sd => sd.Warehouse)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ThenBy(sd => sd.Code)
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
                    .Include(sd => sd.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesOrder)
                    .Include(sd => sd.Warehouse)
                    .Include(sd => sd.DeliveryDetails)
                        .ThenInclude(sdd => sdd.Product)
                    .FirstOrDefaultAsync(sd => sd.Id == id);
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
                return await context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesOrder)
                    .Where(sd =>
                        (sd.Code != null && sd.Code.Contains(searchTerm)) ||
                        (sd.Customer.CompanyName != null && sd.Customer.CompanyName.Contains(searchTerm)) ||
                        (sd.Employee != null && sd.Employee.Name != null && sd.Employee.Name.Contains(searchTerm)) ||
                        (sd.DeliveryAddress != null && sd.DeliveryAddress.Contains(searchTerm)))
                    .OrderByDescending(sd => sd.DeliveryDate)
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

        #endregion

        #region 驗證方法

        public override async Task<ServiceResult> ValidateAsync(SalesDelivery entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("出貨單號不能為空");

                if (entity.CustomerId <= 0)
                    errors.Add("客戶為必選項目");

                if (entity.DeliveryDate == default)
                    errors.Add("出貨日期不能為空");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsSalesDeliveryCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
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
                    EntityName = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自訂方法

        /// <summary>
        /// 檢查銷貨出貨代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        public async Task<bool> IsSalesDeliveryCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesDeliveries.Where(sd => sd.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(sd => sd.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesDeliveryCodeExistsAsync), GetType(), _logger, new {
                    Method = nameof(IsSalesDeliveryCodeExistsAsync),
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<SalesDelivery>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesOrder)
                    .Where(sd => sd.CustomerId == customerId)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByCustomerIdAsync),
                    CustomerId = customerId
                });
                return new List<SalesDelivery>();
            }
        }

        public async Task<List<SalesDelivery>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .Include(sd => sd.Employee)
                    .Include(sd => sd.SalesOrder)
                    .Where(sd => sd.SalesOrderId == salesOrderId)
                    .OrderByDescending(sd => sd.DeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderIdAsync), GetType(), _logger, new {
                    Method = nameof(GetBySalesOrderIdAsync),
                    SalesOrderId = salesOrderId
                });
                return new List<SalesDelivery>();
            }
        }

        public async Task<ServiceResult<decimal>> CalculateTotalAmountAsync(int deliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var details = await context.SalesDeliveryDetails
                    .Where(d => d.SalesDeliveryId == deliveryId)
                    .ToListAsync();

                var totalAmount = details.Sum(d => d.SubtotalAmount);

                return ServiceResult<decimal>.Success(totalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalAmountAsync), GetType(), _logger, new {
                    Method = nameof(CalculateTotalAmountAsync),
                    DeliveryId = deliveryId
                });
                return ServiceResult<decimal>.Failure("計算銷貨單總金額時發生錯誤");
            }
        }

        public async Task<SalesDeliveryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? customerId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.SalesDeliveries.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(sd => sd.DeliveryDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(sd => sd.DeliveryDate <= endDate.Value);

                if (customerId.HasValue)
                    query = query.Where(sd => sd.CustomerId == customerId.Value);

                var statistics = new SalesDeliveryStatistics
                {
                    TotalCount = await query.CountAsync(),
                    TotalAmount = await query.SumAsync(sd => sd.TotalAmount),
                    TotalTaxAmount = await query.SumAsync(sd => sd.TaxAmount),
                    ShippedCount = await query.CountAsync(sd => sd.IsShipped),
                    PendingCount = await query.CountAsync(sd => !sd.IsShipped)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new {
                    Method = nameof(GetStatisticsAsync),
                    StartDate = startDate,
                    EndDate = endDate,
                    CustomerId = customerId
                });
                return new SalesDeliveryStatistics();
            }
        }

        #endregion

        #region 刪除方法覆寫

        /// <summary>
        /// 永久刪除銷貨出貨單（含回寫已出貨數量）
        /// 參考 PurchaseReceivingService 的實作邏輯
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 先取得主記錄（含詳細資料，包含銷貨訂單明細關聯）
                    var entity = await context.SalesDeliveries
                        .Include(sd => sd.DeliveryDetails)
                            .ThenInclude(sdd => sdd.SalesOrderDetail)
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }
                    
                    // 2. 先收集需要回寫的銷貨訂單明細ID（在刪除之前）
                    List<int> salesOrderDetailIdsToRecalculate = new List<int>();
                    if (_salesOrderDetailService != null && entity.DeliveryDetails != null)
                    {
                        salesOrderDetailIdsToRecalculate = entity.DeliveryDetails
                            .Where(d => d.SalesOrderDetailId.HasValue && d.SalesOrderDetailId.Value > 0)
                            .Select(d => d.SalesOrderDetailId!.Value)
                            .Distinct()
                            .ToList();
                    }
                    
                    // 3. 永久刪除主記錄（EF Core 會自動刪除相關的明細）
                    context.SalesDeliveries.Remove(entity);
                    
                    // 4. 先保存刪除變更（重要：讓資料庫先刪除記錄）
                    await context.SaveChangesAsync();
                    
                    // 5. 然後回寫銷貨訂單明細的已出貨數量（此時資料庫中已無刪除的記錄）
                    if (_salesOrderDetailService != null && salesOrderDetailIdsToRecalculate.Any())
                    {
                        foreach (var salesOrderDetailId in salesOrderDetailIdsToRecalculate)
                        {
                            // 使用同一個 context 重新計算（此時會排除已刪除的出貨明細）
                            var recalculateResult = await _salesOrderDetailService.RecalculateDeliveredQuantityAsync(
                                salesOrderDetailId,
                                context  // ← 傳入同一個 context
                            );
                            
                            if (!recalculateResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"回寫銷貨訂單明細已出貨數量失敗：{recalculateResult.ErrorMessage}");
                            }
                        }
                    }
                    
                    // 6. 提交交易
                    await transaction.CommitAsync();
                    
                    return ServiceResult.Success();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure($"永久刪除資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion
    }
}
