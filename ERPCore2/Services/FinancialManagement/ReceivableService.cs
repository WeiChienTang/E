using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class ReceivableService : IReceivableService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ReceivableService> _logger;

        public ReceivableService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<ReceivableService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<ServiceResult<List<ReceivableViewModel>>> GetAllReceivablesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var salesReceivables = await GetSalesOrderReceivablesAsync(context);
                var purchaseReturnReceivables = await GetPurchaseReturnReceivablesAsync(context);

                var allReceivables = salesReceivables
                    .Concat(purchaseReturnReceivables)
                    .OrderByDescending(r => r.DocumentDate)
                    .ToList();

                return ServiceResult<List<ReceivableViewModel>>.Success(allReceivables);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllReceivablesAsync), GetType(), _logger);
                return ServiceResult<List<ReceivableViewModel>>.Failure($"取得所有應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ReceivableViewModel>>> GetUnSettledReceivablesAsync()
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                    return allResult;

                var unSettledReceivables = allResult.Data
                    .Where(r => r.BalanceAmount > 0)
                    .ToList();

                return ServiceResult<List<ReceivableViewModel>>.Success(unSettledReceivables);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnSettledReceivablesAsync), GetType(), _logger);
                return ServiceResult<List<ReceivableViewModel>>.Failure($"取得未結算應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ReceivableViewModel>>> GetOverdueReceivablesAsync()
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                    return allResult;

                var overdueReceivables = allResult.Data
                    .Where(r => r.IsOverdue)
                    .OrderByDescending(r => r.OverdueDays)
                    .ToList();

                return ServiceResult<List<ReceivableViewModel>>.Success(overdueReceivables);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOverdueReceivablesAsync), GetType(), _logger);
                return ServiceResult<List<ReceivableViewModel>>.Failure($"取得逾期應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ReceivableViewModel>>> SearchReceivablesAsync(
            string? documentType = null,
            bool? isSettled = null,
            string? customerOrSupplier = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                    return allResult;

                var query = allResult.Data.AsQueryable();

                if (!string.IsNullOrWhiteSpace(customerOrSupplier))
                {
                    query = query.Where(r => 
                        r.CustomerOrSupplier.Contains(customerOrSupplier));
                }

                if (isSettled.HasValue)
                {
                    query = query.Where(r => r.IsSettled == isSettled.Value);
                }

                if (!string.IsNullOrWhiteSpace(documentType))
                {
                    query = query.Where(r => r.DocumentType == documentType);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(r => r.DocumentDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(r => r.DocumentDate <= endDate.Value);
                }

                var filteredReceivables = query.ToList();

                return ServiceResult<List<ReceivableViewModel>>.Success(filteredReceivables);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchReceivablesAsync), GetType(), _logger);
                return ServiceResult<List<ReceivableViewModel>>.Failure($"搜尋應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateReceivedAmountAsync(int id, string documentType, decimal receivedAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (documentType == "SalesOrder")
                {
                    var salesOrderDetail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(sod => sod.Id == id);

                    if (salesOrderDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的銷貨訂單明細");
                    }

                    // 驗證收款金額
                    var validationResult = await ValidateReceivedAmountAsync(id, documentType, receivedAmount);
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<bool>.Failure(validationResult.ErrorMessage);
                    }

                    salesOrderDetail.ReceivedAmount = receivedAmount;
                    // 不需要修改 ModifiedDate，實體沒有這個屬性
                }
                else if (documentType == "PurchaseReturn")
                {
                    var purchaseReturnDetail = await context.PurchaseReturnDetails
                        .FirstOrDefaultAsync(prd => prd.Id == id);

                    if (purchaseReturnDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的進貨退回明細");
                    }

                    // 驗證收款金額
                    var validationResult = await ValidateReceivedAmountAsync(id, documentType, receivedAmount);
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<bool>.Failure(validationResult.ErrorMessage);
                    }

                    purchaseReturnDetail.ReceivedAmount = receivedAmount;
                    // 不需要修改 ModifiedDate，實體沒有這個屬性
                }
                else
                {
                    return ServiceResult<bool>.Failure("不支援的單據類型");
                }

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceivedAmountAsync), GetType(), _logger);
                return ServiceResult<bool>.Failure($"更新收款金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ValidateReceivedAmountAsync(int id, string documentType, decimal receivedAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                decimal totalAmount = 0;
                decimal currentReceivedAmount = 0;

                if (documentType == "SalesOrder")
                {
                    var salesOrderDetail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(sod => sod.Id == id);

                    if (salesOrderDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的銷貨訂單明細");
                    }

                    totalAmount = salesOrderDetail.UnitPrice * salesOrderDetail.OrderQuantity;
                    currentReceivedAmount = salesOrderDetail.ReceivedAmount;
                }
                else if (documentType == "PurchaseReturn")
                {
                    var purchaseReturnDetail = await context.PurchaseReturnDetails
                        .FirstOrDefaultAsync(prd => prd.Id == id);

                    if (purchaseReturnDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的進貨退回明細");
                    }

                    totalAmount = purchaseReturnDetail.ReturnUnitPrice * purchaseReturnDetail.ReturnQuantity;
                    currentReceivedAmount = purchaseReturnDetail.ReceivedAmount;
                }
                else
                {
                    return ServiceResult<bool>.Failure("不支援的單據類型");
                }

                if (receivedAmount < 0)
                {
                    return ServiceResult<bool>.Failure("收款金額不能為負數");
                }

                if (receivedAmount > totalAmount)
                {
                    return ServiceResult<bool>.Failure($"收款金額不能超過總金額 {totalAmount:C}");
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateReceivedAmountAsync), GetType(), _logger);
                return ServiceResult<bool>.Failure($"驗證收款金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> BatchUpdateReceivedAmountsAsync(
            Dictionary<(int Id, string DocumentType), decimal> receivables)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                foreach (var receivable in receivables)
                {
                    var (id, documentType) = receivable.Key;
                    var receivedAmount = receivable.Value;

                    var validationResult = await ValidateReceivedAmountAsync(id, documentType, receivedAmount);
                    if (!validationResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<bool>.Failure($"批次更新失敗 - ID: {id}, 類型: {documentType}, 錯誤: {validationResult.ErrorMessage}");
                    }

                    if (documentType == "SalesOrder")
                    {
                        var salesOrderDetail = await context.SalesOrderDetails
                            .FirstOrDefaultAsync(sod => sod.Id == id);

                        if (salesOrderDetail != null)
                        {
                            salesOrderDetail.ReceivedAmount = receivedAmount;
                            // 不需要修改 ModifiedDate，實體沒有這個屬性
                        }
                    }
                    else if (documentType == "PurchaseReturn")
                    {
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .FirstOrDefaultAsync(prd => prd.Id == id);

                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.ReceivedAmount = receivedAmount;
                            // 不需要修改 ModifiedDate，實體沒有這個屬性
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateReceivedAmountsAsync), GetType(), _logger);
                return ServiceResult<bool>.Failure($"批次更新收款金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ReceivableViewModel?>> GetReceivableByIdAsync(int id, string documentType)
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                {
                    return ServiceResult<ReceivableViewModel?>.Failure(allResult.ErrorMessage);
                }

                var receivable = allResult.Data
                    .FirstOrDefault(r => r.Id == id && r.DocumentType == documentType);

                if (receivable == null)
                {
                    return ServiceResult<ReceivableViewModel?>.Failure("找不到指定的應收款項");
                }

                return ServiceResult<ReceivableViewModel?>.Success(receivable);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivableByIdAsync), GetType(), _logger);
                return ServiceResult<ReceivableViewModel?>.Failure($"取得應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<decimal>> GetTotalReceivableAmountAsync()
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                {
                    return ServiceResult<decimal>.Failure(allResult.ErrorMessage);
                }

                var totalAmount = allResult.Data.Sum(r => r.BalanceAmount);
                return ServiceResult<decimal>.Success(totalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReceivableAmountAsync), GetType(), _logger);
                return ServiceResult<decimal>.Failure($"計算應收總金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<decimal>> GetTotalOverdueAmountAsync()
        {
            try
            {
                var overdueResult = await GetOverdueReceivablesAsync();
                if (!overdueResult.IsSuccess || overdueResult.Data == null)
                {
                    return ServiceResult<decimal>.Failure(overdueResult.ErrorMessage);
                }

                var totalOverdueAmount = overdueResult.Data.Sum(r => r.BalanceAmount);
                return ServiceResult<decimal>.Success(totalOverdueAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalOverdueAmountAsync), GetType(), _logger);
                return ServiceResult<decimal>.Failure($"計算逾期總金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SettleReceivableAsync(int id, string documentType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (documentType == "SalesOrder")
                {
                    var salesOrderDetail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(sod => sod.Id == id);

                    if (salesOrderDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的銷貨訂單明細");
                    }

                    salesOrderDetail.IsSettled = true;
                    salesOrderDetail.TotalReceivedAmount = salesOrderDetail.UnitPrice * salesOrderDetail.OrderQuantity;
                    salesOrderDetail.ReceivedAmount = salesOrderDetail.TotalReceivedAmount;
                }
                else if (documentType == "PurchaseReturn")
                {
                    var purchaseReturnDetail = await context.PurchaseReturnDetails
                        .FirstOrDefaultAsync(prd => prd.Id == id);

                    if (purchaseReturnDetail == null)
                    {
                        return ServiceResult<bool>.Failure("找不到指定的進貨退回明細");
                    }

                    purchaseReturnDetail.IsSettled = true;
                    purchaseReturnDetail.TotalReceivedAmount = purchaseReturnDetail.ReturnUnitPrice * purchaseReturnDetail.ReturnQuantity;
                    purchaseReturnDetail.ReceivedAmount = purchaseReturnDetail.TotalReceivedAmount;
                }
                else
                {
                    return ServiceResult<bool>.Failure("不支援的單據類型");
                }

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SettleReceivableAsync), GetType(), _logger);
                return ServiceResult<bool>.Failure($"結清應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> BatchUpdateReceivedAmountAsync(List<ReceivableUpdateModel> receivableUpdates)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                int updatedCount = 0;

                foreach (var update in receivableUpdates)
                {
                    var validationResult = await ValidateReceivedAmountAsync(update.Id, update.DocumentType, update.ReceivedAmount);
                    if (!validationResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<int>.Failure($"批次更新失敗 - ID: {update.Id}, 類型: {update.DocumentType}, 錯誤: {validationResult.ErrorMessage}");
                    }

                    if (update.DocumentType == "SalesOrder")
                    {
                        var salesOrderDetail = await context.SalesOrderDetails
                            .FirstOrDefaultAsync(sod => sod.Id == update.Id);

                        if (salesOrderDetail != null)
                        {
                            salesOrderDetail.ReceivedAmount = update.ReceivedAmount;
                            updatedCount++;
                        }
                    }
                    else if (update.DocumentType == "PurchaseReturn")
                    {
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .FirstOrDefaultAsync(prd => prd.Id == update.Id);

                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.ReceivedAmount = update.ReceivedAmount;
                            updatedCount++;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<int>.Success(updatedCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateReceivedAmountAsync), GetType(), _logger);
                return ServiceResult<int>.Failure($"批次更新收款金額失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> BatchSettleReceivablesAsync(List<ReceivableIdentifier> receivableIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                int settledCount = 0;

                foreach (var receivableId in receivableIds)
                {
                    var settleResult = await SettleReceivableAsync(receivableId.Id, receivableId.DocumentType);
                    if (settleResult.IsSuccess)
                    {
                        settledCount++;
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<int>.Success(settledCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchSettleReceivablesAsync), GetType(), _logger);
                return ServiceResult<int>.Failure($"批次結清應收款項失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ReceivableStatistics>> GetReceivableStatisticsAsync()
        {
            try
            {
                var allResult = await GetAllReceivablesAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                {
                    return ServiceResult<ReceivableStatistics>.Failure(allResult.ErrorMessage);
                }

                var receivables = allResult.Data;

                var statistics = new ReceivableStatistics
                {
                    TotalCount = receivables.Count,
                    UnSettledCount = receivables.Count(r => !r.IsSettled),
                    OverdueCount = receivables.Count(r => r.IsOverdue),
                    TotalAmount = receivables.Sum(r => r.TotalAmount),
                    TotalReceivedAmount = receivables.Sum(r => r.TotalReceivedAmount),
                    BalanceAmount = receivables.Sum(r => r.BalanceAmount)
                };

                return ServiceResult<ReceivableStatistics>.Success(statistics);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivableStatisticsAsync), GetType(), _logger);
                return ServiceResult<ReceivableStatistics>.Failure($"取得應收款項統計失敗: {ex.Message}");
            }
        }

        private async Task<List<ReceivableViewModel>> GetSalesOrderReceivablesAsync(AppDbContext context)
        {
            var salesReceivables = await context.SalesOrderDetails
                .Include(sod => sod.SalesOrder)
                .ThenInclude(so => so.Customer)
                .Include(sod => sod.Product)
                .Include(sod => sod.Unit)
                .Include(sod => sod.Warehouse)
                .Where(sod => sod.SalesOrder != null)
                .Select(sod => new ReceivableViewModel
                {
                    Id = sod.Id,
                    DocumentType = "SalesOrder",
                    DocumentNumber = sod.SalesOrder.SalesOrderNumber,
                    DocumentDate = sod.SalesOrder.OrderDate,
                    CustomerOrSupplier = sod.SalesOrder.Customer.CompanyName,
                    ProductName = sod.Product.Name,
                    UnitName = sod.Unit != null ? sod.Unit.Name : null,
                    WarehouseName = sod.Warehouse != null ? sod.Warehouse.Name : null,
                    Quantity = sod.OrderQuantity,
                    TotalAmount = sod.UnitPrice * sod.OrderQuantity,
                    ReceivedAmount = sod.ReceivedAmount,
                    TotalReceivedAmount = sod.TotalReceivedAmount,
                    ExpectedReceiveDate = sod.SalesOrder.OrderDate.AddDays(30),
                    IsSettled = sod.IsSettled
                })
                .ToListAsync();

            return salesReceivables;
        }

        private async Task<List<ReceivableViewModel>> GetPurchaseReturnReceivablesAsync(AppDbContext context)
        {
            var purchaseReturnReceivables = await context.PurchaseReturnDetails
                .Include(prd => prd.PurchaseReturn)
                .ThenInclude(pr => pr.Supplier)
                .Include(prd => prd.Product)
                .Include(prd => prd.Unit)
                .Include(prd => prd.WarehouseLocation)
                .Where(prd => prd.PurchaseReturn != null)
                .Select(prd => new ReceivableViewModel
                {
                    Id = prd.Id,
                    DocumentType = "PurchaseReturn",
                    DocumentNumber = prd.PurchaseReturn.PurchaseReturnNumber,
                    DocumentDate = prd.PurchaseReturn.ReturnDate,
                    CustomerOrSupplier = prd.PurchaseReturn.Supplier.CompanyName,
                    ProductName = prd.Product.Name,
                    UnitName = prd.Unit != null ? prd.Unit.Name : null,
                    WarehouseName = prd.WarehouseLocation != null ? prd.WarehouseLocation.Name : null,
                    Quantity = prd.ReturnQuantity,
                    TotalAmount = prd.ReturnUnitPrice * prd.ReturnQuantity,
                    ReceivedAmount = prd.ReceivedAmount,
                    TotalReceivedAmount = prd.TotalReceivedAmount,
                    ExpectedReceiveDate = prd.PurchaseReturn.ReturnDate.AddDays(15), // 使用退回日期加15天作為預計收款日期
                    IsSettled = prd.IsSettled
                })
                .ToListAsync();

            return purchaseReturnReceivables;
        }
    }
}