using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項服務實作
    /// </summary>
    public class SetoffPrepaymentService : GenericManagementService<SetoffPrepayment>, ISetoffPrepaymentService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SetoffPrepaymentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffPrepayment>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SetoffPrepaymentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含相關資料
        /// </summary>
        public override async Task<List<SetoffPrepayment>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
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
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 覆寫取得單一資料，包含相關資料
        /// </summary>
        public override async Task<SetoffPrepayment?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
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
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<SetoffPrepayment>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
                    .Where(sp => (sp.SourceDocumentCode != null && sp.SourceDocumentCode.Contains(searchTerm)) ||
                                (sp.Customer != null && sp.Customer.CompanyName != null && sp.Customer.CompanyName.Contains(searchTerm)) ||
                                (sp.Supplier != null && sp.Supplier.CompanyName != null && sp.Supplier.CompanyName.Contains(searchTerm)))
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
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 覆寫驗證功能
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffPrepayment entity)
        {
            try
            {
                var errors = new List<string>();

                // 檢查來源單號
                if (string.IsNullOrWhiteSpace(entity.SourceDocumentCode))
                    errors.Add("來源單號為必填");

                // 檢查金額
                if (entity.Amount <= 0)
                    errors.Add("金額必須大於0");

                // 檢查已用金額不能大於總金額
                if (entity.UsedAmount > entity.Amount)
                    errors.Add("已用金額不能大於總金額");

                // 檢查已用金額不能小於0
                if (entity.UsedAmount < 0)
                    errors.Add("已用金額不能小於0");

                // 檢查客戶或供應商至少要有一個
                if (!entity.CustomerId.HasValue && !entity.SupplierId.HasValue)
                    errors.Add("客戶或供應商至少需填寫一個");

                // 檢查客戶和供應商不能同時存在
                if (entity.CustomerId.HasValue && entity.SupplierId.HasValue)
                    errors.Add("客戶和供應商不能同時填寫");

                // 檢查來源單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.SourceDocumentCode) &&
                    await IsSourceDocumentCodeExistsAsync(entity.SourceDocumentCode, entity.Id == 0 ? null : entity.Id))
                    errors.Add("來源單號已存在");

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
                    SourceDocumentCode = entity.SourceDocumentCode
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 根據來源單號取得預收付款項
        /// </summary>
        public async Task<SetoffPrepayment?> GetBySourceDocumentCodeAsync(string sourceDocumentCode)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
                    .FirstOrDefaultAsync(sp => sp.SourceDocumentCode == sourceDocumentCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySourceDocumentCodeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySourceDocumentCodeAsync),
                    ServiceType = GetType().Name,
                    SourceDocumentCode = sourceDocumentCode
                });
                return null;
            }
        }

        /// <summary>
        /// 根據客戶ID取得可用的預收款項
        /// </summary>
        public async Task<List<SetoffPrepayment>> GetAvailableByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.SetoffDocument)
                    .Where(sp => sp.CustomerId == customerId && sp.UsedAmount < sp.Amount)
                    .OrderBy(sp => sp.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableByCustomerIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAvailableByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 根據供應商ID取得可用的預付款項
        /// </summary>
        public async Task<List<SetoffPrepayment>> GetAvailableBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
                    .Where(sp => sp.SupplierId == supplierId && sp.UsedAmount < sp.Amount)
                    .OrderBy(sp => sp.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableBySupplierIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAvailableBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 根據沖款單ID取得預收付款項
        /// </summary>
        public async Task<List<SetoffPrepayment>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
                    .Where(sp => sp.SetoffDocumentId == setoffDocumentId)
                    .OrderBy(sp => sp.CreatedAt)
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
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 根據預收付類型取得款項
        /// </summary>
        public async Task<List<SetoffPrepayment>> GetByPrepaymentTypeAsync(int prepaymentTypeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepayments
                    .Include(sp => sp.Customer)
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.SetoffDocument)
                    .Include(sp => sp.PrepaymentType)
                    .Where(sp => sp.PrepaymentTypeId == prepaymentTypeId)
                    .OrderByDescending(sp => sp.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPrepaymentTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPrepaymentTypeAsync),
                    ServiceType = GetType().Name,
                    PrepaymentTypeId = prepaymentTypeId
                });
                return new List<SetoffPrepayment>();
            }
        }

        /// <summary>
        /// 檢查來源單號是否存在
        /// </summary>
        public async Task<bool> IsSourceDocumentCodeExistsAsync(string sourceDocumentCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffPrepayments.Where(sp => sp.SourceDocumentCode == sourceDocumentCode);
                if (excludeId.HasValue)
                    query = query.Where(sp => sp.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSourceDocumentCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSourceDocumentCodeExistsAsync),
                    ServiceType = GetType().Name,
                    SourceDocumentCode = sourceDocumentCode,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 更新已用金額
        /// </summary>
        public async Task<ServiceResult> UpdateUsedAmountAsync(int id, decimal amountToAdd)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var prepayment = await context.SetoffPrepayments.FindAsync(id);
                
                if (prepayment == null)
                    return ServiceResult.Failure("找不到指定的預收付款項");

                var newUsedAmount = prepayment.UsedAmount + amountToAdd;

                if (newUsedAmount < 0)
                    return ServiceResult.Failure("已用金額不能小於0");

                if (newUsedAmount > prepayment.Amount)
                    return ServiceResult.Failure("已用金額不能大於總金額");

                prepayment.UsedAmount = newUsedAmount;
                prepayment.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateUsedAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateUsedAmountAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    AmountToAdd = amountToAdd
                });
                return ServiceResult.Failure("更新已用金額時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查可用餘額是否足夠
        /// </summary>
        public async Task<bool> HasSufficientBalanceAsync(int id, decimal requiredAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var prepayment = await context.SetoffPrepayments.FindAsync(id);
                
                if (prepayment == null)
                    return false;

                var availableBalance = prepayment.Amount - prepayment.UsedAmount;
                return availableBalance >= requiredAmount;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(HasSufficientBalanceAsync), GetType(), _logger, new
                {
                    Method = nameof(HasSufficientBalanceAsync),
                    ServiceType = GetType().Name,
                    Id = id,
                    RequiredAmount = requiredAmount
                });
                return false;
            }
        }
    }
}
