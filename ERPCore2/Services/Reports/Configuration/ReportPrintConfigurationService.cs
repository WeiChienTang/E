using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Reports.Configuration
{
    /// <summary>
    /// 報表列印配置服務實作
    /// </summary>
    public class ReportPrintConfigurationService : GenericManagementService<ReportPrintConfiguration>, IReportPrintConfigurationService
    {
        public ReportPrintConfigurationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ReportPrintConfiguration>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<ReportPrintConfiguration>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .Where(r => (r.ReportName.Contains(searchTerm) ||
                         (r.Code != null && r.Code.Contains(searchTerm)) ||
                         (r.Remarks != null && r.Remarks.Contains(searchTerm))))
                    .OrderBy(r => r.ReportName)
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
                return new List<ReportPrintConfiguration>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ReportPrintConfiguration entity)
        {
            try
            {
                // 檢查必填欄位
                if (string.IsNullOrWhiteSpace(entity.ReportName))
                    return ServiceResult.Failure("報表名稱為必填欄位");

                // 檢查報表名稱是否重複
                if (await IsReportNameExistsAsync(entity.ReportName, entity.Id))
                    return ServiceResult.Failure("報表名稱已存在");

                // 檢查印表機設定是否存在（如果有提供的話）
                using var context = await _contextFactory.CreateDbContextAsync();
                if (entity.PrinterConfigurationId.HasValue)
                {
                    var printerExists = await context.PrinterConfigurations
                        .AnyAsync(p => p.Id == entity.PrinterConfigurationId.Value);
                    if (!printerExists)
                        return ServiceResult.Failure("指定的印表機設定不存在");
                }

                // 檢查紙張設定是否存在（如果有提供的話）
                if (entity.PaperSettingId.HasValue)
                {
                    var paperExists = await context.PaperSettings
                        .AnyAsync(p => p.Id == entity.PaperSettingId.Value);
                    if (!paperExists)
                        return ServiceResult.Failure("指定的紙張設定不存在");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    ReportName = entity.ReportName
                });
                return ServiceResult.Failure("驗證過程中發生錯誤");
            }
        }

        public override async Task<List<ReportPrintConfiguration>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .AsQueryable()
                    .OrderBy(r => r.ReportName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<ReportPrintConfiguration>();
            }
        }

        public override async Task<ReportPrintConfiguration?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.Id == id);
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

        public async Task<ReportPrintConfiguration?> GetByReportNameAsync(string reportName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.ReportName == reportName);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByReportNameAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByReportNameAsync),
                    ServiceType = GetType().Name,
                    ReportName = reportName
                });
                return null;
            }
        }

        public async Task<bool> IsReportNameExistsAsync(string reportName, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ReportPrintConfigurations
                    .Where(r => r.ReportName == reportName);

                if (excludeId.HasValue)
                    query = query.Where(r => r.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsReportNameExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsReportNameExistsAsync),
                    ServiceType = GetType().Name,
                    ReportName = reportName,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<ReportPrintConfiguration>> GetActiveConfigurationsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .Where(r => r.Status == EntityStatus.Active)
                    .OrderBy(r => r.ReportName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveConfigurationsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetActiveConfigurationsAsync),
                    ServiceType = GetType().Name
                });
                return new List<ReportPrintConfiguration>();
            }
        }

        public async Task<List<string>> GetReportNamesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .AsQueryable()
                    .Select(r => r.ReportName)
                    .Distinct()
                    .OrderBy(rn => rn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReportNamesAsync), GetType(), _logger, new
                {
                    Method = nameof(GetReportNamesAsync),
                    ServiceType = GetType().Name
                });
                return new List<string>();
            }
        }

        public async Task<List<ReportPrintConfiguration>> GetByPrinterConfigurationIdAsync(int printerConfigurationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .Where(r => r.PrinterConfigurationId == printerConfigurationId)
                    .OrderBy(r => r.ReportName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPrinterConfigurationIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPrinterConfigurationIdAsync),
                    ServiceType = GetType().Name,
                    PrinterConfigurationId = printerConfigurationId
                });
                return new List<ReportPrintConfiguration>();
            }
        }

        public async Task<List<ReportPrintConfiguration>> GetByPaperSettingIdAsync(int paperSettingId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .Where(r => r.PaperSettingId == paperSettingId)
                    .OrderBy(r => r.ReportName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPaperSettingIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByPaperSettingIdAsync),
                    ServiceType = GetType().Name,
                    PaperSettingId = paperSettingId
                });
                return new List<ReportPrintConfiguration>();
            }
        }

        public async Task<ReportPrintConfiguration?> GetCompleteConfigurationAsync(string reportName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.ReportName == reportName && r.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompleteConfigurationAsync), GetType(), _logger, new
                {
                    Method = nameof(GetCompleteConfigurationAsync),
                    ServiceType = GetType().Name,
                    ReportName = reportName
                });
                return null;
            }
        }

        public async Task<bool> BatchUpdateAsync(List<ReportPrintConfiguration> configurations)
        {
            try
            {
                if (configurations == null || !configurations.Any())
                    return true;

                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var config in configurations)
                    {
                        var existing = await context.ReportPrintConfigurations
                            .FirstOrDefaultAsync(r => r.Id == config.Id);

                        if (existing != null)
                        {
                            existing.ReportName = config.ReportName;
                            existing.PrinterConfigurationId = config.PrinterConfigurationId;
                            existing.PaperSettingId = config.PaperSettingId;
                            existing.Status = config.Status;
                            existing.Remarks = config.Remarks;
                            existing.UpdatedAt = DateTime.UtcNow;
                            existing.UpdatedBy = "System";
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdateAsync), GetType(), _logger, new
                {
                    Method = nameof(BatchUpdateAsync),
                    ServiceType = GetType().Name,
                    ConfigurationCount = configurations?.Count ?? 0
                });
                return false;
            }
        }

        public async Task<bool> CopyConfigurationAsync(string sourceReportName, string targetReportName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourceReportName) || 
                    string.IsNullOrWhiteSpace(targetReportName))
                    return false;

                // 檢查目標報表名稱是否已存在
                if (await IsReportNameExistsAsync(targetReportName))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var sourceConfig = await context.ReportPrintConfigurations
                    .FirstOrDefaultAsync(r => r.ReportName == sourceReportName);

                if (sourceConfig == null)
                    return false;

                var newConfig = new ReportPrintConfiguration
                {
                    ReportName = targetReportName,
                    PrinterConfigurationId = sourceConfig.PrinterConfigurationId,
                    PaperSettingId = sourceConfig.PaperSettingId,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Remarks = $"複製自 {sourceReportName}"
                };

                context.ReportPrintConfigurations.Add(newConfig);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyConfigurationAsync), GetType(), _logger, new
                {
                    Method = nameof(CopyConfigurationAsync),
                    ServiceType = GetType().Name,
                    SourceReportName = sourceReportName,
                    TargetReportName = targetReportName
                });
                return false;
            }
        }
    }
}
