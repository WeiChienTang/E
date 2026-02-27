using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// 使用別名來避免命名衝突
using EntityPurchaseReturnReason = ERPCore2.Data.Entities.PurchaseReturnReason;

namespace ERPCore2.Services
{
    /// <summary>
    /// 進貨退出原因服務實作
    /// </summary>
    public class PurchaseReturnReasonService : GenericManagementService<EntityPurchaseReturnReason>, IPurchaseReturnReasonService
    {
        public PurchaseReturnReasonService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseReturnReasonService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EntityPurchaseReturnReason>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<EntityPurchaseReturnReason>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnReasons
                    .AsQueryable()
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<EntityPurchaseReturnReason>();
            }
        }

        public override async Task<List<EntityPurchaseReturnReason>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturnReasons.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r => r.Name.Contains(searchTerm));
                }

                return await query
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<EntityPurchaseReturnReason>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EntityPurchaseReturnReason entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("原因名稱為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("原因編號為必填欄位");

                if (!string.IsNullOrWhiteSpace(entity.Name) &&
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("原因名稱已存在");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsPurchaseReturnReasonCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("原因編號已存在");

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

        public async Task<List<EntityPurchaseReturnReason>> GetActiveReasonsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseReturnReasons
                    .Where(r => r.Status == EntityStatus.Active)
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveReasonsAsync), GetType(), _logger, new {
                    Method = nameof(GetActiveReasonsAsync),
                    ServiceType = GetType().Name
                });
                return new List<EntityPurchaseReturnReason>();
            }
        }

        public async Task<bool> IsPurchaseReturnReasonCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturnReasons
                    .Where(r => r.Code == code);

                if (excludeId.HasValue)
                {
                    query = query.Where(r => r.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseReturnReasonCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsPurchaseReturnReasonCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }
    }
}
