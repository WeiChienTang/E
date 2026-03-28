using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Navigation;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 系統參數服務實作
    /// </summary>
    public class SystemParameterService : GenericManagementService<SystemParameter>, ISystemParameterService
    {
        // ===== 審核配置快取 =====
        private SystemParameter? _cachedParameter;
        private DateTime _cacheExpiration = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public SystemParameterService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SystemParameter>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 覆寫更新方法，自動清除審核配置快取
        /// </summary>
        public override async Task<ServiceResult<SystemParameter>> UpdateAsync(SystemParameter entity)
        {
            var result = await base.UpdateAsync(entity);
            if (result.IsSuccess)
            {
                ClearApprovalConfigCache();
            }
            return result;
        }

        public override async Task<List<SystemParameter>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemParameters
                    .Where(sp => (sp.TaxRate.ToString().Contains(searchTerm) ||
                         (sp.Remarks != null && sp.Remarks.Contains(searchTerm))))
                    .OrderBy(sp => sp.Id)
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
                return new List<SystemParameter>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SystemParameter entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.TaxRate < 0 || entity.TaxRate > 100)
                    errors.Add("稅率範圍必須在 0% 到 100% 之間");

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
                    TaxRate = entity.TaxRate
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<decimal> GetTaxRateAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var systemParameter = await context.SystemParameters
                    .Where(sp => sp.Status == EntityStatus.Active)
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();

                return systemParameter?.TaxRate ?? 5.00m; // 預設 5% 稅率
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTaxRateAsync), GetType(), _logger, new
                {
                    Method = nameof(GetTaxRateAsync),
                    ServiceType = GetType().Name
                });
                return 5.00m; // 發生錯誤時返回預設稅率
            }
        }

        public async Task<bool> SetTaxRateAsync(decimal taxRate)
        {
            try
            {
                if (taxRate < 0 || taxRate > 100)
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var systemParameter = await context.SystemParameters
                    .AsQueryable()
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();

                if (systemParameter == null)
                {
                    // 如果不存在系統參數，創建一個新的
                    systemParameter = new SystemParameter
                    {
                        TaxRate = taxRate,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        Remarks = "系統自動創建的稅率設定"
                    };
                    await context.SystemParameters.AddAsync(systemParameter);
                }
                else
                {
                    systemParameter.TaxRate = taxRate;
                    systemParameter.UpdatedAt = DateTime.UtcNow;
                    systemParameter.UpdatedBy = "System";
                    context.SystemParameters.Update(systemParameter);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetTaxRateAsync), GetType(), _logger, new
                {
                    Method = nameof(SetTaxRateAsync),
                    ServiceType = GetType().Name,
                    TaxRate = taxRate
                });
                return false;
            }
        }

        public async Task<SystemParameter?> GetSystemParameterAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SystemParameters
                    .Where(sp => sp.Status == EntityStatus.Active)
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSystemParameterAsync), GetType(), _logger, new
                {
                    Method = nameof(GetSystemParameterAsync),
                    ServiceType = GetType().Name
                });
                return null;
            }
        }

        // ===== 審核流程開關查詢方法 =====

        /// <summary>
        /// 取得系統參數（帶快取）
        /// </summary>
        private async Task<SystemParameter?> GetCachedSystemParameterAsync()
        {
            if (_cachedParameter == null || DateTime.Now > _cacheExpiration)
            {
                _cachedParameter = await GetSystemParameterAsync();
                _cacheExpiration = DateTime.Now.Add(_cacheDuration);
            }
            return _cachedParameter;
        }

        /// <summary>
        /// 清除審核配置快取（當系統參數更新時使用）
        /// </summary>
        public void ClearApprovalConfigCache()
        {
            _cachedParameter = null;
            _cacheExpiration = DateTime.MinValue;
        }

        /// <summary>
        /// 統一的審核模式查詢（false=系統自動審核，true=人工審核）
        /// </summary>
        public async Task<bool> IsManualApprovalAsync(ApprovalType approvalType)
        {
            try
            {
                var parameter = await GetCachedSystemParameterAsync();
                if (parameter == null) return false; // 安全預設：找不到參數時使用自動審核

                return approvalType switch
                {
                    ApprovalType.Quotation => parameter.QuotationManualApproval,
                    ApprovalType.PurchaseOrder => parameter.PurchaseOrderManualApproval,
                    ApprovalType.PurchaseReceiving => parameter.PurchaseReceivingManualApproval,
                    ApprovalType.PurchaseReturn => parameter.PurchaseReturnManualApproval,
                    ApprovalType.SalesOrder => parameter.SalesOrderManualApproval,
                    ApprovalType.SalesReturn => parameter.SalesReturnManualApproval,
                    ApprovalType.SalesDelivery => parameter.SalesDeliveryManualApproval,
                    ApprovalType.InventoryTransfer => parameter.InventoryTransferManualApproval,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsManualApprovalAsync), GetType(), _logger, new
                {
                    Method = nameof(IsManualApprovalAsync),
                    ServiceType = GetType().Name,
                    ApprovalType = approvalType
                });
                return false; // 發生錯誤時預設使用自動審核
            }
        }

        /// <summary>
        /// 報價單是否使用人工審核
        /// </summary>
        public async Task<bool> IsQuotationManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.Quotation);

        /// <summary>
        /// 採購訂單是否使用人工審核
        /// </summary>
        public async Task<bool> IsPurchaseOrderManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.PurchaseOrder);

        /// <summary>
        /// 進貨單是否使用人工審核
        /// </summary>
        public async Task<bool> IsPurchaseReceivingManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.PurchaseReceiving);

        /// <summary>
        /// 進貨退回是否使用人工審核
        /// </summary>
        public async Task<bool> IsPurchaseReturnManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.PurchaseReturn);

        /// <summary>
        /// 銷貨訂單是否使用人工審核
        /// </summary>
        public async Task<bool> IsSalesOrderManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.SalesOrder);

        /// <summary>
        /// 銷貨退回是否使用人工審核
        /// </summary>
        public async Task<bool> IsSalesReturnManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.SalesReturn);

        /// <summary>
        /// 出貨單是否使用人工審核
        /// </summary>
        public async Task<bool> IsSalesDeliveryManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.SalesDelivery);

        /// <summary>
        /// 庫存調撥是否使用人工審核
        /// </summary>
        public async Task<bool> IsInventoryTransferManualApprovalAsync()
            => await IsManualApprovalAsync(ApprovalType.InventoryTransfer);

        /// <summary>
        /// 是否隱藏所有模組的審核資訊欄位（false=顯示，true=隱藏）
        /// </summary>
        public async Task<bool> IsApprovalInfoHiddenAsync()
        {
            var parameter = await GetCachedSystemParameterAsync();
            return parameter?.HideApprovalInfoSection ?? false;
        }

        // ===== 恢復預設 =====

        /// <summary>
        /// 將系統參數重置為預設值
        /// </summary>
        public async Task<ServiceResult<SystemParameter>> ResetToDefaultAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var parameter = await context.SystemParameters
                    .Where(sp => sp.Status == EntityStatus.Active)
                    .OrderBy(sp => sp.Id)
                    .FirstOrDefaultAsync();

                if (parameter == null)
                {
                    return ServiceResult<SystemParameter>.Failure("找不到系統參數記錄");
                }

                // 從 SystemParameterDefaults 套用預設值（保留 Id、CreatedAt、CreatedBy）
                SystemParameterDefaults.ApplyDefaults(parameter);

                parameter.UpdatedAt = DateTime.UtcNow;
                parameter.UpdatedBy = "System";

                context.SystemParameters.Update(parameter);
                await context.SaveChangesAsync();

                ClearApprovalConfigCache();

                return ServiceResult<SystemParameter>.Success(parameter);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetToDefaultAsync), GetType(), _logger, new
                {
                    Method = nameof(ResetToDefaultAsync),
                    ServiceType = GetType().Name
                });
                return ServiceResult<SystemParameter>.Failure("重置系統參數時發生錯誤");
            }
        }
    }
}
