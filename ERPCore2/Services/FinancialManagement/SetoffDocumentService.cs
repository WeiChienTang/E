using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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

        protected override IQueryable<SetoffDocument> BuildGetAllQuery(AppDbContext context)
        {
            return context.SetoffDocuments
                .Include(s => s.Company)
                .OrderByDescending(s => s.SetoffDate)
                .ThenByDescending(s => s.Code);
        }

        /// <summary>
        /// 覆寫後處理鉤子：在 GetAllAsync / GetAllIncludingDraftsAsync 查詢結果後，
        /// 載入無法透過 Include 取得的關聯方名稱（跨 Customer/Supplier 的動態關聯）。
        /// </summary>
        protected override async Task PostProcessGetAllResultsAsync(AppDbContext context, List<SetoffDocument> entities)
        {
            await LoadRelatedPartyNamesAsync(context, entities);
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
                        (s.Code != null && s.Code.ToLower().Contains(searchTermLower)) ||
                        s.Company.CompanyName.ToLower().Contains(searchTermLower))
                    .OrderByDescending(s => s.SetoffDate)
                    .ThenByDescending(s => s.Code)
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

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("沖款單號不能為空");

                if (entity.SetoffDate == default)
                    errors.Add("沖款日期不能為空");

                if (entity.RelatedPartyId <= 0)
                    errors.Add("關聯方為必填");

                if (string.IsNullOrWhiteSpace(entity.RelatedPartyType))
                    errors.Add("關聯方類型不能為空");

                if (entity.CompanyId <= 0)
                    errors.Add("公司為必填");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsSetoffNumberExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
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
                    SetoffNumber = entity.Code
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
                var query = context.SetoffDocuments.Where(s => s.Code == setoffNumber);
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
        /// 檢查沖款單編號是否已存在（別名方法，供 EntityCodeGenerationHelper 使用）
        /// </summary>
        public async Task<bool> IsSetoffDocumentCodeExistsAsync(string code, int? excludeId = null)
        {
            // 直接調用 IsSetoffNumberExistsAsync
            return await IsSetoffNumberExistsAsync(code, excludeId);
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
                    .ThenByDescending(s => s.Code)
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
                    .ThenByDescending(s => s.Code)
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
                    .ThenByDescending(s => s.Code)
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
                    .ThenByDescending(s => s.Code)
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
                
                foreach (var detail in document.SetoffProductDetails)
                {
                    await RollbackSourceDetailAmountAsync(context, detail);
                }

                // 🗑️ 刪除沖款單（級聯刪除所有關聯明細）
                context.SetoffDocuments.Remove(document);

                // 💾 儲存變更
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
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
                        }
                        break;

                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (salesDetail != null)
                        {
                            salesDetail.TotalReceivedAmount = newTotalSetoff;
                            salesDetail.IsSettled = newTotalSetoff >= salesDetail.SubtotalAmount;
                        }
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (salesReturnDetail != null)
                        {
                            salesReturnDetail.TotalPaidAmount = newTotalSetoff;
                            salesReturnDetail.IsSettled = newTotalSetoff >= salesReturnDetail.ReturnSubtotalAmount;
                        }
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .FindAsync(detailToDelete.SourceDetailId);
                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.TotalReceivedAmount = newTotalSetoff;
                            purchaseReturnDetail.IsSettled = newTotalSetoff >= purchaseReturnDetail.ReturnSubtotalAmount;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch
            {
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

        public async Task<List<SetoffDocument>> GetByPurchaseReceivingIdAsync(int purchaseReceivingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var receivingDetailIds = await context.PurchaseReceivingDetails
                    .Where(prd => prd.PurchaseReceivingId == purchaseReceivingId)
                    .Select(prd => prd.Id)
                    .ToListAsync();

                var documentIds = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == SetoffDetailType.PurchaseReceivingDetail &&
                                  receivingDetailIds.Contains(spd.SourceDetailId))
                    .Select(spd => spd.SetoffDocumentId)
                    .Distinct()
                    .ToListAsync();

                return await context.SetoffDocuments
                    .Where(sd => documentIds.Contains(sd.Id))
                    .OrderByDescending(sd => sd.CreatedAt)
                    .ThenBy(sd => sd.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReceivingIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPurchaseReceivingIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReceivingId = purchaseReceivingId
                });
                return new List<SetoffDocument>();
            }
        }

        public async Task<List<SetoffDocument>> GetByPurchaseReturnIdAsync(int purchaseReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var returnDetailIds = await context.PurchaseReturnDetails
                    .Where(prd => prd.PurchaseReturnId == purchaseReturnId)
                    .Select(prd => prd.Id)
                    .ToListAsync();

                var documentIds = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == SetoffDetailType.PurchaseReturnDetail &&
                                  returnDetailIds.Contains(spd.SourceDetailId))
                    .Select(spd => spd.SetoffDocumentId)
                    .Distinct()
                    .ToListAsync();

                return await context.SetoffDocuments
                    .Where(sd => documentIds.Contains(sd.Id))
                    .OrderByDescending(sd => sd.CreatedAt)
                    .ThenBy(sd => sd.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPurchaseReturnIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPurchaseReturnIdAsync),
                    ServiceType = GetType().Name,
                    PurchaseReturnId = purchaseReturnId
                });
                return new List<SetoffDocument>();
            }
        }

        public async Task<List<SetoffDocument>> GetBySalesDeliveryIdAsync(int salesDeliveryId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var deliveryDetailIds = await context.SalesDeliveryDetails
                    .Where(sdd => sdd.SalesDeliveryId == salesDeliveryId)
                    .Select(sdd => sdd.Id)
                    .ToListAsync();

                var documentIds = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == SetoffDetailType.SalesDeliveryDetail &&
                                  deliveryDetailIds.Contains(spd.SourceDetailId))
                    .Select(spd => spd.SetoffDocumentId)
                    .Distinct()
                    .ToListAsync();

                return await context.SetoffDocuments
                    .Where(sd => documentIds.Contains(sd.Id))
                    .OrderByDescending(sd => sd.CreatedAt)
                    .ThenBy(sd => sd.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesDeliveryIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesDeliveryIdAsync),
                    ServiceType = GetType().Name,
                    SalesDeliveryId = salesDeliveryId
                });
                return new List<SetoffDocument>();
            }
        }

        public async Task<List<SetoffDocument>> GetBySalesReturnIdAsync(int salesReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var returnDetailIds = await context.SalesReturnDetails
                    .Where(srd => srd.SalesReturnId == salesReturnId)
                    .Select(srd => srd.Id)
                    .ToListAsync();

                var documentIds = await context.SetoffProductDetails
                    .Where(spd => spd.SourceDetailType == SetoffDetailType.SalesReturnDetail &&
                                  returnDetailIds.Contains(spd.SourceDetailId))
                    .Select(spd => spd.SetoffDocumentId)
                    .Distinct()
                    .ToListAsync();

                return await context.SetoffDocuments
                    .Where(sd => documentIds.Contains(sd.Id))
                    .OrderByDescending(sd => sd.CreatedAt)
                    .ThenBy(sd => sd.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesReturnIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesReturnIdAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId
                });
                return new List<SetoffDocument>();
            }
        }
    }
}
