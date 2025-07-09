using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Inventory
{
    /// <summary>
    /// 庫存異動服務實作
    /// </summary>
    public class InventoryTransactionService : GenericManagementService<InventoryTransaction>, IInventoryTransactionService
    {
        public InventoryTransactionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public InventoryTransactionService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<InventoryTransaction>> logger) : base(contextFactory, logger)
        {
        }

        #region 基本查詢

        public async Task<List<InventoryTransaction>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.ProductId == productId && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.WarehouseId == warehouseId && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByTransactionNumberAsync(string transactionNumber)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.TransactionNumber == transactionNumber && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTransactionNumberAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByTypeAsync(InventoryTransactionTypeEnum transactionType)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.TransactionType == transactionType && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTypeAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.ProductId == productId && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate && 
                               !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductAndDateRangeAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<List<InventoryTransaction>> GetByWarehouseAndDateRangeAsync(int warehouseId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.WarehouseId == warehouseId && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate && 
                               !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseAndDateRangeAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<InventoryTransaction?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdWithDetailsAsync), typeof(InventoryTransactionService), _logger);
                return null;
            }
        }

        #endregion

        #region 統計查詢

        public async Task<decimal> GetTotalInboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.ProductId == productId && 
                                             t.Quantity > 0 && 
                                             !t.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value);

                return await query.SumAsync(t => t.Quantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalInboundByProductAsync), typeof(InventoryTransactionService), _logger);
                return 0;
            }
        }

        public async Task<decimal> GetTotalOutboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.ProductId == productId && 
                                             t.Quantity < 0 && 
                                             !t.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value);

                return await query.SumAsync(t => Math.Abs(t.Quantity));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalOutboundByProductAsync), typeof(InventoryTransactionService), _logger);
                return 0;
            }
        }

        public async Task<Dictionary<InventoryTransactionTypeEnum, int>> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Where(t => t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate && 
                               !t.IsDeleted)
                    .GroupBy(t => t.TransactionType)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTransactionSummaryAsync), typeof(InventoryTransactionService), _logger);
                return new Dictionary<InventoryTransactionTypeEnum, int>();
            }
        }

        #endregion

        #region 庫存異動記錄

        public async Task<ServiceResult> CreateInboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            decimal? unitCost = null, int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("入庫數量必須大於0");

                var transaction = new InventoryTransaction
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = locationId,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = remarks,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var validationResult = await ValidateTransactionAsync(transaction);
                if (!validationResult.IsSuccess)
                    return validationResult;

                await _dbSet.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateInboundTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("建立入庫異動記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateOutboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("出庫數量必須大於0");

                var transaction = new InventoryTransaction
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = locationId,
                    TransactionType = transactionType,
                    TransactionNumber = transactionNumber,
                    Quantity = -quantity, // 出庫為負數
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = remarks,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var validationResult = await ValidateTransactionAsync(transaction);
                if (!validationResult.IsSuccess)
                    return validationResult;

                await _dbSet.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateOutboundTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("建立出庫異動記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateAdjustmentTransactionAsync(int productId, int warehouseId, 
            int originalQuantity, int adjustedQuantity, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            try
            {
                var adjustmentQuantity = adjustedQuantity - originalQuantity;
                
                if (adjustmentQuantity == 0)
                    return ServiceResult.Failure("調整數量不能為0");

                var transaction = new InventoryTransaction
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = locationId,
                    TransactionType = InventoryTransactionTypeEnum.Adjustment,
                    TransactionNumber = transactionNumber,
                    Quantity = adjustmentQuantity,
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = remarks ?? $"庫存調整：{originalQuantity} → {adjustedQuantity}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var validationResult = await ValidateTransactionAsync(transaction);
                if (!validationResult.IsSuccess)
                    return validationResult;

                await _dbSet.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAdjustmentTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("建立庫存調整記錄時發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateTransferTransactionAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null, int? employeeId = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("調撥數量必須大於0");

                if (fromWarehouseId == toWarehouseId)
                    return ServiceResult.Failure("來源倉庫與目標倉庫不能相同");

                // 建立出庫異動記錄
                var outboundTransaction = new InventoryTransaction
                {
                    ProductId = productId,
                    WarehouseId = fromWarehouseId,
                    WarehouseLocationId = fromLocationId,
                    TransactionType = InventoryTransactionTypeEnum.Transfer,
                    TransactionNumber = transactionNumber,
                    Quantity = -quantity,
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = remarks ?? $"調撥至倉庫ID:{toWarehouseId}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // 建立入庫異動記錄
                var inboundTransaction = new InventoryTransaction
                {
                    ProductId = productId,
                    WarehouseId = toWarehouseId,
                    WarehouseLocationId = toLocationId,
                    TransactionType = InventoryTransactionTypeEnum.Transfer,
                    TransactionNumber = transactionNumber,
                    Quantity = quantity,
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = remarks ?? $"從倉庫ID:{fromWarehouseId}調撥",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var outValidation = await ValidateTransactionAsync(outboundTransaction);
                if (!outValidation.IsSuccess)
                    return outValidation;

                var inValidation = await ValidateTransactionAsync(inboundTransaction);
                if (!inValidation.IsSuccess)
                    return inValidation;

                await _dbSet.AddRangeAsync(outboundTransaction, inboundTransaction);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateTransferTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("建立庫存調撥記錄時發生錯誤");
            }
        }

        #endregion

        #region 庫存流水追蹤

        public async Task<List<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, int? warehouseId = null)
        {
            try
            {
                var query = _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => t.ProductId == productId && !t.IsDeleted);

                if (warehouseId.HasValue)
                    query = query.Where(t => t.WarehouseId == warehouseId.Value);

                return await query
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetProductMovementHistoryAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public async Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reason, int? employeeId = null)
        {
            try
            {
                var originalTransaction = await GetByIdAsync(transactionId);
                if (originalTransaction == null)
                    return ServiceResult.Failure("找不到指定的異動記錄");

                if (originalTransaction.IsDeleted)
                    return ServiceResult.Failure("該異動記錄已被刪除");

                var reverseTransaction = new InventoryTransaction
                {
                    ProductId = originalTransaction.ProductId,
                    WarehouseId = originalTransaction.WarehouseId,
                    WarehouseLocationId = originalTransaction.WarehouseLocationId,
                    TransactionType = InventoryTransactionTypeEnum.Adjustment, // 用調整類型來表示沖銷
                    TransactionNumber = $"REV-{originalTransaction.TransactionNumber}",
                    Quantity = -originalTransaction.Quantity, // 反向數量
                    TransactionDate = DateTime.Now,
                    TransactionRemarks = $"沖銷異動ID:{transactionId} - {reason}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var validationResult = await ValidateTransactionAsync(reverseTransaction);
                if (!validationResult.IsSuccess)
                    return validationResult;

                await _dbSet.AddAsync(reverseTransaction);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReverseTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("沖銷異動記錄時發生錯誤");
            }
        }

        #endregion

        #region 驗證方法

        public override async Task<ServiceResult> ValidateAsync(InventoryTransaction transaction)
        {
            return await ValidateTransactionAsync(transaction);
        }

        public async Task<ServiceResult> ValidateTransactionAsync(InventoryTransaction transaction)
        {
            try
            {
                var errors = new List<string>();

                if (transaction.ProductId <= 0)
                    errors.Add("產品ID不能為空");

                if (transaction.WarehouseId <= 0)
                    errors.Add("倉庫ID不能為空");

                if (string.IsNullOrWhiteSpace(transaction.TransactionNumber))
                    errors.Add("異動單號不能為空");

                if (transaction.Quantity == 0)
                    errors.Add("異動數量不能為0");

                // 檢查產品是否存在
                var productExists = await _context.Products.AnyAsync(p => p.Id == transaction.ProductId && !p.IsDeleted);
                if (!productExists)
                    errors.Add("指定的產品不存在");

                // 檢查倉庫是否存在
                var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == transaction.WarehouseId && !w.IsDeleted);
                if (!warehouseExists)
                    errors.Add("指定的倉庫不存在");

                // 檢查倉庫位置是否存在（如果有指定）
                if (transaction.WarehouseLocationId.HasValue)
                {
                    var locationExists = await _context.WarehouseLocations
                        .AnyAsync(l => l.Id == transaction.WarehouseLocationId.Value && 
                                      l.WarehouseId == transaction.WarehouseId && 
                                      !l.IsDeleted);
                    if (!locationExists)
                        errors.Add("指定的倉庫位置不存在或不屬於該倉庫");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateTransactionAsync), typeof(InventoryTransactionService), _logger);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<bool> IsTransactionNumberUniqueAsync(string transactionNumber, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.TransactionNumber == transactionNumber && !t.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(t => t.Id != excludeId.Value);

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTransactionNumberUniqueAsync), typeof(InventoryTransactionService), _logger);
                return false;
            }
        }

        #endregion

        #region Override Methods

        public override async Task<List<InventoryTransaction>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        public override async Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), typeof(InventoryTransactionService), _logger);
                return null;
            }
        }

        public override async Task<List<InventoryTransaction>> SearchAsync(string searchTerm)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Product)
                    .Include(t => t.Warehouse)
                    .Include(t => t.WarehouseLocation)
                    .Where(t => !t.IsDeleted && 
                               (t.TransactionNumber.Contains(searchTerm) ||
                                (t.TransactionRemarks != null && t.TransactionRemarks.Contains(searchTerm)) ||
                                (t.ReferenceNumber != null && t.ReferenceNumber.Contains(searchTerm))))
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        #endregion
    }
}

