using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
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
                        .ThenInclude(sod => sod!.SalesOrder)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.SalesReturn)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .Where(d => (
                        d.Setoff.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        d.Setoff.Customer.CompanyName.ToLower().Contains(searchTermLower) ||
                        (d.SalesOrderDetail != null && 
                         d.SalesOrderDetail.SalesOrder != null &&
                         d.SalesOrderDetail.SalesOrder.SalesOrderNumber.ToLower().Contains(searchTermLower)) ||
                        (d.SalesReturnDetail != null && 
                         d.SalesReturnDetail.SalesReturn != null &&
                         d.SalesReturnDetail.SalesReturn.SalesReturnNumber.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(d.ProductName) && d.ProductName.ToLower().Contains(searchTermLower))
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
                
                // 檢查必須選擇銷貨訂單明細或銷貨退回明細其中之一
                if (!entity.SalesOrderDetailId.HasValue && !entity.SalesReturnDetailId.HasValue)
                    errors.Add("必須指定銷貨訂單明細或銷貨退回明細其中之一");
                
                // 不能同時指定兩者
                if (entity.SalesOrderDetailId.HasValue && entity.SalesReturnDetailId.HasValue)
                    errors.Add("不能同時指定銷貨訂單明細和銷貨退回明細");
                
                // 檢查沖款金額
                if (entity.SetoffAmount <= 0)
                    errors.Add("沖款金額必須大於0");            
                
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
                
                // 驗證單據類型與明細類型的一致性（使用計算屬性）
                var documentType = entity.SalesOrderDetailId.HasValue ? "SalesOrder" : 
                                 entity.SalesReturnDetailId.HasValue ? "SalesReturn" : "";
                
                if (documentType == "SalesOrder" && !entity.SalesOrderDetailId.HasValue)
                    errors.Add("單據類型為銷貨訂單時必須指定銷貨訂單明細");
                
                if (documentType == "SalesReturn" && !entity.SalesReturnDetailId.HasValue)
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
                    
                    // 編輯模式：先回滾既有記錄的影響
                    var existingDetails = await context.AccountsReceivableSetoffDetails
                        .Where(d => d.SetoffId == setoffId)
                        .ToListAsync();
                    
                    if (existingDetails.Any())
                    {
                        // 先回滾原始明細表的收款/付款資料
                        await RevertOriginalDetailsAsync(context, existingDetails);
                        
                        // 刪除既有記錄
                        context.AccountsReceivableSetoffDetails.RemoveRange(existingDetails);
                        await context.SaveChangesAsync();
                    }
                    
                    // 設定所有明細的沖款單ID
                    foreach (var detail in details)
                    {
                        detail.SetoffId = setoffId;
                        
                        // 驗證明細
                        var validationResult = await ValidateAsync(detail);
                        if (!validationResult.IsSuccess)
                            return validationResult;
                        
                        // 計算累計收款金額
                        // AfterReceivedAmount 應該是從相關明細表取得當前累計金額再加上本次沖款
                        decimal currentReceived = 0;
                        if (detail.SalesOrderDetailId.HasValue)
                        {
                            var salesOrderDetail = await context.SalesOrderDetails
                                .FirstOrDefaultAsync(sod => sod.Id == detail.SalesOrderDetailId.Value);
                            currentReceived = salesOrderDetail?.TotalReceivedAmount ?? 0;
                        }
                        else if (detail.SalesReturnDetailId.HasValue)
                        {
                            var salesReturnDetail = await context.SalesReturnDetails
                                .FirstOrDefaultAsync(srd => srd.Id == detail.SalesReturnDetailId.Value);
                            currentReceived = salesReturnDetail?.TotalPaidAmount ?? 0;
                        }
                        
                        detail.AfterReceivedAmount = currentReceived + detail.SetoffAmount;
                    }
                    
                    await context.AccountsReceivableSetoffDetails.AddRangeAsync(details);
                    await context.SaveChangesAsync();
                    
                    // 更新原始明細表的收款/付款資料
                    await UpdateOriginalDetailsAsync(context, details);
                    
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
                    
                    // 先回滾原始明細表的收款/付款資料
                    if (details.Any())
                    {
                        await RevertOriginalDetailsAsync(context, details);
                    }
                    
                    // 硬刪除沖款明細
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
                
                var availableAmount = detail.ReceivableAmount - (detail.AfterReceivedAmount - detail.SetoffAmount);
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

        /// <summary>
        /// 取得客戶的未結清明細項目（轉換為統一的 DTO 格式）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>未結清明細 DTO 列表</returns>
        public async Task<List<SetoffDetailDto>> GetCustomerPendingDetailsAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var result = new List<SetoffDetailDto>();

                // 取得未結清的銷貨訂單明細
                var salesOrderDetails = await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sod => sod.Product)
                    .Where(sod => sod.SalesOrder.CustomerId == customerId && !sod.IsSettled)
                    .ToListAsync();

                foreach (var detail in salesOrderDetails)
                {
                    // 從 FinancialTransaction 計算已折讓金額
                    // 現在沖款和折讓在同一筆記錄中，TransactionType 為 AccountsReceivableSetoff
                    var discountedAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed)
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    var pendingAmount = detail.Subtotal - detail.TotalReceivedAmount - discountedAmount;
                    if (pendingAmount > 0)
                    {
                        result.Add(new SetoffDetailDto
                        {
                            Id = result.Count + 1,
                            OriginalEntityId = detail.Id,
                            Type = "SalesOrder",
                            DocumentNumber = detail.SalesOrder.SalesOrderNumber,
                            DocumentDate = detail.SalesOrder.OrderDate,
                            ProductId = detail.ProductId,
                            ProductName = detail.Product?.Name ?? "未知商品",
                            ProductCode = detail.Product?.Code ?? "",
                            Quantity = detail.OrderQuantity,
                            UnitPrice = detail.UnitPrice,
                            TotalAmount = detail.Subtotal,
                            SettledAmount = detail.TotalReceivedAmount,
                            DiscountedAmount = discountedAmount,  // 設定已折讓金額
                            ThisTimeAmount = 0, // 新增模式預設為0
                            ThisTimeDiscountAmount = 0, // 新增模式預設為0
                            OriginalThisTimeAmount = 0, // 新增模式原始值為0
                            OriginalThisTimeDiscountAmount = 0, // 新增模式原始值為0
                            IsEditMode = false, // 標記為新增模式
                            IsSettled = detail.IsSettled,
                            CustomerId = customerId,
                            CustomerName = detail.SalesOrder.Customer?.CompanyName ?? "",
                            Currency = "TWD"
                        });
                    }
                }

                // 取得未結清的銷貨退回明細
                var salesReturnDetails = await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Where(srd => srd.SalesReturn.CustomerId == customerId && !srd.IsSettled)
                    .ToListAsync();

                foreach (var detail in salesReturnDetails)
                {
                    // 從 FinancialTransaction 計算已折讓金額
                    // 現在沖款和折讓在同一筆記錄中，TransactionType 為 AccountsReceivableSetoff
                    var discountedAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed)
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    var pendingAmount = detail.ReturnSubtotal - detail.TotalPaidAmount - discountedAmount;
                    if (pendingAmount > 0)
                    {
                        result.Add(new SetoffDetailDto
                        {
                            Id = result.Count + 1,
                            OriginalEntityId = detail.Id,
                            Type = "SalesReturn",
                            DocumentNumber = detail.SalesReturn.SalesReturnNumber,
                            DocumentDate = detail.SalesReturn.ReturnDate,
                            ProductId = detail.ProductId,
                            ProductName = detail.Product?.Name ?? "未知商品",
                            ProductCode = detail.Product?.Code ?? "",
                            Quantity = detail.ReturnQuantity,
                            UnitPrice = detail.ReturnUnitPrice,
                            TotalAmount = detail.ReturnSubtotal,
                            SettledAmount = detail.TotalPaidAmount,
                            DiscountedAmount = discountedAmount,  // 設定已折讓金額
                            ThisTimeAmount = 0, // 新增模式預設為0
                            ThisTimeDiscountAmount = 0, // 新增模式預設為0
                            OriginalThisTimeAmount = 0, // 新增模式原始值為0
                            OriginalThisTimeDiscountAmount = 0, // 新增模式原始值為0
                            IsEditMode = false, // 標記為新增模式
                            IsSettled = detail.IsSettled,
                            CustomerId = customerId,
                            CustomerName = detail.SalesReturn.Customer?.CompanyName ?? "",
                            Currency = "TWD"
                        });
                    }
                }

                return result.OrderBy(r => r.DocumentDate).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCustomerPendingDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetCustomerPendingDetailsAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId 
                });
                return new List<SetoffDetailDto>();
            }
        }

        /// <summary>
        /// 取得客戶的所有明細項目（編輯模式用，包含已完成的）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="setoffId">當前沖款單ID（用於載入現有沖款記錄）</param>
        /// <returns>所有明細 DTO 列表</returns>
        public async Task<List<SetoffDetailDto>> GetCustomerAllDetailsForEditAsync(int customerId, int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var result = new List<SetoffDetailDto>();

                // 取得當前沖款單已關聯的明細 ID 清單
                var currentSetoffDetailIds = await context.AccountsReceivableSetoffDetails
                    .Where(arsd => arsd.SetoffId == setoffId)
                    .Select(arsd => new 
                    { 
                        arsd.SalesOrderDetailId, 
                        arsd.SalesReturnDetailId 
                    })
                    .ToListAsync();

                var currentSalesOrderDetailIds = currentSetoffDetailIds
                    .Where(x => x.SalesOrderDetailId.HasValue)
                    .Select(x => x.SalesOrderDetailId!.Value)
                    .ToHashSet();

                var currentSalesReturnDetailIds = currentSetoffDetailIds
                    .Where(x => x.SalesReturnDetailId.HasValue)
                    .Select(x => x.SalesReturnDetailId!.Value)
                    .ToHashSet();

                // 編輯模式：只取得與當前沖款單關聯的銷貨訂單明細
                var salesOrderDetails = await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sod => sod.Product)
                    .Where(sod => currentSalesOrderDetailIds.Contains(sod.Id))
                    .ToListAsync();

                // 取得現有的沖款記錄
                var existingSetoffDetails = await context.AccountsReceivableSetoffDetails
                    .Where(arsd => arsd.SetoffId == setoffId)
                    .ToListAsync();

                foreach (var detail in salesOrderDetails)
                {
                    var totalAmount = detail.Subtotal;
                    
                    // 檢查是否在當前沖款單中有記錄
                    var currentSetoffDetail = existingSetoffDetails
                        .FirstOrDefault(esd => esd.SalesOrderDetailId == detail.Id);
                    
                    var thisTimeAmount = currentSetoffDetail?.SetoffAmount ?? 0;
                    
                    // 顯示真實的累計收款金額（包含所有已完成的沖款）
                    var settledAmount = detail.TotalReceivedAmount;
                    
                    // 從 FinancialTransaction 計算已折讓金額（排除當前沖款單的折讓）
                    // 現在沖款和折讓在同一筆記錄中，TransactionType 為 AccountsReceivableSetoff
                    var discountedAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed
                                    && ft.SourceDocumentId != setoffId) // 排除當前沖款單
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    // 取得當前沖款單的折讓金額
                    var currentDiscountAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed
                                    && ft.SourceDocumentId == setoffId)
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    result.Add(new SetoffDetailDto
                    {
                        Id = result.Count + 1,
                        OriginalEntityId = detail.Id,
                        Type = "SalesOrder",
                        DocumentNumber = detail.SalesOrder.SalesOrderNumber,
                        DocumentDate = detail.SalesOrder.OrderDate,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product?.Name ?? "未知商品",
                        ProductCode = detail.Product?.Code ?? "",
                        Quantity = detail.OrderQuantity,
                        UnitPrice = detail.UnitPrice,
                        TotalAmount = totalAmount,
                        SettledAmount = settledAmount,
                        DiscountedAmount = discountedAmount,  // 設定已折讓金額（排除當前沖款單）
                        ThisTimeAmount = thisTimeAmount,
                        ThisTimeDiscountAmount = currentDiscountAmount, // 設定當前沖款單的折讓金額
                        OriginalThisTimeAmount = thisTimeAmount, // 記錄載入時的原始沖款金額
                        OriginalThisTimeDiscountAmount = currentDiscountAmount, // 記錄載入時的原始折讓金額
                        IsEditMode = true, // 標記為編輯模式
                        IsSettled = detail.IsSettled,
                        CustomerId = customerId,
                        CustomerName = detail.SalesOrder.Customer?.CompanyName ?? "",
                        Currency = "TWD"
                    });
                }

                // 編輯模式：只取得與當前沖款單關聯的銷貨退回明細
                var salesReturnDetails = await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Where(srd => currentSalesReturnDetailIds.Contains(srd.Id))
                    .ToListAsync();

                foreach (var detail in salesReturnDetails)
                {
                    var totalAmount = detail.ReturnSubtotal;
                    
                    // 檢查是否在當前沖款單中有記錄
                    var currentSetoffDetail = existingSetoffDetails
                        .FirstOrDefault(esd => esd.SalesReturnDetailId == detail.Id);
                    
                    var thisTimeAmount = currentSetoffDetail?.SetoffAmount ?? 0;
                    
                    // 顯示真實的累計付款金額（包含所有已完成的沖款）
                    var settledAmount = detail.TotalPaidAmount;
                    
                    // 從 FinancialTransaction 計算已折讓金額（排除當前沖款單的折讓）
                    // 現在沖款和折讓在同一筆記錄中，TransactionType 為 AccountsReceivableSetoff
                    var discountedAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed
                                    && ft.SourceDocumentId != setoffId) // 排除當前沖款單
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    // 取得當前沖款單的折讓金額
                    var currentDiscountAmount = await context.FinancialTransactions
                        .Where(ft => ft.SourceDetailId == detail.Id 
                                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff
                                    && !ft.IsReversed
                                    && ft.SourceDocumentId == setoffId)
                        .SumAsync(ft => ft.CurrentDiscountAmount); // 使用 CurrentDiscountAmount 欄位
                    
                    result.Add(new SetoffDetailDto
                    {
                        Id = result.Count + 1,
                        OriginalEntityId = detail.Id,
                        Type = "SalesReturn",
                        DocumentNumber = detail.SalesReturn.SalesReturnNumber,
                        DocumentDate = detail.SalesReturn.ReturnDate,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product?.Name ?? "未知商品",
                        ProductCode = detail.Product?.Code ?? "",
                        Quantity = detail.ReturnQuantity,
                        UnitPrice = detail.ReturnUnitPrice,
                        TotalAmount = totalAmount,
                        SettledAmount = settledAmount,
                        DiscountedAmount = discountedAmount,  // 設定已折讓金額（排除當前沖款單）
                        ThisTimeAmount = thisTimeAmount,
                        ThisTimeDiscountAmount = currentDiscountAmount, // 設定當前沖款單的折讓金額
                        OriginalThisTimeAmount = thisTimeAmount, // 記錄載入時的原始沖款金額
                        OriginalThisTimeDiscountAmount = currentDiscountAmount, // 記錄載入時的原始折讓金額
                        IsEditMode = true, // 標記為編輯模式
                        IsSettled = detail.IsSettled,
                        CustomerId = customerId,
                        CustomerName = detail.SalesReturn.Customer?.CompanyName ?? "",
                        Currency = "TWD"
                    });
                }

                return result.OrderBy(r => r.DocumentDate).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCustomerAllDetailsForEditAsync), GetType(), _logger, new { 
                    Method = nameof(GetCustomerAllDetailsForEditAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId,
                    SetoffId = setoffId
                });
                return new List<SetoffDetailDto>();
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
                // 需要從相關的明細表取得當前累計金額
                decimal currentReceived = 0;
                if (detail.SalesOrderDetailId.HasValue)
                {
                    var salesOrderDetail = await context.SalesOrderDetails
                        .FirstOrDefaultAsync(sod => sod.Id == detail.SalesOrderDetailId.Value);
                    currentReceived = salesOrderDetail?.TotalReceivedAmount ?? 0;
                    // 減掉本次沖款金額，因為我們要重新計算
                    currentReceived -= detail.SetoffAmount;
                }
                else if (detail.SalesReturnDetailId.HasValue)
                {
                    var salesReturnDetail = await context.SalesReturnDetails
                        .FirstOrDefaultAsync(srd => srd.Id == detail.SalesReturnDetailId.Value);
                    currentReceived = salesReturnDetail?.TotalPaidAmount ?? 0;
                    // 減掉本次沖款金額，因為我們要重新計算
                    currentReceived -= detail.SetoffAmount;
                }
                
                detail.AfterReceivedAmount = currentReceived + detail.SetoffAmount;
                detail.UpdatedAt = DateTime.Now;                await context.SaveChangesAsync();
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

        /// <summary>
        /// 更新原始明細表的收款/付款資料
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="setoffDetails">沖款明細列表</param>
        private async Task UpdateOriginalDetailsAsync(AppDbContext context, List<AccountsReceivableSetoffDetail> setoffDetails)
        {
            // 處理銷貨訂單明細
            var salesOrderDetailIds = setoffDetails
                .Where(d => d.SalesOrderDetailId.HasValue)
                .Select(d => d.SalesOrderDetailId!.Value)
                .Distinct()
                .ToList();

            if (salesOrderDetailIds.Any())
            {
                var salesOrderDetails = await context.SalesOrderDetails
                    .Where(sod => salesOrderDetailIds.Contains(sod.Id))
                    .ToListAsync();

                foreach (var salesOrderDetail in salesOrderDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.SalesOrderDetailId == salesOrderDetail.Id);
                    
                    // 更新本次收款金額
                    salesOrderDetail.ReceivedAmount = setoffDetail.SetoffAmount;
                    
                    // 更新累計收款金額
                    salesOrderDetail.TotalReceivedAmount = setoffDetail.AfterReceivedAmount;
                    
                    // 檢查是否已結清
                    salesOrderDetail.IsSettled = salesOrderDetail.TotalReceivedAmount >= salesOrderDetail.Subtotal;
                    
                    salesOrderDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.SalesOrderDetails.UpdateRange(salesOrderDetails);
            }

            // 處理銷貨退回明細
            var salesReturnDetailIds = setoffDetails
                .Where(d => d.SalesReturnDetailId.HasValue)
                .Select(d => d.SalesReturnDetailId!.Value)
                .Distinct()
                .ToList();

            if (salesReturnDetailIds.Any())
            {
                var salesReturnDetails = await context.SalesReturnDetails
                    .Where(srd => salesReturnDetailIds.Contains(srd.Id))
                    .ToListAsync();

                foreach (var salesReturnDetail in salesReturnDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.SalesReturnDetailId == salesReturnDetail.Id);
                    
                    // 更新本次付款金額
                    salesReturnDetail.PaidAmount = setoffDetail.SetoffAmount;
                    
                    // 更新累計付款金額
                    salesReturnDetail.TotalPaidAmount = setoffDetail.AfterReceivedAmount;
                    
                    // 檢查是否已結清 (退回金額通常是負數，所以用絕對值比較)
                    salesReturnDetail.IsSettled = salesReturnDetail.TotalPaidAmount >= Math.Abs(salesReturnDetail.ReturnSubtotal);
                    
                    salesReturnDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.SalesReturnDetails.UpdateRange(salesReturnDetails);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 回滾原始明細表的收款/付款資料（編輯模式用）
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="setoffDetails">要回滾的沖款明細列表</param>
        private async Task RevertOriginalDetailsAsync(AppDbContext context, List<AccountsReceivableSetoffDetail> setoffDetails)
        {
            // 處理銷貨訂單明細的回滾
            var salesOrderDetailIds = setoffDetails
                .Where(d => d.SalesOrderDetailId.HasValue)
                .Select(d => d.SalesOrderDetailId!.Value)
                .Distinct()
                .ToList();

            if (salesOrderDetailIds.Any())
            {
                var salesOrderDetails = await context.SalesOrderDetails
                    .Where(sod => salesOrderDetailIds.Contains(sod.Id))
                    .ToListAsync();

                foreach (var salesOrderDetail in salesOrderDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.SalesOrderDetailId == salesOrderDetail.Id);
                    
                    // 回滾累計收款金額：減去之前的沖款金額
                    salesOrderDetail.TotalReceivedAmount = Math.Max(0, salesOrderDetail.TotalReceivedAmount - setoffDetail.SetoffAmount);
                    salesOrderDetail.ReceivedAmount = 0; // 清除本次收款記錄
                    
                    // 重新檢查結清狀態
                    salesOrderDetail.IsSettled = salesOrderDetail.TotalReceivedAmount >= salesOrderDetail.Subtotal;
                    
                    salesOrderDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.SalesOrderDetails.UpdateRange(salesOrderDetails);
            }

            // 處理銷貨退回明細的回滾
            var salesReturnDetailIds = setoffDetails
                .Where(d => d.SalesReturnDetailId.HasValue)
                .Select(d => d.SalesReturnDetailId!.Value)
                .Distinct()
                .ToList();

            if (salesReturnDetailIds.Any())
            {
                var salesReturnDetails = await context.SalesReturnDetails
                    .Where(srd => salesReturnDetailIds.Contains(srd.Id))
                    .ToListAsync();

                foreach (var salesReturnDetail in salesReturnDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.SalesReturnDetailId == salesReturnDetail.Id);
                    
                    // 回滾累計付款金額：減去之前的沖款金額
                    salesReturnDetail.TotalPaidAmount = Math.Max(0, salesReturnDetail.TotalPaidAmount - setoffDetail.SetoffAmount);
                    salesReturnDetail.PaidAmount = 0; // 清除本次付款記錄
                    
                    // 重新檢查結清狀態
                    salesReturnDetail.IsSettled = salesReturnDetail.TotalPaidAmount >= Math.Abs(salesReturnDetail.ReturnSubtotal);
                    
                    salesReturnDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.SalesReturnDetails.UpdateRange(salesReturnDetails);
            }

            await context.SaveChangesAsync();
        }

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