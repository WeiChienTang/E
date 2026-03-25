using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 子科目自動產生服務實作
    /// 每位客戶/廠商會在四個統制科目下各建立一支明細子科目：
    ///   應收/應付帳款、應收/應付票據、銷貨退回/進貨退出、預收/預付款項
    /// 代碼格式：{上層科目代碼}.{流水號3位}（例如 1191.001）
    /// </summary>
    public class SubAccountService : ISubAccountService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ISystemParameterService _systemParameterService;
        private readonly ILogger<SubAccountService>? _logger;

        public SubAccountService(
            IDbContextFactory<AppDbContext> contextFactory,
            ISystemParameterService systemParameterService,
            ILogger<SubAccountService>? logger = null)
        {
            _contextFactory = contextFactory;
            _systemParameterService = systemParameterService;
            _logger = logger;
        }

        // ===== 查詢（只回傳主應收/應付帳款子科目，供傳票自動產生用）=====

        public async Task<AccountItem?> GetSubAccountForCustomerAsync(int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedCustomerId == customerId
                    && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable));
        }

        public async Task<AccountItem?> GetSubAccountForSupplierAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedSupplierId == supplierId
                    && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable));
        }

        public async Task<List<AccountItem>> GetAllSubAccountsForCustomerAsync(int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .Where(a => a.LinkedCustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<AccountItem>> GetAllSubAccountsForSupplierAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .Where(a => a.LinkedSupplierId == supplierId)
                .ToListAsync();
        }

        public async Task<AccountItem?> GetSubAccountForItemAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedItemId == productId);
        }

        // ===== 建立（若已存在則直接回傳）=====

        public async Task<AccountItem?> GetOrCreateCustomerSubAccountAsync(int customerId, string createdBy)
        {
            try
            {
                // 先查詢是否已存在主應收子科目，已存在則直接回傳（不受 AutoCreate 開關影響）
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedCustomerId == customerId
                        && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable));
                if (existing != null) return existing;

                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateCustomerSubAccount != true) return null;

                return await CreateCustomerSubAccountCoreAsync(customerId, createdBy, param);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立客戶子科目時發生錯誤（CustomerId={CustomerId}）", customerId);
                return null;
            }
        }

        /// <summary>客戶子科目建立核心邏輯（不檢查 AutoCreate 開關，供批次補建直接呼叫）</summary>
        private async Task<AccountItem?> CreateCustomerSubAccountCoreAsync(int customerId, string createdBy, Data.Entities.SystemParameter param)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var customer = await context.Customers.FindAsync(customerId);
            if (customer == null) return null;

            var customerName = customer.CompanyName ?? $"客戶{customerId}";

            var specs = BuildCustomerSpecs(param);
            var results = await GetOrCreateAllSubAccountsAsync(
                context, specs, customerName, customer.Code,
                param.SubAccountCodeFormat, createdBy,
                subAccount => subAccount.LinkedCustomerId = customerId,
                id => context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedCustomerId == id
                        && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable)),
                (id, type) => context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedCustomerId == id && a.SubAccountLinkType == type),
                customerId);

            var arAccount = results.FirstOrDefault(a =>
                a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable);

            _logger?.LogInformation("已完成客戶子科目建立（CustomerId={CustomerId}，共 {Count} 支）", customerId, results.Count);
            return arAccount;
        }

        public async Task<AccountItem?> GetOrCreateSupplierSubAccountAsync(int supplierId, string createdBy)
        {
            try
            {
                // 先查詢是否已存在主應付子科目，已存在則直接回傳（不受 AutoCreate 開關影響）
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedSupplierId == supplierId
                        && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable));
                if (existing != null) return existing;

                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateSupplierSubAccount != true) return null;

                return await CreateSupplierSubAccountCoreAsync(supplierId, createdBy, param);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立廠商子科目時發生錯誤（SupplierId={SupplierId}）", supplierId);
                return null;
            }
        }

        /// <summary>廠商子科目建立核心邏輯（不檢查 AutoCreate 開關，供批次補建直接呼叫）</summary>
        private async Task<AccountItem?> CreateSupplierSubAccountCoreAsync(int supplierId, string createdBy, Data.Entities.SystemParameter param)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var supplier = await context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return null;

            var supplierName = supplier.CompanyName ?? $"廠商{supplierId}";

            var specs = BuildSupplierSpecs(param);
            var results = await GetOrCreateAllSubAccountsAsync(
                context, specs, supplierName, supplier.Code,
                param.SubAccountCodeFormat, createdBy,
                subAccount => subAccount.LinkedSupplierId = supplierId,
                id => context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedSupplierId == id
                        && (a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable)),
                (id, type) => context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedSupplierId == id && a.SubAccountLinkType == type),
                supplierId);

            var apAccount = results.FirstOrDefault(a =>
                a.SubAccountLinkType == null || a.SubAccountLinkType == SubAccountLinkType.ReceivablePayable);

            _logger?.LogInformation("已完成廠商子科目建立（SupplierId={SupplierId}，共 {Count} 支）", supplierId, results.Count);
            return apAccount;
        }

        public async Task<AccountItem?> GetOrCreateItemSubAccountAsync(int productId, string createdBy)
        {
            try
            {
                // 先查詢是否已存在品項子科目，已存在則直接回傳（不受 AutoCreate 開關影響）
                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedItemId == productId);
                if (existing != null) return existing;

                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateItemSubAccount != true) return null;

                return await CreateItemSubAccountCoreAsync(productId, createdBy, param);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立品項子科目時發生錯誤（ItemId={ItemId}）", productId);
                return null;
            }
        }

        /// <summary>品項子科目建立核心邏輯（不檢查 AutoCreate 開關，供批次補建直接呼叫）</summary>
        private async Task<AccountItem?> CreateItemSubAccountCoreAsync(int productId, string createdBy, Data.Entities.SystemParameter param)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var parentCode = param.ItemSubAccountParentCode;
            var parent = await context.AccountItems
                .FirstOrDefaultAsync(a => a.Code == parentCode);
            if (parent == null)
            {
                _logger?.LogWarning("找不到品項子科目統制科目（{Code}），跳過建立", parentCode);
                return null;
            }

            var product = await context.Items.FindAsync(productId);
            if (product == null) return null;

            var productName = product.Name ?? $"品項{productId}";
            var code = await GenerateSubAccountCodeAsync(context, parent, param.SubAccountCodeFormat, product.Code);
            var subAccount = BuildSubAccount(code, $"{parent.Name} - {productName}", parent, createdBy);
            subAccount.Description = productName;
            subAccount.LinkedItemId = productId;

            context.AccountItems.Add(subAccount);
            await context.SaveChangesAsync();

            _logger?.LogInformation("已建立品項子科目 {Code} 對應品項 {ItemId}", code, productId);
            return subAccount;
        }

        // ===== 批次補建 =====

        public async Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllCustomersAsync(string createdBy)
        {
            int created = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                // 批次補建不檢查 AutoCreateCustomerSubAccount 開關，取得 param 僅供讀取代碼格式與統制科目
                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param == null)
                {
                    errors.Add("無法取得系統參數");
                    return (created, skipped, errors);
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var customerIds = await context.Customers
                    .Where(c => c.Status == EntityStatus.Active)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var id in customerIds)
                {
                    try
                    {
                        // 檢查四種子科目是否全部存在，全部齊全才跳過
                        var existingCount = await context.AccountItems
                            .CountAsync(a => a.LinkedCustomerId == id);
                        var expectedCount = BuildCustomerSpecs(param).Count(s => !string.IsNullOrWhiteSpace(s.ParentCode));
                        if (existingCount >= expectedCount)
                        {
                            skipped++;
                            continue;
                        }

                        // 直接呼叫核心邏輯，不經過 AutoCreate 開關檢查
                        // 核心邏輯會逐類檢查，已存在的類型自動跳過，只補建缺少的
                        var result = await CreateCustomerSubAccountCoreAsync(id, createdBy, param);
                        if (result != null) created++;
                        else skipped++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"客戶 {id}：{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"批次補建客戶子科目發生錯誤：{ex.Message}");
            }

            return (created, skipped, errors);
        }

        public async Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllSuppliersAsync(string createdBy)
        {
            int created = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                // 批次補建不檢查 AutoCreateSupplierSubAccount 開關
                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param == null)
                {
                    errors.Add("無法取得系統參數");
                    return (created, skipped, errors);
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var supplierIds = await context.Suppliers
                    .Where(s => s.Status == EntityStatus.Active)
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var id in supplierIds)
                {
                    try
                    {
                        // 檢查四種子科目是否全部存在，全部齊全才跳過
                        var existingCount = await context.AccountItems
                            .CountAsync(a => a.LinkedSupplierId == id);
                        var expectedCount = BuildSupplierSpecs(param).Count(s => !string.IsNullOrWhiteSpace(s.ParentCode));
                        if (existingCount >= expectedCount)
                        {
                            skipped++;
                            continue;
                        }

                        // 直接呼叫核心邏輯，不經過 AutoCreate 開關檢查
                        // 核心邏輯會逐類檢查，已存在的類型自動跳過，只補建缺少的
                        var result = await CreateSupplierSubAccountCoreAsync(id, createdBy, param);
                        if (result != null) created++;
                        else skipped++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"廠商 {id}：{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"批次補建廠商子科目發生錯誤：{ex.Message}");
            }

            return (created, skipped, errors);
        }

        public async Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllItemsAsync(string createdBy)
        {
            int created = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                // 批次補建不檢查 AutoCreateItemSubAccount 開關
                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param == null)
                {
                    errors.Add("無法取得系統參數");
                    return (created, skipped, errors);
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var productIds = await context.Items
                    .Where(p => p.Status == EntityStatus.Active)
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var id in productIds)
                {
                    try
                    {
                        var existing = await context.AccountItems
                            .AnyAsync(a => a.LinkedItemId == id);
                        if (existing)
                        {
                            skipped++;
                            continue;
                        }

                        // 直接呼叫核心邏輯，不經過 AutoCreate 開關檢查
                        // 使用獨立 DbContext 避免錯誤累積影響後續迭代
                        var result = await CreateItemSubAccountCoreAsync(id, createdBy, param);
                        if (result != null) created++;
                        else skipped++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"品項 {id}：{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"批次補建品項子科目發生錯誤：{ex.Message}");
            }

            return (created, skipped, errors);
        }

        // ===== 私有方法 =====

        /// <summary>
        /// 建立客戶的四種子科目規格（應收帳款、應收票據、銷貨退回、預收款項）
        /// </summary>
        private static List<(SubAccountLinkType LinkType, string ParentCode)> BuildCustomerSpecs(Data.Entities.SystemParameter param) =>
        [
            (SubAccountLinkType.ReceivablePayable,   param.CustomerSubAccountParentCode),
            (SubAccountLinkType.NoteReceivablePayable, param.CustomerNoteSubAccountParentCode),
            (SubAccountLinkType.SalesPurchaseReturn, param.CustomerReturnSubAccountParentCode),
            (SubAccountLinkType.AdvanceReceiptPayment, param.CustomerAdvanceSubAccountParentCode),
        ];

        /// <summary>
        /// 建立廠商的四種子科目規格（應付帳款、應付票據、進貨退出、預付款項）
        /// </summary>
        private static List<(SubAccountLinkType LinkType, string ParentCode)> BuildSupplierSpecs(Data.Entities.SystemParameter param) =>
        [
            (SubAccountLinkType.ReceivablePayable,   param.SupplierSubAccountParentCode),
            (SubAccountLinkType.NoteReceivablePayable, param.SupplierNoteSubAccountParentCode),
            (SubAccountLinkType.SalesPurchaseReturn, param.SupplierReturnSubAccountParentCode),
            (SubAccountLinkType.AdvanceReceiptPayment, param.SupplierAdvanceSubAccountParentCode),
        ];

        /// <summary>
        /// 針對一個實體（客戶或廠商），依規格清單逐類建立子科目（已存在者跳過）。
        /// 回傳本次已存在或新建的全部 AccountItem。
        /// </summary>
        private async Task<List<AccountItem>> GetOrCreateAllSubAccountsAsync(
            AppDbContext context,
            List<(SubAccountLinkType LinkType, string ParentCode)> specs,
            string entityName,
            string? entityCode,
            SubAccountCodeFormat format,
            string createdBy,
            Action<AccountItem> setLink,
            Func<int, Task<AccountItem?>> findMainExisting,
            Func<int, SubAccountLinkType, Task<AccountItem?>> findByType,
            int entityId)
        {
            var results = new List<AccountItem>();

            foreach (var (linkType, parentCode) in specs)
            {
                if (string.IsNullOrWhiteSpace(parentCode)) continue;

                // 查詢是否已存在（主應收/應付類型需相容舊的 null 值）
                AccountItem? existing = linkType == SubAccountLinkType.ReceivablePayable
                    ? await findMainExisting(entityId)
                    : await findByType(entityId, linkType);

                if (existing != null)
                {
                    results.Add(existing);
                    continue;
                }

                var parent = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.Code == parentCode);
                if (parent == null)
                {
                    _logger?.LogWarning("找不到子科目統制科目（{Code}，類型={Type}），跳過", parentCode, linkType);
                    continue;
                }

                var code = await GenerateSubAccountCodeAsync(context, parent, format, entityCode);
                var subAccount = BuildSubAccount(code, $"{parent.Name} - {entityName}", parent, createdBy);
                subAccount.Description = entityName;
                subAccount.SubAccountLinkType = linkType;
                setLink(subAccount);

                context.AccountItems.Add(subAccount);
                await context.SaveChangesAsync();

                _logger?.LogInformation("已建立子科目 {Code}（類型={Type}）對應實體 {EntityId}", code, linkType, entityId);
                results.Add(subAccount);
            }

            return results;
        }

        /// <summary>
        /// 產生下一個子科目代碼
        /// Sequential：{parentCode}.001, .002...
        /// EntityCode：{parentCode}.{entityCode}（點號與特殊字元會被過濾）
        /// </summary>
        private static async Task<string> GenerateSubAccountCodeAsync(
            AppDbContext context,
            AccountItem parent,
            SubAccountCodeFormat format = SubAccountCodeFormat.Sequential,
            string? entityCode = null)
        {
            // 實體編碼模式：直接用實體自身代碼作後綴
            // 建立前確認代碼唯一性：若不同實體共用相同 entityCode（如客戶代碼重複或被修改），
            // 會產生相同的子科目代碼，觸發 DB unique constraint 例外。
            // 衝突時 fallthrough 至流水號模式，確保一定能成功建立。
            if (format == SubAccountCodeFormat.EntityCode && !string.IsNullOrWhiteSpace(entityCode))
            {
                var sanitized = entityCode.Trim().Replace(".", "");
                if (!string.IsNullOrWhiteSpace(sanitized))
                {
                    var candidateCode = $"{parent.Code}.{sanitized}";
                    var codeConflict = await context.AccountItems.AnyAsync(a => a.Code == candidateCode);
                    if (!codeConflict)
                        return candidateCode;
                    // 代碼已被其他科目佔用 → fallthrough 至流水號
                }
                // entityCode 過濾後為空 → fallthrough 至流水號
            }

            // 流水號模式：查詢同父層已有的自動產生子科目，取最大流水號 +1
            var existingCodes = await context.AccountItems
                .Where(a => a.ParentId == parent.Id && a.IsAutoGenerated)
                .Select(a => a.Code)
                .ToListAsync();

            int maxSeq = 0;
            var prefix = $"{parent.Code}.";
            foreach (var code in existingCodes)
            {
                if (code != null && code.StartsWith(prefix))
                {
                    var seqStr = code[prefix.Length..];
                    if (int.TryParse(seqStr, out var seq) && seq > maxSeq)
                        maxSeq = seq;
                }
            }

            return $"{parent.Code}.{(maxSeq + 1):000}";
        }

        /// <summary>建立 AccountItem 子科目基礎物件（繼承父層大類與借貸方向）</summary>
        private static AccountItem BuildSubAccount(string code, string name, AccountItem parent, string createdBy)
        {
            return new AccountItem
            {
                Code            = code,
                Name            = name,
                AccountLevel    = parent.AccountLevel + 1,
                ParentId        = parent.Id,
                AccountType     = parent.AccountType,
                Direction       = parent.Direction,
                IsDetailAccount = true,
                IsAutoGenerated = true,
                SortOrder       = 0,
                Status          = EntityStatus.Active,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow,
                CreatedBy       = createdBy,
                UpdatedBy       = createdBy
            };
        }
    }
}
