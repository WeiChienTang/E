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
    /// 銷貨訂單服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SalesOrderService : GenericManagementService<SalesOrder>, ISalesOrderService
    {
        private readonly IInventoryStockService _inventoryStockService;
        private readonly ISalesOrderDetailService? _detailService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger、InventoryStockService 和 SalesOrderDetailService
        /// </summary>
        public SalesOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesOrder>> logger,
            IInventoryStockService inventoryStockService,
            ISalesOrderDetailService? detailService = null) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _detailService = detailService;
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
                    .AsQueryable()
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
                    .FirstOrDefaultAsync(so => so.Id == id);
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
                    .Where(so => (
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
                var query = context.SalesOrders.Where(so => so.SalesOrderNumber == salesOrderNumber);
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
                    .Where(so => so.CustomerId == customerId)
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
                    .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate)
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
                    .FirstOrDefaultAsync(so => so.Id == orderId);

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
                    .FirstOrDefaultAsync(so => so.Id == orderId);
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

        #region 覆寫刪除方法 - 含庫存回滾機制

        /// <summary>
        /// 覆寫刪除方法 - 刪除主檔時同步回退庫存
        /// 功能：刪除銷貨訂單時，自動回退已扣減的庫存數量
        /// 處理流程：
        /// 1. 驗證訂單存在性
        /// 2. 查找相關庫存交易記錄
        /// 3. 進行庫存回退操作
        /// 4. 執行原本的軟刪除（主檔）
        /// 5. 使用資料庫交易確保資料一致性
        /// 6. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">要刪除的銷貨訂單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 獲取銷貨訂單及明細資料（在刪除前）
                    var salesOrder = await GetByIdAsync(id);
                    if (salesOrder == null)
                        return ServiceResult.Failure("找不到要刪除的銷貨訂單");

                    // 調試日誌：記錄基本資訊
                    Console.WriteLine($"=== 開始刪除銷貨訂單 ===");
                    Console.WriteLine($"銷貨訂單 ID: {id}, 單號: {salesOrder.SalesOrderNumber}");
                    Console.WriteLine($"明細數量: {salesOrder.SalesOrderDetails?.Count ?? 0}");
                    Console.WriteLine($"庫存服務狀態: {(_inventoryStockService != null ? "已注入" : "未注入")}");

                    // 2. 檢查是否有庫存服務可用並進行庫存回退
                    if (_inventoryStockService != null)
                    {
                        // 查找該銷貨訂單相關的庫存交易記錄
                        var inventoryTransactions = await _inventoryStockService.GetInventoryTransactionsBySalesOrderAsync(id);
                        Console.WriteLine($"找到 {inventoryTransactions.Count} 筆需要回滾的庫存交易記錄");
                        
                        // 3. 對每筆交易記錄進行庫存回退
                        foreach (var transaction_record in inventoryTransactions)
                        {
                            if (transaction_record.InventoryStockId.HasValue && transaction_record.Quantity < 0)
                            {
                                // 原始扣減是負數，回滾時要加回正數
                                var revertQuantity = Math.Abs(transaction_record.Quantity);
                                
                                Console.WriteLine($"處理庫存回滾 - 庫存ID: {transaction_record.InventoryStockId}, " +
                                                 $"產品ID: {transaction_record.ProductId}, 倉庫ID: {transaction_record.WarehouseId}, " +
                                                 $"回滾數量: {revertQuantity}, 位置ID: {transaction_record.WarehouseLocationId}");
                                
                                var revertResult = await _inventoryStockService.RevertStockToOriginalAsync(
                                    transaction_record.InventoryStockId.Value,
                                    revertQuantity,
                                    InventoryTransactionTypeEnum.Return,
                                    $"SO-{id}_REVERT_{DateTime.Now:yyyyMMddHHmmss}",
                                    $"刪除銷貨訂單回滾庫存 - {salesOrder.SalesOrderNumber}"
                                );
                                
                                Console.WriteLine($"庫存回滾結果: {(revertResult.IsSuccess ? "成功" : $"失敗 - {revertResult.ErrorMessage}")}");
                                
                                if (!revertResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存回退失敗：{revertResult.ErrorMessage}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("庫存服務未注入，跳過庫存回退操作");
                    }

                    // 4. 刪除銷貨訂單明細（如果有明細服務）
                    if (_detailService != null)
                    {
                        var deleteDetailsResult = await _detailService.DeleteBySalesOrderIdAsync(id);
                        if (!deleteDetailsResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult.Failure($"刪除訂單明細失敗：{deleteDetailsResult.ErrorMessage}");
                        }
                        Console.WriteLine("銷貨訂單明細軟刪除完成");
                    }

                    // 5. 執行原本的軟刪除（主檔）
                    Console.WriteLine($"開始執行軟刪除操作 - 銷貨訂單ID: {id}");
                    
                    var entity = await context.SalesOrders
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"在軟刪除階段找不到銷貨訂單 - ID: {id}");
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }

                    entity.UpdatedAt = DateTime.UtcNow;
                    
                    Console.WriteLine($"軟刪除完成 - 銷貨訂單: {entity.SalesOrderNumber}");

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    Console.WriteLine($"刪除操作成功完成 - 銷貨訂單ID: {id}");
                    Console.WriteLine("=== 刪除操作結束 ===");
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("刪除銷貨訂單過程發生錯誤");
            }
        }

        /// <summary>
        /// 永久刪除銷貨訂單（含庫存回滾）
        /// 這是UI實際調用的刪除方法
        /// 處理流程：
        /// 1. 驗證訂單存在性和刪除權限
        /// 2. 進行庫存回退操作
        /// 3. 永久刪除明細和主檔
        /// 4. 使用資料庫交易確保資料一致性
        /// </summary>
        /// <param name="id">要刪除的銷貨訂單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            Console.WriteLine($"=== SalesOrderService.PermanentDeleteAsync 開始執行 ===");
            Console.WriteLine($"要刪除的 SalesOrder ID: {id}");
            
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                Console.WriteLine("資料庫交易已開始");
                
                try
                {
                    // 1. 先取得主記錄（含詳細資料，包含明細關聯）
                    var entity = await context.SalesOrders
                        .Include(so => so.SalesOrderDetails)
                            .ThenInclude(sod => sod.Product)
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        Console.WriteLine($"找不到 ID 為 {id} 的 SalesOrder");
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }
                    
                    Console.WriteLine($"找到要刪除的 SalesOrder: {entity.SalesOrderNumber}");
                    Console.WriteLine($"包含 {entity.SalesOrderDetails.Count} 筆明細資料");
                    
                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        Console.WriteLine($"無法刪除: {canDeleteResult.ErrorMessage}");
                        return canDeleteResult;
                    }
                    
                    Console.WriteLine("通過刪除檢查");
                    
                    // 3. 檢查是否有庫存服務可用並進行庫存回滾
                    if (_inventoryStockService != null)
                    {
                        // 查找該銷貨訂單相關的庫存交易記錄
                        var inventoryTransactions = await _inventoryStockService.GetInventoryTransactionsBySalesOrderAsync(id);
                        Console.WriteLine($"找到 {inventoryTransactions.Count} 筆需要回滾的庫存交易記錄");
                        
                        // 對每筆交易記錄進行庫存回退
                        foreach (var transaction_record in inventoryTransactions)
                        {
                            if (transaction_record.InventoryStockId.HasValue && transaction_record.Quantity < 0)
                            {
                                // 原始扣減是負數，回滾時要加回正數
                                var revertQuantity = Math.Abs(transaction_record.Quantity);
                                
                                Console.WriteLine($"處理庫存回滾 - 庫存ID: {transaction_record.InventoryStockId}, " +
                                                 $"產品ID: {transaction_record.ProductId}, 倉庫ID: {transaction_record.WarehouseId}, " +
                                                 $"回滾數量: {revertQuantity}, 位置ID: {transaction_record.WarehouseLocationId}");
                                
                                var revertResult = await _inventoryStockService.RevertStockToOriginalAsync(
                                    transaction_record.InventoryStockId.Value,
                                    revertQuantity,
                                    InventoryTransactionTypeEnum.Return,
                                    $"SO-{id}_REVERT_{DateTime.Now:yyyyMMddHHmmss}",
                                    $"永久刪除銷貨訂單回滾庫存 - {entity.SalesOrderNumber}"
                                );
                                
                                Console.WriteLine($"庫存回滾結果: {(revertResult.IsSuccess ? "成功" : $"失敗 - {revertResult.ErrorMessage}")}");
                                
                                if (!revertResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存回退失敗：{revertResult.ErrorMessage}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("庫存服務未注入，跳過庫存回退操作");
                    }

                    // 4. 永久刪除明細資料
                    var detailsToDelete = await context.SalesOrderDetails
                        .Where(sod => sod.SalesOrderId == id)
                        .ToListAsync();
                        
                    if (detailsToDelete.Any())
                    {
                        Console.WriteLine($"永久刪除 {detailsToDelete.Count} 筆明細資料");
                        context.SalesOrderDetails.RemoveRange(detailsToDelete);
                    }

                    // 5. 永久刪除主檔
                    Console.WriteLine($"永久刪除銷貨訂單主檔: {entity.SalesOrderNumber}");
                    context.SalesOrders.Remove(entity);

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    Console.WriteLine($"永久刪除操作成功完成 - 銷貨訂單ID: {id}");
                    Console.WriteLine("=== 永久刪除操作結束 ===");
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("永久刪除銷貨訂單過程發生錯誤");
            }
        }

        #endregion
    }
}

