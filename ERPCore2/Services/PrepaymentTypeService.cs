using ERPCore2.Data;
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項類型服務實作
    /// </summary>
    public class PrepaymentTypeService : GenericManagementService<PrepaymentType>, IPrepaymentTypeService
    {
        public PrepaymentTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PrepaymentTypeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PrepaymentType>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<PrepaymentType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrepaymentTypes
                    .OrderBy(pt => pt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PrepaymentType>();
            }
        }

        public override async Task<List<PrepaymentType>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.PrepaymentTypes
                    .Where(pt => pt.Name.Contains(searchTerm))
                    .OrderBy(pt => pt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PrepaymentType>();
            }
        }

        public async Task<List<PrepaymentType>> SearchByNameAsync(string keyword)
        {
            return await SearchAsync(keyword);
        }

        public override async Task<ServiceResult> ValidateAsync(PrepaymentType entity)
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
                    var exists = await context.PrepaymentTypes
                        .Where(pt => pt.Name == entity.Name && pt.Id != entity.Id)
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
