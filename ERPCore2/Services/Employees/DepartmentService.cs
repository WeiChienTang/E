using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 部門服務實作
    /// </summary>
    public class DepartmentService : GenericManagementService<Department>, IDepartmentService
    {
        public DepartmentService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Department>> logger) : base(contextFactory, logger)
        {
        }

        public DepartmentService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<Department>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Include(d => d.Manager)
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Department>();
            }
        }

        public override async Task<Department?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Include(d => d.Manager)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return null;
            }
        }

        public override async Task<List<Department>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Where(d => !d.IsDeleted &&
                        ((d.Name != null && d.Name.Contains(searchTerm)) ||
                         (d.Code != null && d.Code.Contains(searchTerm))))
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<Department>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Department entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("部門代碼不能為空");
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("部門名稱不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.Code) && 
                    await IsDepartmentCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("部門代碼已存在");
                
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsDepartmentNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("部門名稱已存在");
                
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

        public async Task<bool> IsDepartmentCodeExistsAsync(string Code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Departments.Where(d => d.Code == Code && !d.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDepartmentCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsDepartmentCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = Code,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<bool> IsDepartmentNameExistsAsync(string Name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Departments.Where(d => d.Name == Name && !d.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDepartmentNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsDepartmentNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = Name,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<bool> CanDeleteDepartmentAsync(int departmentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否有員工
                var hasEmployees = await context.Employees
                    .AnyAsync(e => e.DepartmentId == departmentId && !e.IsDeleted);
                
                return !hasEmployees;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteDepartmentAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteDepartmentAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = departmentId 
                });
                return false;
            }
        }
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作部門特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Department entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckDepartmentDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("部門"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = entity.Id 
                });
                return ServiceResult.Failure("檢查部門刪除條件時發生錯誤");
            }
        }

        public async Task<List<Employee>> GetAvailableManagersAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Employees
                    .Where(e => !e.IsDeleted && e.IsSystemUser)
                    .OrderBy(e => e.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableManagersAsync), GetType(), _logger, new { 
                    Method = nameof(GetAvailableManagersAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Employee>();
            }
        }
    }
}
