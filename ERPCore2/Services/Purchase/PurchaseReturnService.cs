using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購退回服務
    /// </summary>
    public class PurchaseReturnService : GenericManagementService<PurchaseReturn>, IPurchaseReturnService
    {
        public PurchaseReturnService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Include(pr => pr.Employee)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Include(pr => pr.Employee)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Include(pr => pr.Employee)
                    .Include(pr => pr.Warehouse)
                    .Include(pr => pr.ConfirmedByUser)
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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.SupplierId == supplierId && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReturnDate)
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

        public async Task<List<PurchaseReturn>> GetByStatusAsync(PurchaseReturnStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.ReturnStatus == status && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByStatusAsync), GetType(), _logger, new { 
                    Method = nameof(GetByStatusAsync),
                    ServiceType = GetType().Name,
                    Status = status 
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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.ReturnDate >= startDate && pr.ReturnDate <= endDate && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReturnDate)
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

        public async Task<List<PurchaseReturn>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.PurchaseOrderId == purchaseOrderId && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseOrderIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByPurchaseOrderIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
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
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => pr.PurchaseReceivingId == purchaseReceivingId && !pr.IsDeleted)
                    .OrderByDescending(pr => pr.ReturnDate)
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
                    query = query.Where(pr => pr.Id != excludeId.Value);
                
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
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => !pr.IsDeleted && (
                        pr.PurchaseReturnNumber.ToLower().Contains(lowerSearchTerm) ||
                        pr.Supplier.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                        (pr.ReturnDescription != null && pr.ReturnDescription.ToLower().Contains(lowerSearchTerm)) ||
                        (pr.PurchaseOrder != null && pr.PurchaseOrder.PurchaseOrderNumber.ToLower().Contains(lowerSearchTerm)) ||
                        (pr.PurchaseReceiving != null && pr.PurchaseReceiving.ReceiptNumber.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderByDescending(pr => pr.ReturnDate)
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
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.PurchaseReturnNumber))
                    errors.Add("退回單號不能為空");
                
                if (entity.SupplierId <= 0)
                    errors.Add("必須選擇供應商");
                
                if (entity.ReturnDate == default)
                    errors.Add("退回日期不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.PurchaseReturnNumber) && 
                    await IsPurchaseReturnNumberExistsAsync(entity.PurchaseReturnNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("退回單號已存在");
                
                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));
                    
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityNumber = entity.PurchaseReturnNumber 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<ServiceResult> SubmitAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus != PurchaseReturnStatus.Draft)
                    return ServiceResult.Failure("只有草稿狀態的退回單可以送出");
                
                purchaseReturn.ReturnStatus = PurchaseReturnStatus.Submitted;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SubmitAsync), GetType(), _logger, new { 
                    Method = nameof(SubmitAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("送出採購退回過程發生錯誤");
            }
        }

        public async Task<ServiceResult> ConfirmAsync(int id, int confirmedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus != PurchaseReturnStatus.Submitted)
                    return ServiceResult.Failure("只有已送出狀態的退回單可以確認");
                
                purchaseReturn.ReturnStatus = PurchaseReturnStatus.Confirmed;
                purchaseReturn.ConfirmedAt = DateTime.UtcNow;
                purchaseReturn.ConfirmedBy = confirmedBy;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmAsync), GetType(), _logger, new { 
                    Method = nameof(ConfirmAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ConfirmedBy = confirmedBy 
                });
                return ServiceResult.Failure("確認採購退回過程發生錯誤");
            }
        }

        public async Task<ServiceResult> StartProcessingAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus != PurchaseReturnStatus.Confirmed)
                    return ServiceResult.Failure("只有已確認狀態的退回單可以開始處理");
                
                purchaseReturn.ReturnStatus = PurchaseReturnStatus.Processing;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(StartProcessingAsync), GetType(), _logger, new { 
                    Method = nameof(StartProcessingAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("開始處理採購退回過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CompleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus != PurchaseReturnStatus.Processing)
                    return ServiceResult.Failure("只有處理中狀態的退回單可以完成");
                
                purchaseReturn.ReturnStatus = PurchaseReturnStatus.Completed;
                purchaseReturn.ProcessCompletedAt = DateTime.UtcNow;
                purchaseReturn.ActualProcessDate = DateTime.UtcNow;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteAsync), GetType(), _logger, new { 
                    Method = nameof(CompleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("完成採購退回過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CancelAsync(int id, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus == PurchaseReturnStatus.Completed)
                    return ServiceResult.Failure("已完成的退回單無法取消");
                
                purchaseReturn.ReturnStatus = PurchaseReturnStatus.Cancelled;
                if (!string.IsNullOrWhiteSpace(remarks))
                    purchaseReturn.ProcessRemarks = remarks;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelAsync), GetType(), _logger, new { 
                    Method = nameof(CancelAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    Remarks = remarks 
                });
                return ServiceResult.Failure("取消採購退回過程發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateStatusAsync(int id, PurchaseReturnStatus status, string? remarks = null)
        {
            try
            {
                return status switch
                {
                    PurchaseReturnStatus.Submitted => await SubmitAsync(id),
                    PurchaseReturnStatus.Processing => await StartProcessingAsync(id),
                    PurchaseReturnStatus.Completed => await CompleteAsync(id),
                    PurchaseReturnStatus.Cancelled => await CancelAsync(id, remarks),
                    _ => ServiceResult.Failure("不支援的狀態變更")
                };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateStatusAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateStatusAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    Status = status,
                    Remarks = remarks 
                });
                return ServiceResult.Failure("更新採購退回狀態過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CalculateTotalsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns
                    .Include(pr => pr.PurchaseReturnDetails)
                    .FirstOrDefaultAsync(pr => pr.Id == id && !pr.IsDeleted);
                
                if (purchaseReturn == null)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                purchaseReturn.TotalReturnAmount = purchaseReturn.PurchaseReturnDetails.Sum(prd => prd.ReturnSubtotal);
                purchaseReturn.ReturnTaxAmount = purchaseReturn.TotalReturnAmount * 0.05m; // 假設稅率5%
                purchaseReturn.TotalReturnAmountWithTax = purchaseReturn.TotalReturnAmount + purchaseReturn.ReturnTaxAmount;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalsAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalsAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("計算採購退回金額過程發生錯誤");
            }
        }

        public async Task<ServiceResult> RefundProcessAsync(int id, decimal refundAmount, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReturn = await context.PurchaseReturns.FindAsync(id);
                
                if (purchaseReturn == null || purchaseReturn.IsDeleted)
                    return ServiceResult.Failure("找不到指定的採購退回記錄");
                
                if (purchaseReturn.ReturnStatus != PurchaseReturnStatus.Completed)
                    return ServiceResult.Failure("只有已完成的退回單可以進行退款");
                
                if (refundAmount <= 0)
                    return ServiceResult.Failure("退款金額必須大於0");
                
                if (refundAmount > purchaseReturn.TotalReturnAmountWithTax)
                    return ServiceResult.Failure("退款金額不能超過退回總金額");
                
                purchaseReturn.IsRefunded = true;
                purchaseReturn.RefundDate = DateTime.UtcNow;
                purchaseReturn.RefundAmount = refundAmount;
                if (!string.IsNullOrWhiteSpace(remarks))
                    purchaseReturn.RefundRemarks = remarks;
                purchaseReturn.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RefundProcessAsync), GetType(), _logger, new { 
                    Method = nameof(RefundProcessAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    RefundAmount = refundAmount,
                    Remarks = remarks 
                });
                return ServiceResult.Failure("退款處理過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateFromPurchaseOrderAsync(int purchaseOrderId, List<PurchaseReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseOrder = await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .FirstOrDefaultAsync(po => po.Id == purchaseOrderId && !po.IsDeleted);
                
                if (purchaseOrder == null)
                    return ServiceResult.Failure("找不到指定的採購訂單");
                
                // 生成退回單號
                var returnNumber = await GenerateReturnNumberAsync(context);
                
                var purchaseReturn = new PurchaseReturn
                {
                    PurchaseReturnNumber = returnNumber,
                    ReturnDate = DateTime.Today,
                    SupplierId = purchaseOrder.SupplierId,
                    PurchaseOrderId = purchaseOrderId,
                    ReturnStatus = PurchaseReturnStatus.Draft,
                    ReturnReason = PurchaseReturnReason.QualityIssue,
                    PurchaseReturnDetails = details
                };
                
                context.PurchaseReturns.Add(purchaseReturn);
                await context.SaveChangesAsync();
                
                // 計算總金額
                await CalculateTotalsAsync(purchaseReturn.Id);
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateFromPurchaseOrderAsync), GetType(), _logger, new { 
                    Method = nameof(CreateFromPurchaseOrderAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
                });
                return ServiceResult.Failure("從採購訂單建立退回單過程發生錯誤");
            }
        }

        public async Task<ServiceResult> CreateFromPurchaseReceivingAsync(int purchaseReceivingId, List<PurchaseReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseReceiving = await context.PurchaseReceivings
                    .Include(pr => pr.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseReceivingId && !pr.IsDeleted);
                
                if (purchaseReceiving == null)
                    return ServiceResult.Failure("找不到指定的採購進貨單");
                
                // 生成退回單號
                var returnNumber = await GenerateReturnNumberAsync(context);
                
                var purchaseReturn = new PurchaseReturn
                {
                    PurchaseReturnNumber = returnNumber,
                    ReturnDate = DateTime.Today,
                    SupplierId = purchaseReceiving.PurchaseOrder.SupplierId,
                    PurchaseOrderId = purchaseReceiving.PurchaseOrderId,
                    PurchaseReceivingId = purchaseReceivingId,
                    ReturnStatus = PurchaseReturnStatus.Draft,
                    ReturnReason = PurchaseReturnReason.QualityIssue,
                    PurchaseReturnDetails = details
                };
                
                context.PurchaseReturns.Add(purchaseReturn);
                await context.SaveChangesAsync();
                
                // 計算總金額
                await CalculateTotalsAsync(purchaseReturn.Id);
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateFromPurchaseReceivingAsync), GetType(), _logger, new { 
                    Method = nameof(CreateFromPurchaseReceivingAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId 
                });
                return ServiceResult.Failure("從採購進貨單建立退回單過程發生錯誤");
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
                    PendingReturns = returns.Count(pr => pr.ReturnStatus == PurchaseReturnStatus.Submitted),
                    ProcessingReturns = returns.Count(pr => pr.ReturnStatus == PurchaseReturnStatus.Processing),
                    CompletedReturns = returns.Count(pr => pr.ReturnStatus == PurchaseReturnStatus.Completed),
                    CancelledReturns = returns.Count(pr => pr.ReturnStatus == PurchaseReturnStatus.Cancelled),
                    TotalReturnAmount = returns.Sum(pr => pr.TotalReturnAmountWithTax),
                    TotalRefundAmount = returns.Where(pr => pr.IsRefunded).Sum(pr => pr.RefundAmount),
                    ReturnReasonCounts = returns.GroupBy(pr => pr.ReturnReason)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    StatusCounts = returns.GroupBy(pr => pr.ReturnStatus)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SupplierReturnAmounts = returns.GroupBy(pr => pr.SupplierId)
                        .ToDictionary(g => g.Key, g => g.Sum(pr => pr.TotalReturnAmountWithTax))
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

        public async Task<List<PurchaseReturn>> GetPendingReturnsAsync()
        {
            try
            {
                return await GetByStatusAsync(PurchaseReturnStatus.Submitted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingReturnsAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingReturnsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReturn>();
            }
        }

        public async Task<List<PurchaseReturn>> GetOverdueReturnsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var currentDate = DateTime.Today;
                
                return await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Include(pr => pr.PurchaseOrder)
                    .Include(pr => pr.PurchaseReceiving)
                    .Where(pr => !pr.IsDeleted &&
                                pr.ReturnStatus != PurchaseReturnStatus.Completed &&
                                pr.ReturnStatus != PurchaseReturnStatus.Cancelled &&
                                pr.ExpectedProcessDate.HasValue &&
                                pr.ExpectedProcessDate.Value < currentDate)
                    .OrderBy(pr => pr.ExpectedProcessDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOverdueReturnsAsync), GetType(), _logger, new { 
                    Method = nameof(GetOverdueReturnsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseReturn>();
            }
        }

        public async Task<decimal> GetTotalReturnAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturns
                    .Where(pr => pr.SupplierId == supplierId && !pr.IsDeleted && pr.ReturnStatus == PurchaseReturnStatus.Completed);
                
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
            var today = DateTime.Today;
            var prefix = $"PR{today:yyyyMM}";
            
            var lastNumber = await context.PurchaseReturns
                .Where(pr => pr.PurchaseReturnNumber.StartsWith(prefix))
                .OrderByDescending(pr => pr.PurchaseReturnNumber)
                .Select(pr => pr.PurchaseReturnNumber)
                .FirstOrDefaultAsync();
            
            var sequence = 1;
            if (!string.IsNullOrEmpty(lastNumber) && lastNumber.Length >= prefix.Length + 4)
            {
                if (int.TryParse(lastNumber.Substring(prefix.Length), out var lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
            
            return $"{prefix}{sequence:D4}";
        }
    }
}
