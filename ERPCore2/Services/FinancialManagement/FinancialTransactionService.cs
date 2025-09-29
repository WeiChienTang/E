using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 財務交易服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class FinancialTransactionService : GenericManagementService<FinancialTransaction>, IFinancialTransactionService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public FinancialTransactionService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<FinancialTransaction>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public FinancialTransactionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<FinancialTransaction>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ThenByDescending(ft => ft.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<FinancialTransaction>();
            }
        }

        public override async Task<FinancialTransaction?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Include(ft => ft.ReversalTransaction)
                    .FirstOrDefaultAsync(ft => ft.Id == id);
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

        public override async Task<List<FinancialTransaction>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Where(ft => ft.TransactionNumber.Contains(searchTerm) ||
                                ft.Description!.Contains(searchTerm) ||
                                ft.SourceDocumentNumber!.Contains(searchTerm) ||
                                ft.Customer!.CompanyName.Contains(searchTerm) ||
                                ft.Company.CompanyName.Contains(searchTerm))
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<FinancialTransaction>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(FinancialTransaction entity)
        {
            try
            {
                var errors = new List<string>();
                
                // 必填欄位驗證
                if (string.IsNullOrWhiteSpace(entity.TransactionNumber))
                    errors.Add("交易單號不能為空");
                
                if (entity.Amount <= 0)
                    errors.Add("交易金額必須大於0");
                
                if (entity.CompanyId <= 0)
                    errors.Add("必須選擇公司");
                
                // 交易單號唯一性驗證
                if (!string.IsNullOrWhiteSpace(entity.TransactionNumber) && 
                    await IsTransactionNumberExistsAsync(entity.TransactionNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("交易單號已存在");
                
                // 業務規則驗證
                if (entity.CustomerId.HasValue && entity.VendorId.HasValue)
                    errors.Add("不能同時指定客戶和供應商");
                
                if (entity.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableSetoff && 
                    !entity.CustomerId.HasValue)
                    errors.Add("應收沖款交易必須指定客戶");
                
                if (entity.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff && 
                    !entity.VendorId.HasValue)
                    errors.Add("應付沖款交易必須指定供應商");
                
                // 外幣交易驗證
                if (entity.OriginalAmount.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(entity.CurrencyCode))
                        errors.Add("外幣交易必須指定貨幣代碼");
                    
                    if (!entity.ExchangeRate.HasValue || entity.ExchangeRate <= 0)
                        errors.Add("外幣交易必須指定有效的匯率");
                }
                
                // 沖銷交易驗證
                if (entity.IsReversed)
                {
                    if (string.IsNullOrWhiteSpace(entity.ReversalReason))
                        errors.Add("已沖銷交易必須填寫沖銷原因");
                    
                    if (!entity.ReversedDate.HasValue)
                        errors.Add("已沖銷交易必須填寫沖銷日期");
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
                    TransactionNumber = entity.TransactionNumber 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自訂方法

        public async Task<bool> IsTransactionNumberExistsAsync(string transactionNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.FinancialTransactions.Where(ft => ft.TransactionNumber == transactionNumber);
                if (excludeId.HasValue)
                    query = query.Where(ft => ft.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTransactionNumberExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsTransactionNumberExistsAsync),
                    ServiceType = GetType().Name,
                    TransactionNumber = transactionNumber,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<List<FinancialTransaction>> GetTransactionsByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Where(ft => ft.CustomerId == customerId)
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTransactionsByCustomerIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetTransactionsByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId 
                });
                return new List<FinancialTransaction>();
            }
        }

        public async Task<List<FinancialTransaction>> GetTransactionsByCompanyIdAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Where(ft => ft.CompanyId == companyId)
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTransactionsByCompanyIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetTransactionsByCompanyIdAsync),
                    ServiceType = GetType().Name,
                    CompanyId = companyId 
                });
                return new List<FinancialTransaction>();
            }
        }

        public async Task<List<FinancialTransaction>> GetTransactionsByTypeAsync(FinancialTransactionTypeEnum transactionType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Where(ft => ft.TransactionType == transactionType)
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTransactionsByTypeAsync), GetType(), _logger, new { 
                    Method = nameof(GetTransactionsByTypeAsync),
                    ServiceType = GetType().Name,
                    TransactionType = transactionType.ToString() 
                });
                return new List<FinancialTransaction>();
            }
        }

        public async Task<List<FinancialTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.FinancialTransactions
                    .Include(ft => ft.Customer)
                    .Include(ft => ft.Company)
                    .Include(ft => ft.PaymentMethod)
                    .Where(ft => ft.TransactionDate >= startDate && ft.TransactionDate <= endDate)
                    .OrderByDescending(ft => ft.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTransactionsByDateRangeAsync), GetType(), _logger, new { 
                    Method = nameof(GetTransactionsByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return new List<FinancialTransaction>();
            }
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Customers
                    .Where(c => c.Status == EntityStatus.Active)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCustomersAsync), GetType(), _logger, new { 
                    Method = nameof(GetCustomersAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Customer>();
            }
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Companies
                    .Where(c => c.Status == EntityStatus.Active)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompaniesAsync), GetType(), _logger, new { 
                    Method = nameof(GetCompaniesAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Company>();
            }
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaymentMethods
                    .Where(pm => pm.Status == EntityStatus.Active)
                    .OrderBy(pm => pm.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPaymentMethodsAsync), GetType(), _logger, new { 
                    Method = nameof(GetPaymentMethodsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PaymentMethod>();
            }
        }

        public async Task<string> GenerateNextTransactionNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var prefix = $"FT{today:yyyyMMdd}";
                
                var lastTransaction = await context.FinancialTransactions
                    .Where(ft => ft.TransactionNumber.StartsWith(prefix))
                    .OrderByDescending(ft => ft.TransactionNumber)
                    .FirstOrDefaultAsync();
                
                var nextNumber = 1;
                if (lastTransaction != null && lastTransaction.TransactionNumber.Length > prefix.Length)
                {
                    var numberPart = lastTransaction.TransactionNumber.Substring(prefix.Length);
                    if (int.TryParse(numberPart, out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }
                
                return $"{prefix}{nextNumber:D4}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateNextTransactionNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateNextTransactionNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"FT{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        public async Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reversalReason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var transaction = await context.FinancialTransactions.FindAsync(transactionId);
                if (transaction == null)
                    return ServiceResult.Failure("找不到指定的交易記錄");
                
                if (transaction.IsReversed)
                    return ServiceResult.Failure("此交易已被沖銷");
                
                // 建立沖銷交易
                var reversalTransaction = new FinancialTransaction
                {
                    TransactionNumber = await GenerateNextTransactionNumberAsync(),
                    TransactionType = transaction.TransactionType,
                    TransactionDate = DateTime.Now,
                    Amount = -transaction.Amount, // 負值表示沖銷
                    Description = $"沖銷交易：{transaction.TransactionNumber}",
                    CustomerId = transaction.CustomerId,
                    VendorId = transaction.VendorId,
                    CompanyId = transaction.CompanyId,
                    SourceDocumentType = "財務交易沖銷",
                    SourceDocumentId = transaction.Id,
                    SourceDocumentNumber = transaction.TransactionNumber,
                    PaymentMethodId = transaction.PaymentMethodId,
                    PaymentAccount = transaction.PaymentAccount,
                    ReferenceNumber = transaction.ReferenceNumber,
                    BalanceBefore = transaction.BalanceAfter,
                    BalanceAfter = transaction.BalanceBefore,
                    OriginalAmount = transaction.OriginalAmount.HasValue ? -transaction.OriginalAmount : null,
                    CurrencyCode = transaction.CurrencyCode,
                    ExchangeRate = transaction.ExchangeRate,
                    CreatedAt = DateTime.Now,
                    Status = EntityStatus.Active
                };
                
                context.FinancialTransactions.Add(reversalTransaction);
                await context.SaveChangesAsync();
                
                // 更新原交易狀態
                transaction.IsReversed = true;
                transaction.ReversedDate = DateTime.Now;
                transaction.ReversalReason = reversalReason;
                transaction.ReversalTransactionId = reversalTransaction.Id;
                transaction.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReverseTransactionAsync), GetType(), _logger, new { 
                    Method = nameof(ReverseTransactionAsync),
                    ServiceType = GetType().Name,
                    TransactionId = transactionId,
                    ReversalReason = reversalReason 
                });
                return ServiceResult.Failure("沖銷交易過程發生錯誤");
            }
        }

        #endregion
    }
}