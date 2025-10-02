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
    /// 應付帳款沖款單服務實作
    /// </summary>
    public class AccountsPayableSetoffService : GenericManagementService<AccountsPayableSetoff>, IAccountsPayableSetoffService
    {
        public AccountsPayableSetoffService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<AccountsPayableSetoff>> logger) : base(contextFactory, logger)
        {
        }

        public AccountsPayableSetoffService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<AccountsPayableSetoff>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
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
                return new List<AccountsPayableSetoff>();
            }
        }

        public override async Task<AccountsPayableSetoff?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
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

        public override async Task<List<AccountsPayableSetoff>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
                    .Where(s => (
                        s.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        s.Supplier.CompanyName.ToLower().Contains(searchTermLower) ||
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
                return new List<AccountsPayableSetoff>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsPayableSetoff entity)
        {
            try
            {
                var errors = new List<string>();
                
                // 檢查必填欄位
                if (string.IsNullOrWhiteSpace(entity.SetoffNumber))
                    errors.Add("沖款單號不能為空");
                
                if (entity.SupplierId <= 0)
                    errors.Add("必須選擇供應商");
                
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
                
                // 檢查供應商是否存在
                using var context = await _contextFactory.CreateDbContextAsync();
                var supplierExists = await context.Suppliers
                    .AnyAsync(c => c.Id == entity.SupplierId);
                if (!supplierExists)
                    errors.Add("選擇的供應商不存在或已被刪除");
                
                // 檢查付款方式是否存在（如果有選擇的話）
                if (entity.PaymentMethodId.HasValue)
                {
                    var paymentMethodExists = await context.PaymentMethods
                        .AnyAsync(pm => pm.Id == entity.PaymentMethodId.Value && pm.Status == EntityStatus.Active);
                    if (!paymentMethodExists)
                        errors.Add("選擇的付款方式不存在或已停用");
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
                var query = context.AccountsPayableSetoffs
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

        public async Task<List<AccountsPayableSetoff>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
                    .Where(s => s.SupplierId == supplierId)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<AccountsPayableSetoff>();
            }
        }

        public async Task<List<AccountsPayableSetoff>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
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
                return new List<AccountsPayableSetoff>();
            }
        }

        public async Task<AccountsPayableSetoff?> GetWithDetailsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.PurchaseReceivingDetail)
                            .ThenInclude(prd => prd!.Product)
                    .Include(s => s.SetoffDetails)
                        .ThenInclude(d => d.PurchaseReturnDetail)
                            .ThenInclude(prd => prd!.Product)
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

        public async Task<ServiceResult> CompleteSetoffAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var setoff = await context.AccountsPayableSetoffs
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (setoff == null)
                    return ServiceResult.Failure("沖款單不存在");
                
                if (setoff.IsCompleted)
                    return ServiceResult.Failure("沖款單已經完成");
                
                // 檢查是否有明細
                var hasDetails = await context.AccountsPayableSetoffDetails
                    .AnyAsync(d => d.SetoffId == id);
                if (!hasDetails)
                    return ServiceResult.Failure("沖款單必須有明細才能完成");
                
                setoff.IsCompleted = true;
                setoff.CompletedDate = DateTime.Now;
                setoff.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CompleteSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(CompleteSetoffAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("完成沖款單時發生錯誤");
            }
        }

        public async Task<ServiceResult> UncompleteSetoffAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var setoff = await context.AccountsPayableSetoffs
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (setoff == null)
                    return ServiceResult.Failure("沖款單不存在");
                
                if (!setoff.IsCompleted)
                    return ServiceResult.Failure("沖款單尚未完成");
                
                setoff.IsCompleted = false;
                setoff.CompletedDate = null;
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
                return await context.AccountsPayableSetoffDetails
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

        public async Task<List<AccountsPayableSetoff>> GetIncompleteSetoffsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffs
                    .Include(s => s.Supplier)
                    .Include(s => s.Company)
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
                return new List<AccountsPayableSetoff>();
            }
        }

        #endregion

        #region 覆寫刪除方法 - 含回滾機制

        /// <summary>
        /// 覆寫刪除方法 - 刪除主檔時同步回滾明細的影響
        /// 功能：刪除應付帳款沖款單時，自動回退已沖銷的金額
        /// 處理流程：
        /// 1. 驗證沖款單存在性
        /// 2. 載入相關的沖款明細記錄
        /// 3. 回滾 PurchaseReceivingDetail 和 PurchaseReturnDetail 的付款/收款金額
        /// 4. 執行原本的軟刪除（主檔）
        /// 5. 使用資料庫交易確保資料一致性
        /// 6. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">要刪除的應付帳款沖款單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            Console.WriteLine($"=== AccountsPayableSetoffService.DeleteAsync 開始執行 ===");
            Console.WriteLine($"要刪除的 AccountsPayableSetoff ID: {id}");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 查找主檔及其明細
                    var setoff = await context.AccountsPayableSetoffs
                        .Include(s => s.SetoffDetails)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (setoff == null)
                    {
                        Console.WriteLine($"沖款單不存在: ID={id}");
                        return ServiceResult.Failure("沖款單不存在");
                    }

                    Console.WriteLine($"找到沖款單: {setoff.SetoffNumber}, 明細數量: {setoff.SetoffDetails?.Count ?? 0}");

                    // 2. 回滾明細的影響
                    if (setoff.SetoffDetails != null && setoff.SetoffDetails.Any())
                    {
                        Console.WriteLine("開始回滾明細的影響...");
                        await RevertSetoffDetailsAsync(context, setoff.SetoffDetails.ToList());
                        Console.WriteLine("明細回滾完成");
                    }

                    // 3. 執行軟刪除主檔
                    setoff.Status = EntityStatus.Deleted;
                    setoff.UpdatedAt = DateTime.UtcNow;

                    Console.WriteLine("標記主檔為已刪除");
                    await context.SaveChangesAsync();

                    // 4. 提交交易
                    await transaction.CommitAsync();
                    Console.WriteLine("交易已提交，刪除成功");

                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"交易回滾: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure($"刪除沖款單時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 永久刪除應付帳款沖款單（含回滾）
        /// 這是UI實際調用的刪除方法
        /// 處理流程：
        /// 1. 驗證沖款單存在性和刪除權限
        /// 2. 回滾所有明細的影響
        /// 3. 沖銷相關的財務交易記錄
        /// 4. 永久刪除明細和主檔
        /// 5. 使用資料庫交易確保資料一致性
        /// </summary>
        /// <param name="id">要刪除的應付帳款沖款單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            Console.WriteLine($"=== AccountsPayableSetoffService.PermanentDeleteAsync 開始執行 ===");
            Console.WriteLine($"要永久刪除的 AccountsPayableSetoff ID: {id}");

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 查找主檔及其明細
                    var setoff = await context.AccountsPayableSetoffs
                        .Include(s => s.SetoffDetails)
                        .Include(s => s.Supplier)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (setoff == null)
                    {
                        Console.WriteLine($"沖款單不存在: ID={id}");
                        return ServiceResult.Failure("沖款單不存在");
                    }

                    Console.WriteLine($"找到沖款單: {setoff.SetoffNumber}, 明細數量: {setoff.SetoffDetails?.Count ?? 0}");

                    // 2. 回滾明細的影響
                    if (setoff.SetoffDetails != null && setoff.SetoffDetails.Any())
                    {
                        Console.WriteLine("開始回滾明細的影響...");
                        await RevertSetoffDetailsAsync(context, setoff.SetoffDetails.ToList());
                        Console.WriteLine("明細回滾完成");
                    }

                    // 3. 處理財務交易記錄（沖銷）
                    Console.WriteLine("開始處理財務交易記錄...");
                    await ReverseFinancialTransactionsAsync(context, setoff);
                    Console.WriteLine("財務交易記錄處理完成");

                    // 4. 永久刪除明細
                    if (setoff.SetoffDetails != null && setoff.SetoffDetails.Any())
                    {
                        Console.WriteLine($"永久刪除 {setoff.SetoffDetails.Count} 筆明細");
                        context.AccountsPayableSetoffDetails.RemoveRange(setoff.SetoffDetails);
                        await context.SaveChangesAsync();
                    }

                    // 5. 永久刪除主檔
                    Console.WriteLine("永久刪除主檔");
                    context.AccountsPayableSetoffs.Remove(setoff);
                    await context.SaveChangesAsync();

                    // 6. 提交交易
                    await transaction.CommitAsync();
                    Console.WriteLine("交易已提交，永久刪除成功");

                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"交易回滾: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure($"永久刪除沖款單時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 回滾沖款明細對原始單據的影響
        /// 包括：PurchaseReceivingDetail 的付款金額、PurchaseReturnDetail 的收款金額
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="setoffDetails">要回滾的沖款明細列表</param>
        private async Task RevertSetoffDetailsAsync(AppDbContext context, List<AccountsPayableSetoffDetail> setoffDetails)
        {
            // 處理採購進貨明細的回滾
            var purchaseReceivingDetailIds = setoffDetails
                .Where(d => d.PurchaseReceivingDetailId.HasValue)
                .Select(d => d.PurchaseReceivingDetailId!.Value)
                .Distinct()
                .ToList();

            if (purchaseReceivingDetailIds.Any())
            {
                Console.WriteLine($"回滾 {purchaseReceivingDetailIds.Count} 筆採購進貨明細");
                var purchaseReceivingDetails = await context.PurchaseReceivingDetails
                    .Where(prd => purchaseReceivingDetailIds.Contains(prd.Id))
                    .ToListAsync();

                foreach (var purchaseReceivingDetail in purchaseReceivingDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.PurchaseReceivingDetailId == purchaseReceivingDetail.Id);
                    
                    Console.WriteLine($"  - PurchaseReceivingDetail ID={purchaseReceivingDetail.Id}: 回滾金額 {setoffDetail.SetoffAmount}");
                    
                    // 回滾累計付款金額：減去之前的沖款金額
                    purchaseReceivingDetail.TotalPaidAmount = Math.Max(0, purchaseReceivingDetail.TotalPaidAmount - setoffDetail.SetoffAmount);
                    purchaseReceivingDetail.PaidAmount = 0; // 清除本次付款記錄
                    
                    // 重新檢查結清狀態
                    purchaseReceivingDetail.IsSettled = purchaseReceivingDetail.TotalPaidAmount >= purchaseReceivingDetail.SubtotalAmount;
                    
                    purchaseReceivingDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.PurchaseReceivingDetails.UpdateRange(purchaseReceivingDetails);
                await context.SaveChangesAsync();
            }

            // 處理採購退回明細的回滾
            var purchaseReturnDetailIds = setoffDetails
                .Where(d => d.PurchaseReturnDetailId.HasValue)
                .Select(d => d.PurchaseReturnDetailId!.Value)
                .Distinct()
                .ToList();

            if (purchaseReturnDetailIds.Any())
            {
                Console.WriteLine($"回滾 {purchaseReturnDetailIds.Count} 筆採購退回明細");
                var purchaseReturnDetails = await context.PurchaseReturnDetails
                    .Where(prd => purchaseReturnDetailIds.Contains(prd.Id))
                    .ToListAsync();

                foreach (var purchaseReturnDetail in purchaseReturnDetails)
                {
                    var setoffDetail = setoffDetails.First(d => d.PurchaseReturnDetailId == purchaseReturnDetail.Id);
                    
                    Console.WriteLine($"  - PurchaseReturnDetail ID={purchaseReturnDetail.Id}: 回滾金額 {setoffDetail.SetoffAmount}");
                    
                    // 回滾累計收款金額：減去之前的沖款金額
                    purchaseReturnDetail.TotalReceivedAmount = Math.Max(0, purchaseReturnDetail.TotalReceivedAmount - setoffDetail.SetoffAmount);
                    purchaseReturnDetail.ReceivedAmount = 0; // 清除本次收款記錄
                    
                    // 重新檢查結清狀態
                    purchaseReturnDetail.IsSettled = purchaseReturnDetail.TotalReceivedAmount >= Math.Abs(purchaseReturnDetail.ReturnSubtotalAmount);
                    
                    purchaseReturnDetail.UpdatedAt = DateTime.UtcNow;
                }

                context.PurchaseReturnDetails.UpdateRange(purchaseReturnDetails);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 沖銷相關的財務交易記錄
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="setoff">要處理的沖款單</param>
        private async Task ReverseFinancialTransactionsAsync(AppDbContext context, AccountsPayableSetoff setoff)
        {
            // 查找相關的財務交易記錄
            var transactions = await context.FinancialTransactions
                .Where(ft => ft.SourceDocumentType == "AccountsPayableSetoff" &&
                            ft.SourceDocumentId == setoff.Id &&
                            !ft.IsReversed)
                .ToListAsync();

            Console.WriteLine($"找到 {transactions.Count} 筆財務交易記錄需要沖銷");

            foreach (var transaction in transactions)
            {
                Console.WriteLine($"  - 沖銷財務交易: {transaction.TransactionNumber}");
                
                // 標記為已沖銷
                transaction.IsReversed = true;
                transaction.ReversedDate = DateTime.UtcNow;
                transaction.Remarks = $"沖銷原因：刪除沖款單 {setoff.SetoffNumber}";
                transaction.UpdatedAt = DateTime.UtcNow;
            }

            if (transactions.Any())
            {
                context.FinancialTransactions.UpdateRange(transactions);
                await context.SaveChangesAsync();
            }
        }

        #endregion
    }
}
