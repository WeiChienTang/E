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
            ILogger<GenericManagementService<PurchaseReturn>> logger) : base(contextFactory, logger)
        {
        }

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
                    .Where(pr => !pr.IsDeleted)
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
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);
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
                    .Where(pr => pr.SupplierId == supplierId && !pr.IsDeleted)
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
                    .Where(pr => pr.ReturnDate >= startDate && pr.ReturnDate <= endDate && !pr.IsDeleted)
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
                    .Where(pr => pr.PurchaseReceivingId == purchaseReceivingId && !pr.IsDeleted)
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
                var query = context.PurchaseReturns.Where(pr => pr.PurchaseReturnNumber == purchaseReturnNumber && !pr.IsDeleted);
                
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
                    .Where(pr => !pr.IsDeleted &&
                        (pr.PurchaseReturnNumber.Contains(searchTerm) ||
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
                var supplierExists = await context.Suppliers.AnyAsync(s => s.Id == entity.SupplierId && !s.IsDeleted);
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
                var purchaseReturn = await context.PurchaseReturns
                    .Include(pr => pr.PurchaseReturnDetails)
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);

                if (purchaseReturn == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "找不到指定的採購退回記錄";
                    return result;
                }

                // 計算總金額
                purchaseReturn.TotalReturnAmount = purchaseReturn.PurchaseReturnDetails
                    .Where(prd => !prd.IsDeleted)
                    .Sum(prd => prd.ReturnQuantity * prd.ReturnUnitPrice);

                // 計算稅額 (假設稅率為 5%)
                purchaseReturn.ReturnTaxAmount = purchaseReturn.TotalReturnAmount * 0.05m;

                // 計算含稅總金額
                purchaseReturn.TotalReturnAmountWithTax = purchaseReturn.TotalReturnAmount + purchaseReturn.ReturnTaxAmount;

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

        public async Task<ServiceResult> RefundProcessAsync(int id, decimal refundAmount)
        {
            var result = new ServiceResult();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);

                if (purchaseReturn == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "找不到指定的採購退回記錄";
                    return result;
                }

                purchaseReturn.IsRefunded = true;
                purchaseReturn.RefundDate = DateTime.Now;

                await context.SaveChangesAsync();

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RefundProcessAsync), GetType(), _logger, new { 
                    Method = nameof(RefundProcessAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    RefundAmount = refundAmount
                });
                result.IsSuccess = false;
                result.ErrorMessage = "退款處理時發生錯誤";
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
                        .FirstOrDefaultAsync(pr => pr.Id == purchaseReceivingId && !pr.IsDeleted);

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
                
                var query = context.PurchaseReturns.Where(pr => !pr.IsDeleted);

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
                    .Where(pr => pr.SupplierId == supplierId && !pr.IsDeleted);

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
                            .FirstOrDefaultAsync(x => x.Id == purchaseReturn.Id && !x.IsDeleted);
                            
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
                        purchaseReturn.IsDeleted = false;
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

                    // 更新庫存邏輯 - 退貨需要減少庫存
                    if (_inventoryStockService != null)
                    {
                        foreach (var detail in details.Where(d => !d.IsDeleted && d.ReturnQuantity > 0))
                        {
                            // 取得倉庫ID
                            int? warehouseId = null;
                            if (detail.WarehouseLocationId.HasValue)
                            {
                                var warehouseLocation = await context.WarehouseLocations
                                    .FirstOrDefaultAsync(wl => wl.Id == detail.WarehouseLocationId.Value);
                                warehouseId = warehouseLocation?.WarehouseId;
                            }

                            // 如果沒有倉庫ID，跳過此明細
                            if (!warehouseId.HasValue)
                                continue;

                            // 退貨需要減少庫存，所以使用負數量
                            var stockResult = await _inventoryStockService.AddStockAsync(
                                detail.ProductId,
                                warehouseId.Value,
                                -detail.ReturnQuantity, // 負數量表示減少庫存
                                InventoryTransactionTypeEnum.Return,
                                savedEntity.PurchaseReturnNumber,
                                detail.ReturnUnitPrice,
                                detail.WarehouseLocationId,
                                $"採購退貨 - {savedEntity.PurchaseReturnNumber}"
                            );

                            if (!stockResult.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return ServiceResult<PurchaseReturn>.Failure($"更新庫存失敗：{stockResult.ErrorMessage}");
                            }
                        }
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
        private async Task<ServiceResult> UpdateDetailsInContext(AppDbContext context, int purchaseReturnId, List<PurchaseReturnDetail> details)
        {
            try
            {
                // 取得現有的明細記錄
                var existingDetails = await context.PurchaseReturnDetails
                    .Where(d => d.PurchaseReturnId == purchaseReturnId && !d.IsDeleted)
                    .ToListAsync();

                // 準備新的明細資料
                var newDetailsToAdd = new List<PurchaseReturnDetail>();
                var updatedDetailsToUpdate = new List<(PurchaseReturnDetail existing, PurchaseReturnDetail updated)>();

                foreach (var detail in details.Where(d => !d.IsDeleted))
                {
                    detail.PurchaseReturnId = purchaseReturnId;

                    if (detail.Id > 0)
                    {
                        // 更新現有明細
                        var existingDetail = existingDetails.FirstOrDefault(ed => ed.Id == detail.Id);
                        if (existingDetail != null)
                        {
                            updatedDetailsToUpdate.Add((existingDetail, detail));
                        }
                    }
                    else
                    {
                        // 新增明細
                        detail.CreatedAt = DateTime.UtcNow;
                        detail.UpdatedAt = DateTime.UtcNow;
                        detail.IsDeleted = false;
                        newDetailsToAdd.Add(detail);
                    }
                }

                // 標記刪除的明細
                var detailIdsToKeep = details.Where(d => d.Id > 0 && !d.IsDeleted).Select(d => d.Id).ToList();
                var detailsToDelete = existingDetails.Where(ed => !detailIdsToKeep.Contains(ed.Id)).ToList();

                // 執行資料庫操作
                foreach (var detailToDelete in detailsToDelete)
                {
                    detailToDelete.IsDeleted = true;
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
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsInContext), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsInContext),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId 
                });
                return ServiceResult.Failure($"更新明細時發生錯誤：{ex.Message}");
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
    }


}
