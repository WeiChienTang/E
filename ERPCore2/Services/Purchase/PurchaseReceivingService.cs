using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購進貨服務實作 - 管理採購進貨單的建立、修改、查詢及相關業務邏輯
    /// 提供進貨單號自動產生、庫存更新、供應商退貨管理等核心功能
    /// </summary>
    public class PurchaseReceivingService : GenericManagementService<PurchaseReceiving>, IPurchaseReceivingService
    {
        /// <summary>
        /// 庫存管理服務 - 用於進貨確認時更新庫存數量
        /// </summary>
        private readonly IInventoryStockService? _inventoryStockService;
        
        /// <summary>
        /// 採購進貨明細服務 - 用於管理進貨單的明細資料
        /// </summary>
        private readonly IPurchaseReceivingDetailService? _detailService;

        /// <summary>
        /// 採購訂單明細服務 - 用於更新已進貨數量
        /// </summary>
        private readonly IPurchaseOrderDetailService? _purchaseOrderDetailService;

    /// <summary>
    /// 採購退貨明細服務 - 用於檢查進貨明細是否有退貨記錄
    /// </summary>
    private readonly IPurchaseReturnDetailService? _purchaseReturnDetailService;

    /// <summary>
    /// 系統參數服務 - 用於檢查是否啟用採購單審核
    /// </summary>
    private readonly ISystemParameterService? _systemParameterService;

    /// <summary>
    /// 簡易建構子 - 適用於測試環境或最小依賴場景
    /// </summary>
    /// <param name="contextFactory">資料庫上下文工廠</param>
    public PurchaseReceivingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    {
    }        /// <summary>
        /// 標準建構子 - 包含日誌記錄功能，適用於一般生產環境
        /// </summary>
        /// <param name="contextFactory">資料庫上下文工廠</param>
        /// <param name="logger">日誌記錄器</param>
        public PurchaseReceivingService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceiving>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 完整建構子 - 注入所有相關服務，提供完整功能支援
        /// 包含庫存管理和明細管理服務，支援進貨確認和明細操作
        /// </summary>
        /// <param name="contextFactory">資料庫上下文工廠</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="inventoryStockService">庫存管理服務</param>
        /// <param name="detailService">進貨明細服務</param>
        /// <param name="purchaseOrderDetailService">採購訂單明細服務</param>
        /// <param name="purchaseReturnDetailService">採購退貨明細服務</param>
        /// <param name="systemParameterService">系統參數服務</param>
        public PurchaseReceivingService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PurchaseReceiving>> logger,
            IInventoryStockService inventoryStockService,
            IPurchaseReceivingDetailService detailService,
            IPurchaseOrderDetailService purchaseOrderDetailService,
            IPurchaseReturnDetailService purchaseReturnDetailService,
            ISystemParameterService systemParameterService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _detailService = detailService;
            _purchaseOrderDetailService = purchaseOrderDetailService;
            _purchaseReturnDetailService = purchaseReturnDetailService;
            _systemParameterService = systemParameterService;
        }

        #region 覆寫基本方法

        /// <summary>
        /// 取得所有採購進貨單資料
        /// 功能：載入所有未刪除的進貨單，包含完整的關聯資料
        /// 關聯資料：採購訂單、供應商、進貨明細、商品、倉庫等
        /// 排序：依進貨日期降序，再依進貨單號升序
        /// </summary>
        /// <returns>採購進貨單列表</returns>
        protected override IQueryable<PurchaseReceiving> BuildGetAllQuery(AppDbContext context)
        {
            return context.PurchaseReceivings
                .Include(pr => pr.Supplier)
                .Include(pr => pr.ApprovedByUser)
                .Include(pr => pr.PurchaseReceivingDetails)
                    .ThenInclude(prd => prd.Product)
                .Include(pr => pr.PurchaseReceivingDetails)
                    .ThenInclude(prd => prd.Warehouse)
                .OrderByDescending(pr => pr.ReceiptDate)
                .ThenBy(pr => pr.Code);
        }

        /// <summary>
        /// 根據ID取得特定採購進貨單
        /// 功能：載入指定ID的進貨單，包含所有相關的詳細資料
        /// 關聯資料：
        /// - 採購訂單及其供應商
        /// - 直接關聯的供應商
        /// - 進貨明細及其商品、單位
        /// - 採購訂單明細及相關資料
        /// - 倉庫和倉庫位置
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <returns>採購進貨單物件，找不到時回傳null</returns>
        public override async Task<PurchaseReceiving?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.ApprovedByUser)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod!.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod!.PurchaseOrder)
                                .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .AsQueryable()
                    .FirstOrDefaultAsync(pr => pr.Id == id);
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

        /// <summary>
        /// 搜尋採購進貨單
        /// 功能：根據關鍵字搜尋進貨單資料
        /// 搜尋範圍：
        /// - 進貨單號
        /// - 供應商公司名稱
        /// - 採購訂單號碼（如果有關聯採購訂單）
        /// 空白或null搜尋詞會回傳所有資料
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字</param>
        /// <returns>符合條件的採購進貨單列表</returns>
        public override async Task<List<PurchaseReceiving>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Where(pr => (pr.Code != null && pr.Code.Contains(searchTerm)) ||
                         (pr.Supplier != null && pr.Supplier.CompanyName!.Contains(searchTerm)))
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PurchaseReceiving>();
            }
        }

        /// <summary>
        /// 驗證採購進貨單資料
        /// 功能：執行完整的業務邏輯驗證
        /// 驗證項目：
        /// 1. 基本資料完整性（物件非null、進貨單號、供應商ID）
        /// 2. 進貨單號唯一性（新增時不可重複，編輯時排除自己）
        /// 3. 採購訂單存在性及核准狀態（僅限有指定採購訂單時）
        /// 4. 供應商存在性驗證
        /// </summary>
        /// <param name="entity">要驗證的採購進貨單實體</param>
        /// <returns>驗證結果，包含是否成功及錯誤訊息</returns>
        public override async Task<ServiceResult> ValidateAsync(PurchaseReceiving entity)
        {
            try
            {
                if (entity == null)
                    return ServiceResult.Failure("進貨單資料不可為空");

                // 已傳票化的進貨入庫單不允許修改（修正 Bug-49）
                if (entity.IsJournalized && entity.Id > 0)
                    return ServiceResult.Failure("進貨入庫單已傳票化，不可再修改");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("進貨單號為必填");

                if (!entity.IsDraft && !(entity.SupplierId > 0))
                    return ServiceResult.Failure("廠商為必填");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查進貨單號是否重複
                bool exists;
                if (entity.Id == 0)
                {
                    exists = await context.PurchaseReceivings
                        .AnyAsync(pr => pr.Code == entity.Code);
                }
                else
                {
                    exists = await context.PurchaseReceivings
                        .AnyAsync(pr => pr.Code == entity.Code && pr.Id != entity.Id);
                }

                if (exists)
                    return ServiceResult.Failure("進貨單號已存在");

                // 檢查供應商（草稿允許不填）
                if (!entity.IsDraft)
                {
                    var supplier = await context.Suppliers
                        .FirstOrDefaultAsync(s => s.Id == entity.SupplierId);

                    if (supplier == null)
                        return ServiceResult.Failure("指定的供應商不存在");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity?.Id,
                    EntityName = entity?.Code 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫刪除方法 - 刪除主檔時同步回退庫存
        /// 功能：刪除採購進貨單時，自動回退已進貨的庫存數量
        /// 處理流程：
        /// 1. 驗證進貨單存在性
        /// 2. 對每個明細進行庫存回退操作
        /// 3. 執行原本的軟刪除（主檔 + EF級聯刪除明細）
        /// 4. 使用資料庫交易確保資料一致性
        /// 5. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">要刪除的進貨單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 獲取進貨單及明細資料（在刪除前）
                    var purchaseReceiving = await GetByIdAsync(id);
                    if (purchaseReceiving == null)
                        return ServiceResult.Failure("找不到要刪除的進貨單");

                    // 2. 檢查是否有庫存服務可用
                    if (_inventoryStockService != null)
                    {
                        var eligibleDetails = purchaseReceiving.PurchaseReceivingDetails?.Where(d => d.ReceivedQuantity > 0).ToList() ?? new List<PurchaseReceivingDetail>();
                        
                        // 🔑 關鍵：檢查是否已經有 Delete 操作的交易記錄（避免重複扣減）
                        var existingDelDetails = await context.InventoryTransactionDetails
                            .Include(td => td.InventoryTransaction)
                            .Where(td => td.InventoryTransaction != null && 
                                        td.InventoryTransaction.TransactionNumber == purchaseReceiving.Code &&
                                        td.OperationType == InventoryOperationTypeEnum.Delete)
                            .Select(td => new { td.ProductId, td.SourceDetailId })
                            .ToListAsync();
                        
                        // 3. 對每個明細進行庫存回退
                        foreach (var detail in eligibleDetails)
                        {
                            // 檢查此明細是否已經被處理過（有對應的刪除記錄）
                            var alreadyProcessed = existingDelDetails.Any(d => 
                                d.ProductId == detail.ProductId && 
                                (d.SourceDetailId == detail.Id || d.SourceDetailId == null));
                            
                            if (alreadyProcessed)
                            {
                                continue;
                            }
                            
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Return,
                                purchaseReceiving.Code ?? string.Empty,  // 使用原始單號
                                detail.WarehouseLocationId,
                                $"刪除採購進貨單 - {purchaseReceiving.Code}",
                                sourceDocumentType: InventorySourceDocumentTypes.PurchaseReceiving,
                                sourceDocumentId: purchaseReceiving.Id,
                                operationType: InventoryOperationTypeEnum.Delete  // 標記為刪除操作
                            );
                            
                            if (!reduceResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回退失敗：{reduceResult.ErrorMessage}");
                            }
                        }
                    }

                    // 4. 執行原本的軟刪除（主檔 + EF級聯刪除明細）
                    
                    var entity = await context.PurchaseReceivings
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }

                    entity.UpdatedAt = DateTime.UtcNow;

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("刪除進貨單過程發生錯誤");
            }
        }

        /// <summary>
        /// 永久刪除進貨驗收單（含庫存回滾）
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
                    // 1. 先取得主記錄（含詳細資料，包含採購訂單明細關聯）
                    var entity = await context.PurchaseReceivings
                        .Include(pr => pr.PurchaseReceivingDetails)
                            .ThenInclude(prd => prd.PurchaseOrderDetail)
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
                    
                    // 3. 檢查是否有庫存服務可用並進行庫存回滾 - 僅在已核准時才回滾
                    // 未核准的進貨單從未更新庫存（ShouldUpdateInventory 在核准前為 false），不需回滾
                    if (_inventoryStockService != null && entity.IsApproved)
                    {
                        var eligibleDetails = entity.PurchaseReceivingDetails.Where(d => d.ReceivedQuantity > 0).ToList();
                        
                        // 🔑 關鍵：檢查是否已經有 Delete 操作的交易記錄（避免重複扣減）
                        var existingDelDetails = await context.InventoryTransactionDetails
                            .Include(td => td.InventoryTransaction)
                            .Where(td => td.InventoryTransaction != null && 
                                        td.InventoryTransaction.TransactionNumber == entity.Code &&
                                        td.OperationType == InventoryOperationTypeEnum.Delete)
                            .Select(td => new { td.ProductId, td.SourceDetailId })
                            .ToListAsync();
                        
                        foreach (var detail in eligibleDetails)
                        {
                            // 檢查此明細是否已經被處理過（有對應的刪除記錄）
                            var alreadyProcessed = existingDelDetails.Any(d => 
                                d.ProductId == detail.ProductId && 
                                (d.SourceDetailId == detail.Id || d.SourceDetailId == null));
                            
                            if (alreadyProcessed)
                            {
                                continue;
                            }
                            
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Return,
                                entity.Code ?? string.Empty,  // 使用原始單號
                                detail.WarehouseLocationId,
                                $"永久刪除採購進貨單 - {entity.Code}",
                                sourceDocumentType: InventorySourceDocumentTypes.PurchaseReceiving,
                                sourceDocumentId: entity.Id,
                                operationType: InventoryOperationTypeEnum.Delete  // 標記為刪除操作
                            );
                            
                            if (!reduceResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回退失敗：{reduceResult.ErrorMessage}");
                            }
                        }
                    }
                    
                    // 🔑 說明：由於現在所有異動都在同一個主檔下，不再需要刪除 _ADJ 記錄
                    // 異動明細透過 OperationType 區分，永久刪除時會一併刪除整個主檔
                    
                    // 4. 更新對應的採購訂單明細已進貨數量
                    if (_purchaseOrderDetailService != null)
                    {
                        // 收集需要更新的採購訂單明細
                        var purchaseOrderDetailUpdates = entity.PurchaseReceivingDetails
                            .Where(d => d.ReceivedQuantity > 0)
                            .GroupBy(d => d.PurchaseOrderDetailId)
                            .ToList();
                        
                        foreach (var group in purchaseOrderDetailUpdates)
                        {
                            var purchaseOrderDetailId = group.Key;
                            var totalReceivedQuantityToReduce = group.Sum(d => d.ReceivedQuantity);
                            // 修正 Bug-59：以實際入庫金額小計加總，而非用訂單單價推算
                            // 若進貨單價與訂單單價不同，原本 newReceivedQuantity * purchaseOrderDetail.UnitPrice 會產生錯誤金額
                            var totalReceivedAmountToReduce = group.Sum(d => d.SubtotalAmount);

                            // 獲取當前的採購訂單明細
                            var purchaseOrderDetail = group.First().PurchaseOrderDetail;
                            if (purchaseOrderDetail != null)
                            {
                                var newReceivedQuantity = Math.Max(0, purchaseOrderDetail.ReceivedQuantity - totalReceivedQuantityToReduce);
                                var newReceivedAmount = Math.Max(0, purchaseOrderDetail.ReceivedAmount - totalReceivedAmountToReduce);
                                
                                var updateResult = await _purchaseOrderDetailService.UpdateReceivedQuantityAsync(
                                    purchaseOrderDetailId!.Value, 
                                    newReceivedQuantity, 
                                    newReceivedAmount
                                );
                                
                                if (!updateResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"更新採購訂單明細已進貨數量失敗：{updateResult.ErrorMessage}");
                                }
                            }
                        }
                    }
                    
                    // 5. 永久刪除主記錄（EF Core 會自動刪除相關的明細）
                    context.PurchaseReceivings.Remove(entity);
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure($"永久刪除資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查採購進貨單是否可以被刪除
        /// 檢查邏輯：
        /// 1. 先執行基類的刪除檢查（外鍵關聯等）
        /// 2. 檢查所有明細項目是否有退貨記錄
        ///    - 資料來源：透過 IPurchaseReturnDetailService.GetReturnedQuantityByReceivingDetailAsync() 查詢
        ///    - 檢查資料表：PurchaseReturnDetail (採購退貨明細)
        ///    - 檢查欄位：PurchaseReceivingDetailId (關聯的進貨明細ID)
        ///    - 限制原因：已有退貨記錄的進貨明細不可刪除，以保持資料一致性
        /// 3. 檢查所有明細項目是否有沖款記錄
        ///    - 資料來源：直接讀取 PurchaseReceivingDetail 實體
        ///    - 檢查資料表：PurchaseReceivingDetail (採購進貨明細)
        ///    - 檢查欄位：TotalPaidAmount (累計付款金額)
        ///    - 限制原因：已沖款的進貨明細不可刪除，避免財務資料錯亂
        /// 
        /// 任一明細被鎖定則整個主檔無法刪除
        /// </summary>
        /// <param name="entity">要檢查的採購進貨單實體</param>
        /// <returns>檢查結果，包含是否可刪除及錯誤訊息</returns>
        protected override async Task<ServiceResult> CanDeleteAsync(PurchaseReceiving entity)
        {
            try
            {
                // 1. 先檢查基類的刪除條件（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                {
                    return baseResult;
                }

                // 2. 載入明細資料（如果尚未載入）
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var loadedEntity = await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .FirstOrDefaultAsync(pr => pr.Id == entity.Id);

                if (loadedEntity == null)
                {
                    return ServiceResult.Failure("找不到要檢查的進貨單");
                }

                // 如果沒有明細，可以刪除
                if (loadedEntity.PurchaseReceivingDetails == null || !loadedEntity.PurchaseReceivingDetails.Any())
                {
                    return ServiceResult.Success();
                }

                // 3. 檢查每個明細項目是否有退貨或沖款記錄
                foreach (var detail in loadedEntity.PurchaseReceivingDetails)
                {
                    // 3.1 檢查退貨記錄
                    if (_purchaseReturnDetailService != null)
                    {
                        var returnedQty = await _purchaseReturnDetailService
                            .GetReturnedQuantityByReceivingDetailAsync(detail.Id);

                        if (returnedQty > 0)
                        {
                            var productName = detail.Product?.Name ?? "未知商品";
                            return ServiceResult.Failure(
                                $"無法刪除此進貨單，因為商品「{productName}」已有退貨記錄（已退貨 {returnedQty} 個）"
                            );
                        }
                    }

                    // 3.2 檢查沖款記錄
                    if (detail.TotalPaidAmount > 0)
                    {
                        var productName = detail.Product?.Name ?? "未知商品";
                        return ServiceResult.Failure(
                            $"無法刪除此進貨單，因為商品「{productName}」已有沖款記錄（已沖款 {detail.TotalPaidAmount:N0} 元）"
                        );
                    }
                }

                // 4. 所有檢查通過，允許刪除
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(CanDeleteAsync), GetType(), _logger,
                    new { EntityId = entity.Id, ReceiptNumber = entity.Code }
                );
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 特定業務方法

        /// <summary>
        /// 自動產生進貨單號
        /// 功能：根據日期和序號自動產生唯一的進貨單號
        /// 格式：R + yyyyMMdd + 3位數序號 (例：R202501140001)
        /// 邏輯：
        /// 1. 取得當日最大序號
        /// 2. 序號自動加1
        /// 3. 發生錯誤時回傳預設格式
        /// </summary>
        /// <returns>新的進貨單號</returns>
        public async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var today = DateTime.Today;
                var prefix = $"R{today:yyyyMMdd}";
                
                var lastNumber = await context.PurchaseReceivings
                    .Where(pr => pr.Code != null && pr.Code.StartsWith(prefix))
                    .OrderByDescending(pr => pr.Code)
                    .Select(pr => pr.Code)
                    .FirstOrDefaultAsync();

                int sequence = 1;
                if (!string.IsNullOrEmpty(lastNumber) && lastNumber.Length > prefix.Length)
                {
                    var sequencePart = lastNumber.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out int lastSequence))
                        sequence = lastSequence + 1;
                }

                return $"{prefix}{sequence:D3}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateReceiptNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateReceiptNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"R{DateTime.Today:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 自動產生批號
        /// 功能：根據日期和序號自動產生唯一的批號
        /// 格式：yyyyMMdd + 3位數序號 (例：20250917001)
        /// 邏輯：
        /// 1. 取得當日最大批號序號
        /// 2. 序號自動加1
        /// 3. 發生錯誤時回傳預設格式
        /// </summary>
        /// <returns>新的批號</returns>
        public async Task<string> GenerateBatchNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var today = DateTime.Today;
                var prefix = today.ToString("yyyyMMdd");
                
                var lastBatchNumber = await context.PurchaseReceivings
                    .Where(pr => pr.BatchNumber != null && pr.BatchNumber.StartsWith(prefix))
                    .OrderByDescending(pr => pr.BatchNumber)
                    .Select(pr => pr.BatchNumber)
                    .FirstOrDefaultAsync();

                int sequence = 1;
                if (!string.IsNullOrEmpty(lastBatchNumber) && lastBatchNumber.Length > prefix.Length)
                {
                    var sequencePart = lastBatchNumber.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out int lastSequence))
                        sequence = lastSequence + 1;
                }

                return $"{prefix}{sequence:D3}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateBatchNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateBatchNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"{DateTime.Today:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 檢查進貨編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">進貨編號</param>
        /// <param name="excludeId">要排除的ID（編輯模式時使用）</param>
        /// <returns>true表示已存在，false表示不存在</returns>
        public async Task<bool> IsPurchaseReceivingCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.PurchaseReceivings.Where(pr => pr.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(pr => pr.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReceivingCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseReceivingCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }
        
        /// <summary>
        /// 根據日期範圍查詢採購進貨單
        /// 功能：取得指定日期區間內的所有進貨單資料
        /// 包含完整的關聯資料載入，依進貨日期降序排列
        /// 適用於報表查詢、期間統計等功能
        /// </summary>
        /// <param name="startDate">開始日期（包含）</param>
        /// <param name="endDate">結束日期（包含）</param>
        /// <returns>指定日期範圍內的採購進貨單列表</returns>
        public async Task<List<PurchaseReceiving>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => pr.ReceiptDate >= startDate && pr.ReceiptDate <= endDate)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
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
                return new List<PurchaseReceiving>();
            }
        }

        /// <summary>
        /// 根據採購訂單查詢相關進貨單
        /// 功能：取得指定採購訂單的所有進貨記錄
        /// 用途：
        /// - 查看採購訂單的進貨履歷
        /// - 追蹤採購訂單的執行狀況
        /// - 進貨數量統計分析
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <returns>與指定採購訂單相關的進貨單列表</returns>
        public async Task<List<PurchaseReceiving>> GetByPurchaseOrderAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var receivingIds = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseOrderDetail != null && d.PurchaseOrderDetail.PurchaseOrderId == purchaseOrderId)
                    .Select(d => d.PurchaseReceivingId)
                    .Distinct()
                    .ToListAsync();

                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => receivingIds.Contains(pr.Id))
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderAsync), GetType(), _logger, new {
                    Method = nameof(GetByPurchaseOrderAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId
                });
                return new List<PurchaseReceiving>();
            }
        }

        public async Task<List<PurchaseReceiving>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Where(pr => pr.SupplierId == supplierId)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new {
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return new List<PurchaseReceiving>();
            }
        }

        /// <summary>
        /// 確認採購進貨單並更新庫存
        /// 功能：執行進貨確認流程，將進貨數量加入庫存系統
        /// 處理流程：
        /// 1. 驗證進貨單存在性
        /// 2. 逐項處理進貨明細，調用庫存服務增加庫存
        /// 3. 建立庫存異動記錄
        /// 4. 使用資料庫交易確保資料一致性
        /// 5. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">進貨單ID</param>
        /// <param name="confirmedBy">確認人員ID（保留參數，未來可能使用）</param>
        /// <returns>確認結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> ConfirmReceiptAsync(int id, int confirmedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var purchaseReceiving = await context.PurchaseReceivings
                        .Include(pr => pr.PurchaseReceivingDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == id);
                    
                    if (purchaseReceiving == null)
                        return ServiceResult.Failure("找不到指定的進貨單");
                    
                    // 更新庫存，傳遞批號資訊
                    foreach (var detail in purchaseReceiving.PurchaseReceivingDetails.AsQueryable())
                    {
                        if (_inventoryStockService != null)
                        {
                            var addStockResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Purchase,
                                purchaseReceiving.Code ?? string.Empty,
                                detail.UnitPrice,
                                detail.WarehouseLocationId,
                                $"採購進貨確認 - {purchaseReceiving.Code ?? string.Empty}",
                                detail.BatchNumber,           // 傳遞批號
                                purchaseReceiving.ReceiptDate,  // 批次日期
                                null, // expiryDate
                                sourceDocumentType: InventorySourceDocumentTypes.PurchaseReceiving,
                                sourceDocumentId: purchaseReceiving.Id
                            );
                            
                            if (!addStockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存更新失敗：{addStockResult.ErrorMessage}");
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmReceiptAsync), GetType(), _logger, new { 
                    Method = nameof(ConfirmReceiptAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ConfirmedBy = confirmedBy 
                });
                return ServiceResult.Failure("確認進貨單過程發生錯誤");
            }
        }

        /// <summary>
        /// 取消採購進貨單
        /// 功能：將進貨單標記為已刪除（軟刪除）
        /// 注意：此方法只進行軟刪除，不處理庫存回退
        /// 如需完整的取消流程，建議另外實作包含庫存回退的方法
        /// </summary>
        /// <param name="id">要取消的進貨單ID</param>
        /// <returns>取消結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> CancelReceiptAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var purchaseReceiving = await context.PurchaseReceivings
                    .FirstOrDefaultAsync(pr => pr.Id == id);
                
                if (purchaseReceiving == null)
                    return ServiceResult.Failure("找不到指定的進貨單");

                // 軟刪除：將狀態改為 Inactive（修正 Bug-55：原本僅更新 UpdatedAt，未實際變更狀態）
                purchaseReceiving.Status = EntityStatus.Inactive;
                purchaseReceiving.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelReceiptAsync), GetType(), _logger, new {
                    Method = nameof(CancelReceiptAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure("取消進貨單過程發生錯誤");
            }
        }

        /// <summary>
        /// 更新採購進貨單的庫存（差異更新模式）
        /// 功能：比較編輯前後的明細差異，使用淨值計算方式確保庫存準確性
        /// 處理邏輯：
        /// 1. 查詢該單號下所有庫存交易記錄，透過 OperationType 區分操作類型
        /// 2. 計算已處理過的庫存淨值（所有交易記錄 Quantity 的總和）
        /// 3. 計算當前明細應有的庫存數量
        /// 4. 比較目標數量與已處理數量，計算需要調整的數量
        /// 5. 根據調整數量進行庫存增減操作
        /// 6. 使用資料庫交易確保資料一致性
        /// 
        /// 修復問題：
        /// - 重複累加：每次編輯基於所有交易記錄的淨值進行計算
        /// - 商品替換：舊商品自動減庫存，新商品自動加庫存
        /// - 數量變更：精確計算差異，避免錯誤累加
        /// </summary>
        /// <param name="id">進貨單ID</param>
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
                    // 取得當前的進貨單及明細
                    var currentReceiving = await context.PurchaseReceivings
                        .Include(pr => pr.PurchaseReceivingDetails.AsQueryable())
                        .FirstOrDefaultAsync(pr => pr.Id == id);
                    
                    if (currentReceiving == null)
                        return ServiceResult.Failure("找不到指定的進貨單");

                    // 🔑 簡化設計：查詢該單據的所有異動明細，透過 TransactionNumber + SourceDocumentId 精確匹配
                    var allTransactionDetails = await context.InventoryTransactionDetails
                        .Include(d => d.InventoryTransaction)
                        .Include(d => d.InventoryStockDetail)
                        .Where(d => d.InventoryTransaction.TransactionNumber == currentReceiving.Code &&
                                    d.InventoryTransaction.SourceDocumentId == currentReceiving.Id &&
                                    d.InventoryTransaction.SourceDocumentType == InventorySourceDocumentTypes.PurchaseReceiving)
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
                    var processedInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal NetProcessedQuantity, decimal? UnitPrice)>();
                    
                    foreach (var detail in existingDetails)
                    {
                        var detailWarehouseId = detail.InventoryStockDetail?.WarehouseId ?? detail.InventoryTransaction.WarehouseId;
                        var key = $"{detail.ProductId}_{detailWarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!processedInventory.ContainsKey(key))
                        {
                            processedInventory[key] = (detail.ProductId, detailWarehouseId, detail.WarehouseLocationId, 0m, detail.UnitCost);
                        }
                        // 累加所有交易的淨值（Quantity已經包含正負號）
                        var oldQty = processedInventory[key].NetProcessedQuantity;
                        var newQty = oldQty + detail.Quantity;
                        processedInventory[key] = (processedInventory[key].ProductId, processedInventory[key].WarehouseId, 
                                                  processedInventory[key].LocationId, newQty, 
                                                  detail.UnitCost);
                    }
                    
                    // 建立當前明細字典
                    var currentInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal CurrentQuantity, decimal UnitPrice)>();
                    
                    foreach (var detail in currentReceiving.PurchaseReceivingDetails)
                    {
                        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!currentInventory.ContainsKey(key))
                        {
                            currentInventory[key] = (detail.ProductId, detail.WarehouseId, detail.WarehouseLocationId, 0, detail.UnitPrice);
                        }
                        var oldQty = currentInventory[key].CurrentQuantity;
                        var newQty = oldQty + detail.ReceivedQuantity;
                        currentInventory[key] = (currentInventory[key].ProductId, currentInventory[key].WarehouseId, 
                                               currentInventory[key].LocationId, newQty, 
                                               detail.UnitPrice);
                    }

                    // 處理庫存差異 - 使用淨值計算方式
                    var allKeys = processedInventory.Keys.Union(currentInventory.Keys).ToList();
                    
                    foreach (var key in allKeys)
                    {
                        var hasProcessed = processedInventory.ContainsKey(key);
                        var hasCurrent = currentInventory.ContainsKey(key);
                        
                        // 計算目標庫存數量（當前明細中應該有的數量）
                        decimal targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0m;
                        
                        // 計算已處理的庫存數量（之前所有交易的淨值）
                        decimal processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0m;
                        
                        // 計算需要調整的數量
                        decimal adjustmentNeeded = targetQuantity - processedQuantity;
                        
                        // 檢查價格是否變化（用於更新平均成本）
                        decimal? currentPrice = hasCurrent ? currentInventory[key].UnitPrice : (decimal?)null;
                        decimal? processedPrice = hasProcessed ? processedInventory[key].UnitPrice : (decimal?)null;
                        bool priceChanged = currentPrice.HasValue && processedPrice.HasValue && currentPrice.Value != processedPrice.Value;
                        
                        // 只有數量有變化，或者價格有變化時才需要處理
                        if (adjustmentNeeded != 0 || (priceChanged && targetQuantity > 0))
                        {
                            var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                            var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                            var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;
                            
                            if (adjustmentNeeded > 0)
                            {
                                // 需要增加庫存 - 使用原始單號 + Adjust 操作類型
                                var unitPrice = hasCurrent ? currentInventory[key].UnitPrice : processedInventory[key].UnitPrice;
                                
                                var addResult = await _inventoryStockService.AddStockAsync(
                                    productId,
                                    warehouseId,
                                    adjustmentNeeded,
                                    InventoryTransactionTypeEnum.Adjustment,
                                    currentReceiving.Code ?? string.Empty,  // 使用原始單號
                                    unitPrice,
                                    locationId,
                                    $"採購進貨編輯調增 - {currentReceiving.Code}",
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.PurchaseReceiving,
                                    sourceDocumentId: currentReceiving.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!addResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調增失敗：{addResult.ErrorMessage}");
                                }
                            }
                            else if (adjustmentNeeded < 0)
                            {
                                // 需要減少庫存 - 使用原始單號 + Adjust 操作類型
                                var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                    productId,
                                    warehouseId,
                                    Math.Abs(adjustmentNeeded),
                                    InventoryTransactionTypeEnum.Adjustment,
                                    currentReceiving.Code ?? string.Empty,  // 使用原始單號
                                    locationId,
                                    $"採購進貨編輯調減 - {currentReceiving.Code}",
                                    sourceDocumentType: InventorySourceDocumentTypes.PurchaseReceiving,
                                    sourceDocumentId: currentReceiving.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!reduceResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調減失敗：{reduceResult.ErrorMessage}");
                                }
                            }
                            else if (priceChanged && targetQuantity > 0)
                            {
                                // � 簡化設計：數量沒變但價格有變，直接調整成本，不產生異動記錄
                                var oldUnitPrice = processedInventory[key].UnitPrice ?? 0m;
                                var newUnitPrice = currentInventory[key].UnitPrice;
                                
                                var costAdjustResult = await _inventoryStockService.AdjustUnitCostAsync(
                                    productId,
                                    warehouseId,
                                    targetQuantity,  // 用於成本重算的數量
                                    oldUnitPrice,
                                    newUnitPrice,
                                    locationId
                                );
                                
                                if (!costAdjustResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"成本調整失敗：{costAdjustResult.ErrorMessage}");
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

        /// <summary>
        /// 覆寫新增方法 - 自動生成批號
        /// 功能：在新增採購進貨單時自動生成批號
        /// 邏輯：如果 BatchNumber 為空，則自動生成唯一批號
        /// </summary>
        /// <param name="entity">要新增的採購進貨單</param>
        /// <returns>新增結果，包含完整的實體資料</returns>
        public override async Task<ServiceResult<PurchaseReceiving>> CreateAsync(PurchaseReceiving entity)
        {
            try
            {
                // 新增時自動生成批號（如果未提供）
                if (string.IsNullOrWhiteSpace(entity.BatchNumber))
                {
                    entity.BatchNumber = await GenerateBatchNumberAsync();
                }
                
                // 調用基類的 CreateAsync 方法
                return await base.CreateAsync(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new { 
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity?.Id,
                    EntityName = entity?.Code,
                    BatchNumber = entity?.BatchNumber
                });
                return ServiceResult<PurchaseReceiving>.Failure("新增進貨單過程發生錯誤");
            }
        }

        /// <summary>
        /// 取得指定商品的最後進貨位置資訊
        /// 功能：查詢商品最近一次的進貨倉庫和位置
        /// 用途：
        /// - 為新進貨提供預設倉庫位置建議
        /// - 保持商品進貨位置的一致性
        /// - 優化倉庫管理效率
        /// 查詢邏輯：依進貨日期和建立時間降序，取得最新記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>倉庫ID和倉庫位置ID的元組，無歷史記錄時回傳(null, null)</returns>
        public async Task<(int? WarehouseId, int? WarehouseLocationId)> GetLastReceivingLocationAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var lastDetail = await context.PurchaseReceivingDetails
                    .Include(prd => prd.PurchaseReceiving)
                    .Where(prd => prd.ProductId == productId)
                    .OrderByDescending(prd => prd.PurchaseReceiving.ReceiptDate)
                    .ThenByDescending(prd => prd.CreatedAt)
                    .FirstOrDefaultAsync();
                
                if (lastDetail != null)
                    return (lastDetail.WarehouseId, lastDetail.WarehouseLocationId);
                
                return (null, null);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastReceivingLocationAsync), GetType(), _logger, new { 
                    Method = nameof(GetLastReceivingLocationAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return (null, null);
            }
        }

        /// <summary>
        /// 取得指定供應商的可退貨明細清單
        /// 功能：查詢指定供應商所有可進行退貨的進貨明細
        /// 篩選條件：
        /// 1. 指定供應商的進貨記錄
        /// 2. 未被刪除的記錄
        /// 3. 已有進貨數量的明細
        /// 4. 進貨數量大於已退貨數量（部分或完全未退貨）
        /// 用途：供應商退貨作業的資料來源
        /// 排序：依進貨日期、進貨單號、商品編號排序
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>可退貨的進貨明細清單</returns>
        public async Task<List<PurchaseReceivingDetail>> GetReturnableDetailsBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.PurchaseReceivingDetails
                    .Include(prd => prd.PurchaseReceiving)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Warehouse)
                    .Include(prd => prd.WarehouseLocation)
                    .Where(prd => 
                        prd.PurchaseReceiving.SupplierId == supplierId &&
                        prd.ReceivedQuantity > 0 &&
                        // 檢查是否已全部退貨 - 計算已退貨數量
                        prd.ReceivedQuantity > context.PurchaseReturnDetails
                            .Where(prt => prt.PurchaseReceivingDetailId == prd.Id)
                            .Sum(prt => prt.ReturnQuantity)
                    )
                    .OrderBy(prd => prd.PurchaseReceiving.ReceiptDate)
                    .ThenBy(prd => prd.PurchaseReceiving.Code)
                    .ThenBy(prd => prd.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnableDetailsBySupplierAsync), GetType(), _logger, new { 
                    Method = nameof(GetReturnableDetailsBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據批次列印條件查詢進貨單
        /// </summary>
        public async Task<List<PurchaseReceiving>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 建立基礎查詢（包含必要的關聯資料）
                IQueryable<PurchaseReceiving> query = context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .AsQueryable();

                // 日期範圍篩選
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(pr => pr.ReceiptDate >= criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate.HasValue)
                {
                    // 包含整天（到當天 23:59:59）
                    var endDate = criteria.EndDate.Value.Date.AddDays(1);
                    query = query.Where(pr => pr.ReceiptDate < endDate);
                }

                // 廠商篩選（RelatedEntityIds 對應廠商ID列表）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(pr => pr.SupplierId.HasValue && criteria.RelatedEntityIds.Contains(pr.SupplierId.Value));
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(pr => pr.Code != null && pr.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 排序：先按廠商分組，同廠商內再按日期和單據編號排序
                // 這樣列印時同一廠商的進貨單會集中在一起
                query = criteria.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(pr => pr.Supplier!.CompanyName)
                           .ThenBy(pr => pr.ReceiptDate)
                           .ThenBy(pr => pr.Code)
                    : query.OrderBy(pr => pr.Supplier!.CompanyName)
                           .ThenByDescending(pr => pr.ReceiptDate)
                           .ThenBy(pr => pr.Code);

                // 限制最大筆數
                if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
                {
                    query = query.Take(criteria.MaxResults.Value);
                }

                // 執行查詢
                var results = await query.ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByBatchCriteriaAsync),
                    ServiceType = GetType().Name,
                    Criteria = new
                    {
                        criteria.StartDate,
                        criteria.EndDate,
                        criteria.RelatedEntityIds,
                        criteria.DocumentNumberKeyword
                    }
                });
                return new List<PurchaseReceiving>();
            }
        }

        public async Task<List<PurchaseReceiving>> GetByPurchaseReturnIdAsync(int purchaseReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var receivingDetailIds = await context.PurchaseReturnDetails
                    .Where(d => d.PurchaseReturnId == purchaseReturnId &&
                                d.PurchaseReceivingDetailId.HasValue)
                    .Select(d => d.PurchaseReceivingDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                var receivingIds = await context.PurchaseReceivingDetails
                    .Where(prd => receivingDetailIds.Contains(prd.Id))
                    .Select(prd => prd.PurchaseReceivingId)
                    .Distinct()
                    .ToListAsync();

                return await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Where(pr => receivingIds.Contains(pr.Id))
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReturnIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPurchaseReturnIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId
                });
                return new List<PurchaseReceiving>();
            }
        }

        #endregion

        #region 審核作業

        public async Task<ServiceResult> ApproveAsync(int id, int? approvedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var entity = await context.PurchaseReceivings.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return ServiceResult.Failure("找不到進貨單");
                if (entity.IsApproved) return ServiceResult.Failure("進貨單已核准，無需重複核准");

                entity.IsApproved = true;
                entity.ApprovedBy = approvedBy;
                entity.ApprovedAt = DateTime.UtcNow;
                entity.RejectReason = null;
                entity.UpdatedAt = DateTime.UtcNow;

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

        public async Task<ServiceResult> RejectAsync(int id, int rejectedBy, string reason)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PurchaseReceivings.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return ServiceResult.Failure("找不到進貨單");

            entity.IsApproved = false;
            entity.ApprovedBy = rejectedBy;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.RejectReason = reason;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        #endregion

        #region 伺服器端分頁

        public async Task<(List<PurchaseReceiving> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<PurchaseReceiving>, IQueryable<PurchaseReceiving>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<PurchaseReceiving> query = context.PurchaseReceivings
                    .Include(pr => pr.Supplier);

                if (filterFunc != null) query = filterFunc(query);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenByDescending(pr => pr.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger, new {
                    Method = nameof(GetPagedWithFiltersAsync),
                    ServiceType = GetType().Name,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
                return (new List<PurchaseReceiving>(), 0);
            }
        }

        #endregion
    }
}
