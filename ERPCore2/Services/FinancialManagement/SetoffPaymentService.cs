using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款收款記錄服務實作
    /// </summary>
    public class SetoffPaymentService : GenericManagementService<SetoffPayment>, ISetoffPaymentService
    {
        public SetoffPaymentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffPayment>> logger) : base(contextFactory, logger)
        {
        }

        public SetoffPaymentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫 GetAllAsync 以包含關聯資料
        /// </summary>
        public override async Task<List<SetoffPayment>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .Include(sp => sp.SetoffDocument)
                    .Include(sp => sp.Bank)
                    .Include(sp => sp.PaymentMethod)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SetoffPayment>();
            }
        }

        /// <summary>
        /// 覆寫 GetByIdAsync 以包含關聯資料
        /// </summary>
        public override async Task<SetoffPayment?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .Include(sp => sp.SetoffDocument)
                    .Include(sp => sp.Bank)
                    .Include(sp => sp.PaymentMethod)
                    .FirstOrDefaultAsync(sp => sp.Id == id);
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

        /// <summary>
        /// 實作搜尋功能
        /// </summary>
        public override async Task<List<SetoffPayment>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .Include(sp => sp.SetoffDocument)
                    .Include(sp => sp.Bank)
                    .Include(sp => sp.PaymentMethod)
                    .Where(sp => (sp.CheckNumber != null && sp.CheckNumber.Contains(searchTerm)) ||
                                (sp.Bank != null && sp.Bank.BankName.Contains(searchTerm)) ||
                                (sp.PaymentMethod != null && sp.PaymentMethod.Name.Contains(searchTerm)))
                    .OrderByDescending(sp => sp.CreatedAt)
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
                return new List<SetoffPayment>();
            }
        }

        /// <summary>
        /// 根據沖款單ID取得所有收款記錄
        /// </summary>
        public async Task<List<SetoffPayment>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .AsNoTracking()  // 不追蹤實體，避免後續操作時的追蹤衝突
                    .Include(sp => sp.Bank)
                    .Include(sp => sp.PaymentMethod)
                    .Where(sp => sp.SetoffDocumentId == setoffDocumentId)
                    .OrderBy(sp => sp.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffDocumentIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySetoffDocumentIdAsync),
                    ServiceType = GetType().Name,
                    SetoffDocumentId = setoffDocumentId
                });
                return new List<SetoffPayment>();
            }
        }

        /// <summary>
        /// 計算指定沖款單的總收款金額
        /// </summary>
        public async Task<decimal> GetTotalReceivedAmountAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .Where(sp => sp.SetoffDocumentId == setoffDocumentId)
                    .SumAsync(sp => sp.ReceivedAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReceivedAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTotalReceivedAmountAsync),
                    ServiceType = GetType().Name,
                    SetoffDocumentId = setoffDocumentId
                });
                return 0;
            }
        }

        /// <summary>
        /// 計算指定沖款單的總折讓金額
        /// </summary>
        public async Task<decimal> GetTotalAllowanceAmountAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPayments
                    .Where(sp => sp.SetoffDocumentId == setoffDocumentId)
                    .SumAsync(sp => sp.AllowanceAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalAllowanceAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTotalAllowanceAmountAsync),
                    ServiceType = GetType().Name,
                    SetoffDocumentId = setoffDocumentId
                });
                return 0;
            }
        }

        /// <summary>
        /// 檢查支票號碼是否已存在
        /// </summary>
        public async Task<bool> IsCheckNumberExistsAsync(string checkNumber, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(checkNumber))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffPayments.Where(sp => sp.CheckNumber == checkNumber);
                if (excludeId.HasValue)
                    query = query.Where(sp => sp.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCheckNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsCheckNumberExistsAsync),
                    ServiceType = GetType().Name,
                    CheckNumber = checkNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 實作驗證邏輯
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffPayment entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (entity.SetoffDocumentId <= 0)
                    errors.Add("沖款單為必填");

                // 驗證金額
                if (entity.ReceivedAmount < 0)
                    errors.Add("收款金額不可為負數");

                if (entity.AllowanceAmount < 0)
                    errors.Add("折讓金額不可為負數");

                if (entity.ReceivedAmount == 0 && entity.AllowanceAmount == 0)
                    errors.Add("收款金額與折讓金額不可同時為零");

                // 驗證支票號碼唯一性
                if (!string.IsNullOrWhiteSpace(entity.CheckNumber))
                {
                    if (await IsCheckNumberExistsAsync(entity.CheckNumber, entity.Id == 0 ? null : entity.Id))
                        errors.Add("支票號碼已存在");
                }

                // 驗證支票相關欄位
                if (!string.IsNullOrWhiteSpace(entity.CheckNumber))
                {
                    if (!entity.DueDate.HasValue)
                        errors.Add("使用支票時，到期日為必填");
                    
                    if (!entity.BankId.HasValue)
                        errors.Add("使用支票時，銀行別為必填");
                }

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
                    SetoffDocumentId = entity.SetoffDocumentId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
