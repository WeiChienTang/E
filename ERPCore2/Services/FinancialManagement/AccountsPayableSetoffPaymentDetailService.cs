using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應付帳款沖款付款明細服務實作
    /// </summary>
    public class AccountsPayableSetoffPaymentDetailService : GenericManagementService<AccountsPayableSetoffPaymentDetail>, IAccountsPayableSetoffPaymentDetailService
    {
        public AccountsPayableSetoffPaymentDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<AccountsPayableSetoffPaymentDetail>> logger) : base(contextFactory, logger)
        {
        }

        public AccountsPayableSetoffPaymentDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<AccountsPayableSetoffPaymentDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffPaymentDetails
                    .Include(d => d.Setoff)
                    .Include(d => d.PaymentMethod)
                    .Include(d => d.Bank)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<AccountsPayableSetoffPaymentDetail>();
            }
        }

        public override async Task<AccountsPayableSetoffPaymentDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffPaymentDetails
                    .Include(d => d.Setoff)
                    .Include(d => d.PaymentMethod)
                    .Include(d => d.Bank)
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger,
                    new { Id = id });
                return null;
            }
        }

        public override async Task<List<AccountsPayableSetoffPaymentDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.AccountsPayableSetoffPaymentDetails
                    .Include(d => d.Setoff)
                    .Include(d => d.PaymentMethod)
                    .Include(d => d.Bank)
                    .Where(d =>
                        (d.PaymentMethod != null && d.PaymentMethod.Name.ToLower().Contains(searchTermLower)) ||
                        (d.Bank != null && d.Bank.BankName.ToLower().Contains(searchTermLower)) ||
                        (d.AccountNumber != null && d.AccountNumber.ToLower().Contains(searchTermLower)) ||
                        (d.TransactionReference != null && d.TransactionReference.ToLower().Contains(searchTermLower)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<AccountsPayableSetoffPaymentDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsPayableSetoffPaymentDetail entity)
        {
            try
            {
                // 驗證付款方式
                if (entity.PaymentMethodId <= 0)
                {
                    return ServiceResult.Failure("請選擇付款方式");
                }

                // 驗證金額
                if (entity.Amount <= 0)
                {
                    return ServiceResult.Failure("付款金額必須大於 0");
                }

                // 驗證沖款單ID
                if (entity.SetoffId <= 0)
                {
                    return ServiceResult.Failure("沖款單ID無效");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger);
                return ServiceResult.Failure($"驗證失敗: {ex.Message}");
            }
        }

        #endregion

        #region 介面實作

        /// <summary>
        /// 依據沖款單ID取得付款明細列表
        /// </summary>
        public async Task<List<SetoffPaymentDetailDto>> GetBySetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.AccountsPayableSetoffPaymentDetails
                    .Include(d => d.PaymentMethod)
                    .Include(d => d.Bank)
                    .Where(d => d.SetoffId == setoffId)
                    .OrderBy(d => d.CreatedAt)
                    .ToListAsync();

                return details.Select(d => new SetoffPaymentDetailDto
                {
                    Id = d.Id,
                    SetoffId = d.SetoffId,
                    PaymentMethodId = d.PaymentMethodId,
                    PaymentMethodName = d.PaymentMethod?.Name ?? string.Empty,
                    BankId = d.BankId,
                    BankName = d.Bank?.BankName,
                    Amount = d.Amount,
                    AccountNumber = d.AccountNumber,
                    TransactionReference = d.TransactionReference,
                    PaymentDate = d.PaymentDate,
                    Remarks = d.Remarks
                }).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffIdAsync), GetType(), _logger,
                    new { SetoffId = setoffId });
                return new List<SetoffPaymentDetailDto>();
            }
        }

        /// <summary>
        /// 批次儲存付款明細
        /// </summary>
        public async Task<(bool Success, string Message)> SavePaymentDetailsAsync(
            int setoffId,
            List<SetoffPaymentDetailDto> details,
            List<int> deletedIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 刪除已標記刪除的明細
                    if (deletedIds.Any())
                    {
                        var itemsToDelete = await context.AccountsPayableSetoffPaymentDetails
                            .Where(d => deletedIds.Contains(d.Id))
                            .ToListAsync();

                        context.AccountsPayableSetoffPaymentDetails.RemoveRange(itemsToDelete);
                    }

                    // 2. 過濾出有效的明細 (有付款方式且金額 > 0)
                    var validDetails = details.Where(d => 
                        d.PaymentMethodId.HasValue && 
                        d.PaymentMethodId.Value > 0 && 
                        d.Amount > 0
                    ).ToList();

                    // 3. 新增或更新明細
                    foreach (var dto in validDetails)
                    {
                        if (dto.Id > 0)
                        {
                            // 更新現有明細
                            var existing = await context.AccountsPayableSetoffPaymentDetails
                                .FirstOrDefaultAsync(d => d.Id == dto.Id);

                            if (existing != null)
                            {
                                existing.PaymentMethodId = dto.PaymentMethodId!.Value;
                                existing.BankId = dto.BankId;
                                existing.Amount = dto.Amount;
                                existing.AccountNumber = dto.AccountNumber;
                                existing.TransactionReference = dto.TransactionReference;
                                existing.PaymentDate = dto.PaymentDate;
                                existing.Remarks = dto.Remarks;
                                existing.UpdatedAt = DateTime.Now;

                                context.AccountsPayableSetoffPaymentDetails.Update(existing);
                            }
                        }
                        else
                        {
                            // 新增明細
                            var newDetail = new AccountsPayableSetoffPaymentDetail
                            {
                                SetoffId = setoffId,
                                PaymentMethodId = dto.PaymentMethodId!.Value,
                                BankId = dto.BankId,
                                Amount = dto.Amount,
                                AccountNumber = dto.AccountNumber,
                                TransactionReference = dto.TransactionReference,
                                PaymentDate = dto.PaymentDate,
                                Remarks = dto.Remarks,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            await context.AccountsPayableSetoffPaymentDetails.AddAsync(newDetail);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "付款明細儲存成功");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SavePaymentDetailsAsync), GetType(), _logger,
                        new { SetoffId = setoffId, DetailCount = details.Count, DeletedCount = deletedIds.Count });
                    return (false, $"儲存失敗: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SavePaymentDetailsAsync), GetType(), _logger);
                return (false, $"儲存失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證付款明細總額是否符合沖款總額
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidatePaymentDetailsAsync(
            int setoffId,
            List<SetoffPaymentDetailDto> details,
            decimal totalSetoffAmount)
        {
            try
            {
                // 過濾有效明細
                var validDetails = details.Where(d =>
                    d.PaymentMethodId.HasValue &&
                    d.PaymentMethodId.Value > 0 &&
                    d.Amount > 0
                ).ToList();

                if (!validDetails.Any())
                {
                    return (false, "請至少新增一筆付款明細");
                }

                // 計算總付款金額
                var totalPaymentAmount = validDetails.Sum(d => d.Amount);

                if (totalPaymentAmount != totalSetoffAmount)
                {
                    return (false, $"付款總額 {totalPaymentAmount:N2} 與沖款總額 {totalSetoffAmount:N2} 不符");
                }

                // 驗證每筆明細
                foreach (var detail in validDetails)
                {
                    var validation = detail.ValidateAll(totalSetoffAmount);
                    if (!validation.IsValid)
                    {
                        return (false, string.Join("; ", validation.Errors));
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidatePaymentDetailsAsync), GetType(), _logger);
                return (false, $"驗證失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 計算付款明細總額
        /// </summary>
        public async Task<decimal> CalculateTotalPaymentAmountAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffPaymentDetails
                    .Where(d => d.SetoffId == setoffId)
                    .SumAsync(d => d.Amount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalPaymentAmountAsync), GetType(), _logger,
                    new { SetoffId = setoffId });
                return 0;
            }
        }

        /// <summary>
        /// 刪除指定沖款單的所有付款明細
        /// </summary>
        public async Task<bool> DeleteBySetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.AccountsPayableSetoffPaymentDetails
                    .Where(d => d.SetoffId == setoffId)
                    .ToListAsync();

                if (details.Any())
                {
                    context.AccountsPayableSetoffPaymentDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBySetoffIdAsync), GetType(), _logger,
                    new { SetoffId = setoffId });
                return false;
            }
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 判斷付款方式是否需要銀行資訊
        /// </summary>
        #endregion
    }
}
