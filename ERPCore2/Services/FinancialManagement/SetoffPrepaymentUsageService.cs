using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項使用記錄服務實作
    /// </summary>
    public class SetoffPrepaymentUsageService : GenericManagementService<SetoffPrepaymentUsage>, ISetoffPrepaymentUsageService
    {
        private readonly ISetoffPrepaymentService _prepaymentService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SetoffPrepaymentUsageService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffPrepaymentUsage>> logger,
            ISetoffPrepaymentService prepaymentService) : base(contextFactory, logger)
        {
            _prepaymentService = prepaymentService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SetoffPrepaymentUsageService(
            IDbContextFactory<AppDbContext> contextFactory,
            ISetoffPrepaymentService prepaymentService) : base(contextFactory)
        {
            _prepaymentService = prepaymentService;
        }

        /// <summary>
        /// 覆寫取得所有資料，包含相關資料
        /// </summary>
        public override async Task<List<SetoffPrepaymentUsage>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .Include(u => u.SetoffPrepayment)
                    .Include(u => u.SetoffDocument)
                    .OrderByDescending(u => u.UsageDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<SetoffPrepaymentUsage>();
            }
        }

        /// <summary>
        /// 覆寫取得單一資料，包含相關資料
        /// </summary>
        public override async Task<SetoffPrepaymentUsage?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .Include(u => u.SetoffPrepayment)
                    .Include(u => u.SetoffDocument)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                return null;
            }
        }

        /// <summary>
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<SetoffPrepaymentUsage>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .Include(u => u.SetoffPrepayment)
                    .Include(u => u.SetoffDocument)
                    .Where(u => (u.SetoffPrepayment != null && u.SetoffPrepayment.SourceDocumentCode != null && u.SetoffPrepayment.SourceDocumentCode.Contains(searchTerm)) ||
                               (u.SetoffDocument != null && u.SetoffDocument.Code != null && u.SetoffDocument.Code.Contains(searchTerm)))
                    .OrderByDescending(u => u.UsageDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<SetoffPrepaymentUsage>();
            }
        }

        /// <summary>
        /// 根據預收付款項ID取得所有使用記錄
        /// </summary>
        public async Task<List<SetoffPrepaymentUsage>> GetByPrepaymentIdAsync(int prepaymentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .Include(u => u.SetoffDocument)
                    .Where(u => u.SetoffPrepaymentId == prepaymentId)
                    .OrderByDescending(u => u.UsageDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPrepaymentIdAsync), GetType(), _logger, 
                    new { PrepaymentId = prepaymentId });
                return new List<SetoffPrepaymentUsage>();
            }
        }

        /// <summary>
        /// 根據沖款單ID取得所有使用記錄
        /// </summary>
        public async Task<List<SetoffPrepaymentUsage>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .AsNoTracking()
                    .Include(u => u.SetoffPrepayment)
                        .ThenInclude(p => p.PrepaymentType)  // 載入預收付類型，供前端判斷是否為轉沖款
                    .Where(u => u.SetoffDocumentId == setoffDocumentId)
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffDocumentIdAsync), GetType(), _logger,
                    new { SetoffDocumentId = setoffDocumentId });
                return new List<SetoffPrepaymentUsage>();
            }
        }

        /// <summary>
        /// 計算某預收付款項的總使用金額
        /// </summary>
        public async Task<decimal> GetTotalUsedAmountAsync(int prepaymentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffPrepaymentUsages
                    .Where(u => u.SetoffPrepaymentId == prepaymentId)
                    .SumAsync(u => u.UsedAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalUsedAmountAsync), GetType(), _logger,
                    new { PrepaymentId = prepaymentId });
                return 0;
            }
        }

        /// <summary>
        /// 驗證使用金額是否超過可用餘額
        /// </summary>
        public async Task<ServiceResult> ValidateUsageAmountAsync(int prepaymentId, decimal usedAmount, int? excludeUsageId = null)
        {
            try
            {
                // 取得原始預收付款項
                var prepayment = await _prepaymentService.GetByIdAsync(prepaymentId);
                if (prepayment == null)
                    return ServiceResult.Failure("找不到指定的預收付款項");

                // 計算已使用金額（排除當前編輯的記錄）
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffPrepaymentUsages.Where(u => u.SetoffPrepaymentId == prepaymentId);
                
                if (excludeUsageId.HasValue)
                    query = query.Where(u => u.Id != excludeUsageId.Value);

                var totalUsed = await query.SumAsync(u => u.UsedAmount);
                var availableBalance = prepayment.Amount - totalUsed;

                if (usedAmount > availableBalance)
                {
                    return ServiceResult.Failure(
                        $"使用金額 {usedAmount:N2} 超過可用餘額 {availableBalance:N2}。" +
                        $"（總金額：{prepayment.Amount:N2}，已使用：{totalUsed:N2}）");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateUsageAmountAsync), GetType(), _logger,
                    new { PrepaymentId = prepaymentId, UsedAmount = usedAmount, ExcludeUsageId = excludeUsageId });
                return ServiceResult.Failure("驗證使用金額時發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫驗證功能
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffPrepaymentUsage entity)
        {
            try
            {
                var errors = new List<string>();

                // 檢查預收付款項ID
                if (entity.SetoffPrepaymentId <= 0)
                    errors.Add("預收付款項為必填");

                // 檢查沖款單ID
                if (entity.SetoffDocumentId <= 0)
                    errors.Add("沖款單為必填");

                // 檢查使用金額
                if (entity.UsedAmount <= 0)
                    errors.Add("使用金額必須大於0");

                // 驗證使用金額是否超過可用餘額
                if (entity.SetoffPrepaymentId > 0 && entity.UsedAmount > 0)
                {
                    var amountValidation = await ValidateUsageAmountAsync(
                        entity.SetoffPrepaymentId, 
                        entity.UsedAmount, 
                        entity.Id > 0 ? entity.Id : null);
                    
                    if (!amountValidation.IsSuccess)
                        errors.Add(amountValidation.ErrorMessage ?? "使用金額驗證失敗");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id, PrepaymentId = entity.SetoffPrepaymentId, UsedAmount = entity.UsedAmount });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫建立方法，建立後自動更新預收付款項的 UsedAmount
        /// </summary>
        public override async Task<ServiceResult<SetoffPrepaymentUsage>> CreateAsync(SetoffPrepaymentUsage entity)
        {
            try
            {
                // ✅ 自動填入來源預收付款項單號（冗餘欄位，方便查詢）
                if (entity.SetoffPrepaymentId > 0 && string.IsNullOrEmpty(entity.SourcePrepaymentCode))
                {
                    var sourcePrepayment = await _prepaymentService.GetByIdAsync(entity.SetoffPrepaymentId);
                    if (sourcePrepayment != null)
                    {
                        entity.SourcePrepaymentCode = sourcePrepayment.SourceDocumentCode;
                    }
                }
                
                // 先執行基礎建立
                var result = await base.CreateAsync(entity);
                if (!result.IsSuccess)
                    return result;

                // 更新預收付款項的 UsedAmount
                await UpdatePrepaymentUsedAmountAsync(entity.SetoffPrepaymentId);

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger,
                    new { PrepaymentId = entity.SetoffPrepaymentId, UsedAmount = entity.UsedAmount });
                return ServiceResult<SetoffPrepaymentUsage>.Failure("建立使用記錄時發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫刪除方法，刪除後自動更新預收付款項的 UsedAmount
        /// </summary>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                // 先取得使用記錄以便刪除後更新
                var usage = await GetByIdAsync(id);
                if (usage == null)
                    return ServiceResult.Failure("找不到指定的使用記錄");

                var prepaymentId = usage.SetoffPrepaymentId;

                // 執行基礎刪除
                var result = await base.DeleteAsync(id);
                if (!result.IsSuccess)
                    return result;

                // 更新預收付款項的 UsedAmount
                await UpdatePrepaymentUsedAmountAsync(prepaymentId);

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("刪除使用記錄時發生錯誤");
            }
        }

        /// <summary>
        /// 刪除沖款單的所有使用記錄
        /// </summary>
        public async Task<ServiceResult> DeleteBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得所有相關的使用記錄
                var usages = await context.SetoffPrepaymentUsages
                    .Where(u => u.SetoffDocumentId == setoffDocumentId)
                    .ToListAsync();

                if (!usages.Any())
                    return ServiceResult.Success(); // 沒有記錄要刪除

                // 收集所有受影響的預收付款項ID
                var affectedPrepaymentIds = usages.Select(u => u.SetoffPrepaymentId).Distinct().ToList();

                // 刪除所有使用記錄
                context.SetoffPrepaymentUsages.RemoveRange(usages);
                await context.SaveChangesAsync();

                // 更新所有受影響的預收付款項的 UsedAmount
                foreach (var prepaymentId in affectedPrepaymentIds)
                {
                    await UpdatePrepaymentUsedAmountAsync(prepaymentId);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBySetoffDocumentIdAsync), GetType(), _logger,
                    new { SetoffDocumentId = setoffDocumentId });
                return ServiceResult.Failure("刪除沖款單使用記錄時發生錯誤");
            }
        }

        /// <summary>
        /// 更新預收付款項的 UsedAmount（私有方法）
        /// </summary>
        private async Task UpdatePrepaymentUsedAmountAsync(int prepaymentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 計算總使用金額
                var totalUsed = await context.SetoffPrepaymentUsages
                    .Where(u => u.SetoffPrepaymentId == prepaymentId)
                    .SumAsync(u => u.UsedAmount);

                // 更新預收付款項
                var prepayment = await context.SetoffPrepayments.FindAsync(prepaymentId);
                if (prepayment != null)
                {
                    prepayment.UsedAmount = totalUsed;
                    prepayment.UpdatedAt = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePrepaymentUsedAmountAsync), GetType(), _logger,
                    new { PrepaymentId = prepaymentId });
            }
        }
    }
}
