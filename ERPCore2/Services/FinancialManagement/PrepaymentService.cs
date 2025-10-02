using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預先款項服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class PrepaymentService : GenericManagementService<Prepayment>, IPrepaymentService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public PrepaymentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<Prepayment>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public PrepaymentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        /// <summary>
        /// 覆寫取得所有資料，包含相關資料
        /// </summary>
        public override async Task<List<Prepayment>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Include(p => p.Supplier)
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenBy(p => p.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 覆寫依據 ID 取得資料
        /// </summary>
        public override async Task<Prepayment?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);
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
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<Prepayment>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Include(p => p.Supplier)
                    .Where(p => (p.Code != null && p.Code.Contains(searchTerm)) ||
                               (p.Remarks != null && p.Remarks.Contains(searchTerm)) ||
                               (p.Customer != null && p.Customer.CompanyName.Contains(searchTerm)) ||
                               (p.Supplier != null && p.Supplier.CompanyName.Contains(searchTerm)))
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenBy(p => p.Code)
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
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 覆寫驗證邏輯
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Prepayment entity)
        {
            try
            {
                var errors = new List<string>();

                // 必填欄位驗證
                if (entity.PaymentDate == default)
                    errors.Add("款項日期不能為空");

                if (entity.Amount <= 0)
                    errors.Add("款項金額必須大於0");

                // 代碼唯一性驗證
                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("代碼已存在");

                // 業務規則驗證
                switch (entity.PrepaymentType)
                {
                    case PrepaymentType.Prepayment:
                        if (!entity.CustomerId.HasValue)
                            errors.Add("預收款必須指定客戶");
                        if (entity.SupplierId.HasValue)
                            errors.Add("預收款不應指定供應商");
                        break;

                    case PrepaymentType.Prepaid:
                        if (!entity.SupplierId.HasValue)
                            errors.Add("預付款必須指定供應商");
                        if (entity.CustomerId.HasValue)
                            errors.Add("預付款不應指定客戶");
                        break;

                    case PrepaymentType.Other:
                        // 其他類型可以不指定客戶或供應商
                        break;
                }

                // 確保不會同時指定客戶和供應商
                if (entity.CustomerId.HasValue && entity.SupplierId.HasValue)
                    errors.Add("不能同時指定客戶和供應商");

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
                    EntityCode = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自訂方法

        /// <summary>
        /// 檢查預先款項代碼是否存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Prepayments.Where(p => p.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 依據客戶取得預收款列表
        /// </summary>
        public async Task<List<Prepayment>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Where(p => p.CustomerId == customerId)
                    .OrderByDescending(p => p.PaymentDate)
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
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 依據供應商取得預付款列表
        /// </summary>
        public async Task<List<Prepayment>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Supplier)
                    .Where(p => p.SupplierId == supplierId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 依據款項類型取得列表
        /// </summary>
        public async Task<List<Prepayment>> GetByPrepaymentTypeAsync(PrepaymentType prepaymentType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Include(p => p.Supplier)
                    .Where(p => p.PrepaymentType == prepaymentType)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPrepaymentTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPrepaymentTypeAsync),
                    ServiceType = GetType().Name,
                    PrepaymentType = prepaymentType
                });
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 取得指定日期範圍的款項
        /// </summary>
        public async Task<List<Prepayment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Prepayments
                    .Include(p => p.Customer)
                    .Include(p => p.Supplier)
                    .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenBy(p => p.Code)
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
                return new List<Prepayment>();
            }
        }

        /// <summary>
        /// 取得客戶的可用預收款餘額
        /// </summary>
        public async Task<decimal> GetAvailableBalanceByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var total = await context.Prepayments
                    .Where(p => p.CustomerId == customerId && 
                               p.PrepaymentType == PrepaymentType.Prepayment &&
                               p.Status == EntityStatus.Active)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableBalanceByCustomerAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAvailableBalanceByCustomerAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return 0;
            }
        }

        /// <summary>
        /// 取得供應商的可用預付款餘額
        /// </summary>
        public async Task<decimal> GetAvailableBalanceBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var total = await context.Prepayments
                    .Where(p => p.SupplierId == supplierId && 
                               p.PrepaymentType == PrepaymentType.Prepaid &&
                               p.Status == EntityStatus.Active)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableBalanceBySupplierAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAvailableBalanceBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return 0;
            }
        }

        #endregion
    }
}
