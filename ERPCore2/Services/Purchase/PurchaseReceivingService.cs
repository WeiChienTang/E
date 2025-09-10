using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購進貨服務實作
    /// </summary>
    public class PurchaseReceivingService : GenericManagementService<PurchaseReceiving>, IPurchaseReceivingService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public PurchaseReceivingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReceivingService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceiving>> logger) : base(contextFactory, logger)
        {
        }

        public PurchaseReceivingService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PurchaseReceiving>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        #region 覆寫基本方法

        public override async Task<List<PurchaseReceiving>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)  // 新增直接供應商關聯
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.ReceiptNumber)
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

        public override async Task<PurchaseReceiving?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)  // 新增直接供應商關聯
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod.Product)
                                .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                            .ThenInclude(pod => pod.PurchaseOrder)
                                .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => !pr.IsDeleted)
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

        public override async Task<List<PurchaseReceiving>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)  // 新增直接供應商關聯
                    .Include(pr => pr.ConfirmedByUser)
                    .Where(pr => !pr.IsDeleted && (
                        pr.ReceiptNumber.Contains(searchTerm) ||
                        (pr.PurchaseOrder != null && pr.PurchaseOrder.PurchaseOrderNumber.Contains(searchTerm)) ||
                        (pr.PurchaseOrder != null && pr.PurchaseOrder.Supplier != null && pr.PurchaseOrder.Supplier.CompanyName.Contains(searchTerm)) ||
                        (pr.Supplier != null && pr.Supplier.CompanyName.Contains(searchTerm)) ||
                        (pr.Remarks != null && pr.Remarks.Contains(searchTerm))
                    ))
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.ReceiptNumber)
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

        public override async Task<ServiceResult> ValidateAsync(PurchaseReceiving entity)
        {
            if (entity == null)
            {
                return ServiceResult.Failure("進貨單資料不可為空");
            }

            if (string.IsNullOrWhiteSpace(entity.ReceiptNumber))
            {
                return ServiceResult.Failure("進貨單號為必填");
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查進貨單號是否重複
                bool exists;
                if (entity.Id == 0)
                {
                    exists = await context.PurchaseReceivings
                        .AnyAsync(pr => pr.ReceiptNumber == entity.ReceiptNumber && !pr.IsDeleted);
                }
                else
                {
                    exists = await context.PurchaseReceivings
                        .AnyAsync(pr => pr.ReceiptNumber == entity.ReceiptNumber && pr.Id != entity.Id && !pr.IsDeleted);
                }
                
                if (exists)
                {
                    return ServiceResult.Failure("進貨單號已存在");
                }

                // 檢查採購訂單（僅當有指定採購單時）
                if (entity.PurchaseOrderId.HasValue)
                {
                    var purchaseOrder = await context.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.Id == entity.PurchaseOrderId && !po.IsDeleted);
                    
                    if (purchaseOrder == null)
                    {
                        return ServiceResult.Failure("指定的採購訂單不存在");
                    }
                    
                    if (!purchaseOrder.IsApproved)
                    {
                        return ServiceResult.Failure("只有已核准的採購訂單才能進行進貨作業");
                    }
                    
                    // 驗證供應商一致性
                    if (entity.SupplierId != purchaseOrder.SupplierId)
                    {
                        return ServiceResult.Failure("採購入庫的供應商必須與採購訂單的供應商一致");
                    }
                }
                else
                {
                    // 多採購單模式，驗證必須有供應商
                    if (entity.SupplierId <= 0)
                    {
                        return ServiceResult.Failure("多採購單模式下，供應商為必填");
                    }
                    
                    // 驗證供應商是否存在
                    var supplierExists = await context.Suppliers
                        .AnyAsync(s => s.Id == entity.SupplierId && !s.IsDeleted);
                    
                    if (!supplierExists)
                    {
                        return ServiceResult.Failure("指定的供應商不存在");
                    }
                }



                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 特定業務方法

        public async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var dateString = today.ToString("yyyyMMdd");
                
                var lastReceipt = await context.PurchaseReceivings
                    .Where(pr => pr.ReceiptNumber.StartsWith($"RCV{dateString}") && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptNumber)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastReceipt != null)
                {
                    var lastNumberPart = lastReceipt.ReceiptNumber.Substring(11); // "RCV" + 8位日期 = 11位
                    if (int.TryParse(lastNumberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"RCV{dateString}{nextNumber:D3}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateReceiptNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateReceiptNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"RCV{DateTime.Today:yyyyMMdd}001";
            }
        }

        public async Task<bool> IsReceiptNumberExistsAsync(string receiptNumber, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(receiptNumber))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                
                if (excludeId.HasValue)
                {
                    return await context.PurchaseReceivings
                        .AnyAsync(pr => pr.ReceiptNumber == receiptNumber && pr.Id != excludeId.Value && !pr.IsDeleted);
                }
                else
                {
                    return await context.PurchaseReceivings
                        .AnyAsync(pr => pr.ReceiptNumber == receiptNumber && !pr.IsDeleted);
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsReceiptNumberExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsReceiptNumberExistsAsync),
                    ServiceType = GetType().Name,
                    ReceiptNumber = receiptNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<PurchaseReceiving>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)  // 新增直接供應商關聯
                    .Where(pr => pr.ReceiptDate >= startDate && pr.ReceiptDate <= endDate && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.ReceiptNumber)
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

        public async Task<List<PurchaseReceiving>> GetByPurchaseOrderAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po!.Supplier)
                    .Include(pr => pr.Supplier)  // 新增直接供應商關聯
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Warehouse)
                    .Where(pr => pr.PurchaseOrderId == purchaseOrderId && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.ReceiptNumber)
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

        public async Task<ServiceResult> ConfirmReceiptAsync(int id, int confirmedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var receipt = await context.PurchaseReceivings
                        .Include(pr => pr.PurchaseReceivingDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);

                    if (receipt == null)
                    {
                        return ServiceResult.Failure("找不到指定的進貨單");
                    }

                    // 移除狀態檢查，直接確認（庫存更新已在儲存時處理）
                    receipt.ConfirmedAt = DateTime.Now;
                    receipt.ConfirmedBy = confirmedBy;
                    receipt.UpdatedAt = DateTime.Now;

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult.Success();
                }
                catch (Exception)
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
                return ServiceResult.Failure("確認進貨單時發生錯誤");
            }
        }

        public async Task<ServiceResult> CancelReceiptAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var receipt = await context.PurchaseReceivings
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);

                if (receipt == null)
                {
                    return ServiceResult.Failure("找不到指定的進貨單");
                }

                // 移除狀態檢查，直接標記更新時間
                receipt.UpdatedAt = DateTime.Now;

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
                return ServiceResult.Failure("取消進貨單時發生錯誤");
            }
        }

        /// <summary>
        /// 儲存採購入庫連同明細
        /// </summary>
        public async Task<ServiceResult<PurchaseReceiving>> SaveWithDetailsAsync(PurchaseReceiving purchaseReceiving, List<PurchaseReceivingDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 驗證主檔
                    var validationResult = await ValidateAsync(purchaseReceiving);
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<PurchaseReceiving>.Failure(validationResult.ErrorMessage);
                    }

                    // 儲存主檔 - 在同一個 context 中處理
                    PurchaseReceiving savedEntity;
                    var dbSet = context.Set<PurchaseReceiving>();

                    if (purchaseReceiving.Id > 0)
                    {
                        // 更新模式
                        var existingEntity = await dbSet
                            .FirstOrDefaultAsync(x => x.Id == purchaseReceiving.Id && !x.IsDeleted);
                            
                        if (existingEntity == null)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult<PurchaseReceiving>.Failure("找不到要更新的採購入庫資料");
                        }

                        // 更新主檔資訊
                        purchaseReceiving.UpdatedAt = DateTime.UtcNow;
                        purchaseReceiving.CreatedAt = existingEntity.CreatedAt; // 保持原建立時間
                        purchaseReceiving.CreatedBy = existingEntity.CreatedBy; // 保持原建立者

                        context.Entry(existingEntity).CurrentValues.SetValues(purchaseReceiving);
                        savedEntity = existingEntity;
                    }
                    else
                    {
                        // 新增模式
                        purchaseReceiving.CreatedAt = DateTime.UtcNow;
                        purchaseReceiving.UpdatedAt = DateTime.UtcNow;
                        purchaseReceiving.IsDeleted = false;
                        purchaseReceiving.Status = EntityStatus.Active;

                        await dbSet.AddAsync(purchaseReceiving);
                        savedEntity = purchaseReceiving;
                    }

                    // 先儲存主檔以取得 ID
                    await context.SaveChangesAsync();

                    // 儲存明細 - 在同一個 context 和 transaction 中處理
                    var detailResult = await UpdateDetailsInContext(context, savedEntity.Id, details);
                    if (!detailResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<PurchaseReceiving>.Failure($"儲存明細失敗：{detailResult.ErrorMessage}");
                    }

                    // 自動計算並更新入庫狀態
                    await UpdateReceivingStatusAsync(context, savedEntity, details);

                    // 更新庫存邏輯
                    if (_inventoryStockService != null)
                    {
                        foreach (var detail in details.Where(d => !d.IsDeleted && d.ReceivedQuantity > 0))
                        {
                            var stockResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                detail.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Purchase,
                                savedEntity.ReceiptNumber,
                                detail.UnitPrice,
                                detail.WarehouseLocationId,
                                $"採購進貨 - {savedEntity.ReceiptNumber}"
                            );

                            if (!stockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult<PurchaseReceiving>.Failure($"更新庫存失敗：{stockResult.ErrorMessage}");
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<PurchaseReceiving>.Success(savedEntity);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveWithDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(SaveWithDetailsAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceiving.Id 
                });
                return ServiceResult<PurchaseReceiving>.Failure($"儲存採購入庫時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 在指定的 DbContext 中更新採購入庫明細
        /// </summary>
        private async Task<ServiceResult> UpdateDetailsInContext(AppDbContext context, int purchaseReceivingId, List<PurchaseReceivingDetail> details)
        {
            try
            {
                // 取得現有的明細記錄
                var existingDetails = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseReceivingId == purchaseReceivingId && !d.IsDeleted)
                    .ToListAsync();

                // 準備新的明細資料
                var newDetailsToAdd = new List<PurchaseReceivingDetail>();
                var detailsToUpdate = new List<PurchaseReceivingDetail>();
                var detailsToDelete = new List<PurchaseReceivingDetail>();

                // 處理傳入的明細
                foreach (var detail in (details ?? new List<PurchaseReceivingDetail>()).Where(d => d.ReceivedQuantity > 0 || d.IsReceivingCompleted))
                {
                    // 驗證必要欄位
                    if (detail.PurchaseOrderDetailId <= 0)
                    {
                        continue; // 跳過無效的明細
                    }
                    
                    detail.PurchaseReceivingId = purchaseReceivingId;
                    
                    if (detail.Id <= 0)
                    {
                        // 新增的明細
                        detail.CreatedAt = DateTime.UtcNow;
                        detail.UpdatedAt = DateTime.UtcNow;
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
                            existingDetail.ReceivedQuantity = detail.ReceivedQuantity;
                            existingDetail.UnitPrice = detail.UnitPrice;
                            existingDetail.InspectionRemarks = detail.InspectionRemarks;
                            existingDetail.BatchNumber = detail.BatchNumber;
                            existingDetail.ExpiryDate = detail.ExpiryDate;
                            existingDetail.WarehouseLocationId = detail.WarehouseLocationId;
                            existingDetail.IsReceivingCompleted = detail.IsReceivingCompleted;
                            existingDetail.UpdatedAt = DateTime.UtcNow;
                            detailsToUpdate.Add(existingDetail);
                        }
                    }
                }

                // 找出要刪除的明細（在現有明細中但不在新明細中）
                var newDetailIds = (details ?? new List<PurchaseReceivingDetail>())
                    .Where(d => d.Id > 0)
                    .Select(d => d.Id)
                    .ToList();
                
                detailsToDelete = existingDetails
                    .Where(ed => !newDetailIds.Contains(ed.Id))
                    .ToList();

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

                // 儲存變更
                await context.SaveChangesAsync();

                // 更新採購單明細的已進貨數量
                await UpdatePurchaseOrderDetailsReceivedQuantity(context, newDetailsToAdd, detailsToUpdate, detailsToDelete);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsInContext), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsInContext),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId 
                });
                return ServiceResult.Failure($"更新採購入庫明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新採購入庫明細
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
                    var detailsToUpdate = new List<PurchaseReceivingDetail>();
                    var detailsToDelete = new List<PurchaseReceivingDetail>();

                    // 處理傳入的明細
                    foreach (var detail in details.Where(d => d.ReceivedQuantity > 0 || d.IsReceivingCompleted))
                    {
                        // 驗證必要欄位
                        if (detail.PurchaseOrderDetailId <= 0)
                        {
                            continue; // 跳過無效的明細
                        }
                        
                        detail.PurchaseReceivingId = purchaseReceivingId;
                        
                        if (detail.Id <= 0)
                        {
                            // 新增的明細
                            detail.CreatedAt = DateTime.UtcNow;
                            detail.UpdatedAt = DateTime.UtcNow;
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
                                existingDetail.ReceivedQuantity = detail.ReceivedQuantity;
                                existingDetail.UnitPrice = detail.UnitPrice;
                                existingDetail.InspectionRemarks = detail.InspectionRemarks;
                                existingDetail.BatchNumber = detail.BatchNumber;
                                existingDetail.ExpiryDate = detail.ExpiryDate;
                                existingDetail.WarehouseLocationId = detail.WarehouseLocationId;
                                existingDetail.IsReceivingCompleted = detail.IsReceivingCompleted;
                                existingDetail.UpdatedAt = DateTime.UtcNow;
                                detailsToUpdate.Add(existingDetail);
                            }
                        }
                    }

                    // 找出要刪除的明細（在現有明細中但不在新明細中）
                    var newDetailIds = details
                        .Where(d => d.Id > 0)
                        .Select(d => d.Id)
                        .ToList();
                    
                    detailsToDelete = existingDetails
                        .Where(ed => !newDetailIds.Contains(ed.Id))
                        .ToList();

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

                    // 儲存變更
                    await context.SaveChangesAsync();

                    // 更新採購單明細的已進貨數量
                    await UpdatePurchaseOrderDetailsReceivedQuantity(context, newDetailsToAdd, detailsToUpdate, detailsToDelete);

                    await transaction.CommitAsync();
                    return ServiceResult.Success();
                }
                catch (Exception)
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
                    PurchaseReceivingId = purchaseReceivingId 
                });
                return ServiceResult.Failure($"更新採購入庫明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新採購單明細的已進貨數量
        /// </summary>
        private async Task UpdatePurchaseOrderDetailsReceivedQuantity(
            AppDbContext context,
            List<PurchaseReceivingDetail> newDetailsToAdd,
            List<PurchaseReceivingDetail> detailsToUpdate,
            List<PurchaseReceivingDetail> detailsToDelete)
        {
            // 收集所有受影響的採購單明細ID
            var affectedPurchaseOrderDetailIds = new HashSet<int>();
            
            // 新增的明細
            foreach (var detail in newDetailsToAdd)
            {
                affectedPurchaseOrderDetailIds.Add(detail.PurchaseOrderDetailId);
            }
            
            // 更新的明細
            foreach (var detail in detailsToUpdate)
            {
                affectedPurchaseOrderDetailIds.Add(detail.PurchaseOrderDetailId);
            }
            
            // 刪除的明細
            foreach (var detail in detailsToDelete)
            {
                affectedPurchaseOrderDetailIds.Add(detail.PurchaseOrderDetailId);
            }

            // 重新計算每個採購單明細的已進貨數量
            foreach (var purchaseOrderDetailId in affectedPurchaseOrderDetailIds)
            {
                // 計算該採購單明細的總已進貨數量（所有有效的進貨明細）
                var totalReceivedQuantity = await context.PurchaseReceivingDetails
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId && !prd.IsDeleted)
                    .SumAsync(prd => prd.ReceivedQuantity);

                // 計算總已進貨金額
                var totalReceivedAmount = await context.PurchaseReceivingDetails
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId && !prd.IsDeleted)
                    .SumAsync(prd => prd.ReceivedQuantity * prd.UnitPrice);

                // 更新採購單明細
                var purchaseOrderDetail = await context.PurchaseOrderDetails
                    .FirstOrDefaultAsync(pod => pod.Id == purchaseOrderDetailId && !pod.IsDeleted);

                if (purchaseOrderDetail != null)
                {
                    purchaseOrderDetail.ReceivedQuantity = totalReceivedQuantity;
                    purchaseOrderDetail.ReceivedAmount = totalReceivedAmount;
                    purchaseOrderDetail.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 儲存採購單明細的變更
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 自動更新入庫狀態 - 根據進貨明細完成情況判定 (已移除狀態管理)
        /// </summary>
        private async Task UpdateReceivingStatusAsync(AppDbContext context, PurchaseReceiving purchaseReceiving, List<PurchaseReceivingDetail> details)
        {
            try
            {
                // 移除狀態設定，僅保留基本邏輯檢查
                if (!details.Any(d => d.ReceivedQuantity > 0))
                {
                    // 無進貨明細時的處理
                    return;
                }

                // 收集所有相關的採購單明細ID
                var purchaseOrderDetailIds = details
                    .Where(d => d.ReceivedQuantity > 0)
                    .Select(d => d.PurchaseOrderDetailId)
                    .Distinct()
                    .ToList();

                if (!purchaseOrderDetailIds.Any())
                {
                    // 無對應採購明細時的處理
                    return;
                }

                // 檢查對應的採購單明細是否全部完成進貨
                bool allCompleted = true;
                bool hasAnyReceived = false;

                // 根據入庫模式判定完成狀態
                if (purchaseReceiving.PurchaseOrderId.HasValue && purchaseReceiving.PurchaseOrderId.Value > 0)
                {
                    // 單一採購單模式 - 檢查整張採購單的所有明細
                    var allOrderDetails = await context.PurchaseOrderDetails
                        .Where(pod => pod.PurchaseOrderId == purchaseReceiving.PurchaseOrderId.Value && !pod.IsDeleted)
                        .ToListAsync();

                    foreach (var orderDetail in allOrderDetails)
                    {
                        // 計算該明細的總已進貨數量（包含其他入庫單的進貨）
                        var totalReceivedQuantity = await context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == orderDetail.Id && !prd.IsDeleted)
                            .SumAsync(prd => prd.ReceivedQuantity);

                        // 檢查是否有手動標記為完成的進貨記錄
                        var isManuallyCompleted = await context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == orderDetail.Id && !prd.IsDeleted)
                            .AnyAsync(prd => prd.IsReceivingCompleted);

                        if (totalReceivedQuantity > 0)
                        {
                            hasAnyReceived = true;
                        }

                        // 如果手動標記為完成，或數量已達到訂購量，則視為完成
                        if (!isManuallyCompleted && totalReceivedQuantity < orderDetail.OrderQuantity)
                        {
                            allCompleted = false;
                        }
                    }
                }
                else
                {
                    // 多採購單模式 - 只檢查本次進貨涉及的明細
                    foreach (var purchaseOrderDetailId in purchaseOrderDetailIds)
                    {
                        var orderDetail = await context.PurchaseOrderDetails
                            .FirstOrDefaultAsync(pod => pod.Id == purchaseOrderDetailId && !pod.IsDeleted);

                        if (orderDetail != null)
                        {
                            // 計算該明細的總已進貨數量（包含其他入庫單的進貨）
                            var totalReceivedQuantity = await context.PurchaseReceivingDetails
                                .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId && !prd.IsDeleted)
                                .SumAsync(prd => prd.ReceivedQuantity);

                            // 檢查是否有手動標記為完成的進貨記錄
                            var isManuallyCompleted = await context.PurchaseReceivingDetails
                                .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId && !prd.IsDeleted)
                                .AnyAsync(prd => prd.IsReceivingCompleted);

                            if (totalReceivedQuantity > 0)
                            {
                                hasAnyReceived = true;
                            }

                            // 如果手動標記為完成，或數量已達到訂購量，則視為完成
                            if (!isManuallyCompleted && totalReceivedQuantity < orderDetail.OrderQuantity)
                            {
                                allCompleted = false;
                            }
                        }
                    }
                }

                // 移除狀態設定，僅更新確認時間
                if (allCompleted && hasAnyReceived)
                {
                    purchaseReceiving.ConfirmedAt = DateTime.UtcNow;
                }

                // 更新最後修改時間
                purchaseReceiving.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // 如果狀態計算失敗，記錄錯誤但不中斷交易
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceivingStatusAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateReceivingStatusAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceiving.Id 
                });
            }
        }

        #endregion
    }
}