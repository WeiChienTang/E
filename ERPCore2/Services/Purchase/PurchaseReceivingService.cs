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
        public PurchaseReceivingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReceivingService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReceiving>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基本方法

        public override async Task<List<PurchaseReceiving>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceivingDetails)
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
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.PurchaseOrderDetail)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
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
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
                    .Where(pr => !pr.IsDeleted && (
                        pr.ReceiptNumber.Contains(searchTerm) ||
                        pr.PurchaseOrder.PurchaseOrderNumber.Contains(searchTerm) ||
                        pr.PurchaseOrder.Supplier.CompanyName.Contains(searchTerm) ||
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

                // 檢查採購訂單是否存在且已核准
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

                // 檢查倉庫是否存在
                var warehouseExists = await context.Warehouses
                    .AnyAsync(w => w.Id == entity.WarehouseId && !w.IsDeleted);
                
                if (!warehouseExists)
                {
                    return ServiceResult.Failure("指定的倉庫不存在");
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

        public async Task<List<PurchaseReceiving>> GetByStatusAsync(PurchaseReceivingStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Where(pr => pr.ReceiptStatus == status && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReceiptDate)
                    .ThenBy(pr => pr.ReceiptNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new { 
                    Method = nameof(GetByStatusAsync),
                    ServiceType = GetType().Name,
                    Status = status 
                });
                return new List<PurchaseReceiving>();
            }
        }

        public async Task<List<PurchaseReceiving>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
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
                        .ThenInclude(po => po.Supplier)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.PurchaseReceivingDetails)
                        .ThenInclude(prd => prd.Product)
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

                    if (receipt.ReceiptStatus != PurchaseReceivingStatus.Draft)
                    {
                        return ServiceResult.Failure("只有草稿狀態的進貨單才能確認");
                    }

                    receipt.ReceiptStatus = PurchaseReceivingStatus.Confirmed;
                    receipt.ConfirmedAt = DateTime.Now;
                    receipt.ConfirmedBy = confirmedBy;
                    receipt.UpdatedAt = DateTime.Now;

                    // TODO: 在這裡實作庫存更新邏輯

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

                if (receipt.ReceiptStatus == PurchaseReceivingStatus.Received)
                {
                    return ServiceResult.Failure("已收貨的進貨單無法取消");
                }

                receipt.ReceiptStatus = PurchaseReceivingStatus.Cancelled;
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

        #endregion
    }
}