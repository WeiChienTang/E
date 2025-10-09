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
    /// 注意：此驗證僅針對「預收/預付」主記錄，不包含「轉沖款」
    /// 轉沖款使用 SetoffPrepaymentUsage 表，由 SetoffPrepaymentUsageService 負責驗證
    /// </summary>
    public override async Task<ServiceResult> ValidateAsync(SetoffPrepayment entity)
    {
        try
        {
            // ===== DEBUG：輸出驗證資訊 =====
            Console.WriteLine("========================================");
            Console.WriteLine($"[SetoffPrepaymentService.ValidateAsync] 開始驗證");
            Console.WriteLine($"  Entity.Id: {entity.Id}");
            Console.WriteLine($"  Entity.SourceDocumentCode: {entity.SourceDocumentCode}");
            Console.WriteLine($"  Entity.Amount: {entity.Amount}");
            Console.WriteLine($"  Entity.UsedAmount: {entity.UsedAmount}");
            Console.WriteLine($"  Entity.CustomerId: {entity.CustomerId}");
            Console.WriteLine($"  Entity.SupplierId: {entity.SupplierId}");
            Console.WriteLine($"  Entity.SetoffDocumentId: {entity.SetoffDocumentId}");
            Console.WriteLine("========================================");
            
            var errors = new List<string>();

            // 檢查來源單號
            if (string.IsNullOrWhiteSpace(entity.SourceDocumentCode))
                errors.Add("來源單號為必填");

            // 檢查金額必須大於0（預收/預付主記錄必須有金額）
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

            // 檢查來源單號是否重複（預收/預付主記錄的來源單號必須唯一）
            // 注意：編輯模式下，如果 Id > 0，則完全跳過唯一性檢查（因為是更新現有記錄）
            // 只有新增模式（Id = 0）才需要檢查來源單號是否重複
            Console.WriteLine($"[DEBUG] 準備檢查來源單號唯一性...");
            Console.WriteLine($"  SourceDocumentCode IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(entity.SourceDocumentCode)}");
            Console.WriteLine($"  entity.Id == 0: {entity.Id == 0}");
            
            if (!string.IsNullOrWhiteSpace(entity.SourceDocumentCode) && entity.Id == 0)
            {
                Console.WriteLine($"[DEBUG] 執行唯一性檢查（新增模式）");
                // 只有新增時才檢查唯一性
                var exists = await IsSourceDocumentCodeExistsAsync(entity.SourceDocumentCode, null);
                Console.WriteLine($"[DEBUG] IsSourceDocumentCodeExistsAsync 結果: {exists}");
                
                if (exists)
                {
                    errors.Add("來源單號已存在");
                    Console.WriteLine($"[DEBUG] ❌ 來源單號重複！");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ✅ 來源單號唯一");
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] ⏭️ 跳過唯一性檢查（編輯模式或來源單號為空）");
            }

            Console.WriteLine($"[DEBUG] 驗證結果 - 錯誤數量: {errors.Count}");
            if (errors.Any())
            {
                Console.WriteLine($"[DEBUG] 錯誤內容: {string.Join("; ", errors)}");
            }
            Console.WriteLine("========================================\n");

            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] ValidateAsync 發生例外: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            
            await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
            {
                Method = nameof(ValidateAsync),
                ServiceType = GetType().Name,
                EntityId = entity.Id,
                SourceDocumentCode = entity.SourceDocumentCode
            });
            return ServiceResult.Failure("驗證過程發生錯誤");
        }
    }        /// <summary>
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
                    .AsNoTracking()  // 不追蹤實體，避免後續操作時的追蹤衝突
                    .Include(sp => sp.PrepaymentType)  // 載入預收付類型，供前端判斷
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
                Console.WriteLine($"[IsSourceDocumentCodeExistsAsync] 開始檢查");
                Console.WriteLine($"  SourceDocumentCode: {sourceDocumentCode}");
                Console.WriteLine($"  ExcludeId: {excludeId}");
                
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffPrepayments.Where(sp => sp.SourceDocumentCode == sourceDocumentCode);
                
                var allMatches = await query.ToListAsync();
                Console.WriteLine($"  找到 {allMatches.Count} 筆符合的記錄:");
                foreach (var match in allMatches)
                {
                    Console.WriteLine($"    - Id={match.Id}, SourceCode={match.SourceDocumentCode}, Amount={match.Amount}");
                }
                
                if (excludeId.HasValue)
                {
                    query = query.Where(sp => sp.Id != excludeId.Value);
                    Console.WriteLine($"  排除 Id={excludeId.Value} 後...");
                }

                var exists = await query.AnyAsync();
                Console.WriteLine($"  最終結果: {(exists ? "存在重複" : "不存在重複")}");
                
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsSourceDocumentCodeExistsAsync 發生例外: {ex.Message}");
                
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
