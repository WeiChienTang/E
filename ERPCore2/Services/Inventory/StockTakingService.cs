using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存盤點服務實作
    /// </summary>
    public class StockTakingService : GenericManagementService<StockTaking>, IStockTakingService
    {
        private readonly IInventoryStockService? _inventoryStockService;
        private readonly IInventoryTransactionService? _inventoryTransactionService;

        public StockTakingService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public StockTakingService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<StockTaking>> logger,
            IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        #region 基本查詢

        public async Task<List<StockTaking>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .Where(st => st.WarehouseId == warehouseId)
                    .OrderByDescending(st => st.TakingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new { 
                    WarehouseId = warehouseId 
                });
                return new List<StockTaking>();
            }
        }

        public async Task<List<StockTaking>> GetByStatusAsync(StockTakingStatusEnum status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .Where(st => st.TakingStatus == status)
                    .OrderByDescending(st => st.TakingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new { 
                    Status = status 
                });
                return new List<StockTaking>();
            }
        }

        public async Task<List<StockTaking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .Where(st => st.TakingDate >= startDate && st.TakingDate <= endDate)
                    .OrderByDescending(st => st.TakingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { 
                    StartDate = startDate, EndDate = endDate 
                });
                return new List<StockTaking>();
            }
        }

        public async Task<StockTaking?> GetByTakingNumberAsync(string takingNumber)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .FirstOrDefaultAsync(st => st.TakingNumber == takingNumber);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTakingNumberAsync), GetType(), _logger, new { 
                    TakingNumber = takingNumber 
                });
                return null;
            }
        }

        public async Task<StockTaking?> GetWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .Include(st => st.StockTakingDetails)
                        .ThenInclude(std => std.Product)
                    .Include(st => st.StockTakingDetails)
                        .ThenInclude(std => std.WarehouseLocation)
                    .FirstOrDefaultAsync(st => st.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new { 
                    Id = id 
                });
                return null;
            }
        }

        #endregion

        #region 盤點管理

        public async Task<ServiceResult> CreateStockTakingAsync(StockTaking stockTaking)
        {
            try
            {
                var validationResult = await ValidateStockTakingAsync(stockTaking);
                if (!validationResult.IsSuccess)
                    return validationResult;

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 產生盤點單號（如果沒有提供）
                if (string.IsNullOrEmpty(stockTaking.TakingNumber))
                {
                    stockTaking.TakingNumber = await GenerateTakingNumberAsync(context);
                }

                await context.StockTakings.AddAsync(stockTaking);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateStockTakingAsync), GetType(), _logger, new { 
                    StockTaking = stockTaking.TakingNumber 
                });
                return ServiceResult.Failure("建立盤點失敗");
            }
        }

        public async Task<ServiceResult> GenerateStockTakingListAsync(int warehouseId, int? warehouseLocationId = null, 
            StockTakingTypeEnum takingType = StockTakingTypeEnum.Full, List<int>? specificProductIds = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 建立盤點主檔
                    var stockTaking = new StockTaking
                    {
                        TakingNumber = await GenerateTakingNumberAsync(context),
                        TakingDate = DateTime.Now,
                        WarehouseId = warehouseId,
                        WarehouseLocationId = warehouseLocationId,
                        TakingType = takingType,
                        TakingStatus = StockTakingStatusEnum.Draft,
                        Status = EntityStatus.Active
                    };

                    await context.StockTakings.AddAsync(stockTaking);
                    await context.SaveChangesAsync();

                    // 產生盤點明細
                    var stockItems = await GetStockItemsForTakingAsync(context, warehouseId, warehouseLocationId, takingType, specificProductIds);
                    
                    var takingDetails = stockItems.Select(item => new StockTakingDetail
                    {
                        StockTakingId = stockTaking.Id,
                        ProductId = item.ProductId,
                        WarehouseLocationId = item.WarehouseLocationId,
                        SystemStock = item.CurrentStock,
                        UnitCost = item.AverageCost,
                        DetailStatus = StockTakingDetailStatusEnum.Pending,
                        Status = EntityStatus.Active
                    }).ToList();

                    await context.StockTakingDetails.AddRangeAsync(takingDetails);
                    
                    // 更新盤點主檔統計
                    stockTaking.TotalItems = takingDetails.Count;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateStockTakingListAsync), GetType(), _logger, new { 
                    WarehouseId = warehouseId, WarehouseLocationId = warehouseLocationId, TakingType = takingType 
                });
                return ServiceResult.Failure("產生盤點清單失敗");
            }
        }

        public async Task<ServiceResult> StartStockTakingAsync(int stockTakingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stockTaking = await context.StockTakings.FindAsync(stockTakingId);
                
                if (stockTaking == null)
                    return ServiceResult.Failure("找不到指定的盤點記錄");

                if (stockTaking.TakingStatus != StockTakingStatusEnum.Draft)
                    return ServiceResult.Failure("只有草稿狀態的盤點才能開始");

                stockTaking.TakingStatus = StockTakingStatusEnum.InProgress;
                stockTaking.StartTime = DateTime.Now;
                stockTaking.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(StartStockTakingAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return ServiceResult.Failure("開始盤點失敗");
            }
        }

        public async Task<ServiceResult> CompleteStockTakingAsync(int stockTakingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stockTaking = await context.StockTakings
                    .Include(st => st.StockTakingDetails)
                    .FirstOrDefaultAsync(st => st.Id == stockTakingId);
                
                if (stockTaking == null)
                    return ServiceResult.Failure("找不到指定的盤點記錄");

                if (stockTaking.TakingStatus != StockTakingStatusEnum.InProgress)
                    return ServiceResult.Failure("只有進行中的盤點才能完成");

                // 計算統計資料
                var completedItems = stockTaking.StockTakingDetails.Count(d => d.ActualStock.HasValue);
                var differenceItems = stockTaking.StockTakingDetails.Count(d => d.HasDifference);

                stockTaking.TakingStatus = StockTakingStatusEnum.Completed;
                stockTaking.EndTime = DateTime.Now;
                stockTaking.CompletedItems = completedItems;
                stockTaking.DifferenceItems = differenceItems;
                stockTaking.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteStockTakingAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return ServiceResult.Failure("完成盤點失敗");
            }
        }

        public async Task<ServiceResult> ApproveStockTakingAsync(int stockTakingId, int approvedBy, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stockTaking = await context.StockTakings.FindAsync(stockTakingId);
                
                if (stockTaking == null)
                    return ServiceResult.Failure("找不到指定的盤點記錄");

                if (stockTaking.TakingStatus != StockTakingStatusEnum.Completed && 
                    stockTaking.TakingStatus != StockTakingStatusEnum.PendingApproval)
                    return ServiceResult.Failure("只有已完成或待審核的盤點才能審核");

                stockTaking.TakingStatus = StockTakingStatusEnum.Approved;
                stockTaking.ApprovedBy = approvedBy;
                stockTaking.ApprovedAt = DateTime.Now;
                stockTaking.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(remarks))
                {
                    stockTaking.TakingRemarks = string.IsNullOrEmpty(stockTaking.TakingRemarks) 
                        ? $"審核備註：{remarks}" 
                        : $"{stockTaking.TakingRemarks}\n審核備註：{remarks}";
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ApproveStockTakingAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId, ApprovedBy = approvedBy 
                });
                return ServiceResult.Failure("審核盤點失敗");
            }
        }

        public async Task<ServiceResult> CancelStockTakingAsync(int stockTakingId, string? reason = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stockTaking = await context.StockTakings.FindAsync(stockTakingId);
                
                if (stockTaking == null)
                    return ServiceResult.Failure("找不到指定的盤點記錄");

                if (stockTaking.TakingStatus == StockTakingStatusEnum.Approved)
                    return ServiceResult.Failure("已審核的盤點無法取消");

                stockTaking.TakingStatus = StockTakingStatusEnum.Cancelled;
                stockTaking.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(reason))
                {
                    stockTaking.TakingRemarks = string.IsNullOrEmpty(stockTaking.TakingRemarks) 
                        ? $"取消原因：{reason}" 
                        : $"{stockTaking.TakingRemarks}\n取消原因：{reason}";
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelStockTakingAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return ServiceResult.Failure("取消盤點失敗");
            }
        }

        #endregion

        #region 盤點明細管理

        public async Task<List<StockTakingDetail>> GetStockTakingDetailsAsync(int stockTakingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakingDetails
                    .Include(std => std.Product)
                    .Include(std => std.WarehouseLocation)
                    .Where(std => std.StockTakingId == stockTakingId)
                    .OrderBy(std => std.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStockTakingDetailsAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return new List<StockTakingDetail>();
            }
        }

        public async Task<ServiceResult> UpdateStockTakingDetailAsync(int detailId, int actualStock, string? personnel = null, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.StockTakingDetails.FindAsync(detailId);
                
                if (detail == null)
                    return ServiceResult.Failure("找不到指定的盤點明細");

                detail.ActualStock = actualStock;
                detail.DetailStatus = StockTakingDetailStatusEnum.Counted;
                detail.TakingTime = DateTime.Now;
                detail.TakingPersonnel = personnel;
                detail.DetailRemarks = remarks;
                detail.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateStockTakingDetailAsync), GetType(), _logger, new { 
                    DetailId = detailId, ActualStock = actualStock 
                });
                return ServiceResult.Failure("更新盤點明細失敗");
            }
        }

        public async Task<ServiceResult> BatchUpdateStockTakingDetailsAsync(List<StockTakingDetailUpdateModel> updates)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                foreach (var update in updates)
                {
                    var detail = await context.StockTakingDetails.FindAsync(update.DetailId);
                    if (detail != null)
                    {
                        detail.ActualStock = update.ActualStock;
                        detail.DetailStatus = StockTakingDetailStatusEnum.Counted;
                        detail.TakingTime = DateTime.Now;
                        detail.TakingPersonnel = update.Personnel;
                        detail.DetailRemarks = update.Remarks;
                        detail.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateStockTakingDetailsAsync), GetType(), _logger, new { 
                    UpdateCount = updates.Count 
                });
                return ServiceResult.Failure("批次更新盤點明細失敗");
            }
        }

        #endregion

        #region 差異處理

        public async Task<List<StockTakingDetail>> GetDifferenceItemsAsync(int stockTakingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakingDetails
                    .Include(std => std.Product)
                    .Include(std => std.WarehouseLocation)
                    .Where(std => std.StockTakingId == stockTakingId && 
                                  std.ActualStock.HasValue && 
                                  std.ActualStock.Value != std.SystemStock)
                    .OrderBy(std => std.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDifferenceItemsAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return new List<StockTakingDetail>();
            }
        }

        public async Task<ServiceResult> GenerateAdjustmentTransactionsAsync(int stockTakingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var stockTaking = await context.StockTakings
                        .Include(st => st.StockTakingDetails)
                        .FirstOrDefaultAsync(st => st.Id == stockTakingId);

                    if (stockTaking == null)
                        return ServiceResult.Failure("找不到指定的盤點記錄");

                    if (stockTaking.TakingStatus != StockTakingStatusEnum.Approved)
                        return ServiceResult.Failure("只有已審核的盤點才能產生調整單");

                    if (stockTaking.IsAdjustmentGenerated)
                        return ServiceResult.Failure("此盤點已產生過調整單");

                    var differenceItems = stockTaking.StockTakingDetails
                        .Where(d => d.HasDifference && !d.IsAdjusted)
                        .ToList();

                    if (!differenceItems.Any())
                        return ServiceResult.Success();

                    int adjustedCount = 0;
                    foreach (var item in differenceItems)
                    {
                        if (item.DifferenceQuantity.HasValue && _inventoryTransactionService != null)
                        {
                            var adjustmentNumber = $"ADJ-{stockTaking.TakingNumber}-{DateTime.Now:yyyyMMddHHmmss}";
                            
                            var result = await _inventoryTransactionService.CreateAdjustmentTransactionAsync(
                                item.ProductId,
                                stockTaking.WarehouseId,
                                item.SystemStock,
                                item.ActualStock!.Value,
                                adjustmentNumber,
                                item.WarehouseLocationId,
                                $"盤點調整 - {stockTaking.TakingNumber}",
                                stockTaking.ApprovedBy
                            );

                            if (result.IsSuccess)
                            {
                                item.IsAdjusted = true;
                                item.AdjustmentNumber = adjustmentNumber;
                                adjustedCount++;
                            }
                        }
                    }

                    stockTaking.IsAdjustmentGenerated = true;
                    stockTaking.UpdatedAt = DateTime.Now;

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateAdjustmentTransactionsAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return ServiceResult.Failure("產生調整單失敗");
            }
        }

        public async Task<StockTakingReportModel> GenerateStockTakingReportAsync(int stockTakingId)
        {
            try
            {
                var stockTaking = await GetWithDetailsAsync(stockTakingId);
                if (stockTaking == null)
                    return new StockTakingReportModel();

                var allDetails = stockTaking.StockTakingDetails.ToList();
                var differenceItems = allDetails.Where(d => d.HasDifference).ToList();
                var countedItems = allDetails.Where(d => d.ActualStock.HasValue).ToList();

                var report = new StockTakingReportModel
                {
                    StockTaking = stockTaking,
                    AllDetails = allDetails,
                    DifferenceItems = differenceItems,
                    TotalDifferenceAmount = differenceItems.Sum(d => d.DifferenceAmount ?? 0),
                    TotalCountedItems = countedItems.Count,
                    TotalDifferenceItems = differenceItems.Count,
                    CompletionRate = stockTaking.CompletionRate,
                    AccuracyRate = countedItems.Count > 0 ? 
                        (decimal)(countedItems.Count - differenceItems.Count) / countedItems.Count * 100 : 0
                };

                return report;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateStockTakingReportAsync), GetType(), _logger, new { 
                    StockTakingId = stockTakingId 
                });
                return new StockTakingReportModel();
            }
        }

        #endregion

        #region 統計查詢

        public async Task<int> GetPendingStockTakingsCountAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings.CountAsync(st => 
                    (st.TakingStatus == StockTakingStatusEnum.Draft || 
                     st.TakingStatus == StockTakingStatusEnum.InProgress ||
                     st.TakingStatus == StockTakingStatusEnum.PendingApproval));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingStockTakingsCountAsync), GetType(), _logger);
                return 0;
            }
        }

        public async Task<List<StockTakingStatisticsModel>> GetStockTakingStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var stockTakings = await context.StockTakings
                    .Include(st => st.StockTakingDetails)
                    .Where(st => st.TakingDate >= startDate && st.TakingDate <= endDate)
                    .ToListAsync();

                var statistics = stockTakings
                    .GroupBy(st => st.TakingDate.Date)
                    .Select(g => new StockTakingStatisticsModel
                    {
                        Date = g.Key,
                        TotalStockTakings = g.Count(),
                        CompletedStockTakings = g.Count(st => st.TakingStatus == StockTakingStatusEnum.Completed || st.TakingStatus == StockTakingStatusEnum.Approved),
                        PendingStockTakings = g.Count(st => st.TakingStatus == StockTakingStatusEnum.Draft || st.TakingStatus == StockTakingStatusEnum.InProgress),
                        AverageCompletionRate = g.Average(st => st.CompletionRate),
                        TotalDifferenceAmount = g.SelectMany(st => st.StockTakingDetails).Sum(d => d.DifferenceAmount ?? 0)
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStockTakingStatisticsAsync), GetType(), _logger, new { 
                    StartDate = startDate, EndDate = endDate 
                });
                return new List<StockTakingStatisticsModel>();
            }
        }

        #endregion

        #region 驗證方法

        public async Task<ServiceResult> ValidateStockTakingAsync(StockTaking stockTaking)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(stockTaking.TakingNumber))
                    errors.Add("盤點單號不能為空");

                if (stockTaking.WarehouseId <= 0)
                    errors.Add("必須選擇倉庫");

                if (stockTaking.TakingDate == default)
                    errors.Add("盤點日期不能為空");

                // 檢查盤點單號是否重複
                if (!string.IsNullOrEmpty(stockTaking.TakingNumber) && 
                    !(await IsStockTakingNumberUniqueAsync(stockTaking.TakingNumber, stockTaking.Id == 0 ? null : stockTaking.Id)))
                {
                    errors.Add("盤點單號已存在");
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查倉庫是否存在
                var warehouseExists = await context.Warehouses.AnyAsync(w => w.Id == stockTaking.WarehouseId);
                if (!warehouseExists)
                    errors.Add("指定的倉庫不存在");

                // 檢查倉庫位置是否存在（如果有指定）
                if (stockTaking.WarehouseLocationId.HasValue)
                {
                    var locationExists = await context.WarehouseLocations.AnyAsync(wl => 
                        wl.Id == stockTaking.WarehouseLocationId.Value && 
                        wl.WarehouseId == stockTaking.WarehouseId);
                    if (!locationExists)
                        errors.Add("指定的倉庫位置不存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateStockTakingAsync), GetType(), _logger, new { 
                    TakingNumber = stockTaking.TakingNumber 
                });
                return ServiceResult.Failure("驗證盤點資料時發生錯誤");
            }
        }

        public async Task<bool> IsStockTakingNumberUniqueAsync(string takingNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.StockTakings.Where(st => st.TakingNumber == takingNumber);
                
                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);

                return !(await query.AnyAsync());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsStockTakingNumberUniqueAsync), GetType(), _logger, new { 
                    TakingNumber = takingNumber, ExcludeId = excludeId 
                });
                return false;
            }
        }

        #endregion

        #region 覆寫基底方法

        public override async Task<List<StockTaking>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .AsQueryable()
                    .OrderByDescending(st => st.TakingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<StockTaking>();
            }
        }

        public override async Task<List<StockTaking>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StockTakings
                    .Include(st => st.Warehouse)
                    .Include(st => st.WarehouseLocation)
                    .Include(st => st.ApprovedByUser)
                    .Where(st => (st.TakingNumber.Contains(searchTerm) ||
                                 st.Warehouse.Name.Contains(searchTerm) ||
                                 (st.TakingPersonnel != null && st.TakingPersonnel.Contains(searchTerm))))
                    .OrderByDescending(st => st.TakingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    SearchTerm = searchTerm 
                });
                return new List<StockTaking>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(StockTaking entity)
        {
            return await ValidateStockTakingAsync(entity);
        }

        #endregion

        #region 私有方法

        private async Task<string> GenerateTakingNumberAsync(AppDbContext context)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var lastNumber = await context.StockTakings
                .Where(st => st.TakingNumber.StartsWith($"ST{date}"))
                .OrderByDescending(st => st.TakingNumber)
                .Select(st => st.TakingNumber)
                .FirstOrDefaultAsync();

            if (lastNumber == null)
                return $"ST{date}001";

            var numberPart = lastNumber.Substring(10);
            if (int.TryParse(numberPart, out int number))
                return $"ST{date}{(number + 1):D3}";

            return $"ST{date}001";
        }

        private async Task<List<InventoryStock>> GetStockItemsForTakingAsync(AppDbContext context, int warehouseId, 
            int? warehouseLocationId, StockTakingTypeEnum takingType, List<int>? specificProductIds)
        {
            var query = context.InventoryStocks
                .Include(i => i.Product)
                .Where(i => i.WarehouseId == warehouseId);

            // 依盤點類型篩選
            switch (takingType)
            {
                case StockTakingTypeEnum.Location:
                    if (warehouseLocationId.HasValue)
                        query = query.Where(i => i.WarehouseLocationId == warehouseLocationId.Value);
                    break;

                case StockTakingTypeEnum.Specific:
                    if (specificProductIds != null && specificProductIds.Any())
                        query = query.Where(i => specificProductIds.Contains(i.ProductId));
                    break;

                case StockTakingTypeEnum.Cycle:
                    // 循環盤點可以加入特定邏輯，例如庫存週轉率高的商品
                    query = query.Where(i => i.CurrentStock > 0);
                    break;

                case StockTakingTypeEnum.Sample:
                    // 抽樣盤點，可以隨機選擇部分商品
                    query = query.OrderBy(i => Guid.NewGuid()).Take(100);
                    break;

                case StockTakingTypeEnum.Full:
                default:
                    // 全盤，不額外篩選
                    break;
            }

            return await query.ToListAsync();
        }

        #endregion
    }
}

