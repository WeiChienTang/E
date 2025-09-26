using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款明細服務實作
    /// </summary>
    public class AccountsReceivableSetoffDetailService : GenericManagementService<AccountsReceivableSetoffDetail>, IAccountsReceivableSetoffDetailService
    {
        public AccountsReceivableSetoffDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<AccountsReceivableSetoffDetail>> logger) : base(contextFactory, logger)
        {
        }

        public AccountsReceivableSetoffDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<AccountsReceivableSetoffDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s.Customer)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public override async Task<AccountsReceivableSetoffDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s.Customer)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .FirstOrDefaultAsync(d => d.Id == id);
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

        public override async Task<List<AccountsReceivableSetoffDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s.Customer)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .Where(d => (
                        d.Setoff.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        d.Setoff.Customer.CompanyName.ToLower().Contains(searchTermLower) ||
                        (!string.IsNullOrEmpty(d.DocumentNumber) && d.DocumentNumber.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(d.ProductName) && d.ProductName.ToLower().Contains(searchTermLower)) ||
                        d.DocumentType.ToLower().Contains(searchTermLower)
                    ))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsReceivableSetoffDetail entity)
        {
            try
            {
                var errors = new List<string>();
                
                // 檢查必填欄位
                if (entity.SetoffId <= 0)
                    errors.Add("必須指定沖款單");
                
                if (string.IsNullOrWhiteSpace(entity.DocumentType))
                    errors.Add("單據類型不能為空");
                
                // 檢查必須選擇銷貨訂單明細或銷貨退回明細其中之一
                if (!entity.SalesOrderDetailId.HasValue && !entity.SalesReturnDetailId.HasValue)
                    errors.Add("必須指定銷貨訂單明細或銷貨退回明細其中之一");
                
                // 不能同時指定兩者
                if (entity.SalesOrderDetailId.HasValue && entity.SalesReturnDetailId.HasValue)
                    errors.Add("不能同時指定銷貨訂單明細和銷貨退回明細");
                
                // 檢查沖款金額
                if (entity.SetoffAmount <= 0)
                    errors.Add("沖款金額必須大於0");
                
                // 檢查沖款金額不能超過剩餘應收金額
                var remainingAmount = entity.ReceivableAmount - entity.PreviousReceivedAmount;
                if (entity.SetoffAmount > remainingAmount)
                    errors.Add($"沖款金額不能超過剩餘應收金額 {remainingAmount:N2}");
                
                // 檢查沖款單是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffExists = await context.AccountsReceivableSetoffs
                    .AnyAsync(s => s.Id == entity.SetoffId);
                if (!setoffExists)
                    errors.Add("指定的沖款單不存在");
                
                // 檢查關聯的銷貨訂單明細或銷貨退回明細是否存在
                if (entity.SalesOrderDetailId.HasValue)
                {
                    var salesOrderDetailExists = await context.SalesOrderDetails
                        .AnyAsync(sod => sod.Id == entity.SalesOrderDetailId.Value);
                    if (!salesOrderDetailExists)
                        errors.Add("指定的銷貨訂單明細不存在");
                }
                
                if (entity.SalesReturnDetailId.HasValue)
                {
                    var salesReturnDetailExists = await context.SalesReturnDetails
                        .AnyAsync(srd => srd.Id == entity.SalesReturnDetailId.Value);
                    if (!salesReturnDetailExists)
                        errors.Add("指定的銷貨退回明細不存在");
                }
                
                // 驗證單據類型與明細類型的一致性
                if (entity.DocumentType == "SalesOrder" && !entity.SalesOrderDetailId.HasValue)
                    errors.Add("單據類型為銷貨訂單時必須指定銷貨訂單明細");
                
                if (entity.DocumentType == "SalesReturn" && !entity.SalesReturnDetailId.HasValue)
                    errors.Add("單據類型為銷貨退回時必須指定銷貨退回明細");
                
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
                    SetoffId = entity.SetoffId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 特殊業務方法

        public async Task<List<AccountsReceivableSetoffDetail>> GetBySetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .Where(d => d.SetoffId == setoffId)
                    .OrderBy(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySetoffIdAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId 
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public async Task<List<AccountsReceivableSetoffDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s.Customer)
                    .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderDetailIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySalesOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId 
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public async Task<List<AccountsReceivableSetoffDetail>> GetBySalesReturnDetailIdAsync(int salesReturnDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s.Customer)
                    .Where(d => d.SalesReturnDetailId == salesReturnDetailId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesReturnDetailIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySalesReturnDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesReturnDetailId = salesReturnDetailId 
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public async Task<decimal> CalculateTotalReceivedAmountBySalesOrderDetailAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalReceivedAmountBySalesOrderDetailAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalReceivedAmountBySalesOrderDetailAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId 
                });
                return 0;
            }
        }

        public async Task<decimal> CalculateTotalReceivedAmountBySalesReturnDetailAsync(int salesReturnDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Where(d => d.SalesReturnDetailId == salesReturnDetailId)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalReceivedAmountBySalesReturnDetailAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalReceivedAmountBySalesReturnDetailAsync),
                    ServiceType = GetType().Name,
                    SalesReturnDetailId = salesReturnDetailId 
                });
                return 0;
            }
        }

        public async Task<bool> IsSalesOrderDetailFullyReceivedAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var salesOrderDetail = await context.SalesOrderDetails
                    .FirstOrDefaultAsync(sod => sod.Id == salesOrderDetailId);
                
                if (salesOrderDetail == null)
                    return false;
                
                var totalReceived = await CalculateTotalReceivedAmountBySalesOrderDetailAsync(salesOrderDetailId);
                var totalAmount = salesOrderDetail.Subtotal; // 使用 Subtotal 屬性
                
                return totalReceived >= totalAmount;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesOrderDetailFullyReceivedAsync), GetType(), _logger, new { 
                    Method = nameof(IsSalesOrderDetailFullyReceivedAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId 
                });
                return false;
            }
        }

        public async Task<bool> IsSalesReturnDetailFullyReceivedAsync(int salesReturnDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var salesReturnDetail = await context.SalesReturnDetails
                    .FirstOrDefaultAsync(srd => srd.Id == salesReturnDetailId);
                
                if (salesReturnDetail == null)
                    return false;
                
                var totalReceived = await CalculateTotalReceivedAmountBySalesReturnDetailAsync(salesReturnDetailId);
                var totalAmount = Math.Abs(salesReturnDetail.ReturnSubtotal); // 使用 ReturnSubtotal 屬性
                
                return totalReceived >= totalAmount;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesReturnDetailFullyReceivedAsync), GetType(), _logger, new { 
                    Method = nameof(IsSalesReturnDetailFullyReceivedAsync),
                    ServiceType = GetType().Name,
                    SalesReturnDetailId = salesReturnDetailId 
                });
                return false;
            }
        }

        public async Task<ServiceResult> CreateBatchForSetoffAsync(int setoffId, List<AccountsReceivableSetoffDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 檢查沖款單是否存在
                    var setoffExists = await context.AccountsReceivableSetoffs
                        .AnyAsync(s => s.Id == setoffId);
                    if (!setoffExists)
                        return ServiceResult.Failure("沖款單不存在");
                    
                    // 設定所有明細的沖款單ID
                    foreach (var detail in details)
                    {
                        detail.SetoffId = setoffId;
                        
                        // 驗證明細
                        var validationResult = await ValidateAsync(detail);
                        if (!validationResult.IsSuccess)
                            return validationResult;
                        
                        // 計算累計收款金額
                        detail.AfterReceivedAmount = detail.PreviousReceivedAmount + detail.SetoffAmount;
                        detail.RemainingAmount = detail.ReceivableAmount - detail.AfterReceivedAmount;
                        detail.IsFullyReceived = detail.RemainingAmount <= 0;
                    }
                    
                    await context.AccountsReceivableSetoffDetails.AddRangeAsync(details);
                    await context.SaveChangesAsync();
                    
                    // 更新沖款單的總金額
                    var totalAmount = details.Sum(d => d.SetoffAmount);
                    var setoff = await context.AccountsReceivableSetoffs.FindAsync(setoffId);
                    if (setoff != null)
                    {
                        setoff.TotalSetoffAmount = await CalculateTotalAmountBySetoffIdAsync(setoffId);
                        await context.SaveChangesAsync();
                    }
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateBatchForSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(CreateBatchForSetoffAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId,
                    DetailCount = details.Count 
                });
                return ServiceResult.Failure("批次新增明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteBySetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var details = await context.AccountsReceivableSetoffDetails
                        .Where(d => d.SetoffId == setoffId)
                        .ToListAsync();
                    
                    // 硬刪除
                    context.AccountsReceivableSetoffDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                    
                    // 更新沖款單的總金額
                    var setoff = await context.AccountsReceivableSetoffs.FindAsync(setoffId);
                    if (setoff != null)
                    {
                        setoff.TotalSetoffAmount = 0;
                        await context.SaveChangesAsync();
                    }
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBySetoffIdAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteBySetoffIdAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId 
                });
                return ServiceResult.Failure("刪除明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> ValidateSetoffAmountAsync(AccountsReceivableSetoffDetail detail)
        {
            try
            {
                var errors = new List<string>();
                
                if (detail.SetoffAmount <= 0)
                    errors.Add("沖款金額必須大於0");
                
                var availableAmount = detail.ReceivableAmount - detail.PreviousReceivedAmount;
                if (detail.SetoffAmount > availableAmount)
                    errors.Add($"沖款金額不能超過可用金額 {availableAmount:N2}");
                
                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));
                    
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateSetoffAmountAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateSetoffAmountAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id,
                    SetoffAmount = detail.SetoffAmount 
                });
                return ServiceResult.Failure("驗證沖款金額時發生錯誤");
            }
        }

        public async Task<List<dynamic>> GetAvailableItemsForSetoffAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var items = new List<dynamic>();
                
                // 取得銷貨訂單明細（尚未完全收款的）
                var salesOrderDetails = await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                    .Include(sod => sod.Product)
                    .Where(sod => sod.SalesOrder.CustomerId == customerId)
                    .ToListAsync();
                
                foreach (var sod in salesOrderDetails)
                {
                    var totalReceived = await CalculateTotalReceivedAmountBySalesOrderDetailAsync(sod.Id);
                    var remaining = sod.Subtotal - totalReceived;
                    
                    if (remaining > 0)
                    {
                        items.Add(new
                        {
                            Type = "SalesOrder",
                            DetailId = sod.Id,
                            DocumentNumber = sod.SalesOrder.SalesOrderNumber,
                            ProductName = sod.Product?.Name ?? "",
                            Quantity = sod.OrderQuantity,
                            UnitName = sod.Product?.Unit?.Name ?? "",
                            TotalAmount = sod.Subtotal,
                            ReceivedAmount = totalReceived,
                            RemainingAmount = remaining
                        });
                    }
                }
                
                // 取得銷貨退回明細（尚未完全退款的）
                var salesReturnDetails = await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                    .Include(srd => srd.Product)
                    .Where(srd => srd.SalesReturn.CustomerId == customerId)
                    .ToListAsync();
                
                foreach (var srd in salesReturnDetails)
                {
                    var totalReceived = await CalculateTotalReceivedAmountBySalesReturnDetailAsync(srd.Id);
                    var remaining = Math.Abs(srd.ReturnSubtotal) - totalReceived;
                    
                    if (remaining > 0)
                    {
                        items.Add(new
                        {
                            Type = "SalesReturn",
                            DetailId = srd.Id,
                            DocumentNumber = srd.SalesReturn.SalesReturnNumber,
                            ProductName = srd.Product?.Name ?? "",
                            Quantity = Math.Abs(srd.ReturnQuantity),
                            UnitName = srd.Product?.Unit?.Name ?? "",
                            TotalAmount = Math.Abs(srd.ReturnSubtotal),
                            ReceivedAmount = totalReceived,
                            RemainingAmount = remaining
                        });
                    }
                }
                
                return items;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableItemsForSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(GetAvailableItemsForSetoffAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId 
                });
                return new List<dynamic>();
            }
        }

        public async Task<ServiceResult> UpdateReceivableAmountsAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var detail = await context.AccountsReceivableSetoffDetails
                    .FirstOrDefaultAsync(d => d.Id == detailId);
                
                if (detail == null)
                    return ServiceResult.Failure("明細不存在");
                
                // 重新計算累計收款金額
                detail.AfterReceivedAmount = detail.PreviousReceivedAmount + detail.SetoffAmount;
                detail.RemainingAmount = detail.ReceivableAmount - detail.AfterReceivedAmount;
                detail.IsFullyReceived = detail.RemainingAmount <= 0;
                detail.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceivableAmountsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateReceivableAmountsAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId 
                });
                return ServiceResult.Failure("更新金額時發生錯誤");
            }
        }

        #endregion

        #region 私有輔助方法

        private async Task<decimal> CalculateTotalAmountBySetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Where(d => d.SetoffId == setoffId)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalAmountBySetoffIdAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalAmountBySetoffIdAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId 
                });
                return 0;
            }
        }

        #endregion
    }
}