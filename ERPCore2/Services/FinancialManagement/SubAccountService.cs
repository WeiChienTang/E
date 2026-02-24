using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 子科目自動產生服務實作
    /// 代碼格式：{上層科目代碼}.{流水號3位}（例如 1191.001）
    /// 新增客戶/廠商/商品後，若系統參數啟用，則在對應統制科目下自動建立明細子科目
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

        // ===== 查詢 =====

        public async Task<AccountItem?> GetSubAccountForCustomerAsync(int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedCustomerId == customerId);
        }

        public async Task<AccountItem?> GetSubAccountForSupplierAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedSupplierId == supplierId);
        }

        public async Task<AccountItem?> GetSubAccountForProductAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AccountItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.LinkedProductId == productId);
        }

        // ===== 建立 =====

        public async Task<AccountItem?> GetOrCreateCustomerSubAccountAsync(int customerId, string createdBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 若已存在，直接回傳
                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedCustomerId == customerId);
                if (existing != null) return existing;

                // 檢查系統參數
                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateCustomerSubAccount != true) return null;

                // 取得統制科目
                var parentCode = param.CustomerSubAccountParentCode;
                var parent = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.Code == parentCode);
                if (parent == null)
                {
                    _logger?.LogWarning("找不到客戶子科目統制科目（{Code}），跳過建立", parentCode);
                    return null;
                }

                // 取得客戶名稱
                var customer = await context.Customers.FindAsync(customerId);
                if (customer == null) return null;

                // 產生代碼並建立子科目（Name 繼承上層科目名稱，公司名稱存 Description）
                var code = await GenerateSubAccountCodeAsync(context, parent, param.SubAccountCodeFormat, customer.Code);
                var subAccount = BuildSubAccount(code, parent.Name, parent, createdBy);
                subAccount.Description = customer.CompanyName ?? $"客戶{customerId}";
                subAccount.LinkedCustomerId = customerId;

                context.AccountItems.Add(subAccount);
                await context.SaveChangesAsync();

                _logger?.LogInformation("已建立客戶子科目 {Code} 對應客戶 {CustomerId}", code, customerId);
                return subAccount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立客戶子科目時發生錯誤（CustomerId={CustomerId}）", customerId);
                return null;
            }
        }

        public async Task<AccountItem?> GetOrCreateSupplierSubAccountAsync(int supplierId, string createdBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedSupplierId == supplierId);
                if (existing != null) return existing;

                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateSupplierSubAccount != true) return null;

                var parentCode = param.SupplierSubAccountParentCode;
                var parent = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.Code == parentCode);
                if (parent == null)
                {
                    _logger?.LogWarning("找不到廠商子科目統制科目（{Code}），跳過建立", parentCode);
                    return null;
                }

                var supplier = await context.Suppliers.FindAsync(supplierId);
                if (supplier == null) return null;

                var code = await GenerateSubAccountCodeAsync(context, parent, param.SubAccountCodeFormat, supplier.Code);
                var subAccount = BuildSubAccount(code, parent.Name, parent, createdBy);
                subAccount.Description = supplier.CompanyName ?? $"廠商{supplierId}";
                subAccount.LinkedSupplierId = supplierId;

                context.AccountItems.Add(subAccount);
                await context.SaveChangesAsync();

                _logger?.LogInformation("已建立廠商子科目 {Code} 對應廠商 {SupplierId}", code, supplierId);
                return subAccount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立廠商子科目時發生錯誤（SupplierId={SupplierId}）", supplierId);
                return null;
            }
        }

        public async Task<AccountItem?> GetOrCreateProductSubAccountAsync(int productId, string createdBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.LinkedProductId == productId);
                if (existing != null) return existing;

                var param = await _systemParameterService.GetSystemParameterAsync();
                if (param?.AutoCreateProductSubAccount != true) return null;

                var parentCode = param.ProductSubAccountParentCode;
                var parent = await context.AccountItems
                    .FirstOrDefaultAsync(a => a.Code == parentCode);
                if (parent == null)
                {
                    _logger?.LogWarning("找不到商品子科目統制科目（{Code}），跳過建立", parentCode);
                    return null;
                }

                var product = await context.Products.FindAsync(productId);
                if (product == null) return null;

                var code = await GenerateSubAccountCodeAsync(context, parent, param.SubAccountCodeFormat, product.Code);
                var subAccount = BuildSubAccount(code, parent.Name, parent, createdBy);
                subAccount.Description = product.Name ?? $"商品{productId}";
                subAccount.LinkedProductId = productId;

                context.AccountItems.Add(subAccount);
                await context.SaveChangesAsync();

                _logger?.LogInformation("已建立商品子科目 {Code} 對應商品 {ProductId}", code, productId);
                return subAccount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "建立商品子科目時發生錯誤（ProductId={ProductId}）", productId);
                return null;
            }
        }

        // ===== 批次補建 =====

        public async Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllCustomersAsync(string createdBy)
        {
            int created = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var customerIds = await context.Customers
                    .Where(c => c.Status == EntityStatus.Active)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var id in customerIds)
                {
                    try
                    {
                        var existing = await context.AccountItems
                            .AnyAsync(a => a.LinkedCustomerId == id);
                        if (existing)
                        {
                            skipped++;
                            continue;
                        }

                        var result = await GetOrCreateCustomerSubAccountAsync(id, createdBy);
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var supplierIds = await context.Suppliers
                    .Where(s => s.Status == EntityStatus.Active)
                    .Select(s => s.Id)
                    .ToListAsync();

                foreach (var id in supplierIds)
                {
                    try
                    {
                        var existing = await context.AccountItems
                            .AnyAsync(a => a.LinkedSupplierId == id);
                        if (existing)
                        {
                            skipped++;
                            continue;
                        }

                        var result = await GetOrCreateSupplierSubAccountAsync(id, createdBy);
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

        public async Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllProductsAsync(string createdBy)
        {
            int created = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var productIds = await context.Products
                    .Where(p => p.Status == EntityStatus.Active)
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var id in productIds)
                {
                    try
                    {
                        var existing = await context.AccountItems
                            .AnyAsync(a => a.LinkedProductId == id);
                        if (existing)
                        {
                            skipped++;
                            continue;
                        }

                        var result = await GetOrCreateProductSubAccountAsync(id, createdBy);
                        if (result != null) created++;
                        else skipped++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"商品 {id}：{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"批次補建商品子科目發生錯誤：{ex.Message}");
            }

            return (created, skipped, errors);
        }

        // ===== 私有方法 =====

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
            if (format == SubAccountCodeFormat.EntityCode && !string.IsNullOrWhiteSpace(entityCode))
            {
                // 過濾點號（避免與分隔符衝突）及前後空白
                var sanitized = entityCode.Trim().Replace(".", "");
                if (!string.IsNullOrWhiteSpace(sanitized))
                    return $"{parent.Code}.{sanitized}";
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
                Code        = code,
                Name        = name,
                AccountLevel    = parent.AccountLevel + 1,
                ParentId    = parent.Id,
                AccountType = parent.AccountType,
                Direction   = parent.Direction,
                IsDetailAccount = true,
                IsAutoGenerated = true,
                SortOrder   = 0,
                Status      = EntityStatus.Active,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow,
                CreatedBy   = createdBy,
                UpdatedBy   = createdBy
            };
        }
    }
}
