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

兩層檢查皆通過，使用者才能正常使用功能。SuperAdmin（持有 `System.Admin` 權限）可繞過第一層限制。

### 分層結構圖

```
Entity（資料模型）
  ├─ CompanyModule.cs         ← 公司層級模組控制
  ├─ Permission.cs            ← 使用者層級功能權限
  └─ RolePermission.cs        ← 角色與權限對應

Service（商業邏輯）
  ├─ ICompanyModuleService.cs
  ├─ CompanyModuleService.cs  ← 模組啟用查詢（含 30 分鐘快取）
  └─ NavigationPermissionService.cs ← 整合雙層檢查

UI（使用者介面）
  ├─ SystemParameterSettingsModal.razor（模組管理 Tab，SuperAdmin 限定）
  ├─ GenericIndexPageComponent.razor（頁面層級封鎖）
  └─ NavigationPermissionCheck.razor（導航列隱藏）

Seeder（種子資料）
  └─ Migration AddCompanyModule（12 個預設模組，全部啟用）
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

由 Migration `AddCompanyModule` 種子資料寫入，初始全部啟用：

| ModuleKey | DisplayName | SortOrder |
|-----------|------------|-----------|
| Customers | 客戶管理 | 10 |
| Suppliers | 廠商管理 | 20 |
| Products | 商品管理 | 30 |
| Purchase | 採購管理 | 40 |
| Sales | 銷售管理 | 50 |
| Warehouse | 倉庫管理 | 60 |
| FinancialManagement | 財務管理 | 70 |
| ProductionManagement | 生產管理 | 80 |
| Employees | 員工管理 | 90 |
| Vehicles | 車輛管理 | 100 |
| WasteManagement | 廢棄物管理 | 110 |
| Reports | 報表 | 120 |

### 2-3. Service 介面

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

### 2-4. Service 實作重點

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
    ├─── 含有 "System.Admin"？
    │         ↓ 是
    │    直接回傳 true（SuperAdmin 不受限）
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

SuperAdmin 透過**擁有 `System.Admin` 權限**來識別，而非 `IsSuperAdmin` 資料庫欄位。
這是因為 `IsSuperAdmin` 未存入 JWT Claim，使用現有的權限快取系統更一致且高效。

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

### 6-1. SystemParameterSettingsModal 模組管理 Tab

位置：`Components/Pages/Systems/SystemParameterSettingsModal.razor`

- Tab 名稱：「模組管理」，僅 SuperAdmin 可見
- SuperAdmin 判斷：`NavigationPermissionService.CanAccessAsync("System.Admin")`
- 顯示所有模組的開關清單（依 SortOrder 排序）
- 每個模組顯示：模組名稱、說明文字、啟用/停用 Toggle
- 儲存按鈕呼叫 `CompanyModuleService.UpdateModulesAsync()`，儲存後顯示成功通知

### 6-2. 操作流程

```
SuperAdmin 開啟系統設定
    │
    ▼
點擊「模組管理」Tab
    │
    ▼
顯示 12 個模組開關清單
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

1. **新增 Migration**：在 `CompanyModules` 資料表插入新模組記錄（`ModuleKey`、`DisplayName`、`SortOrder`）
2. **建立頁面目錄**：在 `Components/Pages/[ModuleKey]/` 建立對應頁面
3. **設定 RequiredModule**：所有新頁面的 `GenericIndexPageComponent` 加入 `RequiredModule="[ModuleKey]"`
4. **設定導航**：在 `NavMenu.razor` 新增對應導航項目，設定 `ModuleKey` 參數供 `NavigationPermissionCheck` 使用
5. **定義權限**：在 Permission Seeder 新增該模組的功能權限（格式：`[ModuleKey].[Feature].[Action]`）

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

---

## 十、檔案異動清單

| 檔案 | 異動類型 | 說明 |
|------|---------|------|
| `Data/Entities/Systems/CompanyModule.cs` | 新增 | 公司層級模組控制 Entity |
| `Data/Context/AppDbContext.cs` | 修改 | 新增 `DbSet<CompanyModule>` |
| `Migrations/AddCompanyModule.cs` | 新增 | 建立資料表 + 種子資料（12 個模組） |
| `Services/Systems/ICompanyModuleService.cs` | 新增 | 模組管理服務介面 |
| `Services/Systems/CompanyModuleService.cs` | 新增 | 服務實作（含 30 分鐘快取） |
| `Services/Auth/NavigationPermissionService.cs` | 修改 | `CanAccessModuleAsync` 整合雙層檢查 |
| `Data/ServiceRegistration.cs` | 修改 | 注冊 `ICompanyModuleService` |
| `Components/Pages/Systems/SystemParameterSettingsModal.razor` | 修改 | 新增「模組管理」Tab（SuperAdmin 限定） |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | 修改 | 新增 `RequiredModule` 參數與封鎖畫面 |
| 全部 46 個 Index 頁面 | 修改 | 加入 `RequiredModule` 參數 |
