using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動服務實作（主/明細結構）
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

        /// <summary>
        /// 根據商品ID查詢異動記錄（透過明細查詢）
        /// </summary>
        public async Task<List<InventoryTransaction>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(t => t.Details.Any(d => d.ProductId == productId))
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.WarehouseId == warehouseId)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.WarehouseLocation)
                    .Where(t => t.TransactionNumber == transactionNumber)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.TransactionType == transactionType)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.Details.Any(d => d.ProductId == productId) && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.WarehouseId == warehouseId && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdWithDetailsAsync), typeof(InventoryTransactionService), _logger);
                return null;
            }
        }
        
        /// <summary>
        /// 根據來源單據查詢異動記錄
        /// </summary>
        public async Task<List<InventoryTransaction>> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.SourceDocumentType == sourceDocumentType && 
                               t.SourceDocumentId == sourceDocumentId)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySourceDocumentAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransaction>();
            }
        }

        #endregion

        #region 統計查詢

        /// <summary>
        /// 取得商品總入庫量（透過明細彙總）
        /// </summary>
        public async Task<decimal> GetTotalInboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryTransactionDetails
                    .Include(d => d.InventoryTransaction)
                    .Where(d => d.ProductId == productId && d.Quantity > 0);

                if (startDate.HasValue)
                    query = query.Where(d => d.InventoryTransaction.TransactionDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(d => d.InventoryTransaction.TransactionDate <= endDate.Value);

                return await query.SumAsync(d => d.Quantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalInboundByProductAsync), typeof(InventoryTransactionService), _logger);
                return 0;
            }
        }

        /// <summary>
        /// 取得商品總出庫量（透過明細彙總）
        /// </summary>
        public async Task<decimal> GetTotalOutboundByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryTransactionDetails
                    .Include(d => d.InventoryTransaction)
                    .Where(d => d.ProductId == productId && d.Quantity < 0);

                if (startDate.HasValue)
                    query = query.Where(d => d.InventoryTransaction.TransactionDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(d => d.InventoryTransaction.TransactionDate <= endDate.Value);

                return await query.SumAsync(d => Math.Abs(d.Quantity));
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Where(t => t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
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

        #region 庫存異動記錄（已過時，建議使用 InventoryStockService）

        [Obsolete("請使用 IInventoryStockService.AddStockAsync")]
        public async Task<ServiceResult> CreateInboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            decimal? unitCost = null, int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            // 此方法已過時，應使用 IInventoryStockService.AddStockAsync
            return ServiceResult.Failure("此方法已過時，請使用 IInventoryStockService.AddStockAsync");
        }

        [Obsolete("請使用 IInventoryStockService.ReduceStockAsync")]
        public async Task<ServiceResult> CreateOutboundTransactionAsync(int productId, int warehouseId, int quantity,
            InventoryTransactionTypeEnum transactionType, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            // 此方法已過時，應使用 IInventoryStockService.ReduceStockAsync
            return ServiceResult.Failure("此方法已過時，請使用 IInventoryStockService.ReduceStockAsync");
        }

        [Obsolete("請使用 IInventoryStockService.AdjustStockAsync")]
        public async Task<ServiceResult> CreateAdjustmentTransactionAsync(int productId, int warehouseId, 
            decimal originalQuantity, decimal adjustedQuantity, string transactionNumber,
            int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            // 此方法已過時，應使用 IInventoryStockService.AdjustStockAsync
            return ServiceResult.Failure("此方法已過時，請使用 IInventoryStockService.AdjustStockAsync");
        }

        [Obsolete("請使用 IInventoryStockService.TransferStockAsync")]
        public async Task<ServiceResult> CreateTransferTransactionAsync(int productId, int fromWarehouseId, int toWarehouseId,
            int quantity, string transactionNumber, int? fromLocationId = null, int? toLocationId = null,
            string? remarks = null, int? employeeId = null)
        {
            // 此方法已過時，應使用 IInventoryStockService.TransferStockAsync
            return ServiceResult.Failure("此方法已過時，請使用 IInventoryStockService.TransferStockAsync");
        }

        #endregion

        #region 庫存流水追蹤

        /// <summary>
        /// 取得商品的異動歷史（透過明細查詢）
        /// </summary>
        public async Task<List<InventoryTransactionDetail>> GetProductMovementHistoryDetailsAsync(int productId, int? warehouseId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryTransactionDetails
                    .Include(d => d.InventoryTransaction)
                        .ThenInclude(t => t.Warehouse)
                    .Include(d => d.Product)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.ProductId == productId);

                if (warehouseId.HasValue)
                    query = query.Where(d => d.InventoryTransaction.WarehouseId == warehouseId.Value);

                return await query
                    .OrderByDescending(d => d.InventoryTransaction.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetProductMovementHistoryDetailsAsync), typeof(InventoryTransactionService), _logger);
                return new List<InventoryTransactionDetail>();
            }
        }

        /// <summary>
        /// 取得商品的異動歷史（主檔層級）
        /// </summary>
        public async Task<List<InventoryTransaction>> GetProductMovementHistoryAsync(int productId, int? warehouseId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => t.Details.Any(d => d.ProductId == productId));

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

        /// <summary>
        /// 取得關聯的庫存異動記錄（原始交易 + 調整記錄）
        /// 用於顯示一張單據相關的所有庫存異動
        /// </summary>
        /// <param name="baseTransactionNumber">基礎交易編號（不含 _ADJ、_DEL 等後綴）</param>
        /// <returns>包含原始交易和所有調整記錄的 RelatedDocument 列表</returns>
        public async Task<List<ERPCore2.Models.RelatedDocument>> GetRelatedTransactionsAsync(string baseTransactionNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(baseTransactionNumber))
                    return new List<ERPCore2.Models.RelatedDocument>();

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 提取基礎編號（移除可能的後綴）
                var cleanBaseNumber = baseTransactionNumber
                    .Replace("_ADJ", "")
                    .Replace("_DEL", "")
                    .Replace("_PRICE_ADJ_IN", "")
                    .Replace("_PRICE_ADJ_OUT", "");
                
                // 查詢所有相關的異動記錄（原始 + 調整 + 刪除）
                var relatedTransactions = await context.InventoryTransactions
                    .Include(t => t.Details)
                    .Where(t => t.TransactionNumber == cleanBaseNumber ||
                               t.TransactionNumber.StartsWith(cleanBaseNumber + "_"))
                    .OrderBy(t => t.TransactionDate)
                    .ToListAsync();
                
                // 轉換為 RelatedDocument 格式
                var documents = new List<ERPCore2.Models.RelatedDocument>();
                
                foreach (var transaction in relatedTransactions)
                {
                    // 判斷是原始記錄還是調整記錄
                    var isOriginal = transaction.TransactionNumber == cleanBaseNumber;
                    var isAdjustment = transaction.TransactionNumber.Contains("_ADJ");
                    var isDelete = transaction.TransactionNumber.Contains("_DEL");
                    var isPriceAdjust = transaction.TransactionNumber.Contains("_PRICE_ADJ");
                    
                    // 計算該交易的淨數量
                    var netQuantity = transaction.Details?.Sum(d => d.Quantity) ?? transaction.TotalQuantity;
                    var netAmount = transaction.Details?.Sum(d => d.Amount) ?? transaction.TotalAmount;
                    
                    // 根據類型設定標籤
                    string label;
                    if (isOriginal)
                        label = $"[原始] {GetTransactionTypeDisplayName(transaction.TransactionType)}";
                    else if (isDelete)
                        label = "[刪除回退]";
                    else if (isPriceAdjust)
                        label = "[價格調整]";
                    else if (isAdjustment)
                        label = "[編輯調整]";
                    else
                        label = transaction.TransactionType.ToString();
                    
                    documents.Add(new ERPCore2.Models.RelatedDocument
                    {
                        DocumentId = transaction.Id,
                        DocumentType = ERPCore2.Models.RelatedDocumentType.InventoryTransaction,
                        DocumentNumber = $"{transaction.TransactionNumber} {label}",
                        DocumentDate = transaction.TransactionDate,
                        Quantity = netQuantity,
                        Amount = netAmount,
                        Remarks = transaction.Remarks
                    });
                }
                
                return documents;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRelatedTransactionsAsync), typeof(InventoryTransactionService), _logger);
                return new List<ERPCore2.Models.RelatedDocument>();
            }
        }
        
        /// <summary>
        /// 取得交易類型的中文顯示名稱
        /// </summary>
        private string GetTransactionTypeDisplayName(InventoryTransactionTypeEnum transactionType)
        {
            return transactionType switch
            {
                InventoryTransactionTypeEnum.OpeningBalance => "期初庫存",
                InventoryTransactionTypeEnum.Purchase => "進貨",
                InventoryTransactionTypeEnum.Sale => "銷貨",
                InventoryTransactionTypeEnum.Return => "進貨退出",
                InventoryTransactionTypeEnum.SalesReturn => "銷貨退回",
                InventoryTransactionTypeEnum.Adjustment => "調整",
                InventoryTransactionTypeEnum.Transfer => "轉倉",
                InventoryTransactionTypeEnum.StockTaking => "盤點",
                InventoryTransactionTypeEnum.ProductionConsumption => "生產投料",
                InventoryTransactionTypeEnum.ProductionCompletion => "生產完工",
                InventoryTransactionTypeEnum.Scrap => "報廢",
                InventoryTransactionTypeEnum.MaterialIssue => "領料",
                InventoryTransactionTypeEnum.MaterialReturn => "領料退回",
                _ => transactionType.ToString()
            };
        }

        [Obsolete("沖銷功能需重新設計以支援主/明細結構")]
        public async Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reason, int? employeeId = null)
        {
            // 沖銷功能需要重新設計
            return ServiceResult.Failure("沖銷功能暫不支援，請聯繫系統管理員");
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

                if (transaction.WarehouseId <= 0)
                    errors.Add("倉庫ID不能為空");

                if (string.IsNullOrWhiteSpace(transaction.TransactionNumber))
                    errors.Add("異動單號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查倉庫是否存在
                var warehouseExists = await context.Warehouses.AnyAsync(w => w.Id == transaction.WarehouseId);
                if (!warehouseExists)
                    errors.Add("指定的倉庫不存在");

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
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.InventoryTransactions.Where(t => t.TransactionNumber == transactionNumber);
                
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .AsQueryable()
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(t => t.Id == id);
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
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .Include(t => t.Employee)
                    .Include(t => t.Details)
                        .ThenInclude(d => d.Product)
                    .Where(t => (t.TransactionNumber.Contains(searchTerm) ||
                                (t.Remarks != null && t.Remarks.Contains(searchTerm))))
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


