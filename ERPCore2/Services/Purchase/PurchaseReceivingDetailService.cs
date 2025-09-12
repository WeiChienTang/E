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
        public PurchaseReceivingDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReceivingDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceivingDetail>> logger) : base(contextFactory, logger)
        {
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => !d.IsDeleted)
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => !d.IsDeleted && (
                        d.PurchaseReceiving.ReceiptNumber.Contains(searchTerm) ||
                        d.Product.Name.Contains(searchTerm) ||
                        (d.Product.Code != null && d.Product.Code.Contains(searchTerm)) ||
                        d.Warehouse.Name.Contains(searchTerm) ||
                        (d.BatchNumber != null && d.BatchNumber.Contains(searchTerm)) ||
                        (d.InspectionRemarks != null && d.InspectionRemarks.Contains(searchTerm))
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
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(PurchaseReceivingDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本欄位驗證
                if (entity.PurchaseReceivingId <= 0)
                    errors.Add("採購入庫單不能為空");

                if (entity.PurchaseOrderDetailId <= 0)
                    errors.Add("採購訂單明細不能為空");

                if (entity.ProductId <= 0)
                    errors.Add("商品不能為空");

                if (entity.ReceivedQuantity <= 0 && !entity.IsReceivingCompleted)
                    errors.Add("入庫數量必須大於 0 或標記為已完成進貨");

                if (entity.UnitPrice < 0)
                    errors.Add("單價不能小於 0");

                if (entity.WarehouseId <= 0)
                    errors.Add("倉庫不能為空");

                // 檢查商品在同一採購入庫單中是否重複
                if (await IsProductExistsInReceivingAsync(
                    entity.PurchaseReceivingId, 
                    entity.ProductId, 
                    entity.PurchaseOrderDetailId, 
                    entity.Id == 0 ? null : entity.Id))
                {
                    errors.Add("該商品在此採購入庫單中已存在");
                }

                // 驗證入庫數量是否超過訂購數量
                if (entity.ReceivedQuantity > 0)
                {
                    var quantityValidation = await ValidateReceivingQuantityAsync(
                        entity.PurchaseOrderDetailId, 
                        entity.ReceivedQuantity, 
                        entity.Id == 0 ? null : entity.Id);
                    
                    if (!quantityValidation.IsSuccess)
                        errors.Add(quantityValidation.ErrorMessage);
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.PurchaseReceivingId == purchaseReceivingId && !d.IsDeleted)
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
                    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId && !d.IsDeleted)
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.ProductId == productId && !d.IsDeleted)
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseId == warehouseId && !d.IsDeleted)
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
                        .ThenInclude(pod => pod.PurchaseOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => !d.IsDeleted && 
                               d.PurchaseReceiving.SupplierId == supplierId &&
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

        #endregion

        #region 批次操作方法

        /// <summary>
        /// 批次更新採購入庫明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int purchaseReceivingId, List<PurchaseReceivingDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得現有的明細記錄
                    var existingDetails = await context.PurchaseReceivingDetails
                        .Where(d => d.PurchaseReceivingId == purchaseReceivingId && !d.IsDeleted)
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
                        foreach (var detail in details.Where(d => d.ReceivedQuantity > 0 || d.IsReceivingCompleted))
                        {
                            // 驗證必要欄位
                            if (detail.ProductId <= 0 || detail.PurchaseOrderDetailId <= 0 || detail.WarehouseId <= 0)
                                continue;

                            detail.PurchaseReceivingId = purchaseReceivingId;
                            detail.UpdatedAt = DateTime.UtcNow;

                            if (detail.Id == 0)
                            {
                                // 新增的明細
                                detail.CreatedAt = DateTime.UtcNow;
                                detail.IsDeleted = false;
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
                                    existingDetail.ReceivedQuantity = detail.ReceivedQuantity;
                                    existingDetail.UnitPrice = detail.UnitPrice;
                                    existingDetail.WarehouseId = detail.WarehouseId;
                                    existingDetail.WarehouseLocationId = detail.WarehouseLocationId;
                                    existingDetail.InspectionRemarks = detail.InspectionRemarks;
                                    existingDetail.BatchNumber = detail.BatchNumber;
                                    existingDetail.ExpiryDate = detail.ExpiryDate;
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
                        detailToDelete.IsDeleted = true;
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
                    d.PurchaseOrderDetailId == purchaseOrderDetailId && 
                    !d.IsDeleted);
                    
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
        /// 驗證入庫數量是否超過訂購數量
        /// </summary>
        public async Task<ServiceResult> ValidateReceivingQuantityAsync(int purchaseOrderDetailId, int newReceivedQuantity, int? excludeDetailId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得採購訂單明細
                var purchaseOrderDetail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(pod => pod.Id == purchaseOrderDetailId && !pod.IsDeleted);
                
                if (purchaseOrderDetail == null)
                    return ServiceResult.Failure("找不到對應的採購訂單明細");

                // 計算已入庫數量（排除當前編輯的明細）
                var existingReceivedQuantity = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId && 
                               !d.IsDeleted && 
                               (excludeDetailId == null || d.Id != excludeDetailId.Value))
                    .SumAsync(d => d.ReceivedQuantity);

                // 檢查總入庫數量是否超過訂購數量
                var totalReceivedQuantity = existingReceivedQuantity + newReceivedQuantity;
                if (totalReceivedQuantity > purchaseOrderDetail.OrderQuantity)
                {
                    return ServiceResult.Failure(
                        $"入庫數量超過訂購數量。訂購：{purchaseOrderDetail.OrderQuantity}，已入庫：{existingReceivedQuantity}，本次入庫：{newReceivedQuantity}");
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
                    .FirstOrDefaultAsync(d => d.Id == purchaseReceivingDetailId && !d.IsDeleted);
                
                if (detail == null)
                    return new List<InventoryTransaction>();

                // 根據入庫單號查詢相關的庫存異動記錄
                return await context.InventoryTransactions
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => !t.IsDeleted && 
                               (t.ReferenceNumber != null && t.ReferenceNumber.Contains(detail.PurchaseReceiving.ReceiptNumber)) &&
                               t.ProductId == detail.ProductId)
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
    }
}