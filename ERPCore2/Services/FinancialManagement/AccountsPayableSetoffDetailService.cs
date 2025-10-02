using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應付帳款沖款明細服務 - 處理供應商的應付帳款沖銷
    /// </summary>
    public class AccountsPayableSetoffDetailService : GenericManagementService<AccountsPayableSetoffDetail>, IAccountsPayableSetoffDetailService
    {
        public AccountsPayableSetoffDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<AccountsPayableSetoffDetail>> logger) : base(contextFactory, logger)
        {
        }

        #region 抽象方法實作

        public override async Task<List<AccountsPayableSetoffDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.AccountsPayableSetoffDetails
                    .Include(d => d.Setoff)
                        .ThenInclude(s => s!.Supplier)
                    .Include(d => d.PurchaseReceivingDetail)
                        .ThenInclude(prd => prd!.PurchaseReceiving)
                    .Include(d => d.PurchaseReceivingDetail)
                        .ThenInclude(prd => prd!.Product)
                    .Include(d => d.PurchaseReturnDetail)
                        .ThenInclude(prd => prd!.PurchaseReturn)
                    .Include(d => d.PurchaseReturnDetail)
                        .ThenInclude(prd => prd!.Product)
                    .Where(d => (
                        d.Setoff.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        d.Setoff.Supplier!.CompanyName.ToLower().Contains(searchTermLower) ||
                        (d.PurchaseReceivingDetail != null && 
                         d.PurchaseReceivingDetail.PurchaseReceiving != null &&
                         d.PurchaseReceivingDetail.PurchaseReceiving.ReceiptNumber.ToLower().Contains(searchTermLower)) ||
                        (d.PurchaseReturnDetail != null && 
                         d.PurchaseReturnDetail.PurchaseReturn != null &&
                         d.PurchaseReturnDetail.PurchaseReturn.PurchaseReturnNumber.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(d.ProductName) && d.ProductName.ToLower().Contains(searchTermLower))
                    ))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<AccountsPayableSetoffDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(AccountsPayableSetoffDetail entity)
        {
            try
            {
                var errors = new List<string>();
                
                // 檢查必填欄位
                if (entity.SetoffId <= 0)
                    errors.Add("必須指定沖款單");
                
                // 檢查必須選擇採購進貨明細或採購退回明細其中之一
                if (!entity.PurchaseReceivingDetailId.HasValue && !entity.PurchaseReturnDetailId.HasValue)
                    errors.Add("必須指定採購進貨明細或採購退回明細其中之一");
                
                // 不能同時指定兩者
                if (entity.PurchaseReceivingDetailId.HasValue && entity.PurchaseReturnDetailId.HasValue)
                    errors.Add("不能同時指定採購進貨明細和採購退回明細");
                
                // 檢查沖款金額
                if (entity.SetoffAmount <= 0)
                    errors.Add("沖款金額必須大於 0");
                
                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id 
                });
                return ServiceResult.Failure($"驗證失敗: {ex.Message}");
            }
        }

        #endregion

        #region 基本查詢方法

        /// <summary>
        /// 依據沖款單ID取得明細列表
        /// </summary>
        public async Task<List<AccountsPayableSetoffDetail>> GetBySetoffIdAsync(int setoffId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountsPayableSetoffDetails
                .Include(d => d.PurchaseReceivingDetail)
                    .ThenInclude(prd => prd!.Product)
                .Include(d => d.PurchaseReturnDetail)
                    .ThenInclude(prd => prd!.Product)
                .Where(d => d.SetoffId == setoffId)
                .ToListAsync();
        }

        /// <summary>
        /// 依據採購進貨明細ID取得沖款明細列表
        /// </summary>
        public async Task<List<AccountsPayableSetoffDetail>> GetByPurchaseReceivingDetailIdAsync(int purchaseReceivingDetailId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountsPayableSetoffDetails
                .Include(d => d.Setoff)
                .Where(d => d.PurchaseReceivingDetailId == purchaseReceivingDetailId)
                .ToListAsync();
        }

        /// <summary>
        /// 依據採購退回明細ID取得沖款明細列表
        /// </summary>
        public async Task<List<AccountsPayableSetoffDetail>> GetByPurchaseReturnDetailIdAsync(int purchaseReturnDetailId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountsPayableSetoffDetails
                .Include(d => d.Setoff)
                .Where(d => d.PurchaseReturnDetailId == purchaseReturnDetailId)
                .ToListAsync();
        }

        #endregion

        #region 供應商未結清明細查詢（新增模式）

        /// <summary>
        /// 取得供應商的未結清明細項目（轉換為統一的 DTO 格式）- 新增模式
        /// </summary>
        public async Task<List<SetoffDetailDto>> GetSupplierPendingDetailsAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new List<SetoffDetailDto>();

            // 1. 取得未結清的採購進貨明細
            var purchaseReceivingDetails = await context.PurchaseReceivingDetails
                .Include(prd => prd.PurchaseReceiving)
                    .ThenInclude(pr => pr.Supplier)
                .Include(prd => prd.Product)
                .Where(prd => prd.PurchaseReceiving.SupplierId == supplierId && !prd.IsSettled)
                .ToListAsync();

            foreach (var detail in purchaseReceivingDetails)
            {
                var totalAmount = detail.SubtotalAmount;
                var settledAmount = detail.TotalPaidAmount;

                // 從 FinancialTransaction 計算已折讓金額
                var discountedAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                // 只要 IsSettled = false 就顯示，不管金額是多少
                // 因為已經在 WHERE 條件中過濾了 !prd.IsSettled，所以這裡直接加入
                // var pendingAmount = totalAmount - settledAmount - discountedAmount;
                result.Add(new SetoffDetailDto
                {
                    Id = result.Count + 1,
                    OriginalEntityId = detail.Id,
                    Mode = SetoffMode.Payable,
                    Type = "PurchaseReceiving",
                    DocumentNumber = detail.PurchaseReceiving.ReceiptNumber,
                    DocumentDate = detail.PurchaseReceiving.ReceiptDate,
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.Name ?? "未知商品",
                    ProductCode = detail.Product?.Code ?? "",
                    Quantity = detail.ReceivedQuantity,
                    UnitPrice = detail.UnitPrice,
                    TotalAmount = totalAmount,
                    SettledAmount = settledAmount,
                    DiscountedAmount = discountedAmount,
                    IsSettled = detail.IsSettled,
                    PartnerId = supplierId,
                    PartnerName = detail.PurchaseReceiving.Supplier?.CompanyName ?? "",
                    Currency = "TWD"
                });
            }

            // 2. 取得未結清的採購退回明細
            var purchaseReturnDetails = await context.PurchaseReturnDetails
                .Include(prd => prd.PurchaseReturn)
                    .ThenInclude(pr => pr.Supplier)
                .Include(prd => prd.Product)
                .Where(prd => prd.PurchaseReturn.SupplierId == supplierId && !prd.IsSettled)
                .ToListAsync();

            foreach (var detail in purchaseReturnDetails)
            {
                var totalAmount = Math.Abs(detail.ReturnSubtotalAmount);
                var settledAmount = detail.TotalReceivedAmount;

                // 從 FinancialTransaction 計算已折讓金額
                var discountedAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                // 只要 IsSettled = false 就顯示，不管金額是多少
                // 因為已經在 WHERE 條件中過濾了 !prd.IsSettled，所以這裡直接加入
                // var pendingAmount = totalAmount - settledAmount - discountedAmount;
                result.Add(new SetoffDetailDto
                {
                    Id = result.Count + 1,
                    OriginalEntityId = detail.Id,
                    Mode = SetoffMode.Payable,
                    Type = "PurchaseReturn",
                    DocumentNumber = detail.PurchaseReturn.PurchaseReturnNumber,
                    DocumentDate = detail.PurchaseReturn.ReturnDate,
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.Name ?? "未知商品",
                    ProductCode = detail.Product?.Code ?? "",
                    Quantity = detail.ReturnQuantity,
                    UnitPrice = detail.ReturnUnitPrice,
                    TotalAmount = totalAmount,
                    SettledAmount = settledAmount,
                    DiscountedAmount = discountedAmount,
                    IsSettled = detail.IsSettled,
                    PartnerId = supplierId,
                    PartnerName = detail.PurchaseReturn.Supplier?.CompanyName ?? "",
                    Currency = "TWD"
                });
            }

            return result.OrderBy(r => r.DocumentDate).ToList();
        }

        #endregion

        #region 供應商所有明細查詢（編輯模式）

        /// <summary>
        /// 取得供應商的所有明細項目（編輯模式用，包含已完成的）
        /// </summary>
        public async Task<List<SetoffDetailDto>> GetSupplierAllDetailsForEditAsync(int supplierId, int setoffId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new List<SetoffDetailDto>();

            // 取得當前沖款單的所有明細
            var existingSetoffDetails = await context.AccountsPayableSetoffDetails
                .Where(d => d.SetoffId == setoffId)
                .ToListAsync();

            // 取得與當前沖款單相關的明細 ID
            var currentPurchaseReceivingDetailIds = existingSetoffDetails
                .Where(d => d.PurchaseReceivingDetailId.HasValue)
                .Select(d => d.PurchaseReceivingDetailId!.Value)
                .ToList();

            var currentPurchaseReturnDetailIds = existingSetoffDetails
                .Where(d => d.PurchaseReturnDetailId.HasValue)
                .Select(d => d.PurchaseReturnDetailId!.Value)
                .ToList();

            // 1. 編輯模式：只取得與當前沖款單關聯的採購進貨明細
            var purchaseReceivingDetails = await context.PurchaseReceivingDetails
                .Include(prd => prd.PurchaseReceiving)
                    .ThenInclude(pr => pr.Supplier)
                .Include(prd => prd.Product)
                .Where(prd => currentPurchaseReceivingDetailIds.Contains(prd.Id))
                .ToListAsync();

            foreach (var detail in purchaseReceivingDetails)
            {
                var totalAmount = detail.SubtotalAmount;

                // 檢查是否在當前沖款單中有記錄
                var currentSetoffDetail = existingSetoffDetails
                    .FirstOrDefault(esd => esd.PurchaseReceivingDetailId == detail.Id);

                var thisTimeAmount = currentSetoffDetail?.SetoffAmount ?? 0;

                // 顯示真實的累計付款金額
                var settledAmount = detail.TotalPaidAmount;

                // 計算已折讓金額（排除當前沖款單）
                var discountedAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed
                                && ft.SourceDocumentId != setoffId)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                // 取得當前沖款單的折讓金額
                var currentDiscountAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed
                                && ft.SourceDocumentId == setoffId)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                result.Add(new SetoffDetailDto
                {
                    Id = result.Count + 1,
                    OriginalEntityId = detail.Id,
                    Mode = SetoffMode.Payable,
                    Type = "PurchaseReceiving",
                    DocumentNumber = detail.PurchaseReceiving.ReceiptNumber,
                    DocumentDate = detail.PurchaseReceiving.ReceiptDate,
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.Name ?? "未知商品",
                    ProductCode = detail.Product?.Code ?? "",
                    Quantity = detail.ReceivedQuantity,
                    UnitPrice = detail.UnitPrice,
                    TotalAmount = totalAmount,
                    SettledAmount = settledAmount,
                    DiscountedAmount = discountedAmount,
                    ThisTimeAmount = thisTimeAmount,
                    ThisTimeDiscountAmount = currentDiscountAmount,
                    OriginalThisTimeAmount = thisTimeAmount,
                    OriginalThisTimeDiscountAmount = currentDiscountAmount,
                    IsEditMode = true,
                    IsSettled = detail.IsSettled,
                    PartnerId = supplierId,
                    PartnerName = detail.PurchaseReceiving.Supplier?.CompanyName ?? "",
                    Currency = "TWD"
                });
            }

            // 2. 編輯模式：只取得與當前沖款單關聯的採購退回明細
            var purchaseReturnDetails = await context.PurchaseReturnDetails
                .Include(prd => prd.PurchaseReturn)
                    .ThenInclude(pr => pr.Supplier)
                .Include(prd => prd.Product)
                .Where(prd => currentPurchaseReturnDetailIds.Contains(prd.Id))
                .ToListAsync();

            foreach (var detail in purchaseReturnDetails)
            {
                var totalAmount = Math.Abs(detail.ReturnSubtotalAmount);

                // 檢查是否在當前沖款單中有記錄
                var currentSetoffDetail = existingSetoffDetails
                    .FirstOrDefault(esd => esd.PurchaseReturnDetailId == detail.Id);

                var thisTimeAmount = currentSetoffDetail?.SetoffAmount ?? 0;

                // 顯示真實的累計收款金額
                var settledAmount = detail.TotalReceivedAmount;

                // 計算已折讓金額（排除當前沖款單）
                var discountedAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed
                                && ft.SourceDocumentId != setoffId)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                // 取得當前沖款單的折讓金額
                var currentDiscountAmount = await context.FinancialTransactions
                    .Where(ft => ft.SourceDetailId == detail.Id 
                                && ft.TransactionType == FinancialTransactionTypeEnum.AccountsPayableSetoff
                                && !ft.IsReversed
                                && ft.SourceDocumentId == setoffId)
                    .SumAsync(ft => ft.CurrentDiscountAmount);

                result.Add(new SetoffDetailDto
                {
                    Id = result.Count + 1,
                    OriginalEntityId = detail.Id,
                    Mode = SetoffMode.Payable,
                    Type = "PurchaseReturn",
                    DocumentNumber = detail.PurchaseReturn.PurchaseReturnNumber,
                    DocumentDate = detail.PurchaseReturn.ReturnDate,
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.Name ?? "未知商品",
                    ProductCode = detail.Product?.Code ?? "",
                    Quantity = detail.ReturnQuantity,
                    UnitPrice = detail.ReturnUnitPrice,
                    TotalAmount = totalAmount,
                    SettledAmount = settledAmount,
                    DiscountedAmount = discountedAmount,
                    ThisTimeAmount = thisTimeAmount,
                    ThisTimeDiscountAmount = currentDiscountAmount,
                    OriginalThisTimeAmount = thisTimeAmount,
                    OriginalThisTimeDiscountAmount = currentDiscountAmount,
                    IsEditMode = true,
                    IsSettled = detail.IsSettled,
                    PartnerId = supplierId,
                    PartnerName = detail.PurchaseReturn.Supplier?.CompanyName ?? "",
                    Currency = "TWD"
                });
            }

            return result.OrderBy(r => r.DocumentDate).ToList();
        }

        #endregion

        #region 金額計算方法

        /// <summary>
        /// 計算指定採購進貨明細的累計付款金額
        /// </summary>
        public async Task<decimal> CalculateTotalPaidAmountByPurchaseReceivingDetailAsync(int purchaseReceivingDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffDetails
                    .Where(d => d.PurchaseReceivingDetailId == purchaseReceivingDetailId)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalPaidAmountByPurchaseReceivingDetailAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalPaidAmountByPurchaseReceivingDetailAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingDetailId = purchaseReceivingDetailId 
                });
                return 0;
            }
        }

        /// <summary>
        /// 計算指定採購退回明細的累計收款金額
        /// </summary>
        public async Task<decimal> CalculateTotalReceivedAmountByPurchaseReturnDetailAsync(int purchaseReturnDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.AccountsPayableSetoffDetails
                    .Where(d => d.PurchaseReturnDetailId == purchaseReturnDetailId)
                    .SumAsync(d => d.SetoffAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTotalReceivedAmountByPurchaseReturnDetailAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTotalReceivedAmountByPurchaseReturnDetailAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnDetailId = purchaseReturnDetailId 
                });
                return 0;
            }
        }

        /// <summary>
        /// 檢查指定的採購進貨明細是否已完全付款
        /// </summary>
        public async Task<bool> IsPurchaseReceivingDetailFullyPaidAsync(int purchaseReceivingDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReceivingDetails
                    .FirstOrDefaultAsync(prd => prd.Id == purchaseReceivingDetailId);
                
                if (detail == null)
                    return false;
                
                var totalPaid = await CalculateTotalPaidAmountByPurchaseReceivingDetailAsync(purchaseReceivingDetailId);
                return totalPaid >= detail.SubtotalAmount;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReceivingDetailFullyPaidAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseReceivingDetailFullyPaidAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingDetailId = purchaseReceivingDetailId 
                });
                return false;
            }
        }

        /// <summary>
        /// 檢查指定的採購退回明細是否已完全收款
        /// </summary>
        public async Task<bool> IsPurchaseReturnDetailFullyReceivedAsync(int purchaseReturnDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.PurchaseReturnDetails
                    .FirstOrDefaultAsync(prd => prd.Id == purchaseReturnDetailId);
                
                if (detail == null)
                    return false;
                
                var totalReceived = await CalculateTotalReceivedAmountByPurchaseReturnDetailAsync(purchaseReturnDetailId);
                return totalReceived >= Math.Abs(detail.ReturnSubtotalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReturnDetailFullyReceivedAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseReturnDetailFullyReceivedAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnDetailId = purchaseReturnDetailId 
                });
                return false;
            }
        }

        #endregion

        #region 批次操作方法

        /// <summary>
        /// 批次新增沖款明細
        /// </summary>
        public async Task<ServiceResult> CreateBatchForSetoffAsync(int setoffId, List<AccountsPayableSetoffDetail> details)
        {
            try
            {
                foreach (var detail in details)
                {
                    detail.SetoffId = setoffId;
                    await CreateAsync(detail);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateBatchForSetoffAsync), GetType(), _logger, new { 
                    Method = nameof(CreateBatchForSetoffAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId,
                    DetailCount = details.Count 
                });
                return ServiceResult.Failure($"批次新增沖款明細失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 依據沖款單ID刪除所有明細
        /// </summary>
        public async Task<ServiceResult> DeleteBySetoffIdAsync(int setoffId)
        {
            try
            {
                var details = await GetBySetoffIdAsync(setoffId);
                foreach (var detail in details)
                {
                    await DeleteAsync(detail.Id);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBySetoffIdAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteBySetoffIdAsync),
                    ServiceType = GetType().Name,
                    SetoffId = setoffId 
                });
                return ServiceResult.Failure($"刪除沖款明細失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證沖款明細的沖款金額是否有效
        /// </summary>
        public async Task<ServiceResult> ValidateSetoffAmountAsync(AccountsPayableSetoffDetail detail)
        {
            try
            {
                if (detail.SetoffAmount <= 0)
                {
                    return ServiceResult.Failure("沖款金額必須大於 0");
                }

                // 驗證採購進貨明細
                if (detail.PurchaseReceivingDetailId.HasValue)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var receivingDetail = await context.PurchaseReceivingDetails
                        .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReceivingDetailId.Value);

                    if (receivingDetail == null)
                    {
                        return ServiceResult.Failure("找不到對應的採購進貨明細");
                    }

                    var totalPaid = await CalculateTotalPaidAmountByPurchaseReceivingDetailAsync(detail.PurchaseReceivingDetailId.Value);
                    var remaining = receivingDetail.SubtotalAmount - totalPaid;

                    if (detail.SetoffAmount > remaining)
                    {
                        return ServiceResult.Failure($"沖款金額不能超過剩餘應付金額 {remaining:N2}");
                    }
                }

                // 驗證採購退回明細
                if (detail.PurchaseReturnDetailId.HasValue)
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var returnDetail = await context.PurchaseReturnDetails
                        .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseReturnDetailId.Value);

                    if (returnDetail == null)
                    {
                        return ServiceResult.Failure("找不到對應的採購退回明細");
                    }

                    var totalReceived = await CalculateTotalReceivedAmountByPurchaseReturnDetailAsync(detail.PurchaseReturnDetailId.Value);
                    var remaining = Math.Abs(returnDetail.ReturnSubtotalAmount) - totalReceived;

                    if (detail.SetoffAmount > remaining)
                    {
                        return ServiceResult.Failure($"沖款金額不能超過剩餘應收金額 {remaining:N2}");
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateSetoffAmountAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateSetoffAmountAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id 
                });
                return ServiceResult.Failure($"驗證失敗: {ex.Message}");
            }
        }

        #endregion

        #region 未實作方法（保留介面完整性）

        public Task<List<dynamic>> GetAvailableItemsForSetoffAsync(int supplierId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> UpdatePayableAmountsAsync(int detailId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
