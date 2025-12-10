using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .AsQueryable()
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.SalesDeliveryDetail)
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => ((sr.Code != null && sr.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (sr.Customer != null && sr.Customer.CompanyName != null && sr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm))))
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
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

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("退回單號不能為空");

                if (entity.ReturnDate == default)
                    errors.Add("退回日期不能為空");

                if (entity.ReturnDate > DateTime.Today)
                    errors.Add("退回日期不能大於今天");

                if (entity.CustomerId <= 0)
                    errors.Add("必須選擇客戶");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsSalesReturnCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
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
                    EntityNumber = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 檢查銷貨退回單是否可以被刪除
        /// 如果退回明細已有沖款記錄，則不允許刪除
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(SalesReturn entity)
        {
            try
            {
                ConsoleHelper.WriteDebug($"🔍 [CanDeleteAsync] 檢查銷貨退回單 ID={entity.Id} 是否可刪除");
                
                // 1. 執行基類檢查（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                {
                    ConsoleHelper.WriteWarning($"⚠ [CanDeleteAsync] 基類檢查失敗: {baseResult.ErrorMessage}");
                    return baseResult;
                }

                // 2. 如果實體沒有載入明細資料，從資料庫重新載入
                if (entity.SalesReturnDetails == null || !entity.SalesReturnDetails.Any())
                {
                    ConsoleHelper.WriteDebug($"📋 [CanDeleteAsync] 重新載入明細資料");
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var entityWithDetails = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails)
                            .ThenInclude(srd => srd.Product)
                        .FirstOrDefaultAsync(sr => sr.Id == entity.Id);

                    if (entityWithDetails == null)
                    {
                        ConsoleHelper.WriteError($"❌ [CanDeleteAsync] 找不到退回單 ID={entity.Id}");
                        return ServiceResult.Failure("找不到要刪除的銷貨退回單");
                    }

                    entity = entityWithDetails;
                }

                ConsoleHelper.WriteDebug($"📋 [CanDeleteAsync] 檢查 {entity.SalesReturnDetails.Count} 筆明細");
                
                // 3. 檢查每個明細是否被鎖定
                foreach (var detail in entity.SalesReturnDetails)
                {
                    var productName = detail.Product?.Name ?? $"商品ID:{detail.ProductId}";
                    ConsoleHelper.WriteDebug($"  🔍 明細 ID={detail.Id}, 商品={productName}, TotalPaidAmount={detail.TotalPaidAmount:N2}, IsSettled={detail.IsSettled}");
                    
                    // 檢查是否有沖款記錄（TotalPaidAmount > 0 表示已有沖款）
                    if (detail.TotalPaidAmount > 0)
                    {
                        var errorMsg = $"無法刪除此銷貨退回單，因為商品「{productName}」已有沖款記錄（已沖款 {detail.TotalPaidAmount:N2} 元）";
                        ConsoleHelper.WriteWarning($"⚠ [CanDeleteAsync] {errorMsg}");
                        return ServiceResult.Failure(errorMsg);
                    }
                }

                // 4. 所有檢查都通過
                ConsoleHelper.WriteSuccess($"✓ [CanDeleteAsync] 銷貨退回單 ID={entity.Id} 可以刪除");
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityNumber = entity.Code
                });
                return ServiceResult.Failure("檢查刪除權限時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查銷貨退回代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        public async Task<bool> IsSalesReturnCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesReturns.Where(sr => sr.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(sr => sr.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesReturnCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSalesReturnCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.CustomerId == customerId)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
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

        public async Task<List<SalesReturn>> GetBySalesDeliveryIdAsync(int salesDeliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.SalesDeliveryId == salesDeliveryId)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesDeliveryIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesDeliveryIdAsync),
                    ServiceType = GetType().Name,
                    SalesDeliveryId = salesDeliveryId
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => sr.ReturnDate >= startDate && sr.ReturnDate <= endDate)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
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
                    .Where(sr => sr.Code != null && sr.Code.StartsWith(prefix))
                    .OrderByDescending(sr => sr.Code)
                    .Select(sr => sr.Code)
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
        /// 根據批次列印條件查詢銷貨退回單（支援多條件組合篩選）
        /// 設計理念：靈活組合日期、客戶、狀態等多種篩選條件，適用於批次列印場景
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的銷貨退回單列表（包含完整關聯資料）</returns>
        public async Task<List<SalesReturn>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 基本查詢 - 載入所有需要的關聯資料
                var query = context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.SalesReturnDetails.AsQueryable())
                        .ThenInclude(srd => srd.Product)
                    .AsQueryable();

                // 篩選條件 1: 日期範圍（退回日期）
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(sr => sr.ReturnDate >= criteria.StartDate.Value);
                }
                if (criteria.EndDate.HasValue)
                {
                    // 結束日期包含當天 23:59:59
                    var endDateInclusive = criteria.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(sr => sr.ReturnDate <= endDateInclusive);
                }

                // 篩選條件 2: 客戶ID列表（RelatedEntityIds 在銷貨退回單中代表客戶ID）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(sr => criteria.RelatedEntityIds.Contains(sr.CustomerId));
                }

                // 篩選條件 3: 倉庫ID（透過明細關聯的銷貨出貨明細）
                if (criteria.WarehouseId.HasValue)
                {
                    query = query.Where(sr => sr.SalesReturnDetails.Any(srd => 
                        srd.SalesDeliveryDetail != null && 
                        srd.SalesDeliveryDetail.WarehouseId == criteria.WarehouseId.Value));
                }

                // 篩選條件 4: 狀態列表（Status 欄位，銷貨退回單使用 EntityStatus）
                if (criteria.Statuses != null && criteria.Statuses.Any())
                {
                    // 將字串狀態轉換為 EntityStatus 枚舉
                    var statusEnums = criteria.Statuses
                        .Select(s => Enum.TryParse<EntityStatus>(s, true, out var status) ? status : (EntityStatus?)null)
                        .Where(s => s.HasValue)
                        .Select(s => s!.Value)
                        .ToList();

                    if (statusEnums.Any())
                    {
                        query = query.Where(sr => statusEnums.Contains(sr.Status));
                    }
                }

                // 篩選條件 5: 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(sr => sr.Code != null && sr.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 篩選條件 6: 是否包含已刪除的單據
                if (!criteria.IncludeCancelled)
                {
                    query = query.Where(sr => sr.Status != EntityStatus.Deleted);
                }

                // 排序（按退回日期降序，再按單號）
                query = query
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenByDescending(sr => sr.Code);

                // 限制結果數量（避免一次載入過多資料）
                if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
                {
                    query = query.Take(criteria.MaxResults.Value);
                }

                // 執行查詢
                var results = await query.ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByBatchCriteriaAsync),
                    ServiceType = GetType().Name,
                    Criteria = new
                    {
                        criteria.StartDate,
                        criteria.EndDate,
                        criteria.RelatedEntityIds,
                        criteria.CompanyId,
                        criteria.WarehouseId,
                        criteria.Statuses,
                        criteria.DocumentNumberKeyword,
                        criteria.IncludeCancelled,
                        criteria.MaxResults
                    }
                });
                return new List<SalesReturn>();
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
                            // 從關聯的銷貨出貨明細取得倉庫ID
                            int? warehouseId = null;
                            
                            // 如果有關聯的銷貨出貨明細，直接從中取得倉庫ID
                            if (detail.SalesDeliveryDetailId.HasValue)
                            {
                                var deliveryDetail = await context.SalesDeliveryDetails
                                    .FirstOrDefaultAsync(sdd => sdd.Id == detail.SalesDeliveryDetailId.Value);
                                warehouseId = deliveryDetail?.WarehouseId;
                            }

                            // 如果還是沒有倉庫ID，跳過此明細並記錄警告
                            if (!warehouseId.HasValue)
                            {
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
                                    savedEntity.Code ?? string.Empty,
                                    detail.OriginalUnitPrice, // 使用原始單價
                                    null, // 倉庫位置ID (銷貨退回通常不指定特定位置)
                                    $"銷貨退回增量 - {savedEntity.Code ?? string.Empty}"
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
                                    savedEntity.Code ?? string.Empty,
                                    null, // 倉庫位置ID
                                    $"銷貨退回撤銷 - {savedEntity.Code ?? string.Empty}"
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
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 先取得主記錄（含明細資料，包含關聯的銷售訂單明細）
                    var entity = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails.AsQueryable())
                            .ThenInclude(srd => srd.Product)
                        .Include(sr => sr.SalesReturnDetails.AsQueryable())
                            .ThenInclude(srd => srd.SalesDeliveryDetail)
                                .ThenInclude(sdd => sdd != null ? sdd.SalesDelivery : null)
                        .FirstOrDefaultAsync(sr => sr.Id == id);

                    if (entity == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的銷貨退回記錄");
                    }
                    
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

                        foreach (var detail in eligibleDetails)
                        {
                            // 從關聯的銷貨出貨明細取得倉庫ID
                            int? warehouseId = null;
                            if (detail.SalesDeliveryDetailId.HasValue && detail.SalesDeliveryDetail != null)
                            {
                                warehouseId = detail.SalesDeliveryDetail.WarehouseId;
                            }

                            if (!warehouseId.HasValue)
                            {
                                continue;
                            }
                            
                            // 執行庫存減少（撤銷退貨時增加的庫存）
                            var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                detail.ProductId,
                                warehouseId.Value,
                                (int)Math.Ceiling(detail.ReturnQuantity), // 轉為整數，向上取整
                                InventoryTransactionTypeEnum.Return,
                                $"{entity.Code}_DEL", // 標記為刪除操作
                                null, // 倉庫位置ID (銷貨退回通常不指定特定位置)
                                $"刪除銷貨退回單回滾庫存 - {entity.Code}"
                            );
                            
                            if (!reduceResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回滾失敗：{reduceResult.ErrorMessage}");
                            }
                        }
                    }

                    // 4. 執行實際的資料刪除（硬刪除）
                    
                    // 刪除明細
                    context.SalesReturnDetails.RemoveRange(entity.SalesReturnDetails);
                    
                    // 刪除主檔
                    context.SalesReturns.Remove(entity);

                    // 5. 儲存變更
                    await context.SaveChangesAsync();
                    
                    // 6. 提交交易
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

