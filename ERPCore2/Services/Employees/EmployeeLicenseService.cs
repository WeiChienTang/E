using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工證照服務實作
    /// </summary>
    public class EmployeeLicenseService : GenericManagementService<EmployeeLicense>, IEmployeeLicenseService
    {
        public EmployeeLicenseService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeeLicense>> logger) : base(contextFactory, logger)
        {
        }

        public EmployeeLicenseService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<EmployeeLicense>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeLicenses
                    .Include(l => l.Employee)
                    .OrderByDescending(l => l.IssuedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<EmployeeLicense?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeLicenses
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<EmployeeLicense>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeLicenses
                    .Include(l => l.Employee)
                    .Where(l => l.LicenseName.ToLower().Contains(lowerSearchTerm) ||
                                (l.LicenseNumber != null && l.LicenseNumber.ToLower().Contains(lowerSearchTerm)) ||
                                (l.IssuingAuthority != null && l.IssuingAuthority.ToLower().Contains(lowerSearchTerm)) ||
                                (l.Remarks != null && l.Remarks.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(l => l.IssuedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeLicense entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EmployeeId <= 0)
                    errors.Add("請選擇員工");

                if (string.IsNullOrWhiteSpace(entity.LicenseName))
                    errors.Add("請輸入證照名稱");

                if (entity.IssuedDate == default)
                    errors.Add("請輸入取得日期");

                if (entity.ExpiryDate.HasValue && entity.ExpiryDate.Value <= entity.IssuedDate)
                    errors.Add("到期日必須晚於取得日期");

                if (entity.AlertDays < 0)
                    errors.Add("提醒天數不可為負數");

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

        public async Task<List<EmployeeLicense>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeLicenses
                    .Where(l => l.EmployeeId == employeeId)
                    .OrderByDescending(l => l.IssuedDate)
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
