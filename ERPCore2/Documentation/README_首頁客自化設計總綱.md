# 首頁客自化設計

## 概述

讓每位使用者可以自訂首頁，首頁分為兩大區塊：

- **頁面捷徑**：卡片只顯示圖示與名稱，點擊後導航至對應頁面或開啟全域 Modal
- **快速功能**：卡片點擊後直接在首頁開啟業務 EditModal（新增模式），不離開首頁

```
┌──────────────────────────────────────────┐
│ 頁面捷徑                    [編輯] │
│ ─────────────────────────────────────── │
│ [員工管理] [客戶管理] [廠商管理] ...     │
│                                          │
│ 快速功能                    [編輯] │
│ ─────────────────────────────────────── │
│ [新增採購單] [新增訂單] ...              │
└──────────────────────────────────────────┘
```

---

## 設計原則

1. **單一資料來源**：以 `NavigationConfig` 作為可選項目的唯一來源，不需要維護額外的資料表或 Seeder
2. **每人獨立配置**：每位員工有自己的首頁佈局，互不影響
3. **權限自動過濾**：使用者只能看到自己有權限的功能
4. **預設配置**：新使用者首次登入自動套用預設組合（兩個區塊各自有預設清單）
5. **自動同步**：NavigationConfig 新增項目後，立即成為可選項目（無需任何資料庫操作）
6. **雙區塊獨立管理**：頁面捷徑與快速功能各自有獨立的編輯、重置、拖曳排序
7. **QuickAction 與 NavigationItem 合一**：快速功能不是獨立項目，而是現有導航項目的附加屬性，避免重複維護

---

## 三種導航項目類型

| 類型 | 枚舉值 | 處理層級 | 用途 |
|------|--------|---------|------|
| Route | `NavigationItemType.Route` | `NavigationManager.NavigateTo()` | 導航至 Index 頁面 |
| Action | `NavigationItemType.Action` | MainLayout 的 `CascadingParameter` | 開啟全域 Modal（報表中心、權限管理等） |
| QuickAction | `NavigationItemType.QuickAction` | Home.razor 自行處理 | 在首頁直接開啟業務 EditModal（新增模式） |

**Action vs QuickAction 的關鍵差異**：Action 的 Modal 宣告在 MainLayout（全站可用），QuickAction 的 Modal 宣告在 Home.razor（僅首頁使用）。

**QuickAction 是衍生的**：NavigationConfig 裡不存在 `ItemType = QuickAction` 的項目。QuickAction 項目是從設有 `QuickActionId` 屬性的現有 Route/Action 項目自動衍生而來。

---

## QuickAction 合一設計

### 核心概念

幾乎每個功能性的 NavigationItem 都有對應的 EditModal，因此 QuickAction 能力是 NavigationItem 本身的可選屬性，而非另外建立一筆項目。

### NavigationItem 的 QuickAction 屬性

| 屬性 | 型別 | 說明 |
|------|------|------|
| `QuickActionId` | string? | 設定後表示此項目支援快速功能，Home.razor 透過此 Id 對應 Modal |
| `QuickActionName` | string? | 快速功能顯示名稱（預設「新增」+ Name） |
| `QuickActionDescription` | string? | 快速功能描述（預設「快速開啟{Name}新增畫面」） |
| `QuickActionIconClass` | string? | 快速功能圖示（預設 `"bi bi-plus-circle-fill"`） |

### 自動衍生機制

`NavigationConfig.DeriveQuickActionItems()` 私有方法會：
1. 掃描所有扁平化的導航項目
2. 篩選出 `QuickActionId` 不為空的項目
3. 為每個符合條件的項目產生一個 `ItemType = QuickAction` 的衍生 NavigationItem
4. 衍生項目繼承原項目的 `Category`、`RequiredPermission` 等屬性

這表示在 NavigationConfig 裡只需維護一筆資料，QuickAction 的顯示資訊由可選屬性控制或自動產生預設值。

---

## 架構設計

### 資料來源

```
NavigationConfig（靜態類）
  ├─ GetAllNavigationItems()        → 取得完整導航樹
  ├─ GetDashboardWidgetItems()      → 取得所有可作為捷徑的項目（Route + Action + 衍生 QuickAction）
  ├─ GetShortcutWidgetItems()       → 取得頁面捷徑項目（Route + Action）
  ├─ GetQuickActionWidgetItems()    → 取得快速功能項目（衍生 QuickAction）
  ├─ DeriveQuickActionItems()       → [私有] 從有 QuickActionId 的項目自動衍生 QuickAction
  ├─ GetNavigationItemByKey(key)    → 根據識別鍵取得導航項目
  └─ GetCategoryIcon(category)      → 根據分類取得對應圖示（備援用）

DashboardDefaults（靜態類）
  ├─ DefaultWidgetKeys              → 預設頁面捷徑識別鍵清單
  ├─ DefaultQuickActionKeys         → 預設快速功能識別鍵清單
  ├─ GetNavigationItemKey(item)     → 從 NavigationItem 生成識別鍵
  ├─ GetSectionType(key)            → 根據識別鍵推斷區塊類型
  ├─ GetDefaultSortOrder(key)       → 取得頁面捷徑預設排序
  └─ GetDefaultQuickActionSortOrder(key) → 取得快速功能預設排序

EmployeeDashboardConfig（資料表）
  └─ 儲存員工個人的捷徑配置（NavigationItemKey + SortOrder + SectionType）
```

### 資料流程

```
首頁載入
  ├─ DashboardService.GetEmployeeDashboardBySectionAsync(employeeId, "Shortcut")
  │     ├─ 查詢 EmployeeDashboardConfig（SectionType = "Shortcut"）
  │     ├─ 如果配置為空且未初始化 → InitializeDefaultDashboardAsync()（兩個區塊同時初始化）
  │     ├─ 用 NavigationItemKey 對應 NavigationConfig（QuickAction 透過衍生取得）
  │     ├─ 過濾權限
  │     └─ 回傳 List<DashboardConfigWithNavItem>
  ├─ DashboardService.GetEmployeeDashboardBySectionAsync(employeeId, "QuickAction")
  │     └─ 同上邏輯，過濾 SectionType = "QuickAction"
  └─ Home.razor 分別渲染兩個區塊

點擊捷徑卡片
  ├─ Route → NavigationManager.NavigateTo(navItem.Route)
  ├─ Action → NavigationActionHandler.Invoke(navItem.ActionId)（MainLayout 處理）
  └─ QuickAction → Home.HandleQuickAction(navItem.ActionId)（開啟對應 Modal）
```

---

## 資料表設計

### Employee（員工表 - 相關欄位）

| 欄位 | 型別 | 說明 |
|------|------|------|
| HasInitializedDashboard | bool | 是否已初始化首頁儀表板（預設 false） |

**初始化邏輯**：`false` 表示新用戶，首次登入時自動套用預設配置（兩個區塊同時初始化）。`true` 表示已初始化，即使配置為空也不會自動重置。

### EmployeeDashboardConfig（員工儀表板配置表）

一筆記錄 = 一個被選用的捷徑或快速功能。

| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int (PK) | 繼承 BaseEntity |
| EmployeeId | int (FK) | 所屬員工 |
| NavigationItemKey | string(200) | 導航項目識別鍵（對應 NavigationConfig） |
| SortOrder | int | 顯示排序（數字越小越前面） |
| IsVisible | bool | 是否顯示（預留，允許暫時隱藏而不刪除） |
| SectionType | string(50) | 區塊類型：`"Shortcut"` 或 `"QuickAction"`，預設 `"Shortcut"` |
| WidgetSettings | string(max)? | JSON 格式的個人化參數（預留給未來進階功能） |

**複合唯一索引**：`EmployeeId + NavigationItemKey`

### NavigationItemKey 識別鍵格式

| 類型 | 格式 | 範例 |
|------|------|------|
| Route | 路由路徑 | `/employees`、`/purchase/orders` |
| Action | `Action:{ActionId}` | `Action:OpenCustomerReportIndex` |
| QuickAction | `QuickAction:{ActionId}` | `QuickAction:NewPurchaseOrder` |

---

## 檔案結構

```
Models/
├─ Enums/
│   └─ NavigationItemType.cs               # Route / Action / QuickAction 枚舉
└─ Navigation/
    └─ NavigationItem.cs                   # 導航項目模型（含 QuickAction 可選屬性）

Helpers/Common/
└─ NavigationActionHelper.cs               # CreateActionItem 工廠方法 + ActionHandlerRegistry

Data/
├─ Navigation/
│   ├─ NavigationConfig.cs                 # 導航選單配置（唯一資料來源 + QuickAction 衍生邏輯）
│   └─ DashboardDefaults.cs                # 預設配置（頁面捷徑 + 快速功能）
├─ Entities/Employees/
│   └─ EmployeeDashboardConfig.cs          # 員工配置 Entity（含 SectionType）

Services/Dashboard/
├─ IDashboardService.cs                    # 服務介面（含分區方法）
└─ DashboardService.cs                     # 服務實作

Components/
├─ Pages/
│   └─ Home.razor                          # 首頁（雙區塊 + QuickActionModalHost 引用）
└─ Shared/
    └─ Dashboard/
        ├─ DashboardShortcutCard.razor      # 捷徑卡片元件
        ├─ DashboardShortcutCard.razor.css  # 卡片樣式
        ├─ DashboardWidgetPickerModal.razor # 捷徑選擇 Modal
        └─ QuickActionModalHost.razor       # 集中管理所有 QuickAction Modal
```

---

## 元件設計

### Home.razor

```
Home.razor
├─ 頁面捷徑區塊
│   ├─ 標題 + [編輯] / [完成] [預設] 按鈕
│   ├─ DashboardShortcutCard × N（Route + Action 類型）
│   └─ 編輯模式：拖曳排序 + 移除按鈕 + [新增捷徑] 卡片
├─ 快速功能區塊
│   ├─ 標題 + [編輯] / [完成] [預設] 按鈕
│   ├─ DashboardShortcutCard × N（QuickAction 類型）
│   └─ 編輯模式：拖曳排序 + 移除按鈕 + [新增快速功能] 卡片
├─ DashboardWidgetPickerModal（共用，透過 PickerTitle 區分標題）
├─ GenericConfirmModalComponent（重置確認，透過 resetSectionType 區分區塊）
└─ <QuickActionModalHost @ref="quickActionHost" />
    └─ 集中管理所有 QuickAction EditModal
```

### QuickActionModalHost.razor

使用 DynamicComponent + 靜態註冊表（Registry）集中管理所有快速功能 Modal：

| 功能 | 說明 |
|------|------|
| `_registry` | 靜態字典，映射 QuickActionId → (元件類型, Id參數名, 額外參數) |
| `ConfiguredActionIds` | Parameter，接收已配置的 QuickAction Id 清單進行預渲染 |
| `Open(actionId)` | 公開方法，根據 QuickActionId 開啟對應 Modal |
| 延遲開啟機制 | 確保 firstRender 在 IsVisible=false 時完成，避免 ActionButtons 問題 |

**設計原則**：透過 DynamicComponent 動態渲染 Modal，新增 QuickAction 只需在 `_registry` 新增一行註冊。

### DashboardWidgetPickerModal.razor

| Parameter | 型別 | 說明 |
|-----------|------|------|
| AvailableWidgets | List\<NavigationItem\> | 可選的導航項目清單 |
| ExistingWidgetKeys | HashSet\<string\> | 已存在的識別鍵集合 |
| OnConfirm | EventCallback\<List\<string\>\> | 確認新增事件 |
| PickerTitle | string | Modal 標題（預設「新增首頁捷徑」） |

---

## 如何新增快速功能（只需兩步）

### 步驟 1：在現有的 NavigationItem 上設定 QuickActionId

在 `NavigationConfig.cs` 中找到要開放快速功能的項目，加上 QuickAction 屬性：

```csharp
new NavigationItem
{
    Name = "採購管理",
    Description = "管理採購訂單",
    Route = "/purchase/orders",
    IconClass = "bi bi-caret-right-fill",
    Category = "採購管理",
    RequiredPermission = "PurchaseOrder.Read",
    SearchKeywords = new List<string> { "採購單", "訂購單", "purchase order", "PO" },
    // 加上以下屬性即可支援快速功能
    QuickActionId = "NewPurchaseOrder",
    QuickActionName = "新增採購單",        // 選填，預設「新增」+ Name
    // QuickActionDescription = "...",     // 選填，預設自動產生
    // QuickActionIconClass = "...",       // 選填，預設 bi-plus-circle-fill
}
```

**完成第一步！** 系統會自動衍生一個 QuickAction 項目出現在「新增快速功能」選單中。

### 步驟 2：在 QuickActionModalHost.razor 註冊 Modal

在 `Components/Shared/Dashboard/QuickActionModalHost.razor` 的 `_registry` 字典中新增一行：

```csharp
// 在 _registry 字典中新增註冊
["NewPurchaseOrder"] = new(typeof(PurchaseOrderEditModalComponent), "PurchaseOrderId"),
```

如果有額外參數（例如 SetoffDocument 的 DefaultSetoffType）：

```csharp
["NewARSetoff"] = new(typeof(SetoffDocumentEditModalComponent), "SetoffDocumentId",
    new() { ["DefaultSetoffType"] = SetoffType.AccountsReceivable }),
```

### 步驟 3（選擇性）：設為預設

在 `DashboardDefaults.cs` 的 `DefaultQuickActionKeys` 清單新增識別鍵。

### 注意事項

- `RequiredPermission` 應使用已存在於 PermissionSeeder 的權限碼（通常是 `.Read`），否則所有使用者都無法看到此項目
- QuickAction Modal **不可使用 `@if` 包裹**，否則 ActionButtons 會失效

---

## 如何新增頁面捷徑選項

在 `NavigationConfig.cs` 的 `GetAllNavigationItems()` 中新增 Route 或 Action 項目即可。新項目自動出現在「新增捷徑」選單中。（選擇性）在 `DashboardDefaults.DefaultWidgetKeys` 加入預設。

---

## 目前已實作的快速功能

所有設有 QuickActionId 的 NavigationItem 都已註冊在 QuickActionModalHost.razor 中，包括：

| 分類 | QuickActionId | 衍生名稱 |
|------|---------------|----------|
| 人力資源管理 | NewEmployee, NewDepartment, NewEmployeePosition, NewRole, NewPermission | 新增員工/部門/職位/權限組/權限 |
| 廠商管理 | NewSupplier | 新增廠商 |
| 客戶管理 | NewCustomer | 新增客戶 |
| 商品管理 | NewProduct, NewProductCategory, NewUnit, NewSize, NewProductComposition, NewCompositionCategory, NewProductionSchedule | 新增商品/類型/單位/尺寸/物料清單/... |
| 庫存管理 | NewWarehouse, NewWarehouseLocation, NewStockTaking, NewMaterialIssue | 新增倉庫/庫位/庫存盤點/領料 |
| 採購管理 | NewPurchaseOrder, NewPurchaseReceiving, NewPurchaseReturn | 新增採購單/進貨單/進貨退出單 |
| 銷貨管理 | NewQuotation, NewSalesOrder, NewSalesDelivery, NewSalesReturn, NewSalesReturnReason | 新增報價單/訂單/銷貨單/退回單/退回原因 |
| 財務管理 | NewSetoffDocumentAR, NewSetoffDocumentAP, NewPaymentMethod, NewBank, NewCurrency | 新增應收沖款/應付沖款/付款方式/銀行/貨幣 |
| 系統管理 | NewCompany, NewPrinterConfiguration, NewPaperSetting, NewReportPrintConfiguration | 新增公司/印表機設定/紙張設定/報表設定 |

---

## 效能考量

- **Modal 常駐渲染**：QuickAction 的 Modal 不使用 `@if` 條件渲染（避免 `setupButtonTabNavigation` 問題），但 `IsVisible = false` 時 BaseModalComponent 不會顯示任何內容
- **分區獨立查詢**：`GetEmployeeDashboardBySectionAsync` 各自查詢，避免一次載入所有配置
- **新用戶初始化**：`InitializeDefaultDashboardAsync` 同時建立兩個區塊的預設配置，只需一次資料庫操作

---

## 快速參考

### 新增頁面捷徑
1. 在 `NavigationConfig.cs` 的 `GetAllNavigationItems()` 新增 Route 或 Action 項目
2. （選擇性）在 `DashboardDefaults.DefaultWidgetKeys` 加入預設

### 新增快速功能
1. 在 `NavigationConfig.cs` 的現有 NavigationItem 上設定 `QuickActionId`（和可選的 `QuickActionName`）
2. 在 `QuickActionModalHost.razor` 新增 Modal 宣告和 Open() case 分支（**不用 @if 包裹**）
3. （選擇性）在 `DashboardDefaults.DefaultQuickActionKeys` 加入預設

### 識別鍵格式
- Route 類型：`/path/to/page`
- Action 類型：`Action:ActionId`
- QuickAction 類型：`QuickAction:ActionId`

### 區塊類型常數
- `"Shortcut"` — 頁面捷徑區塊
- `"QuickAction"` — 快速功能區塊
