using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購訂單明細服務實作
    /// </summary>
    public class PurchaseOrderDetailService : GenericManagementService<PurchaseOrderDetail>, IPurchaseOrderDetailService
    {
        public PurchaseOrderDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseOrderDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseOrderDetail>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基底類別方法

        /// <summary>
        /// 取得所有採購訂單明細（包含關聯資料）
        /// </summary>
        public override async Task<List<PurchaseOrderDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
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
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 根據ID取得採購訂單明細（包含關聯資料）
        /// </summary>
        public override async Task<PurchaseOrderDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
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
        public override async Task<List<PurchaseOrderDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
                    .Where(d => (
                        d.PurchaseOrder.PurchaseOrderNumber.Contains(searchTerm) ||
                        d.Product.Name.Contains(searchTerm) ||
                        (d.Product.Code != null && d.Product.Code.Contains(searchTerm)) ||
                        d.PurchaseOrder.Supplier.CompanyName.Contains(searchTerm)
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
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 實作特定驗證邏輯
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(PurchaseOrderDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本欄位驗證
                if (entity.PurchaseOrderId <= 0)
                    errors.Add("採購訂單不能為空");

                if (entity.ProductId <= 0)
                    errors.Add("商品不能為空");

                if (entity.OrderQuantity <= 0)
                    errors.Add("訂購數量必須大於 0");

                if (entity.UnitPrice < 0)
                    errors.Add("單價不能小於 0");

                if (entity.ReceivedQuantity < 0)
                    errors.Add("已進貨數量不能小於 0");

                if (entity.ReceivedQuantity > entity.OrderQuantity)
                    errors.Add("已進貨數量不能大於訂購數量");

                // 注釋：允許同一商品在同一採購訂單中多次出現
                // 檢查商品在同一採購訂單中是否重複 (已停用 - 允許同一商品多次輸入)
                /*
                if (await IsProductExistsInOrderAsync(
                    entity.PurchaseOrderId, 
                    entity.ProductId, 
                    entity.Id == 0 ? null : entity.Id))
                {
                    errors.Add("該商品在此採購訂單中已存在");
                }
                */

                // 檢查預計到貨日期不能早於訂單日期
                if (entity.ExpectedDeliveryDate.HasValue)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var purchaseOrder = await context.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.Id == entity.PurchaseOrderId);
                    
                    if (purchaseOrder != null && entity.ExpectedDeliveryDate.Value < purchaseOrder.OrderDate)
                    {
                        errors.Add("預計到貨日期不能早於訂單日期");
                    }
                }

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
                    PurchaseOrderId = entity.PurchaseOrderId,
                    ProductId = entity.ProductId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 檢查採購訂單明細是否可以被刪除
        /// 檢查邏輯：
        /// 1. 先執行基類的刪除檢查（外鍵關聯等）
        /// 2. 檢查明細是否已有入庫記錄
        ///    - 檢查欄位：ReceivedQuantity (已進貨數量)
        ///    - 限制原因：已入庫的明細不可刪除，以保持庫存資料一致性
        /// </summary>
        /// <param name="entity">要檢查的採購訂單明細實體</param>
        /// <returns>檢查結果，包含是否可刪除及錯誤訊息</returns>
        protected override async Task<ServiceResult> CanDeleteAsync(PurchaseOrderDetail entity)
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
                
                var loadedEntity = await context.PurchaseOrderDetails
                    .Include(pod => pod.Product)
                    .FirstOrDefaultAsync(pod => pod.Id == entity.Id);

                if (loadedEntity == null)
                {
                    return ServiceResult.Failure("找不到要檢查的採購訂單明細");
                }

                // 3. 檢查是否已有入庫記錄
                if (loadedEntity.ReceivedQuantity > 0)
                {
                    var productName = loadedEntity.Product?.Name ?? "未知商品";
                    return ServiceResult.Failure(
                        $"無法刪除此明細，因為商品「{productName}」已有入庫記錄（已入庫 {loadedEntity.ReceivedQuantity} 個）"
                    );
                }

                // 4. 所有檢查通過，允許刪除
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(CanDeleteAsync), GetType(), _logger,
                    new { EntityId = entity.Id, ProductId = entity.ProductId }
                );
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 特定查詢方法

        /// <summary>
        /// 根據採購訂單ID取得所有明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
                    .Where(d => d.PurchaseOrderId == purchaseOrderId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseOrderIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 根據商品ID取得所有採購訂單明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
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
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 根據供應商ID取得所有採購訂單明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
                    .Where(d => d.PurchaseOrder.SupplierId == supplierId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 取得有待進貨數量的採購訂單明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsAsync(int? supplierId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseOrderDetails
                    .Include(d => d.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseReceiving)
                    .Where(d => d.PurchaseOrder.IsApproved && 
                               d.ReceivedQuantity < d.OrderQuantity);

                if (supplierId.HasValue)
                {
                    query = query.Where(d => d.PurchaseOrder.SupplierId == supplierId.Value);
                }

                return await query
                    .OrderBy(d => d.PurchaseOrder.OrderDate)
                    .ThenBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingReceivingDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingReceivingDetailsAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 根據供應商ID取得有待進貨數量的採購訂單明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierAsync(int supplierId)
        {
            return await GetPendingReceivingDetailsAsync(supplierId);
        }

        #endregion

        #region 批次操作方法

        /// <summary>
        /// 批次更新採購訂單明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int purchaseOrderId, List<PurchaseOrderDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得現有的明細記錄
                    var existingDetails = await context.PurchaseOrderDetails
                        .Where(d => d.PurchaseOrderId == purchaseOrderId)
                        .ToListAsync();

                    // 準備新的明細資料
                    var newDetailsToAdd = new List<PurchaseOrderDetail>();
                    var newDetailIds = details?.Where(d => d.Id > 0).Select(d => d.Id).ToList() ?? new List<int>();
                    var detailsToDelete = existingDetails
                        .Where(ed => !newDetailIds.Contains(ed.Id))
                        .ToList();

                    // 處理傳入的明細
                    if (details != null)
                    {
                        foreach (var detail in details.Where(d => d.OrderQuantity > 0))
                        {
                            // 驗證必要欄位
                            if (detail.ProductId <= 0 || detail.OrderQuantity <= 0 || detail.UnitPrice < 0)
                                continue;

                            detail.PurchaseOrderId = purchaseOrderId;
                            detail.UpdatedAt = DateTime.UtcNow;

                            if (detail.Id == 0)
                            {
                                // 新增的明細
                                detail.CreatedAt = DateTime.UtcNow;
                                detail.Status = EntityStatus.Active;
                                detail.ReceivedQuantity = 0; // 新增時已進貨數量為 0
                                detail.ReceivedAmount = 0; // 新增時已進貨金額為 0
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
                                    existingDetail.OrderQuantity = detail.OrderQuantity;
                                    existingDetail.UnitPrice = detail.UnitPrice;
                                    existingDetail.ExpectedDeliveryDate = detail.ExpectedDeliveryDate;
                                    // 不更新 ReceivedQuantity 和 ReceivedAmount，這些由進貨作業更新
                                    existingDetail.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                        }
                    }

                    // 執行資料庫操作
                    // 新增明細
                    if (newDetailsToAdd.Any())
                    {
                        await context.PurchaseOrderDetails.AddRangeAsync(newDetailsToAdd);
                    }

                    // 軟刪除不需要的明細
                    foreach (var detailToDelete in detailsToDelete)
                    {
                        detailToDelete.UpdatedAt = DateTime.UtcNow;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId,
                    DetailsCount = details?.Count ?? 0 
                });
                return ServiceResult.Failure("更新採購訂單明細時發生錯誤");
            }
        }

        /// <summary>
        /// 更新已進貨數量
        /// </summary>
        public async Task<ServiceResult> UpdateReceivedQuantityAsync(int purchaseOrderDetailId, int receivedQuantity, decimal receivedAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var detail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == purchaseOrderDetailId);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的採購訂單明細");

                if (receivedQuantity < 0)
                    return ServiceResult.Failure("已進貨數量不能小於 0");

                if (receivedQuantity > detail.OrderQuantity)
                    return ServiceResult.Failure("已進貨數量不能大於訂購數量");

                detail.ReceivedQuantity = receivedQuantity;
                detail.ReceivedAmount = receivedAmount;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceivedQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateReceivedQuantityAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId,
                    ReceivedQuantity = receivedQuantity,
                    ReceivedAmount = receivedAmount 
                });
                return ServiceResult.Failure("更新已進貨數量時發生錯誤");
            }
        }

        /// <summary>
        /// 批次更新已進貨數量
        /// </summary>
        public async Task<ServiceResult> BatchUpdateReceivedQuantityAsync(List<(int DetailId, int ReceivedQuantity, decimal ReceivedAmount)> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                    return ServiceResult.Success();

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var (detailId, receivedQuantity, receivedAmount) in updates)
                    {
                        var detail = await context.PurchaseOrderDetails
                            .FirstOrDefaultAsync(d => d.Id == detailId);
                        
                        if (detail != null)
                        {
                            detail.ReceivedQuantity = receivedQuantity;
                            detail.ReceivedAmount = receivedAmount;
                            detail.UpdatedAt = DateTime.UtcNow;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateReceivedQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(BatchUpdateReceivedQuantityAsync),
                    ServiceType = GetType().Name,
                    UpdatesCount = updates?.Count ?? 0 
                });
                return ServiceResult.Failure("批次更新已進貨數量時發生錯誤");
            }
        }

        #endregion

        #region 驗證方法

        /// <summary>
        /// 檢查商品在指定採購訂單中是否已存在
        /// </summary>
        public async Task<bool> IsProductExistsInOrderAsync(int purchaseOrderId, int productId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseOrderDetails.Where(d => 
                    d.PurchaseOrderId == purchaseOrderId && 
                    d.ProductId == productId);
                    
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductExistsInOrderAsync), GetType(), _logger, new { 
                    Method = nameof(IsProductExistsInOrderAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = productId,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        #endregion

        #region 計算方法

        /// <summary>
        /// 計算採購訂單明細的小計金額
        /// </summary>
        public decimal CalculateSubtotalAmount(int orderQuantity, decimal unitPrice)
        {
            return orderQuantity * unitPrice;
        }

        /// <summary>
        /// 計算待進貨數量
        /// </summary>
        public int CalculatePendingQuantity(int orderQuantity, int receivedQuantity)
        {
            return Math.Max(0, orderQuantity - receivedQuantity);
        }

        #endregion

        #region 關聯資料查詢

        /// <summary>
        /// 取得採購訂單明細的進貨記錄
        /// </summary>
        public async Task<List<PurchaseReceivingDetail>> GetReceivingDetailsAsync(int purchaseOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivingDetails
                    .Include(prd => prd.PurchaseReceiving)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Warehouse)
                    .Include(prd => prd.WarehouseLocation)
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId)
                    .OrderByDescending(prd => prd.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivingDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetReceivingDetailsAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId 
                });
                return new List<PurchaseReceivingDetail>();
            }
        }

        /// <summary>
        /// 檢查採購訂單明細是否已完成進貨
        /// </summary>
        public async Task<bool> IsReceivingCompletedAsync(int purchaseOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == purchaseOrderDetailId);
                
                if (detail == null)
                    return false;

                return detail.ReceivedQuantity >= detail.OrderQuantity;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsReceivingCompletedAsync), GetType(), _logger, new { 
                    Method = nameof(IsReceivingCompletedAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId 
                });
                return false;
            }
        }

        /// <summary>
        /// 取得採購訂單明細的統計資料
        /// </summary>
        public async Task<(int TotalQuantity, int ReceivedQuantity, int PendingQuantity, decimal TotalAmount, decimal ReceivedAmount)> GetStatisticsAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.PurchaseOrderDetails
                    .Where(d => d.PurchaseOrderId == purchaseOrderId)
                    .ToListAsync();

                var totalQuantity = details.Sum(d => d.OrderQuantity);
                var receivedQuantity = details.Sum(d => d.ReceivedQuantity);
                var pendingQuantity = totalQuantity - receivedQuantity;
                var totalAmount = details.Sum(d => d.SubtotalAmount);
                var receivedAmount = details.Sum(d => d.ReceivedAmount);

                return (totalQuantity, receivedQuantity, pendingQuantity, totalAmount, receivedAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new { 
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
                });
                return (0, 0, 0, 0m, 0m);
            }
        }

        /// <summary>
        /// 獲取廠商最近一次完整的採購訂單明細（智能下單用）
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetLastCompletePurchaseAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該廠商最近一次已核准的採購單
                var lastPurchaseOrder = await context.PurchaseOrders
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .Where(po => po.SupplierId == supplierId 
                              && po.IsApproved 
                              && po.PurchaseOrderDetails.Any())
                    .OrderByDescending(po => po.OrderDate)
                    .ThenByDescending(po => po.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastPurchaseOrder == null)
                    return new List<PurchaseOrderDetail>();

                // 返回該採購單的所有明細
                return lastPurchaseOrder.PurchaseOrderDetails
                    .Where(pod => pod.Product != null)
                    .OrderBy(pod => pod.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastCompletePurchaseAsync), GetType(), _logger, new { 
                    SupplierId = supplierId
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 重新計算並更新已進貨數量和金額
        /// </summary>
        public async Task<ServiceResult> RecalculateReceivedQuantityAsync(int purchaseOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var detail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == purchaseOrderDetailId);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的採購訂單明細");

                // 重新計算已進貨數量和金額
                var receivingDetails = await context.PurchaseReceivingDetails
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId)
                    .ToListAsync();

                var totalReceivedQuantity = receivingDetails.Sum(prd => prd.ReceivedQuantity);
                var totalReceivedAmount = receivingDetails.Sum(prd => prd.SubtotalAmount);

                detail.ReceivedQuantity = totalReceivedQuantity;
                detail.ReceivedAmount = totalReceivedAmount;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RecalculateReceivedQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(RecalculateReceivedQuantityAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId 
                });
                return ServiceResult.Failure("重新計算已進貨數量時發生錯誤");
            }
        }

        #endregion
    }
}
