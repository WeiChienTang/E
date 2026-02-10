using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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
        /// 檢查銷貨出貨編號是否已存在（符合 EntityCodeGenerationHelper 約定）
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

        #region 庫存異動方法

        /// <summary>
        /// 更新銷貨出貨單的庫存（差異更新模式）
        /// 功能：比較編輯前後的明細差異，使用淨值計算方式確保庫存準確性
        /// 處理邏輯：
        /// 1. 查詢該單號下所有庫存交易記錄，透過 OperationType 區分操作類型
        /// 2. 計算已處理過的庫存淨值（所有交易記錄 Quantity 的總和）
        /// 3. 計算當前明細應有的庫存數量
        /// 4. 比較目標數量與已處理數量，計算需要調整的數量
        /// 5. 根據調整數量進行庫存增減操作
        /// 6. 使用資料庫交易確保資料一致性
        /// 
        /// 注意：銷貨出貨是「出庫」操作，所以：
        /// - 出貨數量增加 → ReduceStockAsync（扣減庫存）
        /// - 出貨數量減少 → AddStockAsync（回補庫存）
        /// </summary>
        /// <param name="id">出貨單ID</param>
        /// <param name="updatedBy">更新人員ID（保留參數）</param>
        /// <returns>更新結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 取得當前的出貨單及明細
                    var currentDelivery = await context.SalesDeliveries
                        .Include(sd => sd.DeliveryDetails.AsQueryable())
                        .FirstOrDefaultAsync(sd => sd.Id == id);
                    
                    if (currentDelivery == null)
                        return ServiceResult.Failure("找不到指定的出貨單");

                    // 🔑 簡化設計：查詢該單據的所有異動明細，透過 OperationType 過濾
                    var allTransactionDetails = await context.InventoryTransactionDetails
                        .Include(d => d.InventoryTransaction)
                        .Where(d => d.InventoryTransaction.TransactionNumber == currentDelivery.Code)
                        .OrderBy(d => d.OperationTime)
                        .ThenBy(d => d.Id)
                        .ToListAsync();
                    
                    // 找到最後一次刪除記錄（OperationType = Delete）
                    var lastDeleteDetail = allTransactionDetails
                        .Where(d => d.OperationType == InventoryOperationTypeEnum.Delete)
                        .OrderByDescending(d => d.OperationTime)
                        .ThenByDescending(d => d.Id)
                        .FirstOrDefault();
                    
                    // 只計算最後一次刪除之後的記錄（不含刪除操作本身）
                    var existingDetails = lastDeleteDetail != null
                        ? allTransactionDetails.Where(d => d.Id > lastDeleteDetail.Id && 
                                                          d.OperationType != InventoryOperationTypeEnum.Delete).ToList()
                        : allTransactionDetails.Where(d => d.OperationType != InventoryOperationTypeEnum.Delete).ToList();

                    // 建立已處理過庫存的明細字典（ProductId + WarehouseId + LocationId -> 已處理庫存淨值）
                    var processedInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal NetProcessedQuantity)>();
                    
                    foreach (var detail in existingDetails)
                    {
                        var key = $"{detail.ProductId}_{detail.InventoryTransaction.WarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!processedInventory.ContainsKey(key))
                        {
                            processedInventory[key] = (detail.ProductId, detail.InventoryTransaction.WarehouseId, detail.WarehouseLocationId, 0m);
                        }
                        // 累加所有交易的淨值（注意：出庫的 Quantity 是負數）
                        var oldQty = processedInventory[key].NetProcessedQuantity;
                        var newQty = oldQty + detail.Quantity;
                        processedInventory[key] = (processedInventory[key].ProductId, processedInventory[key].WarehouseId, 
                                                  processedInventory[key].LocationId, newQty);
                    }
                    
                    // 建立當前明細字典
                    var currentInventory = new Dictionary<string, (int ProductId, int? WarehouseId, int? LocationId, decimal CurrentQuantity)>();
                    
                    foreach (var detail in currentDelivery.DeliveryDetails)
                    {
                        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!currentInventory.ContainsKey(key))
                        {
                            currentInventory[key] = (detail.ProductId, detail.WarehouseId, detail.WarehouseLocationId, 0);
                        }
                        var oldQty = currentInventory[key].CurrentQuantity;
                        var newQty = oldQty + (int)detail.DeliveryQuantity;
                        currentInventory[key] = (currentInventory[key].ProductId, currentInventory[key].WarehouseId, 
                                               currentInventory[key].LocationId, newQty);
                    }
                    
                    // 處理庫存差異 - 使用淨值計算方式
                    var allKeys = processedInventory.Keys.Union(currentInventory.Keys).ToList();
                    
                    foreach (var key in allKeys)
                    {
                        var hasProcessed = processedInventory.ContainsKey(key);
                        var hasCurrent = currentInventory.ContainsKey(key);
                        
                        // 計算目標數量（當前明細中應該出貨的數量，以負數表示）
                        decimal targetQuantity = hasCurrent ? -currentInventory[key].CurrentQuantity : 0m;
                        
                        // 計算已處理的庫存數量（之前所有交易的淨值，已經是負數）
                        decimal processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0m;
                        
                        // 計算需要調整的數量
                        decimal adjustmentNeeded = targetQuantity - processedQuantity;
                        
                        if (adjustmentNeeded != 0)
                        {
                            var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                            var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                            var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;
                            
                            // 跳過沒有指定倉庫的明細
                            if (!warehouseId.HasValue)
                                continue;
                            
                            if (adjustmentNeeded < 0)
                            {
                                // 需要扣減更多庫存（出貨數量增加）
                                var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                    productId,
                                    warehouseId.Value,
                                    Math.Abs(adjustmentNeeded),
                                    InventoryTransactionTypeEnum.Sale,
                                    currentDelivery.Code ?? string.Empty,  // 使用原始單號
                                    locationId,
                                    $"銷貨出貨編輯調增 - {currentDelivery.Code}",
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesDelivery,
                                    sourceDocumentId: currentDelivery.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!reduceResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存扣減失敗：{reduceResult.ErrorMessage}");
                                }
                            }
                            else
                            {
                                // 需要回補庫存（出貨數量減少）
                                var addResult = await _inventoryStockService.AddStockAsync(
                                    productId,
                                    warehouseId.Value,
                                    adjustmentNeeded,
                                    InventoryTransactionTypeEnum.SalesReturn,
                                    currentDelivery.Code ?? string.Empty,  // 使用原始單號
                                    null,  // 銷貨回補不需要成本
                                    locationId,
                                    $"銷貨出貨編輯調減 - {currentDelivery.Code}",
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesDelivery,
                                    sourceDocumentId: currentDelivery.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!addResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存回補失敗：{addResult.ErrorMessage}");
                                }
                            }
                        }
                    }

                    await context.SaveChangesAsync();
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInventoryByDifferenceAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateInventoryByDifferenceAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    UpdatedBy = updatedBy 
                });
                return ServiceResult.Failure("更新庫存差異過程發生錯誤");
            }
        }

        #endregion

        #region 刪除方法覆寫

        /// <summary>
        /// 永久刪除銷貨出貨單（含回寫已出貨數量及回補庫存）
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
                    
                    // 2. 回補庫存（刪除出貨單 = 商品退回倉庫）
                    if (entity.DeliveryDetails != null && entity.DeliveryDetails.Any())
                    {
                        foreach (var detail in entity.DeliveryDetails)
                        {
                            if (detail.DeliveryQuantity > 0 && detail.WarehouseId.HasValue)
                            {
                                var addStockResult = await _inventoryStockService.AddStockAsync(
                                    detail.ProductId,
                                    detail.WarehouseId.Value,
                                    (int)detail.DeliveryQuantity,
                                    InventoryTransactionTypeEnum.SalesReturn,
                                    entity.Code ?? string.Empty,  // 使用原始單號
                                    null,  // 刪除回補不需要成本
                                    detail.WarehouseLocationId,
                                    $"刪除銷貨出貨單 - {entity.Code}",
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesDelivery,
                                    sourceDocumentId: entity.Id,
                                    operationType: InventoryOperationTypeEnum.Delete  // 標記為刪除操作
                                );
                                
                                if (!addStockResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存回補失敗：{addStockResult.ErrorMessage}");
                                }
                            }
                        }
                    }
                    
                    // 3. 收集需要回寫的銷貨訂單明細ID（在刪除之前）
                    List<int> salesOrderDetailIdsToRecalculate = new List<int>();
                    if (_salesOrderDetailService != null && entity.DeliveryDetails != null)
                    {
                        salesOrderDetailIdsToRecalculate = entity.DeliveryDetails
                            .Where(d => d.SalesOrderDetailId.HasValue && d.SalesOrderDetailId.Value > 0)
                            .Select(d => d.SalesOrderDetailId!.Value)
                            .Distinct()
                            .ToList();
                    }
                    
                    // 4. 永久刪除主記錄（EF Core 會自動刪除相關的明細）
                    context.SalesDeliveries.Remove(entity);
                    
                    // 5. 先保存刪除變更（重要：讓資料庫先刪除記錄）
                    await context.SaveChangesAsync();
                    
                    // 6. 然後回寫銷貨訂單明細的已出貨數量（此時資料庫中已無刪除的記錄）
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
                    
                    // 7. 提交交易
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

        #region 確認方法

        /// <summary>
        /// 確認銷貨出貨單並更新庫存（首次新增時使用）
        /// 功能：執行出貨確認流程，將出貨數量從庫存扣除
        /// 處理流程：
        /// 1. 驗證出貨單存在性
        /// 2. 對每個明細進行庫存扣減操作
        /// 3. 使用原始單號作為 TransactionNumber，搭配 OperationType 區分操作類型
        /// 4. 使用資料庫交易確保資料一致性
        /// 5. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">出貨單ID</param>
        /// <param name="confirmedBy">確認人員ID（保留參數，未來可能使用）</param>
        /// <returns>確認結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> ConfirmDeliveryAsync(int id, int confirmedBy = 0)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var salesDelivery = await context.SalesDeliveries
                        .Include(sd => sd.DeliveryDetails)
                        .FirstOrDefaultAsync(sd => sd.Id == id);
                    
                    if (salesDelivery == null)
                        return ServiceResult.Failure("找不到指定的出貨單");
                    
                    // 更新庫存 - 出貨會減少庫存
                    foreach (var detail in salesDelivery.DeliveryDetails)
                    {
                        if (detail.DeliveryQuantity > 0)
                        {
                            var reduceStockResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                detail.WarehouseId ?? 0,
                                (int)detail.DeliveryQuantity,
                                InventoryTransactionTypeEnum.Sale,
                                salesDelivery.Code ?? string.Empty,
                                detail.WarehouseLocationId,
                                $"銷貨出貨確認 - {salesDelivery.Code ?? string.Empty}",
                                sourceDocumentType: InventorySourceDocumentTypes.SalesDelivery,
                                sourceDocumentId: salesDelivery.Id
                            );
                            
                            if (!reduceStockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存扣減失敗：{reduceStockResult.ErrorMessage}");
                            }
                        }
                    }

                    await context.SaveChangesAsync();
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmDeliveryAsync), GetType(), _logger, new { 
                    Method = nameof(ConfirmDeliveryAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ConfirmedBy = confirmedBy 
                });
                return ServiceResult.Failure("確認出貨單過程發生錯誤");
            }
        }

        #endregion
    }
}
