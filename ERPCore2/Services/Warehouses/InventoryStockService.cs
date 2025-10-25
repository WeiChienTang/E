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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .AsQueryable()
                    .OrderBy(i => i.Product!.Code)
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Include(i => i.InventoryTransactions)
                    .Include(i => i.InventoryReservations)
                    .FirstOrDefaultAsync(i => i.Id == id);
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => (i.Product!.Code != null && i.Product.Code.ToLower().Contains(term)) ||
                         (i.Product!.Name != null && i.Product.Name.ToLower().Contains(term)))
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

                // 新結構：驗證明細（如果有）
                if (entity.InventoryStockDetails != null && entity.InventoryStockDetails.Any())
                {
                    foreach (var detail in entity.InventoryStockDetails)
                    {
                        if (detail.CurrentStock < 0)
                            errors.Add($"倉庫 {detail.WarehouseId} 的庫存數量不能為負數");

                        if (detail.ReservedStock < 0)
                            errors.Add($"倉庫 {detail.WarehouseId} 的預留數量不能為負數");

                        if (detail.ReservedStock > detail.CurrentStock)
                            errors.Add($"倉庫 {detail.WarehouseId} 的預留數量不能大於現有庫存");
                    }
                }

                // 檢查是否已存在相同的庫存記錄（一個商品只能有一筆主檔）
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.InventoryStocks
                    .FirstOrDefaultAsync(i => i.ProductId == entity.ProductId && i.Id != entity.Id);

                if (existing != null)
                    errors.Add("該商品已有庫存主檔記錄");

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
                    ProductId = entity.ProductId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 覆寫 Create/Update 方法以處理明細

        /// <summary>
        /// 覆寫 CreateAsync 以處理庫存明細的新增
        /// </summary>
        public override async Task<ServiceResult<InventoryStock>> CreateAsync(InventoryStock entity)
        {
            try
            {
                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<InventoryStock>.Failure(validationResult.ErrorMessage);
                }

                // 檢查明細中是否有重複的倉庫+庫位組合
                var duplicateCheck = CheckDuplicateWarehouseLocations(entity.InventoryStockDetails?.ToList() ?? new List<InventoryStockDetail>());
                if (!duplicateCheck.IsSuccess)
                {
                    return ServiceResult<InventoryStock>.Failure(duplicateCheck.ErrorMessage);
                }

                // 設定建立資訊
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                
                if (entity.Status == default)
                {
                    entity.Status = EntityStatus.Active;
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 先新增主檔（不包含明細）
                    var detailsToAdd = entity.InventoryStockDetails?.ToList() ?? new List<InventoryStockDetail>();
                    entity.InventoryStockDetails = new List<InventoryStockDetail>();
                    
                    context.InventoryStocks.Add(entity);
                    await context.SaveChangesAsync(); // 儲存以取得主檔 ID

                    // 2. 新增明細並設定外鍵
                    if (detailsToAdd.Any())
                    {
                        foreach (var detail in detailsToAdd)
                        {
                            detail.InventoryStockId = entity.Id;
                            detail.CreatedAt = DateTime.UtcNow;
                            detail.UpdatedAt = DateTime.UtcNow;
                            detail.Status = EntityStatus.Active;
                            
                            context.InventoryStockDetails.Add(detail);
                        }
                        
                        await context.SaveChangesAsync();
                        entity.InventoryStockDetails = detailsToAdd;
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<InventoryStock>.Success(entity);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new {
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    ProductId = entity.ProductId,
                    DetailsCount = entity.InventoryStockDetails?.Count ?? 0
                });
                return ServiceResult<InventoryStock>.Failure($"建立庫存資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 覆寫 UpdateAsync 以處理庫存明細的更新
        /// </summary>
        public override async Task<ServiceResult<InventoryStock>> UpdateAsync(InventoryStock entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查實體是否存在
                var existingEntity = await context.InventoryStocks
                    .Include(i => i.InventoryStockDetails)
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                if (existingEntity == null)
                {
                    return ServiceResult<InventoryStock>.Failure("找不到要更新的資料");
                }

                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<InventoryStock>.Failure(validationResult.ErrorMessage);
                }

                // 檢查明細中是否有重複的倉庫+庫位組合
                var duplicateCheck = CheckDuplicateWarehouseLocations(entity.InventoryStockDetails?.ToList() ?? new List<InventoryStockDetail>());
                if (!duplicateCheck.IsSuccess)
                {
                    return ServiceResult<InventoryStock>.Failure(duplicateCheck.ErrorMessage);
                }

                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 更新主檔資訊
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.CreatedAt = existingEntity.CreatedAt;
                    entity.CreatedBy = existingEntity.CreatedBy;
                    
                    context.Entry(existingEntity).CurrentValues.SetValues(entity);

                    // 處理明細的增刪改
                    var existingDetails = existingEntity.InventoryStockDetails?.ToList() ?? new List<InventoryStockDetail>();
                    var newDetails = entity.InventoryStockDetails?.ToList() ?? new List<InventoryStockDetail>();

                    // 刪除不存在於新明細中的舊明細
                    foreach (var existingDetail in existingDetails)
                    {
                        var isStillPresent = newDetails.Any(d => d.Id == existingDetail.Id && existingDetail.Id > 0);
                        if (!isStillPresent)
                        {
                            context.InventoryStockDetails.Remove(existingDetail);
                        }
                    }

                    // 更新或新增明細
                    foreach (var newDetail in newDetails)
                    {
                        if (newDetail.Id > 0)
                        {
                            // 更新現有明細
                            var existingDetail = existingDetails.FirstOrDefault(d => d.Id == newDetail.Id);
                            if (existingDetail != null)
                            {
                                newDetail.UpdatedAt = DateTime.UtcNow;
                                newDetail.CreatedAt = existingDetail.CreatedAt;
                                newDetail.CreatedBy = existingDetail.CreatedBy;
                                newDetail.InventoryStockId = entity.Id;
                                
                                context.Entry(existingDetail).CurrentValues.SetValues(newDetail);
                            }
                        }
                        else
                        {
                            // 新增明細
                            newDetail.InventoryStockId = entity.Id;
                            newDetail.CreatedAt = DateTime.UtcNow;
                            newDetail.UpdatedAt = DateTime.UtcNow;
                            newDetail.Status = EntityStatus.Active;
                            
                            context.InventoryStockDetails.Add(newDetail);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult<InventoryStock>.Success(entity);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new {
                    Method = nameof(UpdateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    ProductId = entity.ProductId,
                    DetailsCount = entity.InventoryStockDetails?.Count ?? 0
                });
                return ServiceResult<InventoryStock>.Failure($"更新庫存資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查明細中是否有重複的倉庫+庫位組合
        /// </summary>
        private ServiceResult CheckDuplicateWarehouseLocations(List<InventoryStockDetail> details)
        {
            if (details == null || !details.Any())
            {
                return ServiceResult.Success();
            }

            // 找出重複的倉庫+庫位組合
            var duplicates = details
                .GroupBy(d => new { d.WarehouseId, d.WarehouseLocationId })
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    g.Key.WarehouseId,
                    g.Key.WarehouseLocationId,
                    Count = g.Count(),
                    Details = g.ToList()
                })
                .ToList();

            if (duplicates.Any())
            {
                var errorMessages = new List<string>();
                errorMessages.Add("發現重複的倉庫與庫位組合：");
                
                foreach (var dup in duplicates)
                {
                    var locationInfo = dup.WarehouseLocationId.HasValue 
                        ? $"倉庫ID {dup.WarehouseId}，庫位ID {dup.WarehouseLocationId}" 
                        : $"倉庫ID {dup.WarehouseId}（無指定庫位）";
                    errorMessages.Add($"- {locationInfo}（共 {dup.Count} 筆）");
                }
                
                errorMessages.Add(""); // 空行
                errorMessages.Add("❌ 無法儲存重複的倉庫與庫位組合");
                errorMessages.Add("💡 請修改為不同的倉庫或庫位，或使用合併功能");
                
                return ServiceResult.Failure(string.Join("\n", errorMessages));
            }

            return ServiceResult.Success();
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.ProductId == productId)
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.InventoryStockDetails.Any(d => d.WarehouseId == warehouseId))
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
                // 現在改為查詢主檔，然後透過明細篩選
                return await context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.ProductId == productId &&
                               i.InventoryStockDetails.Any(d => d.WarehouseId == warehouseId &&
                                                               (locationId == null || d.WarehouseLocationId == locationId)))
                    .FirstOrDefaultAsync();
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
                var query = context.InventoryStocks
                    .Include(i => i.Product)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.ProductId == productId);

                if (warehouseId.HasValue)
                {
                    query = query.Where(i => i.InventoryStockDetails.Any(d => d.WarehouseId == warehouseId.Value &&
                                                                              (locationId == null || d.WarehouseLocationId == locationId)));
                }

                return await query.FirstOrDefaultAsync();
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.InventoryStockDetails.Any(d => 
                              d.MinStockLevel.HasValue && 
                              d.CurrentStock <= d.MinStockLevel.Value))
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
                if (stock == null) return 0;

                // 計算指定倉庫和位置的可用庫存
                var detail = stock.InventoryStockDetails?
                    .FirstOrDefault(d => d.WarehouseId == warehouseId && 
                                        (locationId == null || d.WarehouseLocationId == locationId));
                
                return detail?.AvailableStock ?? 0;
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
        /// 取得商品在指定倉庫和位置的庫存明細（含詳細資訊）
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="warehouseId">倉庫ID</param>
        /// <param name="locationId">庫位ID（可選）</param>
        /// <returns>庫存明細</returns>
        public async Task<InventoryStockDetail?> GetStockDetailAsync(int productId, int warehouseId, int? locationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.InventoryStock.ProductId == productId && d.WarehouseId == warehouseId);
                
                if (locationId.HasValue)
                {
                    query = query.Where(d => d.WarehouseLocationId == locationId.Value);
                }
                
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStockDetailAsync), GetType(), _logger, new { 
                    Method = nameof(GetStockDetailAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    LocationId = locationId 
                });
                return null;
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
                
                // 取得該商品的庫存主檔
                var stock = await context.InventoryStocks
                    .Include(i => i.InventoryStockDetails)
                    .Where(i => i.ProductId == productId)
                    .FirstOrDefaultAsync();
                
                if (stock == null) return 0;

                // 計算指定倉庫內所有位置的總可用庫存
                var totalAvailableStock = stock.InventoryStockDetails?
                    .Where(d => d.WarehouseId == warehouseId)
                    .Sum(d => d.AvailableStock) ?? 0;
                    
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

        /// <summary>
        /// 根據銷貨訂單ID查找相關的庫存交易記錄（用於回滾）
        /// </summary>
        public async Task<List<InventoryTransaction>> GetInventoryTransactionsBySalesOrderAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 先找到所有該銷貨訂單的交易記錄（包含回滾記錄）
                var allTransactions = await context.InventoryTransactions
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Include(t => t.InventoryStock)
                    .Where(t => t.TransactionNumber.StartsWith($"SO-{salesOrderId}"))
                    .OrderBy(t => t.TransactionDate)
                    .ThenBy(t => t.Id)
                    .ToListAsync();
                
                // 分離銷貨記錄和回滾記錄
                var saleTransactions = allTransactions
                    .Where(t => !t.TransactionNumber.Contains("_REVERT") && 
                               t.TransactionType == InventoryTransactionTypeEnum.Sale)
                    .ToList();
                
                var revertTransactions = allTransactions
                    .Where(t => t.TransactionNumber.Contains("_REVERT") && 
                               t.TransactionType == InventoryTransactionTypeEnum.Return)
                    .ToList();
                
                Console.WriteLine($"[GetInventoryTransactionsBySalesOrderAsync] 找到 {saleTransactions.Count} 筆銷貨記錄，{revertTransactions.Count} 筆回滾記錄");
                
                // 計算每個InventoryStockId的淨扣減量
                var netReductions = new Dictionary<int, int>();
                
                // 加入所有銷貨扣減
                foreach (var sale in saleTransactions)
                {
                    if (sale.InventoryStockId.HasValue)
                    {
                        var stockId = sale.InventoryStockId.Value;
                        if (!netReductions.ContainsKey(stockId))
                            netReductions[stockId] = 0;
                        netReductions[stockId] += Math.Abs(sale.Quantity); // 累計扣減量（轉為正數）
                    }
                }
                
                // 減去已回滾的量
                foreach (var revert in revertTransactions)
                {
                    if (revert.InventoryStockId.HasValue)
                    {
                        var stockId = revert.InventoryStockId.Value;
                        if (netReductions.ContainsKey(stockId))
                        {
                            netReductions[stockId] -= revert.Quantity; // 回滾是正數，所以用減法
                        }
                    }
                }
                
                // 創建需要回滾的虛擬交易記錄（只包含淨扣減量大於0的）
                var virtualTransactions = new List<InventoryTransaction>();
                foreach (var kvp in netReductions.Where(x => x.Value > 0))
                {
                    // 找到對應的原始銷貨記錄來獲取詳細信息
                    var originalSale = saleTransactions.FirstOrDefault(s => s.InventoryStockId == kvp.Key);
                    if (originalSale != null)
                    {
                        virtualTransactions.Add(new InventoryTransaction
                        {
                            Id = originalSale.Id, // 使用原始記錄ID
                            ProductId = originalSale.ProductId,
                            WarehouseId = originalSale.WarehouseId,
                            WarehouseLocationId = originalSale.WarehouseLocationId,
                            InventoryStockId = originalSale.InventoryStockId,
                            Quantity = -kvp.Value, // 負數表示需要回滾的量
                            TransactionNumber = originalSale.TransactionNumber,
                            TransactionType = originalSale.TransactionType,
                            Product = originalSale.Product,
                            Warehouse = originalSale.Warehouse,
                            WarehouseLocation = originalSale.WarehouseLocation,
                            InventoryStock = originalSale.InventoryStock
                        });
                    }
                }
                
                Console.WriteLine($"[GetInventoryTransactionsBySalesOrderAsync] 計算出 {virtualTransactions.Count} 筆需要回滾的淨記錄");
                return virtualTransactions;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetInventoryTransactionsBySalesOrderAsync), GetType(), _logger, new { 
                    Method = nameof(GetInventoryTransactionsBySalesOrderAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId 
                });
                return new List<InventoryTransaction>();
            }
        }

        #endregion

        #region 庫存異動方法

        /// <summary>
        /// 精確回滾庫存到原始記錄（基於 InventoryStockDetailId）
        /// 注意：此方法已過時，現在使用 InventoryStockDetailId 而非 InventoryStockId
        /// </summary>
        [Obsolete("請使用 RevertStockToOriginalDetailAsync，傳入 InventoryStockDetailId")]
        public async Task<ServiceResult> RevertStockToOriginalAsync(
            int inventoryStockId, 
            int quantity, 
            InventoryTransactionTypeEnum transactionType, 
            string transactionNumber, 
            string? remarks = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("回滾數量必須大於0");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 注意：此方法已過時，但為了向後兼容，將 inventoryStockId 視為 inventoryStockDetailId
                    var originalDetail = await context.InventoryStockDetails
                        .Include(d => d.InventoryStock)
                        .FirstOrDefaultAsync(d => d.Id == inventoryStockId);

                    if (originalDetail == null)
                    {
                        Console.WriteLine($"[RevertStockToOriginalAsync] 找不到原始庫存明細記錄 ID: {inventoryStockId}");
                        return ServiceResult.Failure($"ORIGINAL_NOT_FOUND:{inventoryStockId}");
                    }

                    var stockBefore = originalDetail.CurrentStock;
                    originalDetail.CurrentStock += quantity; // 回滾是增加庫存
                    originalDetail.LastTransactionDate = DateTime.Now;

                    Console.WriteLine($"[RevertStockToOriginalAsync] 精確回滾: DetailId={inventoryStockId}, 產品={originalDetail.InventoryStock?.ProductId}, " +
                                    $"倉庫={originalDetail.WarehouseId}, 位置={originalDetail.WarehouseLocationId}, " +
                                    $"批號={originalDetail.BatchNumber}, 數量={quantity}, 回滾前={stockBefore}, 回滾後={originalDetail.CurrentStock}");

                    // 建立回滾交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = originalDetail.InventoryStock?.ProductId ?? 0,
                        WarehouseId = originalDetail.WarehouseId,
                        WarehouseLocationId = originalDetail.WarehouseLocationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = quantity, // 正數表示入庫（回滾）
                        UnitCost = originalDetail.AverageCost,
                        StockBefore = stockBefore,
                        StockAfter = originalDetail.CurrentStock,
                        Remarks = remarks,
                        InventoryStockId = originalDetail.InventoryStockId,
                        InventoryStockDetailId = originalDetail.Id,
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RevertStockToOriginalAsync), GetType(), _logger, new {
                    Method = nameof(RevertStockToOriginalAsync),
                    ServiceType = GetType().Name,
                    InventoryStockId = inventoryStockId,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    Remarks = remarks
                });
                return ServiceResult.Failure("精確庫存回滾失敗");
            }
        }

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
                    // 1. 取得或建立庫存主檔（依商品）
                    var stock = await context.InventoryStocks
                        .Include(s => s.InventoryStockDetails)
                        .FirstOrDefaultAsync(i => i.ProductId == productId);
                    
                    if (stock == null)
                    {
                        // 建立新的庫存主檔
                        stock = new InventoryStock
                        {
                            ProductId = productId,
                            Status = EntityStatus.Active
                        };
                        await context.InventoryStocks.AddAsync(stock);
                        await context.SaveChangesAsync(); // 儲存以取得 Id
                    }

                    // 2. 取得或建立庫存明細（依倉庫+庫位）
                    var detail = stock.InventoryStockDetails?
                        .FirstOrDefault(d => d.WarehouseId == warehouseId && 
                                            d.WarehouseLocationId == locationId);
                    
                    if (detail == null)
                    {
                        detail = new InventoryStockDetail
                        {
                            InventoryStockId = stock.Id,
                            WarehouseId = warehouseId,
                            WarehouseLocationId = locationId,
                            CurrentStock = 0,
                            ReservedStock = 0,
                            InTransitStock = 0,
                            BatchNumber = batchNumber,
                            BatchDate = batchDate ?? DateTime.Now,
                            ExpiryDate = expiryDate,
                            LastTransactionDate = DateTime.Now,
                            Status = EntityStatus.Active
                        };
                        await context.InventoryStockDetails.AddAsync(detail);
                        await context.SaveChangesAsync();
                    }

                    // 3. 更新庫存數量
                    var stockBefore = detail.CurrentStock;
                    detail.CurrentStock += quantity;
                    detail.LastTransactionDate = DateTime.Now;

                    // 更新批次資訊（如果提供）
                    if (!string.IsNullOrEmpty(batchNumber))
                        detail.BatchNumber = batchNumber;
                    if (batchDate.HasValue)
                        detail.BatchDate = batchDate.Value;
                    if (expiryDate.HasValue)
                        detail.ExpiryDate = expiryDate.Value;

                    // 4. 更新平均成本
                    if (unitCost.HasValue && unitCost.Value > 0)
                    {
                        if (detail.AverageCost.HasValue && stockBefore > 0)
                        {
                            var totalCostBefore = detail.AverageCost.Value * stockBefore;
                            var newTotalCost = totalCostBefore + (unitCost.Value * quantity);
                            detail.AverageCost = newTotalCost / detail.CurrentStock;
                        }
                        else
                        {
                            detail.AverageCost = unitCost.Value;
                        }
                    }

                    // 5. 建立交易記錄
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
                        StockAfter = detail.CurrentStock,
                        Remarks = remarks,
                        InventoryStockId = stock.Id,
                        InventoryStockDetailId = detail.Id,
                        TransactionBatchNumber = batchNumber,
                        TransactionBatchDate = batchDate,
                        TransactionExpiryDate = expiryDate,
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

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 取得庫存主檔
                    var stock = await context.InventoryStocks
                        .Include(s => s.InventoryStockDetails)
                        .FirstOrDefaultAsync(i => i.ProductId == productId);
                    
                    if (stock == null)
                        return ServiceResult.Failure("找不到庫存記錄");

                    // 2. 取得指定倉庫/庫位的明細
                    var detail = stock.InventoryStockDetails?
                        .FirstOrDefault(d => d.WarehouseId == warehouseId && 
                                            (locationId == null || d.WarehouseLocationId == locationId));
                    
                    if (detail == null)
                        return ServiceResult.Failure($"找不到倉庫 {warehouseId} 的庫存記錄");

                    if (detail.AvailableStock < quantity)
                        return ServiceResult.Failure($"可用庫存不足，目前可用庫存：{detail.AvailableStock}");

                    // 3. 更新庫存數量
                    var stockBefore = detail.CurrentStock;
                    detail.CurrentStock -= quantity;
                    detail.LastTransactionDate = DateTime.Now;

                    // 4. 建立交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseId = warehouseId,
                        WarehouseLocationId = locationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = -quantity, // 負數表示出庫
                        UnitCost = detail.AverageCost,
                        StockBefore = stockBefore,
                        StockAfter = detail.CurrentStock,
                        Remarks = remarks,
                        InventoryStockId = stock.Id,
                        InventoryStockDetailId = detail.Id,
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

                    // 取得來源倉庫的平均成本
                    var fromStock = await GetByProductWarehouseAsync(productId, fromWarehouseId, fromLocationId);
                    var fromDetail = fromStock?.InventoryStockDetails?
                        .FirstOrDefault(d => d.WarehouseId == fromWarehouseId && 
                                            (fromLocationId == null || d.WarehouseLocationId == fromLocationId));
                    
                    // 增加到目標倉庫
                    var addResult = await AddStockAsync(productId, toWarehouseId, quantity,
                        InventoryTransactionTypeEnum.Transfer, transactionNumber, 
                        fromDetail?.AverageCost, toLocationId, remarks);

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

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得庫存主檔
                var stock = await context.InventoryStocks
                    .Include(s => s.InventoryStockDetails)
                    .FirstOrDefaultAsync(i => i.ProductId == productId);
                
                if (stock == null)
                    return ServiceResult.Failure("找不到庫存記錄");

                // 取得指定倉庫/庫位的明細
                var detail = stock.InventoryStockDetails?
                    .FirstOrDefault(d => d.WarehouseId == warehouseId && 
                                        (locationId == null || d.WarehouseLocationId == locationId));
                
                if (detail == null)
                    return ServiceResult.Failure($"找不到倉庫 {warehouseId} 的庫存記錄");

                var difference = newQuantity - detail.CurrentStock;
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
            int? locationId = null, string? remarks = null, int? salesOrderDetailId = null)
        {
            try
            {
                // 取得該商品在指定倉庫的所有批號庫存，按批次日期排序 (FIFO)
                var batchDetails = await GetBatchDetailsByProductAndWarehouseAsync(productId, warehouseId, locationId);
                
                var totalAvailable = batchDetails.Sum(b => b.AvailableStock);
                if (totalAvailable < quantity)
                    return ServiceResult.Failure($"庫存不足，可用：{totalAvailable}，需要：{quantity}");

                var remainingQuantity = quantity;
                var reductionDetails = new List<BatchReductionDetail>();

                // 按批次日期順序扣減 (FIFO)
                foreach (var detail in batchDetails.OrderBy(d => d.BatchDate).ThenBy(d => d.Id))
                {
                    if (remainingQuantity <= 0) break;
                    
                    var availableFromThis = detail.AvailableStock;
                    if (availableFromThis <= 0) continue;
                    
                    var reduceFromThis = Math.Min(availableFromThis, remainingQuantity);
                    
                    reductionDetails.Add(new BatchReductionDetail 
                    {
                        BatchId = detail.Id,
                        BatchNumber = detail.BatchNumber,
                        ReduceQuantity = reduceFromThis
                    });
                    
                    remainingQuantity -= reduceFromThis;
                }

                // 執行實際扣減
                foreach (var detail in reductionDetails)
                {
                    var result = await ReduceStockFromSpecificBatchAsync(
                        detail.BatchId, detail.ReduceQuantity, 
                        transactionType, transactionNumber, remarks, salesOrderDetailId);
                        
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
        /// 取得商品在指定倉庫的批號明細清單（新版：使用 InventoryStockDetail）
        /// </summary>
        private async Task<List<InventoryStockDetail>> GetBatchDetailsByProductAndWarehouseAsync(
            int productId, int warehouseId, int? locationId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var stock = await context.InventoryStocks
                .Include(s => s.InventoryStockDetails)
                .FirstOrDefaultAsync(s => s.ProductId == productId);
            
            if (stock == null) return new List<InventoryStockDetail>();

            var query = stock.InventoryStockDetails
                .Where(d => d.WarehouseId == warehouseId && d.CurrentStock > 0);
            
            // 如果指定了位置，才篩選特定位置；否則查詢整個倉庫
            if (locationId.HasValue)
            {
                query = query.Where(d => d.WarehouseLocationId == locationId.Value);
            }
            
            return query
                .OrderBy(d => d.BatchDate)  // FIFO 排序
                .ThenBy(d => d.Id)         // 相同日期按ID排序
                .ToList();
        }

        /// <summary>
        /// 取得商品在指定倉庫的批號庫存清單（舊版：保留向後兼容）
        /// </summary>
        [Obsolete("請使用 GetBatchDetailsByProductAndWarehouseAsync")]
        private Task<List<InventoryStock>> GetBatchStocksByProductAndWarehouseAsync(
            int productId, int warehouseId, int? locationId = null)
        {
            // 此方法已過時，返回空列表
            return Task.FromResult(new List<InventoryStock>());
        }

        /// <summary>
        /// 從特定批號扣減庫存（新版：使用 InventoryStockDetailId）
        /// </summary>
        private async Task<ServiceResult> ReduceStockFromSpecificBatchAsync(
            int batchDetailId, int quantity, 
            InventoryTransactionTypeEnum transactionType, string transactionNumber, 
            string? remarks = null, int? salesOrderDetailId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var detail = await context.InventoryStockDetails
                        .Include(d => d.InventoryStock)
                        .FirstOrDefaultAsync(d => d.Id == batchDetailId);
                    
                    if (detail == null)
                        return ServiceResult.Failure("找不到批號庫存記錄");

                    if (detail.AvailableStock < quantity)
                        return ServiceResult.Failure($"批號 {detail.BatchNumber} 可用庫存不足");

                    var stockBefore = detail.CurrentStock;
                    detail.CurrentStock -= quantity;
                    detail.LastTransactionDate = DateTime.Now;

                    // 建立交易記錄
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = detail.InventoryStock?.ProductId ?? 0,
                        WarehouseId = detail.WarehouseId,
                        WarehouseLocationId = detail.WarehouseLocationId,
                        TransactionType = transactionType,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = -quantity, // 負數表示出庫
                        UnitCost = detail.AverageCost,
                        StockBefore = stockBefore,
                        StockAfter = detail.CurrentStock,
                        Remarks = $"{remarks} (批號: {detail.BatchNumber})" + (salesOrderDetailId.HasValue ? $" (銷貨明細ID: {salesOrderDetailId})" : ""),
                        InventoryStockId = detail.InventoryStockId,
                        InventoryStockDetailId = detail.Id,
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
                    BatchDetailId = batchDetailId,
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .AsQueryable();

                // 篩選條件（透過明細篩選）
                if (warehouseId.HasValue || locationId.HasValue)
                {
                    query = query.Where(i => i.InventoryStockDetails.Any(d =>
                        (!warehouseId.HasValue || d.WarehouseId == warehouseId.Value) &&
                        (!locationId.HasValue || d.WarehouseLocationId == locationId.Value)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(i => i.Product.ProductCategoryId == categoryId.Value);
                }

                return await query
                    .OrderBy(i => i.Product!.Code)
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
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(i => i.InventoryStockDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(i => i.InventoryStockDetails.Any(d => 
                               d.MinStockLevel.HasValue && 
                               d.CurrentStock <= d.MinStockLevel.Value))
                    .OrderBy(i => i.TotalCurrentStock)
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

                // 載入所有庫存資料到記憶體進行統計（因為使用計算屬性）
                var allStocks = await context.InventoryStocks
                    .Include(i => i.InventoryStockDetails)
                    .ToListAsync();

                // 總商品數量
                var totalProducts = allStocks.Select(i => i.ProductId).Distinct().Count();

                // 總庫存價值（使用明細計算）
                var totalInventoryValue = allStocks
                    .SelectMany(s => s.InventoryStockDetails ?? Enumerable.Empty<InventoryStockDetail>())
                    .Where(d => d.AverageCost.HasValue)
                    .Sum(d => d.CurrentStock * (d.AverageCost ?? 0));

                // 低庫存警戒：計算有多少筆庫位明細低於最低警戒線
                var lowStockCount = allStocks
                    .SelectMany(i => i.InventoryStockDetails ?? Enumerable.Empty<InventoryStockDetail>())
                    .Count(d => d.MinStockLevel.HasValue && d.CurrentStock <= d.MinStockLevel.Value);

                // 零庫存商品數量
                var zeroStockCount = allStocks
                    .Where(i => i.TotalCurrentStock == 0)
                    .Count();

                // 倉庫數量
                var warehouseCount = await context.Warehouses.CountAsync();

                // 未設警戒線：計算有多少筆庫位明細未設定任何警戒線
                var noWarningLevelCount = allStocks
                    .SelectMany(i => i.InventoryStockDetails ?? Enumerable.Empty<InventoryStockDetail>())
                    .Count(d => (!d.MinStockLevel.HasValue || d.MinStockLevel.Value == 0) &&
                               (!d.MaxStockLevel.HasValue || d.MaxStockLevel.Value == 0));

                // 庫存過多警戒：計算有多少筆庫位明細超過最高警戒線
                var overStockCount = allStocks
                    .SelectMany(i => i.InventoryStockDetails ?? Enumerable.Empty<InventoryStockDetail>())
                    .Count(d => d.MaxStockLevel.HasValue &&
                               d.MaxStockLevel.Value > 0 &&
                               d.CurrentStock > d.MaxStockLevel.Value);

                // 呆滯庫存數量（30天沒有異動）- 從明細判斷
                var staleStockCount = allStocks
                    .Where(i => i.InventoryStockDetails != null &&
                               i.InventoryStockDetails.All(d => !d.LastTransactionDate.HasValue ||
                                                               d.LastTransactionDate.Value <= DateTime.Now.AddDays(-30)))
                    .Count();

                // 預留庫存過高的商品數量（預留庫存佔總庫存比例 > 50%）
                var highReservedStockCount = allStocks
                    .Where(i => i.TotalCurrentStock > 0 &&
                               i.TotalReservedStock > 0 &&
                               (decimal)i.TotalReservedStock / i.TotalCurrentStock > 0.5m)
                    .Count();

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
                        .Include(i => i.InventoryStockDetails)
                        .FirstOrDefaultAsync(i => i.ProductId == productId);
                    
                    if (stock == null)
                        return ServiceResult.Failure("找不到庫存記錄");

                    // 找到或建立對應的明細記錄
                    var detail = stock.InventoryStockDetails
                        .FirstOrDefault(d => d.WarehouseId == warehouseId && 
                                           d.WarehouseLocationId == locationId);
                    
                    if (detail == null)
                        return ServiceResult.Failure("找不到指定倉庫位置的庫存明細");

                    // 增加預留數量
                    detail.ReservedStock += quantity;

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
                        InventoryStockDetailId = detail.Id,
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
                    .Include(r => r.InventoryStockDetail)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

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
                    if (reservation.InventoryStockDetail != null)
                    {
                        reservation.InventoryStockDetail.ReservedStock -= toRelease;
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

                    if (stock.TotalAvailableStock < quantity)
                        return ServiceResult.Failure($"可用庫存不足，目前可用：{stock.TotalAvailableStock}，需要：{quantity}");
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
                if (entity.TotalCurrentStock > 0 || entity.TotalReservedStock > 0)
                {
                    return ServiceResult.Failure($"無法刪除此庫存記錄，目前庫存：{entity.TotalCurrentStock}，預留庫存：{entity.TotalReservedStock}");
                }

                // 檢查是否有相關的交易記錄，如果有則先軟刪除相關記錄
                var relatedTransactions = await context.InventoryTransactions
                    .Where(t => t.InventoryStockId == entity.Id)
                    .ToListAsync();

                if (relatedTransactions.Any())
                {
                    // 移除相關的交易記錄的外鍵關聯
                    foreach (var transaction in relatedTransactions)
                    {
                        transaction.UpdatedAt = DateTime.UtcNow;
                        transaction.InventoryStockId = null; // 移除外鍵關聯
                    }
                }

                // 檢查是否有相關的預留記錄
                var relatedReservations = await context.InventoryReservations
                    .Where(r => r.InventoryStockId == entity.Id)
                    .ToListAsync();

                if (relatedReservations.Any())
                {
                    // 移除相關的預留記錄的外鍵關聯，或直接刪除
                    foreach (var reservation in relatedReservations)
                    {
                        reservation.UpdatedAt = DateTime.UtcNow;
                        reservation.InventoryStockId = null; // 移除外鍵關聯
                    }
                }

                // 儲存級聯刪除的變更
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
                    ProductId = entity.ProductId
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 警戒線管理方法

        /// <summary>
        /// 取得所有未設定警戒線的庫存明細
        /// </summary>
        public async Task<List<InventoryStockDetail>> GetStockDetailsWithoutAlertAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => (!d.MinStockLevel.HasValue || d.MinStockLevel.Value == 0) &&
                               (!d.MaxStockLevel.HasValue || d.MaxStockLevel.Value == 0))
                    .OrderBy(d => d.InventoryStock.Product.Name)
                    .ThenBy(d => d.Warehouse.Name)
                    .ThenBy(d => d.WarehouseLocation!.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStockDetailsWithoutAlertAsync), GetType(), _logger);
                return new List<InventoryStockDetail>();
            }
        }

        /// <summary>
        /// 批次更新庫存明細的警戒線設定
        /// </summary>
        public async Task<ServiceResult> BatchUpdateStockLevelAlertsAsync(List<(int detailId, int? minLevel, int? maxLevel)> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                {
                    return ServiceResult.Failure("沒有需要更新的資料");
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得所有需要更新的明細
                var detailIds = updates.Select(u => u.detailId).ToList();
                var details = await context.InventoryStockDetails
                    .Where(d => detailIds.Contains(d.Id))
                    .ToListAsync();

                if (!details.Any())
                {
                    return ServiceResult.Failure("找不到需要更新的庫存明細");
                }

                // 驗證並更新警戒線設定
                var errors = new List<string>();
                var updatedCount = 0;

                foreach (var update in updates)
                {
                    var detail = details.FirstOrDefault(d => d.Id == update.detailId);
                    if (detail == null)
                    {
                        errors.Add($"找不到ID為 {update.detailId} 的庫存明細");
                        continue;
                    }

                    // 驗證警戒線設定
                    if (update.minLevel.HasValue && update.minLevel.Value < 0)
                    {
                        errors.Add($"明細 {detail.Id} 的最低警戒線不能為負數");
                        continue;
                    }

                    if (update.maxLevel.HasValue && update.maxLevel.Value < 0)
                    {
                        errors.Add($"明細 {detail.Id} 的最高警戒線不能為負數");
                        continue;
                    }

                    if (update.minLevel.HasValue && update.maxLevel.HasValue && 
                        update.minLevel.Value > update.maxLevel.Value)
                    {
                        errors.Add($"明細 {detail.Id} 的最低警戒線不能大於最高警戒線");
                        continue;
                    }

                    // 更新警戒線
                    detail.MinStockLevel = update.minLevel;
                    detail.MaxStockLevel = update.maxLevel;
                    detail.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }

                if (errors.Any())
                {
                    return ServiceResult.Failure($"部分更新失敗：{string.Join(", ", errors)}");
                }

                // 儲存變更
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateStockLevelAlertsAsync), GetType(), _logger);
                return ServiceResult.Failure("批次更新警戒線設定時發生錯誤");
            }
        }

        /// <summary>
        /// 取得所有低庫存的庫存明細（當前庫存低於最低警戒線）
        /// </summary>
        public async Task<List<InventoryStockDetail>> GetLowStockDetailsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MinStockLevel.HasValue && 
                               d.MinStockLevel.Value > 0 && 
                               d.CurrentStock < d.MinStockLevel.Value)
                    .OrderBy(d => d.InventoryStock.Product.Name)
                    .ThenBy(d => d.Warehouse.Name)
                    .ThenBy(d => d.WarehouseLocation!.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLowStockDetailsAsync), GetType(), _logger);
                return new List<InventoryStockDetail>();
            }
        }

        /// <summary>
        /// 取得所有庫存過多的庫存明細（當前庫存高於最高警戒線）
        /// </summary>
        public async Task<List<InventoryStockDetail>> GetOverStockDetailsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MaxStockLevel.HasValue && 
                               d.MaxStockLevel.Value > 0 && 
                               d.CurrentStock > d.MaxStockLevel.Value)
                    .OrderBy(d => d.InventoryStock.Product.Name)
                    .ThenBy(d => d.Warehouse.Name)
                    .ThenBy(d => d.WarehouseLocation!.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOverStockDetailsAsync), GetType(), _logger);
                return new List<InventoryStockDetail>();
            }
        }

        #endregion
    }
}


