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
    /// 採購入庫明細服務實作
    /// </summary>
    public class PurchaseReceivingDetailService : GenericManagementService<PurchaseReceivingDetail>, IPurchaseReceivingDetailService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public PurchaseReceivingDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
            _inventoryStockService = null;
        }

        public PurchaseReceivingDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceivingDetail>> logger) : base(contextFactory, logger)
        {
            _inventoryStockService = null;
        }

        public PurchaseReceivingDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceivingDetail>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        #region 覆寫基底類別方法

        /// <summary>
        /// 取得所有採購入庫明細（包含關聯資料）
        /// </summary>
        public override async Task<List<PurchaseReceivingDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .AsQueryable()
                    .OrderByDescending(d => d.CreatedAt)
                    .ThenBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據ID取得採購入庫明細（包含關聯資料）
        /// </summary>
        public override async Task<PurchaseReceivingDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(d => d.Id == id);
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
        /// 實作特定搜尋邏輯
        /// </summary>
        public override async Task<List<PurchaseReceivingDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => (
                        (d.PurchaseReceiving.Code != null && d.PurchaseReceiving.Code.Contains(searchTerm)) ||
                        d.Product.Name.Contains(searchTerm) ||
                        (d.Product.Code != null && d.Product.Code.Contains(searchTerm)) ||
                        d.Warehouse.Name.Contains(searchTerm) ||
                        (d.BatchNumber != null && d.BatchNumber.Contains(searchTerm))
                    ))
                    .OrderByDescending(d => d.CreatedAt)
                    .ThenBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 實作特定驗證邏輯
        /// 注意：PurchaseOrderDetailId 允許為 null，以支援無採購單的直接進貨
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(PurchaseReceivingDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本欄位驗證 - 只檢查必要的欄位
                if (entity.PurchaseReceivingId <= 0)
                    errors.Add("採購入庫單不能為空");

                if (entity.ProductId <= 0)
                    errors.Add("商品不能為空");

                if (entity.WarehouseId <= 0)
                    errors.Add("倉庫不能為空");

                // 數量驗證 - 允許為 0，但不能為負數
                if (entity.ReceivedQuantity < 0)
                    errors.Add("入庫數量不能小於 0");

                // 單價驗證 - 允許為 0，但不能為負數
                if (entity.UnitPrice < 0)
                    errors.Add("單價不能小於 0");

                // 注意：PurchaseOrderDetailId 不驗證必填，允許為 null（支援無採購單的直接進貨）
                // 移除重複檢查：允許同一採購訂單明細分配到不同倉庫
                // 因為業務需求允許同一採購明細可以分配到不同倉庫位置

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
                    PurchaseReceivingId = entity.PurchaseReceivingId,
                    ProductId = entity.ProductId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 特定查詢方法

        /// <summary>
        /// 根據採購入庫單ID取得所有明細
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.PurchaseReceivingId == purchaseReceivingId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReceivingIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseReceivingIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據採購訂單明細ID取得所有入庫明細
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetByPurchaseOrderDetailIdAsync(int purchaseOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderDetailIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據商品ID取得所有入庫明細
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.ProductId == productId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據倉庫ID取得所有入庫明細
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseId == warehouseId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = warehouseId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 根據廠商ID取得可退回的入庫明細
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetReturnableDetailsBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.PurchaseReceiving.SupplierId == supplierId &&
                               d.ReceivedQuantity > 0)
                    .OrderByDescending(d => d.CreatedAt)
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
        /// 取得指定廠商的最後一次完整入庫明細（智能載入用）
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetLastCompleteReceivingAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該廠商最近一次的採購入庫單
                var lastReceiving = await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .Where(pr => pr.SupplierId == supplierId && 
                                 pr.PurchaseReceivingDetails.Any())
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenByDescending(pr => pr.CreatedAt)
                    .FirstOrDefaultAsync();
                
                if (lastReceiving == null)
                {
                    return new List<PurchaseReceivingDetail>();
                }
                
                // 返回該入庫單的所有明細
                return lastReceiving.PurchaseReceivingDetails
                    .Where(prd => prd.Product != null)
                    .OrderBy(prd => prd.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastCompleteReceivingAsync), GetType(), _logger, new { 
                    Method = nameof(GetLastCompleteReceivingAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        #endregion

        #region 批次操作方法

        /// <summary>
        /// 批次更新採購入庫明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details)
        {
            return await UpdateDetailsAsync(purchaseReceivingId, details, null);
        }

        /// <summary>
        /// 批次更新採購入庫明細 - 支援外部交易
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? externalTransaction)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
                bool shouldCommitTransaction = false;

                try
                {
                    // 如果沒有外部交易，則創建新的交易
                    if (externalTransaction == null)
                    {
                        transaction = await context.Database.BeginTransactionAsync();
                        shouldCommitTransaction = true;
                    }
                    // 取得現有的明細記錄
                    var existingDetails = await context.PurchaseReceivingDetails
                        .Where(d => d.PurchaseReceivingId == purchaseReceivingId)
                        .ToListAsync();

                    // 準備新的明細資料
                    var newDetailsToAdd = new List<PurchaseReceivingDetail>();
                    var newDetailIds = details?.Where(d => d.Id > 0).Select(d => d.Id).ToList() ?? new List<int>();
                    var detailsToDelete = existingDetails
                        .Where(ed => !newDetailIds.Contains(ed.Id))
                        .ToList();

                    // 處理傳入的明細
                    if (details != null)
                    {
                        foreach (var detail in details)
                        {
                            // 驗證必要欄位：檢查商品、倉庫和入庫數量
                            // 注意：空白的入庫數量會被前端轉換為 0，這裡會被過濾掉
                            if (detail.ProductId <= 0 || detail.WarehouseId <= 0 || detail.ReceivedQuantity <= 0)
                                continue;

                            detail.PurchaseReceivingId = purchaseReceivingId;
                            detail.UpdatedAt = DateTime.UtcNow;

                            if (detail.Id == 0)
                            {
                                // 新增的明細
                                detail.CreatedAt = DateTime.UtcNow;
                                detail.Status = EntityStatus.Active;
                                newDetailsToAdd.Add(detail);
                            }
                            else
                            {
                                // 更新的明細
                                var existingDetail = existingDetails.FirstOrDefault(ed => ed.Id == detail.Id);
                                if (existingDetail != null)
                                {
                                    // 更新現有明細的屬性
                                    existingDetail.ProductId = detail.ProductId;
                                    existingDetail.PurchaseOrderDetailId = detail.PurchaseOrderDetailId;
                                    existingDetail.OrderQuantity = detail.OrderQuantity;
                                    existingDetail.ReceivedQuantity = detail.ReceivedQuantity;
                                    existingDetail.UnitPrice = detail.UnitPrice;
                                    existingDetail.WarehouseId = detail.WarehouseId;
                                    existingDetail.WarehouseLocationId = detail.WarehouseLocationId;
                                    existingDetail.BatchNumber = detail.BatchNumber;
                                    existingDetail.IsReceivingCompleted = detail.IsReceivingCompleted;
                                    existingDetail.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                        }
                    }

                    // 執行資料庫操作
                    // 新增明細
                    if (newDetailsToAdd.Any())
                    {
                        await context.PurchaseReceivingDetails.AddRangeAsync(newDetailsToAdd);
                    }

                    // 軟刪除不需要的明細
                    foreach (var detailToDelete in detailsToDelete)
                    {
                        detailToDelete.UpdatedAt = DateTime.UtcNow;
                    }

                    await context.SaveChangesAsync();
                    
                    // 只有在創建了自己的交易時才提交
                    if (shouldCommitTransaction && transaction != null)
                    {
                        await transaction.CommitAsync();
                    }

                    return ServiceResult.Success();
                }
                catch
                {
                    // 只有在創建了自己的交易時才回滾
                    if (shouldCommitTransaction && transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                    throw;
                }
                finally
                {
                    // 只釋放自己創建的交易
                    if (shouldCommitTransaction && transaction != null)
                    {
                        transaction.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId,
                    DetailsCount = details?.Count ?? 0 
                });
                return ServiceResult.Failure("更新採購入庫明細時發生錯誤");
            }
        }

        #endregion

        #region 驗證方法

        /// <summary>
        /// 檢查商品在指定採購入庫單中是否已存在
        /// </summary>
        public async Task<bool> IsProductExistsInReceivingAsync(int purchaseReceivingId, int productId, int purchaseOrderDetailId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReceivingDetails.Where(d => 
                    d.PurchaseReceivingId == purchaseReceivingId && 
                    d.ProductId == productId && 
                    d.PurchaseOrderDetailId == purchaseOrderDetailId);
                    
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductExistsInReceivingAsync), GetType(), _logger, new { 
                    Method = nameof(IsProductExistsInReceivingAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId,
                    ProductId = productId,
                    PurchaseOrderDetailId = purchaseOrderDetailId,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 檢查商品在指定採購入庫單的特定倉庫位置是否已存在
        /// </summary>
        public async Task<bool> IsProductWarehouseLocationExistsInReceivingAsync(
            int purchaseReceivingId, 
            int productId, 
            int warehouseId, 
            int? warehouseLocationId, 
            int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReceivingDetails.Where(d => 
                    d.PurchaseReceivingId == purchaseReceivingId && 
                    d.ProductId == productId && 
                    d.WarehouseId == warehouseId && 
                    d.WarehouseLocationId == warehouseLocationId);
                    
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductWarehouseLocationExistsInReceivingAsync), GetType(), _logger, new { 
                    Method = nameof(IsProductWarehouseLocationExistsInReceivingAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = warehouseLocationId,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 驗證入庫數量是否超過訂購數量（已簡化，現在主要驗證在 ValidateAsync 中進行）
        /// </summary>
        public async Task<ServiceResult> ValidateReceivingQuantityAsync(int purchaseOrderDetailId, int newReceivedQuantity, int? excludeDetailId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得採購訂單明細
                var purchaseOrderDetail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(pod => pod.Id == purchaseOrderDetailId);
                
                if (purchaseOrderDetail == null)
                    return ServiceResult.Failure("找不到對應的採購訂單明細");

                // 計算已入庫數量（排除當前編輯的明細）
                var existingReceivedQuantity = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId && 
                               (excludeDetailId == null || d.Id != excludeDetailId.Value))
                    .SumAsync(d => d.ReceivedQuantity);

                // 計算該訂單明細的總訂購數量
                var totalOrderQuantity = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId)
                    .SumAsync(d => d.OrderQuantity);

                // 檢查總入庫數量是否超過總訂購數量
                var totalReceivedQuantity = existingReceivedQuantity + newReceivedQuantity;
                if (totalReceivedQuantity > totalOrderQuantity && totalOrderQuantity > 0)
                {
                    return ServiceResult.Failure(
                        $"入庫數量超過訂購數量。總訂購：{totalOrderQuantity}，已入庫：{existingReceivedQuantity}，本次入庫：{newReceivedQuantity}");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateReceivingQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateReceivingQuantityAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId,
                    NewReceivedQuantity = newReceivedQuantity,
                    ExcludeDetailId = excludeDetailId 
                });
                return ServiceResult.Failure("驗證入庫數量時發生錯誤");
            }
        }

        #endregion

        #region 計算方法

        /// <summary>
        /// 計算採購入庫明細的小計金額
        /// </summary>
        public decimal CalculateSubtotalAmount(int receivedQuantity, decimal unitPrice)
        {
            return receivedQuantity * unitPrice;
        }

        /// <summary>
        /// 計算採購入庫明細的訂購金額
        /// </summary>
        public decimal CalculateOrderAmount(int orderQuantity, decimal unitPrice)
        {
            return orderQuantity * unitPrice;
        }

        /// <summary>
        /// 計算指定採購入庫單的總訂購金額
        /// </summary>
        public async Task<decimal> CalculateTotalOrderAmountAsync(int purchaseReceivingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseReceivingId == purchaseReceivingId)
                    .SumAsync(d => d.OrderQuantity * d.UnitPrice);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalOrderAmountAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalOrderAmountAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId 
                });
                return 0;
            }
        }

        #endregion

        #region 關聯資料查詢

        /// <summary>
        /// 取得採購入庫明細的庫存異動記錄
        /// </summary>
        public async Task<List<InventoryTransaction>> GetRelatedInventoryTransactionsAsync(int purchaseReceivingDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得採購入庫明細資訊
                var detail = await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .FirstOrDefaultAsync(d => d.Id == purchaseReceivingDetailId);
                
                if (detail == null)
                    return new List<InventoryTransaction>();

                // 根據入庫單號查詢相關的庫存異動記錄（透過明細的商品ID過濾）
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(t => (t.Remarks != null && detail.PurchaseReceiving.Code != null && t.Remarks.Contains(detail.PurchaseReceiving.Code)) &&
                               t.Details.Any(d => d.ProductId == detail.ProductId))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRelatedInventoryTransactionsAsync), GetType(), _logger, new { 
                    Method = nameof(GetRelatedInventoryTransactionsAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingDetailId = purchaseReceivingDetailId 
                });
                return new List<InventoryTransaction>();
            }
        }

        #endregion

        /// <summary>
        /// 覆蓋永久刪除方法，加入庫存回滾邏輯
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                // 查詢要刪除的明細，包含相關資訊
                var entity = await context.PurchaseReceivingDetails
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.Product)
                    .FirstOrDefaultAsync(d => d.Id == id);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure($"找不到ID為 {id} 的進貨明細");
                }

                // 回滾採購訂單明細的已進貨數量
                if (entity.PurchaseOrderDetailId > 0 && entity.ReceivedQuantity > 0)
                {
                    var purchaseOrderDetail = entity.PurchaseOrderDetail ?? 
                        await context.PurchaseOrderDetails.FirstOrDefaultAsync(pod => pod.Id == entity.PurchaseOrderDetailId);
                        
                    if (purchaseOrderDetail != null)
                    {
                        // 減少已進貨數量
                        purchaseOrderDetail.ReceivedQuantity = Math.Max(0, purchaseOrderDetail.ReceivedQuantity - entity.ReceivedQuantity);
                        purchaseOrderDetail.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 庫存回滾：因為取消進貨，所以要扣減庫存
                if (_inventoryStockService != null && entity.ReceivedQuantity > 0)
                {
                    // 使用 ReduceStockAsync 扣減庫存（因為取消進貨，實際收到的商品應該從庫存中減少）
                    var stockResult = await _inventoryStockService.ReduceStockAsync(
                        productId: entity.ProductId,
                        warehouseId: entity.WarehouseId,
                        quantity: entity.ReceivedQuantity,
                        transactionType: Data.Enums.InventoryTransactionTypeEnum.Adjustment,
                        transactionNumber: $"取消進貨-{entity.Id}",
                        locationId: entity.WarehouseLocationId,
                        remarks: $"取消進貨明細ID: {entity.Id}",
                        sourceDocumentType: Data.Enums.InventorySourceDocumentTypes.PurchaseReceiving,
                        sourceDocumentId: entity.PurchaseReceivingId
                    );

                    if (!stockResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure($"庫存回滾失敗: {stockResult.ErrorMessage}");
                    }
                }

                // 執行實際刪除
                context.PurchaseReceivingDetails.Remove(entity);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure($"永久刪除進貨明細時發生錯誤：{ex.Message}");
            }
        }
    }
}
