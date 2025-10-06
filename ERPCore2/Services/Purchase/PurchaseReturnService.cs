using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
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
                    .ThenBy(pr => pr.PurchaseReturnNumber)
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
                    .ThenBy(pr => pr.PurchaseReturnNumber)
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
                    .ThenBy(pr => pr.PurchaseReturnNumber)
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
                    .ThenBy(pr => pr.PurchaseReturnNumber)
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

        public async Task<bool> IsPurchaseReturnNumberExistsAsync(string purchaseReturnNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturns.Where(pr => pr.PurchaseReturnNumber == purchaseReturnNumber);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(pr => pr.Id != excludeId.Value);
                }
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReturnNumberExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseReturnNumberExistsAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnNumber = purchaseReturnNumber,
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
                    .Where(pr => (pr.PurchaseReturnNumber.Contains(searchTerm) ||
                         (pr.Supplier != null && pr.Supplier.CompanyName.Contains(searchTerm)) ||
                         (pr.PurchaseReceiving != null && pr.PurchaseReceiving.ReceiptNumber.Contains(searchTerm))))
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ThenBy(pr => pr.PurchaseReturnNumber)
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
                if (await IsPurchaseReturnNumberExistsAsync(entity.PurchaseReturnNumber, entity.Id > 0 ? entity.Id : null))
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

                // 計算總金額
                purchaseReturn.TotalReturnAmount = purchaseReturn.PurchaseReturnDetails
                    .AsQueryable()
                    .Sum(prd => prd.ReturnQuantity * prd.OriginalUnitPrice);

                // 計算稅額 (假設稅率為 5%)
                purchaseReturn.ReturnTaxAmount = purchaseReturn.TotalReturnAmount * 0.05m;

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
                        PurchaseReturnNumber = await GenerateReturnNumberAsync(context),
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
                    .Where(pr => pr.PurchaseReturnNumber.StartsWith(prefix))
                    .OrderByDescending(pr => pr.PurchaseReturnNumber)
                    .FirstOrDefaultAsync();

                if (lastReturn == null)
                {
                    return $"{prefix}001";
                }

                var lastNumber = lastReturn.PurchaseReturnNumber.Substring(prefix.Length);
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
                                _logger?.LogWarning("退貨明細 ID:{DetailId} 無法取得倉庫ID，跳過庫存更新", detail.Id);
                                continue;
                            }

                            // 根據數量差異進行庫存調整
                            ServiceResult stockResult;
                            string operationDescription;
                            
                            if (quantityDiff > 0)
                            {
                                // 退貨數量增加，需要減少庫存
                                operationDescription = $"採購退貨增量 - {savedEntity.PurchaseReturnNumber} (商品ID: {detail.ProductId})";
                                stockResult = await _inventoryStockService.ReduceStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    quantityDiff,
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.PurchaseReturnNumber,
                                    detail.WarehouseLocationId,
                                    operationDescription
                                );
                                
                                _logger?.LogInformation("執行庫存扣減 - 商品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量: {Quantity}", 
                                                      detail.ProductId, warehouseId.Value, quantityDiff);
                            }
                            else
                            {
                                // 退貨數量減少，需要增加庫存 (撤銷部分退貨)
                                operationDescription = $"採購退貨撤銷 - {savedEntity.PurchaseReturnNumber} (商品ID: {detail.ProductId})";
                                stockResult = await _inventoryStockService.AddStockAsync(
                                    detail.ProductId,
                                    warehouseId.Value,
                                    Math.Abs(quantityDiff),
                                    InventoryTransactionTypeEnum.Return,
                                    savedEntity.PurchaseReturnNumber,
                                    detail.OriginalUnitPrice,
                                    detail.WarehouseLocationId,
                                    operationDescription
                                );
                                
                                _logger?.LogInformation("執行庫存回復 - 商品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量: {Quantity}", 
                                                      detail.ProductId, warehouseId.Value, Math.Abs(quantityDiff));
                            }

                            if (!stockResult.IsSuccess)
                            {
                                _logger?.LogError("庫存更新失敗 - 商品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量差異: {QuantityDiff}, 錯誤: {Error}", 
                                                detail.ProductId, warehouseId.Value, quantityDiff, stockResult.ErrorMessage);
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
                                    _logger?.LogInformation("檢測到退貨明細商品變更 - 明細ID: {DetailId}, 原商品: {OldProductId}, 新商品: {NewProductId}, 回滾數量: {Quantity}", 
                                                          detail.Id, existingDetail.ProductId, detail.ProductId, existingDetail.ReturnQuantity);
                                }
                                
                                // 2. 扣減新商品的庫存（減少新的退回數量）
                                if (detail.ReturnQuantity > 0)
                                {
                                    quantityChanges.Add((detail, detail.ReturnQuantity));
                                    _logger?.LogInformation("商品變更後新增庫存扣減 - 明細ID: {DetailId}, 新商品: {ProductId}, 扣減數量: {Quantity}", 
                                                          detail.Id, detail.ProductId, detail.ReturnQuantity);
                                }
                            }
                            else
                            {
                                // 只有數量變更：計算差異
                                var quantityDiff = detail.ReturnQuantity - existingDetail.ReturnQuantity;
                                if (quantityDiff != 0)
                                {
                                    quantityChanges.Add((detail, quantityDiff));
                                    _logger?.LogInformation("退貨明細數量變更 - 明細ID: {DetailId}, 商品: {ProductId}, 數量差異: {QuantityDiff}", 
                                                          detail.Id, detail.ProductId, quantityDiff);
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
                    
                    detailToDelete.UpdatedAt = DateTime.UtcNow;
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
                    
                    _logger?.LogInformation("開始刪除退貨單: {ReturnNumber}, ID: {Id}", entity.PurchaseReturnNumber, entity.Id);
                    
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
                        _logger?.LogInformation("需要回復庫存的明細數量: {Count}", eligibleDetails.Count);
                        
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
                                _logger?.LogWarning("退貨明細 ID:{DetailId} 無法取得倉庫ID，跳過庫存回復", detail.Id);
                                continue;
                            }

                            _logger?.LogInformation("回復庫存 - 產品ID: {ProductId}, 倉庫ID: {WarehouseId}, 數量: {Quantity}", 
                                detail.ProductId, warehouseId.Value, detail.ReturnQuantity);

                            // 刪除退貨單時需要增加庫存（回復之前扣減的數量）
                            var addResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                warehouseId.Value,
                                detail.ReturnQuantity, // 回復退貨的數量
                                InventoryTransactionTypeEnum.Return,
                                $"{entity.PurchaseReturnNumber}_DEL", // 標記為刪除操作
                                detail.OriginalUnitPrice, // 使用原始單價
                                detail.WarehouseLocationId,
                                $"刪除採購退貨單回復庫存 - {entity.PurchaseReturnNumber}"
                            );

                            if (!addResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult.Failure($"回復庫存失敗：{addResult.ErrorMessage}");
                            }
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("庫存服務未注入，無法回復庫存");
                    }

                    // 4. 執行實體刪除
                    context.PurchaseReturns.Remove(entity);
                    await context.SaveChangesAsync();
                    
                    await transaction.CommitAsync();
                    
                    _logger?.LogInformation("成功刪除退貨單: {ReturnNumber}", entity.PurchaseReturnNumber);
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
    }
}

