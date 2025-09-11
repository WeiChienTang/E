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
        public PurchaseReturnService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PurchaseReturn>> logger) : base(contextFactory, logger)
        {
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
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.Unit)
                    .Include(pr => pr.PurchaseReturnDetails)
                        .ThenInclude(prd => prd.WarehouseLocation)
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
    }


}
