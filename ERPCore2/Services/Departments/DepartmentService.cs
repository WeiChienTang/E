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
                    .Include(d => d.ParentDepartment)
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.SortOrder)
                    .ThenBy(d => d.Name)
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
                    .Include(d => d.ParentDepartment)
                    .Include(d => d.ChildDepartments.Where(c => !c.IsDeleted))
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
                    .Include(d => d.ParentDepartment)
                    .Where(d => !d.IsDeleted &&
                        (d.Name.Contains(searchTerm) ||
                         d.DepartmentCode.Contains(searchTerm) ||
                         (d.Description != null && d.Description.Contains(searchTerm))))
                    .OrderBy(d => d.SortOrder)
                    .ThenBy(d => d.Name)
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
                
                if (string.IsNullOrWhiteSpace(entity.DepartmentCode))
                    errors.Add("部門代碼不能為空");
                
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("部門名稱不能為空");
                
                if (!string.IsNullOrWhiteSpace(entity.DepartmentCode) && 
                    await IsDepartmentCodeExistsAsync(entity.DepartmentCode, entity.Id == 0 ? null : entity.Id))
                    errors.Add("部門代碼已存在");
                
                // 檢查是否會造成循環參考
                if (entity.ParentDepartmentId.HasValue && entity.Id > 0)
                {
                    if (await WouldCreateCircularReferenceAsync(entity.Id, entity.ParentDepartmentId.Value))
                        errors.Add("不能選擇自己或下級部門作為上級部門");
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

        public async Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Departments.Where(d => d.DepartmentCode == departmentCode && !d.IsDeleted);
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsDepartmentCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsDepartmentCodeExistsAsync),
                    ServiceType = GetType().Name,
                    DepartmentCode = departmentCode,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<List<Department>> GetTopLevelDepartmentsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Where(d => !d.IsDeleted && d.ParentDepartmentId == null)
                    .OrderBy(d => d.SortOrder)
                    .ThenBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTopLevelDepartmentsAsync), GetType(), _logger, new { 
                    Method = nameof(GetTopLevelDepartmentsAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Department>();
            }
        }

        public async Task<List<Department>> GetChildDepartmentsAsync(int parentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Where(d => !d.IsDeleted && d.ParentDepartmentId == parentId)
                    .OrderBy(d => d.SortOrder)
                    .ThenBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetChildDepartmentsAsync), GetType(), _logger, new { 
                    Method = nameof(GetChildDepartmentsAsync),
                    ServiceType = GetType().Name,
                    ParentId = parentId 
                });
                return new List<Department>();
            }
        }

        public async Task<List<Department>> GetDepartmentTreeAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Departments
                    .Include(d => d.ChildDepartments.Where(c => !c.IsDeleted))
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.SortOrder)
                    .ThenBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDepartmentTreeAsync), GetType(), _logger, new { 
                    Method = nameof(GetDepartmentTreeAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Department>();
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
                
                // 檢查是否有下級部門
                var hasChildDepartments = await context.Departments
                    .AnyAsync(d => d.ParentDepartmentId == departmentId && !d.IsDeleted);
                
                return !hasEmployees && !hasChildDepartments;
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

        private async Task<bool> WouldCreateCircularReferenceAsync(int departmentId, int parentId)
        {
            try
            {
                if (departmentId == parentId)
                    return true;

                using var context = await _contextFactory.CreateDbContextAsync();
                var currentParent = await context.Departments
                    .FirstOrDefaultAsync(d => d.Id == parentId && !d.IsDeleted);

                while (currentParent?.ParentDepartmentId != null)
                {
                    if (currentParent.ParentDepartmentId == departmentId)
                        return true;

                    currentParent = await context.Departments
                        .FirstOrDefaultAsync(d => d.Id == currentParent.ParentDepartmentId && !d.IsDeleted);
                }

                return false;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(WouldCreateCircularReferenceAsync), GetType(), _logger, new { 
                    Method = nameof(WouldCreateCircularReferenceAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = departmentId,
                    ParentId = parentId 
                });
                return true; // 安全起見，如果檢查失敗就假設會造成循環參考
            }
        }
    }
}
