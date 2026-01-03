using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 物料清單類型服務實作
    /// </summary>
    public class CompositionCategoryService : GenericManagementService<CompositionCategory>, ICompositionCategoryService
    {
        public CompositionCategoryService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public CompositionCategoryService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<CompositionCategory>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<CompositionCategory>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CompositionCategories
                    .OrderBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<CompositionCategory>();
            }
        }

        public override async Task<List<CompositionCategory>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.CompositionCategories
                    .Where(cc => ((cc.Name != null && cc.Name.Contains(searchTerm)) ||
                         (cc.Code != null && cc.Code.Contains(searchTerm))))
                    .OrderBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<CompositionCategory>();
            }
        }

        public async Task<List<CompositionCategory>> SearchByNameAsync(string keyword)
        {
            return await SearchAsync(keyword);
        }

        public override async Task<ServiceResult> ValidateAsync(CompositionCategory entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("物料清單類型編號不能為空");
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("名稱為必填欄位");
                
                if (!string.IsNullOrWhiteSpace(entity.Code) && 
                    await IsCompositionCategoryCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("物料清單類型編號已存在");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsCompositionCategoryNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("此名稱已存在");
                
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

        public async Task<bool> IsCompositionCategoryCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.CompositionCategories.Where(cc => cc.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(cc => cc.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCompositionCategoryCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCompositionCategoryCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<bool> IsCompositionCategoryNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.CompositionCategories.Where(cc => cc.Name == name);
                if (excludeId.HasValue)
                    query = query.Where(cc => cc.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCompositionCategoryNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCompositionCategoryNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = name,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }
    }
}
