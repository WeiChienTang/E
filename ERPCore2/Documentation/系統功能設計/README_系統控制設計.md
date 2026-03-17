# 系統控制設計

> 最後更新：2026-02-24
> 專案：ERPCore2

---

## 一、架構總覽

ERPCore2 採用**雙層存取控制**架構：

```
第一層：公司層級控制（CompanyModule）
  └─ 此功能模組是否對此公司開放？
      → 由 SuperAdmin 透過「模組管理」Tab 設定

第二層：使用者層級控制（Permission / RolePermission）
  └─ 此用戶是否有此功能的操作權限？
      → 由管理員透過角色與權限指派設定
```

兩層檢查皆通過，使用者才能正常使用功能。SuperAdmin（`Employee.IsSuperAdmin = true`）可繞過第一層限制。

### 分層結構圖

```
Entity（資料模型）
  ├─ CompanyModule.cs         ← 公司層級模組控制
  ├─ Permission.cs            ← 使用者層級功能權限
  └─ RolePermission.cs        ← 角色與權限對應

Model（導航模型）
  └─ NavigationItem.cs        ← 新增 ModuleKey 屬性（對應 CompanyModule）

Config（導航配置）
  └─ NavigationConfig.cs      ← 父級選單設定 ModuleKey，作為模組唯一來源

Service（商業邏輯）
  ├─ ICompanyModuleService.cs
  ├─ CompanyModuleService.cs  ← 模組啟用查詢（含 30 分鐘快取）
  └─ NavigationPermissionService.cs ← 整合雙層檢查

Seeder（種子資料）
  └─ CompanyModuleSeeder.cs   ← 從 NavigationConfig 自動衍生模組記錄

UI（使用者介面）
  ├─ SystemParameter/                    ← 系統參數設定（多檔拆分）
  │   ├─ SystemParameterSettingsModal.razor  ← 主檔（Modal 外殼、Tab 切換、CRUD）
  │   ├─ TaxSettingsTab.razor                ← 稅務設定 Tab
  │   ├─ SubAccountSettingsTab.razor         ← 子科目設定 Tab
  │   ├─ CertificateTab.razor                ← 安全憑證 Tab（目前停用）
  │   └─ ModuleManagementTab.razor           ← 模組管理 Tab（SuperAdmin 限定）
  ├─ GenericIndexPageComponent.razor（頁面層級封鎖）
  └─ NavigationPermissionCheck.razor（導航列隱藏）
```

---

## 二、公司層級模組控制（CompanyModule）

### 2-1. Entity 定義

位置：`Data/Entities/Systems/CompanyModule.cs`

繼承自 `BaseEntity`，包含以下業務欄位：

| 欄位名稱 | 型別 | 預設值 | 說明 |
|---------|------|--------|------|
| ModuleKey | string | - | 模組識別鍵（唯一索引），對應頁面目錄名稱 |
| DisplayName | string | - | 模組顯示名稱（如「財務管理」） |
| Description | string? | null | 模組說明文字 |
| IsEnabled | bool | true | 是否啟用此模組 |
| SortOrder | int | 0 | 排序順序（數字越小越靠前） |

`ModuleKey` 建有唯一索引（`[Index(nameof(ModuleKey), IsUnique = true)]`），確保一個模組只有一筆記錄。

### 2-2. 預設模組清單

由 `CompanyModuleSeeder` 從 `NavigationConfig.cs` 父級選單自動衍生，初始全部啟用：

| ModuleKey | DisplayName | 對應 NavigationConfig MenuKey |
|-----------|------------|-------------------------------|
| Employees | 人力管理 | employee_management |
| Suppliers | 供應鏈管理 | supplier_management |
| Customers | 客戶關係管理 | customer_management |
| Products | 品項管理 | product_management |
| Warehouse | 庫存管理 | inventory_management |
| Purchase | 採購管理 | purchase_management |
| Sales | 銷售管理 | sales_management |
| Vehicles | 車輛管理 | vehicle_management |
| WasteManagement | 磅秤管理 | waste_management |
| FinancialManagement | 財務管理 | financial_management |
| Accounting | 會計管理 | accounting_management |

> **注意**：`system_management` 不設定 `ModuleKey`，系統管理不可被停用。

### 2-3. 自動衍生機制（CompanyModuleSeeder）

位置：`Data/SeedDataManager/Seeders/CompanyModuleSeeder.cs`

遵循與 `ReportPrintConfigurationSeeder`（從 `ReportRegistry` 衍生）相同的設計模式：

```
應用程式啟動
    │
    ▼
SeedData.InitializeAsync()
    │
    ▼
CompanyModuleSeeder.SeedAsync()
    │
    ├─ 從 NavigationConfig.GetAllNavigationItems() 篩選
    │   IsParent = true 且 ModuleKey 非空的項目
    │
    ├─ 查詢資料庫已存在的 ModuleKey
    │
    ├─ 比對後僅新增尚未存在的模組（insert-if-not-exists）
    │   ├─ ModuleKey ← NavigationItem.ModuleKey
    │   ├─ DisplayName ← NavigationItem.Category（如「財務管理」）
    │   ├─ Description ← NavigationItem.Description
    │   ├─ IsEnabled ← true（預設啟用）
    │   └─ SortOrder ← 自動遞增（從現有最大值 + 10）
    │
    └─ 批次寫入資料庫
```

**設計要點**：
- `Order = 5`，在 PermissionSeeder 等基礎 seeder 之前執行
- 不會修改已存在的模組記錄（包括 Migration 建立的初始 12 筆）
- 新增模組預設啟用，SuperAdmin 可隨時在模組管理 Tab 停用

### 2-4. NavigationItem.ModuleKey 屬性

位置：`Models/Navigation/NavigationItem.cs`

```csharp
/// <summary>
/// 對應的公司模組識別鍵（僅父級選單使用）
/// 設定後 CompanyModuleSeeder 會自動建立對應的 CompanyModule 記錄
/// 未設定表示此選單群組不受模組啟用控制（如系統管理）
/// </summary>
public string? ModuleKey { get; set; }
```

NavigationConfig 中的設定範例：

```csharp
new NavigationItem
{
    Name = "會計",
    Description = "會計相關功能管理",
    IsParent = true,
    MenuKey = "accounting_management",
    ModuleKey = "Accounting",              // ← Seeder 自動建立對應 CompanyModule
    ...
}
```

### 2-5. Service 介面

位置：`Services/Systems/ICompanyModuleService.cs`

```csharp
public interface ICompanyModuleService
{
    Task<List<CompanyModule>> GetAllAsync();
    Task<bool> IsModuleEnabledAsync(string moduleKey);
    Task<ServiceResult> UpdateModulesAsync(List<CompanyModule> modules, string updatedBy);
    void ClearCache();
}
```

### 2-6. Service 實作重點

位置：`Services/Systems/CompanyModuleService.cs`

- 使用 `IDbContextFactory<AppDbContext>` 建立 DbContext（符合 Blazor Server 模式）
- 模組啟用狀態以 `Dictionary<string, bool>` 快取於記憶體，快取有效期 **30 分鐘**
- 快取鍵值：`"CompanyModules_IsEnabled"`
- `IsModuleEnabledAsync` 安全預設：若資料庫無此模組記錄，回傳 `true`（允許存取），避免因遺漏設定而誤封鎖
- `UpdateModulesAsync` 批次更新後自動呼叫 `ClearCache()` 使快取失效

---

## 三、使用者層級權限控制

### 3-1. NavigationPermissionService

位置：`Services/Auth/NavigationPermissionService.cs`

整合雙層存取控制的核心服務，主要方法：

| 方法 | 說明 |
|------|------|
| `CanAccessAsync(permission)` | 檢查使用者是否有特定功能權限（使用者層級） |
| `CanAccessModuleAsync(module)` | 整合雙層檢查（公司層級 + 使用者層級） |
| `GetCurrentEmployeeIdAsync()` | 從 JWT Claim 取得當前員工 ID |
| `GetAllEmployeePermissionsAsync(employeeId)` | 批次取得員工所有權限（含 10 分鐘快取） |
| `ClearEmployeePermissionCache(employeeId)` | 手動清除員工權限快取 |

### 3-2. CanAccessModuleAsync 雙層檢查流程

```
呼叫 CanAccessModuleAsync("FinancialManagement")
    │
    ▼
取得當前員工 ID（EmployeeId）
    │
    ▼
批次載入員工所有權限（含快取）
    │
    ├─── 含有 "System.Admin" 權限？
    │         ↓ 是
    │    直接回傳 true（SuperAdmin 不受模組限制）
    │
    ▼ 否
檢查 CompanyModuleService.IsModuleEnabledAsync("FinancialManagement")
    │
    ├─── 模組未啟用？
    │         ↓ 是
    │    回傳 false（公司層級封鎖）
    │
    ▼ 否（模組已啟用）
檢查使用者是否有 "FinancialManagement.*" 開頭的任何權限
    │
    └─── 回傳 true / false（使用者層級結果）
```

### 3-3. SuperAdmin 識別方式

SuperAdmin 在不同場景有兩種識別方式：

| 場景 | 識別方式 | 說明 |
|------|---------|------|
| **導航/頁面存取**（NavigationPermissionService） | `System.Admin` 權限 | 使用權限快取系統，判斷是否繞過模組限制 |
| **功能 UI 控制**（如模組管理 Tab 顯示） | `Employee.IsSuperAdmin` 欄位 | 透過 `EmployeeService.IsSuperAdminAsync()` 查詢資料庫，僅 Seeder 設定的超級管理員才為 true |

`Employee.IsSuperAdmin` 欄位由 Seeder 設定，Service 層會保護此欄位不被前端修改，確保只有真正的超級管理員才能存取敏感功能（如模組管理）。

---

## 四、三層防護機制

即使使用者直接在瀏覽器輸入 URL，也無法繞過權限檢查。防護由外而內共三層：

```
[第一層] NavigationPermissionCheck.razor
  ← 隱藏導航列中無權限的選單項目
  ← 依賴 CanAccessModuleAsync() 進行雙層判斷

[第二層] GenericIndexPageComponent.razor
  ← OnInitializedAsync() 在每次頁面載入時執行（含直接輸入 URL）
  ← RequiredModule 參數為空時跳過模組檢查
  ← 模組未啟用時顯示「此功能未開放」畫面，不載入頁面資料
  ← RequiredPermission 交由 PagePermissionCheck 處理使用者層級

[第三層] Service 層（如 JournalEntryService）
  ← 最終防線，即使 UI 被繞過，Service 也可加入業務邏輯驗證
```

### GenericIndexPageComponent 模組封鎖畫面

當 `RequiredModule` 對應的模組被停用時，頁面顯示：

```
[🔒 鎖定圖示]

此功能未開放

您的系統目前未啟用此功能模組，請聯絡系統管理員。
```

整個 `<PagePermissionCheck>` 區塊不會渲染，確保不洩漏任何頁面內容。

---

## 五、各頁面 RequiredModule 對照表

所有 Index 頁面（共 46 個）均已設定 `RequiredModule` 參數：

| ModuleKey | 涵蓋頁面 |
|-----------|---------|
| Customers | CustomerIndex |
| Suppliers | SupplierIndex |
| Products | ProductIndex, ProductCategoryIndex, UnitIndex, ProductCompositionIndex |
| Purchase | PurchaseOrderIndex, PurchaseReceivingIndex, PurchaseReturnIndex |
| Sales | QuotationIndex, SalesOrderIndex, SalesShipmentIndex, SalesReturnIndex, CollectionIndex |
| Warehouse | WarehouseIndex, InventoryItemIndex, InventoryCheckIndex, InventoryTransferIndex, MaterialRequisitionIndex, ShipmentIndex |
| FinancialManagement | AccountItemIndex, JournalEntryIndex, PaymentIndex, ReceiptIndex, OffsetIndex, SubAccountIndex |
| ProductionManagement | ProductionScheduleIndex, BomIndex, BomDetailIndex, ProductionOrderIndex, WorkOrderIndex, ProductionCompletionIndex |
| Employees | EmployeeIndex, DepartmentIndex, PositionIndex, RoleIndex, PermissionIndex |
| Vehicles | VehicleIndex, VehicleMaintenanceIndex, VehicleTypeIndex |
| WasteManagement | WasteTypeIndex, WasteRecordIndex |
| Systems | SystemParameterIndex, CompanyIndex, CompanyModuleIndex, ErrorLogIndex（無 RequiredModule） |

> **注意**：`ErrorLogIndex.razor` 不設定 `RequiredModule`，確保系統錯誤記錄永遠可存取。

### Index 頁面使用範例

```razor
<GenericIndexPageComponent TEntity="AccountItem"
                           TService="IAccountItemService"
                           RequiredPermission="FinancialManagement.AccountItem.View"
                           RequiredModule="FinancialManagement"
                           ...>
```

---

## 六、模組管理 UI（SuperAdmin 專屬）

### 6-1. 系統參數設定（多檔拆分架構）

位置：`Components/Pages/Systems/SystemParameter/`

系統參數設定 Modal 已拆分為多個獨立元件，主檔負責 Modal 外殼與 Tab 切換，各 Tab 內容獨立維護：

| 檔案 | 說明 |
|------|------|
| `SystemParameterSettingsModal.razor` | 主檔：Modal 外殼、Tab 切換（使用 `GenericFormComponent` + `CustomContent`）、共用 CRUD 操作 |
| `TaxSettingsTab.razor` | 稅務設定 Tab：稅率、備註（內嵌獨立的 `GenericFormComponent`） |
| `SubAccountSettingsTab.razor` | 子科目設定 Tab：自動產生設定、統制科目代碼、批次補建子科目（需 `SubAccount.Read` 權限才可見；批次補建區塊需 `SubAccount.BatchCreate`） |
| `CertificateTab.razor` | 安全憑證 Tab（目前停用，預留未來啟用） |
| `ModuleManagementTab.razor` | 模組管理 Tab（僅 SuperAdmin 可見） |

### 6-2. 模組管理 Tab

- Tab 名稱：「模組管理」，僅 SuperAdmin 可見
- SuperAdmin 判斷：`EmployeeService.IsSuperAdminAsync()`（檢查 `Employee.IsSuperAdmin` 資料庫欄位）
- 顯示所有模組的開關清單（依 SortOrder 排序）
- 每個模組顯示：模組名稱、說明文字、啟用/停用 Toggle
- 儲存按鈕呼叫 `CompanyModuleService.UpdateModulesAsync()`，儲存後顯示成功通知

### 6-3. 操作流程

```
SuperAdmin 開啟系統設定
    │
    ▼
點擊「模組管理」Tab
    │
    ▼
顯示所有模組開關清單（從 CompanyModuleSeeder 自動衍生的模組）
    │
    ▼
切換特定模組的啟用狀態
    │
    ▼
點擊「儲存模組設定」
    │
    ▼
CompanyModuleService.UpdateModulesAsync()
    ├─ 批次更新 IsEnabled 至資料庫
    └─ 清除 30 分鐘快取
    │
    ▼
NotificationService.ShowSuccessAsync("模組設定已儲存")
```

---

## 七、快取策略

| 快取層級 | 快取鍵 | 有效期 | 清除時機 |
|---------|-------|--------|---------|
| 模組啟用狀態 | `CompanyModules_IsEnabled` | 30 分鐘 | `UpdateModulesAsync()` 後 |
| 員工所有權限 | `all_nav_perms_{employeeId}` | 10 分鐘 | `ClearEmployeePermissionCache(id)` |

快取使用 `IMemoryCache`（記憶體快取），應用程式重啟後自動失效。

---

## 八、新增模組時的擴充步驟

若系統需要新增功能模組，依序執行以下步驟：

1. **在 NavigationConfig 新增父級選單**：設定 `IsParent = true`、`MenuKey`、`ModuleKey`
2. **重啟應用**：`CompanyModuleSeeder` 自動從 NavigationConfig 衍生新的 CompanyModule 記錄（insert-if-not-exists）
3. **建立頁面目錄**：在 `Components/Pages/[ModuleKey]/` 建立對應頁面
4. **設定 RequiredModule**：所有新頁面的 `GenericIndexPageComponent` 加入 `RequiredModule="[ModuleKey]"`
5. **定義權限**：在 Permission Seeder 新增該模組的功能權限（格式：`[ModuleKey].[Feature].[Action]`）

> **重要**：不需要手動新增 Migration 來插入 CompanyModule 記錄，Seeder 會自動處理。

### 範例：新增「排班管理」模組

```csharp
// Step 1: NavigationConfig.cs 新增父級選單
new NavigationItem
{
    Name = "排班",
    Description = "排班相關功能管理",
    Route = "#",
    IconClass = "bi bi-calendar-week",
    Category = "排班管理",
    IsParent = true,
    MenuKey = "scheduling_management",
    ModuleKey = "Scheduling",          // ← Seeder 自動建立 CompanyModule
    Children = new List<NavigationItem> { ... }
}

// Step 2: 重啟應用 → CompanyModuleSeeder 自動建立：
//   ModuleKey = "Scheduling"
//   DisplayName = "排班管理"  (取自 Category)
//   IsEnabled = true          (預設啟用)

// Step 3-4: 頁面設定
// <GenericIndexPageComponent RequiredModule="Scheduling" ... />
```

---

## 九、設計決策說明

### Q：為何不直接在 SystemParameter 加欄位，而是獨立建立 CompanyModule？

SystemParameter 的定位是「全域營運設定」（稅率、簽核開關等），屬單一設定記錄；而模組控制需要管理多筆記錄（一個模組一筆），兩者資料結構不同。獨立 Entity 讓模組管理更清晰，且未來可擴充模組的其他屬性（如版本、授權日期等）。

### Q：為何 GenericEditModalComponent 不需要模組檢查？

Edit Modal 永遠由 Index 頁面的事件觸發渲染，不存在直接 URL 存取的問題。只要 Index 頁面受到保護，Modal 就不會被渲染。避免重複的防護邏輯，符合最小複雜度原則。

### Q：模組停用後，已存在的資料是否受影響？

不受影響。模組停用僅阻擋 UI 存取，資料庫中的資料完整保留。重新啟用模組後，資料可立即存取。

### Q：IsModuleEnabledAsync 找不到模組時為何回傳 true？

安全預設（fail-open）的設計：若管理員尚未在資料庫建立某模組記錄，系統預設允許存取，避免因設定遺漏而意外封鎖正當用戶。反之，若需要嚴格封鎖（fail-closed），則需確保所有模組均已種子資料建立。

### Q：為何改用 Seeder 而非 Migration 管理模組記錄？

Migration 適合結構變更（建立資料表），但不適合隨業務演進持續新增的資料。Seeder 具備以下優勢：

- **自動衍生**：從 NavigationConfig 自動建立模組，與 `ReportPrintConfigurationSeeder`（從 `ReportRegistry` 衍生）採用相同模式
- **insert-if-not-exists**：每次啟動只新增缺少的模組，不影響已存在的記錄（包括 IsEnabled 狀態）
- **自動恢復**：即使手動刪除資料庫記錄，下次啟動會自動補回
- **零維護成本**：新增模組只需在 NavigationConfig 加上 `ModuleKey`，無需額外寫 Migration

---

## 十、檔案異動清單

| 檔案 | 異動類型 | 說明 |
|------|---------|------|
| `Data/Entities/Systems/CompanyModule.cs` | 新增 | 公司層級模組控制 Entity |
| `Data/Context/AppDbContext.cs` | 修改 | 新增 `DbSet<CompanyModule>` |
| `Migrations/AddCompanyModule.cs` | 新增 | 建立資料表（含初始 12 筆種子資料）|
| `Models/Navigation/NavigationItem.cs` | 修改 | 新增 `ModuleKey` 屬性（對應 CompanyModule） |
| `Data/Navigation/NavigationConfig.cs` | 修改 | 11 個父級選單設定 `ModuleKey` |
| `Data/SeedDataManager/Seeders/CompanyModuleSeeder.cs` | 新增 | 從 NavigationConfig 自動衍生模組記錄 |
| `Data/SeedData.cs` | 修改 | 註冊 `CompanyModuleSeeder`（兩個分支皆加入） |
| `Services/Systems/ICompanyModuleService.cs` | 新增 | 模組管理服務介面 |
| `Services/Systems/CompanyModuleService.cs` | 新增 | 服務實作（含 30 分鐘快取） |
| `Services/Auth/NavigationPermissionService.cs` | 修改 | `CanAccessModuleAsync` 整合雙層檢查 |
| `Data/ServiceRegistration.cs` | 修改 | 注冊 `ICompanyModuleService` |
| `Components/Pages/Systems/SystemParameter/` | 重構 | 系統參數設定拆分為多檔案架構（主檔 + 4 個 Tab 元件） |
| `Services/Employees/IEmployeeService.cs` | 修改 | 新增 `IsSuperAdminAsync()` 介面方法 |
| `Services/Employees/EmployeeService.cs` | 修改 | 公開 `IsSuperAdminAsync()` 實作 |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | 修改 | 新增 `RequiredModule` 參數與封鎖畫面 |
| 全部 46 個 Index 頁面 | 修改 | 加入 `RequiredModule` 參數 |
