using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        public PurchaseReturnDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReturnDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseReturnDetail>> logger) : base(contextFactory, logger)
        {
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
                    .Where(prd => !prd.IsDeleted)
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
                    .FirstOrDefaultAsync(prd => prd.Id == id && !prd.IsDeleted);
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
                    .FirstOrDefaultAsync(prd => prd.Id == id && !prd.IsDeleted);
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
                    .Where(prd => prd.PurchaseReturnId == purchaseReturnId && !prd.IsDeleted)
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
                    .Where(prd => prd.ProductId == productId && !prd.IsDeleted)
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
                    .Where(prd => prd.PurchaseOrderDetailId == purchaseOrderDetailId && !prd.IsDeleted)
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
                    .Where(prd => prd.PurchaseReceivingDetailId == purchaseReceivingDetailId && !prd.IsDeleted)
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
                    .Where(prd => !prd.IsDeleted && (
                        (prd.Product.Name != null && prd.Product.Name.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.Product.Code != null && prd.Product.Code.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.PurchaseReturn.PurchaseReturnNumber != null && prd.PurchaseReturn.PurchaseReturnNumber.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.BatchNumber != null && prd.BatchNumber.ToLower().Contains(lowerSearchTerm)) ||
                        (prd.DetailRemarks != null && prd.DetailRemarks.ToLower().Contains(lowerSearchTerm))
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
                
                if (entity.ReturnUnitPrice < 0)
                    errors.Add("退回單價不能為負數");
                
                if (entity.ProcessedQuantity < 0)
                    errors.Add("已處理數量不能為負數");
                
                if (entity.ProcessedQuantity > entity.ReturnQuantity)
                    errors.Add("已處理數量不能超過退回數量");
                
                if (entity.ShippedQuantity < 0)
                    errors.Add("已出庫數量不能為負數");
                
                if (entity.ShippedQuantity > entity.ReturnQuantity)
                    errors.Add("已出庫數量不能超過退回數量");
                
                if (entity.ScrapQuantity < 0)
                    errors.Add("報廢數量不能為負數");
                
                if (entity.ScrapQuantity > entity.ReturnQuantity)
                    errors.Add("報廢數量不能超過退回數量");
                
                if ((entity.ShippedQuantity + entity.ScrapQuantity) > entity.ReturnQuantity)
                    errors.Add("出庫數量與報廢數量總和不能超過退回數量");
                
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

        public async Task<ServiceResult> ProcessShipmentAsync(int id, int shippedQuantity, DateTime? shippedDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .FirstOrDefaultAsync(prd => prd.Id == id && !prd.IsDeleted);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的退回明細");
                
                if (detail.PurchaseReturn.ReturnStatus != PurchaseReturnStatus.Processing)
                    return ServiceResult.Failure("只有處理中的退回單才能進行出庫作業");
                
                if (shippedQuantity <= 0)
                    return ServiceResult.Failure("出庫數量必須大於0");
                
                if (detail.ShippedQuantity + shippedQuantity > detail.ReturnQuantity)
                    return ServiceResult.Failure("出庫數量超過可退回數量");
                
                detail.ShippedQuantity += shippedQuantity;
                detail.IsShipped = detail.ShippedQuantity >= detail.ReturnQuantity;
                detail.ShippedDate = shippedDate ?? DateTime.UtcNow;
                detail.ProcessedQuantity = Math.Max(detail.ProcessedQuantity, detail.ShippedQuantity + detail.ScrapQuantity);
                detail.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ProcessShipmentAsync), GetType(), _logger, new { 
                    Method = nameof(ProcessShipmentAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ShippedQuantity = shippedQuantity,
                    ShippedDate = shippedDate 
                });
                return ServiceResult.Failure("出庫處理過程發生錯誤");
            }
        }

        public async Task<ServiceResult> ProcessScrapAsync(int id, int scrapQuantity, string? reason = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .FirstOrDefaultAsync(prd => prd.Id == id && !prd.IsDeleted);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的退回明細");
                
                if (detail.PurchaseReturn.ReturnStatus != PurchaseReturnStatus.Processing)
                    return ServiceResult.Failure("只有處理中的退回單才能進行報廢作業");
                
                if (scrapQuantity <= 0)
                    return ServiceResult.Failure("報廢數量必須大於0");
                
                if (detail.ScrapQuantity + scrapQuantity > detail.ReturnQuantity)
                    return ServiceResult.Failure("報廢數量超過可退回數量");
                
                if (detail.ShippedQuantity + detail.ScrapQuantity + scrapQuantity > detail.ReturnQuantity)
                    return ServiceResult.Failure("出庫與報廢數量總和超過退回數量");
                
                detail.ScrapQuantity += scrapQuantity;
                detail.ProcessedQuantity = Math.Max(detail.ProcessedQuantity, detail.ShippedQuantity + detail.ScrapQuantity);
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    detail.DetailRemarks = string.IsNullOrWhiteSpace(detail.DetailRemarks) 
                        ? $"報廢原因: {reason}" 
                        : $"{detail.DetailRemarks}; 報廢原因: {reason}";
                }
                detail.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ProcessScrapAsync), GetType(), _logger, new { 
                    Method = nameof(ProcessScrapAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ScrapQuantity = scrapQuantity,
                    Reason = reason 
                });
                return ServiceResult.Failure("報廢處理過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateProcessedQuantityAsync(int id, int processedQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails.FindAsync(id);
                
                if (detail == null || detail.IsDeleted)
                    return ServiceResult.Failure("找不到指定的退回明細");
                
                if (processedQuantity < 0)
                    return ServiceResult.Failure("已處理數量不能為負數");
                
                if (processedQuantity > detail.ReturnQuantity)
                    return ServiceResult.Failure("已處理數量不能超過退回數量");
                
                detail.ProcessedQuantity = processedQuantity;
                detail.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateProcessedQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateProcessedQuantityAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ProcessedQuantity = processedQuantity 
                });
                return ServiceResult.Failure("更新已處理數量過程發生錯誤");
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
                    .FirstOrDefaultAsync(prd => prd.Id == id && !prd.IsDeleted);
                
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
                
                if (detail == null || detail.IsDeleted)
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

        public async Task<ServiceResult> ProcessBatchShipmentAsync(List<int> detailIds, DateTime? shippedDate = null)
        {
            try
            {
                var results = new List<string>();
                var successCount = 0;
                
                foreach (var id in detailIds)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var detail = await context.PurchaseReturnDetails.FindAsync(id);
                    
                    if (detail == null || detail.IsDeleted)
                    {
                        results.Add($"明細 {id}: 找不到記錄");
                        continue;
                    }
                    
                    var pendingQuantity = detail.ReturnQuantity - detail.ShippedQuantity - detail.ScrapQuantity;
                    if (pendingQuantity > 0)
                    {
                        var result = await ProcessShipmentAsync(id, pendingQuantity, shippedDate);
                        if (result.IsSuccess)
                        {
                            successCount++;
                            results.Add($"明細 {id}: 出庫成功");
                        }
                        else
                        {
                            results.Add($"明細 {id}: {result.ErrorMessage}");
                        }
                    }
                    else
                    {
                        results.Add($"明細 {id}: 無待處理數量");
                    }
                }
                
                var message = $"批次出庫完成，成功 {successCount}/{detailIds.Count} 筆";
                if (results.Any())
                    message += $"\n詳細結果: {string.Join("; ", results)}";
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ProcessBatchShipmentAsync), GetType(), _logger, new { 
                    Method = nameof(ProcessBatchShipmentAsync),
                    ServiceType = GetType().Name,
                    DetailIds = detailIds,
                    ShippedDate = shippedDate 
                });
                return ServiceResult.Failure("批次出庫處理過程發生錯誤");
            }
        }

        public async Task<ServiceResult> ProcessBatchScrapAsync(List<int> detailIds, string? reason = null)
        {
            try
            {
                var results = new List<string>();
                var successCount = 0;
                
                foreach (var id in detailIds)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var detail = await context.PurchaseReturnDetails.FindAsync(id);
                    
                    if (detail == null || detail.IsDeleted)
                    {
                        results.Add($"明細 {id}: 找不到記錄");
                        continue;
                    }
                    
                    var pendingQuantity = detail.ReturnQuantity - detail.ShippedQuantity - detail.ScrapQuantity;
                    if (pendingQuantity > 0)
                    {
                        var result = await ProcessScrapAsync(id, pendingQuantity, reason);
                        if (result.IsSuccess)
                        {
                            successCount++;
                            results.Add($"明細 {id}: 報廢成功");
                        }
                        else
                        {
                            results.Add($"明細 {id}: {result.ErrorMessage}");
                        }
                    }
                    else
                    {
                        results.Add($"明細 {id}: 無待處理數量");
                    }
                }
                
                var message = $"批次報廢完成，成功 {successCount}/{detailIds.Count} 筆";
                if (results.Any())
                    message += $"\n詳細結果: {string.Join("; ", results)}";
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ProcessBatchScrapAsync), GetType(), _logger, new { 
                    Method = nameof(ProcessBatchScrapAsync),
                    ServiceType = GetType().Name,
                    DetailIds = detailIds,
                    Reason = reason 
                });
                return ServiceResult.Failure("批次報廢處理過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateBatchProcessedQuantityAsync(Dictionary<int, int> detailQuantities)
        {
            try
            {
                var results = new List<string>();
                var successCount = 0;
                
                foreach (var kvp in detailQuantities)
                {
                    var result = await UpdateProcessedQuantityAsync(kvp.Key, kvp.Value);
                    if (result.IsSuccess)
                    {
                        successCount++;
                        results.Add($"明細 {kvp.Key}: 更新成功");
                    }
                    else
                    {
                        results.Add($"明細 {kvp.Key}: {result.ErrorMessage}");
                    }
                }
                
                var message = $"批次更新完成，成功 {successCount}/{detailQuantities.Count} 筆";
                if (results.Any())
                    message += $"\n詳細結果: {string.Join("; ", results)}";
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateBatchProcessedQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateBatchProcessedQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailQuantities = detailQuantities 
                });
                return ServiceResult.Failure("批次更新處理數量過程發生錯誤");
            }
        }

        public async Task<decimal> GetTotalReturnAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                    .Where(prd => prd.ProductId == productId && !prd.IsDeleted);
                
                if (startDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(prd => prd.PurchaseReturn.ReturnDate <= endDate.Value);
                
                return await query.SumAsync(prd => prd.ReturnSubtotal);
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
                    .Where(prd => !prd.IsDeleted);
                
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

        public async Task<List<PurchaseReturnDetail>> GetPendingShipmentDetailsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnDetails
                    .Include(prd => prd.PurchaseReturn)
                        .ThenInclude(pr => pr.Supplier)
                    .Include(prd => prd.Product)
                    .Where(prd => !prd.IsDeleted &&
                                 prd.PurchaseReturn.ReturnStatus == PurchaseReturnStatus.Processing &&
                                 !prd.IsShipped &&
                                 prd.ShippedQuantity < prd.ReturnQuantity)
                    .OrderBy(prd => prd.PurchaseReturn.ExpectedProcessDate)
                    .ThenBy(prd => prd.PurchaseReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingShipmentDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingShipmentDetailsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReturnDetail>();
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
                    .Where(prd => !prd.IsDeleted && prd.ReturnSubtotal >= minAmount)
                    .OrderByDescending(prd => prd.ReturnSubtotal)
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
    }
}
