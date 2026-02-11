# 首頁客自化設計

## 概述

讓每位使用者可以自訂首頁要顯示的捷徑卡片，快速進入常用功能頁面（如員工管理、客戶管理、報表中心等）。  
採用**輕量級捷徑模式**：卡片只顯示圖示與名稱，點擊後直接導航至對應頁面或開啟對應 Modal，不在卡片上顯示資料內容。

---

## 設計原則

1. **單一資料來源**：以 `NavigationConfig` 作為可選捷徑的唯一來源，不需要維護額外的資料表或 Seeder
2. **每人獨立配置**：每位員工有自己的首頁佈局，互不影響
3. **權限自動過濾**：使用者只能看到自己有權限的功能捷徑
4. **預設配置**：新使用者首次登入自動套用預設捷徑組合
5. **自動同步**：NavigationConfig 新增項目後，立即成為可選捷徑（無需任何資料庫操作）
6. **可擴展**：未來可升級為顯示統計數字、圖表等進階小工具

---

## 架構設計

### 資料來源

```
NavigationConfig（靜態類）
  └─ GetDashboardWidgetItems() → 取得所有可作為捷徑的導航項目
  └─ GetNavigationItemByKey(key) → 根據識別鍵取得導航項目
  └─ GetCategoryIcon(category) → 根據分類取得對應圖示（圖示備援用）

DashboardDefaults（靜態類）
  └─ DefaultWidgetKeys → 預設捷徑識別鍵清單
  └─ GetNavigationItemKey(item) → 從 NavigationItem 生成識別鍵
  └─ GetDefaultSortOrder(key) → 取得預設排序

EmployeeDashboardConfig（資料表）
  └─ 儲存員工個人的捷徑配置（NavigationItemKey + SortOrder）
```

### 資料流程

```
首頁載入
  ├─ DashboardService.GetEmployeeDashboardAsync(employeeId)
  │     ├─ 查詢 EmployeeDashboardConfig（從資料庫）
  │     ├─ 如果配置為空：
  │     │     ├─ 檢查 Employee.HasInitializedDashboard
  │     │     ├─ false → 新用戶，呼叫 InitializeDefaultDashboardAsync()
  │     │     └─ true → 用戶故意清空，回傳空清單
  │     ├─ 用 NavigationItemKey 對應 NavigationConfig（從靜態類）
  │     ├─ 過濾權限
  │     └─ 回傳 List<DashboardConfigWithNavItem>
  └─ Home.razor 渲染捷徑卡片

新增/移除捷徑
  ├─ 執行新增或移除操作
  ├─ 設定 Employee.HasInitializedDashboard = true
  └─ 儲存變更

新增捷徑選單
  ├─ DashboardService.GetAvailableWidgetsAsync(employeeId)
  │     ├─ 從 NavigationConfig.GetDashboardWidgetItems() 取得所有項目
  │     ├─ 過濾有權限的項目
  │     └─ 回傳 List<NavigationItem>
  └─ 選擇後儲存 NavigationItemKey 到 EmployeeDashboardConfig
```

---

## 資料表設計

### Employee（員工表 - 相關欄位）

| 欄位 | 型別 | 說明 |
|------|------|------|
| HasInitializedDashboard | bool | 是否已初始化首頁儀表板（預設 false） |

**初始化邏輯：**
- `false`：新用戶，首次登入時自動套用預設配置
- `true`：已初始化，即使配置為空也不會自動重置（用戶可能故意清空）

**設定時機：** 任何儀表板操作（新增、移除、初始化）均會設定為 `true`

### EmployeeDashboardConfig（員工儀表板配置表）

每位員工的個人首頁配置，一筆記錄 = 一個被選用的捷徑。

| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int (PK) | 繼承 BaseEntity |
| EmployeeId | int (FK) | 所屬員工 |
| NavigationItemKey | string(200) | 導航項目識別鍵（對應 NavigationConfig） |
| SortOrder | int | 顯示排序（數字越小越前面） |
| IsVisible | bool | 是否顯示（預留，允許暫時隱藏而不刪除） |
| WidgetSettings | string(max)? | JSON 格式的個人化參數（預留給未來進階功能） |

**複合唯一索引**：`EmployeeId + NavigationItemKey`（同一員工不能重複加入同一個捷徑）

### NavigationItemKey 識別鍵格式

| 類型 | 格式 | 範例 |
|------|------|------|
| Route 類型 | 直接使用路由路徑 | `/employees`、`/customers` |
| Action 類型 | `Action:{ActionId}` | `Action:OpenCustomerReportIndex` |

---

## 檔案結構

```
Data/
├─ Navigation/
│   ├─ NavigationConfig.cs      # 導航選單配置（唯一資料來源）
│   └─ DashboardDefaults.cs     # 預設捷徑配置
├─ Entities/
│   └─ Employees/
│       └─ EmployeeDashboardConfig.cs  # 員工配置 Entity

Services/
└─ Dashboard/
    ├─ IDashboardService.cs     # 服務介面
    └─ DashboardService.cs      # 服務實作

Components/
├─ Pages/
│   └─ Home.razor               # 首頁（整合儀表板）
└─ Shared/
    └─ Dashboard/
        ├─ DashboardShortcutCard.razor        # 捷徑卡片元件
        ├─ DashboardShortcutCard.razor.css    # 卡片樣式
        └─ DashboardWidgetPickerModal.razor   # 捷徑選擇 Modal
```

---

## 如何新增新的捷徑選項

### 步驟 1：在 NavigationConfig 新增導航項目

只需在 `Data/Navigation/NavigationConfig.cs` 的 `GetAllNavigationItems()` 方法中新增項目：

```csharp
// 範例：新增一個 Route 類型的項目
new NavigationItem
{
    Name = "新功能",
    Description = "新功能的描述",
    Route = "/new-feature",
    IconClass = "bi bi-star-fill",
    Category = "系統管理",
    RequiredPermission = "NewFeature.Read",
    SearchKeywords = new List<string> { "新功能", "new feature" }
}

// 範例：新增一個 Action 類型的項目
NavigationActionHelper.CreateActionItem(
    name: "新報表中心",
    description: "開啟新報表中心",
    iconClass: "bi bi-graph-up",
    actionId: "OpenNewReportIndex",
    category: "報表管理",
    requiredPermission: "NewReport.Read",
    searchKeywords: new List<string> { "新報表", "new report" }
)
```

**完成！** 新項目會自動出現在「新增捷徑」選單中（無需任何資料庫操作或 Seeder）。

### 步驟 2（選擇性）：設為預設捷徑

如果希望新使用者自動獲得此捷徑，修改 `Data/Navigation/DashboardDefaults.cs`：

```csharp
public static readonly List<string> DefaultWidgetKeys = new()
{
    "/employees",
    "/customers",
    "/suppliers",
    "/products",
    "/inventoryStocks",
    "/purchase/orders",
    "/salesOrders",
    "/new-feature",     // ← 新增這行（Route 類型）
    // 或
    "Action:OpenNewReportIndex",  // ← Action 類型
};
```

---

## Service 設計

### IDashboardService 介面

```
檔案位置：Services/Dashboard/IDashboardService.cs
```

| 方法 | 說明 |
|------|------|
| `GetAvailableWidgetsAsync(int employeeId)` | 取得該員工有權限使用的所有導航項目（用於「新增捷徑」選擇畫面） |
| `GetEmployeeDashboardAsync(int employeeId)` | 取得該員工目前的首頁配置；若配置為空且未初始化過 → 自動建立預設配置；若已初始化過 → 回傳空清單 |
| `AddWidgetAsync(int employeeId, string navigationItemKey)` | 新增一個捷徑到員工首頁，並標記 HasInitializedDashboard = true |
| `AddWidgetBatchAsync(int employeeId, List<string> navigationItemKeys)` | 批次新增捷徑，並標記 HasInitializedDashboard = true |
| `RemoveWidgetAsync(int employeeId, int configId)` | 從員工首頁移除一個捷徑，並標記 HasInitializedDashboard = true |
| `UpdateSortOrderAsync(int employeeId, List<int> configIds)` | 批次更新排序（拖曳排序後呼叫） |
| `InitializeDefaultDashboardAsync(int employeeId)` | 根據員工權限建立預設配置，並標記 HasInitializedDashboard = true |
| `ResetToDefaultAsync(int employeeId)` | 刪除現有配置並重新建立預設配置 |

### DashboardConfigWithNavItem 回傳類別

```csharp
public class DashboardConfigWithNavItem
{
    public EmployeeDashboardConfig Config { get; set; }
    public NavigationItem NavigationItem { get; set; }
}
```

---

## 元件設計

### Home.razor

首頁結構：

```
Home.razor
├─ 頂部工具列
│   ├─ 頁面標題 "系統首頁"
│   ├─ 「編輯首頁」按鈕（切換編輯模式）
│   └─ 編輯模式下：「重置預設」、「完成編輯」按鈕
├─ 捷徑卡片區域（Grid 佈局）
│   ├─ DashboardShortcutCard × N（根據配置動態渲染）
│   └─ （編輯模式下）「+ 新增捷徑」卡片
├─ 編輯模式下的操作
│   ├─ 每張卡片右上角出現「×」移除按鈕
│   └─ 支援拖曳排序
└─ DashboardWidgetPickerModal（新增捷徑時的選擇 Modal）
```

### DashboardShortcutCard.razor

單一捷徑卡片元件：

| Parameter | 型別 | 說明 |
|-----------|------|------|
| NavigationItem | NavigationItem | 導航項目資料 |
| IsEditMode | bool | 是否處於編輯模式 |
| OnRemove | EventCallback | 移除回呼 |
| OnClick | EventCallback | 點擊回呼（導航或觸發動作） |

**圖示備援邏輯：**
元件內部的 `GetEffectiveIcon()` 方法會自動處理圖示：
- 如果導航項目有有效圖示（如 `bi-printer-fill`）→ 使用原圖示
- 如果圖示為空或箭頭（`bi-caret-right-fill`）→ 使用 Category 對應圖示

| Category | 備援圖示 |
|----------|----------|
| 人力資源管理 | bi-person-badge-fill |
| 供應鏈管理 | bi-building-gear |
| 客戶關係管理 | bi-people-fill |
| 商品管理 | bi-box-seam-fill |
| 庫存管理 | bi-boxes |
| 採購管理 | bi-truck |
| 銷售管理 | bi-cart-fill |
| 財務管理 | bi-journal-text |
| 系統管理 | bi-gear-fill |

### DashboardWidgetPickerModal.razor

新增捷徑時的選擇 Modal：

| Parameter | 型別 | 說明 |
|-----------|------|------|
| AvailableWidgets | List\<NavigationItem\> | 可選的導航項目清單 |
| ExistingWidgetKeys | HashSet\<string\> | 已存在的識別鍵集合 |
| OnConfirm | EventCallback\<List\<string\>\> | 確認新增事件 |

---

## 點擊行為處理

捷徑卡片的點擊行為根據 `NavigationItemType` 處理：

### Route 類型
```csharp
NavigationManager.NavigateTo(navItem.Route);
```

### Action 類型
透過 `CascadingParameter` 傳遞的 `NavigationActionHandler` 處理：
```csharp
NavigationActionHandler.Invoke(navItem.ActionId);
```

---

## 拖曳排序

專案已有 `DragDropState.cs` 和 `DragDropEventArgs.cs`，首頁直接使用 HTML5 拖放 API：

1. 編輯模式下，卡片啟用 `draggable="true"`
2. 處理 `ondragstart`、`ondragover`、`ondrop` 事件
3. 拖放完成後呼叫 `DashboardService.UpdateSortOrderAsync` 儲存新順序

---

## 設計優勢

### 相較於資料庫 Seeder 方案

| 面向 | Seeder 方案 | NavigationConfig 方案（目前採用）|
|------|-------------|----------------------------------|
| 新增項目 | 需修改 Seeder + 執行 Seed | 只需修改 NavigationConfig |
| 資料同步 | 需處理 Seeder 更新邏輯 | 自動同步，即時反映 |
| 資料表數量 | 2 張（Widget + Config） | 1 張（Config） |
| 維護成本 | 較高（雙資料來源） | 較低（單一來源） |
| 離線可用 | ✓ | ✓ |

---

## 未來擴展方向

此設計預留了擴展空間：

1. **統計數字顯示**：在 NavigationItem 新增 `StatisticsServiceMethod` 欄位或使用 `WidgetSettings` JSON
2. **圖表小工具**：透過 `WidgetSettings` JSON 儲存圖表類型和資料範圍設定
3. **卡片大小自訂**：在 EmployeeDashboardConfig 新增 `ColSpan` / `RowSpan` 欄位
4. **多頁面儀表板**：在 EmployeeDashboardConfig 新增 `PageIndex` 欄位，支援多個首頁分頁

---

## 快速參考

### 新增一個可選捷徑
1. 在 `NavigationConfig.cs` 的 `GetAllNavigationItems()` 新增 `NavigationItem`
2. 完成（新項目自動出現在選單中）

### 新增預設捷徑
1. 完成上述步驟
2. 在 `DashboardDefaults.cs` 的 `DefaultWidgetKeys` 清單新增識別鍵

### 識別鍵格式
- Route 類型：`/path/to/page`
- Action 類型：`Action:ActionId`
