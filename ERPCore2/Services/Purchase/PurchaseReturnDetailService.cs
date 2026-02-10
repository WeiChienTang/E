using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購退回明細服務
    /// </summary>
    public class PurchaseReturnDetailService : GenericManagementService<PurchaseReturnDetail>, IPurchaseReturnDetailService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public PurchaseReturnDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
            _inventoryStockService = null;
        }

        public PurchaseReturnDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReturnDetail>> logger) : base(contextFactory, logger)
        {
            _inventoryStockService = null;
        }

        public PurchaseReturnDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReturnDetail>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        public override async Task<List<PurchaseReturnDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Include(prd => prd.WarehouseLocation)
                    .AsQueryable()
                    .OrderBy(prd => prd.PurchaseReturnId)
                    .ThenBy(prd => prd.Product.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public override async Task<PurchaseReturnDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Include(prd => prd.WarehouseLocation)
                    .FirstOrDefaultAsync(prd => prd.Id == id);
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

        public async Task<PurchaseReturnDetail?> GetWithNavigationAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                        .ThenInclude(pr => pr.Supplier)
                    .Include(prd => prd.Product)
                        .ThenInclude(p => p!.ProductCategory)
                    .Include(prd => prd.PurchaseOrderDetail)
                        .ThenInclude(pod => pod!.PurchaseOrder)
                    .Include(prd => prd.PurchaseReceivingDetail)
                        .ThenInclude(prd => prd!.PurchaseReceiving)
                    .Include(prd => prd.Unit)
                    .Include(prd => prd.WarehouseLocation)
                    .FirstOrDefaultAsync(prd => prd.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithNavigationAsync), GetType(), _logger, new { 
                    Method = nameof(GetWithNavigationAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return null;
            }
        }

        public async Task<List<PurchaseReturnDetail>> GetByPurchaseReturnIdAsync(int purchaseReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Include(prd => prd.WarehouseLocation)
                    .Include(prd => prd.PurchaseOrderDetail)
                    .Include(prd => prd.PurchaseReceivingDetail)
                    .Where(prd => prd.PurchaseReturnId == purchaseReturnId)
                    .OrderBy(prd => prd.Product.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReturnIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseReturnIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public async Task<List<PurchaseReturnDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                        .ThenInclude(pr => pr.Supplier)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Where(prd => prd.ProductId == productId)
                    .OrderByDescending(prd => prd.PurchaseReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public async Task<List<PurchaseReturnDetail>> GetByPurchaseOrderDetailIdAsync(int purchaseOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId)
                    .OrderByDescending(prd => prd.PurchaseReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderDetailIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderDetailId = purchaseOrderDetailId 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public async Task<List<PurchaseReturnDetail>> GetByPurchaseReceivingDetailIdAsync(int purchaseReceivingDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Where(prd => prd.PurchaseReceivingDetailId == purchaseReceivingDetailId)
                    .OrderByDescending(prd => prd.PurchaseReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReceivingDetailIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseReceivingDetailIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingDetailId = purchaseReceivingDetailId 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public override async Task<List<PurchaseReturnDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.Product)
                    .Include(prd => prd.Unit)
                    .Where(prd => (
                        (prd.Product.Name != null && prd.Product.Name.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.Product.Code != null && prd.Product.Code.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.PurchaseReturn.Code != null && prd.PurchaseReturn.Code.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.BatchNumber != null && prd.BatchNumber.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.Remarks != null && prd.Remarks.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderByDescending(prd => prd.PurchaseReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PurchaseReturnDetail entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (entity.PurchaseReturnId <= 0)
                    errors.Add("必須指定採購退回記錄");
                
                if (entity.ProductId <= 0)
                    errors.Add("必須選擇商品");
                
                if (entity.ReturnQuantity <= 0)
                    errors.Add("退回數量必須大於0");
                
                if (entity.OriginalUnitPrice < 0)
                    errors.Add("原始單價不能為負數");
                
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



        public async Task<ServiceResult> ValidateReturnQuantityAsync(int id, int returnQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseOrderDetail)
                    .Include(prd => prd.PurchaseReceivingDetail)
                    .FirstOrDefaultAsync(prd => prd.Id == id);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的退回明細");
                
                if (returnQuantity <= 0)
                    return ServiceResult.Failure("退回數量必須大於0");
                
                // 檢查是否超過原始可退回數量
                int maxReturnQuantity = 0;
                
                if (detail.PurchaseReceivingDetailId.HasValue && detail.PurchaseReceivingDetail != null)
                {
                    maxReturnQuantity = detail.PurchaseReceivingDetail.ReceivedQuantity;
                }
                else if (detail.PurchaseOrderDetailId.HasValue && detail.PurchaseOrderDetail != null)
                {
                    maxReturnQuantity = detail.PurchaseOrderDetail.ReceivedQuantity;
                }
                
                if (maxReturnQuantity > 0 && returnQuantity > maxReturnQuantity)
                    return ServiceResult.Failure($"退回數量不能超過原始數量 {maxReturnQuantity}");
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateReturnQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateReturnQuantityAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ReturnQuantity = returnQuantity 
                });
                return ServiceResult.Failure("驗證退回數量過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CalculateSubtotalAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails.FindAsync(id);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的退回明細");
                
                // 小計會由計算屬性自動計算，但這裡可以觸發相關業務邏輯
                detail.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateSubtotalAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateSubtotalAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("計算小計過程發生錯誤");
            }
        }



        public async Task<decimal> GetTotalReturnAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Where(prd => prd.ProductId == productId);
                
                if (startDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate <= endDate.Value);
                
                return await query.SumAsync(prd => prd.ReturnSubtotalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReturnAmountByProductAsync), GetType(), _logger, new { 
                    Method = nameof(GetTotalReturnAmountByProductAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return 0;
            }
        }

        public async Task<Dictionary<int, int>> GetReturnQuantityByProductAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .AsQueryable();
                
                if (startDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate <= endDate.Value);
                
                return await query
                    .GroupBy(prd => prd.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(prd => prd.ReturnQuantity));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnQuantityByProductAsync), GetType(), _logger, new { 
                    Method = nameof(GetReturnQuantityByProductAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return new Dictionary<int, int>();
            }
        }

        public async Task<List<PurchaseReturnDetail>> GetHighValueReturnsAsync(decimal minAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                        .ThenInclude(pr => pr.Supplier)
                    .Include(prd => prd.Product)
                    .Where(prd => prd.ReturnSubtotalAmount >= minAmount)
                    .OrderByDescending(prd => prd.ReturnSubtotalAmount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetHighValueReturnsAsync), GetType(), _logger, new { 
                    Method = nameof(GetHighValueReturnsAsync),
                    ServiceType = GetType().Name,
                    MinAmount = minAmount 
                });
                return new List<PurchaseReturnDetail>();
            }
        }

        /// <summary>
        /// 取得指定進貨明細的已退貨數量
        /// </summary>
        /// <param name="purchaseReceivingDetailId">進貨明細ID</param>
        /// <returns>已退貨數量</returns>
        public async Task<int> GetReturnedQuantityByReceivingDetailAsync(int purchaseReceivingDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Where(prd => prd.PurchaseReceivingDetailId == purchaseReceivingDetailId)
                    .SumAsync(prd => prd.ReturnQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnedQuantityByReceivingDetailAsync), GetType(), _logger, new { 
                    Method = nameof(GetReturnedQuantityByReceivingDetailAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingDetailId = purchaseReceivingDetailId 
                });
                return 0;
            }
        }

        /// <summary>
        /// 覆蓋永久刪除方法，加入庫存回滾邏輯
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                var dbSet = context.Set<PurchaseReturnDetail>();
                
                // 先取得要刪除的明細（包含相關資訊）
                var entity = await dbSet
                    .Include(prd => prd.PurchaseReturn)
                    .Include(prd => prd.PurchaseReceivingDetail)
                        .ThenInclude(prd => prd!.Warehouse)
                    .FirstOrDefaultAsync(x => x.Id == id);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到要刪除的退回明細");
                }

                // 檢查是否可以刪除
                var canDeleteResult = await CanDeleteAsync(entity);
                if (!canDeleteResult.IsSuccess)
                {
                    return canDeleteResult;
                }

                // 執行庫存回滾（撤銷退回 = 增加庫存）
                if (_inventoryStockService != null && entity.ReturnQuantity > 0)
                {
                    // 確定倉庫ID
                    var warehouseId = entity.PurchaseReceivingDetail?.WarehouseId;
                    if (warehouseId.HasValue)
                    {
                        var operationDescription = $"撤銷採購退回 - {entity.PurchaseReturn?.Code} (明細ID: {entity.Id})";
                        var stockResult = await _inventoryStockService.AddStockAsync(
                            entity.ProductId,
                            warehouseId.Value,
                            entity.ReturnQuantity,
                            InventoryTransactionTypeEnum.Return,
                            entity.PurchaseReturn?.Code ?? $"DELETED-{entity.Id}",
                            entity.OriginalUnitPrice,
                            entity.WarehouseLocationId,
                            operationDescription,
                            null, null, null, // batchNumber, batchDate, expiryDate
                            sourceDocumentType: InventorySourceDocumentTypes.PurchaseReturn,
                            sourceDocumentId: entity.PurchaseReturnId
                        );

                        if (!stockResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult.Failure($"庫存回滾失敗：{stockResult.ErrorMessage}");
                        }
                    }
                }

                // 執行實際刪除
                dbSet.Remove(entity);
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
                return ServiceResult.Failure($"永久刪除退回明細時發生錯誤: {ex.Message}");
            }
        }
    }
}

