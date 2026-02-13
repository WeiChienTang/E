# 首頁客自化設計

## 概述

讓每位使用者可以自訂首頁，首頁採用**動態面板**架構：

- **自訂面板**：使用者可建立多個面板，每個面板有自訂標題
- **混合內容**：每個面板可同時包含頁面連結和快速功能
- **頁面連結**：卡片點擊後導航至對應頁面或開啟全域 Modal
- **快速功能**：卡片點擊後直接在首頁開啟業務 EditModal（新增模式）

```
┌──────────────────────────────────────────┐
│ 常用功能                     [編輯]      │
│ ─────────────────────────────────────── │
│ [員工管理] [新增採購單] [客戶管理] ...   │
│                                          │
│ 銷售區                       [編輯]      │
│ ─────────────────────────────────────── │
│ [訂單管理] [新增訂單] [客戶管理] ...     │
│                                          │
│ [+ 新增面板]                             │
└──────────────────────────────────────────┘
```

---

## 設計原則

1. **動態面板架構**：使用者可自訂面板數量（上限 6 個）與標題
2. **混合內容**：每個面板可同時包含頁面連結（Route/Action）與快速功能（QuickAction）
3. **單一資料來源**：以 `NavigationConfig` 作為可選項目的唯一來源
4. **每人獨立配置**：每位員工有自己的首頁佈局，互不影響
5. **權限自動過濾**：使用者只能看到自己有權限的功能
6. **預設配置**：新使用者首次登入自動套用預設面板組合
7. **同項目可重複**：同一個項目可出現在不同面板中
8. **QuickAction 與 NavigationItem 合一**：快速功能不是獨立項目，而是現有導航項目的附加屬性

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

---

## 架構設計

### 資料來源

```
NavigationConfig（靜態類）
  ├─ GetAllNavigationItems()        → 取得完整導航樹
  ├─ GetDashboardWidgetItems()      → 取得所有可作為捷徑的項目（Route + Action + 衍生 QuickAction）
  ├─ GetShortcutWidgetItems()       → 取得頁面連結項目（Route + Action）
  ├─ GetQuickActionWidgetItems()    → 取得快速功能項目（衍生 QuickAction）
  ├─ DeriveQuickActionItems()       → [私有] 從有 QuickActionId 的項目自動衍生 QuickAction
  ├─ GetNavigationItemByKey(key)    → 根據識別鍵取得導航項目
  └─ GetCategoryIcon(category)      → 根據分類取得對應圖示（備援用）

DashboardDefaults（靜態類）
  ├─ MaxPanelCount                  → 面板最大數量限制（6）
  ├─ MaxPanelTitleLength            → 面板標題最大長度（20）
  ├─ DefaultPanelDefinitions        → 預設面板定義清單
  ├─ GetNavigationItemKey(item)     → 從 NavigationItem 生成識別鍵
  ├─ IsQuickActionKey(key)          → 判斷 Key 是否為 QuickAction 類型
  └─ GetDefaultItemSortOrder(...)   → 取得預設面板中項目的排序

EmployeeDashboardPanel（資料表）
  └─ 儲存員工的面板定義（Title + SortOrder + IconClass）

EmployeeDashboardConfig（資料表）
  └─ 儲存面板內的項目配置（NavigationItemKey + SortOrder）
```

### 資料流程

```
首頁載入
  ├─ DashboardService.GetEmployeePanelsAsync(employeeId)
  │     ├─ 查詢 EmployeeDashboardPanel（含 DashboardConfigs）
  │     ├─ 如果配置為空且未初始化 → InitializeDefaultDashboardAsync()
  │     ├─ 用 NavigationItemKey 對應 NavigationConfig
  │     ├─ 過濾權限
  │     └─ 回傳 List<DashboardPanelWithItems>
  └─ Home.razor 動態迴圈渲染所有面板

點擊捷徑卡片
  ├─ Route → NavigationManager.NavigateTo(navItem.Route)
  ├─ Action → NavigationActionHandler.Invoke(navItem.ActionId)（MainLayout 處理）
  └─ QuickAction → quickActionHost.Open(navItem.ActionId)（開啟對應 Modal）
```

---

## 資料表設計

### Employee（員工表 - 相關欄位）

| 欄位 | 型別 | 說明 |
|------|------|------|
| HasInitializedDashboard | bool | 是否已初始化首頁儀表板（預設 false） |

**初始化邏輯**：`false` 表示新用戶，首次登入時自動套用預設配置。`true` 表示已初始化，即使配置為空也不會自動重置。

### EmployeeDashboardPanel（員工儀表板面板表）

一筆記錄 = 一個使用者自訂的面板。

| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int (PK) | 繼承 BaseEntity |
| EmployeeId | int (FK) | 所屬員工 |
| Title | string(50) | 面板標題（使用者自訂，最長 20 字） |
| SortOrder | int | 面板排序（數字越小越前面） |
| IconClass | string(50)? | 面板圖示（選填，預設 `bi-grid-fill`） |

**索引**：`(EmployeeId, SortOrder)`

### EmployeeDashboardConfig（員工儀表板項目配置表）

一筆記錄 = 一個被選用的捷徑或快速功能。

| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int (PK) | 繼承 BaseEntity |
| PanelId | int (FK) | 所屬面板 |
| EmployeeId | int (FK) | 所屬員工（冗餘，方便查詢） |
| NavigationItemKey | string(200) | 導航項目識別鍵（對應 NavigationConfig） |
| SortOrder | int | 顯示排序（數字越小越前面） |
| IsVisible | bool | 是否顯示（預留，允許暫時隱藏而不刪除） |
| WidgetSettings | string(max)? | JSON 格式的個人化參數（預留給未來進階功能） |

**複合唯一索引**：`PanelId + NavigationItemKey`（同一面板內項目不重複，但允許跨面板重複）

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
│   └─ DashboardDefaults.cs                # 預設面板定義
├─ Entities/Employees/
│   ├─ EmployeeDashboardPanel.cs           # 面板 Entity
│   └─ EmployeeDashboardConfig.cs          # 面板項目 Entity

Services/Dashboard/
├─ IDashboardService.cs                    # 服務介面（面板 CRUD + 項目管理）
└─ DashboardService.cs                     # 服務實作

Components/
├─ Pages/
│   └─ Home.razor                          # 首頁（動態面板 + QuickActionModalHost 引用）
└─ Shared/
    └─ Dashboard/
        ├─ DashboardShortcutCard.razor      # 捷徑卡片元件
        ├─ DashboardShortcutCard.razor.css  # 卡片樣式
        ├─ DashboardWidgetPickerModal.razor # 項目選擇 Modal（雙 Tab）
        └─ QuickActionModalHost.razor       # 集中管理所有 QuickAction Modal
```

---

## 元件設計

### Home.razor

```
Home.razor
├─ 動態面板迴圈
│   ├─ 面板標題（編輯模式下可修改）
│   ├─ [編輯] / [完成] [預設] [刪除] 按鈕
│   ├─ DashboardShortcutCard × N（Route + Action + QuickAction 混放）
│   └─ 編輯模式：拖曳排序 + 移除按鈕 + [新增項目] 卡片
├─ [新增面板] 按鈕（編輯模式且未達上限時顯示）
├─ DashboardWidgetPickerModal（雙 Tab：頁面連結 / 快速功能）
├─ GenericConfirmModalComponent（重置/刪除確認）
├─ BaseModalComponent（新增面板）
└─ <QuickActionModalHost @ref="quickActionHost" />
```

### DashboardWidgetPickerModal.razor

| Parameter | 型別 | 說明 |
|-----------|------|------|
| AvailableShortcutWidgets | List\<NavigationItem\> | 可選的頁面連結項目（Route + Action） |
| AvailableQuickActionWidgets | List\<NavigationItem\> | 可選的快速功能項目（QuickAction） |
| ExistingWidgetKeys | HashSet\<string\> | 已存在的識別鍵集合 |
| OnConfirm | EventCallback\<List\<string\>\> | 確認新增事件 |

**雙 Tab 設計**：頂部有「頁面連結」和「快速功能」兩個 Tab，使用者可切換查看不同類型的項目，選擇後一起新增到面板中。

### QuickActionModalHost.razor

使用 DynamicComponent + 靜態註冊表（Registry）集中管理所有快速功能 Modal：

| 功能 | 說明 |
|------|------|
| `_registry` | 靜態字典，映射 QuickActionId → (元件類型, Id參數名, 額外參數) |
| `ConfiguredActionIds` | Parameter，接收已配置的 QuickAction Id 清單進行預渲染 |
| `Open(actionId)` | 公開方法，根據 QuickActionId 開啟對應 Modal |
| 延遲開啟機制 | 確保 firstRender 在 IsVisible=false 時完成，避免 ActionButtons 問題 |

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
}
```

**完成第一步！** 系統會自動衍生一個 QuickAction 項目出現在「新增快速功能」選單中。

### 步驟 2：在 QuickActionModalHost.razor 註冊 Modal

在 `Components/Shared/Dashboard/QuickActionModalHost.razor` 的 `_registry` 字典中新增一行：

```csharp
["NewPurchaseOrder"] = new(typeof(PurchaseOrderEditModalComponent), "PurchaseOrderId"),
```

如果有額外參數（例如 SetoffDocument 的 DefaultSetoffType）：

```csharp
["NewARSetoff"] = new(typeof(SetoffDocumentEditModalComponent), "SetoffDocumentId",
    new() { ["DefaultSetoffType"] = SetoffType.AccountsReceivable }),
```

### 步驟 3（選擇性）：設為預設

在 `DashboardDefaults.cs` 的 `DefaultPanelDefinitions` 對應面板的 `ItemKeys` 清單新增識別鍵。

### 注意事項

- `RequiredPermission` 應使用已存在於 PermissionSeeder 的權限碼（通常是 `.Read`），否則所有使用者都無法看到此項目
- QuickAction Modal **不可使用 `@if` 包裹**，否則 ActionButtons 會失效

---

## 如何新增頁面連結選項

在 `NavigationConfig.cs` 的 `GetAllNavigationItems()` 中新增 Route 或 Action 項目即可。新項目自動出現在「新增項目」選單的「頁面連結」Tab 中。

（選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板加入預設。

---

## DashboardService 主要方法

### 面板管理

| 方法 | 說明 |
|------|------|
| `GetEmployeePanelsAsync(employeeId)` | 取得員工所有面板（含項目） |
| `CreatePanelAsync(employeeId, title)` | 建立新面板 |
| `UpdatePanelTitleAsync(panelId, title)` | 更新面板標題 |
| `DeletePanelAsync(panelId)` | 刪除面板（連同其項目） |
| `UpdatePanelSortOrderAsync(employeeId, panelIds)` | 更新面板排序 |

### 項目管理

| 方法 | 說明 |
|------|------|
| `GetAvailableWidgetsAsync(employeeId, isQuickAction)` | 取得可用項目 |
| `GetPanelExistingKeysAsync(panelId)` | 取得面板已有項目 Key |
| `AddWidgetBatchAsync(panelId, keys)` | 批次新增項目 |
| `RemoveWidgetAsync(configId)` | 移除項目 |
| `UpdateItemSortOrderAsync(panelId, configIds)` | 更新項目排序 |

### 初始化與重置

| 方法 | 說明 |
|------|------|
| `InitializeDefaultDashboardAsync(employeeId)` | 初始化預設面板 |
| `ResetPanelToDefaultAsync(panelId)` | 重置單一面板 |
| `ResetAllPanelsToDefaultAsync(employeeId)` | 重置所有面板 |

---

## 效能考量

- **Modal 常駐渲染**：QuickAction 的 Modal 不使用 `@if` 條件渲染（避免 `setupButtonTabNavigation` 問題），但 `IsVisible = false` 時 BaseModalComponent 不會顯示任何內容
- **Include 載入**：`GetEmployeePanelsAsync` 使用 `Include` 一次載入面板與項目
- **新用戶初始化**：`InitializeDefaultDashboardAsync` 建立預設面板結構，只需一次查詢觸發

---

## 快速參考

### 新增頁面連結
1. 在 `NavigationConfig.cs` 的 `GetAllNavigationItems()` 新增 Route 或 Action 項目
2. （選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板的 `ItemKeys` 加入

### 新增快速功能
1. 在 `NavigationConfig.cs` 的現有 NavigationItem 上設定 `QuickActionId`（和可選的 `QuickActionName`）
2. 在 `QuickActionModalHost.razor` 的 `_registry` 新增 Modal 註冊（**不用 @if 包裹**）
3. （選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板加入

### 識別鍵格式
- Route 類型：`/path/to/page`
- Action 類型：`Action:ActionId`
- QuickAction 類型：`QuickAction:ActionId`

### 面板限制
- 最多 **6 個**面板
- 標題最長 **20 字元**
- 同一項目可出現在多個面板中
