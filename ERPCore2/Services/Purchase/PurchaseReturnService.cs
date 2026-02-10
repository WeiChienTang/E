using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class PurchaseReturnService : GenericManagementService<PurchaseReturn>, IPurchaseReturnService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public PurchaseReturnService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PurchaseReturn>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        public override async Task<List<PurchaseReturn>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .AsQueryable()
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReturn>();
            }
        }

        public override async Task<PurchaseReturn?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
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

        public async Task<PurchaseReturn?> GetWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .AsNoTracking()  // 🔑 不追蹤實體，確保每次都載入最新資料
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.Unit)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.PurchaseReceivingDetail)
                            .ThenInclude(prd => prd!.PurchaseReceiving)
                                .ThenInclude(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.PurchaseReceivingDetail)
                            .ThenInclude(prd => prd!.Product)
                                .ThenInclude(p => p.Unit)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.PurchaseReceivingDetail)
                            .ThenInclude(prd => prd!.Warehouse)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.PurchaseReceivingDetail)
                            .ThenInclude(prd => prd!.WarehouseLocation)
                    .FirstOrDefaultAsync(pr => pr.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return null;
            }
        }

        public async Task<List<PurchaseReturn>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.SupplierId == supplierId)
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return new List<PurchaseReturn>();
            }
        }

        public async Task<List<PurchaseReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.ReturnDate >= startDate && pr.ReturnDate <= endDate)
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.Code)
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
                return new List<PurchaseReturn>();
            }
        }

        public async Task<List<PurchaseReturn>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.PurchaseReceivingId == purchaseReceivingId)
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReceivingIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseReceivingIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId
                });
                return new List<PurchaseReturn>();
            }
        }

        /// <summary>
        /// 根據批次列印條件查詢進貨退出單（支援多條件組合篩選）
        /// 設計理念：靈活組合日期、廠商、狀態等多種篩選條件，適用於批次列印場景
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的進貨退出單列表（包含完整關聯資料）</returns>
        public async Task<List<PurchaseReturn>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 建立基礎查詢（包含必要的關聯資料）
                IQueryable<PurchaseReturn> query = context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.Product)
                    .AsQueryable();

                // 日期範圍篩選
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(pr => pr.ReturnDate >= criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate.HasValue)
                {
                    // 包含整天（到當天 23:59:59）
                    var endDate = criteria.EndDate.Value.Date.AddDays(1);
                    query = query.Where(pr => pr.ReturnDate < endDate);
                }

                // 廠商篩選（RelatedEntityIds 對應廠商ID列表）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(pr => criteria.RelatedEntityIds.Contains(pr.SupplierId));
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(pr => pr.Code != null && pr.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 排序：先按廠商分組，同廠商內再按日期和單據編號排序
                // 這樣列印時同一廠商的退貨單會集中在一起
                query = criteria.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(pr => pr.Supplier.CompanyName)
                           .ThenBy(pr => pr.ReturnDate)
                           .ThenBy(pr => pr.Code)
                    : query.OrderBy(pr => pr.Supplier.CompanyName)
                           .ThenByDescending(pr => pr.ReturnDate)
                           .ThenBy(pr => pr.Code);

                // 限制最大筆數
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
                        SupplierCount = criteria.RelatedEntityIds?.Count ?? 0,
                        criteria.DocumentNumberKeyword,
                        criteria.MaxResults
                    }
                });
                return new List<PurchaseReturn>();
            }
        }

        /// <summary>
        /// 檢查退回編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        public async Task<bool> IsPurchaseReturnCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturns.Where(pr => pr.Code == code);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(pr => pr.Id != excludeId.Value);
                }
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReturnCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseReturnCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public override async Task<List<PurchaseReturn>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => (pr.Code != null && pr.Code.Contains(searchTerm)) ||
                         (pr.Supplier != null && pr.Supplier.CompanyName.Contains(searchTerm)) ||
                         (pr.PurchaseReceiving != null && pr.PurchaseReceiving.Code != null && pr.PurchaseReceiving.Code.Contains(searchTerm)))
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<PurchaseReturn>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PurchaseReturn entity)
        {
            var result = new ServiceResult();

            try
            {
                // 檢查退回單號是否已存在
                if (await IsPurchaseReturnCodeExistsAsync(entity.Code ?? string.Empty, entity.Id > 0 ? entity.Id : null))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "退回單號已存在";
                    return result;
                }

                // 檢查供應商是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var supplierExists = await context.Suppliers.AnyAsync(s => s.Id == entity.SupplierId);
                if (!supplierExists)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "指定的供應商不存在";
                    return result;
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity?.Id
                });
                result.IsSuccess = false;
                result.ErrorMessage = "驗證時發生錯誤";
                return result;
            }
        }

        public async Task<ServiceResult> CalculateTotalsAsync(int id)
        {
            var result = new ServiceResult();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var calculateResult = await CalculateTotalsInContext(context, id);
                if (!calculateResult.IsSuccess)
                {
                    return calculateResult;
                }

                await context.SaveChangesAsync();

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalsAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalsAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                result.IsSuccess = false;
                result.ErrorMessage = "計算時發生錯誤";
                return result;
            }
        }

        /// <summary>
        /// 在指定的 DbContext 中計算採購退回總金額
        /// </summary>
        private async Task<ServiceResult> CalculateTotalsInContext(AppDbContext context, int id)
        {
            var result = new ServiceResult();

            try
            {
                var purchaseReturn = await context.PurchaseReturns
                    .Include(pr => pr.PurchaseReturnDetails)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (purchaseReturn == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "找不到指定的採購退回記錄";
                    return result;
                }

                // 計算明細小計（根據稅別）
                var detailSubtotals = purchaseReturn.PurchaseReturnDetails.Select(d => new
                {
                    BaseAmount = d.ReturnQuantity * d.OriginalUnitPrice,
                    TaxRate = d.TaxRate ?? 5.0m // 如果明細沒有設定稅率，使用預設 5%
                }).ToList();
                
                switch (purchaseReturn.TaxCalculationMethod)
                {
                    case TaxCalculationMethod.TaxExclusive:
                        // 外加稅：金額 = Σ(退回數量 × 單價)（四捨五入到整數），稅額 = 金額 × 稅率%（四捨五入到整數）
                        purchaseReturn.TotalReturnAmount = Math.Round(detailSubtotals.Sum(d => d.BaseAmount), 0, MidpointRounding.AwayFromZero);
                        purchaseReturn.ReturnTaxAmount = detailSubtotals.Sum(d =>
                        {
                            var detailTaxAmount = d.BaseAmount * (d.TaxRate / 100m);
                            return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                        });
                        break;
                        
                    case TaxCalculationMethod.TaxInclusive:
                        // 內含稅：總額 = Σ(退回數量 × 單價)，金額 = 總額 - 稅額，稅額 = 總額 / (1 + 稅率%) × 稅率%（四捨五入到整數）
                        var totalWithTax = detailSubtotals.Sum(d => d.BaseAmount);
                        var totalTax = detailSubtotals.Sum(d =>
                        {
                            var detailTaxAmount = d.BaseAmount / (1 + d.TaxRate / 100m) * (d.TaxRate / 100m);
                            return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                        });
                        purchaseReturn.TotalReturnAmount = Math.Round(totalWithTax - totalTax, 0, MidpointRounding.AwayFromZero);
                        purchaseReturn.ReturnTaxAmount = totalTax;
                        break;
                        
                    case TaxCalculationMethod.NoTax:
                        // 免稅：金額 = Σ(退回數量 × 單價)（四捨五入到整數），稅額 = 0
                        purchaseReturn.TotalReturnAmount = Math.Round(detailSubtotals.Sum(d => d.BaseAmount), 0, MidpointRounding.AwayFromZero);
                        purchaseReturn.ReturnTaxAmount = 0;
                        break;
                        
                    default:
                        // 預設使用外加稅（四捨五入到整數）
                        purchaseReturn.TotalReturnAmount = Math.Round(detailSubtotals.Sum(d => d.BaseAmount), 0, MidpointRounding.AwayFromZero);
                        purchaseReturn.ReturnTaxAmount = detailSubtotals.Sum(d =>
                        {
                            var detailTaxAmount = d.BaseAmount * (d.TaxRate / 100m);
                            return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                        });
                        break;
                }

                // TotalReturnAmountWithTax 是計算屬性，會自動計算，無需手動賦值

                // 保存總金額的變更 (在同一個交易中)
                await context.SaveChangesAsync();

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalsInContext), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalsInContext),
                    ServiceType = GetType().Name,
                    Id = id
                });
                result.IsSuccess = false;
                result.ErrorMessage = "計算總金額時發生錯誤";
                return result;
            }
        }



        public async Task<ServiceResult> CreateFromPurchaseReceivingAsync(int purchaseReceivingId, List<PurchaseReturnDetail> details)
        {
            var result = new ServiceResult();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var purchaseReceiving = await context.PurchaseReceivings
                        .FirstOrDefaultAsync(pr => pr.Id == purchaseReceivingId);

                    if (purchaseReceiving == null)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "找不到指定的進貨單";
                        return result;
                    }

                    var purchaseReturn = new PurchaseReturn
                    {
                        Code = await GenerateReturnNumberAsync(context),
                        SupplierId = purchaseReceiving.SupplierId,
                        PurchaseReceivingId = purchaseReceivingId,
                        ReturnDate = DateTime.Today,
                        Status = EntityStatus.Active,
                        PurchaseReturnDetails = details
                    };

                    context.PurchaseReturns.Add(purchaseReturn);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    result.IsSuccess = true;
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateFromPurchaseReceivingAsync), GetType(), _logger, new { 
                    Method = nameof(CreateFromPurchaseReceivingAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId
                });
                result.IsSuccess = false;
                result.ErrorMessage = "創建採購退回單時發生錯誤";
                return result;
            }
        }

        public async Task<PurchaseReturnStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.PurchaseReturns.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(pr => pr.ReturnDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pr => pr.ReturnDate <= endDate.Value);

                var returns = await query.ToListAsync();

                return new PurchaseReturnStatistics
                {
                    TotalReturns = returns.Count,
                    TotalReturnAmount = returns.Sum(pr => pr.TotalReturnAmountWithTax)
                };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new { 
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new PurchaseReturnStatistics();
            }
        }

        public async Task<decimal> GetTotalReturnAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.PurchaseReturns
                    .Where(pr => pr.SupplierId == supplierId);

                if (startDate.HasValue)
                    query = query.Where(pr => pr.ReturnDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pr => pr.ReturnDate <= endDate.Value);

                return await query.SumAsync(pr => pr.TotalReturnAmountWithTax);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReturnAmountAsync), GetType(), _logger, new { 
                    Method = nameof(GetTotalReturnAmountAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return 0;
            }
        }

        private async Task<string> GenerateReturnNumberAsync(AppDbContext context)
        {
            try
            {
                var today = DateTime.Today;
                var prefix = $"RT{today:yyyyMMdd}";
                
                var lastReturn = await context.PurchaseReturns
                    .Where(pr => pr.Code != null && pr.Code.StartsWith(prefix))
                    .OrderByDescending(pr => pr.Code)
                    .FirstOrDefaultAsync();

                if (lastReturn == null || lastReturn.Code == null)
                {
                    return $"{prefix}001";
                }

                var lastNumber = lastReturn.Code.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out int number))
                {
                    return $"{prefix}{(number + 1):D3}";
                }

                return $"{prefix}001";
            }
            catch
            {
                var today = DateTime.Today;
                return $"RT{today:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 儲存採購退貨連同明細
        /// </summary>
        public async Task<ServiceResult<PurchaseReturn>> SaveWithDetailsAsync(PurchaseReturn purchaseReturn, List<PurchaseReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 驗證主檔
                    var validationResult = await ValidateAsync(purchaseReturn);
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<PurchaseReturn>.Failure(validationResult.ErrorMessage);
                    }

                    // 儲存主檔 - 在同一個 context 中處理
                    PurchaseReturn savedEntity;
                    var dbSet = context.Set<PurchaseReturn>();

                    if (purchaseReturn.Id > 0)
                    {
                        // 更新模式
                        var existingEntity = await dbSet
                            .FirstOrDefaultAsync(x => x.Id == purchaseReturn.Id);
                            
                        if (existingEntity == null)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult<PurchaseReturn>.Failure("找不到要更新的採購退貨資料");
                        }

                        // 更新主檔資訊
                        purchaseReturn.UpdatedAt = DateTime.UtcNow;
                        purchaseReturn.CreatedAt = existingEntity.CreatedAt; // 保持原建立時間
                        purchaseReturn.CreatedBy = existingEntity.CreatedBy; // 保持原建立者

                        context.Entry(existingEntity).CurrentValues.SetValues(purchaseReturn);
                        savedEntity = existingEntity;
                    }
                    else
                    {
                        // 新增模式
                        purchaseReturn.CreatedAt = DateTime.UtcNow;
                        purchaseReturn.UpdatedAt = DateTime.UtcNow;
                        purchaseReturn.Status = EntityStatus.Active;

                        await dbSet.AddAsync(purchaseReturn);
                        savedEntity = purchaseReturn;
                    }

                    // 先儲存主檔以取得 ID
                    await context.SaveChangesAsync();

                    // 儲存明細 - 在同一個 context 和 transaction 中處理
                    var detailResult = await UpdateDetailsInContext(context, savedEntity.Id, details);
                    if (!detailResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<PurchaseReturn>.Failure($"儲存明細失敗：{detailResult.ErrorMessage}");
                    }

                    // 更新庫存邏輯 - 處理退貨的庫存變更（包含商品變更和數量變更）
                    if (_inventoryStockService != null)
                    {
                        var stockChanges = detailResult.Data ?? new List<(PurchaseReturnDetail, int)>();
                        
                        foreach (var (detail, quantityDiff) in stockChanges.Where(sc => sc.Item2 != 0))
                        {
                            // 從關聯的進貨明細取得倉庫ID
                            int? warehouseId = null;
                            
                            // 方法1：如果有關聯的進貨明細，直接從中取得倉庫ID
                            if (detail.PurchaseReceivingDetailId.HasValue)
                            {
                                var receivingDetail = await context.PurchaseReceivingDetails
                                    .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReceivingDetailId.Value);
                                warehouseId = receivingDetail?.WarehouseId;
                            }
                            
                            // 方法2：如果沒有進貨明細關聯，嘗試從倉庫位置反查
                            if (!warehouseId.HasValue && detail.WarehouseLocationId.HasValue)
                            {
                                var warehouseLocation = await context.WarehouseLocations
                                    .FirstOrDefaultAsync(wl => wl.Id == detail.WarehouseLocationId.Value);
                                warehouseId = warehouseLocation?.WarehouseId;
                            }

                            // 如果還是沒有倉庫ID，跳過此明細並記錄警告
                            if (!warehouseId.HasValue)
                            {
                                continue;
                            }

                            // 根據數量差異進行庫存調整
                            ServiceResult stockResult;
                            string operationDescription;
                            
                            if (quantityDiff > 0)
                            {
                                // 退貨數量增加，需要減少庫存
                                operationDescription = $"採購退貨增量 - {savedEntity.Code} (商品ID: {detail.ProductId})";
                                stockResult = await _inventoryStockService.ReduceStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    quantityDiff,
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.Code ?? string.Empty,
                                    detail.WarehouseLocationId,
                                    operationDescription,
                                    sourceDocumentType: InventorySourceDocumentTypes.PurchaseReturn,
                                    sourceDocumentId: savedEntity.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust,
                                    operationNote: $"編輯調整 - 退貨數量增加 {quantityDiff}"
                                );
                            }
                            else
                            {
                                // 退貨數量減少，需要增加庫存 (撤銷部分退貨)
                                operationDescription = $"採購退貨撤銷 - {savedEntity.Code} (商品ID: {detail.ProductId})";
                                stockResult = await _inventoryStockService.AddStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    Math.Abs(quantityDiff),
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.Code ?? string.Empty,
                                    detail.OriginalUnitPrice,
                                    detail.WarehouseLocationId,
                                    operationDescription,
                                    null, null, null, // batchNumber, batchDate, expiryDate
                                    sourceDocumentType: InventorySourceDocumentTypes.PurchaseReturn,
                                    sourceDocumentId: savedEntity.Id,
                                    operationType: InventoryOperationTypeEnum.Adjust,
                                    operationNote: $"編輯調整 - 退貨數量減少 {Math.Abs(quantityDiff)}"
                                );
                            }

                            if (!stockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult<PurchaseReturn>.Failure($"更新庫存失敗：{stockResult.ErrorMessage}");
                            }
                        }
                    }

                    // 計算並更新總金額
                    var calculateResult = await CalculateTotalsInContext(context, savedEntity.Id);
                    if (!calculateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<PurchaseReturn>.Failure($"計算總金額失敗：{calculateResult.ErrorMessage}");
                    }

                    await transaction.CommitAsync();
                    return ServiceResult<PurchaseReturn>.Success(savedEntity);
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
                    PurchaseReturnId = purchaseReturn.Id 
                });
                return ServiceResult<PurchaseReturn>.Failure($"儲存採購退貨時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 在指定的 DbContext 中更新採購退貨明細
        /// </summary>
        /// <returns>ServiceResult，其中Data包含數量變更資訊的列表</returns>
        private async Task<ServiceResult<List<(PurchaseReturnDetail detail, int quantityDifference)>>> UpdateDetailsInContext(AppDbContext context, int purchaseReturnId, List<PurchaseReturnDetail> details)
        {
            try
            {
                // 取得現有的明細記錄
                var existingDetails = await context.PurchaseReturnDetails
                    .Where(d => d.PurchaseReturnId == purchaseReturnId)
                    .ToListAsync();

                // 準備新的明細資料和數量變更追蹤
                var newDetailsToAdd = new List<PurchaseReturnDetail>();
                var updatedDetailsToUpdate = new List<(PurchaseReturnDetail existing, PurchaseReturnDetail updated)>();
                var quantityChanges = new List<(PurchaseReturnDetail detail, int quantityDifference)>();

                foreach (var detail in details.AsQueryable())
                {
                    detail.PurchaseReturnId = purchaseReturnId;

                    if (detail.Id > 0)
                    {
                        // 更新現有明細 - 檢查商品變更和數量變更
                        var existingDetail = existingDetails.FirstOrDefault(ed => ed.Id == detail.Id);
                        if (existingDetail != null)
                        {
                            // 檢查是否有商品變更（關鍵修正點）
                            bool productChanged = existingDetail.ProductId != detail.ProductId || 
                                                 existingDetail.PurchaseReceivingDetailId != detail.PurchaseReceivingDetailId;
                            
                            if (productChanged)
                            {
                                // 商品變更：需要完整的庫存回滾和重新扣減
                                // 1. 創建原商品的庫存回滾記錄（使用原始資料）
                                if (existingDetail.ReturnQuantity > 0)
                                {
                                    var originalProductDetail = new PurchaseReturnDetail
                                    {
                                        Id = existingDetail.Id,
                                        ProductId = existingDetail.ProductId, // 保持原商品ID
                                        PurchaseReceivingDetailId = existingDetail.PurchaseReceivingDetailId, // 保持原進貨明細ID
                                        WarehouseLocationId = existingDetail.WarehouseLocationId,
                                        ReturnQuantity = existingDetail.ReturnQuantity,
                                        OriginalUnitPrice = existingDetail.OriginalUnitPrice
                                    };
                                    
                                    quantityChanges.Add((originalProductDetail, -existingDetail.ReturnQuantity));
                                }
                                
                                // 2. 扣減新商品的庫存（減少新的退回數量）
                                if (detail.ReturnQuantity > 0)
                                {
                                    quantityChanges.Add((detail, detail.ReturnQuantity));
                                }
                            }
                            else
                            {
                                // 只有數量變更：計算差異
                                var quantityDiff = detail.ReturnQuantity - existingDetail.ReturnQuantity;
                                if (quantityDiff != 0)
                                {
                                    quantityChanges.Add((detail, quantityDiff));
                                }
                            }
                            
                            updatedDetailsToUpdate.Add((existingDetail, detail));
                        }
                    }
                    else
                    {
                        // 新增明細 - 整個數量都是新增
                        detail.CreatedAt = DateTime.UtcNow;
                        detail.UpdatedAt = DateTime.UtcNow;
                        newDetailsToAdd.Add(detail);
                        
                        // 新增的明細，數量差異就是全部數量
                        quantityChanges.Add((detail, detail.ReturnQuantity));
                    }
                }

                // 標記刪除的明細 - 追蹤被刪除的數量
                var detailIdsToKeep = details.Where(d => d.Id > 0).Select(d => d.Id).ToList();
                var detailsToDelete = existingDetails.Where(ed => !detailIdsToKeep.Contains(ed.Id)).ToList();
                
                foreach (var detailToDelete in detailsToDelete)
                {
                    // 被刪除的明細，數量差異是負的原數量 (撤銷退貨)
                    quantityChanges.Add((detailToDelete, -detailToDelete.ReturnQuantity));
                    
                    // 實際從資料庫刪除明細（硬刪除）
                    context.PurchaseReturnDetails.Remove(detailToDelete);
                }

                foreach (var (existing, updated) in updatedDetailsToUpdate)
                {
                    updated.CreatedAt = existing.CreatedAt; // 保持原建立時間
                    updated.UpdatedAt = DateTime.UtcNow;
                    context.Entry(existing).CurrentValues.SetValues(updated);
                }

                if (newDetailsToAdd.Any())
                {
                    await context.PurchaseReturnDetails.AddRangeAsync(newDetailsToAdd);
                }

                // 🔥 更新進貨明細的累計退貨數量
                foreach (var (detail, quantityDiff) in quantityChanges)
                {
                    if (detail.PurchaseReceivingDetailId.HasValue && detail.PurchaseReceivingDetailId.Value > 0)
                    {
                        var receivingDetail = await context.PurchaseReceivingDetails
                            .FirstOrDefaultAsync(rd => rd.Id == detail.PurchaseReceivingDetailId.Value);
                        
                        if (receivingDetail != null)
                        {
                            // 累加退貨數量（quantityDiff 可能為正或負）
                            receivingDetail.TotalReturnedQuantity += quantityDiff;
                            
                            // 確保不會變成負數
                            if (receivingDetail.TotalReturnedQuantity < 0)
                            {
                                receivingDetail.TotalReturnedQuantity = 0;
                            }
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult<List<(PurchaseReturnDetail detail, int quantityDifference)>>.Success(quantityChanges);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsInContext), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsInContext),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId 
                });
                return ServiceResult<List<(PurchaseReturnDetail detail, int quantityDifference)>>.Failure($"更新明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新採購退貨明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int purchaseReturnId, List<PurchaseReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await UpdateDetailsInContext(context, purchaseReturnId, details);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId 
                });
                return ServiceResult.Failure($"更新採購退貨明細時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查採購退貨單是否可以被刪除
        /// 
        /// 檢查項目：
        /// 1. 基類檢查（外鍵關聯等）
        /// 2. 檢查明細是否有收款記錄
        ///    - 檢查欄位：TotalReceivedAmount
        ///    - 限制原因：已收款的退貨明細不可刪除，避免財務資料錯亂
        /// 
        /// 任一明細被鎖定則整個主檔無法刪除
        /// </summary>
        /// <param name="entity">要檢查的採購退貨單實體</param>
        /// <returns>檢查結果，包含是否可刪除及錯誤訊息</returns>
        protected override async Task<ServiceResult> CanDeleteAsync(PurchaseReturn entity)
        {
            try
            {
                // 1. 先檢查基類的刪除條件（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                {
                    return baseResult;
                }

                // 2. 載入明細資料（如果尚未載入）
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var loadedEntity = await context.PurchaseReturns
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.Product)
                    .FirstOrDefaultAsync(pr => pr.Id == entity.Id);

                if (loadedEntity == null)
                {
                    return ServiceResult.Failure("找不到要檢查的退貨單");
                }

                // 如果沒有明細，可以刪除
                if (loadedEntity.PurchaseReturnDetails == null || !loadedEntity.PurchaseReturnDetails.Any())
                {
                    return ServiceResult.Success();
                }

                // 3. 檢查每個明細項目是否有收款記錄
                foreach (var detail in loadedEntity.PurchaseReturnDetails)
                {
                    // 檢查收款記錄
                    if (detail.TotalReceivedAmount > 0)
                    {
                        var productName = detail.Product?.Name ?? "未知商品";
                        return ServiceResult.Failure(
                            $"無法刪除此退貨單，因為商品「{productName}」已有收款記錄（已收款 {detail.TotalReceivedAmount:N0} 元）"
                        );
                    }
                }

                // 4. 所有檢查通過，允許刪除
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(CanDeleteAsync), GetType(), _logger,
                    new { EntityId = entity.Id, PurchaseReturnNumber = entity.Code }
                );
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        /// <summary>
        /// 永久刪除採購退貨單（含庫存回復）
        /// 刪除退貨單時，需要將之前因退貨而扣減的庫存回復到退貨前的狀態
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 取得要刪除的退貨單（含明細資料）
                    var entity = await context.PurchaseReturns
                        .Include(pr => pr.PurchaseReturnDetails)
                            .ThenInclude(prd => prd.PurchaseReceivingDetail)
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的退貨單");
                    }
                    
                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        return canDeleteResult;
                    }
                    
                    // 3. 回復庫存 - 將之前因退貨而扣減的庫存回復
                    if (_inventoryStockService != null)
                    {
                        var eligibleDetails = entity.PurchaseReturnDetails.Where(d => d.ReturnQuantity > 0).ToList();
                        
                        foreach (var detail in eligibleDetails)
                        {
                            // 從關聯的進貨明細取得倉庫ID
                            int? warehouseId = null;
                            
                            // 方法1：從關聯的進貨明細取得倉庫ID
                            if (detail.PurchaseReceivingDetailId.HasValue)
                            {
                                var receivingDetail = await context.PurchaseReceivingDetails
                                    .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReceivingDetailId.Value);
                                warehouseId = receivingDetail?.WarehouseId;
                            }
                            
                            // 方法2：如果沒有進貨明細關聯，嘗試從倉庫位置反查
                            if (!warehouseId.HasValue && detail.WarehouseLocationId.HasValue)
                            {
                                var warehouseLocation = await context.WarehouseLocations
                                    .FirstOrDefaultAsync(wl => wl.Id == detail.WarehouseLocationId.Value);
                                warehouseId = warehouseLocation?.WarehouseId;
                            }

                            // 如果還是沒有倉庫ID，跳過此明細並記錄警告
                            if (!warehouseId.HasValue)
                            {
                                continue;
                            }

                            // 刪除退貨單時需要增加庫存（回復之前扣減的數量）
                            var addResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                warehouseId.Value,
                                detail.ReturnQuantity, // 回復退貨的數量
                                InventoryTransactionTypeEnum.Return,
                                entity.Code ?? string.Empty, // 使用原始單號
                                detail.OriginalUnitPrice, // 使用原始單價
                                detail.WarehouseLocationId,
                                $"刪除採購退貨單回復庫存 - {entity.Code}",
                                null, null, null, // batchNumber, batchDate, expiryDate
                                sourceDocumentType: InventorySourceDocumentTypes.PurchaseReturn,
                                sourceDocumentId: entity.Id,
                                operationType: InventoryOperationTypeEnum.Delete,
                                operationNote: "刪除單據回復庫存"
                            );

                            if (!addResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"回復庫存失敗：{addResult.ErrorMessage}");
                            }
                            
                            // 🔥 回退進貨明細的累計退貨數量
                            if (detail.PurchaseReceivingDetailId.HasValue)
                            {
                                var receivingDetail = await context.PurchaseReceivingDetails
                                    .FirstOrDefaultAsync(rd => rd.Id == detail.PurchaseReceivingDetailId.Value);
                                
                                if (receivingDetail != null)
                                {
                                    receivingDetail.TotalReturnedQuantity -= detail.ReturnQuantity;
                                    
                                    // 確保不會變成負數
                                    if (receivingDetail.TotalReturnedQuantity < 0)
                                    {
                                        receivingDetail.TotalReturnedQuantity = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                    }

                    // 4. 執行實體刪除
                    context.PurchaseReturns.Remove(entity);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure($"刪除退貨單時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 確認採購退回單並更新庫存（首次新增時使用）
        /// 功能：執行退回確認流程，將退回數量從庫存扣除（因為商品退還給供應商）
        /// 處理流程：
        /// 1. 驗證退回單存在性
        /// 2. 對每個明細進行庫存扣減操作
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
                    var purchaseReturn = await context.PurchaseReturns
                        .Include(pr => pr.PurchaseReturnDetails)
                            .ThenInclude(prd => prd.PurchaseReceivingDetail)
                        .FirstOrDefaultAsync(pr => pr.Id == id);
                    
                    if (purchaseReturn == null)
                        return ServiceResult.Failure("找不到指定的退回單");
                    
                    // 更新庫存 - 退回會減少庫存（商品退還給供應商）
                    foreach (var detail in purchaseReturn.PurchaseReturnDetails)
                    {
                        if (detail.ReturnQuantity > 0)
                        {
                            // 從關聯的進貨明細取得倉庫ID
                            int? warehouseId = null;
                            
                            if (detail.PurchaseReceivingDetailId.HasValue)
                            {
                                var receivingDetail = await context.PurchaseReceivingDetails
                                    .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReceivingDetailId.Value);
                                warehouseId = receivingDetail?.WarehouseId;
                            }
                            
                            // 如果沒有進貨明細關聯，嘗試從倉庫位置反查
                            if (!warehouseId.HasValue && detail.WarehouseLocationId.HasValue)
                            {
                                var warehouseLocation = await context.WarehouseLocations
                                    .FirstOrDefaultAsync(wl => wl.Id == detail.WarehouseLocationId.Value);
                                warehouseId = warehouseLocation?.WarehouseId;
                            }

                            // 如果還是沒有倉庫ID，跳過此明細
                            if (!warehouseId.HasValue)
                            {
                                continue;
                            }

                            if (_inventoryStockService != null)
                            {
                                var reduceStockResult = await _inventoryStockService.ReduceStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    detail.ReturnQuantity,
                                    InventoryTransactionTypeEnum.Return,
                                    purchaseReturn.Code ?? string.Empty,
                                    detail.WarehouseLocationId,
                                    $"採購退回確認 - {purchaseReturn.Code ?? string.Empty}",
                                    sourceDocumentType: InventorySourceDocumentTypes.PurchaseReturn,
                                    sourceDocumentId: purchaseReturn.Id,
                                    operationType: InventoryOperationTypeEnum.Initial,
                                    operationNote: "首次確認退回"
                                    );
                                
                                if (!reduceStockResult.IsSuccess)
                                {
                                    await transaction.RollbackAsync();
                                    return ServiceResult.Failure($"庫存扣減失敗：{reduceStockResult.ErrorMessage}");
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
    }
}

