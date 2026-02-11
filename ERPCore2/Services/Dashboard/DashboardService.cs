using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Navigation;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Navigation;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 儀表板服務 - 管理首頁小工具配置
    /// 使用 NavigationConfig 作為小工具資料來源
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDbContextFactory<AppDbContext> contextFactory,
            IPermissionService permissionService,
            ILogger<DashboardService> logger)
        {
            _contextFactory = contextFactory;
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<NavigationItem>> GetAvailableWidgetsAsync(int employeeId)
        {
            try
            {
                // 從 NavigationConfig 取得所有可作為捷徑的項目
                var allWidgets = NavigationConfig.GetDashboardWidgetItems();

                // 取得員工的所有權限代碼
                var permissionResult = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
                var permissionCodes = permissionResult.IsSuccess ? permissionResult.Data ?? new List<string>() : new List<string>();

                // 過濾有權限的項目
                var availableWidgets = allWidgets.Where(item =>
                    string.IsNullOrEmpty(item.RequiredPermission) ||
                    permissionCodes.Contains(item.RequiredPermission)
                ).ToList();

                return availableWidgets;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableWidgetsAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<NavigationItem>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<DashboardConfigWithNavItem>> GetEmployeeDashboardAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 查詢員工的儀表板配置
                var configs = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId && c.IsVisible)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();

                // 如果沒有任何配置，檢查是否為新用戶
                if (!configs.Any())
                {
                    // 查詢員工是否已初始化過儀表板
                    var employee = await context.Employees.FindAsync(employeeId);
                    if (employee != null && !employee.HasInitializedDashboard)
                    {
                        // 新用戶，自動套用預設配置
                        return await InitializeDefaultDashboardAsync(employeeId);
                    }
                    // 已初始化過但配置為空，回傳空清單
                    return new List<DashboardConfigWithNavItem>();
                }

                // 取得員工權限
                var permissionResult = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
                var permissionCodes = permissionResult.IsSuccess ? permissionResult.Data ?? new List<string>() : new List<string>();

                // 將配置與導航項目對應
                var result = new List<DashboardConfigWithNavItem>();
                foreach (var config in configs)
                {
                    var navItem = NavigationConfig.GetNavigationItemByKey(config.NavigationItemKey);
                    if (navItem == null) continue; // 導航項目已不存在，跳過

                    // 檢查權限
                    if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                        !permissionCodes.Contains(navItem.RequiredPermission))
                        continue;

                    result.Add(new DashboardConfigWithNavItem
                    {
                        Config = config,
                        NavigationItem = navItem
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEmployeeDashboardAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<DashboardConfigWithNavItem>();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> AddWidgetAsync(int employeeId, string navigationItemKey)
        {
            try
            {
                // 檢查導航項目是否存在
                var navItem = NavigationConfig.GetNavigationItemByKey(navigationItemKey);
                if (navItem == null)
                    return ServiceResult.Failure("找不到對應的功能項目");

                // 檢查權限
                if (!string.IsNullOrEmpty(navItem.RequiredPermission))
                {
                    var permResult = await _permissionService.HasPermissionAsync(employeeId, navItem.RequiredPermission);
                    if (!permResult.IsSuccess || !permResult.Data)
                        return ServiceResult.Failure("您沒有此功能的權限");
                }

                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查是否已存在
                var exists = await context.EmployeeDashboardConfigs
                    .AnyAsync(c => c.EmployeeId == employeeId && c.NavigationItemKey == navigationItemKey);

                if (exists)
                    return ServiceResult.Failure("此捷徑已存在於首頁中");

                // 取得目前最大排序值
                var maxSortOrder = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId)
                    .MaxAsync(c => (int?)c.SortOrder) ?? 0;

                var config = new EmployeeDashboardConfig
                {
                    EmployeeId = employeeId,
                    NavigationItemKey = navigationItemKey,
                    SortOrder = maxSortOrder + 10,
                    IsVisible = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                };

                context.EmployeeDashboardConfigs.Add(config);

                // 標記員工已初始化儀表板
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddWidgetAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    NavigationItemKey = navigationItemKey
                });
                return ServiceResult.Failure("新增捷徑失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> AddWidgetBatchAsync(int employeeId, List<string> navigationItemKeys)
        {
            try
            {
                if (navigationItemKeys == null || !navigationItemKeys.Any())
                    return ServiceResult.Failure("請選擇至少一個捷徑");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得已存在的配置
                var existingKeys = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId && navigationItemKeys.Contains(c.NavigationItemKey))
                    .Select(c => c.NavigationItemKey)
                    .ToListAsync();

                // 取得員工權限
                var permissionResult = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
                var permissionCodes = permissionResult.IsSuccess ? permissionResult.Data ?? new List<string>() : new List<string>();

                // 取得目前最大排序值
                var maxSortOrder = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId)
                    .MaxAsync(c => (int?)c.SortOrder) ?? 0;

                var newConfigs = new List<EmployeeDashboardConfig>();
                var sortOrder = maxSortOrder;

                foreach (var key in navigationItemKeys)
                {
                    // 跳過已存在的
                    if (existingKeys.Contains(key))
                        continue;

                    // 檢查導航項目是否存在
                    var navItem = NavigationConfig.GetNavigationItemByKey(key);
                    if (navItem == null)
                        continue;

                    // 跳過沒有權限的
                    if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                        !permissionCodes.Contains(navItem.RequiredPermission))
                        continue;

                    sortOrder += 10;
                    newConfigs.Add(new EmployeeDashboardConfig
                    {
                        EmployeeId = employeeId,
                        NavigationItemKey = key,
                        SortOrder = sortOrder,
                        IsVisible = true,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now
                    });
                }

                if (newConfigs.Any())
                {
                    context.EmployeeDashboardConfigs.AddRange(newConfigs);
                }

                // 標記員工已初始化儀表板
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddWidgetBatchAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    KeyCount = navigationItemKeys?.Count ?? 0
                });
                return ServiceResult.Failure("批次新增捷徑失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> RemoveWidgetAsync(int employeeId, int configId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var config = await context.EmployeeDashboardConfigs
                    .FirstOrDefaultAsync(c => c.Id == configId && c.EmployeeId == employeeId);

                if (config == null)
                    return ServiceResult.Failure("找不到該配置");

                context.EmployeeDashboardConfigs.Remove(config);

                // 標記員工已初始化儀表板（避免移除全部後又自動套用預設）
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RemoveWidgetAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    ConfigId = configId
                });
                return ServiceResult.Failure("移除捷徑失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> UpdateSortOrderAsync(int employeeId, List<int> configIds)
        {
            try
            {
                if (configIds == null || !configIds.Any())
                    return ServiceResult.Failure("排序清單不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();

                var configs = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId && configIds.Contains(c.Id))
                    .ToListAsync();

                // 按照 configIds 的順序更新 SortOrder
                for (int i = 0; i < configIds.Count; i++)
                {
                    var config = configs.FirstOrDefault(c => c.Id == configIds[i]);
                    if (config != null)
                    {
                        config.SortOrder = (i + 1) * 10;
                        config.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSortOrderAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    ConfigCount = configIds?.Count ?? 0
                });
                return ServiceResult.Failure("更新排序失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<List<DashboardConfigWithNavItem>> InitializeDefaultDashboardAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得預設捷徑清單
                var defaultKeys = DashboardDefaults.DefaultWidgetKeys;

                // 取得員工權限
                var permissionResult = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
                var permissionCodes = permissionResult.IsSuccess ? permissionResult.Data ?? new List<string>() : new List<string>();

                var newConfigs = new List<EmployeeDashboardConfig>();
                var result = new List<DashboardConfigWithNavItem>();

                foreach (var key in defaultKeys)
                {
                    var navItem = NavigationConfig.GetNavigationItemByKey(key);
                    if (navItem == null) continue;

                    // 檢查權限
                    if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                        !permissionCodes.Contains(navItem.RequiredPermission))
                        continue;

                    var config = new EmployeeDashboardConfig
                    {
                        EmployeeId = employeeId,
                        NavigationItemKey = key,
                        SortOrder = DashboardDefaults.GetDefaultSortOrder(key),
                        IsVisible = true,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now
                    };

                    newConfigs.Add(config);
                }

                // 標記員工已初始化儀表板
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                if (newConfigs.Any())
                {
                    context.EmployeeDashboardConfigs.AddRange(newConfigs);
                }

                await context.SaveChangesAsync();

                // 建立回傳結果
                foreach (var config in newConfigs)
                {
                    var navItem = NavigationConfig.GetNavigationItemByKey(config.NavigationItemKey);
                    if (navItem != null)
                    {
                        result.Add(new DashboardConfigWithNavItem
                        {
                            Config = config,
                            NavigationItem = navItem
                        });
                    }
                }

                return result.OrderBy(r => r.Config.SortOrder).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(InitializeDefaultDashboardAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<DashboardConfigWithNavItem>();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> ResetToDefaultAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 刪除現有配置
                var existingConfigs = await context.EmployeeDashboardConfigs
                    .Where(c => c.EmployeeId == employeeId)
                    .ToListAsync();

                if (existingConfigs.Any())
                {
                    context.EmployeeDashboardConfigs.RemoveRange(existingConfigs);
                    await context.SaveChangesAsync();
                }

                // 重新初始化預設配置
                await InitializeDefaultDashboardAsync(employeeId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetToDefaultAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure("重置配置失敗");
            }
        }
    }
}
