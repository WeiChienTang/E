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
                    .OrderBy(i => i.Product.Code)
                    .ThenBy(i => i.Warehouse.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), typeof(InventoryStockService), _logger);
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
                        ((i.Product.Code != null && i.Product.Code.ToLower().Contains(term)) ||
                         (i.Product.Name != null && i.Product.Name.ToLower().Contains(term)) ||
                         (i.Warehouse.Code != null && i.Warehouse.Code.ToLower().Contains(term)) ||
                         (i.Warehouse.Name != null && i.Warehouse.Name.ToLower().Contains(term))))
                    .OrderBy(i => i.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), typeof(InventoryStockService), _logger);
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

                if (entity.WarehouseId <= 0)
                    errors.Add("必須選擇倉庫");

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), typeof(InventoryStockService), _logger);
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
                    .Include(i => i.Warehouse)
                    .Include(i => i.WarehouseLocation)
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .OrderBy(i => i.Warehouse.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductWarehouseAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLowStockItemsAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableStockAsync), typeof(InventoryStockService), _logger);
                return 0;
            }
        }

        #endregion

        #region 庫存異動方法

        public async Task<ServiceResult> AddStockAsync(int productId, int warehouseId, int quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, 
            decimal? unitCost = null, int? locationId = null, string? remarks = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("數量必須大於0");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得或建立庫存記錄
                    var stock = await context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                                 i.WarehouseId == warehouseId &&
                                                 i.WarehouseLocationId == locationId && 
                                                 !i.IsDeleted);
                    
                    if (stock == null)
                    {
                        stock = new InventoryStock
                        {
                            ProductId = productId,
                            WarehouseId = warehouseId,
                            WarehouseLocationId = locationId,
                            CurrentStock = 0,
                            ReservedStock = 0,
                            Status = EntityStatus.Active
                        };
                        await context.InventoryStocks.AddAsync(stock);
                        await context.SaveChangesAsync();
                    }

                    var stockBefore = stock.CurrentStock;
                    stock.CurrentStock += quantity;
                    stock.LastTransactionDate = DateTime.Now;

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddStockAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReduceStockAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TransferStockAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AdjustStockAsync), typeof(InventoryStockService), _logger);
                return ServiceResult.Failure("庫存調整失敗");
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
                    .OrderBy(i => i.Warehouse.Name)
                    .ThenBy(i => i.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex,
                    nameof(GetInventoryOverviewAsync),
                    GetType(),
                    _logger,
                    new { WarehouseId = warehouseId, CategoryId = categoryId, LocationId = locationId }
                );
                throw;
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
                    _logger
                );
                throw;
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

                // 確保數據類型一致性
                stats.Add("TotalProducts", totalProducts);
                stats.Add("TotalInventoryValue", totalInventoryValue);
                stats.Add("LowStockCount", lowStockCount);
                stats.Add("ZeroStockCount", zeroStockCount);
                stats.Add("WarehouseCount", warehouseCount);

                return stats;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex,
                    nameof(GetInventoryStatisticsAsync),
                    GetType(),
                    _logger
                );
                throw;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReserveStockAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReleaseReservationAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelReservationAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveReservationsAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsStockAvailableAsync), typeof(InventoryStockService), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateStockOperationAsync), typeof(InventoryStockService), _logger);
                return ServiceResult.Failure("庫存操作驗證失敗");
            }
        }

        #endregion
    }
}

