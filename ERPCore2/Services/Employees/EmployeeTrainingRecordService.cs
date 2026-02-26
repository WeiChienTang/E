using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工受訓紀錄服務實作
    /// </summary>
    public class EmployeeTrainingRecordService : GenericManagementService<EmployeeTrainingRecord>, IEmployeeTrainingRecordService
    {
        public EmployeeTrainingRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeeTrainingRecord>> logger) : base(contextFactory, logger)
        {
        }

        public EmployeeTrainingRecordService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<EmployeeTrainingRecord>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTrainingRecords
                    .Include(r => r.Employee)
                    .OrderByDescending(r => r.TrainingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<EmployeeTrainingRecord?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTrainingRecords
                    .Include(r => r.Employee)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<EmployeeTrainingRecord>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTrainingRecords
                    .Include(r => r.Employee)
                    .Where(r => r.CourseName.ToLower().Contains(lowerSearchTerm) ||
                                (r.TrainingOrganization != null && r.TrainingOrganization.ToLower().Contains(lowerSearchTerm)) ||
                                (r.Result != null && r.Result.ToLower().Contains(lowerSearchTerm)) ||
                                (r.Remarks != null && r.Remarks.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(r => r.TrainingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeTrainingRecord entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.EmployeeId <= 0)
                    errors.Add("請選擇員工");

                if (string.IsNullOrWhiteSpace(entity.CourseName))
                    errors.Add("請輸入課程名稱");

                if (entity.TrainingDate == default)
                    errors.Add("請輸入受訓日期");

                if (entity.CompletedDate.HasValue && entity.CompletedDate.Value < entity.TrainingDate)
                    errors.Add("完成日期不可早於受訓日期");

                if (entity.TrainingHours.HasValue && entity.TrainingHours.Value < 0)
                    errors.Add("訓練時數不可為負數");

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

        public async Task<List<EmployeeTrainingRecord>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeTrainingRecords
                    .Where(r => r.EmployeeId == employeeId)
                    .OrderByDescending(r => r.TrainingDate)
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
