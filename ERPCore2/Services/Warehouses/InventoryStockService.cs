using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 批次扣減明細
    /// </summary>
    public class BatchReductionDetail
    {
        public int BatchId { get; set; }
        public string? BatchNumber { get; set; }
        public int ReduceQuantity { get; set; }
    }

    /// <summary>
    /// 庫存管理服務實作
    /// </summary>
    public class InventoryStockService : GenericManagementService<InventoryStock>, IInventoryStockService
    {
        public InventoryStockService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public InventoryStockService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<InventoryStock>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基本方法

        public override async Task<List<InventoryStock>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.Product!.Code)
                    .ThenBy(i => i.Warehouse!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<InventoryStock>();
            }
        }

        public override async Task<InventoryStock?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Include(i => i.InventoryTransactions.Where(t => !t.IsDeleted))
                    .Include(i => i.InventoryReservations.Where(r => !r.IsDeleted))
                    .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
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

        public override async Task<List<InventoryStock>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.ToLower();
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => !i.IsDeleted && 
                        ((i.Product!.Code != null && i.Product.Code.ToLower().Contains(term)) ||
                         (i.Product!.Name != null && i.Product.Name.ToLower().Contains(term)) ||
                         (i.Warehouse!.Code != null && i.Warehouse.Code.ToLower().Contains(term)) ||
                         (i.Warehouse!.Name != null && i.Warehouse.Name.ToLower().Contains(term))))
                    .OrderBy(i => i.Product!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<InventoryStock>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(InventoryStock entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ProductId <= 0)
                    errors.Add("必須選擇商品");

                if (entity.CurrentStock < 0)
                    errors.Add("庫存數量不能為負數");

                if (entity.ReservedStock < 0)
                    errors.Add("預留數量不能為負數");

                if (entity.ReservedStock > entity.CurrentStock)
                    errors.Add("預留數量不能大於現有庫存");

                // 檢查是否已存在相同的庫存記錄
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.InventoryStocks
                    .FirstOrDefaultAsync(i => i.ProductId == entity.ProductId && 
                                            i.WarehouseId == entity.WarehouseId &&
                                            i.WarehouseLocationId == entity.WarehouseLocationId &&
                                            i.Id != entity.Id && !i.IsDeleted);

                if (existing != null)
                    errors.Add("該商品在此倉庫位置已有庫存記錄");

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
                    ProductId = entity.ProductId,
                    WarehouseId = entity.WarehouseId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 基本查詢方法

        public async Task<List<InventoryStock>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .OrderBy(i => i.Warehouse!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<InventoryStock>();
            }
        }

        public async Task<List<InventoryStock>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => i.WarehouseId == warehouseId && !i.IsDeleted)
                    .OrderBy(i => i.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = warehouseId 
                });
                return new List<InventoryStock>();
            }
        }

        public async Task<InventoryStock?> GetByProductWarehouseAsync(int productId, int warehouseId, int? locationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                            i.WarehouseId == warehouseId &&
                                            i.WarehouseLocationId == locationId && 
                                            !i.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductWarehouseAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductWarehouseAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    LocationId = locationId 
                });
                return null;
            }
        }

        public async Task<InventoryStock?> GetByProductWarehouseAsync(int productId, int? warehouseId = null, int? locationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                            i.WarehouseId == warehouseId &&
                                            i.WarehouseLocationId == locationId && 
                                            !i.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductWarehouseAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductWarehouseAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    LocationId = locationId 
                });
                return null;
            }
        }

        public async Task<List<InventoryStock>> GetLowStockItemsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => !i.IsDeleted && 
                              i.MinStockLevel.HasValue && 
                              i.CurrentStock <= i.MinStockLevel.Value)
                    .OrderBy(i => i.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLowStockItemsAsync), GetType(), _logger, new { 
                    Method = nameof(GetLowStockItemsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<InventoryStock>();
            }
        }

        public async Task<int> GetAvailableStockAsync(int productId, int warehouseId, int? locationId = null)
        {
            try
            {
                var stock = await GetByProductWarehouseAsync(productId, warehouseId, locationId);
                return stock?.AvailableStock ?? 0;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableStockAsync), GetType(), _logger, new { 
                    Method = nameof(GetAvailableStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    LocationId = locationId 
                });
                return 0;
            }
        }

        /// <summary>
        /// 取得商品在指定倉庫內所有位置的總可用庫存
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="warehouseId">倉庫ID</param>
        /// <returns>總可用庫存數量</returns>
        public async Task<int> GetTotalAvailableStockByWarehouseAsync(int productId, int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得該商品在指定倉庫內所有位置的庫存記錄
                var stocks = await context.InventoryStocks
                    .Where(i => i.ProductId == productId && 
                               i.WarehouseId == warehouseId &&
                               !i.IsDeleted)
                    .ToListAsync();
                
                // 在記憶體中計算總可用庫存 (CurrentStock - ReservedStock)
                var totalAvailableStock = stocks.Sum(i => i.AvailableStock);
                    
                return totalAvailableStock;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalAvailableStockByWarehouseAsync), GetType(), _logger, new { 
                    Method = nameof(GetTotalAvailableStockByWarehouseAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId
                });
                return 0;
            }
        }

        #endregion

        #region 庫存異動方法

        public async Task<ServiceResult> AddStockAsync(int productId, int warehouseId, int quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, 
            decimal? unitCost = null, int? locationId = null, string? remarks = null,
            string? batchNumber = null, DateTime? batchDate = null, DateTime? expiryDate = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("數量必須大於0");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    InventoryStock? stock = null;
                    
                    if (!string.IsNullOrEmpty(batchNumber))
                    {
                        // 有批號：尋找特定批號的庫存記錄
                        stock = await context.InventoryStocks
                            .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                                     i.WarehouseId == warehouseId &&
                                                     i.WarehouseLocationId == locationId && 
                                                     i.BatchNumber == batchNumber &&
                                                     !i.IsDeleted);
                    }
                    else
                    {
                        // 無批號：尋找沒有批號的庫存記錄（向後相容）
                        stock = await context.InventoryStocks
                            .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                                     i.WarehouseId == warehouseId &&
                                                     i.WarehouseLocationId == locationId && 
                                                     string.IsNullOrEmpty(i.BatchNumber) &&
                                                     !i.IsDeleted);
                    }
                    
                    if (stock == null)
                    {
                        stock = new InventoryStock
                        {
                            ProductId = productId,
                            WarehouseId = warehouseId,
                            WarehouseLocationId = locationId,
                            CurrentStock = 0,
                            ReservedStock = 0,
                            BatchNumber = batchNumber,
                            BatchDate = batchDate ?? DateTime.Now,
                            ExpiryDate = expiryDate,
                            Status = EntityStatus.Active
                        };
                        await context.InventoryStocks.AddAsync(stock);
                        await context.SaveChangesAsync();
                    }

                    var stockBefore = stock.CurrentStock;
                    stock.CurrentStock += quantity;
                    stock.LastTransactionDate = DateTime.Now;

                    // 更新批次資訊（如果提供且原本沒有）
                    if (batchDate.HasValue && !stock.BatchDate.HasValue)
                        stock.BatchDate = batchDate.Value;
                    if (expiryDate.HasValue && !stock.ExpiryDate.HasValue)  
                        stock.ExpiryDate = expiryDate.Value;

                    // 更新平均成本
                    if (unitCost.HasValue && unitCost.Value > 0)
                    {
                        if (stock.AverageCost.HasValue && stockBefore > 0)
                        {
                            var totalCostBefore = stock.AverageCost.Value * stockBefore;
                            var newTotalCost = totalCostBefore + (unitCost.Value * quantity);
                            stock.AverageCost = newTotalCost / stock.CurrentStock;
                        }
                        else
                        {
                            stock.AverageCost = unitCost.Value;
                        }
                    }

                    // 建立交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        WarehouseLocationId = locationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = quantity,
                        UnitCost = unitCost,
                        StockBefore = stockBefore,
                        StockAfter = stock.CurrentStock,
                        TransactionRemarks = remarks,
                        InventoryStockId = stock.Id,
                        Status = EntityStatus.Active
                    };

                    await context.InventoryTransactions.AddAsync(inventoryTransaction);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddStockAsync), GetType(), _logger, new { 
                    Method = nameof(AddStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    UnitCost = unitCost,
                    LocationId = locationId,
                    Remarks = remarks 
                });
                return ServiceResult.Failure("庫存增加失敗");
            }
        }

        public async Task<ServiceResult> ReduceStockAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("數量必須大於0");

                var stock = await GetByProductWarehouseAsync(productId, warehouseId, locationId);
                if (stock == null)
                    return ServiceResult.Failure("找不到庫存記錄");

                if (stock.AvailableStock < quantity)
                    return ServiceResult.Failure($"可用庫存不足，目前可用庫存：{stock.AvailableStock}");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 重新取得庫存記錄以確保資料一致性
                    var contextStock = await context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.Id == stock.Id && !i.IsDeleted);
                    
                    if (contextStock == null)
                        return ServiceResult.Failure("找不到庫存記錄");

                    var stockBefore = contextStock.CurrentStock;
                    contextStock.CurrentStock -= quantity;
                    contextStock.LastTransactionDate = DateTime.Now;

                    // 建立交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        WarehouseLocationId = locationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = -quantity, // 負數表示出庫
                        UnitCost = contextStock.AverageCost,
                        StockBefore = stockBefore,
                        StockAfter = contextStock.CurrentStock,
                        TransactionRemarks = remarks,
                        InventoryStockId = contextStock.Id,
                        Status = EntityStatus.Active
                    };

                    await context.InventoryTransactions.AddAsync(inventoryTransaction);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReduceStockAsync), GetType(), _logger, new { 
                    Method = nameof(ReduceStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    LocationId = locationId,
                    Remarks = remarks 
                });
                return ServiceResult.Failure("庫存扣減失敗");
            }
        }

        public async Task<ServiceResult> TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("數量必須大於0");

                if (fromWarehouseId == toWarehouseId && fromLocationId == toLocationId)
                    return ServiceResult.Failure("來源和目標不能相同");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 從來源倉庫扣減
                    var reduceResult = await ReduceStockAsync(productId, fromWarehouseId, quantity,
                        InventoryTransactionTypeEnum.Transfer, transactionNumber, fromLocationId, remarks);
                    
                    if (!reduceResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return reduceResult;
                    }

                    // 增加到目標倉庫
                    var fromStock = await GetByProductWarehouseAsync(productId, fromWarehouseId, fromLocationId);
                    var addResult = await AddStockAsync(productId, toWarehouseId, quantity,
                        InventoryTransactionTypeEnum.Transfer, transactionNumber, 
                        fromStock?.AverageCost, toLocationId, remarks);

                    if (!addResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return addResult;
                    }

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TransferStockAsync), GetType(), _logger, new {
                    Method = nameof(TransferStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    Quantity = quantity,
                    TransactionNumber = transactionNumber
                });
                return ServiceResult.Failure("庫存轉移失敗");
            }
        }

        public async Task<ServiceResult> AdjustStockAsync(int productId, int warehouseId, int newQuantity,
            string transactionNumber, string? remarks = null, int? locationId = null)
        {
            try
            {
                if (newQuantity < 0)
                    return ServiceResult.Failure("調整後數量不能為負數");

                var stock = await GetByProductWarehouseAsync(productId, warehouseId, locationId);
                if (stock == null)
                    return ServiceResult.Failure("找不到庫存記錄");

                var difference = newQuantity - stock.CurrentStock;
                if (difference == 0)
                    return ServiceResult.Success();

                if (difference > 0)
                {
                    // 增加庫存
                    return await AddStockAsync(productId, warehouseId, difference,
                        InventoryTransactionTypeEnum.Adjustment, transactionNumber, null, locationId, remarks);
                }
                else
                {
                    // 減少庫存
                    return await ReduceStockAsync(productId, warehouseId, Math.Abs(difference),
                        InventoryTransactionTypeEnum.Adjustment, transactionNumber, locationId, remarks);
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AdjustStockAsync), GetType(), _logger, new {
                    Method = nameof(AdjustStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    NewQuantity = newQuantity,
                    TransactionNumber = transactionNumber
                });
                return ServiceResult.Failure("庫存調整失敗");
            }
        }

        /// <summary>
        /// FIFO 方式減少庫存
        /// </summary>
        public async Task<ServiceResult> ReduceStockWithFIFOAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null)
        {
            try
            {
                // 取得該商品在指定倉庫的所有批號庫存，按批次日期排序 (FIFO)
                var batchStocks = await GetBatchStocksByProductAndWarehouseAsync(productId, warehouseId, locationId);
                
                var totalAvailable = batchStocks.Sum(b => b.AvailableStock);
                if (totalAvailable < quantity)
                    return ServiceResult.Failure($"庫存不足，可用：{totalAvailable}，需要：{quantity}");

                var remainingQuantity = quantity;
                var reductionDetails = new List<BatchReductionDetail>();

                // 按批次日期順序扣減 (FIFO)
                foreach (var batch in batchStocks.OrderBy(b => b.BatchDate).ThenBy(b => b.Id))
                {
                    if (remainingQuantity <= 0) break;
                    
                    var availableFromThisBatch = batch.AvailableStock;
                    if (availableFromThisBatch <= 0) continue;
                    
                    var reduceFromThisBatch = Math.Min(availableFromThisBatch, remainingQuantity);
                    
                    reductionDetails.Add(new BatchReductionDetail 
                    {
                        BatchId = batch.Id,
                        BatchNumber = batch.BatchNumber,
                        ReduceQuantity = reduceFromThisBatch
                    });
                    
                    remainingQuantity -= reduceFromThisBatch;
                }

                // 執行實際扣減
                foreach (var detail in reductionDetails)
                {
                    var result = await ReduceStockFromSpecificBatchAsync(
                        detail.BatchId, detail.ReduceQuantity, 
                        transactionType, transactionNumber, remarks);
                        
                    if (!result.IsSuccess)
                        return result;
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReduceStockWithFIFOAsync), GetType(), _logger, new {
                    Method = nameof(ReduceStockWithFIFOAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    LocationId = locationId,
                    Remarks = remarks
                });
                return ServiceResult.Failure("FIFO庫存扣減失敗");
            }
        }

        /// <summary>
        /// 取得商品在指定倉庫的批號庫存清單
        /// </summary>
        private async Task<List<InventoryStock>> GetBatchStocksByProductAndWarehouseAsync(
            int productId, int warehouseId, int? locationId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.InventoryStocks
                .Where(i => i.ProductId == productId && 
                           i.WarehouseId == warehouseId &&
                           i.CurrentStock > 0 &&
                           !i.IsDeleted);
            
            // 如果指定了位置，才篩選特定位置；否則查詢整個倉庫
            if (locationId.HasValue)
            {
                query = query.Where(i => i.WarehouseLocationId == locationId.Value);
            }
            
            return await query
                .OrderBy(i => i.BatchDate)  // FIFO 排序
                .ThenBy(i => i.Id)         // 相同日期按ID排序
                .ToListAsync();
        }

        /// <summary>
        /// 從特定批號扣減庫存
        /// </summary>
        private async Task<ServiceResult> ReduceStockFromSpecificBatchAsync(
            int batchStockId, int quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var stock = await context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.Id == batchStockId && !i.IsDeleted);
                    
                    if (stock == null)
                        return ServiceResult.Failure("找不到批號庫存記錄");

                    if (stock.AvailableStock < quantity)
                        return ServiceResult.Failure($"批號 {stock.BatchNumber} 可用庫存不足");

                    var stockBefore = stock.CurrentStock;
                    stock.CurrentStock -= quantity;
                    stock.LastTransactionDate = DateTime.Now;

                    // 建立交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = stock.ProductId,
                        WarehouseId = stock.WarehouseId ?? 0,
                        WarehouseLocationId = stock.WarehouseLocationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = -quantity, // 負數表示出庫
                        UnitCost = stock.AverageCost,
                        StockBefore = stockBefore,
                        StockAfter = stock.CurrentStock,
                        TransactionRemarks = $"{remarks} (批號: {stock.BatchNumber})",
                        InventoryStockId = stock.Id,
                        Status = EntityStatus.Active
                    };

                    await context.InventoryTransactions.AddAsync(inventoryTransaction);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReduceStockFromSpecificBatchAsync), GetType(), _logger, new {
                    Method = nameof(ReduceStockFromSpecificBatchAsync),
                    ServiceType = GetType().Name,
                    BatchStockId = batchStockId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber
                });
                return ServiceResult.Failure("批號庫存扣減失敗");
            }
        }

        #endregion

        #region 庫存總覽查詢方法

        public async Task<List<InventoryStock>> GetInventoryOverviewAsync(int? warehouseId = null, int? categoryId = null, int? locationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryStocks
                    .Include(i => i.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => !i.IsDeleted);

                // 篩選條件
                if (warehouseId.HasValue)
                {
                    query = query.Where(i => i.WarehouseId == warehouseId.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(i => i.Product.ProductCategoryId == categoryId.Value);
                }

                if (locationId.HasValue)
                {
                    query = query.Where(i => i.WarehouseLocationId == locationId.Value);
                }

                return await query
                    .OrderBy(i => i.Warehouse!.Name)
                    .ThenBy(i => i.Product!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex,
                    nameof(GetInventoryOverviewAsync),
                    GetType(),
                    _logger,
                    new { 
                        Method = nameof(GetInventoryOverviewAsync),
                        ServiceType = GetType().Name,
                        WarehouseId = warehouseId, 
                        CategoryId = categoryId, 
                        LocationId = locationId 
                    }
                );
                return new List<InventoryStock>();
            }
        }

        public async Task<List<InventoryStock>> GetLowStockOverviewAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStocks
                    .Include(i => i.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => !i.IsDeleted && 
                               i.MinStockLevel.HasValue && 
                               i.CurrentStock <= i.MinStockLevel.Value)
                    .OrderBy(i => i.CurrentStock)
                    .ThenBy(i => i.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex,
                    nameof(GetLowStockOverviewAsync),
                    GetType(),
                    _logger,
                    new {
                        Method = nameof(GetLowStockOverviewAsync),
                        ServiceType = GetType().Name
                    }
                );
                return new List<InventoryStock>();
            }
        }

        public async Task<Dictionary<string, object>> GetInventoryStatisticsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stats = new Dictionary<string, object>();

                // 總商品數量
                var totalProducts = await context.InventoryStocks
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.ProductId)
                    .Distinct()
                    .CountAsync();

                // 總庫存價值
                var totalInventoryValue = await context.InventoryStocks
                    .Where(i => !i.IsDeleted && i.AverageCost.HasValue)
                    .SumAsync(i => i.CurrentStock * (i.AverageCost ?? 0));

                // 低庫存商品數量
                var lowStockCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted &&
                               i.MinStockLevel.HasValue &&
                               i.CurrentStock <= i.MinStockLevel.Value)
                    .CountAsync();

                // 零庫存商品數量
                var zeroStockCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted && i.CurrentStock == 0)
                    .CountAsync();

                // 倉庫數量
                var warehouseCount = await context.Warehouses
                    .Where(w => !w.IsDeleted)
                    .CountAsync();

                // 新增：未設定警戒線的商品數量（MinStockLevel 或 MaxStockLevel 任一為 null 或 <= 0）
                var noWarningLevelCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted &&
                               (!i.MinStockLevel.HasValue || i.MinStockLevel.Value <= 0 ||
                                !i.MaxStockLevel.HasValue || i.MaxStockLevel.Value <= 0))
                    .CountAsync();

                // 新增：超過最高警戒線的商品數量（庫存過多）
                var overStockCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted &&
                               i.MaxStockLevel.HasValue &&
                               i.MaxStockLevel.Value > 0 &&
                               i.CurrentStock > i.MaxStockLevel.Value)
                    .CountAsync();

                // 新增：呆滯庫存數量（30天沒有異動）
                var staleStockCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted &&
                               (!i.LastTransactionDate.HasValue ||
                                i.LastTransactionDate.Value <= DateTime.Now.AddDays(-30)))
                    .CountAsync();

                // 新增：預留庫存過高的商品數量（預留庫存佔總庫存比例 > 50%）
                var highReservedStockCount = await context.InventoryStocks
                    .Where(i => !i.IsDeleted &&
                               i.CurrentStock > 0 &&
                               i.ReservedStock > 0 &&
                               (decimal)i.ReservedStock / i.CurrentStock > 0.5m)
                    .CountAsync();

                // 確保數據類型一致性
                stats.Add("TotalProducts", totalProducts);
                stats.Add("TotalInventoryValue", totalInventoryValue);
                stats.Add("LowStockCount", lowStockCount);
                stats.Add("ZeroStockCount", zeroStockCount);
                stats.Add("WarehouseCount", warehouseCount);
                
                // 新增的統計項目
                stats.Add("NoWarningLevelCount", noWarningLevelCount);
                stats.Add("OverStockCount", overStockCount);
                stats.Add("StaleStockCount", staleStockCount);
                stats.Add("HighReservedStockCount", highReservedStockCount);

                return stats;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex,
                    nameof(GetInventoryStatisticsAsync),
                    GetType(),
                    _logger,
                    new {
                        Method = nameof(GetInventoryStatisticsAsync),
                        ServiceType = GetType().Name
                    }
                );
                return new Dictionary<string, object>();
            }
        }

        #endregion

        #region 庫存預留方法

        public async Task<ServiceResult> ReserveStockAsync(int productId, int warehouseId, int quantity,
            InventoryReservationType reservationType, string referenceNumber,
            DateTime? expiryDate = null, int? locationId = null, string? remarks = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("預留數量必須大於0");

                var available = await GetAvailableStockAsync(productId, warehouseId, locationId);
                if (available < quantity)
                    return ServiceResult.Failure($"可用庫存不足，目前可用：{available}");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var stock = await context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                                 i.WarehouseId == warehouseId &&
                                                 i.WarehouseLocationId == locationId && 
                                                 !i.IsDeleted);
                    
                    if (stock == null)
                        return ServiceResult.Failure("找不到庫存記錄");

                    // 增加預留數量
                    stock.ReservedStock += quantity;

                    // 建立預留記錄
                    var reservationNumber = $"RSV{DateTime.Now:yyyyMMddHHmmss}";
                    var reservation = new InventoryReservation
                    {
                        ReservationNumber = reservationNumber,
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        WarehouseLocationId = locationId,
                        ReservationType = reservationType,
                        ReservationStatus = InventoryReservationStatus.Reserved,
                        ReservationDate = DateTime.Now,
                        ExpiryDate = expiryDate,
                        ReservedQuantity = quantity,
                        ReleasedQuantity = 0,
                        ReferenceNumber = referenceNumber,
                        ReservationRemarks = remarks,
                        InventoryStockId = stock.Id,
                        Status = EntityStatus.Active
                    };

                    await context.InventoryReservations.AddAsync(reservation);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReserveStockAsync), GetType(), _logger, new {
                    Method = nameof(ReserveStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    ReservationType = reservationType,
                    ReferenceNumber = referenceNumber
                });
                return ServiceResult.Failure("庫存預留失敗");
            }
        }

        public async Task<ServiceResult> ReleaseReservationAsync(int reservationId, int? releaseQuantity = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var reservation = await context.InventoryReservations
                    .Include(r => r.InventoryStock)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                    return ServiceResult.Failure("找不到預留記錄");

                if (reservation.ReservationStatus != InventoryReservationStatus.Reserved &&
                    reservation.ReservationStatus != InventoryReservationStatus.PartiallyReleased)
                    return ServiceResult.Failure("預留記錄狀態不正確");

                var toRelease = releaseQuantity ?? reservation.RemainingQuantity;
                if (toRelease <= 0 || toRelease > reservation.RemainingQuantity)
                    return ServiceResult.Failure("釋放數量不正確");

                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 更新預留記錄
                    reservation.ReleasedQuantity += toRelease;
                    
                    if (reservation.RemainingQuantity == 0)
                        reservation.ReservationStatus = InventoryReservationStatus.Released;
                    else
                        reservation.ReservationStatus = InventoryReservationStatus.PartiallyReleased;

                    // 減少庫存預留數量
                    if (reservation.InventoryStock != null)
                    {
                        reservation.InventoryStock.ReservedStock -= toRelease;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReleaseReservationAsync), GetType(), _logger, new {
                    Method = nameof(ReleaseReservationAsync),
                    ServiceType = GetType().Name,
                    ReservationId = reservationId,
                    ReleaseQuantity = releaseQuantity
                });
                return ServiceResult.Failure("預留釋放失敗");
            }
        }

        public async Task<ServiceResult> CancelReservationAsync(int reservationId)
        {
            try
            {
                return await ReleaseReservationAsync(reservationId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelReservationAsync), GetType(), _logger, new {
                    Method = nameof(CancelReservationAsync),
                    ServiceType = GetType().Name,
                    ReservationId = reservationId
                });
                return ServiceResult.Failure("預留取消失敗");
            }
        }

        public async Task<List<InventoryReservation>> GetActiveReservationsAsync(int productId, int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryReservations
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ProductId == productId && 
                              r.WarehouseId == warehouseId && 
                              !r.IsDeleted &&
                              (r.ReservationStatus == InventoryReservationStatus.Reserved ||
                               r.ReservationStatus == InventoryReservationStatus.PartiallyReleased))
                    .OrderBy(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveReservationsAsync), GetType(), _logger, new {
                    Method = nameof(GetActiveReservationsAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId
                });
                return new List<InventoryReservation>();
            }
        }

        #endregion

        #region 庫存驗證方法

        public async Task<bool> IsStockAvailableAsync(int productId, int warehouseId, int requiredQuantity, int? locationId = null)
        {
            try
            {
                var available = await GetAvailableStockAsync(productId, warehouseId, locationId);
                return available >= requiredQuantity;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsStockAvailableAsync), GetType(), _logger, new {
                    Method = nameof(IsStockAvailableAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    RequiredQuantity = requiredQuantity,
                    LocationId = locationId
                });
                return false;
            }
        }

        public async Task<ServiceResult> ValidateStockOperationAsync(int productId, int warehouseId, int quantity, bool isReduce)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("數量必須大於0");

                var stock = await GetByProductWarehouseAsync(productId, warehouseId);
                
                if (isReduce)
                {
                    if (stock == null)
                        return ServiceResult.Failure("找不到庫存記錄");

                    if (stock.AvailableStock < quantity)
                        return ServiceResult.Failure($"可用庫存不足，目前可用：{stock.AvailableStock}，需要：{quantity}");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateStockOperationAsync), GetType(), _logger, new {
                    Method = nameof(ValidateStockOperationAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    IsReduce = isReduce
                });
                return ServiceResult.Failure("庫存操作驗證失敗");
            }
        }

        #endregion

        #region 覆寫基底類別方法

        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作庫存特定的刪除檢查和級聯刪除
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(InventoryStock entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否還有庫存數量
                if (entity.CurrentStock > 0 || entity.ReservedStock > 0)
                {
                    return ServiceResult.Failure($"無法刪除此庫存記錄，目前庫存：{entity.CurrentStock}，預留庫存：{entity.ReservedStock}");
                }

                // 檢查是否有相關的交易記錄，如果有則先軟刪除相關記錄
                var relatedTransactions = await context.InventoryTransactions
                    .Where(t => t.InventoryStockId == entity.Id && !t.IsDeleted)
                    .ToListAsync();

                if (relatedTransactions.Any())
                {
                    // 軟刪除相關的交易記錄
                    foreach (var transaction in relatedTransactions)
                    {
                        transaction.IsDeleted = true;
                        transaction.UpdatedAt = DateTime.UtcNow;
                        transaction.InventoryStockId = null; // 移除外鍵關聯
                    }
                }

                // 檢查是否有相關的預留記錄
                var relatedReservations = await context.InventoryReservations
                    .Where(r => r.InventoryStockId == entity.Id && !r.IsDeleted)
                    .ToListAsync();

                if (relatedReservations.Any())
                {
                    // 軟刪除相關的預留記錄
                    foreach (var reservation in relatedReservations)
                    {
                        reservation.IsDeleted = true;
                        reservation.UpdatedAt = DateTime.UtcNow;
                        reservation.InventoryStockId = null; // 移除外鍵關聯
                    }
                }

                // 儲存級聯軟刪除的變更
                if (relatedTransactions.Any() || relatedReservations.Any())
                {
                    await context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    ProductId = entity.ProductId,
                    WarehouseId = entity.WarehouseId,
                    WarehouseLocationId = entity.WarehouseLocationId
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion
    }
}

