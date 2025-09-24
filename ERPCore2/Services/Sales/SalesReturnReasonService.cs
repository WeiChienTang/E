using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.Services.Sales
{
    /// <summary>
    /// 銷貨退貨原因服務實作
    /// </summary>
    public class SalesReturnReasonService : GenericManagementService<EntitySalesReturnReason>, ISalesReturnReasonService
    {
        public SalesReturnReasonService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public SalesReturnReasonService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<EntitySalesReturnReason>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<EntitySalesReturnReason>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnReasons
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<EntitySalesReturnReason>();
            }
        }

        public override async Task<List<EntitySalesReturnReason>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesReturnReasons.Where(r => !r.IsDeleted);

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
                return new List<EntitySalesReturnReason>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EntitySalesReturnReason entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("原因名稱為必填欄位");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("原因名稱已存在");
                
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

        public async Task<List<EntitySalesReturnReason>> GetActiveReasonsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnReasons
                    .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveReasonsAsync), GetType(), _logger, new { 
                    Method = nameof(GetActiveReasonsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<EntitySalesReturnReason>();
            }
        }
    }
}