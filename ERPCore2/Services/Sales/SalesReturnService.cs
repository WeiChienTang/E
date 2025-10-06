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
    /// 銷貨退回服務實作
    /// </summary>
    public class SalesReturnService : GenericManagementService<SalesReturn>, ISalesReturnService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public SalesReturnService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public SalesReturnService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesReturn>> logger) : base(contextFactory, logger)
        {
        }

        public SalesReturnService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesReturn>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        public override async Task<List<SalesReturn>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .AsQueryable()
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesReturn>();
            }
        }

        public override async Task<SalesReturn?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.SalesOrderDetail)
                    .FirstOrDefaultAsync(sr => sr.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<SalesReturn>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => (sr.SalesReturnNumber.ToLower().Contains(lowerSearchTerm) ||
                         sr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesReturn>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesReturn entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.SalesReturnNumber))
                    errors.Add("退回單號不能為空");

                if (entity.ReturnDate == default)
                    errors.Add("退回日期不能為空");

                if (entity.ReturnDate > DateTime.Today)
                    errors.Add("退回日期不能大於今天");

                if (entity.CustomerId <= 0)
                    errors.Add("必須選擇客戶");

                if (!string.IsNullOrWhiteSpace(entity.SalesReturnNumber) &&
                    await IsSalesReturnNumberExistsAsync(entity.SalesReturnNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("退回單號已存在");

                if (entity.TotalReturnAmount < 0)
                    errors.Add("退回總金額不能為負數");

                if (entity.ReturnTaxAmount < 0)
                    errors.Add("退回稅額不能為負數");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityNumber = entity.SalesReturnNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<bool> IsSalesReturnNumberExistsAsync(string salesReturnNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesReturns.Where(sr => sr.SalesReturnNumber == salesReturnNumber);
                if (excludeId.HasValue)
                    query = query.Where(sr => sr.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesReturnNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSalesReturnNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SalesReturnNumber = salesReturnNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<SalesReturn>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.CustomerId == customerId)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<SalesReturn>();
            }
        }

        public async Task<List<SalesReturn>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.SalesOrderId == salesOrderId)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesOrderIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId
                });
                return new List<SalesReturn>();
            }
        }



        public async Task<List<SalesReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.ReturnDate >= startDate && sr.ReturnDate <= endDate)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<SalesReturn>();
            }
        }

        public async Task<decimal> CalculateTotalReturnAmountAsync(int salesReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Where(srd => srd.SalesReturnId == salesReturnId)
                    .SumAsync(srd => srd.ReturnSubtotalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalReturnAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(CalculateTotalReturnAmountAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId
                });
                return 0;
            }
        }

        public async Task<string> GenerateSalesReturnNumberAsync(DateTime returnDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var prefix = $"SR{returnDate:yyyyMM}";
                
                var lastNumber = await context.SalesReturns
                    .Where(sr => sr.SalesReturnNumber.StartsWith(prefix))
                    .OrderByDescending(sr => sr.SalesReturnNumber)
                    .Select(sr => sr.SalesReturnNumber)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(lastNumber))
                    return $"{prefix}001";

                var lastSequence = lastNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out var sequence))
                    return $"{prefix}{(sequence + 1):D3}";

                return $"{prefix}001";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateSalesReturnNumberAsync), GetType(), _logger, new
                {
                    Method = nameof(GenerateSalesReturnNumberAsync),
                    ServiceType = GetType().Name,
                    ReturnDate = returnDate
                });
                return $"SR{returnDate:yyyyMM}001";
            }
        }

        public async Task<SalesReturnStatistics> GetStatisticsAsync(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesReturns.AsQueryable();

                if (customerId.HasValue)
                    query = query.Where(sr => sr.CustomerId == customerId.Value);

                if (startDate.HasValue)
                    query = query.Where(sr => sr.ReturnDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(sr => sr.ReturnDate <= endDate.Value);

                var returns = await query.ToListAsync();

                var statistics = new SalesReturnStatistics
                {
                    TotalReturns = returns.Count,
                    TotalReturnAmount = returns.Sum(sr => sr.TotalReturnAmount),
                    ReturnReasonCounts = returns
                        .Where(sr => sr.ReturnReason != null)
                        .GroupBy(sr => sr.ReturnReason!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new SalesReturnStatistics();
            }
        }

        /// <summary>
        /// 儲存銷貨退回連同明細
        /// </summary>
        public async Task<ServiceResult<SalesReturn>> SaveWithDetailsAsync(SalesReturn salesReturn, List<SalesReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 驗證主檔
                    var validationResult = await ValidateAsync(salesReturn);
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<SalesReturn>.Failure(validationResult.ErrorMessage);
                    }

                    // 儲存主檔 - 在同一個 context 中處理
                    SalesReturn savedEntity;
                    var dbSet = context.Set<SalesReturn>();

                    if (salesReturn.Id > 0)
                    {
                        // 更新模式
                        var existingEntity = await dbSet
                            .FirstOrDefaultAsync(x => x.Id == salesReturn.Id);
                            
                        if (existingEntity == null)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult<SalesReturn>.Failure("找不到要更新的銷貨退回資料");
                        }

                        // 更新主檔資訊
                        salesReturn.UpdatedAt = DateTime.UtcNow;
                        salesReturn.CreatedAt = existingEntity.CreatedAt; // 保持原建立時間
                        salesReturn.CreatedBy = existingEntity.CreatedBy; // 保持原建立者

                        context.Entry(existingEntity).CurrentValues.SetValues(salesReturn);
                        savedEntity = existingEntity;
                    }
                    else
                    {
                        // 新增模式
                        salesReturn.CreatedAt = DateTime.UtcNow;
                        salesReturn.UpdatedAt = DateTime.UtcNow;
                        salesReturn.Status = EntityStatus.Active;

                        await dbSet.AddAsync(salesReturn);
                        savedEntity = salesReturn;
                    }

                    // 先儲存主檔以取得 ID
                    await context.SaveChangesAsync();

                    // 儲存明細 - 在同一個 context 和 transaction 中處理
                    var detailResult = await UpdateDetailsInContext(context, savedEntity.Id, details);
                    if (!detailResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<SalesReturn>.Failure($"儲存明細失敗：{detailResult.ErrorMessage}");
                    }

                    // 更新庫存邏輯 - 銷貨退回會增加庫存 (只處理有數量變更的明細)
                    if (_inventoryStockService != null)
                    {
                        var stockChanges = detailResult.Data ?? new List<(SalesReturnDetail, decimal)>();
                        
                        foreach (var (detail, quantityDiff) in stockChanges.Where(sc => sc.Item2 != 0))
                        {
                            // 從關聯的銷貨訂單明細取得倉庫ID
                            int? warehouseId = null;
                            
                            // 方法1：如果有關聯的銷貨訂單明細，直接從中取得倉庫ID
                            if (detail.SalesOrderDetailId.HasValue)
                            {
                                var orderDetail = await context.SalesOrderDetails
                                    .FirstOrDefaultAsync(sod => sod.Id == detail.SalesOrderDetailId.Value);
                                warehouseId = orderDetail?.WarehouseId;
                            }

                            // 如果還是沒有倉庫ID，跳過此明細並記錄警告
                            if (!warehouseId.HasValue)
                            {
                                _logger?.LogWarning("退回明細 ID:{DetailId} 無法取得倉庫ID，跳過庫存更新", detail.Id);
                                continue;
                            }

                            // 根據數量差異進行庫存調整
                            ServiceResult stockResult;
                            if (quantityDiff > 0)
                            {
                                // 退回數量增加，需要增加庫存
                                stockResult = await _inventoryStockService.AddStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    (int)Math.Ceiling(quantityDiff), // 轉為整數，向上取整
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.SalesReturnNumber,
                                    detail.OriginalUnitPrice, // 使用原始單價
                                    null, // 倉庫位置ID (銷貨退回通常不指定特定位置)
                                    $"銷貨退回增量 - {savedEntity.SalesReturnNumber}"
                                );
                            }
                            else
                            {
                                // 退回數量減少，需要減少庫存 (撤銷部分退回)
                                stockResult = await _inventoryStockService.ReduceStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    (int)Math.Ceiling(Math.Abs(quantityDiff)), // 轉為正整數
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.SalesReturnNumber,
                                    null, // 倉庫位置ID
                                    $"銷貨退回撤銷 - {savedEntity.SalesReturnNumber}"
                                );
                            }

                            if (!stockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult<SalesReturn>.Failure($"更新庫存失敗：{stockResult.ErrorMessage}");
                            }
                        }
                    }

                    // 計算並更新總金額
                    var calculateResult = await CalculateTotalsInContext(context, savedEntity.Id);
                    if (!calculateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<SalesReturn>.Failure($"計算總金額失敗：{calculateResult.ErrorMessage}");
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<SalesReturn>.Success(savedEntity);
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
                    SalesReturnId = salesReturn.Id 
                });
                return ServiceResult<SalesReturn>.Failure($"儲存銷貨退回時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 在指定的 DbContext 中更新銷貨退回明細
        /// </summary>
        /// <returns>ServiceResult，其中Data包含數量變更資訊的列表</returns>
        private async Task<ServiceResult<List<(SalesReturnDetail detail, decimal quantityDifference)>>> UpdateDetailsInContext(AppDbContext context, int salesReturnId, List<SalesReturnDetail> details)
        {
            try
            {
                // 取得現有明細
                var existingDetails = await context.SalesReturnDetails
                    .Where(d => d.SalesReturnId == salesReturnId)
                    .ToListAsync();

                var quantityChanges = new List<(SalesReturnDetail detail, decimal quantityDifference)>();

                // 準備要處理的明細
                var newDetailsToAdd = new List<SalesReturnDetail>();
                var updatedDetailsToUpdate = new List<(SalesReturnDetail existing, SalesReturnDetail updated)>();
                var existingDetailsToKeep = new List<int>();

                foreach (var detail in details.Where(d => d.ProductId > 0 && d.ReturnQuantity > 0))
                {
                    detail.SalesReturnId = salesReturnId;
                    detail.UpdatedAt = DateTime.UtcNow;

                    if (detail.Id > 0)
                    {
                        // 更新現有明細
                        var existing = existingDetails.FirstOrDefault(ed => ed.Id == detail.Id);
                        if (existing != null)
                        {
                            var oldQuantity = existing.ReturnQuantity;
                            var quantityDiff = detail.ReturnQuantity - oldQuantity;

                            if (Math.Abs(quantityDiff) > 0.001m) // 有數量變更
                            {
                                quantityChanges.Add((detail, quantityDiff));
                            }

                            updatedDetailsToUpdate.Add((existing, detail));
                            existingDetailsToKeep.Add(existing.Id);
                        }
                    }
                    else
                    {
                        // 新增明細
                        detail.CreatedAt = DateTime.UtcNow;
                        detail.Status = EntityStatus.Active;
                        newDetailsToAdd.Add(detail);

                        // 新增的明細，數量差異就是新增的數量
                        quantityChanges.Add((detail, detail.ReturnQuantity));
                    }
                }

                // 刪除不再需要的明細
                var detailsToDelete = existingDetails.Where(ed => !existingDetailsToKeep.Contains(ed.Id)).ToList();
                foreach (var detail in detailsToDelete)
                {
                    detail.UpdatedAt = DateTime.UtcNow;
                    // 被刪除的明細，數量差異是負的原數量
                    quantityChanges.Add((detail, -detail.ReturnQuantity));
                }

                // 執行資料庫更新
                foreach (var (existing, updated) in updatedDetailsToUpdate)
                {
                    updated.CreatedAt = existing.CreatedAt; // 保持原建立時間
                    updated.UpdatedAt = DateTime.UtcNow;
                    context.Entry(existing).CurrentValues.SetValues(updated);
                }

                if (newDetailsToAdd.Any())
                {
                    await context.SalesReturnDetails.AddRangeAsync(newDetailsToAdd);
                }

                await context.SaveChangesAsync();
                return ServiceResult<List<(SalesReturnDetail detail, decimal quantityDifference)>>.Success(quantityChanges);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsInContext), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsInContext),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId 
                });
                return ServiceResult<List<(SalesReturnDetail detail, decimal quantityDifference)>>.Failure($"更新明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 計算並更新銷貨退回總金額
        /// </summary>
        private async Task<ServiceResult> CalculateTotalsInContext(AppDbContext context, int salesReturnId)
        {
            try
            {
                var salesReturn = await context.SalesReturns
                    .FirstOrDefaultAsync(sr => sr.Id == salesReturnId);

                if (salesReturn == null)
                {
                    return ServiceResult.Failure("找不到銷貨退回資料");
                }

                var details = await context.SalesReturnDetails
                    .Where(d => d.SalesReturnId == salesReturnId)
                    .ToListAsync();

                // 計算總金額
                var totalAmount = details.Sum(d => d.ReturnQuantity * d.OriginalUnitPrice);
                salesReturn.TotalReturnAmount = totalAmount;
                salesReturn.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalsInContext), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalsInContext),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId 
                });
                return ServiceResult.Failure($"計算總金額時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新銷貨退回明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int salesReturnId, List<SalesReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var result = await UpdateDetailsInContext(context, salesReturnId, details);
                return result.IsSuccess ? ServiceResult.Success() : ServiceResult.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId 
                });
                return ServiceResult.Failure($"更新銷貨退回明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 永久刪除銷貨退回單（含庫存回滾）
        /// 這是UI實際調用的刪除方法
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            Console.WriteLine($"=== SalesReturnService.PermanentDeleteAsync 開始執行 ===");
            Console.WriteLine($"要刪除的 SalesReturn ID: {id}");
            
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                Console.WriteLine("資料庫交易已開始");
                
                try
                {
                    // 1. 先取得主記錄（含明細資料，包含關聯的銷售訂單明細）
                    var entity = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails.AsQueryable())
                            .ThenInclude(srd => srd.Product)
                        .Include(sr => sr.SalesReturnDetails.AsQueryable())
                            .ThenInclude(srd => srd.SalesOrderDetail)
                                .ThenInclude(sod => sod != null ? sod.SalesOrder : null)
                        .FirstOrDefaultAsync(sr => sr.Id == id);

                    if (entity == null)
                    {
                        Console.WriteLine($"找不到ID為 {id} 的銷貨退回記錄");
                        return ServiceResult.Failure("找不到要刪除的銷貨退回記錄");
                    }
                    
                    Console.WriteLine($"找到銷貨退回記錄: {entity.SalesReturnNumber}, 明細數量: {entity.SalesReturnDetails.Count}");
                    
                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        return canDeleteResult;
                    }
                    
                    // 3. 回滾庫存 - 將之前因退貨而增加的庫存減少回去
                    if (_inventoryStockService != null)
                    {
                        var eligibleDetails = entity.SalesReturnDetails.Where(d => d.ReturnQuantity > 0).ToList();
                        Console.WriteLine($"需要進行庫存回滾的明細數量: {eligibleDetails.Count}");

                        foreach (var detail in eligibleDetails)
                        {
                            // 從關聯的銷貨訂單明細取得倉庫ID
                            int? warehouseId = null;
                            if (detail.SalesOrderDetailId.HasValue && detail.SalesOrderDetail != null)
                            {
                                warehouseId = detail.SalesOrderDetail.WarehouseId;
                            }

                            if (!warehouseId.HasValue)
                            {
                                Console.WriteLine($"明細 {detail.Id} 無法取得倉庫ID，跳過庫存回滾");
                                continue;
                            }

                            Console.WriteLine($"處理明細庫存回滾 - 產品ID: {detail.ProductId}, 倉庫ID: {warehouseId}, " +
                                             $"退貨數量: {detail.ReturnQuantity}");
                            
                            // 執行庫存減少（撤銷退貨時增加的庫存）
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                warehouseId.Value,
                                (int)Math.Ceiling(detail.ReturnQuantity), // 轉為整數，向上取整
                                InventoryTransactionTypeEnum.Return,
                                $"{entity.SalesReturnNumber}_DEL", // 標記為刪除操作
                                null, // 倉庫位置ID (銷貨退回通常不指定特定位置)
                                $"刪除銷貨退回單回滾庫存 - {entity.SalesReturnNumber}"
                            );

                            Console.WriteLine($"庫存回滾結果: {(reduceResult.IsSuccess ? "成功" : $"失敗 - {reduceResult.ErrorMessage}")}");
                            
                            if (!reduceResult.IsSuccess)
                            {
                                Console.WriteLine($"庫存回滾失敗，取消刪除操作: {reduceResult.ErrorMessage}");
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回滾失敗：{reduceResult.ErrorMessage}");
                            }
                        }
                        
                        Console.WriteLine("庫存回滾處理完成");
                    }
                    else
                    {
                        Console.WriteLine("庫存服務未注入，跳過庫存回滾操作");
                    }

                    // 4. 執行實際的資料刪除（硬刪除）
                    Console.WriteLine($"開始執行資料刪除操作 - 銷貨退回單ID: {id}");
                    
                    // 刪除明細
                    context.SalesReturnDetails.RemoveRange(entity.SalesReturnDetails);
                    Console.WriteLine($"已標記刪除 {entity.SalesReturnDetails.Count} 筆明細記錄");
                    
                    // 刪除主檔
                    context.SalesReturns.Remove(entity);
                    Console.WriteLine("已標記刪除主檔記錄");

                    // 5. 儲存變更
                    var changesCount = await context.SaveChangesAsync();
                    Console.WriteLine($"資料庫變更已保存，影響 {changesCount} 筆記錄");
                    
                    // 6. 提交交易
                    await transaction.CommitAsync();
                    Console.WriteLine("資料庫交易已提交");
                    
                    Console.WriteLine("=== SalesReturnService.PermanentDeleteAsync 執行成功 ===");
                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"執行過程中發生異常: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("永久刪除銷貨退回單過程發生錯誤");
            }
        }
    }
}

