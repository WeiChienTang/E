using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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
                if (string.IsNullOrWhiteSpace(entity.ReportId))
                    return ServiceResult.Failure("報表識別碼為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.ReportName))
                    return ServiceResult.Failure("報表名稱為必填欄位");

                // 檢查報表識別碼是否重複
                using var context = await _contextFactory.CreateDbContextAsync();
                var reportIdExists = await context.ReportPrintConfigurations
                    .AnyAsync(r => r.ReportId == entity.ReportId && r.Id != entity.Id);
                if (reportIdExists)
                    return ServiceResult.Failure("報表識別碼已存在，每個報表只能有一個列印配置");

                // 檢查印表機設定是否存在（如果有提供的話）
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

        public async Task<ReportPrintConfiguration?> GetByReportIdAsync(string reportId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportId))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.ReportId == reportId && r.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByReportIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByReportIdAsync),
                    ServiceType = GetType().Name,
                    ReportId = reportId
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

        public async Task<List<ReportPrintConfiguration>> GetReportsWithoutPrinterOrPaperSettingAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .Where(r => r.Status == EntityStatus.Active &&
                               (!r.PrinterConfigurationId.HasValue || !r.PaperSettingId.HasValue))
                    .OrderBy(r => r.ReportName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReportsWithoutPrinterOrPaperSettingAsync), GetType(), _logger, new
                {
                    Method = nameof(GetReportsWithoutPrinterOrPaperSettingAsync),
                    ServiceType = GetType().Name
                });
                return new List<ReportPrintConfiguration>();
            }
        }

        public async Task<ServiceResult> BatchUpdatePrinterAndPaperSettingsAsync(List<(int configId, int? printerConfigurationId, int? paperSettingId)> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                    return ServiceResult.Failure("更新資料不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                var configIds = updates.Select(u => u.configId).ToList();
                var configs = await context.ReportPrintConfigurations
                    .Where(r => configIds.Contains(r.Id))
                    .ToListAsync();

                if (configs.Count != updates.Count)
                    return ServiceResult.Failure("部分報表配置不存在");

                // 驗證印表機和紙張設定是否存在
                var printerIds = updates.Where(u => u.printerConfigurationId.HasValue)
                    .Select(u => u.printerConfigurationId!.Value).Distinct().ToList();
                var paperIds = updates.Where(u => u.paperSettingId.HasValue)
                    .Select(u => u.paperSettingId!.Value).Distinct().ToList();

                if (printerIds.Any())
                {
                    var existingPrinters = await context.PrinterConfigurations
                        .Where(p => printerIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();
                    if (existingPrinters.Count != printerIds.Count)
                        return ServiceResult.Failure("部分印表機設定不存在");
                }

                if (paperIds.Any())
                {
                    var existingPapers = await context.PaperSettings
                        .Where(p => paperIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();
                    if (existingPapers.Count != paperIds.Count)
                        return ServiceResult.Failure("部分紙張設定不存在");
                }

                // 執行更新
                foreach (var update in updates)
                {
                    var config = configs.First(c => c.Id == update.configId);
                    if (update.printerConfigurationId.HasValue)
                        config.PrinterConfigurationId = update.printerConfigurationId.Value;
                    if (update.paperSettingId.HasValue)
                        config.PaperSettingId = update.paperSettingId.Value;
                    config.UpdatedAt = DateTime.UtcNow;
                    config.UpdatedBy = "System";
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(BatchUpdatePrinterAndPaperSettingsAsync), GetType(), _logger, new
                {
                    Method = nameof(BatchUpdatePrinterAndPaperSettingsAsync),
                    ServiceType = GetType().Name,
                    UpdateCount = updates?.Count ?? 0
                });
                return ServiceResult.Failure($"批次更新失敗：{ex.Message}");
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var statistics = new Dictionary<string, object>();

                // 總報表數量
                var totalCount = await context.ReportPrintConfigurations
                    .Where(r => r.Status == EntityStatus.Active)
                    .CountAsync();
                statistics["TotalCount"] = totalCount;

                // 未設定印表機或紙張的報表數量
                var noSettingCount = await context.ReportPrintConfigurations
                    .Where(r => r.Status == EntityStatus.Active &&
                               (!r.PrinterConfigurationId.HasValue || !r.PaperSettingId.HasValue))
                    .CountAsync();
                statistics["NoSettingCount"] = noSettingCount;

                // 已完整設定的報表數量
                var configuredCount = await context.ReportPrintConfigurations
                    .Where(r => r.Status == EntityStatus.Active &&
                               r.PrinterConfigurationId.HasValue && r.PaperSettingId.HasValue)
                    .CountAsync();
                statistics["ConfiguredCount"] = configuredCount;

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name
                });
                return new Dictionary<string, object>();
            }
        }
    }
}
