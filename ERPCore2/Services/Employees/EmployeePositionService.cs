using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
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
                    .AsQueryable()
                    .OrderBy(ep => ep.Name)
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
                    .Where(ep => (ep.Name.ToUpper().Contains(normalizedSearch) ||
                                 (ep.Code != null && ep.Code.ToUpper().Contains(normalizedSearch))))
                    .OrderBy(ep => ep.Name)
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
                
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("職位代碼不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("職位名稱已存在");
                
                if (!string.IsNullOrWhiteSpace(entity.Code) && 
                    await IsEmployeePositionCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
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

        public async Task<bool> IsEmployeePositionCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EmployeePositions.Where(ep => ep.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(ep => ep.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEmployeePositionCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsEmployeePositionCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.EmployeePositions.Where(ep => ep.Name == name);
                if (excludeId.HasValue)
                    query = query.Where(ep => ep.Id != excludeId.Value);
                
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
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作職位特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(EmployeePosition entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckEmployeePositionDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("職位"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    PositionId = entity.Id 
                });
                return ServiceResult.Failure("檢查職位刪除條件時發生錯誤");
            }
        }
    }
}

