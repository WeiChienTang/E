using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨服務實作
    /// </summary>
    public class MaterialIssueService : GenericManagementService<MaterialIssue>, IMaterialIssueService
    {
        public MaterialIssueService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public MaterialIssueService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<MaterialIssue>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<MaterialIssue>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<MaterialIssue?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Product)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(mi => mi.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<MaterialIssue>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => (mi.Code != null && mi.Code.Contains(searchTerm)) ||
                                (mi.Employee != null && mi.Employee.Name != null && mi.Employee.Name.Contains(searchTerm)) ||
                                (mi.Department != null && mi.Department.Name != null && mi.Department.Name.Contains(searchTerm)) ||
                                (mi.Remarks != null && mi.Remarks.Contains(searchTerm)))
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(MaterialIssue entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證領貨單號 (Code)
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("領貨單號為必填欄位");
                }
                else
                {
                    // 檢查單號唯一性
                    var isDuplicate = await IsIssueNumberExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("領貨單號已存在");
                    }
                }

                // 驗證領貨日期
                if (entity.IssueDate == default)
                {
                    errors.Add("領貨日期為必填欄位");
                }

                // 驗證明細
                if (entity.Id == 0 && (entity.MaterialIssueDetails == null || !entity.MaterialIssueDetails.Any()))
                {
                    errors.Add("至少需要一筆領貨明細");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 業務邏輯方法

        public async Task<string> GenerateIssueNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var today = DateTime.Today;
                var prefix = $"MI{today:yyyyMMdd}";

                // 查詢今天最後一筆單號
                var lastIssue = await context.MaterialIssues
                    .Where(mi => mi.Code != null && mi.Code.StartsWith(prefix))
                    .OrderByDescending(mi => mi.Code)
                    .FirstOrDefaultAsync();

                int nextSequence = 1;
                if (lastIssue != null && !string.IsNullOrEmpty(lastIssue.Code))
                {
                    // 取得流水號部分並加1
                    var sequencePart = lastIssue.Code.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out int currentSequence))
                    {
                        nextSequence = currentSequence + 1;
                    }
                }

                // 格式: MI + yyyyMMdd + 4位流水號
                return $"{prefix}{nextSequence:D4}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateIssueNumberAsync), GetType(), _logger, new
                {
                    Method = nameof(GenerateIssueNumberAsync),
                    ServiceType = GetType().Name
                });
                // 發生錯誤時返回預設單號
                return $"MI{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        public async Task<bool> IsIssueNumberExistsAsync(string issueNumber, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.MaterialIssues.Where(mi => mi.Code == issueNumber);

                if (excludeId.HasValue)
                    query = query.Where(mi => mi.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsIssueNumberExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsIssueNumberExistsAsync),
                    ServiceType = GetType().Name,
                    IssueNumber = issueNumber,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<MaterialIssue?> GetWithDetailsAsync(int materialIssueId)
        {
            return await GetByIdAsync(materialIssueId);
        }

        public async Task<List<MaterialIssueDetail>> GetDetailsAsync(int materialIssueId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MaterialIssueId == materialIssueId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDetailsAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId
                });
                return new List<MaterialIssueDetail>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.IssueDate >= startDate && mi.IssueDate <= endDate)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.EmployeeId == employeeId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByEmployeeAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDepartmentAsync(int departmentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.DepartmentId == departmentId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDepartmentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDepartmentAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = departmentId
                });
                return new List<MaterialIssue>();
            }
        }

        #endregion
    }
}
