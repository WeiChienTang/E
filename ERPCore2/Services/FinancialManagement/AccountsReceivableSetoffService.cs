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
    /// 應收帳款沖款單服務實作
    /// </summary>
    public class AccountsReceivableSetoffService : GenericManagementService<AccountsReceivableSetoff>, IAccountsReceivableSetoffService
    {
        public AccountsReceivableSetoffService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<AccountsReceivableSetoff>> logger) : base(contextFactory, logger)
        {
        }

        public AccountsReceivableSetoffService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<AccountsReceivableSetoff>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
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
                    .FirstOrDefaultAsync(s => s.Id == id);
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

        public override async Task<List<AccountsReceivableSetoff>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .Where(s => (
                        s.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        s.Customer.CompanyName.ToLower().Contains(searchTermLower) ||
                        (!string.IsNullOrEmpty(s.PaymentAccount) && s.PaymentAccount.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(s.Remarks) && s.Remarks.ToLower().Contains(searchTermLower))
                    ))
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
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
                
                // 檢查必填欄位
                if (string.IsNullOrWhiteSpace(entity.SetoffNumber))
                    errors.Add("沖款單號不能為空");
                
                if (entity.CustomerId <= 0)
                    errors.Add("必須選擇客戶");
                
                if (entity.SetoffDate == default)
                    errors.Add("沖款日期不能為空");
                
                // 檢查沖款單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.SetoffNumber) && 
                    await IsSetoffNumberExistsAsync(entity.SetoffNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("沖款單號已存在");
                
                // 檢查沖款日期不能是未來日期
                if (entity.SetoffDate > DateTime.Today)
                    errors.Add("沖款日期不能是未來日期");
                
                // 檢查總沖款金額
                if (entity.TotalSetoffAmount < 0)
                    errors.Add("總沖款金額不能為負數");
                
                // 檢查客戶是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var customerExists = await context.Customers
                    .AnyAsync(c => c.Id == entity.CustomerId);
                if (!customerExists)
                    errors.Add("選擇的客戶不存在或已被刪除");
                
                // 檢查收款方式是否存在（如果有選擇的話）
                if (entity.PaymentMethodId.HasValue)
                {
                    var paymentMethodExists = await context.PaymentMethods
                        .AnyAsync(pm => pm.Id == entity.PaymentMethodId.Value && pm.Status == EntityStatus.Active);
                    if (!paymentMethodExists)
                        errors.Add("選擇的收款方式不存在或已停用");
                }
                
                // 檢查審核者是否存在（如果有選擇的話）
                if (entity.ApproverId.HasValue)
                {
                    var approverExists = await context.Employees
                        .AnyAsync(e => e.Id == entity.ApproverId.Value && e.Status == EntityStatus.Active);
                    if (!approverExists)
                        errors.Add("選擇的審核者不存在或已停用");
                }
                
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
                    SetoffNumber = entity.SetoffNumber 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 特殊業務方法

        public async Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.AccountsReceivableSetoffs
                    .Where(s => s.SetoffNumber == setoffNumber);
                
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSetoffNumberExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsSetoffNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SetoffNumber = setoffNumber,
                    ExcludeId = excludeId 
                });
                return false;
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
                    .Include(s => s.Approver)
                    .Where(s => s.CustomerId == customerId)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId 
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Approver)
                    .Where(s => s.SetoffDate >= startDate && s.SetoffDate <= endDate)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
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
                return new List<AccountsReceivableSetoff>();
            }
        }

        public async Task<AccountsReceivableSetoff?> GetWithDetailsAsync(int id)
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
                    .FirstOrDefaultAsync(s => s.Id == id);
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

        public async Task<ServiceResult> CompleteSetoffAsync(int id, int approverId, string? approvalRemarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var setoff = await context.AccountsReceivableSetoffs
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (setoff == null)
                    return ServiceResult.Failure("沖款單不存在");
                
                if (setoff.IsCompleted)
                    return ServiceResult.Failure("沖款單已經完成");
                
                // 檢查審核者是否存在
                var approverExists = await context.Employees
                    .AnyAsync(e => e.Id == approverId && e.Status == EntityStatus.Active);
                if (!approverExists)
                    return ServiceResult.Failure("審核者不存在或已停用");
                
                // 檢查是否有明細
                var hasDetails = await context.AccountsReceivableSetoffDetails
                    .AnyAsync(d => d.SetoffId == id);
                if (!hasDetails)
                    return ServiceResult.Failure("沖款單必須有明細才能完成");
                
                setoff.IsCompleted = true;
                setoff.CompletedDate = DateTime.Now;
                setoff.ApproverId = approverId;
                setoff.ApprovedDate = DateTime.Now;
                setoff.ApprovalRemarks = approvalRemarks;
                setoff.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(CompleteSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    ApproverId = approverId 
                });
                return ServiceResult.Failure("完成沖款單時發生錯誤");
            }
        }

        public async Task<ServiceResult> UncompleteSetoffAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var setoff = await context.AccountsReceivableSetoffs
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (setoff == null)
                    return ServiceResult.Failure("沖款單不存在");
                
                if (!setoff.IsCompleted)
                    return ServiceResult.Failure("沖款單尚未完成");
                
                setoff.IsCompleted = false;
                setoff.CompletedDate = null;
                setoff.ApproverId = null;
                setoff.ApprovedDate = null;
                setoff.ApprovalRemarks = null;
                setoff.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UncompleteSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(UncompleteSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("取消完成沖款單時發生錯誤");
            }
        }

        public async Task<decimal> CalculateTotalAmountAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffDetails
                    .Where(d => d.SetoffId == id)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalAmountAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalAmountAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return 0;
            }
        }

        public async Task<List<AccountsReceivableSetoff>> GetIncompleteSetoffsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsReceivableSetoffs
                    .Include(s => s.Customer)
                    .Include(s => s.PaymentMethod)
                    .Where(s => !s.IsCompleted)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetIncompleteSetoffsAsync), GetType(), _logger, new { 
                    Method = nameof(GetIncompleteSetoffsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<AccountsReceivableSetoff>();
            }
        }

        #endregion
    }
}