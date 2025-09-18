using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SalesOrderService : GenericManagementService<SalesOrder>, ISalesOrderService
    {
        private readonly IInventoryStockService _inventoryStockService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger 和 InventoryStockService
        /// </summary>
        public SalesOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesOrder>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger 但需要 InventoryStockService
        /// </summary>
        public SalesOrderService(
            IDbContextFactory<AppDbContext> contextFactory,
            IInventoryStockService inventoryStockService) : base(contextFactory)
        {
            _inventoryStockService = inventoryStockService;
        }

        #region 覆寫基底方法

        public override async Task<List<SalesOrder>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => !so.IsDeleted)
                    .OrderByDescending(so => so.OrderDate)
                    .ThenBy(so => so.SalesOrderNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesOrder>();
            }
        }

        public override async Task<SalesOrder?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Include(so => so.SalesOrderDetails)
                    .ThenInclude(sod => sod.Product)
                    .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted);
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

        public override async Task<List<SalesOrder>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => !so.IsDeleted && (
                        so.SalesOrderNumber.ToLower().Contains(lowerSearchTerm) ||
                        so.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)
                    ))
                    .OrderByDescending(so => so.OrderDate)
                    .ThenBy(so => so.SalesOrderNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesOrder>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesOrder entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.SalesOrderNumber))
                    errors.Add("銷貨單號不能為空");

                if (entity.CustomerId <= 0)
                    errors.Add("客戶為必選項目");

                if (entity.OrderDate == default)
                    errors.Add("訂單日期不能為空");

                if (!string.IsNullOrWhiteSpace(entity.SalesOrderNumber) &&
                    await IsSalesOrderNumberExistsAsync(entity.SalesOrderNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("銷貨單號已存在");

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
                    EntityName = entity.SalesOrderNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自定義方法

        public async Task<bool> IsSalesOrderNumberExistsAsync(string salesOrderNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesOrders.Where(so => so.SalesOrderNumber == salesOrderNumber && !so.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(so => so.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesOrderNumberExistsAsync), GetType(), _logger, new {
                    Method = nameof(IsSalesOrderNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SalesOrderNumber = salesOrderNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<SalesOrder>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => so.CustomerId == customerId && !so.IsDeleted)
                    .OrderByDescending(so => so.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<SalesOrder>();
            }
        }



        public async Task<List<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate && !so.IsDeleted)
                    .OrderByDescending(so => so.OrderDate)
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
                return new List<SalesOrder>();
            }
        }



        public async Task<ServiceResult> CalculateOrderTotalAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.SalesOrders
                    .Include(so => so.SalesOrderDetails)
                    .FirstOrDefaultAsync(so => so.Id == orderId && !so.IsDeleted);

                if (order == null)
                    return ServiceResult.Failure("找不到指定的銷貨訂單");

                var totalAmount = order.SalesOrderDetails.Sum(d => d.Subtotal);
                var taxAmount = totalAmount * 0.05m; // 假設稅率 5%
                var totalAmountWithTax = totalAmount + taxAmount;

                order.TotalAmount = totalAmount;
                order.TaxAmount = taxAmount;
                order.TotalAmountWithTax = totalAmountWithTax;
                order.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateOrderTotalAsync), GetType(), _logger, new {
                    Method = nameof(CalculateOrderTotalAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId
                });
                return ServiceResult.Failure("計算訂單總金額時發生錯誤");
            }
        }

        public async Task<SalesOrder?> GetWithDetailsAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Product)
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Unit)
                    .FirstOrDefaultAsync(so => so.Id == orderId && !so.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId
                });
                return null;
            }
        }

        /// <summary>
        /// 驗證銷貨訂單明細的倉庫選擇和庫存是否足夠
        /// </summary>
        /// <param name="salesOrderDetails">銷貨訂單明細清單</param>
        /// <returns>驗證結果，包含倉庫和庫存不足的詳細訊息</returns>
        public async Task<ServiceResult> ValidateWarehouseInventoryStockAsync(List<SalesOrderDetail> salesOrderDetails)
        {
            try
            {
                if (salesOrderDetails == null || !salesOrderDetails.Any())
                {
                    return ServiceResult.Success();
                }

                var errors = new List<string>();
                
                foreach (var detail in salesOrderDetails.Where(d => d.ProductId > 0 && d.OrderQuantity > 0))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var product = await context.Products.FindAsync(detail.ProductId);
                    var productName = $"{product?.Code ?? "N/A"} - {product?.Name ?? "未知商品"}";
                    
                    // 1. 檢查是否選擇倉庫
                    if (!detail.WarehouseId.HasValue)
                    {
                        errors.Add($"{productName} 必須選擇倉庫");
                        continue;
                    }

                    // 2. 檢查指定倉庫的庫存（合併計算該倉庫內所有位置的庫存）
                    var availableStock = await _inventoryStockService.GetTotalAvailableStockByWarehouseAsync(
                        detail.ProductId, detail.WarehouseId.Value);
                        
                    if (availableStock < detail.OrderQuantity)
                    {
                        var warehouse = await context.Warehouses.FindAsync(detail.WarehouseId.Value);
                        var warehouseName = warehouse?.Name ?? "未知倉庫";
                        
                        errors.Add($"{productName} 在倉庫 {warehouseName} 庫存不足");
                        errors.Add($"可用庫存: {availableStock}，需要: {detail.OrderQuantity}");
                        errors.Add(""); // 空行分隔
                    }
                }
                
                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("\n", errors).TrimEnd())
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateWarehouseInventoryStockAsync), GetType(), _logger, new {
                    Method = nameof(ValidateWarehouseInventoryStockAsync),
                    ServiceType = GetType().Name,
                    DetailCount = salesOrderDetails?.Count ?? 0
                });
                return ServiceResult.Failure("驗證倉庫庫存時發生錯誤");
            }
        }

        #endregion
    }
}
