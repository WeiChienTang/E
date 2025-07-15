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
    public class PurchaseReceiptService : GenericManagementService<PurchaseReceipt>, IPurchaseReceiptService
    {
        private readonly IInventoryStockService? _inventoryStockService;
        private readonly IPurchaseOrderService? _purchaseOrderService;

        public PurchaseReceiptService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReceiptService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceipt>> logger,
            IInventoryStockService inventoryStockService,
            IPurchaseOrderService purchaseOrderService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _purchaseOrderService = purchaseOrderService;
        }

        #region 覆寫基本方法

        public override async Task<List<PurchaseReceipt>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.Product)
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
                return new List<PurchaseReceipt>();
            }
        }

        public override async Task<PurchaseReceipt?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);
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

        public override async Task<List<PurchaseReceipt>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Where(pr => !pr.IsDeleted && 
                               (pr.ReceiptNumber.Contains(searchTerm) ||
                                pr.PurchaseOrder.PurchaseOrderNumber.Contains(searchTerm) ||
                                pr.PurchaseOrder.Supplier.CompanyName.Contains(searchTerm) ||
                                pr.InspectionPersonnel != null && pr.InspectionPersonnel.Contains(searchTerm) ||
                                pr.ReceiptRemarks != null && pr.ReceiptRemarks.Contains(searchTerm)))
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PurchaseReceipt>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PurchaseReceipt entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.ReceiptNumber))
                    errors.Add("進貨單號不能為空");
                
                if (entity.PurchaseOrderId <= 0)
                    errors.Add("必須選擇採購訂單");
                
                if (entity.WarehouseId <= 0)
                    errors.Add("必須選擇倉庫");
                
                if (entity.ReceiptDate > DateTime.Today.AddDays(1))
                    errors.Add("進貨日期不能超過明天");
                
                // 檢查進貨單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.ReceiptNumber))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var exists = await context.PurchaseReceipts
                        .AnyAsync(pr => pr.ReceiptNumber == entity.ReceiptNumber && 
                                       pr.Id != entity.Id && !pr.IsDeleted);
                    if (exists)
                        errors.Add("進貨單號已存在");
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
                    ReceiptNumber = entity.ReceiptNumber 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 基本查詢

        public async Task<List<PurchaseReceipt>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.Product)
                    .Where(pr => pr.PurchaseOrderId == purchaseOrderId && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseOrderIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
                });
                return new List<PurchaseReceipt>();
            }
        }

        public async Task<List<PurchaseReceipt>> GetByStatusAsync(PurchaseReceiptStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Where(pr => pr.ReceiptStatus == status && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new { 
                    Method = nameof(GetByStatusAsync),
                    ServiceType = GetType().Name,
                    Status = status 
                });
                return new List<PurchaseReceipt>();
            }
        }

        public async Task<List<PurchaseReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Where(pr => pr.ReceiptDate >= startDate && 
                               pr.ReceiptDate <= endDate && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
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
                return new List<PurchaseReceipt>();
            }
        }

        public async Task<PurchaseReceipt?> GetByNumberAsync(string receiptNumber)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.Product)
                    .FirstOrDefaultAsync(pr => pr.ReceiptNumber == receiptNumber && !pr.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GetByNumberAsync),
                    ServiceType = GetType().Name,
                    ReceiptNumber = receiptNumber 
                });
                return null;
            }
        }

        #endregion

        #region 進貨操作

        public async Task<ServiceResult> ConfirmReceiptAsync(int receiptId, int confirmedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var receipt = await context.PurchaseReceipts
                        .Include(pr => pr.PurchaseReceiptDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == receiptId && !pr.IsDeleted);
                    
                    if (receipt == null)
                        return ServiceResult.Failure("找不到進貨單");
                    
                    if (receipt.ReceiptStatus != PurchaseReceiptStatus.Draft)
                        return ServiceResult.Failure("只有草稿狀態的進貨單可以確認");
                    
                    if (!receipt.PurchaseReceiptDetails.Any())
                        return ServiceResult.Failure("進貨單必須包含至少一項商品");
                    
                    receipt.ReceiptStatus = PurchaseReceiptStatus.Confirmed;
                    receipt.ConfirmedBy = confirmedBy;
                    receipt.ConfirmedAt = DateTime.Now;
                    receipt.UpdatedAt = DateTime.Now;
                    
                    await context.SaveChangesAsync();
                    
                    // 處理庫存更新
                    var inventoryResult = await UpdateInventoryStockAsync(receiptId);
                    if (!inventoryResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return inventoryResult;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmReceiptAsync), GetType(), _logger, new { 
                    Method = nameof(ConfirmReceiptAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId,
                    ConfirmedBy = confirmedBy 
                });
                return ServiceResult.Failure("確認進貨單時發生錯誤");
            }
        }

        public async Task<ServiceResult> CancelReceiptAsync(int receiptId, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var receipt = await context.PurchaseReceipts
                    .FirstOrDefaultAsync(pr => pr.Id == receiptId && !pr.IsDeleted);
                
                if (receipt == null)
                    return ServiceResult.Failure("找不到進貨單");
                
                if (receipt.ReceiptStatus == PurchaseReceiptStatus.Received)
                    return ServiceResult.Failure("已入庫的進貨單無法取消");
                
                receipt.ReceiptStatus = PurchaseReceiptStatus.Cancelled;
                receipt.ReceiptRemarks = string.IsNullOrWhiteSpace(receipt.ReceiptRemarks) 
                    ? $"取消原因：{reason}" 
                    : $"{receipt.ReceiptRemarks}\n取消原因：{reason}";
                receipt.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelReceiptAsync), GetType(), _logger, new { 
                    Method = nameof(CancelReceiptAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId,
                    Reason = reason 
                });
                return ServiceResult.Failure("取消進貨單時發生錯誤");
            }
        }

        public async Task<ServiceResult> ProcessInventoryAsync(int receiptId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var receipt = await context.PurchaseReceipts
                        .Include(pr => pr.PurchaseReceiptDetails)
                            .ThenInclude(prd => prd.Product)
                        .FirstOrDefaultAsync(pr => pr.Id == receiptId && !pr.IsDeleted);
                    
                    if (receipt == null)
                        return ServiceResult.Failure("找不到進貨單");
                    
                    if (receipt.ReceiptStatus != PurchaseReceiptStatus.Confirmed)
                        return ServiceResult.Failure("只有已確認的進貨單可以入庫");
                    
                    // 處理每個明細的庫存入庫
                    foreach (var detail in receipt.PurchaseReceiptDetails.Where(d => !d.IsDeleted))
                    {
                        if (_inventoryStockService != null)
                        {
                            var addStockResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                receipt.WarehouseId,
                                detail.ReceivedQuantity,
                                InventoryTransactionTypeEnum.Purchase,
                                receipt.ReceiptNumber,
                                detail.UnitPrice,
                                detail.WarehouseLocationId,
                                $"採購進貨入庫 - {receipt.ReceiptNumber}"
                            );
                            
                            if (!addStockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"商品 {detail.Product.ProductCode} 入庫失敗：{addStockResult.ErrorMessage}");
                            }
                        }
                    }
                    
                    receipt.ReceiptStatus = PurchaseReceiptStatus.Received;
                    receipt.UpdatedAt = DateTime.Now;
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ProcessInventoryAsync), GetType(), _logger, new { 
                    Method = nameof(ProcessInventoryAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId 
                });
                return ServiceResult.Failure("處理庫存入庫時發生錯誤");
            }
        }

        #endregion

        #region 明細相關

        public async Task<List<PurchaseReceiptDetail>> GetReceiptDetailsAsync(int receiptId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceiptDetails
                    .Include(prd => prd.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(prd => prd.PurchaseOrderDetail)
                    .Include(prd => prd.WarehouseLocation)
                    .Where(prd => prd.PurchaseReceiptId == receiptId && !prd.IsDeleted)
                    .OrderBy(prd => prd.Product.ProductCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceiptDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetReceiptDetailsAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId 
                });
                return new List<PurchaseReceiptDetail>();
            }
        }

        public async Task<ServiceResult> AddReceiptDetailAsync(PurchaseReceiptDetail detail)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    detail.Status = EntityStatus.Active;
                    detail.CreatedAt = DateTime.Now;
                    
                    await context.PurchaseReceiptDetails.AddAsync(detail);
                    await context.SaveChangesAsync();
                    
                    // 更新進貨單總金額
                    var receipt = await context.PurchaseReceipts
                        .Include(pr => pr.PurchaseReceiptDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == detail.PurchaseReceiptId);
                    
                    if (receipt != null)
                    {
                        receipt.TotalAmount = receipt.PurchaseReceiptDetails
                            .Where(prd => !prd.IsDeleted)
                            .Sum(prd => prd.SubtotalAmount);
                        receipt.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
                    // 更新採購訂單明細的已進貨數量
                    var orderDetail = await context.PurchaseOrderDetails
                        .FirstOrDefaultAsync(pod => pod.Id == detail.PurchaseOrderDetailId);
                    
                    if (orderDetail != null)
                    {
                        var totalReceived = await context.PurchaseReceiptDetails
                            .Where(prd => prd.PurchaseOrderDetailId == detail.PurchaseOrderDetailId && !prd.IsDeleted)
                            .SumAsync(prd => prd.ReceivedQuantity);
                        
                        orderDetail.ReceivedQuantity = totalReceived;
                        orderDetail.ReceivedAmount = totalReceived * orderDetail.UnitPrice;
                        orderDetail.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddReceiptDetailAsync), GetType(), _logger, new { 
                    Method = nameof(AddReceiptDetailAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = detail.PurchaseReceiptId,
                    ProductId = detail.ProductId 
                });
                return ServiceResult.Failure("新增進貨明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateReceiptDetailAsync(PurchaseReceiptDetail detail)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var existing = await context.PurchaseReceiptDetails
                        .FirstOrDefaultAsync(prd => prd.Id == detail.Id && !prd.IsDeleted);
                    
                    if (existing == null)
                        return ServiceResult.Failure("找不到進貨明細");
                    
                    existing.ReceivedQuantity = detail.ReceivedQuantity;
                    existing.UnitPrice = detail.UnitPrice;
                    existing.WarehouseLocationId = detail.WarehouseLocationId;
                    existing.InspectionRemarks = detail.InspectionRemarks;
                    existing.QualityInspectionPassed = detail.QualityInspectionPassed;
                    existing.QualityRemarks = detail.QualityRemarks;
                    existing.BatchNumber = detail.BatchNumber;
                    existing.ExpiryDate = detail.ExpiryDate;
                    existing.UpdatedAt = DateTime.Now;
                    
                    await context.SaveChangesAsync();
                    
                    // 更新進貨單總金額
                    var receipt = await context.PurchaseReceipts
                        .Include(pr => pr.PurchaseReceiptDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == existing.PurchaseReceiptId);
                    
                    if (receipt != null)
                    {
                        receipt.TotalAmount = receipt.PurchaseReceiptDetails
                            .Where(prd => !prd.IsDeleted)
                            .Sum(prd => prd.SubtotalAmount);
                        receipt.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
                    // 更新採購訂單明細的已進貨數量
                    var orderDetail = await context.PurchaseOrderDetails
                        .FirstOrDefaultAsync(pod => pod.Id == existing.PurchaseOrderDetailId);
                    
                    if (orderDetail != null)
                    {
                        var totalReceived = await context.PurchaseReceiptDetails
                            .Where(prd => prd.PurchaseOrderDetailId == existing.PurchaseOrderDetailId && !prd.IsDeleted)
                            .SumAsync(prd => prd.ReceivedQuantity);
                        
                        orderDetail.ReceivedQuantity = totalReceived;
                        orderDetail.ReceivedAmount = totalReceived * orderDetail.UnitPrice;
                        orderDetail.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceiptDetailAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateReceiptDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id 
                });
                return ServiceResult.Failure("更新進貨明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteReceiptDetailAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var detail = await context.PurchaseReceiptDetails
                        .FirstOrDefaultAsync(prd => prd.Id == detailId && !prd.IsDeleted);
                    
                    if (detail == null)
                        return ServiceResult.Failure("找不到進貨明細");
                    
                    detail.IsDeleted = true;
                    detail.UpdatedAt = DateTime.Now;
                    
                    await context.SaveChangesAsync();
                    
                    // 更新進貨單總金額
                    var receipt = await context.PurchaseReceipts
                        .Include(pr => pr.PurchaseReceiptDetails)
                        .FirstOrDefaultAsync(pr => pr.Id == detail.PurchaseReceiptId);
                    
                    if (receipt != null)
                    {
                        receipt.TotalAmount = receipt.PurchaseReceiptDetails
                            .Where(prd => !prd.IsDeleted)
                            .Sum(prd => prd.SubtotalAmount);
                        receipt.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
                    // 更新採購訂單明細的已進貨數量
                    var orderDetail = await context.PurchaseOrderDetails
                        .FirstOrDefaultAsync(pod => pod.Id == detail.PurchaseOrderDetailId);
                    
                    if (orderDetail != null)
                    {
                        var totalReceived = await context.PurchaseReceiptDetails
                            .Where(prd => prd.PurchaseOrderDetailId == detail.PurchaseOrderDetailId && !prd.IsDeleted)
                            .SumAsync(prd => prd.ReceivedQuantity);
                        
                        orderDetail.ReceivedQuantity = totalReceived;
                        orderDetail.ReceivedAmount = totalReceived * orderDetail.UnitPrice;
                        orderDetail.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteReceiptDetailAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteReceiptDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId 
                });
                return ServiceResult.Failure("刪除進貨明細時發生錯誤");
            }
        }

        #endregion

        #region 自動產生編號

        public async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var prefix = $"PR{today:yyyyMMdd}";
                
                var lastReceipt = await context.PurchaseReceipts
                    .Where(pr => pr.ReceiptNumber.StartsWith(prefix))
                    .OrderByDescending(pr => pr.ReceiptNumber)
                    .FirstOrDefaultAsync();
                
                if (lastReceipt == null)
                {
                    return $"{prefix}001";
                }
                
                var lastNumber = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out var number))
                {
                    return $"{prefix}{(number + 1):D3}";
                }
                
                return $"{prefix}001";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateReceiptNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateReceiptNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"PR{DateTime.Today:yyyyMMdd}001";
            }
        }

        #endregion

        #region 庫存相關

        public async Task<ServiceResult> UpdateInventoryStockAsync(int receiptId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var receipt = await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseReceiptDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                    .FirstOrDefaultAsync(pr => pr.Id == receiptId && !pr.IsDeleted);
                
                if (receipt == null)
                    return ServiceResult.Failure("找不到進貨單");
                
                // 更新採購訂單明細的已進貨數量和金額
                foreach (var detail in receipt.PurchaseReceiptDetails.Where(d => !d.IsDeleted))
                {
                    var orderDetail = detail.PurchaseOrderDetail;
                    if (orderDetail != null)
                    {
                        var totalReceived = await context.PurchaseReceiptDetails
                            .Where(prd => prd.PurchaseOrderDetailId == orderDetail.Id && !prd.IsDeleted)
                            .SumAsync(prd => prd.ReceivedQuantity);
                        
                        orderDetail.ReceivedQuantity = totalReceived;
                        orderDetail.ReceivedAmount = totalReceived * orderDetail.UnitPrice;
                        orderDetail.UpdatedAt = DateTime.Now;
                    }
                }
                
                await context.SaveChangesAsync();
                
                // 更新採購訂單的已進貨金額和狀態
                if (_purchaseOrderService != null)
                {
                    await _purchaseOrderService.UpdateReceivedAmountAsync(receipt.PurchaseOrderId);
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInventoryStockAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateInventoryStockAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId 
                });
                return ServiceResult.Failure("更新庫存時發生錯誤");
            }
        }

        public async Task<bool> CanDeleteAsync(int receiptId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var receipt = await context.PurchaseReceipts
                    .FirstOrDefaultAsync(pr => pr.Id == receiptId && !pr.IsDeleted);
                
                if (receipt == null) return false;
                
                // 已入庫的進貨單不能刪除
                if (receipt.ReceiptStatus == PurchaseReceiptStatus.Received)
                    return false;
                
                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    ReceiptId = receiptId 
                });
                return false;
            }
        }

        #endregion

        #region 統計查詢

        public async Task<List<PurchaseReceipt>> GetPendingReceiptsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Where(pr => !pr.IsDeleted && 
                               (pr.ReceiptStatus == PurchaseReceiptStatus.Draft || 
                                pr.ReceiptStatus == PurchaseReceiptStatus.Confirmed))
                    .OrderBy(pr => pr.ReceiptDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingReceiptsAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingReceiptsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReceipt>();
            }
        }

        public async Task<decimal> GetTotalReceiptAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReceipts
                    .Include(pr => pr.PurchaseOrder)
                    .Where(pr => pr.PurchaseOrder.SupplierId == supplierId && 
                               pr.ReceiptStatus == PurchaseReceiptStatus.Received && !pr.IsDeleted);
                
                if (startDate.HasValue)
                    query = query.Where(pr => pr.ReceiptDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(pr => pr.ReceiptDate <= endDate.Value);
                
                return await query.SumAsync(pr => pr.TotalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReceiptAmountAsync), GetType(), _logger, new { 
                    Method = nameof(GetTotalReceiptAmountAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return 0;
            }
        }

        #endregion
    }
}
