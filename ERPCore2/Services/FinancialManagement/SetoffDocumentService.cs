using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款單服務實作
    /// </summary>
    public class SetoffDocumentService : GenericManagementService<SetoffDocument>, ISetoffDocumentService
    {
        public SetoffDocumentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffDocument>> logger) : base(contextFactory, logger)
        {
        }

        public SetoffDocumentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫 GetAllAsync 以包含關聯資料
        /// </summary>
        public override async Task<List<SetoffDocument>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffDocuments = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                // 載入關聯方名稱
                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                return setoffDocuments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 覆寫 GetByIdAsync 以包含關聯資料
        /// </summary>
        public override async Task<SetoffDocument?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffDocument = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Include(s => s.FinancialTransactions)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (setoffDocument != null)
                {
                    await LoadRelatedPartyNamesAsync(context, new List<SetoffDocument> { setoffDocument });
                }

                return setoffDocument;
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
        public override async Task<List<SetoffDocument>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                var setoffDocuments = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s =>
                        s.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        s.Company.CompanyName.ToLower().Contains(searchTermLower))
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                // 載入關聯方名稱
                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                // 在記憶體中進一步篩選關聯方名稱
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    setoffDocuments = setoffDocuments
                        .Where(s => s.RelatedPartyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return setoffDocuments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 實作驗證功能
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffDocument entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.SetoffNumber))
                    errors.Add("沖款單號不能為空");

                if (entity.SetoffDate == default)
                    errors.Add("沖款日期不能為空");

                if (entity.RelatedPartyId <= 0)
                    errors.Add("關聯方為必填");

                if (string.IsNullOrWhiteSpace(entity.RelatedPartyType))
                    errors.Add("關聯方類型不能為空");

                if (entity.CompanyId <= 0)
                    errors.Add("公司為必填");

                if (!string.IsNullOrWhiteSpace(entity.SetoffNumber) &&
                    await IsSetoffNumberExistsAsync(entity.SetoffNumber, entity.Id == 0 ? null : entity.Id))
                    errors.Add("沖款單號已存在");

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
                    SetoffNumber = entity.SetoffNumber
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 檢查沖款單號是否已存在
        /// </summary>
        public async Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffDocuments.Where(s => s.SetoffNumber == setoffNumber);
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSetoffNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsSetoffNumberExistsAsync),
                    ServiceType = GetType().Name,
                    SetoffNumber = setoffNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        /// <summary>
        /// 根據沖款類型取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetBySetoffTypeAsync(SetoffType setoffType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffDocuments = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.SetoffType == setoffType)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                return setoffDocuments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySetoffTypeAsync),
                    ServiceType = GetType().Name,
                    SetoffType = setoffType
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據關聯方ID取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByRelatedPartyIdAsync(int relatedPartyId, SetoffType? setoffType = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.RelatedPartyId == relatedPartyId);

                if (setoffType.HasValue)
                    query = query.Where(s => s.SetoffType == setoffType.Value);

                var setoffDocuments = await query
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                return setoffDocuments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByRelatedPartyIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByRelatedPartyIdAsync),
                    ServiceType = GetType().Name,
                    RelatedPartyId = relatedPartyId,
                    SetoffType = setoffType
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據公司ID取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffDocuments = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.CompanyId == companyId)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                return setoffDocuments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCompanyIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByCompanyIdAsync),
                    ServiceType = GetType().Name,
                    CompanyId = companyId
                });
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 根據日期區間取得沖款單列表
        /// </summary>
        public async Task<List<SetoffDocument>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var setoffDocuments = await context.SetoffDocuments
                    .Include(s => s.Company)
                    .Where(s => s.SetoffDate >= startDate && s.SetoffDate <= endDate)
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.SetoffNumber)
                    .ToListAsync();

                await LoadRelatedPartyNamesAsync(context, setoffDocuments);

                return setoffDocuments;
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
                return new List<SetoffDocument>();
            }
        }

        /// <summary>
        /// 載入沖款單的關聯方名稱
        /// </summary>
        private async Task LoadRelatedPartyNamesAsync(AppDbContext context, List<SetoffDocument> setoffDocuments)
        {
            if (setoffDocuments == null || !setoffDocuments.Any())
                return;

            // 分組取得客戶和供應商的 ID
            var customerIds = setoffDocuments
                .Where(s => s.RelatedPartyType == "Customer")
                .Select(s => s.RelatedPartyId)
                .Distinct()
                .ToList();

            var supplierIds = setoffDocuments
                .Where(s => s.RelatedPartyType == "Supplier")
                .Select(s => s.RelatedPartyId)
                .Distinct()
                .ToList();

            // 批次載入客戶和供應商資料
            var customers = customerIds.Any()
                ? await context.Customers
                    .Where(c => customerIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, c => c.CompanyName ?? "")
                : new Dictionary<int, string>();

            var suppliers = supplierIds.Any()
                ? await context.Suppliers
                    .Where(s => supplierIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.CompanyName ?? "")
                : new Dictionary<int, string>();

            // 填充 RelatedPartyName
            foreach (var setoffDoc in setoffDocuments)
            {
                if (setoffDoc.RelatedPartyType == "Customer" && customers.TryGetValue(setoffDoc.RelatedPartyId, out var customerName))
                {
                    setoffDoc.RelatedPartyName = customerName;
                }
                else if (setoffDoc.RelatedPartyType == "Supplier" && suppliers.TryGetValue(setoffDoc.RelatedPartyId, out var supplierName))
                {
                    setoffDoc.RelatedPartyName = supplierName;
                }
            }
        }

        /// <summary>
        /// 覆寫刪除方法 - 在刪除沖款單前先回朔來源明細的累計金額
        /// </summary>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            // 直接調用 PermanentDeleteAsync，保持與基礎類別的一致性
            return await PermanentDeleteAsync(id);
        }

        /// <summary>
        /// 覆寫永久刪除方法 - 在刪除沖款單前先回朔來源明細的累計金額
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 📦 載入完整資料（含所有關聯明細）
                var document = await context.SetoffDocuments
                    .Include(d => d.SetoffProductDetails)
                    .Include(d => d.SetoffPayments)
                    .Include(d => d.Prepayments)
                    .Include(d => d.FinancialTransactions)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Failure("找不到要刪除的沖款單");
                }

                // 檢查是否可以刪除
                var canDeleteResult = await CanDeleteAsync(document);
                if (!canDeleteResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return canDeleteResult;
                }

                // 🔄 【關鍵步驟】先回朔所有來源 Detail 的累計金額
                _logger?.LogInformation("開始回朔沖款單 {SetoffNumber} 的來源明細累計金額", document.SetoffNumber);
                
                foreach (var detail in document.SetoffProductDetails)
                {
                    await RollbackSourceDetailAmountAsync(context, detail);
                }

                _logger?.LogInformation("已完成 {Count} 筆來源明細的金額回朔", document.SetoffProductDetails.Count);

                // 🗑️ 刪除沖款單（級聯刪除所有關聯明細）
                context.SetoffDocuments.Remove(document);

                // 💾 儲存變更
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger?.LogInformation("成功刪除沖款單 {SetoffNumber} (Id={Id})", document.SetoffNumber, id);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return ServiceResult.Failure($"刪除沖款單時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 回朔來源明細的累計金額（排除當前要刪除的明細）
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="detailToDelete">要刪除的沖款明細</param>
        private async Task RollbackSourceDetailAmountAsync(AppDbContext context, SetoffProductDetail detailToDelete)
        {
            try
            {
                // 🔍 重新計算累計金額（排除當前要刪除的明細）
                var newTotalSetoff = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == detailToDelete.SourceDetailType
                               && spd.SourceDetailId == detailToDelete.SourceDetailId
                               && spd.Id != detailToDelete.Id)  // ← 排除當前要刪除的
                    .SumAsync(spd => spd.TotalSetoffAmount);

                var newTotalAllowance = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == detailToDelete.SourceDetailType
                               && spd.SourceDetailId == detailToDelete.SourceDetailId
                               && spd.Id != detailToDelete.Id)
                    .SumAsync(spd => spd.TotalAllowanceAmount);

                // 💾 根據來源明細類型，更新對應的累計金額（快取欄位）
                switch (detailToDelete.SourceDetailType)
                {
                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (purchaseDetail != null)
                        {
                            purchaseDetail.TotalPaidAmount = newTotalSetoff;
                            purchaseDetail.IsSettled = newTotalSetoff >= purchaseDetail.SubtotalAmount;
                            
                            _logger?.LogDebug(
                                "回朔 PurchaseReceivingDetail Id={Id}: TotalPaidAmount {Old} → {New}",
                                purchaseDetail.Id,
                                purchaseDetail.TotalPaidAmount + detailToDelete.TotalSetoffAmount,
                                newTotalSetoff);
                        }
                        break;

                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (salesDetail != null)
                        {
                            salesDetail.TotalReceivedAmount = newTotalSetoff;
                            salesDetail.IsSettled = newTotalSetoff >= salesDetail.SubtotalAmount;
                            
                            _logger?.LogDebug(
                                "回朔 SalesOrderDetail Id={Id}: TotalReceivedAmount {Old} → {New}",
                                salesDetail.Id,
                                salesDetail.TotalReceivedAmount + detailToDelete.TotalSetoffAmount,
                                newTotalSetoff);
                        }
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (salesReturnDetail != null)
                        {
                            salesReturnDetail.TotalPaidAmount = newTotalSetoff;
                            salesReturnDetail.IsSettled = newTotalSetoff >= salesReturnDetail.ReturnSubtotalAmount;
                            
                            _logger?.LogDebug(
                                "回朔 SalesReturnDetail Id={Id}: TotalPaidAmount {Old} → {New}",
                                salesReturnDetail.Id,
                                salesReturnDetail.TotalPaidAmount + detailToDelete.TotalSetoffAmount,
                                newTotalSetoff);
                        }
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.TotalReceivedAmount = newTotalSetoff;
                            purchaseReturnDetail.IsSettled = newTotalSetoff >= purchaseReturnDetail.ReturnSubtotalAmount;
                            
                            _logger?.LogDebug(
                                "回朔 PurchaseReturnDetail Id={Id}: TotalReceivedAmount {Old} → {New}",
                                purchaseReturnDetail.Id,
                                purchaseReturnDetail.TotalReceivedAmount + detailToDelete.TotalSetoffAmount,
                                newTotalSetoff);
                        }
                        break;

                    default:
                        _logger?.LogWarning(
                            "未知的來源明細類型: {SourceDetailType}",
                            detailToDelete.SourceDetailType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, 
                    "回朔來源明細金額時發生錯誤 SourceType={SourceType} SourceId={SourceId}",
                    detailToDelete.SourceDetailType,
                    detailToDelete.SourceDetailId);
                throw; // 重新拋出例外，讓 Transaction 回滾
            }
        }

        /// <summary>
        /// 重建所有來源明細的快取金額（修復工具）
        /// </summary>
        /// <param name="sourceDetailType">來源明細類型（null 表示全部）</param>
        /// <returns>重建結果</returns>
        public async Task<ServiceResult> RebuildCacheAsync(SetoffDetailType? sourceDetailType = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var rebuiltCount = 0;

                // 根據類型重建快取
                var typesToRebuild = sourceDetailType.HasValue
                    ? new[] { sourceDetailType.Value }
                    : Enum.GetValues<SetoffDetailType>();

                foreach (var type in typesToRebuild)
                {
                    rebuiltCount += await RebuildCacheByTypeAsync(context, type);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger?.LogInformation("成功重建 {Count} 筆快取資料", rebuiltCount);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RebuildCacheAsync), GetType(), _logger);
                return ServiceResult.Failure($"重建快取時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 根據類型重建快取
        /// </summary>
        private async Task<int> RebuildCacheByTypeAsync(AppDbContext context, SetoffDetailType type)
        {
            var count = 0;

            switch (type)
            {
                case SetoffDetailType.PurchaseReceivingDetail:
                    var purchaseDetails = await context.PurchaseReceivingDetails.ToListAsync();
                    foreach (var detail in purchaseDetails)
                    {
                        var total = await context.SetoffProductDetails
                            .Where(spd => spd.SourceDetailType == type && spd.SourceDetailId == detail.Id)
                            .SumAsync(spd => spd.TotalSetoffAmount);
                        
                        detail.TotalPaidAmount = total;
                        detail.IsSettled = total >= detail.SubtotalAmount;
                        count++;
                    }
                    break;

                case SetoffDetailType.SalesOrderDetail:
                    var salesDetails = await context.SalesOrderDetails.ToListAsync();
                    foreach (var detail in salesDetails)
                    {
                        var total = await context.SetoffProductDetails
                            .Where(spd => spd.SourceDetailType == type && spd.SourceDetailId == detail.Id)
                            .SumAsync(spd => spd.TotalSetoffAmount);
                        
                        detail.TotalReceivedAmount = total;
                        detail.IsSettled = total >= detail.SubtotalAmount;
                        count++;
                    }
                    break;

                case SetoffDetailType.SalesReturnDetail:
                    var salesReturnDetails = await context.SalesReturnDetails.ToListAsync();
                    foreach (var detail in salesReturnDetails)
                    {
                        var total = await context.SetoffProductDetails
                            .Where(spd => spd.SourceDetailType == type && spd.SourceDetailId == detail.Id)
                            .SumAsync(spd => spd.TotalSetoffAmount);
                        
                        detail.TotalPaidAmount = total;
                        detail.IsSettled = total >= detail.ReturnSubtotalAmount;
                        count++;
                    }
                    break;

                case SetoffDetailType.PurchaseReturnDetail:
                    var purchaseReturnDetails = await context.PurchaseReturnDetails.ToListAsync();
                    foreach (var detail in purchaseReturnDetails)
                    {
                        var total = await context.SetoffProductDetails
                            .Where(spd => spd.SourceDetailType == type && spd.SourceDetailId == detail.Id)
                            .SumAsync(spd => spd.TotalSetoffAmount);
                        
                        detail.TotalReceivedAmount = total;
                        detail.IsSettled = total >= detail.ReturnSubtotalAmount;
                        count++;
                    }
                    break;
            }

            return count;
        }
    }
}
