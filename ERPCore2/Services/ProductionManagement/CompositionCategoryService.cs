using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 合成表類型服務實作
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
                    .Where(cc => cc.Name.Contains(searchTerm))
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
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("名稱為必填欄位");
                
                // 檢查名稱是否重複
                if (!string.IsNullOrWhiteSpace(entity.Name))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var exists = await context.CompositionCategories
                        .Where(cc => cc.Name == entity.Name && cc.Id != entity.Id)
                        .AnyAsync();
                    
                    if (exists)
                        errors.Add("此名稱已存在");
                }
                
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
    }
}
