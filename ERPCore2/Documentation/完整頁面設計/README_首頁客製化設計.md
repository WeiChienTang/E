# 首頁客製化設計

## 更新日期
2026-03-07

---

## 概述

首頁採用**動態面板**架構，每位使用者可自訂面板數量（上限 6 個）與標題。每個面板可混合包含三種類型的項目：**頁面連結**（導航至 Index 頁面或開啟全域 Modal）、**快速功能**（在首頁直接開啟業務 EditModal 新增模式）、**圖表介面**（開啟對應模組的圖表分析 Modal）。

---

## 檔案結構

| 檔案 | 路徑 | 說明 |
|------|------|------|
| 首頁 | `Components/Pages/Home.razor` | 動態面板渲染、編輯模式、拖曳排序 |
| 捷徑卡片 | `Components/Shared/Dashboard/DashboardShortcutCard.razor` | 單一捷徑卡片元件 |
| 項目選擇器 | `Components/Shared/Dashboard/DashboardWidgetPickerModal.razor` | 新增項目 Modal（三個 Tab） |
| 圖示選擇器 | `Components/Shared/Dashboard/IconPickerModal.razor` | 面板圖示選擇 Modal |
| 快速功能宿主 | `Components/Shared/Dashboard/QuickActionModalHost.razor` | 集中管理所有 QuickAction Modal |
| 導航配置 | `Data/Navigation/NavigationConfig.cs` | 唯一資料來源（含 QuickAction 衍生 + ChartWidget 篩選） |
| 預設配置 | `Data/Navigation/DashboardDefaults.cs` | 預設面板定義、面板限制、可選圖示清單 |
| 服務介面 | `Services/Dashboard/IDashboardService.cs` | 面板 CRUD + 項目管理 |
| 服務實作 | `Services/Dashboard/DashboardService.cs` | 服務實作 |
| 面板 Entity | `Data/Entities/Employees/EmployeeDashboardPanel.cs` | 面板資料表 |
| 項目 Entity | `Data/Entities/Employees/EmployeeDashboardConfig.cs` | 面板項目配置資料表 |

---

## 三種導航項目類型

| 類型 | 枚舉值 | 處理層級 | 用途 |
|------|--------|----------|------|
| Route | `NavigationItemType.Route` | `NavigationManager.NavigateTo()` | 導航至 Index 頁面 |
| Action | `NavigationItemType.Action` | MainLayout `CascadingParameter` | 開啟全域 Modal（報表中心、圖表分析等） |
| QuickAction | `NavigationItemType.QuickAction` | `Home.razor` 自行處理 | 在首頁直接開啟業務 EditModal（新增模式） |

**重要**：`NavigationConfig.cs` 中不存在 `ItemType = QuickAction` 的項目。QuickAction 是從設有 `QuickActionId` 屬性的現有 Route/Action 項目**自動衍生**而來。

---

## NavigationItem 擴充屬性

### QuickAction 屬性（設定後自動衍生快速功能項目）

| 屬性 | 型別 | 說明 |
|------|------|------|
| `QuickActionId` | `string?` | 設定後表示此項目支援快速功能，對應 `QuickActionModalHost._registry` 的 Key |
| `QuickActionName` | `string?` | 快速功能顯示名稱（選填，預設「新增」+ Name） |
| `QuickActionDescription` | `string?` | 快速功能描述（選填） |
| `QuickActionIconClass` | `string?` | 快速功能圖示（選填，預設 `bi bi-plus-circle-fill`） |

### ChartWidget 屬性（設定後移至圖表介面 Tab）

| 屬性 | 型別 | 說明 |
|------|------|------|
| `IsChartWidget` | `bool` | `true` 表示此項目為圖表介面，從「頁面連結」Tab 移至「圖表介面」Tab，預設 `false` |

---

## NavigationConfig 主要方法

| 方法 | 說明 |
|------|------|
| `GetAllNavigationItems()` | 取得完整導航樹 |
| `GetShortcutWidgetItems()` | 取得頁面連結項目（Route + Action，不含 IsChartWidget） |
| `GetQuickActionWidgetItems()` | 取得快速功能項目（衍生 QuickAction） |
| `GetChartWidgetItems()` | 取得圖表介面項目（IsChartWidget = true 的 Action） |
| `GetNavigationItemByKey(key)` | 根據識別鍵取得導航項目 |
| `DeriveQuickActionItems()` | [私有] 從有 QuickActionId 的項目自動衍生 QuickAction |

---

## DashboardDefaults 常數與配置

| 成員 | 說明 |
|------|------|
| `MaxPanelCount` | 面板最大數量（6） |
| `MaxPanelTitleLength` | 面板標題最大長度（20） |
| `DefaultPanelDefinitions` | 預設面板定義清單（新用戶首次登入自動套用） |
| `AvailableIcons` | 可選用的面板圖示清單（精選 Bootstrap Icons，依分類） |
| `GetNavigationItemKey(item)` | 從 NavigationItem 生成識別鍵 |
| `IsQuickActionKey(key)` | 判斷 Key 是否為 QuickAction 類型 |

---

## NavigationItemKey 識別鍵格式

| 類型 | 格式 | 範例 |
|------|------|------|
| Route | 路由路徑 | `/employees`、`/purchase/orders` |
| Action（含圖表） | `Action:{ActionId}` | `Action:OpenCustomerCharts` |
| QuickAction | `QuickAction:{ActionId}` | `QuickAction:NewPurchaseOrder` |

圖表介面項目與一般 Action 使用相同識別鍵格式，儲存層無需區分。

---

## 資料表設計

### Employee（相關欄位）

| 欄位 | 說明 |
|------|------|
| `HasInitializedDashboard` | 是否已初始化儀表板。`false` 表示新用戶，首次載入自動套用預設配置；`true` 表示已初始化，配置為空時不重置 |

### EmployeeDashboardPanel（員工儀表板面板表）

| 欄位 | 型別 | 說明 |
|------|------|------|
| `EmployeeId` | int (FK) | 所屬員工 |
| `Title` | string(50) | 面板標題 |
| `SortOrder` | int | 面板排序 |
| `IconClass` | string(50)? | 面板圖示（選填，預設 `bi-grid-fill`） |

### EmployeeDashboardConfig（員工儀表板項目配置表）

| 欄位 | 型別 | 說明 |
|------|------|------|
| `PanelId` | int (FK) | 所屬面板 |
| `EmployeeId` | int (FK) | 所屬員工（冗餘，方便查詢） |
| `NavigationItemKey` | string(200) | 導航項目識別鍵（對應 NavigationConfig） |
| `SortOrder` | int | 顯示排序 |
| `IsVisible` | bool | 是否顯示（預留，允許暫時隱藏而不刪除） |
| `WidgetSettings` | string(max)? | JSON 格式個人化參數（預留） |

**複合唯一索引**：`PanelId + NavigationItemKey`（同面板內不重複，跨面板允許重複）

---

## 元件設計

### DashboardWidgetPickerModal 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `AvailableShortcutWidgets` | `List<NavigationItem>` | 可選的頁面連結項目 |
| `AvailableQuickActionWidgets` | `List<NavigationItem>` | 可選的快速功能項目 |
| `AvailableChartWidgets` | `List<NavigationItem>` | 可選的圖表介面項目 |
| `ExistingWidgetKeys` | `HashSet<string>` | 已存在的識別鍵集合（已加入者置灰） |
| `OnConfirm` | `EventCallback<List<string>>` | 確認新增事件，傳回選取的識別鍵清單 |

搜尋模式下合併顯示三種類型的結果。

### IconPickerModal 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | 顯示狀態變更事件 |
| `CurrentIcon` | `string?` | 目前選用的圖示（標示當前選擇） |
| `OnConfirm` | `EventCallback<string>` | 確認選擇事件，傳回選中的圖示 CSS class |

### QuickActionModalHost 機制

| 功能 | 說明 |
|------|------|
| `_registry` | 靜態字典：`QuickActionId → (元件類型, Id參數名, 額外參數字典?)` |
| `ConfiguredActionIds` | Parameter，接收已配置的 QuickAction Id 清單進行預渲染 |
| `Open(actionId)` | 公開方法，根據 QuickActionId 開啟對應 Modal |
| 權限守衛 | `OnParametersSetAsync` 呼叫 `NavigationPermissionService.CanAccessAsync` 檢查 `RequiredPermission`，未授權者不渲染 Modal 也無法開啟 |
| 延遲開啟機制 | 確保 firstRender 在 `IsVisible=false` 時完成，避免 ActionButtons 問題 |

---

## DashboardService 主要方法

### 面板管理

| 方法 | 說明 |
|------|------|
| `GetEmployeePanelsAsync(employeeId)` | 取得員工所有面板（含項目，Include 一次載入） |
| `CreatePanelAsync(employeeId, title)` | 建立新面板 |
| `UpdatePanelTitleAsync(panelId, title)` | 更新面板標題 |
| `UpdatePanelIconAsync(panelId, iconClass)` | 更新面板圖示 |
| `DeletePanelAsync(panelId)` | 刪除面板（連同其項目） |
| `UpdatePanelSortOrderAsync(employeeId, panelIds)` | 更新面板排序 |

### 項目管理

| 方法 | 說明 |
|------|------|
| `GetAvailableWidgetsAsync(employeeId, isQuickAction)` | 取得頁面連結或快速功能的可用項目（含權限過濾） |
| `GetAvailableChartWidgetsAsync(employeeId)` | 取得圖表介面的可用項目 |
| `GetPanelExistingKeysAsync(panelId)` | 取得面板已有項目 Key |
| `AddWidgetBatchAsync(panelId, keys)` | 批次新增項目 |
| `RemoveWidgetAsync(configId)` | 移除項目 |
| `UpdateItemSortOrderAsync(panelId, configIds)` | 更新項目排序 |

### 初始化與重置

| 方法 | 說明 |
|------|------|
| `InitializeDefaultDashboardAsync(employeeId)` | 初始化預設面板（新用戶首次登入觸發） |
| `ResetPanelToDefaultAsync(panelId)` | 重置單一面板至預設 |
| `ResetAllPanelsToDefaultAsync(employeeId)` | 重置所有面板至預設 |

---

## 重要設計規則

### 1. QuickAction Modal 不使用 @if 條件渲染

`QuickActionModalHost` 中的 Modal 必須常駐 DOM，以 `IsVisible = false` 控制顯示。使用 `@if` 包裹會導致 ActionButtons（`setupButtonTabNavigation`）失效。

### 2. 圖表介面不使用 CreateActionItem() 建立

圖表介面項目需要直接宣告 `NavigationItem` 以設定 `IsChartWidget = true`，`NavigationActionHelper.CreateActionItem()` 不支援此屬性。

### 3. 權限自動繼承

`QuickActionModalHost` 的權限守衛自動從 `NavigationConfig` 的 `RequiredPermission` 繼承，新增快速功能時無需在 `QuickActionModalHost` 額外設定權限。

### 4. MainLayout Modal 同樣不使用 @if

MainLayout 中的圖表 Modal 宣告不使用 `@if` 包裹，原因同 QuickAction。

### 5. Picker Modal 平行載入

開啟 Picker Modal 時，三種類型的可用項目使用 `Task.WhenAll` 平行載入，避免序列等待。

### 6. 權限快取

`NavigationPermissionService.CanAccessAsync` 使用 10 分鐘記憶體快取，大量 QuickAction 預渲染時不重複查詢資料庫。

---

## 新增功能快速參考

### 新增頁面連結
1. 在 `NavigationConfig.GetAllNavigationItems()` 新增 Route 或 Action 項目（`IsChartWidget` 保持預設 `false`）
2. （選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板的 `ItemKeys` 加入識別鍵

### 新增快速功能
1. 在 `NavigationConfig.cs` 的現有 NavigationItem 設定 `QuickActionId`（和選填的 `QuickActionName`）
2. 在 `QuickActionModalHost._registry` 新增 `QuickActionId → (元件類型, Id參數名)` 對應（勿用 `@if` 包裹）
3. （選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板加入 `QuickAction:{ActionId}`

### 新增圖表介面
1. 在 `NavigationConfig.cs` 直接宣告 NavigationItem，設定 `ItemType = Action` 且 `IsChartWidget = true`（不使用 `CreateActionItem()`）
2. 在 `MainLayout.razor` 的 `OnInitialized` 註冊 Handler，並宣告 Modal（勿用 `@if` 包裹）
3. （選擇性）在 `DashboardDefaults.DefaultPanelDefinitions` 對應面板加入 `Action:{ActionId}`

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md)
- [README_Index頁面設計.md](README_Index頁面設計.md)
- [README_EditModal設計.md](README_EditModal設計.md)
