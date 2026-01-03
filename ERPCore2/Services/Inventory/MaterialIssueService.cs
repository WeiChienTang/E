using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨服務實作
    /// </summary>
    public class MaterialIssueService : GenericManagementService<MaterialIssue>, IMaterialIssueService
    {
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly IMaterialIssueDetailService? _materialIssueDetailService;

        public MaterialIssueService(
            IDbContextFactory<AppDbContext> contextFactory,
            IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService) : base(contextFactory)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        public MaterialIssueService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<MaterialIssue>> logger,
            IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService,
            IMaterialIssueDetailService materialIssueDetailService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
            _materialIssueDetailService = materialIssueDetailService;
        }

        #region 覆寫基底方法 - 包含庫存處理

        /// <summary>
        /// 建立領料單並扣除庫存
        /// </summary>
        public override async Task<ServiceResult<MaterialIssue>> CreateAsync(MaterialIssue entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. 先執行基本的建立邏輯（包含驗證）
                var createResult = await base.CreateAsync(entity);
                if (!createResult.IsSuccess || createResult.Data == null)
                {
                    return createResult;
                }

                var materialIssue = createResult.Data;

                // 2. 載入領料明細
                var details = await context.MaterialIssueDetails
                    .Where(d => d.MaterialIssueId == materialIssue.Id && d.Status == EntityStatus.Active)
                    .ToListAsync();

                if (!details.Any())
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<MaterialIssue>.Failure("領料單沒有明細資料，無法扣除庫存");
                }

                // 3. 逐筆扣除庫存
                foreach (var detail in details)
                {
                    // 扣除庫存（ReduceStockAsync 內部已經會建立庫存交易記錄）
                    var reduceResult = await _inventoryStockService.ReduceStockAsync(
                        detail.ProductId,
                        detail.WarehouseId,
                        detail.IssueQuantity,
                        InventoryTransactionTypeEnum.MaterialIssue,
                        materialIssue.Code ?? $"MI-{materialIssue.Id}",
                        detail.WarehouseLocationId,
                        $"領料單號：{materialIssue.Code}");

                    if (!reduceResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<MaterialIssue>.Failure($"扣除庫存失敗：{reduceResult.ErrorMessage}");
                    }
                }

                await transaction.CommitAsync();
                return ServiceResult<MaterialIssue>.Success(materialIssue);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult<MaterialIssue>.Failure($"建立領料單並扣除庫存時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新領料單（不處理庫存，庫存由 UpdateInventoryByDifferenceAsync 單獨處理）
        /// </summary>
        public override async Task<ServiceResult<MaterialIssue>> UpdateAsync(MaterialIssue entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 更新主檔
                    var existingEntity = await context.MaterialIssues
                        .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                    if (existingEntity == null)
                        return ServiceResult<MaterialIssue>.Failure("找不到要更新的資料");

                    // 驗證實體
                    var validationResult = await ValidateAsync(entity);
                    if (!validationResult.IsSuccess)
                        return ServiceResult<MaterialIssue>.Failure(validationResult.ErrorMessage);

                    // 保持原建立資訊
                    entity.CreatedAt = existingEntity.CreatedAt;
                    entity.CreatedBy = existingEntity.CreatedBy;
                    entity.UpdatedAt = DateTime.UtcNow;

                    // 分離舊實體並附加新實體
                    context.Entry(existingEntity).State = EntityState.Detached;
                    context.Entry(entity).State = EntityState.Modified;
                    await context.SaveChangesAsync();

                    // 2. 更新明細（使用內建 context 和 transaction）
                    if (_materialIssueDetailService != null)
                    {
                        var detailResult = await _materialIssueDetailService.UpdateDetailsInContextAsync(
                            context, 
                            entity.Id, 
                            entity.MaterialIssueDetails?.ToList() ?? new List<MaterialIssueDetail>(),
                            transaction);
                        
                        if (!detailResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult<MaterialIssue>.Failure(detailResult.ErrorMessage);
                        }
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<MaterialIssue>.Success(entity);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = entity.Id
                });
                return ServiceResult<MaterialIssue>.Failure($"更新領料單時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 永久刪除領料單並還原庫存（使用 _DEL 批次邊界標記）
        /// 這是UI實際調用的刪除方法
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 先取得主記錄（含明細資料）
                    var entity = await context.MaterialIssues
                        .Include(mi => mi.MaterialIssueDetails)
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (entity == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }

                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        return canDeleteResult;
                    }

                    // 3. 檢查是否有庫存服務可用並進行庫存回滾
                    if (_inventoryStockService != null)
                    {
                        var eligibleDetails = entity.MaterialIssueDetails
                            .Where(d => d.Status == EntityStatus.Active && d.IssueQuantity > 0)
                            .ToList();

                        foreach (var detail in eligibleDetails)
                        {
                            // 使用 AddStockAsync 增加庫存（回沖領料）
                            var addResult = await _inventoryStockService.AddStockAsync(
                                productId: detail.ProductId,
                                warehouseId: detail.WarehouseId,
                                quantity: detail.IssueQuantity,
                                transactionType: InventoryTransactionTypeEnum.MaterialReturn,
                                transactionNumber: $"{entity.Code}_DEL",  // 使用 _DEL 後綴作為批次邊界
                                unitCost: detail.UnitCost,
                                locationId: detail.WarehouseLocationId,
                                remarks: $"永久刪除領料單 - {entity.Code}");

                            if (!addResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回退失敗：{addResult.ErrorMessage}");
                            }
                        }
                    }

                    // 4. 刪除所有 _ADJ 交易記錄（編輯產生的調整記錄）
                    var adjTransactions = await context.InventoryTransactions
                        .Where(t => t.TransactionNumber.StartsWith(entity.Code + "_ADJ"))
                        .ToListAsync();

                    if (adjTransactions.Any())
                    {
                        context.InventoryTransactions.RemoveRange(adjTransactions);
                    }

                    // 5. 永久刪除主記錄（EF Core 會自動刪除相關的明細）
                    context.MaterialIssues.Remove(entity);

                    // 6. 保存變更
                    await context.SaveChangesAsync();

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = id
                });
                return ServiceResult.Failure($"刪除領料單時發生錯誤：{ex.Message}");
            }
        }

        public override async Task<List<MaterialIssue>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<MaterialIssue?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Product)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(mi => mi.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<MaterialIssue>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => (mi.Code != null && mi.Code.Contains(searchTerm)) ||
                                (mi.Employee != null && mi.Employee.Name != null && mi.Employee.Name.Contains(searchTerm)) ||
                                (mi.Department != null && mi.Department.Name != null && mi.Department.Name.Contains(searchTerm)) ||
                                (mi.Remarks != null && mi.Remarks.Contains(searchTerm)))
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(MaterialIssue entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證領貨單號 (Code)
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("領貨單號為必填欄位");
                }
                else
                {
                    // 檢查單號唯一性
                    var isDuplicate = await IsMaterialIssueCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("領貨單號已存在");
                    }
                }

                // 驗證領貨日期
                if (entity.IssueDate == default)
                {
                    errors.Add("領貨日期為必填欄位");
                }

                // 驗證明細
                if (entity.Id == 0 && (entity.MaterialIssueDetails == null || !entity.MaterialIssueDetails.Any()))
                {
                    errors.Add("至少需要一筆領貨明細");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 庫存差異處理

        /// <summary>
        /// 更新領料單的庫存（差異更新模式）
        /// 功能：比較編輯前後的明細差異，使用淨值計算方式確保庫存準確性
        /// 完全參照 PurchaseReceivingService 的模式，確保邏輯一致性
        /// </summary>
        /// <param name="id">領料單ID</param>
        /// <param name="updatedBy">更新人員ID（保留參數）</param>
        /// <returns>更新結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
        {
            try
            {
                if (_inventoryStockService == null)
                    return ServiceResult.Failure("庫存服務未初始化");

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 載入領料單及明細
                    var currentMaterialIssue = await context.MaterialIssues
                        .Include(mi => mi.MaterialIssueDetails)
                        .FirstOrDefaultAsync(mi => mi.Id == id);

                    if (currentMaterialIssue == null)
                    {
                        return ServiceResult.Failure("找不到指定的領料單");
                    }

                    // 2. 查詢所有相關的庫存交易記錄
                    var allTransactions = await context.InventoryTransactions
                        .Where(t => t.TransactionNumber == currentMaterialIssue.Code ||
                                    t.TransactionNumber.StartsWith(currentMaterialIssue.Code + "_"))
                        .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
                        .ToListAsync();

                    // 3. 找到最後一次刪除記錄（_DEL）作為批次邊界
                    var lastDeleteTransaction = allTransactions
                        .Where(t => t.TransactionNumber.EndsWith("_DEL"))
                        .OrderByDescending(t => t.TransactionDate)
                        .ThenByDescending(t => t.Id)
                        .FirstOrDefault();

                    // 4. 只計算最後一次刪除之後的記錄（不含 _DEL 本身）
                    var existingTransactions = lastDeleteTransaction != null
                        ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id && !t.TransactionNumber.EndsWith("_DEL")).ToList()
                        : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL")).ToList();

                    // 5. 建立已處理過庫存的明細字典（ProductId + WarehouseId + LocationId -> 已處理庫存淨值）
                    var processedInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal NetProcessedQuantity, decimal UnitCost)>();

                    foreach (var trans in existingTransactions)
                    {
                        var key = $"{trans.ProductId}_{trans.WarehouseId}_{trans.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!processedInventory.ContainsKey(key))
                        {
                            processedInventory[key] = (trans.ProductId, trans.WarehouseId, trans.WarehouseLocationId, 0m, trans.UnitCost.GetValueOrDefault());
                        }
                        // 累加所有交易的淨值（Quantity 已經包含正負號）
                        // MaterialIssue 是負數，MaterialReturn 是正數
                        var oldQty = processedInventory[key].NetProcessedQuantity;
                        var newQty = oldQty + trans.Quantity;
                        processedInventory[key] = (processedInventory[key].ProductId, processedInventory[key].WarehouseId,
                                                  processedInventory[key].LocationId, newQty,
                                                  trans.UnitCost.GetValueOrDefault());
                    }

                    // 6. 建立當前明細字典
                    var currentInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal CurrentQuantity, decimal UnitCost)>();

                    foreach (var detail in currentMaterialIssue.MaterialIssueDetails)
                    {
                        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!currentInventory.ContainsKey(key))
                        {
                            currentInventory[key] = (detail.ProductId, detail.WarehouseId, detail.WarehouseLocationId, 0, detail.UnitCost.GetValueOrDefault());
                        }
                        var oldQty = currentInventory[key].CurrentQuantity;
                        var newQty = oldQty + detail.IssueQuantity;
                        currentInventory[key] = (currentInventory[key].ProductId, currentInventory[key].WarehouseId,
                                               currentInventory[key].LocationId, newQty,
                                               detail.UnitCost.GetValueOrDefault());
                    }

                    // 7. 處理庫存差異 - 使用淨值計算方式
                    var allKeys = processedInventory.Keys.Union(currentInventory.Keys).ToList();

                    foreach (var key in allKeys)
                    {
                        var hasProcessed = processedInventory.ContainsKey(key);
                        var hasCurrent = currentInventory.ContainsKey(key);

                        // 計算目標庫存數量（當前明細中應該有的數量）
                        decimal targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0m;

                        // 計算已處理的庫存數量（之前所有交易的淨值的絕對值）
                        // 對於領料：淨值是負數（如 -20+10=-10），取絕對值得到實際領了 10 個
                        decimal processedQuantity = hasProcessed ? Math.Abs(processedInventory[key].NetProcessedQuantity) : 0m;

                        // 計算需要調整的數量
                        decimal adjustmentNeeded = targetQuantity - processedQuantity;

                        if (adjustmentNeeded != 0)
                        {
                            if (adjustmentNeeded > 0)
                            {
                                // 需要增加領料（減少庫存）
                                var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                                var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                                var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;

                                var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                    productId,
                                    warehouseId,
                                    adjustmentNeeded,
                                    InventoryTransactionTypeEnum.MaterialIssue,
                                    $"{currentMaterialIssue.Code}_ADJ",
                                    locationId,
                                    $"編輯領料單差異調整 - 增加領料 {adjustmentNeeded}");

                                if (!reduceResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調整失敗：{reduceResult.ErrorMessage}");
                                }
                            }
                            else
                            {
                                // 需要減少領料（增加庫存）
                                var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                                var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                                var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;
                                var unitCost = hasCurrent ? currentInventory[key].UnitCost : processedInventory[key].UnitCost;

                                var addResult = await _inventoryStockService.AddStockAsync(
                                    productId,
                                    warehouseId,
                                    Math.Abs(adjustmentNeeded),
                                    InventoryTransactionTypeEnum.MaterialReturn,
                                    $"{currentMaterialIssue.Code}_ADJ",
                                    unitCost,
                                    locationId,
                                    $"編輯領料單差異調整 - 減少領料 {Math.Abs(adjustmentNeeded)}");

                                if (!addResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調整失敗：{addResult.ErrorMessage}");
                                }
                            }
                        }
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInventoryByDifferenceAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateInventoryByDifferenceAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = id
                });
                return ServiceResult.Failure($"更新庫存差異時發生錯誤：{ex.Message}");
            }
        }

        #endregion

        #region 業務邏輯方法

        public async Task<bool> IsMaterialIssueCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.MaterialIssues.Where(mi => mi.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(mi => mi.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsMaterialIssueCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsMaterialIssueCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<string> GenerateIssueNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var today = DateTime.Today;
                var prefix = $"MI{today:yyyyMMdd}";

                // 查詢今天最後一筆單號
                var lastIssue = await context.MaterialIssues
                    .Where(mi => mi.Code != null && mi.Code.StartsWith(prefix))
                    .OrderByDescending(mi => mi.Code)
                    .FirstOrDefaultAsync();

                int nextSequence = 1;
                if (lastIssue != null && !string.IsNullOrEmpty(lastIssue.Code))
                {
                    // 取得流水號部分並加1
                    var sequencePart = lastIssue.Code.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out int currentSequence))
                    {
                        nextSequence = currentSequence + 1;
                    }
                }

                // 格式: MI + yyyyMMdd + 4位流水號
                return $"{prefix}{nextSequence:D4}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateIssueNumberAsync), GetType(), _logger, new
                {
                    Method = nameof(GenerateIssueNumberAsync),
                    ServiceType = GetType().Name
                });
                // 發生錯誤時返回預設單號
                return $"MI{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        public async Task<MaterialIssue?> GetWithDetailsAsync(int materialIssueId)
        {
            return await GetByIdAsync(materialIssueId);
        }

        public async Task<List<MaterialIssueDetail>> GetDetailsAsync(int materialIssueId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MaterialIssueId == materialIssueId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDetailsAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId
                });
                return new List<MaterialIssueDetail>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.IssueDate >= startDate && mi.IssueDate <= endDate)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.EmployeeId == employeeId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByEmployeeAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDepartmentAsync(int departmentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.DepartmentId == departmentId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDepartmentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDepartmentAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = departmentId
                });
                return new List<MaterialIssue>();
            }
        }

        #endregion
    }
}
