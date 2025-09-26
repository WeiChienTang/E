using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款服務實作
    /// </summary>
    public class AccountsReceivableSetoffService : GenericManagementService<AccountsReceivableSetoff>, IAccountsReceivableSetoffService
    {
        public AccountsReceivableSetoffService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public AccountsReceivableSetoffService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<AccountsReceivableSetoff>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<AccountsReceivableSetoff>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenBy(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetAllWithDetailsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.SalesOrderDetail)
                            .ThenInclude(sod => sod!.Product)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.SalesReturnDetail)
                            .ThenInclude(srd => srd!.Product)
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenBy(s => s.SetoffNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllWithDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllWithDetailsAsync),
                    ServiceType = GetType().Name
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        public override async Task<AccountsReceivableSetoff?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
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

        public async Task<AccountsReceivableSetoff?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.SalesOrderDetail)
                            .ThenInclude(sod => sod!.Product)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.SalesReturnDetail)
                            .ThenInclude(srd => srd!.Product)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdWithDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdWithDetailsAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Where(s => s.CustomerId == customerId && !s.IsDeleted)
                    .OrderByDescending(s => s.SetoffDate)
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
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetIncompleteAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Where(s => !s.IsCompleted && !s.IsDeleted)
                    .OrderByDescending(s => s.SetoffDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetIncompleteAsync), GetType(), _logger, new
                {
                    Method = nameof(GetIncompleteAsync),
                    ServiceType = GetType().Name
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetPendingApprovalAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Pending && !s.IsDeleted)
                    .OrderByDescending(s => s.SetoffDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingApprovalAsync), GetType(), _logger, new
                {
                    Method = nameof(GetPendingApprovalAsync),
                    ServiceType = GetType().Name
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<string> GenerateSetoffNumberAsync()
        {
            try
            {
                return await CodeGenerationHelper.GenerateEntityCodeAsync(
                    "AR",
                    DateTime.Today,
                    async (prefix) =>
                    {
                        using var context = await _contextFactory.CreateDbContextAsync();
                        return await context.AccountsReceivableSetoffs
                            .Where(s => s.SetoffNumber.StartsWith(prefix))
                            .CountAsync() + 1;
                    }
                );
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateSetoffNumberAsync), GetType(), _logger, new
                {
                    Method = nameof(GenerateSetoffNumberAsync),
                    ServiceType = GetType().Name
                });
                return $"AR{DateTime.Today:yyyyMMdd}001";
            }
        }

        public async Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.AccountsReceivableSetoffs.Where(s => s.SetoffNumber == setoffNumber && !s.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSetoffNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSetoffNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SetoffNumber = setoffNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<ServiceResult> CompleteSetoffAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoff = await context.AccountsReceivableSetoffs.FindAsync(id);
                
                if (setoff == null || setoff.IsDeleted)
                    return ServiceResult.Failure("找不到指定的沖款單");

                if (setoff.IsCompleted)
                    return ServiceResult.Failure("沖款單已經完成");

                setoff.IsCompleted = true;
                setoff.CompletedDate = DateTime.Now;
                setoff.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteSetoffAsync), GetType(), _logger, new
                {
                    Method = nameof(CompleteSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure("完成沖款單時發生錯誤");
            }
        }

        public async Task<ServiceResult> CancelSetoffAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                var setoff = await context.AccountsReceivableSetoffs
                    .Include(s => s.SetoffDetails)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (setoff == null)
                    return ServiceResult.Failure("找不到指定的沖款單");

                if (setoff.IsCompleted)
                    return ServiceResult.Failure("已完成的沖款單無法取消");

                // 回滾相關明細的收款金額
                foreach (var detail in setoff.SetoffDetails)
                {
                    if (detail.SalesOrderDetailId.HasValue)
                    {
                        var salesDetail = await context.SalesOrderDetails.FindAsync(detail.SalesOrderDetailId.Value);
                        if (salesDetail != null)
                        {
                            salesDetail.TotalReceivedAmount -= detail.SetoffAmount;
                            if (salesDetail.TotalReceivedAmount < 0)
                                salesDetail.TotalReceivedAmount = 0;
                        }
                    }
                    else if (detail.SalesReturnDetailId.HasValue)
                    {
                        var returnDetail = await context.SalesReturnDetails.FindAsync(detail.SalesReturnDetailId.Value);
                        if (returnDetail != null)
                        {
                            returnDetail.TotalReceivedAmount -= detail.SetoffAmount;
                            if (returnDetail.TotalReceivedAmount < 0)
                                returnDetail.TotalReceivedAmount = 0;
                        }
                    }
                }

                // 刪除沖款單
                setoff.IsDeleted = true;
                setoff.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelSetoffAsync), GetType(), _logger, new
                {
                    Method = nameof(CancelSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure("取消沖款單時發生錯誤");
            }
        }

        public async Task<ServiceResult> ApproveSetoffAsync(int id, int approverId, bool isApproved, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoff = await context.AccountsReceivableSetoffs.FindAsync(id);

                if (setoff == null || setoff.IsDeleted)
                    return ServiceResult.Failure("找不到指定的沖款單");

                if (setoff.ApprovalStatus != ApprovalStatus.Pending)
                    return ServiceResult.Failure("沖款單已經審核過");

                setoff.ApprovalStatus = isApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                setoff.ApproverId = approverId;
                setoff.ApprovedDate = DateTime.Now;
                setoff.ApprovalRemarks = remarks;
                setoff.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ApproveSetoffAsync), GetType(), _logger, new
                {
                    Method = nameof(ApproveSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ApproverId = approverId,
                    IsApproved = isApproved
                });
                return ServiceResult.Failure("審核沖款單時發生錯誤");
            }
        }

        public async Task<SetoffStatistics> GetStatisticsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                var totalCount = await context.AccountsReceivableSetoffs
                    .Where(s => !s.IsDeleted)
                    .CountAsync();

                var incompleteCount = await context.AccountsReceivableSetoffs
                    .Where(s => !s.IsCompleted && !s.IsDeleted)
                    .CountAsync();

                var pendingApprovalCount = await context.AccountsReceivableSetoffs
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Pending && !s.IsDeleted)
                    .CountAsync();

                var monthlyAmount = await context.AccountsReceivableSetoffs
                    .Where(s => s.SetoffDate >= firstDayOfMonth && !s.IsDeleted)
                    .SumAsync(s => s.TotalSetoffAmount);

                var todayAmount = await context.AccountsReceivableSetoffs
                    .Where(s => s.SetoffDate == today && !s.IsDeleted)
                    .SumAsync(s => s.TotalSetoffAmount);

                var averageAmount = totalCount > 0 
                    ? await context.AccountsReceivableSetoffs
                        .Where(s => !s.IsDeleted)
                        .AverageAsync(s => s.TotalSetoffAmount)
                    : 0;

                return new SetoffStatistics
                {
                    TotalSetoffCount = totalCount,
                    IncompleteSetoffCount = incompleteCount,
                    PendingApprovalCount = pendingApprovalCount,
                    MonthlySetoffAmount = monthlyAmount,
                    TodaySetoffAmount = todayAmount,
                    AverageSetoffAmount = averageAmount
                };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name
                });
                return new SetoffStatistics();
            }
        }

        public async Task<List<ReceivableDetailViewModel>> GetReceivableDetailsAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var result = new List<ReceivableDetailViewModel>();

                // 取得銷貨訂單明細
                var salesDetails = await context.SalesOrderDetails
                    .Include(d => d.SalesOrder)
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Where(d => d.SalesOrder.CustomerId == customerId && !d.IsDeleted && !d.SalesOrder.IsDeleted)
                    .Select(d => new ReceivableDetailViewModel
                    {
                        DetailId = d.Id,
                        DocumentType = "SalesOrder",
                        DocumentNumber = d.SalesOrder.OrderNumber,
                        DocumentDate = d.SalesOrder.OrderDate,
                        ProductName = d.Product.Name,
                        Quantity = d.Quantity,
                        UnitName = d.Unit != null ? d.Unit.Name : null,
                        ReceivableAmount = d.Subtotal,
                        ReceivedAmount = d.TotalReceivedAmount
                    })
                    .Where(d => d.BalanceAmount > 0) // 只顯示有餘額的
                    .ToListAsync();

                result.AddRange(salesDetails);

                // 取得銷貨退回明細 (退回是負的應收)
                var returnDetails = await context.SalesReturnDetails
                    .Include(d => d.SalesReturn)
                    .Include(d => d.Product)
                    .Include(d => d.Unit)
                    .Where(d => d.SalesReturn.CustomerId == customerId && !d.IsDeleted && !d.SalesReturn.IsDeleted)
                    .Select(d => new ReceivableDetailViewModel
                    {
                        DetailId = d.Id,
                        DocumentType = "SalesReturn",
                        DocumentNumber = d.SalesReturn.ReturnNumber,
                        DocumentDate = d.SalesReturn.ReturnDate,
                        ProductName = d.Product.Name,
                        Quantity = d.Quantity,
                        UnitName = d.Unit != null ? d.Unit.Name : null,
                        ReceivableAmount = -d.Subtotal, // 退回是負數
                        ReceivedAmount = d.TotalReceivedAmount
                    })
                    .Where(d => d.BalanceAmount != 0) // 顯示有餘額的
                    .ToListAsync();

                result.AddRange(returnDetails);

                return result.OrderByDescending(d => d.DocumentDate).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivableDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetReceivableDetailsAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<ReceivableDetailViewModel>();
            }
        }

        public override async Task<List<AccountsReceivableSetoff>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Where(s => !s.IsDeleted && 
                               (s.SetoffNumber.Contains(searchTerm) ||
                                s.Customer.CompanyName.Contains(searchTerm)))
                    .OrderByDescending(s => s.SetoffDate)
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
                return new List<AccountsReceivableSetoff>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsReceivableSetoff entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.SetoffNumber))
                    errors.Add("沖款單號不能為空");

                if (entity.CustomerId <= 0)
                    errors.Add("必須選擇客戶");

                if (entity.TotalSetoffAmount <= 0)
                    errors.Add("沖款金額必須大於 0");

                if (!string.IsNullOrWhiteSpace(entity.SetoffNumber) &&
                    await IsSetoffNumberExistsAsync(entity.SetoffNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("沖款單號已存在");

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
                    SetoffNumber = entity.SetoffNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}