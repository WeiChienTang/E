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
        public SalesReturnService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public SalesReturnService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesReturn>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<SalesReturn>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .Where(sr => !sr.IsDeleted)
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.Product)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.SalesOrderDetail)
                    .Include(sr => sr.SalesReturnDetails)
                        .ThenInclude(srd => srd.SalesDeliveryDetail)
                    .FirstOrDefaultAsync(sr => sr.Id == id && !sr.IsDeleted);
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => !sr.IsDeleted &&
                        (sr.SalesReturnNumber.ToLower().Contains(lowerSearchTerm) ||
                         sr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                         (sr.ReturnDescription != null && sr.ReturnDescription.ToLower().Contains(lowerSearchTerm)) ||
                         (sr.ProcessPersonnel != null && sr.ProcessPersonnel.ToLower().Contains(lowerSearchTerm))))
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

                if (entity.RefundAmount < 0)
                    errors.Add("退款金額不能為負數");

                if (entity.IsRefunded && entity.RefundDate == null)
                    errors.Add("已標示為退款但未設定退款日期");

                if (entity.ExpectedProcessDate.HasValue && entity.ExpectedProcessDate < entity.ReturnDate)
                    errors.Add("預計處理日期不能早於退回日期");

                if (entity.ActualProcessDate.HasValue && entity.ActualProcessDate < entity.ReturnDate)
                    errors.Add("實際處理日期不能早於退回日期");

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
                var query = context.SalesReturns.Where(sr => sr.SalesReturnNumber == salesReturnNumber && !sr.IsDeleted);
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => sr.CustomerId == customerId && !sr.IsDeleted)
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

        public async Task<List<SalesReturn>> GetByStatusAsync(SalesReturnStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => sr.ReturnStatus == status && !sr.IsDeleted)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByStatusAsync),
                    ServiceType = GetType().Name,
                    Status = status
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
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => sr.SalesOrderId == salesOrderId && !sr.IsDeleted)
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

        public async Task<List<SalesReturn>> GetBySalesDeliveryIdAsync(int salesDeliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => sr.SalesDeliveryId == salesDeliveryId && !sr.IsDeleted)
                    .OrderByDescending(sr => sr.ReturnDate)
                    .ThenBy(sr => sr.SalesReturnNumber)
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
                    .Include(sr => sr.SalesOrder)
                    .Include(sr => sr.SalesDelivery)
                    .Include(sr => sr.Employee)
                    .Where(sr => sr.ReturnDate >= startDate && sr.ReturnDate <= endDate && !sr.IsDeleted)
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

        public async Task<ServiceResult> UpdateStatusAsync(int id, SalesReturnStatus status, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var salesReturn = await context.SalesReturns.FindAsync(id);

                if (salesReturn == null || salesReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的銷貨退回記錄");

                salesReturn.ReturnStatus = status;
                if (!string.IsNullOrWhiteSpace(remarks))
                    salesReturn.ProcessRemarks = remarks;

                salesReturn.UpdatedAt = DateTime.UtcNow;

                // 根據狀態更新相關日期
                switch (status)
                {
                    case SalesReturnStatus.Processing:
                        if (salesReturn.ActualProcessDate == null)
                            salesReturn.ActualProcessDate = DateTime.Today;
                        break;
                    case SalesReturnStatus.Completed:
                        if (salesReturn.ActualProcessDate == null)
                            salesReturn.ActualProcessDate = DateTime.Today;
                        break;
                    case SalesReturnStatus.Cancelled:
                        salesReturn.ActualProcessDate = null;
                        break;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateStatusAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateStatusAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    Status = status,
                    Remarks = remarks
                });
                return ServiceResult.Failure("更新狀態時發生錯誤");
            }
        }

        public async Task<decimal> CalculateTotalReturnAmountAsync(int salesReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Where(srd => srd.SalesReturnId == salesReturnId && !srd.IsDeleted)
                    .SumAsync(srd => srd.ReturnSubtotal);
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

        public async Task<ServiceResult> SetRefundInfoAsync(int salesReturnId, decimal refundAmount, DateTime refundDate, string? refundRemarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var salesReturn = await context.SalesReturns.FindAsync(salesReturnId);

                if (salesReturn == null || salesReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的銷貨退回記錄");

                if (refundAmount < 0)
                    return ServiceResult.Failure("退款金額不能為負數");

                if (refundDate < salesReturn.ReturnDate)
                    return ServiceResult.Failure("退款日期不能早於退回日期");

                salesReturn.IsRefunded = true;
                salesReturn.RefundAmount = refundAmount;
                salesReturn.RefundDate = refundDate;
                salesReturn.RefundRemarks = refundRemarks;
                salesReturn.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetRefundInfoAsync), GetType(), _logger, new
                {
                    Method = nameof(SetRefundInfoAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId,
                    RefundAmount = refundAmount,
                    RefundDate = refundDate,
                    RefundRemarks = refundRemarks
                });
                return ServiceResult.Failure("設定退款資訊時發生錯誤");
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
                var query = context.SalesReturns.Where(sr => !sr.IsDeleted);

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
                    TotalRefundAmount = returns.Sum(sr => sr.RefundAmount),
                    PendingReturns = returns.Count(sr => sr.ReturnStatus == SalesReturnStatus.Submitted),
                    CompletedReturns = returns.Count(sr => sr.ReturnStatus == SalesReturnStatus.Completed),
                    CancelledReturns = returns.Count(sr => sr.ReturnStatus == SalesReturnStatus.Cancelled),
                    ReturnReasonCounts = returns.GroupBy(sr => sr.ReturnReason)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    StatusCounts = returns.GroupBy(sr => sr.ReturnStatus)
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
    }
}
