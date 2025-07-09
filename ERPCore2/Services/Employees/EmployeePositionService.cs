using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Employees
{
    public class EmployeePositionService : GenericManagementService<EmployeePosition>, IEmployeePositionService
    {
        public EmployeePositionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public EmployeePositionService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<EmployeePosition>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<EmployeePosition>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeePositions
                    .Where(ep => !ep.IsDeleted)
                    .OrderBy(ep => ep.SortOrder)
                    .ThenBy(ep => ep.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<EmployeePosition>();
            }
        }

        public override async Task<List<EmployeePosition>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var normalizedSearch = searchTerm.Trim().ToUpper();
                
                return await context.EmployeePositions
                    .Where(ep => !ep.IsDeleted && 
                                (ep.Name.ToUpper().Contains(normalizedSearch) ||
                                 (ep.Code != null && ep.Code.ToUpper().Contains(normalizedSearch)) ||
                                 (ep.Description != null && ep.Description.ToUpper().Contains(normalizedSearch))))
                    .OrderBy(ep => ep.SortOrder)
                    .ThenBy(ep => ep.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<EmployeePosition>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeePosition entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("職位名稱不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("職位名稱已存在");
                
                if (!string.IsNullOrWhiteSpace(entity.Code) && 
                    await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("職位代碼已存在");
                
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

        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EmployeePositions.Where(ep => ep.Code == code && !ep.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(ep => ep.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<List<EmployeePosition>> GetByLevelAsync(int level)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeePositions
                    .Where(ep => !ep.IsDeleted && ep.Level == level)
                    .OrderBy(ep => ep.SortOrder)
                    .ThenBy(ep => ep.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByLevelAsync), GetType(), _logger, new { 
                    Method = nameof(GetByLevelAsync),
                    ServiceType = GetType().Name,
                    Level = level 
                });
                return new List<EmployeePosition>();
            }
        }

        public async Task<List<EmployeePosition>> GetOrderedByLevelAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeePositions
                    .Where(ep => !ep.IsDeleted)
                    .OrderByDescending(ep => ep.Level ?? 0)
                    .ThenBy(ep => ep.SortOrder)
                    .ThenBy(ep => ep.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOrderedByLevelAsync), GetType(), _logger, new { 
                    Method = nameof(GetOrderedByLevelAsync),
                    ServiceType = GetType().Name 
                });
                return new List<EmployeePosition>();
            }
        }
    }
}
