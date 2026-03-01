# 權限系統設計說明

## 概覽

本系統採用「**單一來源（Single Source of Truth）**」架構管理所有權限定義。
所有權限皆在 `PermissionRegistry.cs` 中以靜態常數宣告，
程式碼各處（頁面、導覽、Seeder）皆引用此常數，不使用魔法字串。

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
         └─── 各頁面 RequiredPermission ──→ PagePermissionCheck 元件（頁面層）
```

---

## 核心檔案

| 檔案 | 職責 |
|------|------|
| `Models/PermissionRegistry.cs` | 所有權限的唯一定義來源 |
| `Data/SeedDataManager/Seeders/PermissionSeeder.cs` | 啟動時將 Registry 同步至資料庫 |
| `Services/Auth/PermissionCheckMiddleware.cs` | Middleware 層路由保護 |
| `Data/Navigation/NavigationConfig.cs` | 導覽項目的權限宣告 |
| `Components/Shared/UI/Auth/PagePermissionCheck.razor` | 頁面層權限遮罩元件 |

---

## PermissionRegistry 結構

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

    public static class Document
    {
        public const string Read      = "Document.Read";
        public const string Sensitive = "Document.Sensitive";   // Sensitive 層級
        public const string Manage    = "Document.Manage";      // Sensitive 層級
    }

    // ... 其他模組

    // Seeder 呼叫此方法取得完整定義（含名稱、層級、說明）
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

### 步驟 2：在 GetAllPermissions() 加入定義

```csharp
new(Customer.Export, "匯出客戶資料", PermissionLevel.Normal, "允許將客戶資料匯出為 Excel"),
new(Logistics.Read,  "檢視物流",     PermissionLevel.Normal, "檢視物流配送相關資訊"),
```

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

一般業務頁面（客戶、商品、採購⋯）由 `PagePermissionCheck` 處理，
不在 Middleware 設定，避免設定分散、難以維護。

---

## 權限指派（角色管理）

透過 `RolePermissionManagement.razor` 頁面（系統管理 → 角色管理），
管理員可為每個角色勾選要授予的權限。

- 一般權限可直接勾選
- 敏感權限（Sensitive）需額外確認對話框
- `System.Admin` 角色擁有所有權限，不可修改

---

## 設計原則

1. **單一來源**：所有權限 Code 只在 `PermissionRegistry.cs` 定義一次
2. **編譯時期安全**：字串拼錯會產生編譯錯誤，而非靜默失效
3. **自動同步**：新增權限後重啟即可，Seeder 負責補差異
4. **僅追加**：Seeder 只新增不存在的 Code，不修改現有記錄，不影響已指派的角色權限
