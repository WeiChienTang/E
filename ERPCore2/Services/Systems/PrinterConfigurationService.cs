using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 印表機設定服務實作
    /// </summary>
    public class PrinterConfigurationService : GenericManagementService<PrinterConfiguration>, IPrinterConfigurationService
    {
        private readonly IPrinterTestService _printerTestService;

        public PrinterConfigurationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PrinterConfiguration>> logger,
            IPrinterTestService? printerTestService = null) : base(contextFactory, logger)
        {
            _printerTestService = printerTestService ?? new PrinterTestService(logger as ILogger<PrinterTestService>);
        }

        public PrinterConfigurationService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
            _printerTestService = new PrinterTestService();
        }

        public override async Task<List<PrinterConfiguration>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .AsQueryable()
                    .OrderByDescending(p => p.IsDefault)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PrinterConfiguration>();
            }
        }

        public override async Task<List<PrinterConfiguration>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .Where(p => (p.Name.Contains(searchTerm) ||
                                (p.IpAddress != null && p.IpAddress.Contains(searchTerm))))
                    .OrderByDescending(p => p.IsDefault)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PrinterConfiguration>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PrinterConfiguration entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證印表機名稱
                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("印表機名稱為必填欄位");
                else if (entity.Name.Length > 100)
                    errors.Add("印表機名稱不可超過100個字元");

                // 檢查名稱是否重複
                if (!string.IsNullOrWhiteSpace(entity.Name) && 
                    await IsNameExistsAsync(entity.Name, entity.Id == 0 ? null : entity.Id))
                    errors.Add("印表機名稱已存在");

                // 驗證IP位址格式
                if (!string.IsNullOrWhiteSpace(entity.IpAddress))
                {
                    var ipValidation = ValidateIpAddress(entity.IpAddress);
                    if (!ipValidation.IsSuccess)
                        errors.Add(ipValidation.ErrorMessage);

                    // 檢查IP位址是否重複
                    if (await IsIpAddressExistsAsync(entity.IpAddress, entity.Id == 0 ? null : entity.Id))
                        errors.Add("IP位址已存在");
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

        public async Task<PrinterConfiguration?> GetDefaultPrinterAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .Where(p => p.IsDefault && p.Status == EntityStatus.Active)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDefaultPrinterAsync), GetType(), _logger, new { 
                    Method = nameof(GetDefaultPrinterAsync),
                    ServiceType = GetType().Name 
                });
                return null;
            }
        }

        public async Task<PrinterConfiguration?> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .Where(p => p.Name == name)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByNameAsync), GetType(), _logger, new { 
                    Method = nameof(GetByNameAsync),
                    ServiceType = GetType().Name,
                    Name = name 
                });
                return null;
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PrinterConfigurations
                    .Where(p => p.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = name,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<PrinterConfiguration?> GetByIpAddressAsync(string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return null;

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .Where(p => p.IpAddress == ipAddress)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIpAddressAsync), GetType(), _logger, new { 
                    Method = nameof(GetByIpAddressAsync),
                    ServiceType = GetType().Name,
                    IpAddress = ipAddress 
                });
                return null;
            }
        }

        public async Task<bool> IsIpAddressExistsAsync(string ipAddress, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PrinterConfigurations
                    .Where(p => p.IpAddress == ipAddress);

                if (excludeId.HasValue)
                    query = query.Where(p => p.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsIpAddressExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsIpAddressExistsAsync),
                    ServiceType = GetType().Name,
                    IpAddress = ipAddress,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        public async Task<ServiceResult> SetAsDefaultAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查印表機是否存在且啟用
                var printer = await context.PrinterConfigurations
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (printer == null)
                    return ServiceResult.Failure("找不到指定的印表機設定");

                if (printer.Status != EntityStatus.Active)
                    return ServiceResult.Failure("無法將停用的印表機設為預設");

                // 取消所有其他印表機的預設狀態
                var otherPrinters = await context.PrinterConfigurations
                    .Where(p => p.Id != id && p.IsDefault)
                    .ToListAsync();

                foreach (var otherPrinter in otherPrinters)
                {
                    otherPrinter.IsDefault = false;
                    otherPrinter.UpdatedAt = DateTime.UtcNow;
                }

                // 設定為預設
                printer.IsDefault = true;
                printer.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetAsDefaultAsync), GetType(), _logger, new { 
                    Method = nameof(SetAsDefaultAsync),
                    ServiceType = GetType().Name,
                    PrinterId = id 
                });
                return ServiceResult.Failure("設定預設印表機時發生錯誤");
            }
        }

        public async Task<List<PrinterConfiguration>> GetActivePrintersAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PrinterConfigurations
                    .Where(p => p.Status == EntityStatus.Active)
                    .OrderByDescending(p => p.IsDefault)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActivePrintersAsync), GetType(), _logger, new { 
                    Method = nameof(GetActivePrintersAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PrinterConfiguration>();
            }
        }

        public async Task<List<PrinterConfiguration>> GetByConnectionTypeAsync(bool isNetworkPrinter)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                if (isNetworkPrinter)
                {
                    // 網路印表機：有IP位址
                    return await context.PrinterConfigurations
                        .Where(p => !string.IsNullOrEmpty(p.IpAddress))
                        .OrderByDescending(p => p.IsDefault)
                        .ThenBy(p => p.Name)
                        .ToListAsync();
                }
                else
                {
                    // 本機印表機：無IP位址
                    return await context.PrinterConfigurations
                        .Where(p => string.IsNullOrEmpty(p.IpAddress))
                        .OrderByDescending(p => p.IsDefault)
                        .ThenBy(p => p.Name)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByConnectionTypeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByConnectionTypeAsync),
                    ServiceType = GetType().Name,
                    IsNetworkPrinter = isNetworkPrinter 
                });
                return new List<PrinterConfiguration>();
            }
        }

        public ServiceResult ValidateIpAddress(string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return ServiceResult.Success();

                if (ipAddress.Length > 50)
                    return ServiceResult.Failure("IP位址不可超過50個字元");

                // 使用正規表達式驗證IPv4格式
                var ipv4Pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
                if (!Regex.IsMatch(ipAddress, ipv4Pattern))
                    return ServiceResult.Failure("IP位址格式不正確");

                // 使用IPAddress.TryParse進行進一步驗證
                if (!IPAddress.TryParse(ipAddress, out _))
                    return ServiceResult.Failure("IP位址格式不正確");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateIpAddress), GetType(), _logger, new { 
                    Method = nameof(ValidateIpAddress),
                    ServiceType = GetType().Name,
                    IpAddress = ipAddress 
                });
                return ServiceResult.Failure("IP位址驗證時發生錯誤");
            }
        }

        public async Task<ServiceResult> TestPrintAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                if (printerConfiguration == null)
                    return ServiceResult.Failure("印表機配置不能為空");

                // 驗證印表機配置
                var validationResult = await ValidateAsync(printerConfiguration);
                if (!validationResult.IsSuccess)
                    return ServiceResult.Failure($"印表機配置無效: {validationResult.ErrorMessage}");

                // 使用測試服務執行測試列印
                var testResult = await _printerTestService.TestPrintAsync(printerConfiguration);
                return testResult;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TestPrintAsync), GetType(), _logger, new { 
                    Method = nameof(TestPrintAsync),
                    ServiceType = GetType().Name,
                    PrinterId = printerConfiguration?.Id,
                    PrinterName = printerConfiguration?.Name 
                });
                return ServiceResult.Failure("測試列印時發生錯誤");
            }
        }
    }
}

