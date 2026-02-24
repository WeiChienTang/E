using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class EmployeePreferenceService : GenericManagementService<EmployeePreference>, IEmployeePreferenceService
    {
        public EmployeePreferenceService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public EmployeePreferenceService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<EmployeePreference>> logger) : base(contextFactory, logger)
        {
        }

        /// <inheritdoc />
        public async Task<EmployeePreference> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var preference = await context.EmployeePreferences
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

                // 不存在時回傳預設值（不寫入 DB）
                return preference ?? new EmployeePreference { EmployeeId = employeeId };
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger,
                    new { EmployeeId = employeeId });
                return new EmployeePreference { EmployeeId = employeeId };
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> SavePreferenceAsync(int employeeId, EmployeePreference preference)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existing = await context.EmployeePreferences
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

                if (existing == null)
                {
                    // 新增
                    preference.EmployeeId = employeeId;
                    preference.CreatedAt = DateTime.Now;
                    context.EmployeePreferences.Add(preference);
                }
                else
                {
                    // 更新只更新語言欄位（未來新增其他設定時在此擴充）
                    existing.Language = preference.Language;
                    existing.UpdatedAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SavePreferenceAsync), GetType(), _logger,
                    new { EmployeeId = employeeId });
                return ServiceResult.Failure("儲存個人化設定時發生錯誤");
            }
        }

        public override async Task<List<EmployeePreference>> SearchAsync(string searchTerm)
        {
            // 個人化設定不需要搜尋功能
            return await Task.FromResult(new List<EmployeePreference>());
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeePreference entity)
        {
            // 個人化設定無複雜驗證規則
            return await Task.FromResult(ServiceResult.Success());
        }
    }
}
