using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
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
                    .Where(r => (r.ReportType.Contains(searchTerm) ||
                         r.ReportName.Contains(searchTerm) ||
                         (r.Code != null && r.Code.Contains(searchTerm)) ||
                         (r.Remarks != null && r.Remarks.Contains(searchTerm))))
                    .OrderBy(r => r.ReportType)
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
                if (string.IsNullOrWhiteSpace(entity.ReportType))
                    return ServiceResult.Failure("報表類型為必填欄位");

                if (string.IsNullOrWhiteSpace(entity.ReportName))
                    return ServiceResult.Failure("報表名稱為必填欄位");

                // 檢查報表類型是否重複
                if (await IsReportTypeExistsAsync(entity.ReportType, entity.Id))
                    return ServiceResult.Failure("報表類型已存在");

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
                    ReportType = entity.ReportType
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
                    .OrderBy(r => r.ReportType)
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

        public async Task<ReportPrintConfiguration?> GetByReportTypeAsync(string reportType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportType))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.ReportType == reportType);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByReportTypeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByReportTypeAsync),
                    ServiceType = GetType().Name,
                    ReportType = reportType
                });
                return null;
            }
        }

        public async Task<bool> IsReportTypeExistsAsync(string reportType, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportType))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ReportPrintConfigurations
                    .Where(r => r.ReportType == reportType);

                if (excludeId.HasValue)
                    query = query.Where(r => r.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsReportTypeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsReportTypeExistsAsync),
                    ServiceType = GetType().Name,
                    ReportType = reportType,
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
                    .OrderBy(r => r.ReportType)
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

        public async Task<List<string>> GetReportTypesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .AsQueryable()
                    .Select(r => r.ReportType)
                    .Distinct()
                    .OrderBy(rt => rt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReportTypesAsync), GetType(), _logger, new
                {
                    Method = nameof(GetReportTypesAsync),
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
                    .OrderBy(r => r.ReportType)
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
                    .OrderBy(r => r.ReportType)
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

        public async Task<ReportPrintConfiguration?> GetCompleteConfigurationAsync(string reportType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportType))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ReportPrintConfigurations
                    .Include(r => r.PrinterConfiguration)
                    .Include(r => r.PaperSetting)
                    .FirstOrDefaultAsync(r => r.ReportType == reportType && r.Status == EntityStatus.Active);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCompleteConfigurationAsync), GetType(), _logger, new
                {
                    Method = nameof(GetCompleteConfigurationAsync),
                    ServiceType = GetType().Name,
                    ReportType = reportType
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

        public async Task<bool> CopyConfigurationAsync(string sourceReportType, string targetReportType, string targetReportName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourceReportType) || 
                    string.IsNullOrWhiteSpace(targetReportType) || 
                    string.IsNullOrWhiteSpace(targetReportName))
                    return false;

                // 檢查目標報表類型是否已存在
                if (await IsReportTypeExistsAsync(targetReportType))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var sourceConfig = await context.ReportPrintConfigurations
                    .FirstOrDefaultAsync(r => r.ReportType == sourceReportType);

                if (sourceConfig == null)
                    return false;

                var newConfig = new ReportPrintConfiguration
                {
                    ReportType = targetReportType,
                    ReportName = targetReportName,
                    PrinterConfigurationId = sourceConfig.PrinterConfigurationId,
                    PaperSettingId = sourceConfig.PaperSettingId,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Remarks = $"複製自 {sourceReportType}"
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
                    SourceReportType = sourceReportType,
                    TargetReportType = targetReportType,
                    TargetReportName = targetReportName
                });
                return false;
            }
        }
    }
}

