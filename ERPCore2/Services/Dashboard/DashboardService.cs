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
    /// 儀表板服務 - 管理首頁動態面板與項目配置
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

        #region 面板管理

        /// <inheritdoc/>
        public async Task<List<DashboardPanelWithItems>> GetEmployeePanelsAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 查詢員工的所有面板與項目
                var panels = await context.EmployeeDashboardPanels
                    .Where(p => p.EmployeeId == employeeId)
                    .Include(p => p.DashboardConfigs.Where(c => c.IsVisible).OrderBy(c => c.SortOrder))
                    .OrderBy(p => p.SortOrder)
                    .ToListAsync();

                // 如果沒有任何面板，檢查是否為新用戶
                if (!panels.Any())
                {
                    var employee = await context.Employees.FindAsync(employeeId);
                    if (employee != null && !employee.HasInitializedDashboard)
                    {
                        // 新用戶，自動套用預設配置
                        return await InitializeDefaultDashboardAsync(employeeId);
                    }
                    return new List<DashboardPanelWithItems>();
                }

                // 取得員工權限
                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);

                // 組裝結果
                var result = new List<DashboardPanelWithItems>();
                foreach (var panel in panels)
                {
                    var panelWithItems = new DashboardPanelWithItems
                    {
                        Panel = panel,
                        Items = new List<DashboardConfigWithNavItem>()
                    };

                    foreach (var config in panel.DashboardConfigs)
                    {
                        var navItem = NavigationConfig.GetNavigationItemByKey(config.NavigationItemKey);
                        if (navItem == null) continue;

                        // 檢查權限
                        if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                            !permissionCodes.Contains(navItem.RequiredPermission))
                            continue;

                        panelWithItems.Items.Add(new DashboardConfigWithNavItem
                        {
                            Config = config,
                            NavigationItem = navItem
                        });
                    }

                    result.Add(panelWithItems);
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEmployeePanelsAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<DashboardPanelWithItems>();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<DashboardPanelWithItems>> CreatePanelAsync(int employeeId, string title)
        {
            try
            {
                // 驗證標題
                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult<DashboardPanelWithItems>.Failure("面板標題不可為空");

                title = title.Trim();
                if (title.Length > DashboardDefaults.MaxPanelTitleLength)
                    return ServiceResult<DashboardPanelWithItems>.Failure($"面板標題不可超過 {DashboardDefaults.MaxPanelTitleLength} 字");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查面板數量上限
                var panelCount = await context.EmployeeDashboardPanels
                    .CountAsync(p => p.EmployeeId == employeeId);

                if (panelCount >= DashboardDefaults.MaxPanelCount)
                    return ServiceResult<DashboardPanelWithItems>.Failure($"面板數量已達上限（{DashboardDefaults.MaxPanelCount} 個）");

                // 取得目前最大排序值
                var maxSortOrder = await context.EmployeeDashboardPanels
                    .Where(p => p.EmployeeId == employeeId)
                    .MaxAsync(p => (int?)p.SortOrder) ?? -1;

                var panel = new EmployeeDashboardPanel
                {
                    EmployeeId = employeeId,
                    Title = title,
                    SortOrder = maxSortOrder + 1,
                    IconClass = "bi bi-grid-fill",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                };

                context.EmployeeDashboardPanels.Add(panel);

                // 標記員工已初始化儀表板
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                await context.SaveChangesAsync();

                return ServiceResult<DashboardPanelWithItems>.Success(new DashboardPanelWithItems
                {
                    Panel = panel,
                    Items = new List<DashboardConfigWithNavItem>()
                });
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreatePanelAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    Title = title
                });
                return ServiceResult<DashboardPanelWithItems>.Failure("建立面板失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> UpdatePanelTitleAsync(int panelId, string title)
        {
            try
            {
                // 驗證標題
                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult.Failure("面板標題不可為空");

                title = title.Trim();
                if (title.Length > DashboardDefaults.MaxPanelTitleLength)
                    return ServiceResult.Failure($"面板標題不可超過 {DashboardDefaults.MaxPanelTitleLength} 字");

                using var context = await _contextFactory.CreateDbContextAsync();

                var panel = await context.EmployeeDashboardPanels.FindAsync(panelId);
                if (panel == null)
                    return ServiceResult.Failure("找不到該面板");

                panel.Title = title;
                panel.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePanelTitleAsync), GetType(), _logger, new
                {
                    PanelId = panelId,
                    Title = title
                });
                return ServiceResult.Failure("更新面板標題失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> UpdatePanelIconAsync(int panelId, string iconClass)
        {
            try
            {
                // 驗證圖示
                if (string.IsNullOrWhiteSpace(iconClass))
                    return ServiceResult.Failure("圖示不可為空");

                iconClass = iconClass.Trim();
                if (iconClass.Length > 50)
                    return ServiceResult.Failure("圖示名稱過長");

                // 驗證是否為有效的圖示（必須在可選清單中）
                if (!DashboardDefaults.AvailableIcons.Any(i => i.IconClass == iconClass))
                    return ServiceResult.Failure("無效的圖示選擇");

                using var context = await _contextFactory.CreateDbContextAsync();

                var panel = await context.EmployeeDashboardPanels.FindAsync(panelId);
                if (panel == null)
                    return ServiceResult.Failure("找不到該面板");

                panel.IconClass = iconClass;
                panel.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePanelIconAsync), GetType(), _logger, new
                {
                    PanelId = panelId,
                    IconClass = iconClass
                });
                return ServiceResult.Failure("更新面板圖示失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> DeletePanelAsync(int panelId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var panel = await context.EmployeeDashboardPanels
                    .Include(p => p.DashboardConfigs)
                    .FirstOrDefaultAsync(p => p.Id == panelId);

                if (panel == null)
                    return ServiceResult.Failure("找不到該面板");

                // 刪除面板（Cascade Delete 會自動刪除其下項目）
                context.EmployeeDashboardPanels.Remove(panel);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeletePanelAsync), GetType(), _logger, new
                {
                    PanelId = panelId
                });
                return ServiceResult.Failure("刪除面板失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> UpdatePanelSortOrderAsync(int employeeId, List<int> panelIds)
        {
            try
            {
                if (panelIds == null || !panelIds.Any())
                    return ServiceResult.Failure("排序清單不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();

                var panels = await context.EmployeeDashboardPanels
                    .Where(p => p.EmployeeId == employeeId && panelIds.Contains(p.Id))
                    .ToListAsync();

                for (int i = 0; i < panelIds.Count; i++)
                {
                    var panel = panels.FirstOrDefault(p => p.Id == panelIds[i]);
                    if (panel != null)
                    {
                        panel.SortOrder = i;
                        panel.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdatePanelSortOrderAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId,
                    PanelCount = panelIds?.Count ?? 0
                });
                return ServiceResult.Failure("更新面板排序失敗");
            }
        }

        #endregion

        #region 項目管理

        /// <inheritdoc/>
        public async Task<List<NavigationItem>> GetAvailableWidgetsAsync(int employeeId, bool isQuickAction)
        {
            try
            {
                // 根據類型取得對應的導航項目
                var allWidgets = isQuickAction
                    ? NavigationConfig.GetQuickActionWidgetItems()
                    : NavigationConfig.GetShortcutWidgetItems();

                // 過濾有權限的項目
                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);

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
                    EmployeeId = employeeId,
                    IsQuickAction = isQuickAction
                });
                return new List<NavigationItem>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<NavigationItem>> GetAvailableChartWidgetsAsync(int employeeId)
        {
            try
            {
                var allWidgets = NavigationConfig.GetChartWidgetItems();
                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);

                return allWidgets.Where(item =>
                    string.IsNullOrEmpty(item.RequiredPermission) ||
                    permissionCodes.Contains(item.RequiredPermission)
                ).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableChartWidgetsAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<NavigationItem>();
            }
        }

        /// <inheritdoc/>
        public async Task<HashSet<string>> GetPanelExistingKeysAsync(int panelId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var keys = await context.EmployeeDashboardConfigs
                    .Where(c => c.PanelId == panelId)
                    .Select(c => c.NavigationItemKey)
                    .ToListAsync();

                return keys.ToHashSet();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPanelExistingKeysAsync), GetType(), _logger, new
                {
                    PanelId = panelId
                });
                return new HashSet<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> AddWidgetBatchAsync(int panelId, List<string> navigationItemKeys)
        {
            try
            {
                if (navigationItemKeys == null || !navigationItemKeys.Any())
                    return ServiceResult.Failure("請選擇至少一個項目");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得面板資訊
                var panel = await context.EmployeeDashboardPanels.FindAsync(panelId);
                if (panel == null)
                    return ServiceResult.Failure("找不到該面板");

                // 取得已存在的配置
                var existingKeys = await context.EmployeeDashboardConfigs
                    .Where(c => c.PanelId == panelId && navigationItemKeys.Contains(c.NavigationItemKey))
                    .Select(c => c.NavigationItemKey)
                    .ToListAsync();

                // 取得員工權限
                var permissionCodes = await GetEmployeePermissionCodesAsync(panel.EmployeeId);

                // 取得目前最大排序值
                var maxSortOrder = await context.EmployeeDashboardConfigs
                    .Where(c => c.PanelId == panelId)
                    .MaxAsync(c => (int?)c.SortOrder) ?? 0;

                var newConfigs = new List<EmployeeDashboardConfig>();
                var sortOrder = maxSortOrder;

                foreach (var key in navigationItemKeys)
                {
                    if (existingKeys.Contains(key))
                        continue;

                    var navItem = NavigationConfig.GetNavigationItemByKey(key);
                    if (navItem == null)
                        continue;

                    if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                        !permissionCodes.Contains(navItem.RequiredPermission))
                        continue;

                    sortOrder += 10;
                    newConfigs.Add(new EmployeeDashboardConfig
                    {
                        PanelId = panelId,
                        EmployeeId = panel.EmployeeId,
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
                    await context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddWidgetBatchAsync), GetType(), _logger, new
                {
                    PanelId = panelId,
                    KeyCount = navigationItemKeys?.Count ?? 0
                });
                return ServiceResult.Failure("新增項目失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> RemoveWidgetAsync(int configId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var config = await context.EmployeeDashboardConfigs.FindAsync(configId);
                if (config == null)
                    return ServiceResult.Failure("找不到該配置");

                context.EmployeeDashboardConfigs.Remove(config);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RemoveWidgetAsync), GetType(), _logger, new
                {
                    ConfigId = configId
                });
                return ServiceResult.Failure("移除項目失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> UpdateItemSortOrderAsync(int panelId, List<int> configIds)
        {
            try
            {
                if (configIds == null || !configIds.Any())
                    return ServiceResult.Failure("排序清單不可為空");

                using var context = await _contextFactory.CreateDbContextAsync();

                var configs = await context.EmployeeDashboardConfigs
                    .Where(c => c.PanelId == panelId && configIds.Contains(c.Id))
                    .ToListAsync();

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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateItemSortOrderAsync), GetType(), _logger, new
                {
                    PanelId = panelId,
                    ConfigCount = configIds?.Count ?? 0
                });
                return ServiceResult.Failure("更新排序失敗");
            }
        }

        #endregion

        #region 初始化與重置

        /// <inheritdoc/>
        public async Task<List<DashboardPanelWithItems>> InitializeDefaultDashboardAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得員工權限
                var permissionCodes = await GetEmployeePermissionCodesAsync(employeeId);

                var result = new List<DashboardPanelWithItems>();

                // 根據預設定義建立面板
                foreach (var panelDef in DashboardDefaults.DefaultPanelDefinitions)
                {
                    var panel = new EmployeeDashboardPanel
                    {
                        EmployeeId = employeeId,
                        Title = panelDef.Title,
                        SortOrder = panelDef.SortOrder,
                        IconClass = panelDef.IconClass,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now
                    };

                    context.EmployeeDashboardPanels.Add(panel);
                    await context.SaveChangesAsync(); // 先儲存以取得 Panel Id

                    var panelWithItems = new DashboardPanelWithItems
                    {
                        Panel = panel,
                        Items = new List<DashboardConfigWithNavItem>()
                    };

                    // 建立面板內的項目
                    var sortOrder = 0;
                    foreach (var key in panelDef.ItemKeys)
                    {
                        var navItem = NavigationConfig.GetNavigationItemByKey(key);
                        if (navItem == null) continue;

                        if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                            !permissionCodes.Contains(navItem.RequiredPermission))
                            continue;

                        sortOrder += 10;
                        var config = new EmployeeDashboardConfig
                        {
                            PanelId = panel.Id,
                            EmployeeId = employeeId,
                            NavigationItemKey = key,
                            SortOrder = sortOrder,
                            IsVisible = true,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now
                        };

                        context.EmployeeDashboardConfigs.Add(config);

                        panelWithItems.Items.Add(new DashboardConfigWithNavItem
                        {
                            Config = config,
                            NavigationItem = navItem
                        });
                    }

                    result.Add(panelWithItems);
                }

                // 標記員工已初始化儀表板
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null && !employee.HasInitializedDashboard)
                {
                    employee.HasInitializedDashboard = true;
                }

                await context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(InitializeDefaultDashboardAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return new List<DashboardPanelWithItems>();
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> ResetPanelToDefaultAsync(int panelId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var panel = await context.EmployeeDashboardPanels
                    .Include(p => p.DashboardConfigs)
                    .FirstOrDefaultAsync(p => p.Id == panelId);

                if (panel == null)
                    return ServiceResult.Failure("找不到該面板");

                // 尋找對應的預設定義
                var defaultDef = DashboardDefaults.DefaultPanelDefinitions
                    .FirstOrDefault(d => d.Title == panel.Title);

                if (defaultDef == null)
                    return ServiceResult.Failure("此面板沒有對應的預設配置");

                // 刪除現有項目
                context.EmployeeDashboardConfigs.RemoveRange(panel.DashboardConfigs);

                // 取得員工權限
                var permissionCodes = await GetEmployeePermissionCodesAsync(panel.EmployeeId);

                // 重新建立預設項目
                var sortOrder = 0;
                foreach (var key in defaultDef.ItemKeys)
                {
                    var navItem = NavigationConfig.GetNavigationItemByKey(key);
                    if (navItem == null) continue;

                    if (!string.IsNullOrEmpty(navItem.RequiredPermission) &&
                        !permissionCodes.Contains(navItem.RequiredPermission))
                        continue;

                    sortOrder += 10;
                    context.EmployeeDashboardConfigs.Add(new EmployeeDashboardConfig
                    {
                        PanelId = panelId,
                        EmployeeId = panel.EmployeeId,
                        NavigationItemKey = key,
                        SortOrder = sortOrder,
                        IsVisible = true,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now
                    });
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetPanelToDefaultAsync), GetType(), _logger, new
                {
                    PanelId = panelId
                });
                return ServiceResult.Failure("重置面板失敗");
            }
        }

        /// <inheritdoc/>
        public async Task<ServiceResult> ResetAllPanelsToDefaultAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 刪除所有現有面板（Cascade Delete 會自動刪除項目）
                var existingPanels = await context.EmployeeDashboardPanels
                    .Where(p => p.EmployeeId == employeeId)
                    .ToListAsync();

                if (existingPanels.Any())
                {
                    context.EmployeeDashboardPanels.RemoveRange(existingPanels);
                    await context.SaveChangesAsync();
                }

                // 重置初始化狀態以觸發重新初始化
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    employee.HasInitializedDashboard = false;
                    await context.SaveChangesAsync();
                }

                // 重新初始化
                await InitializeDefaultDashboardAsync(employeeId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetAllPanelsToDefaultAsync), GetType(), _logger, new
                {
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure("重置所有面板失敗");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 取得員工權限代碼清單
        /// </summary>
        private async Task<HashSet<string>> GetEmployeePermissionCodesAsync(int employeeId)
        {
            var permissionResult = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
            var codes = permissionResult.IsSuccess ? permissionResult.Data ?? new List<string>() : new List<string>();
            return codes.ToHashSet();
        }

        #endregion
    }
}
