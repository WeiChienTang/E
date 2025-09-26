using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款明細服務實作
    /// </summary>
    public class AccountsReceivableSetoffDetailService : GenericManagementService<AccountsReceivableSetoffDetail>, IAccountsReceivableSetoffDetailService
    {
        public AccountsReceivableSetoffDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public AccountsReceivableSetoffDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<AccountsReceivableSetoffDetail>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<AccountsReceivableSetoffDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                    .Include(d => d.SalesOrderDetail)
                        .ThenInclude(sod => sod!.Product)
                    .Include(d => d.SalesReturnDetail)
                        .ThenInclude(srd => srd!.Product)
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

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
                    .Where(d => d.SetoffId == setoffId && !d.IsDeleted)
                    .OrderBy(d => d.DocumentType)
                    .ThenBy(d => d.DocumentNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySetoffIdAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId
                });
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public async Task<ServiceResult> CreateBatchAsync(List<AccountsReceivableSetoffDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                foreach (var detail in details)
                {
                    // 驗證明細
                    var validateResult = await ValidateSetoffDetailAsync(detail);
                    if (!validateResult.IsSuccess)
                        return validateResult;

                    // 新增明細
                    context.AccountsReceivableSetoffDetails.Add(detail);

                    // 更新原始明細的收款金額
                    await UpdateOriginalDetailReceivedAmountAsync(context, detail, detail.SetoffAmount);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateBatchAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateBatchAsync),
                    ServiceType = GetType().Name,
                    DetailsCount = details.Count
                });
                return ServiceResult.Failure("批量建立沖款明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateBatchAsync(List<AccountsReceivableSetoffDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                foreach (var detail in details)
                {
                    var existing = await context.AccountsReceivableSetoffDetails.FindAsync(detail.Id);
                    if (existing == null || existing.IsDeleted)
                        continue;

                    // 計算金額差異
                    var amountDifference = detail.SetoffAmount - existing.SetoffAmount;

                    // 更新明細
                    existing.SetoffAmount = detail.SetoffAmount;
                    existing.AfterReceivedAmount = existing.PreviousReceivedAmount + detail.SetoffAmount;
                    existing.RemainingAmount = existing.ReceivableAmount - existing.AfterReceivedAmount;
                    existing.IsFullyReceived = existing.RemainingAmount <= 0;
                    existing.UpdatedAt = DateTime.Now;

                    // 更新原始明細的收款金額
                    if (amountDifference != 0)
                    {
                        await UpdateOriginalDetailReceivedAmountAsync(context, existing, amountDifference);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateBatchAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateBatchAsync),
                    ServiceType = GetType().Name,
                    DetailsCount = details.Count
                });
                return ServiceResult.Failure("批量更新沖款明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> ValidateSetoffDetailAsync(AccountsReceivableSetoffDetail detail)
        {
            try
            {
                var errors = new List<string>();

                if (!detail.IsValid())
                    errors.Add("沖款明細資料不完整或不正確");

                if (detail.SetoffAmount <= 0)
                    errors.Add("沖款金額必須大於 0");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 驗證是否超過可沖款金額
                if (detail.SalesOrderDetailId.HasValue)
                {
                    var salesDetail = await context.SalesOrderDetails.FindAsync(detail.SalesOrderDetailId.Value);
                    if (salesDetail != null)
                    {
                        var availableAmount = salesDetail.Subtotal - salesDetail.TotalReceivedAmount;
                        if (detail.SetoffAmount > availableAmount)
                            errors.Add($"沖款金額不能超過可沖款金額 {availableAmount:C}");
                    }
                }
                else if (detail.SalesReturnDetailId.HasValue)
                {
                    var returnDetail = await context.SalesReturnDetails.FindAsync(detail.SalesReturnDetailId.Value);
                    if (returnDetail != null)
                    {
                        var availableAmount = Math.Abs(returnDetail.Subtotal) - returnDetail.TotalReceivedAmount;
                        if (detail.SetoffAmount > availableAmount)
                            errors.Add($"沖款金額不能超過可沖款金額 {availableAmount:C}");
                    }
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateSetoffDetailAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateSetoffDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id
                });
                return ServiceResult.Failure("驗證沖款明細時發生錯誤");
            }
        }

        /// <summary>
        /// 更新原始明細的收款金額
        /// </summary>
        private async Task UpdateOriginalDetailReceivedAmountAsync(AppDbContext context, AccountsReceivableSetoffDetail detail, decimal amountChange)
        {
            if (detail.SalesOrderDetailId.HasValue)
            {
                var salesDetail = await context.SalesOrderDetails.FindAsync(detail.SalesOrderDetailId.Value);
                if (salesDetail != null)
                {
                    salesDetail.TotalReceivedAmount += amountChange;
                    if (salesDetail.TotalReceivedAmount < 0)
                        salesDetail.TotalReceivedAmount = 0;
                    salesDetail.UpdatedAt = DateTime.Now;
                }
            }
            else if (detail.SalesReturnDetailId.HasValue)
            {
                var returnDetail = await context.SalesReturnDetails.FindAsync(detail.SalesReturnDetailId.Value);
                if (returnDetail != null)
                {
                    returnDetail.TotalReceivedAmount += amountChange;
                    if (returnDetail.TotalReceivedAmount < 0)
                        returnDetail.TotalReceivedAmount = 0;
                    returnDetail.UpdatedAt = DateTime.Now;
                }
            }
        }

        public override async Task<List<AccountsReceivableSetoffDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Include(d => d.Setoff)
                    .Where(d => !d.IsDeleted &&
                               (d.DocumentNumber != null && d.DocumentNumber.Contains(searchTerm) ||
                                d.ProductName != null && d.ProductName.Contains(searchTerm) ||
                                d.Setoff.SetoffNumber.Contains(searchTerm)))
                    .OrderByDescending(d => d.CreatedAt)
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
                return new List<AccountsReceivableSetoffDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsReceivableSetoffDetail entity)
        {
            return await ValidateSetoffDetailAsync(entity);
        }
    }
}