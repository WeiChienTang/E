# 讀取資料庫優化 - 權限檢查查詢優化方案

## 📋 問題描述

### 現象
開啟任何 Index 頁面時，會觸發大量重複的資料庫查詢：

```
開啟 QuotationIndex.razor 時的資料庫查詢統計:
- Employees 表: 10 次
- Roles 表: 10 次  
- Departments 表: 11 次
- Companies 表: 7 次
- Suppliers 表: 7 次
- PaymentMethods 表: 6 次
- Customers 表: 9 次
- EmployeePositions 表: 9 次

總計: 60+ 次資料庫查詢
```

### 根本原因

#### 1. 導航選單權限檢查 (主要問題)
每個導航選單項目渲染時都會檢查權限：

```csharp
// NavDropdownItem.razor
<NavigationPermissionCheck Permission="@RequiredPermission">
    // 每個選單項目都觸發一次權限查詢
</NavigationPermissionCheck>

// 流程:
NavigationPermissionCheck 
→ NavigationPermissionService.CanAccessAsync()
→ PermissionService.GetEmployeePermissionsAsync()
→ 查詢資料庫: Employees + Roles + RolePermissions + Permissions
```

**問題:** 
- 導航選單有 30+ 個項目
- 每個項目獨立查詢權限
- 沒有共享快取
- **結果: 30+ 次相同查詢**

#### 2. 頁面權限檢查 (次要問題)
```csharp
// GenericIndexPageComponent
<PagePermissionCheck RequiredPermission="@requiredPermission">
    // 頁面載入時再次檢查權限
</PagePermissionCheck>

// 又是相同的查詢流程
```

#### 3. 雙重渲染機制 (加劇問題)
```csharp
// NavMenuItem.razor
<div style="position: absolute; left: -9999px; visibility: hidden;">
    @DropdownItems  // 隱藏渲染以收集權限
</div>
<NavigationPermissionCheck AnyPermissions="@GetCollectedPermissions()">
    @DropdownItems  // 正式渲染
</NavigationPermissionCheck>

// 每個下拉選單的子項目都渲染兩次!
```

---

## 🎯 解決方案

### 核心策略: 預先批次載入 + 記憶體快取

**原理:**
1. 在應用啟動時(MainLayout)預先載入當前使用者的所有權限
2. 將權限存入記憶體快取 (HashSet)
3. 後續所有權限檢查都從快取讀取
4. 完全避免重複查詢

**效果:**
- ✅ 從 60+ 次查詢降為 1-2 次
- ✅ 查詢減少 97%
- ✅ 頁面載入速度提升 50-70%

---

## 📝 實作計畫

### 階段一: 核心權限快取機制 (必須)

#### 1. 修改 `INavigationPermissionService.cs`
**檔案位置:** `Services/Auth/NavigationPermissionService.cs`

**新增方法:**
```csharp
public interface INavigationPermissionService
{
    Task<bool> CanAccessAsync(string permission);
    Task<bool> CanAccessModuleAsync(string module);
    Task<int> GetCurrentEmployeeIdAsync();
    
    // ⭐ 新增: 批次取得所有權限
    Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId);
    
    // ⭐ 新增: 清除權限快取
    void ClearEmployeePermissionCache(int employeeId);
}
```

#### 2. 修改 `NavigationPermissionService.cs`
**檔案位置:** `Services/Auth/NavigationPermissionService.cs`

**新增字段:**
```csharp
private readonly IMemoryCache _cache;
private const string ALL_PERMS_CACHE_PREFIX = "all_nav_perms_";
private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
```

**新增方法:**
```csharp
/// <summary>
/// 批次取得員工所有權限(含快取)
/// </summary>
public async Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId)
{
    var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
    
    // 檢查快取
    if (_cache.TryGetValue(cacheKey, out HashSet<string>? cachedPermissions))
    {
        return cachedPermissions!;
    }
    
    // 從資料庫載入
    var result = await _permissionService.GetEmployeePermissionCodesAsync(employeeId);
    
    var permissions = new HashSet<string>(
        result.Data ?? new List<string>(), 
        StringComparer.OrdinalIgnoreCase
    );
    
    // 快取 10 分鐘
    _cache.Set(cacheKey, permissions, _cacheExpiration);
    
    return permissions;
}

/// <summary>
/// 清除員工權限快取
/// </summary>
public void ClearEmployeePermissionCache(int employeeId)
{
    var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
    _cache.Remove(cacheKey);
}
```

**修改現有方法:**
```csharp
public async Task<bool> CanAccessAsync(string permission)
{
    try
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId <= 0) return false;

        // ⭐ 使用批次快取
        var allPermissions = await GetAllEmployeePermissionsAsync(employeeId);
        
        // 先檢查 System.Admin
        if (allPermissions.Contains("System.Admin"))
            return true;
        
        // 再檢查特定權限
        return allPermissions.Contains(permission);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanAccessAsync), GetType(), _logger, new { 
            Permission = permission 
        });
        return false;
    }
}

public async Task<bool> CanAccessModuleAsync(string module)
{
    try
    {
        var employeeId = await GetCurrentEmployeeIdAsync();
        if (employeeId <= 0) return false;

        // ⭐ 使用批次快取
        var allPermissions = await GetAllEmployeePermissionsAsync(employeeId);
        
        // 檢查是否有該模組的任何權限
        return allPermissions.Any(p => p.StartsWith(module + ".", StringComparison.OrdinalIgnoreCase));
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanAccessModuleAsync), GetType(), _logger, new { 
            Module = module 
        });
        return false;
    }
}
```

#### 3. 修改 `PermissionService.cs`
**檔案位置:** `Services/Employees/PermissionService.cs`

**修改 `GetEmployeePermissionsAsync` 方法:**
```csharp
public async Task<ServiceResult<List<Permission>>> GetEmployeePermissionsAsync(int employeeId)
{
    try
    {
        var cacheKey = $"employee_permissions_{employeeId}";
        
        if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions) && cachedPermissions != null)
            return ServiceResult<List<Permission>>.Success(cachedPermissions);

        using var context = await _contextFactory.CreateDbContextAsync();
        var employee = await context.Employees
            .Include(e => e.Role)
            .ThenInclude(r => r != null ? r.RolePermissions : null!)
            .ThenInclude(rp => rp.Permission)
            .AsNoTracking()        // ⭐ 新增: 不追蹤變更，提升效能
            .AsSplitQuery()        // ⭐ 新增: 避免笛卡爾積
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        // ... 後續邏輯不變
    }
    catch (Exception ex)
    {
        // ... 錯誤處理
    }
}
```

**說明:**
- `AsNoTracking()`: 因為只是讀取權限，不需要追蹤實體變更
- `AsSplitQuery()`: 將 Include 拆成多個查詢，避免 JOIN 產生的笛卡爾積

#### 4. 修改 `MainLayout.razor`
**檔案位置:** `Components/Layout/MainLayout.razor`

**在 `OnInitializedAsync` 加入預載邏輯:**
```csharp
@inject INavigationPermissionService NavigationPermissionService

@code {
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // ⭐ 預先載入當前使用者的所有權限到快取
            var employeeId = await NavigationPermissionService.GetCurrentEmployeeIdAsync();
            if (employeeId > 0)
            {
                // 這會觸發一次資料庫查詢，並將結果快取
                _ = await NavigationPermissionService.GetAllEmployeePermissionsAsync(employeeId);
            }
        }
        catch
        {
            // 忽略預載錯誤，不影響主要功能
        }
        
        // 初始化 Action 註冊表
        actionRegistry = NavigationActionHelper.CreateRegistry();
        actionRegistry.Register("OpenAccountsReceivableReport", OpenAccountsReceivableReport);
        actionRegistry.Register("OpenRolePermissionManagement", OpenRolePermissionManagement);
    }
    
    // ... 其他方法
}
```

---

### 階段二: 快取清除機制 (建議)

#### 1. 登出時清除快取
**檔案位置:** `Controllers/AuthController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> SignOut()
{
    var employeeId = GetCurrentEmployeeId();
    if (employeeId > 0)
    {
        // ⭐ 清除權限快取
        _navigationPermissionService.ClearEmployeePermissionCache(employeeId);
    }
    
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Login");
}
```

#### 2. 更新員工角色時清除快取
**檔案位置:** `Services/Employees/EmployeeService.cs`

```csharp
public async Task<ServiceResult<Employee>> UpdateAsync(Employee employee)
{
    var result = await base.UpdateAsync(employee);
    
    if (result.IsSuccess)
    {
        // ⭐ 清除權限快取 (如果有注入 NavigationPermissionService)
        // _navigationPermissionService?.ClearEmployeePermissionCache(employee.Id);
        
        // 或者直接清除 PermissionService 的快取
        await _permissionService.RefreshEmployeePermissionCacheAsync(employee.Id);
    }
    
    return result;
}
```

#### 3. 更新角色權限時清除快取
**檔案位置:** `Services/Employees/RolePermissionService.cs` (如果存在)

```csharp
public async Task<ServiceResult> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
{
    // ... 更新邏輯
    
    // ⭐ 清除所有使用該角色的員工權限快取
    using var context = await _contextFactory.CreateDbContextAsync();
    var employees = await context.Employees
        .Where(e => e.RoleId == roleId)
        .Select(e => e.Id)
        .ToListAsync();
    
    foreach (var empId in employees)
    {
        _navigationPermissionService.ClearEmployeePermissionCache(empId);
    }
    
    return ServiceResult.Success();
}
```

---

### 階段三: 資料庫索引優化 (建議)

#### 建立 Migration
**指令:**
```bash
dotnet ef migrations add OptimizePermissionQueryIndexes
```

**Migration 內容:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 員工角色關聯索引
    migrationBuilder.CreateIndex(
        name: "IX_Employees_RoleId_Status",
        table: "Employees",
        columns: new[] { "RoleId", "Status" },
        filter: "[RoleId] IS NOT NULL AND [Status] = 1");
    
    // 角色權限關聯索引
    migrationBuilder.CreateIndex(
        name: "IX_RolePermissions_RoleId_Status",
        table: "RolePermissions",
        columns: new[] { "RoleId", "Status" },
        filter: "[Status] = 1");
    
    // 權限代碼唯一索引
    migrationBuilder.CreateIndex(
        name: "IX_Permissions_Code",
        table: "Permissions",
        column: "Code",
        unique: true,
        filter: "[Code] IS NOT NULL");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(name: "IX_Employees_RoleId_Status", table: "Employees");
    migrationBuilder.DropIndex(name: "IX_RolePermissions_RoleId_Status", table: "RolePermissions");
    migrationBuilder.DropIndex(name: "IX_Permissions_Code", table: "Permissions");
}
```

**或直接執行 SQL:**
```sql
-- 員工角色關聯
CREATE INDEX IX_Employees_RoleId_Status 
ON Employees(RoleId, Status) 
WHERE RoleId IS NOT NULL AND Status = 1;

-- 角色權限關聯
CREATE INDEX IX_RolePermissions_RoleId_Status 
ON RolePermissions(RoleId, Status) 
WHERE Status = 1;

-- 權限代碼
CREATE UNIQUE INDEX IX_Permissions_Code 
ON Permissions(Code) 
WHERE Code IS NOT NULL;
```

---

## 📊 預期效果

### 效能提升
| 指標 | 修改前 | 修改後 | 改善 |
|------|--------|--------|------|
| 資料庫查詢次數 | 60+ 次 | 1-2 次 | ↓ 97% |
| 頁面載入時間 | ~800ms | ~200ms | ↓ 75% |
| 伺服器負載 | 高 | 低 | ↓ 60% |

### 記憶體影響
- 每個使用者快取: ~2-5 KB (HashSet<string>)
- 100 個同時在線使用者: ~500 KB
- **完全可接受** ✅

### 快取策略
- **TTL:** 10 分鐘 (可調整)
- **失效時機:** 
  - 自動: 10 分鐘後過期
  - 手動: 登出、角色變更、權限變更
- **一致性:** 強一致性 (主動清除)

---

## ✅ 修改檢查清單

### 階段一 (核心 - 必須)
- [ ] 修改 `INavigationPermissionService.cs` - 新增 2 個方法簽章
- [ ] 修改 `NavigationPermissionService.cs` - 實作批次權限快取
- [ ] 修改 `PermissionService.cs` - 加入 AsNoTracking + AsSplitQuery
- [ ] 修改 `MainLayout.razor` - 預載權限

### 階段二 (快取清除 - 建議)
- [ ] 修改 `AuthController.cs` - 登出清除快取
- [ ] 修改 `EmployeeService.cs` - 更新員工時清除快取
- [ ] 修改 `RolePermissionService.cs` - 更新角色權限時清除快取

### 階段三 (索引 - 建議)
- [ ] 建立資料庫索引 Migration
- [ ] 執行 Migration 或 SQL

---

## 🧪 測試計畫

### 1. 功能測試
```
測試項目:
✅ 登入後導航選單正常顯示
✅ 有權限的選單項目可見
✅ 無權限的選單項目隱藏
✅ 頁面權限檢查正常運作
✅ 不同角色看到不同選單
```

### 2. 效能測試
```
測試方法:
1. 開啟 SQL Profiler 或查看 EF Core 日誌
2. 登入系統
3. 瀏覽不同頁面 (QuotationIndex, CustomerIndex 等)
4. 記錄查詢次數

預期結果:
- 首次載入: 1-2 次權限查詢
- 後續頁面: 0 次權限查詢 (從快取讀取)
```

### 3. 快取測試
```
測試場景:
1. 登入 → 檢查快取建立
2. 瀏覽頁面 → 確認使用快取
3. 等待 10 分鐘 → 快取過期，重新載入
4. 變更角色 → 快取清除
5. 登出 → 快取清除
```

---

## 🚨 注意事項

### 1. 相依性注入
確保 `NavigationPermissionService` 有注入 `IMemoryCache`:

```csharp
// Program.cs 或 Startup.cs
builder.Services.AddMemoryCache();
builder.Services.AddScoped<INavigationPermissionService, NavigationPermissionService>();
```

### 2. 快取過期時間調整
根據實際需求調整快取時間:

```csharp
// 開發環境: 較短時間方便測試
private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

// 生產環境: 較長時間減少查詢
private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);
```

### 3. 權限變更即時性
如果需要權限變更立即生效，務必實作階段二的快取清除機制。

### 4. 多伺服器部署
如果是多伺服器部署 (Load Balancer):
- 目前方案: 每台伺服器獨立快取 (可接受)
- 未來升級: 使用 Redis 分散式快取

---

## 🔮 未來擴展方向

### 升級到分散式快取 (Redis)
當系統擴展到多伺服器時:

```csharp
// 只需修改 NavigationPermissionService.cs
// 將 IMemoryCache 改為 IDistributedCache

public class NavigationPermissionService : INavigationPermissionService
{
    private readonly IDistributedCache _cache; // ← 改用分散式快取
    
    public async Task<HashSet<string>> GetAllEmployeePermissionsAsync(int employeeId)
    {
        var cacheKey = $"{ALL_PERMS_CACHE_PREFIX}{employeeId}";
        
        // 從 Redis 讀取
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<HashSet<string>>(cachedData)!;
        }
        
        // ... 載入並存入 Redis
    }
}
```

**優點:**
- 所有伺服器共享快取
- 權限變更立即同步
- 減少記憶體使用

---

## 📚 相關文件

- [EF Core 效能最佳化](https://learn.microsoft.com/zh-tw/ef/core/performance/)
- [ASP.NET Core 記憶體快取](https://learn.microsoft.com/zh-tw/aspnet/core/performance/caching/memory)
- [查詢追蹤 vs 無追蹤](https://learn.microsoft.com/zh-tw/ef/core/querying/tracking)

---

## 📅 修改歷程

| 日期 | 版本 | 修改內容 | 修改人 |
|------|------|----------|--------|
| 2025-12-02 | 1.0 | 建立文件，規劃優化方案 | - |
| | | | |

---

**最後更新:** 2025年12月2日
**文件狀態:** ✅ 規劃完成，待實作
