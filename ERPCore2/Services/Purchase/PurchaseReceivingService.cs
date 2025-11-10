using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        public override async Task<List<PurchaseReceiving>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .AsQueryable()
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReceiving>();
            }
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
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod.PurchaseOrder)
                                .ThenInclude(po => po.Supplier)
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
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Where(pr => (pr.Code != null && pr.Code.Contains(searchTerm)) ||
                         (pr.Supplier != null && pr.Supplier.CompanyName.Contains(searchTerm)) ||
                         (pr.PurchaseOrder != null && pr.PurchaseOrder.Code != null && pr.PurchaseOrder.Code.Contains(searchTerm)))
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

                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("進貨單號為必填");

                if (entity.SupplierId <= 0)
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

                // 檢查採購訂單（僅當有指定採購單時）
                if (entity.PurchaseOrderId.HasValue)
                {
                    var purchaseOrder = await context.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.Id == entity.PurchaseOrderId);
                    
                    if (purchaseOrder == null)
                        return ServiceResult.Failure("指定的採購訂單不存在");
                    
                    // 檢查是否啟用採購單審核
                    var isApprovalEnabled = false;
                    if (_systemParameterService != null)
                    {
                        isApprovalEnabled = await _systemParameterService.IsPurchaseOrderApprovalEnabledAsync();
                    }
                    
                    // 只有在啟用審核時才檢查核准狀態
                    if (isApprovalEnabled && !purchaseOrder.IsApproved)
                        return ServiceResult.Failure("只有已核准的採購訂單才能進行進貨作業");
                }

                // 檢查供應商
                var supplier = await context.Suppliers
                    .FirstOrDefaultAsync(s => s.Id == entity.SupplierId);
                
                if (supplier == null)
                    return ServiceResult.Failure("指定的供應商不存在");

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
                        
                        // 3. 對每個明細進行庫存回退
                        foreach (var detail in eligibleDetails)
                        {
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Return,
                                $"{purchaseReceiving.Code}_DEL",
                                detail.WarehouseLocationId,
                                $"刪除採購進貨單 - {purchaseReceiving.Code}"
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
                    
                    // 3. 檢查是否有庫存服務可用並進行庫存回滾
                    if (_inventoryStockService != null)
                    {
                        var eligibleDetails = entity.PurchaseReceivingDetails.Where(d => d.ReceivedQuantity > 0).ToList();
                        
                        foreach (var detail in eligibleDetails)
                        {
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Return,
                                $"{entity.Code}_DEL",
                                detail.WarehouseLocationId,
                                $"永久刪除採購進貨單 - {entity.Code}"
                            );
                            
                            if (!reduceResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回退失敗：{reduceResult.ErrorMessage}");
                            }
                        }
                    }
                    
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
                            
                            // 獲取當前的採購訂單明細
                            var purchaseOrderDetail = group.First().PurchaseOrderDetail;
                            if (purchaseOrderDetail != null)
                            {
                                var newReceivedQuantity = Math.Max(0, purchaseOrderDetail.ReceivedQuantity - totalReceivedQuantityToReduce);
                                var newReceivedAmount = newReceivedQuantity * purchaseOrderDetail.UnitPrice;
                                
                                var updateResult = await _purchaseOrderDetailService.UpdateReceivedQuantityAsync(
                                    purchaseOrderDetailId, 
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
        /// 檢查進貨代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">進貨代碼</param>
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
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
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
                
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => pr.PurchaseOrderId == purchaseOrderId)
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
                                purchaseReceiving.ReceiptDate  // 批次日期
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
        /// 1. 查詢所有相關的庫存交易記錄（包含原始、_ADJ、_REVERT 等後綴）
        /// 2. 計算已處理過的庫存淨值（所有交易記錄 Quantity 的總和）
        /// 3. 計算當前明細應有的庫存數量
        /// 4. 比較目標數量與已處理數量，計算需要調整的數量
        /// 5. 根據調整數量進行庫存增減操作
        /// 6. 使用資料庫交易確保資料一致性
        /// 
        /// 修復問題：
        /// - 重複累加：每次編輯基於所有交易記錄的淨值進行計算
        /// - 產品替換：舊產品自動減庫存，新產品自動加庫存
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

                    // 查詢所有與此進貨單相關的庫存交易記錄（包含原始、調整、回退等所有類型）
                    var existingTransactions = await context.InventoryTransactions
                        .Where(t => (t.TransactionNumber == currentReceiving.Code ||
                                   t.TransactionNumber.StartsWith(currentReceiving.Code + "_")))
                        .ToListAsync();

                    // 建立已處理過庫存的明細字典（ProductId + WarehouseId + LocationId -> 已處理庫存淨值）
                    var processedInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, int NetProcessedQuantity, decimal? UnitPrice)>();
                    
                    foreach (var trans in existingTransactions)
                    {
                        var key = $"{trans.ProductId}_{trans.WarehouseId}_{trans.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!processedInventory.ContainsKey(key))
                        {
                            processedInventory[key] = (trans.ProductId, trans.WarehouseId, trans.WarehouseLocationId, 0, trans.UnitCost);
                        }
                        // 累加所有交易的淨值（Quantity已經包含正負號）
                        processedInventory[key] = (processedInventory[key].ProductId, processedInventory[key].WarehouseId, 
                                                  processedInventory[key].LocationId, processedInventory[key].NetProcessedQuantity + trans.Quantity, 
                                                  trans.UnitCost);
                    }

                    // 建立當前明細字典
                    var currentInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, int CurrentQuantity, decimal UnitPrice)>();
                    
                    foreach (var detail in currentReceiving.PurchaseReceivingDetails)
                    {
                        var key = $"{detail.ProductId}_{detail.WarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!currentInventory.ContainsKey(key))
                        {
                            currentInventory[key] = (detail.ProductId, detail.WarehouseId, detail.WarehouseLocationId, 0, detail.UnitPrice);
                        }
                        currentInventory[key] = (currentInventory[key].ProductId, currentInventory[key].WarehouseId, 
                                               currentInventory[key].LocationId, currentInventory[key].CurrentQuantity + detail.ReceivedQuantity, 
                                               detail.UnitPrice);
                    }

                    // 處理庫存差異 - 使用淨值計算方式
                    var allKeys = processedInventory.Keys.Union(currentInventory.Keys).ToList();
                    
                    foreach (var key in allKeys)
                    {
                        var hasProcessed = processedInventory.ContainsKey(key);
                        var hasCurrent = currentInventory.ContainsKey(key);
                        
                        // 計算目標庫存數量（當前明細中應該有的數量）
                        int targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0;
                        
                        // 計算已處理的庫存數量（之前所有交易的淨值）
                        int processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0;
                        
                        // 計算需要調整的數量
                        int adjustmentNeeded = targetQuantity - processedQuantity;
                        
                        if (adjustmentNeeded != 0)
                        {
                            if (adjustmentNeeded > 0)
                            {
                                // 需要增加庫存
                                var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                                var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                                var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;
                                var unitPrice = hasCurrent ? currentInventory[key].UnitPrice : processedInventory[key].UnitPrice;
                                
                                var addResult = await _inventoryStockService.AddStockAsync(
                                    productId,
                                    warehouseId,
                                    adjustmentNeeded,
                                    InventoryTransactionTypeEnum.Purchase,
                                    currentReceiving.Code + "_ADJ",
                                    unitPrice,
                                    locationId,
                                    $"採購進貨編輯調增 - {currentReceiving.Code}"
                                );
                                
                                if (!addResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調增失敗：{addResult.ErrorMessage}");
                                }
                            }
                            else
                            {
                                // 需要減少庫存
                                var productId = hasProcessed ? processedInventory[key].ProductId : currentInventory[key].ProductId;
                                var warehouseId = hasProcessed ? processedInventory[key].WarehouseId : currentInventory[key].WarehouseId;
                                var locationId = hasProcessed ? processedInventory[key].LocationId : currentInventory[key].LocationId;
                                
                                var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                    productId,
                                    warehouseId,
                                    Math.Abs(adjustmentNeeded),
                                    InventoryTransactionTypeEnum.Return,
                                    currentReceiving.Code + "_ADJ",
                                    locationId,
                                    $"採購進貨編輯調減 - {currentReceiving.Code}"
                                );
                                
                                if (!reduceResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存調減失敗：{reduceResult.ErrorMessage}");
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
        /// 排序：依進貨日期、進貨單號、商品代碼排序
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
                    query = query.Where(pr => criteria.RelatedEntityIds.Contains(pr.SupplierId));
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(pr => pr.Code != null && pr.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 排序：先按廠商分組，同廠商內再按日期和單據編號排序
                // 這樣列印時同一廠商的進貨單會集中在一起
                query = criteria.SortDirection == Models.SortDirection.Ascending
                    ? query.OrderBy(pr => pr.Supplier.CompanyName)
                           .ThenBy(pr => pr.ReceiptDate)
                           .ThenBy(pr => pr.Code)
                    : query.OrderBy(pr => pr.Supplier.CompanyName)
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

        #endregion
    }
}
