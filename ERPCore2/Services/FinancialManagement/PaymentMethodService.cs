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
    /// 付款方式服務實作
    /// </summary>
    public class PaymentMethodService : GenericManagementService<PaymentMethod>, IPaymentMethodService
    {
        public PaymentMethodService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PaymentMethod>> logger) : base(contextFactory, logger)
        {
        }

        public PaymentMethodService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以提供自訂排序
        public override async Task<List<PaymentMethod>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaymentMethods
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenBy(pm => pm.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PaymentMethod>();
            }
        }

        // 覆寫 GetActiveAsync 以提供自訂排序
        public override async Task<List<PaymentMethod>> GetActiveAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaymentMethods
                    .Where(pm => pm.Status == EntityStatus.Active)
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenBy(pm => pm.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveAsync), GetType(), _logger, new { 
                    Method = nameof(GetActiveAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PaymentMethod>();
            }
        }

        // 實作搜尋功能
        public override async Task<List<PaymentMethod>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.PaymentMethods
                    .Where(pm => pm.Name.ToLower().Contains(searchTermLower))
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenBy(pm => pm.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PaymentMethod>();
            }
        }

        // 實作驗證功能
        public override async Task<ServiceResult> ValidateAsync(PaymentMethod entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("付款方式名稱為必填");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("付款方式名稱已存在");
                
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
                    EntityName = entity.Name 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 檢查付款方式代碼是否已存在
        public async Task<bool> IsPaymentMethodCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PaymentMethods.Where(pm => pm.Code == code);
                
                if (excludeId.HasValue)
                    query = query.Where(pm => pm.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPaymentMethodCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsPaymentMethodCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        // 檢查付款方式名稱是否已存在
        public new async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PaymentMethods.Where(pm => pm.Name == name);
                
                if (excludeId.HasValue)
                    query = query.Where(pm => pm.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = name,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        // 獲取預設付款方式
        public async Task<PaymentMethod?> GetDefaultAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PaymentMethods
                    .Where(pm => pm.IsDefault && pm.Status == EntityStatus.Active)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDefaultAsync), GetType(), _logger, new { 
                    Method = nameof(GetDefaultAsync),
                    ServiceType = GetType().Name 
                });
                return null;
            }
        }

        // 設定預設付款方式
        public async Task<ServiceResult> SetDefaultAsync(int paymentMethodId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查目標付款方式是否存在且為啟用狀態
                var targetPaymentMethod = await context.PaymentMethods
                    .Where(pm => pm.Id == paymentMethodId && pm.Status == EntityStatus.Active)
                    .FirstOrDefaultAsync();

                if (targetPaymentMethod == null)
                    return ServiceResult.Failure("找不到指定的付款方式或該付款方式未啟用");

                // 先清除所有預設設定
                var allPaymentMethods = await context.PaymentMethods
                    .Where(pm => pm.IsDefault)
                    .ToListAsync();

                foreach (var pm in allPaymentMethods)
                {
                    pm.IsDefault = false;
                    pm.UpdatedAt = DateTime.Now;
                }

                // 設定新的預設付款方式
                targetPaymentMethod.IsDefault = true;
                targetPaymentMethod.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetDefaultAsync), GetType(), _logger, new { 
                    Method = nameof(SetDefaultAsync),
                    ServiceType = GetType().Name,
                    PaymentMethodId = paymentMethodId 
                });
                return ServiceResult.Failure("設定預設付款方式時發生錯誤");
            }
        }
    }
}