# Module 模組限制設計說明

## 概觀

ERPCore2 採用**兩層存取控制**機制來限制頁面與功能的使用：

1. **公司層（Company-level）**：模組是否啟用 → 由 SuperAdmin 在模組管理介面控制
2. **使用者層（User-level）**：使用者是否擁有對應權限 → 由角色/員工權限控制

只有**兩層都通過**，使用者才能正常使用功能。

> **權限層級（由高至低）**
> 1. **IsSuperAdmin = true**（DB 欄位）：唯一可完全繞過公司層級模組限制的身分，不受任何存取控制。系統中只有一位，通常為系統設計者。
> 2. **System.Admin**（權限代碼）：可繞過使用者層級的功能權限檢查，但仍受公司層級模組限制約束。
> 3. **一般使用者**：兩層均須通過。

---

## 核心資料結構

### CompanyModule 實體

位置：`Data/Entities/Systems/CompanyModule.cs`

| 欄位 | 類型 | 說明 |
|------|------|------|
| `ModuleKey` | string | 模組唯一識別鍵（如 `"Sales"`、`"Warehouse"`） |
| `DisplayName` | string | 顯示名稱（如 `"銷貨管理"`） |
| `Description` | string | 說明文字（可空） |
| `IsEnabled` | bool | 是否啟用（預設 `true`） |
| `SortOrder` | int | 排序順序 |

資料庫表格由 `AppDbContext.CompanyModules` 管理，`ModuleKey` 建有唯一索引。

---

## 模組鍵對照表

所有模組鍵由 `Data/Navigation/NavigationConfig.cs` 的父層選單項目定義，並在資料庫初始化時由 `CompanyModuleSeeder` 自動寫入：

| ModuleKey | 功能模組 |
|-----------|----------|
| `Employees` | 人力資源管理 |
| `Suppliers` | 廠商管理 |
| `Customers` | 客戶管理 |
| `Products` | 產品管理 |
| `Warehouse` | 倉庫/庫存管理 |
| `Purchase` | 採購管理 |
| `Sales` | 銷貨管理 |
| `Vehicles` | 車輛管理 |
| `WasteManagement` | 廢棄物管理 |
| `FinancialManagement` | 財務管理 |
| `Accounting` | 會計管理 |
| `ProductionManagement` | 生產管理 |

### 模組與子項目的關係

在 `NavigationConfig` 中，每個父層選單（`IsParent = true`）設有 `ModuleKey`，其下的**所有子項目**皆屬於同一模組。

`GetFlattenedNavigationItems()` 在攤平巡覽項目時，會自動將父層的 `ModuleKey` 繼承給子項目：

```csharp
// NavigationConfig.GetFlattenedNavigationItems()
// 繼承父級的模組鍵（子項目屬於同一模組）
if (string.IsNullOrEmpty(child.ModuleKey) && !string.IsNullOrEmpty(item.ModuleKey))
    child.ModuleKey = item.ModuleKey;
```

因此停用一個模組，會同時限制該模組底下的：
- **頁面**（Route 類型）
- **所有 Action 類型項目**（含圖表入口，如 `OpenCustomerCharts`）
- **快速功能**（QuickAction 類型，衍生自有 `QuickActionId` 的 Route 項目）

> **注意**：`DeriveQuickActionItems()` 在產生衍生的 QuickAction 項目時，必須複製來源項目的 `ModuleKey`，否則 `DashboardService` 的模組過濾器（`string.IsNullOrEmpty(item.ModuleKey)` 判斷）會將其視為無模組限制而放行。此問題已於 2026-02-27 修復。

---

## 服務層

### ICompanyModuleService / CompanyModuleService

位置：`Services/Systems/CompanyModuleService.cs`

```csharp
// 檢查模組是否啟用（含快取，30 分鐘有效）
Task<bool> IsModuleEnabledAsync(string moduleKey);

// 取得所有模組清單
Task<List<CompanyModule>> GetAllAsync();

// 批次更新模組啟用狀態（更新後清除快取）
Task<ServiceResult> UpdateModulesAsync(List<CompanyModule> modules, string updatedBy);
```

**重要行為**：
- 快取機制：以 `"CompanyModules_IsEnabled"` 為快取鍵，有效期 **30 分鐘**
- 若 `moduleKey` 為 null 或空字串 → 視為**允許**（安全預設值）
- 若資料庫中找不到對應模組 → 視為**允許**（種子資料尚未寫入時的安全預設值）

---

### INavigationPermissionService / NavigationPermissionService

位置：`Services/Auth/NavigationPermissionService.cs`

提供整合公司層 + 使用者層的複合檢查：

```csharp
// 複合檢查：公司層模組 + 使用者層模組權限（以 "ModuleKey." 開頭的權限）
Task<bool> CanAccessModuleAsync(string module);

// 僅檢查公司層級模組是否啟用（IsSuperAdmin 可繞過，不做使用者權限前綴檢查）
// 用於報表等需要獨立模組檢查、且權限命名不符合 "ModuleKey." 格式的場景
Task<bool> IsModuleEnabledAsync(string moduleKey);

// 僅檢查使用者是否擁有特定權限（不考慮公司層級模組狀態）
Task<bool> CanAccessAsync(string permission);
```

**`CanAccessModuleAsync()` 執行邏輯**：

```
1. 取得目前登入員工 ID → 若無效則拒絕
2. 查詢員工 IsSuperAdmin 狀態（含快取）→ 若為 true 直接允許（繞過所有限制）
3. 呼叫 CompanyModuleService.IsModuleEnabledAsync(module) → 模組停用則拒絕（System.Admin 亦受此限制）
4. 取得員工所有權限清單
5. 若包含 "System.Admin" → 允許（模組已啟用前提下的管理者存取）
6. 檢查員工是否有任何以 "ModuleKey." 開頭的權限 → 無則拒絕
```

> ⚠️ **命名不一致問題**：`CanAccessModuleAsync("Accounting")` 步驟 6 會尋找以 `"Accounting."` 開頭的權限，但實際會計模組的權限命名為 `"AccountItem.Read"` / `"JournalEntry.Read"`（非 `"Accounting.*"` 格式）。因此**不可對報表使用 `CanAccessModuleAsync`**，應改用 `IsModuleEnabledAsync`（只查公司層） + `CanAccessAsync`（只查個人權限）的雙層分離方式。

**`IsModuleEnabledAsync()` 執行邏輯**（2026-03-02 新增）：

```
1. 取得目前登入員工 ID → 若無效則拒絕
2. 查詢員工 IsSuperAdmin 狀態 → 若為 true 直接允許（繞過所有限制）
3. 呼叫 CompanyModuleService.IsModuleEnabledAsync(moduleKey) → 回傳模組啟用狀態
   （不做使用者層級的 "ModuleKey." 前綴權限檢查）
```

---

## 存取控制流程

### 1. 頁面層 — GenericIndexPageComponent

位置：`Components/Shared/Page/GenericIndexPageComponent.razor`

Index 頁面透過 `RequiredModule` 參數宣告所屬模組：

```razor
<GenericIndexPageComponent
    RequiredModule="Sales"
    RequiredPermission="SalesOrder.Read"
    ... />
```

**OnInitializedAsync 流程**：

```
1. 呼叫 CompanyModuleService.IsModuleEnabledAsync(RequiredModule)
2. 若模組停用 (_isModuleEnabled = false)：
   → 不載入資料，顯示 🔒「此功能未開放」鎖定畫面
3. 若模組啟用：
   → 執行 InitializePageAsync()，並用 <PagePermissionCheck> 包覆頁面內容
```

### 2. Action 類型層 — MainLayout.HandleNavigationAction

位置：`Components/Layout/MainLayout.razor`

圖表介面等 Action 類型項目透過 `NavigationActionHandler` CascadingValue 觸發。`HandleNavigationAction` 在執行 Action 前會先做模組檢查：

```csharp
private async void HandleNavigationAction(string actionId)
{
    // 從扁平化清單找到對應 Action 項目（繼承了父層 ModuleKey）
    var navItem = NavigationConfig.GetFlattenedNavigationItems()
        .FirstOrDefault(item => item.ItemType == NavigationItemType.Action
                             && item.ActionId == actionId);

    // 若有 ModuleKey，檢查模組（CanAccessModuleAsync 內部處理 SuperAdmin 繞過）
    if (!string.IsNullOrEmpty(navItem?.ModuleKey))
    {
        var canAccess = await NavigationPermissionService.CanAccessModuleAsync(navItem.ModuleKey);
        if (!canAccess)
        {
            // 模組停用：開啟模組停用提示 Modal，與 Index 頁面鎖定畫面視覺一致
            _showModuleDisabledModal = true;
            StateHasChanged();
            return;
        }
    }

    actionRegistry?.Execute(actionId);
}
```

**涵蓋範圍**：所有透過 ActionRegistry 觸發的 Action 類型項目（含圖表等）。

**模組停用時的行為**（2026-02-27 更新）：開啟 `_showModuleDisabledModal`（`BaseModalComponent`），Modal 內顯示鎖定圖示（`bi-lock-fill`）、標題「此功能未開放」、說明「您的系統目前未啟用此功能模組，請聯絡系統管理員。」，與 Index 頁面的鎖定畫面視覺完全一致。Modal 宣告在 `MainLayout.razor` Razor 區塊末段，使用者可按右上角關閉鍵或點擊背景關閉。

### 3. 報表層 — MainLayout.HandleReportSelected + GenericReportFilterModalComponent

（2026-03-02 新增）

報表的存取控制需要特殊處理，因為報表的權限命名（如 `AccountItem.Read`）不符合 `CanAccessModuleAsync` 要求的 `"ModuleKey."` 前綴格式。改用雙層分離檢查：

**`ReportDefinition` 新增 `ModuleKey` 欄位**（`Models/Reports/ReportDefinition.cs`）：

```csharp
/// <summary>
/// 公司模組鍵值（對應 CompanyModule.ModuleKey）
/// 設定後會在開啟報表前檢查公司層級模組是否啟用
/// </summary>
public string? ModuleKey { get; set; }
```

**報表模組鍵對照**（`Data/Reports/ReportRegistry.cs`）：

| ReportCategory | ModuleKey |
|----------------|-----------|
| `Customer` | `"Customers"` |
| `Supplier` | `"Suppliers"` |
| `Product` | `"Products"` |
| `Inventory` | `"Warehouse"` |
| `Sales` | `"Sales"` |
| `Purchase` | `"Purchase"` |
| `HR` | `"Employees"` |
| `Vehicle` | `"Vehicles"` |
| `Waste` | `"WasteManagement"` |
| `Financial` | `"FinancialManagement"` |
| `Accounting` | `"Accounting"` |

**`HandleReportSelected` 執行邏輯**（`Components/Layout/MainLayout.razor`）：

```csharp
private async Task HandleReportSelected(string actionId)
{
    var reportDef = Data.ReportRegistry.GetAllReports()
        .FirstOrDefault(r => r.ActionId == actionId);
    if (reportDef == null) return;

    // 1. 公司層：檢查模組是否啟用（IsSuperAdmin 自動繞過）
    if (!string.IsNullOrEmpty(reportDef.ModuleKey))
    {
        var moduleEnabled = await NavigationPermissionService.IsModuleEnabledAsync(reportDef.ModuleKey);
        if (!moduleEnabled)
        {
            _showModuleDisabledModal = true;  // 顯示「此功能未開放」Modal
            StateHasChanged();
            return;
        }
    }

    // 2. 使用者層：檢查個人權限（SuperAdmin 自動繞過）
    if (!string.IsNullOrEmpty(reportDef.RequiredPermission))
    {
        var hasPermission = await NavigationPermissionService.CanAccessAsync(reportDef.RequiredPermission);
        if (!hasPermission) return;
    }

    // 3. 開啟篩選 Modal 或直接執行 Action
}
```

**第二道防線 — `GenericReportFilterModalComponent.OnParametersSetAsync`**：

同樣進行雙層檢查（模組 → 個人權限），防止直接設定 `currentFilterReportId` 時繞過檢查。模組停用時靜默關閉（呼叫端 `HandleReportSelected` 已顯示 Modal，此處不重複）。

**涵蓋的入口點**：
- 從 NavMenu 進入的報表中心（`GenericReportIndexPage`）→ `HandleReportSelected`
- 從報表搜尋 Modal（`reportSearchModal`）選擇 → `HandleReportSelected`
- 直接設定 `currentFilterReportId` 的情境 → `GenericReportFilterModalComponent` 備援

### 4. 元件層 — PagePermissionCheck

位置：`Components/Shared/PagePermissionCheck.razor`

在模組通過後，進一步檢查使用者是否擁有頁面所需的特定權限（如 `SalesOrder.Read`）。

### 4. EditModal Tab 層 — 跨模組 Tab 的條件式顯示

某些 EditModal 包含屬於其他模組的 Tab（例如廠商、客戶、員工的「車輛資訊」Tab 屬於 `Vehicles` 模組）。當對應模組停用時，該 Tab 應完全隱藏。

**實作位置**：各 EditModal 的 `InitializeFormFieldsAsync()`

```csharp
// 車輛模組狀態（停用時隱藏車輛 Tab）
var isVehiclesEnabled = await CompanyModuleService.IsModuleEnabledAsync("Vehicles");
if (!isVehiclesEnabled)
    isVehiclesEnabled = await NavigationPermissionService.IsCurrentEmployeeSuperAdminAsync();

var builder = FormSectionHelper<Customer>.Create()
    // ... 欄位區段設定 ...
    .GroupIntoTab("客戶資料", "bi-building", ...);

if (isVehiclesEnabled)
    builder = builder.GroupIntoCustomTab("車輛資訊", "bi-truck", CreateVehicleTabContent());

var layout = builder
    .GroupIntoCustomTab("拜訪紀錄", ...)
    .BuildAll();
```

**為何 null 保護能自動生效**：Tab 未加入時，`vehicleTab` 元件參考保持 `null`，所有後續呼叫（`LoadAsync`、`SavePendingChangesAsync`、`Clear`）均已具備 `if (vehicleTab != null)` 保護，無須額外修改。

**目前套用此模式的 EditModal**：
- `CustomerEditModalComponent.razor`（客戶）
- `SupplierEditModalComponent.razor`（廠商）
- `EmployeeEditModalComponent.razor`（員工）

### 5. 首頁儀表板層 — DashboardService

位置：`Services/Dashboard/DashboardService.cs`

儀表板在以下操作中均過濾停用模組的項目：

| 方法 | 說明 |
|------|------|
| `GetEmployeePanelsAsync()` | 載入儀表板時，隱藏停用模組的捷徑卡片 |
| `GetAvailableWidgetsAsync()` | 新增捷徑選擇器中，排除停用模組的頁面連結與快速功能 |
| `GetAvailableChartWidgetsAsync()` | 新增捷徑選擇器中，排除停用模組的圖表介面 |
| `InitializeDefaultDashboardAsync()` | 初始化預設面板時，跳過停用模組的預設項目 |
| `AddWidgetBatchAsync()` | 批次新增捷徑時，驗證模組是否啟用 |
| `ResetPanelToDefaultAsync()` | 重置面板時，跳過停用模組的預設項目 |

**IsSuperAdmin 繞過機制**（`GetEnabledModuleKeysAsync`）：

```csharp
private async Task<HashSet<string>?> GetEnabledModuleKeysAsync(int employeeId)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    var isSuperAdmin = await context.Employees
        .Where(e => e.Id == employeeId)
        .Select(e => e.IsSuperAdmin)
        .FirstOrDefaultAsync();
    if (isSuperAdmin)
        return null;  // null 表示跳過所有模組與權限過濾

    var allModules = await _companyModuleService.GetAllAsync();
    return allModules.Where(m => m.IsEnabled).Select(m => m.ModuleKey).ToHashSet();
}
```

所有過濾點均判斷 `enabledModuleKeys == null`（IsSuperAdmin）時跳過模組過濾。

### 6. 選單層 — NavMenu

位置：`Components/Layout/NavMenu.razor`

`FilterMenuGroupsAsync()` 透過 `CanAccessModuleAsync()` 過濾，停用模組對應的父層選單項目會對一般使用者隱藏（SuperAdmin 除外）。

---

## 完整存取控制流程圖

```
使用者觸發功能（頁面 / 圖表 Action / 報表 / 儀表板捷徑 / EditModal Tab）
        │
        ├── 頁面導航（Route）
        │       ↓
        │   GenericIndexPageComponent.OnInitializedAsync()
        │       ↓
        │   CompanyModuleService.IsModuleEnabledAsync()
        │       ├── 停用 → 顯示 🔒 鎖定畫面
        │       └── 啟用 → PagePermissionCheck → 正常顯示
        │
        ├── 圖表等 Action 類型（Action via NavigationActionHandler）
        │       ↓
        │   MainLayout.HandleNavigationAction(actionId)
        │       ↓
        │   查找 navItem.ModuleKey（繼承自父層）
        │       ↓
        │   NavigationPermissionService.CanAccessModuleAsync()
        │       ├── IsSuperAdmin → 直接允許
        │       ├── 模組停用 → 顯示模組停用 Modal
        │       └── 模組啟用且有權限 → 開啟 Modal
        │
        ├── 報表（從報表中心或報表搜尋選擇）
        │       ↓
        │   MainLayout.HandleReportSelected(actionId)
        │       ↓
        │   查找 reportDef.ModuleKey（ReportRegistry 設定）
        │       ↓
        │   NavigationPermissionService.IsModuleEnabledAsync()  ← 公司層
        │       ├── IsSuperAdmin → 直接允許
        │       ├── 模組停用 → 顯示模組停用 Modal
        │       └── 模組啟用 → 繼續
        │                   ↓
        │           NavigationPermissionService.CanAccessAsync()  ← 使用者層
        │                   ├── 無權限 → 靜默 return
        │                   └── 有權限 → 開啟篩選 Modal
        │                               ↓
        │                   GenericReportFilterModalComponent（備援雙重檢查）
        │
        ├── EditModal 中的跨模組 Tab（如車輛資訊）
        │       ↓
        │   InitializeFormFieldsAsync()
        │       ↓
        │   CompanyModuleService.IsModuleEnabledAsync("Vehicles")
        │       ├── 停用且非 IsSuperAdmin → 不加入 Tab（Tab 消失）
        │       └── 啟用或 IsSuperAdmin → 加入 Tab → 正常顯示
        │
        └── 首頁儀表板捷徑
                ↓
            DashboardService.GetEmployeePanelsAsync()
                ↓
            GetEnabledModuleKeysAsync(employeeId)
                ├── IsSuperAdmin (null) → 顯示所有項目
                └── 一般使用者 → 過濾掉停用模組的項目
```

---

## 權限層級說明

### IsSuperAdmin（最高身分，DB 欄位）

`Employee.IsSuperAdmin = true` 的帳號可**繞過所有模組限制**，亦繞過所有使用者層級的功能權限檢查。系統中只有一位，通常為系統設計者或程式設計師。

| 檢查點 | IsSuperAdmin 行為 |
|--------|------------------|
| `NavigationPermissionService.CanAccessModuleAsync()` | 直接回傳 `true`，跳過模組啟用檢查與權限檢查 |
| `NavigationPermissionService.IsModuleEnabledAsync()` | 直接回傳 `true`，跳過公司層級模組檢查 |
| `NavigationPermissionService.IsCurrentEmployeeSuperAdminAsync()` | 回傳 `true` |
| `MainLayout.HandleNavigationAction()` | 透過 `CanAccessModuleAsync()` 自動繞過 |
| `MainLayout.HandleReportSelected()` | 透過 `IsModuleEnabledAsync()` 自動繞過 |
| `DashboardService.GetEnabledModuleKeysAsync()` | 回傳 `null`，所有過濾點跳過模組與權限檢查 |
| `GenericIndexPageComponent` 模組鎖定畫面 | 不顯示（透過 `IsCurrentEmployeeSuperAdminAsync()` 繞過） |
| EditModal 跨模組 Tab（如車輛資訊） | Tab 正常顯示（透過 `IsCurrentEmployeeSuperAdminAsync()` 繞過） |
| 選單過濾 | 停用的模組對應選單仍會顯示 |
| `ModuleManagementTab`（設定介面） | 可見並操作所有模組開關 |

### System.Admin（管理者權限代碼）

擁有 `System.Admin` 權限代碼的帳號，可繞過**使用者層級**的功能權限檢查，但**仍受公司層級模組限制約束**。

| 檢查點 | System.Admin 行為 |
|--------|------------------|
| `NavigationPermissionService.CanAccessModuleAsync()` | 模組啟用才能存取（公司層級限制依然有效） |
| `CanAccessAsync(permission)` | 直接回傳 `true`，跳過特定功能權限檢查 |
| 儀表板小工具/捷徑過濾 | 模組啟用前提下，可看到所有功能 |
| `GenericIndexPageComponent` 模組鎖定畫面 | 仍顯示（不能繞過公司模組限制） |

---

## 新增模組的步驟

1. **在 `NavigationConfig.cs` 加入父層 NavigationItem**，設定 `ModuleKey`：
   ```csharp
   new NavigationItem
   {
       Name = "新功能",
       NameKey = "Nav.NewModule",
       IsParent = true,
       ModuleKey = "NewModule",   // 此值將成為 CompanyModule.ModuleKey
       Children = new List<NavigationItem> { ... }
   }
   ```
   子項目（頁面、報表 Action、圖表 Action）不需要手動設定 `ModuleKey`，會自動繼承。

2. **資料庫初始化時自動建立**：`CompanyModuleSeeder` 會讀取 NavigationConfig，自動寫入 CompanyModules 資料表，預設 `IsEnabled = true`。

3. **在 Index 頁面宣告 RequiredModule**：
   ```razor
   <GenericIndexPageComponent
       RequiredModule="NewModule"
       RequiredPermission="NewModule.Read"
       ... />
   ```

4. **SuperAdmin 可在模組管理介面控制啟用/停用**，效果會同步到頁面、報表、圖表和儀表板。

---

## 快取注意事項

- 模組狀態快取時間：**30 分鐘**（`CompanyModuleService`）
- 使用者權限快取時間：**10 分鐘**（`NavigationPermissionService`）
- SuperAdmin 在 ModuleManagementTab 儲存後會立即呼叫 `ClearCache()`，使變更即時生效
- 若直接修改資料庫，需等快取過期（最多 30 分鐘）才會生效

---

## 相關檔案索引

| 功能 | 檔案路徑 |
|------|----------|
| 實體定義 | `Data/Entities/Systems/CompanyModule.cs` |
| 服務介面 | `Services/Systems/ICompanyModuleService.cs` |
| 服務實作 | `Services/Systems/CompanyModuleService.cs` |
| 複合權限服務 | `Services/Auth/NavigationPermissionService.cs` |
| 資料庫種子 | `Data/SeedDataManager/Seeders/CompanyModuleSeeder.cs` |
| 選單設定（含 ModuleKey 繼承） | `Data/Navigation/NavigationConfig.cs` |
| Index 頁面元件 | `Components/Shared/Page/GenericIndexPageComponent.razor` |
| 權限檢查元件 | `Components/Shared/PagePermissionCheck.razor` |
| 圖表 Action 模組檢查 | `Components/Layout/MainLayout.razor`（`HandleNavigationAction`） |
| 報表模組 + 權限檢查（主） | `Components/Layout/MainLayout.razor`（`HandleReportSelected`） |
| 報表模組 + 權限檢查（備援） | `Components/Shared/Report/GenericReportFilterModalComponent.razor`（`OnParametersSetAsync`） |
| 報表定義（含 ModuleKey） | `Models/Reports/ReportDefinition.cs` |
| 報表模組鍵設定 | `Data/Reports/ReportRegistry.cs` |
| 儀表板模組過濾 | `Services/Dashboard/DashboardService.cs` |
| EditModal Tab — 客戶車輛 | `Components/Pages/Customers/CustomerEditModal/CustomerEditModalComponent.razor` |
| EditModal Tab — 廠商車輛 | `Components/Pages/Suppliers/SupplierEditModal/SupplierEditModalComponent.razor` |
| EditModal Tab — 員工車輛 | `Components/Pages/Employees/EmployeeEditModal/EmployeeEditModalComponent.razor` |
| 管理介面 Tab | `Components/Pages/Systems/SystemParameter/ModuleManagementTab.razor` |
