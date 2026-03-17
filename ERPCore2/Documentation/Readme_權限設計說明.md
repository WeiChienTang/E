# 權限系統設計說明

## 概覽

本系統採用「**單一來源（Single Source of Truth）**」架構管理所有權限定義。
所有權限皆在 `PermissionRegistry.cs` 中以靜態常數宣告，
程式碼各處（頁面、導覽、Seeder、權限管理 UI 分組）皆引用此常數，不使用魔法字串。

---

## 架構流程

```
PermissionRegistry.cs（靜態定義，開發者維護）
         │
         ├─── PermissionSeeder.cs ──→ 資料庫 Permissions 表（啟動時自動同步）
         │                                │
         │                         RolePermission 表（角色-權限對應）
         │                                │
         ├─── NavigationConfig.cs  ──→ 導覽選單過濾
         │
         ├─── PermissionCheckMiddleware.cs ──→ 系統管理路由保護（Middleware 層）
         │
         ├─── 各頁面 RequiredPermission ──→ PagePermissionCheck 元件（頁面層）
         │
         ├─── QuickActionModalHost.razor ──→ 快速功能 Modal 權限守衛（首頁層）
         │
         └─── RolePermissionManagement.razor ──→ 權限管理 UI 分組（讀取 GroupKey）
```

---

## 核心檔案

| 檔案 | 職責 |
|------|------|
| `Models/PermissionRegistry.cs` | 所有權限的唯一定義來源（含分組 GroupKey） |
| `Data/SeedDataManager/Seeders/PermissionSeeder.cs` | 啟動時將 Registry 同步至資料庫 |
| `Services/Auth/PermissionCheckMiddleware.cs` | Middleware 層路由保護 |
| `Data/Navigation/NavigationConfig.cs` | 導覽項目的權限宣告 |
| `Components/Shared/UI/Auth/PagePermissionCheck.razor` | 頁面層權限遮罩元件 |
| `Components/Shared/Dashboard/QuickActionModalHost.razor` | 首頁快速功能 Modal 權限守衛 |
| `Services/Employees/RoleService.cs` | 角色權限指派，負責雙層快取清除 |

---

## PermissionRegistry 結構

### PermissionDefinition

```csharp
// 權限定義 record，含分組歸屬
public record PermissionDefinition(
    string Code,             // 權限編號（如 "SubAccount.Read"）
    string Name,             // 顯示名稱（如 "檢視子科目設定"）
    PermissionLevel Level,   // Normal 或 Sensitive
    string Remarks,          // 說明文字
    string GroupKey = "Nav.SystemGroup"  // 分組歸屬（Nav.* resource key）
);
```

`GroupKey` 決定權限在 `RolePermissionManagement.razor` 中顯示於哪個分組（如「會計管理」、「系統管理」），
統一由 `PermissionRegistry.GetAllPermissions()` 維護，不需在其他地方維護分組對應表。

### 權限常數類別

```csharp
// Models/PermissionRegistry.cs

public static class PermissionRegistry
{
    // 每個模組對應一個巢狀靜態類別
    public static class Customer
    {
        public const string Read = "Customer.Read";
    }

    public static class PurchaseOrder
    {
        public const string Read    = "PurchaseOrder.Read";
        public const string Approve = "PurchaseOrder.Approve";  // 多個動作
    }

    public static class AccountItem
    {
        public const string Read                = "AccountItem.Read";
        public const string SubAccountRead      = "SubAccount.Read";          // 檢視子科目設定
        public const string SubAccountBatchCreate = "SubAccount.BatchCreate";   // 批次補建子科目
    }

    public static class Document
    {
        public const string Read      = "Document.Read";
        public const string Sensitive = "Document.Sensitive";   // Sensitive 層級
        public const string Manage    = "Document.Manage";      // Sensitive 層級
    }

    // ... 其他模組

    // Seeder 呼叫此方法取得完整定義（含名稱、層級、說明、分組）
    public static IEnumerable<PermissionDefinition> GetAllPermissions() => [ ... ];
}
```

### 權限層級（PermissionLevel）

| 層級 | 說明 | 典型用途 |
|------|------|---------|
| `Normal` | 一般權限，可授予一般員工 | `Customer.Read`、`Product.Read` 等 |
| `Sensitive` | 敏感權限，需要更高層級授予 | `System.Admin`、`Permission.Read`、`Document.Manage` 等 |

---

## 新增權限的步驟

只需修改 **一個檔案**：`Models/PermissionRegistry.cs`

### 步驟 1：在 Registry 加入常數

```csharp
// 若已有對應模組的巢狀類別，直接加入 Action
public static class Customer
{
    public const string Read   = "Customer.Read";
    public const string Export = "Customer.Export";  // 新增
}

// 若是全新模組，新增巢狀類別
public static class Logistics
{
    public const string Read = "Logistics.Read";
}
```

### 步驟 2：在 GetAllPermissions() 加入定義（含 GroupKey）

```csharp
// GroupKey 決定權限管理 UI 的分組歸屬
new(Customer.Export,  "匯出客戶資料", PermissionLevel.Normal, "允許將客戶資料匯出為 Excel", "Nav.CustomerGroup"),
new(Logistics.Read,  "檢視物流",     PermissionLevel.Normal, "檢視物流配送相關資訊",       "Nav.InventoryGroup"),
```

> **GroupKey 參考值**：`Nav.SystemGroup`、`Nav.HumanResources`、`Nav.CustomerGroup`、`Nav.SupplierGroup`、`Nav.ProductGroup`、`Nav.InventoryGroup`、`Nav.SalesGroup`、`Nav.PurchaseGroup`、`Nav.FinanceGroup`、`Nav.AccountingGroup`、`Nav.VehicleGroup`、`Nav.WasteGroup`、`Nav.DocumentGroup`

### 步驟 3：重新啟動應用程式

`PermissionSeeder` 會在啟動時自動偵測新權限並寫入資料庫。
無需手動執行任何 SQL 或 Migration。

### 步驟 4：在頁面或導覽中引用

**Razor 頁面：**
```razor
<GenericIndexPageComponent RequiredPermission="@PermissionRegistry.Customer.Export" ... />
```

**NavigationConfig.cs：**
```csharp
new NavigationItem
{
    Route              = "/customers/export",
    RequiredPermission = PermissionRegistry.Customer.Export,
    ...
}
```

---

## 頁面層權限保護（PagePermissionCheck）

大多數 CRUD 頁面使用 `PagePermissionCheck` 元件遮罩內容：

```razor
<PagePermissionCheck RequiredPermission="@PermissionRegistry.Customer.Read">
    <!-- 有權限才看得到的內容 -->
</PagePermissionCheck>
```

`GenericIndexPageComponent` 與 `GenericEditModalComponent` 內建 `RequiredPermission` 參數，
會自動交由 `PagePermissionCheck` 處理，不需要手動包裹。

---

## Middleware 層保護（系統管理路由）

下列路由在 HTTP 請求層即攔截，不依賴 Blazor 元件渲染：

| 路由 | 所需權限 |
|------|---------|
| `/permissions` | `PermissionRegistry.System.Admin` |
| `/roles` | `PermissionRegistry.System.Admin` |
| `/error-logs` | `PermissionRegistry.System.Admin` |
| `/role-permission-management` | `PermissionRegistry.System.Admin` |

一般業務頁面（客戶、品項、採購⋯）由 `PagePermissionCheck` 處理，
不在 Middleware 設定，避免設定分散、難以維護。

---

## 權限管理 UI 分組機制

`RolePermissionManagement.razor` 的權限分組顯示（如「會計管理」、「系統管理」）統一從 `PermissionRegistry.GetAllPermissions()` 的 `GroupKey` 讀取：

```csharp
// RolePermissionManagement.razor — BuildPermissionCategoryMapping()
permissionCodeToCategory = PermissionRegistry.GetAllPermissions()
    .ToDictionary(d => d.Code, d => d.GroupKey);
```

**不再**使用 NavigationConfig 掃描或 supplemental 字典。
新增權限時只需在 `PermissionRegistry.GetAllPermissions()` 中指定正確的 `GroupKey`，權限管理 UI 即自動歸入對應分組。

---

## 子科目權限控制

子科目設定（`SubAccountSettingsTab.razor`）位於「系統參數設定」 Modal 內，採用單獨的權限控制：

| 權限 | Code | 等級 | 控制範圍 |
|------|------|------|----------|
| 檢視子科目設定 | `SubAccount.Read` | Normal | 控制子科目設定 Tab 是否顯示 |
| 批次補建子科目 | `SubAccount.BatchCreate` | **Sensitive** | 控制批次補建按鈕是否顯示 |

### 控制流程

```
SystemParameterSettingsModal.OnInitializedAsync()
    ├─ 檢查 SubAccount.Read
    │     └─ 有權限（或 SuperAdmin）→ 加入子科目設定 Tab
    ├─ 檢查 SubAccount.BatchCreate
    │     └─ 有權限 → SubAccountSettingsTab 顯示批次補建按鈕
    └─ 無權限 → 只能查看設定，不能執行批次操作
```

> 此機制從「系統參數存取」（`SystemParameter.Read`）中分離出「會計子科目」權限，讓會計人員可獨立取得權限。

---

## 權限指派（角色管理）

透過 `RolePermissionManagement.razor` 頁面（系統管理 → 角色管理），
管理員可為每個角色勾選要授予的權限。

- 一般權限可直接勾選
- 敏感權限（Sensitive）需額外確認對話框
- `System.Admin` 角色擁有所有權限，不可修改

---

## 權限快取機制

### 雙層快取架構

系統有兩個獨立的權限快取層，各自負責不同的查詢路徑：

| 快取層 | 服務 | Cache Key | TTL | 清除方法 |
|--------|------|-----------|-----|---------|
| 第一層 | `PermissionService` | `employee_permissions_{id}` | 30 分鐘 | `RefreshEmployeePermissionCacheAsync(employeeId)` |
| 第二層 | `NavigationPermissionService` | `all_nav_perms_{id}` | 10 分鐘 | `ClearEmployeePermissionCache(employeeId)` |
| | | `is_superadmin_{id}` | 10 分鐘 | （同上，一起清除） |

`PagePermissionCheck` 元件（頁面層）呼叫的是 **第二層**（`NavigationPermissionService.CanAccessAsync`）。

### 快取清除時機

`RoleService.AssignPermissionsToRoleAsync` 儲存角色權限後，會透過 `ClearRoleEmployeePermissionCacheAsync` **同時清除兩層快取**：

```csharp
foreach (var employeeId in employeeIds)
{
    await _permissionService.RefreshEmployeePermissionCacheAsync(employeeId);      // 第一層
    _navigationPermissionService?.ClearEmployeePermissionCache(employeeId);        // 第二層
}
```

> **重要**：若只清除第一層快取，`PagePermissionCheck` 仍會讀取第二層的舊快取，導致角色權限修改後最長 10 分鐘內無法生效。

---

## 快速功能（QuickAction）權限保護

`QuickActionModalHost.razor` 在首頁提供額外的 Modal 層權限守衛：

### 保護機制

`OnParametersSetAsync` 在預渲染每個 Modal 前，讀取 `_quickActionPermissions`（從 `NavigationConfig.GetQuickActionWidgetItems()` 衍生）進行權限檢查：

```csharp
// 有 RequiredPermission 的 QuickAction，未授權則不加入 _activeActionIds
if (!await NavigationPermissionService.CanAccessAsync(perm))
    continue;
```

未通過檢查的 Modal 不會被 DynamicComponent 渲染，`Open()` 也會同步拒絕開啟。

### 多層防護

即便 QuickActionModalHost 的守衛失效，每個 EditModal 仍透過 `GenericEditModalComponent` 內建的 `RequiredPermission` 參數作為最後一道防線（預設拒絕無 `System.Admin` 的使用者）。

---

## 設計原則

1. **單一來源**：所有權限 Code 及分組歸屬只在 `PermissionRegistry.cs` 定義一次
2. **編譯時期安全**：字串拼錯會產生編譯錯誤，而非靜默失效
3. **自動同步**：新增權限後重啟即可，Seeder 負責補差異
4. **僅追加**：Seeder 只新增不存在的 Code，不修改現有記錄，不影響已指派的角色權限
5. **雙層快取同步清除**：`RoleService` 同時清除兩層快取，確保角色權限修改立即生效
6. **分組自動化**：`GroupKey` 隨權限定義一起維護，權限管理 UI 無需額外維護分組對應表
