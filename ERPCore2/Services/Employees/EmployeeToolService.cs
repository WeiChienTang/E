using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工工具配給紀錄服務實作
    /// </summary>
    public class EmployeeToolService : GenericManagementService<EmployeeTool>, IEmployeeToolService
    {
        public EmployeeToolService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeeTool>> logger) : base(contextFactory, logger)
        {
        }

        public EmployeeToolService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<EmployeeTool>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTools
                    .Include(t => t.Employee)
                    .OrderByDescending(t => t.AssignedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<EmployeeTool?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTools
                    .Include(t => t.Employee)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<EmployeeTool>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTools
                    .Include(t => t.Employee)
                    .Where(t => t.ToolName.ToLower().Contains(lowerSearchTerm) ||
                                (t.ToolCode != null && t.ToolCode.ToLower().Contains(lowerSearchTerm)) ||
                                (t.Remarks != null && t.Remarks.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(t => t.AssignedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeTool entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EmployeeId <= 0)
                    errors.Add("請選擇員工");

                if (string.IsNullOrWhiteSpace(entity.ToolName))
                    errors.Add("請輸入工具名稱");

                if (entity.AssignedDate == default)
                    errors.Add("請輸入配給日期");

                if (entity.ReturnedDate.HasValue && entity.ReturnedDate < entity.AssignedDate)
                    errors.Add("歸還日期不可早於配給日期");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<List<EmployeeTool>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTools
                    .Where(t => t.EmployeeId == employeeId)
                    .OrderByDescending(t => t.AssignedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new { EmployeeId = employeeId });
                throw;
            }
        }
    }
}
