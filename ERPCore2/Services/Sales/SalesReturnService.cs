using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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

        protected override IQueryable<SalesReturn> BuildGetAllQuery(AppDbContext context)
        {
            return context.SalesReturns
                .Include(sr => sr.Customer)
                .Include(sr => sr.Employee)
                .Include(sr => sr.ReturnReason)
                .Include(sr => sr.ApprovedByUser)
                .Include(sr => sr.SalesReturnDetails)
                    .ThenInclude(srd => srd.Product)
                .OrderByDescending(sr => sr.ReturnDate)
                .ThenBy(sr => sr.Code);
        }

        public override async Task<SalesReturn?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Include(sr => sr.ApprovedByUser)
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

                if (!entity.IsDraft && !(entity.CustomerId > 0))
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
                // 1. 執行基類檢查（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                {
                    return baseResult;
                }

                // 2. 如果實體沒有載入明細資料，從資料庫重新載入
                if (entity.SalesReturnDetails == null || !entity.SalesReturnDetails.Any())
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var entityWithDetails = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails)
                            .ThenInclude(srd => srd.Product)
                        .FirstOrDefaultAsync(sr => sr.Id == entity.Id);

                    if (entityWithDetails == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的銷貨退回單");
                    }

                    entity = entityWithDetails;
                }

                // 3. 檢查每個明細是否被鎖定
                foreach (var detail in entity.SalesReturnDetails)
                {
                    var productName = detail.Product?.Name ?? $"商品ID:{detail.ProductId}";
                    
                    // 檢查是否有沖款記錄（TotalPaidAmount > 0 表示已有沖款）
                    if (detail.TotalPaidAmount > 0)
                    {
                        var errorMsg = $"無法刪除此銷貨退回單，因為商品「{productName}」已有沖款記錄（已沖款 {detail.TotalPaidAmount:N2} 元）";
                        return ServiceResult.Failure(errorMsg);
                    }
                }

                // 4. 所有檢查都通過
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
        /// 檢查銷貨退回編號是否已存在（符合 EntityCodeGenerationHelper 約定）
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

                var returnIds = await context.SalesReturnDetails
                    .Where(d => d.SalesDeliveryDetailId.HasValue &&
                                context.SalesDeliveryDetails
                                    .Where(sdd => sdd.SalesDeliveryId == salesDeliveryId)
                                    .Select(sdd => sdd.Id)
                                    .Contains(d.SalesDeliveryDetailId.Value))
                    .Select(d => d.SalesReturnId)
                    .Distinct()
                    .ToListAsync();

                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => returnIds.Contains(sr.Id))
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
                    query = query.Where(sr => sr.CustomerId.HasValue && criteria.RelatedEntityIds.Contains(sr.CustomerId.Value));
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

                    // 🔥 注意：庫存更新改為由 Component 層統一呼叫 UpdateInventoryByDifferenceAsync
                    // 這裡不處理庫存邏輯，避免與差異計算重複

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

                // 🔥 更新銷貨明細的累計退貨數量
                foreach (var (detail, quantityDiff) in quantityChanges)
                {
                    if (detail.SalesDeliveryDetailId.HasValue && detail.SalesDeliveryDetailId.Value > 0)
                    {
                        var deliveryDetail = await context.SalesDeliveryDetails
                            .FirstOrDefaultAsync(dd => dd.Id == detail.SalesDeliveryDetailId.Value);
                        
                        if (deliveryDetail != null)
                        {
                            // 累加退貨數量（quantityDiff 可能為正或負）
                            deliveryDetail.TotalReturnedQuantity += quantityDiff;
                            
                            // 確保不會變成負數
                            if (deliveryDetail.TotalReturnedQuantity < 0)
                            {
                                deliveryDetail.TotalReturnedQuantity = 0;
                            }
                        }
                    }
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
                                InventoryTransactionTypeEnum.SalesReturn,
                                entity.Code ?? string.Empty,  // 使用原始單號
                                null, // 倉庫位置ID (銷貨退回通常不指定特定位置)
                                $"刪除銷貨退回單回滾庫存 - {entity.Code}",
                                sourceDocumentType: InventorySourceDocumentTypes.SalesReturn,
                                sourceDocumentId: entity.Id,
                                operationType: InventoryOperationTypeEnum.Delete  // 標記為刪除操作
                            );
                            
                            if (!reduceResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"庫存回滾失敗：{reduceResult.ErrorMessage}");
                            }
                            
                            // 🔥 回退銷貨明細的累計退貨數量
                            if (detail.SalesDeliveryDetailId.HasValue)
                            {
                                var deliveryDetail = await context.SalesDeliveryDetails
                                    .FirstOrDefaultAsync(dd => dd.Id == detail.SalesDeliveryDetailId.Value);
                                
                                if (deliveryDetail != null)
                                {
                                    deliveryDetail.TotalReturnedQuantity -= detail.ReturnQuantity;
                                    
                                    // 確保不會變成負數
                                    if (deliveryDetail.TotalReturnedQuantity < 0)
                                    {
                                        deliveryDetail.TotalReturnedQuantity = 0;
                                    }
                                }
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

        /// <summary>
        /// 使用差異計算更新庫存
        /// 退貨會增加庫存（與出貨相反）
        /// </summary>
        public async Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0)
        {
            try
            {
                if (_inventoryStockService == null)
                {
                    return ServiceResult.Failure("庫存服務未初始化");
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 查詢退回單及其明細（包含倉庫、庫位等完整資訊）
                    var currentReturn = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails)
                            .ThenInclude(srd => srd.Product)
                        .FirstOrDefaultAsync(sr => sr.Id == id);

                    if (currentReturn == null)
                    {
                        return ServiceResult.Failure($"找不到ID為 {id} 的銷貨退回單");
                    }

                    // 🔑 簡化設計：查詢該單據的所有異動明細，透過 OperationType 過濾
                    var allTransactionDetails = await context.InventoryTransactionDetails
                        .Include(d => d.InventoryTransaction)
                        .Include(d => d.InventoryStockDetail)
                        .Where(d => d.InventoryTransaction.TransactionNumber == currentReturn.Code)
                        .OrderBy(d => d.OperationTime)
                        .ThenBy(d => d.Id)
                        .ToListAsync();
                    
                    // 找到最後一次刪除記錄（OperationType = Delete）
                    var lastDeleteDetail = allTransactionDetails
                        .Where(d => d.OperationType == InventoryOperationTypeEnum.Delete)
                        .OrderByDescending(d => d.OperationTime)
                        .ThenByDescending(d => d.Id)
                        .FirstOrDefault();
                    
                    // 只計算最後一次刪除之後的記錄（不含刪除操作本身）
                    var existingDetails = lastDeleteDetail != null
                        ? allTransactionDetails.Where(d => d.Id > lastDeleteDetail.Id && 
                                                          d.OperationType != InventoryOperationTypeEnum.Delete).ToList()
                        : allTransactionDetails.Where(d => d.OperationType != InventoryOperationTypeEnum.Delete).ToList();

                    // 3. 建立已處理過庫存的明細字典（ProductId + WarehouseId + LocationId -> 已處理庫存淨值）
                    var processedInventory = new Dictionary<string, (int ProductId, int WarehouseId, int? LocationId, decimal NetProcessedQuantity)>();
                    
                    foreach (var detail in existingDetails)
                    {
                        var detailWarehouseId = detail.InventoryStockDetail?.WarehouseId ?? detail.InventoryTransaction.WarehouseId;
                        var key = $"{detail.ProductId}_{detailWarehouseId}_{detail.WarehouseLocationId?.ToString() ?? "null"}";
                        if (!processedInventory.ContainsKey(key))
                        {
                            processedInventory[key] = (detail.ProductId, detailWarehouseId, detail.WarehouseLocationId, 0m);
                        }
                        // 累加所有交易的淨值（退貨的 Quantity 是正數）
                        var oldQty = processedInventory[key].NetProcessedQuantity;
                        var newQty = oldQty + detail.Quantity;
                        processedInventory[key] = (processedInventory[key].ProductId, processedInventory[key].WarehouseId, 
                                                  processedInventory[key].LocationId, newQty);
                    }
                    
                    // 4. 建立當前明細字典
                    var currentInventory = new Dictionary<string, (int ProductId, int? WarehouseId, int? LocationId, decimal CurrentQuantity)>();
                    
                    foreach (var detail in currentReturn.SalesReturnDetails)
                    {
                        // 從關聯的銷貨出貨明細取得倉庫ID
                        int? warehouseId = null;
                        int? locationId = null;
                        
                        if (detail.SalesDeliveryDetailId.HasValue)
                        {
                            var deliveryDetail = await context.SalesDeliveryDetails
                                .FirstOrDefaultAsync(sdd => sdd.Id == detail.SalesDeliveryDetailId.Value);
                            warehouseId = deliveryDetail?.WarehouseId;
                            locationId = deliveryDetail?.WarehouseLocationId;
                        }
                        
                        // 如果沒有倉庫ID，跳過此明細
                        if (!warehouseId.HasValue)
                        {
                            continue;
                        }
                        
                        var key = $"{detail.ProductId}_{warehouseId}_{locationId?.ToString() ?? "null"}";
                        if (!currentInventory.ContainsKey(key))
                        {
                            currentInventory[key] = (detail.ProductId, warehouseId, locationId, 0);
                        }
                        var oldQty = currentInventory[key].CurrentQuantity;
                        var newQty = oldQty + (int)detail.ReturnQuantity;
                        currentInventory[key] = (currentInventory[key].ProductId, currentInventory[key].WarehouseId, 
                                               currentInventory[key].LocationId, newQty);
                    }
                    
                    // 5. 處理庫存差異 - 使用淨值計算方式
                    var allKeys = processedInventory.Keys.Union(currentInventory.Keys).ToList();
                    
                    foreach (var key in allKeys)
                    {
                        var hasProcessed = processedInventory.ContainsKey(key);
                        var hasCurrent = currentInventory.ContainsKey(key);
                        
                        // 🔑 關鍵：退貨是增加庫存，所以目標數量是正數
                        decimal targetQuantity = hasCurrent ? currentInventory[key].CurrentQuantity : 0m;
                        
                        // 計算已處理的庫存數量（之前所有交易的淨值，已經是正數）
                        decimal processedQuantity = hasProcessed ? processedInventory[key].NetProcessedQuantity : 0m;
                        
                        // 計算需要調整的數量
                        decimal adjustmentNeeded = targetQuantity - processedQuantity;
                        
                        if (adjustmentNeeded != 0)
                        {
                            var productId = hasCurrent ? currentInventory[key].ProductId : processedInventory[key].ProductId;
                            var warehouseId = hasCurrent ? currentInventory[key].WarehouseId : processedInventory[key].WarehouseId;
                            var locationId = hasCurrent ? currentInventory[key].LocationId : processedInventory[key].LocationId;
                            
                            // 跳過沒有指定倉庫的明細
                            if (!warehouseId.HasValue)
                                continue;
                            
                            if (adjustmentNeeded > 0)
                            {
                                // 需要增加更多庫存（退貨數量增加）
                                var addResult = await _inventoryStockService.AddStockAsync(
                                    productId,
                                    warehouseId.Value,
                                    adjustmentNeeded,
                                    InventoryTransactionTypeEnum.SalesReturn,
                                    currentReturn.Code ?? string.Empty,  // 使用原始單號
                                    null,  // 退貨不需要成本
                                    locationId,
                                    $"銷貨退回編輯調增 - {currentReturn.Code}",
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesReturn,
                                    sourceDocumentId: currentReturn.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!addResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存增加失敗：{addResult.ErrorMessage}");
                                }
                            }
                            else
                            {
                                // 需要扣減庫存（退貨數量減少）
                                var reduceResult = await _inventoryStockService.ReduceStockAsync(
                                    productId,
                                    warehouseId.Value,
                                    Math.Abs(adjustmentNeeded),
                                    InventoryTransactionTypeEnum.Sale,
                                    currentReturn.Code ?? string.Empty,  // 使用原始單號
                                    locationId,
                                    $"銷貨退回編輯調減 - {currentReturn.Code}",
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesReturn,
                                    sourceDocumentId: currentReturn.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust  // 標記為調整操作
                                );
                                
                                if (!reduceResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存扣減失敗：{reduceResult.ErrorMessage}");
                                }
                            }
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInventoryByDifferenceAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateInventoryByDifferenceAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("更新庫存差異時發生錯誤");
            }
        }

        /// <summary>
        /// 確認銷貨退回單並更新庫存（首次新增時使用）
        /// 功能：執行退回確認流程，將退回數量回補到庫存
        /// 處理流程：
        /// 1. 驗證退回單存在性
        /// 2. 對每個明細進行庫存回補操作
        /// 3. 使用原始單號作為 TransactionNumber，搭配 OperationType 區分操作類型
        /// 4. 使用資料庫交易確保資料一致性
        /// 5. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">退回單ID</param>
        /// <param name="confirmedBy">確認人員ID（保留參數，未來可能使用）</param>
        /// <returns>確認結果，包含成功狀態及錯誤訊息</returns>
        public async Task<ServiceResult> ConfirmReturnAsync(int id, int confirmedBy = 0)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var salesReturn = await context.SalesReturns
                        .Include(sr => sr.SalesReturnDetails)
                        .FirstOrDefaultAsync(sr => sr.Id == id);
                    
                    if (salesReturn == null)
                        return ServiceResult.Failure("找不到指定的退回單");
                    
                    // 更新庫存 - 退回會增加庫存
                    foreach (var detail in salesReturn.SalesReturnDetails)
                    {
                        if (detail.ReturnQuantity > 0)
                        {
                            // 從關聯的出貨明細取得倉庫ID和倉庫位置ID
                            int? warehouseId = null;
                            int? warehouseLocationId = null;
                            
                            if (detail.SalesDeliveryDetailId.HasValue)
                            {
                                var deliveryDetail = await context.SalesDeliveryDetails
                                    .FirstOrDefaultAsync(sdd => sdd.Id == detail.SalesDeliveryDetailId.Value);
                                if (deliveryDetail != null)
                                {
                                    warehouseId = deliveryDetail.WarehouseId;
                                    warehouseLocationId = deliveryDetail.WarehouseLocationId;
                                }
                            }

                            // 如果還是沒有倉庫ID，跳過此明細
                            if (!warehouseId.HasValue)
                            {
                                continue;
                            }

                            if (_inventoryStockService != null)
                            {
                                var addStockResult = await _inventoryStockService.AddStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    (int)detail.ReturnQuantity,
                                    InventoryTransactionTypeEnum.SalesReturn,
                                    salesReturn.Code ?? string.Empty,
                                    null,  // 退回不需要成本
                                    warehouseLocationId,
                                    $"銷貨退回確認 - {salesReturn.Code ?? string.Empty}",
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.SalesReturn,
                                    sourceDocumentId: salesReturn.Id
                                    );
                                
                                if (!addStockResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存回補失敗：{addStockResult.ErrorMessage}");
                                }
                            }
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmReturnAsync), GetType(), _logger, new { 
                    Method = nameof(ConfirmReturnAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ConfirmedBy = confirmedBy 
                });
                return ServiceResult.Failure("確認退回單過程發生錯誤");
            }
        }

        public async Task<List<SalesReturn>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var orderDetailIds = await context.SalesOrderDetails
                    .Where(sod => sod.SalesOrderId == salesOrderId)
                    .Select(sod => sod.Id)
                    .ToListAsync();

                var deliveryDetailIds = await context.SalesDeliveryDetails
                    .Where(sdd => sdd.SalesOrderDetailId.HasValue &&
                                  orderDetailIds.Contains(sdd.SalesOrderDetailId.Value))
                    .Select(sdd => sdd.Id)
                    .ToListAsync();

                var returnIds = await context.SalesReturnDetails
                    .Where(d => d.SalesDeliveryDetailId.HasValue &&
                                deliveryDetailIds.Contains(d.SalesDeliveryDetailId.Value))
                    .Select(d => d.SalesReturnId)
                    .Distinct()
                    .ToListAsync();

                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ReturnReason)
                    .Where(sr => returnIds.Contains(sr.Id))
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.Code)
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

        #region 審核作業

        public async Task<ServiceResult> ApproveAsync(int id, int? approvedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var entity = await context.SalesReturns.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return ServiceResult.Failure("找不到銷貨退回單");
                if (entity.IsApproved) return ServiceResult.Failure("銷貨退回單已核准，無需重複核准");

                entity.IsApproved = true;
                entity.ApprovedBy = approvedBy;
                entity.ApprovedAt = DateTime.Now;
                entity.RejectReason = null;
                entity.UpdatedAt = DateTime.Now;

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

        public async Task<ServiceResult> RejectAsync(int id, int rejectedBy, string reason)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.SalesReturns.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return ServiceResult.Failure("找不到銷貨退回單");

            entity.IsApproved = false;
            entity.ApprovedBy = rejectedBy;
            entity.ApprovedAt = DateTime.Now;
            entity.RejectReason = reason;
            entity.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        #endregion
    }
}

