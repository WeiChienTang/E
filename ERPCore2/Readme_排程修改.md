# 生產排程模組 — 看板重構說明

> 文件日期：2026-03-07
> 目的：記錄本次將生產排程從傳統 Index/Edit 模式全面改為看板 Modal 的設計決策、實作細節，以及後續待辦項目。

---

## 一、設計決策

### 問題
原有的生產排程模組有兩個介面：
- `ProductionScheduleIndex.razor` — 傳統清單頁面（分頁、搜尋、批次刪除）
- `ProductionScheduleEditModalComponent.razor` — 對應的編輯 Modal

兩個介面功能重疊，且「清單」的意義不大（生產目的是掌握生產狀態，而非對資料做 CRUD 維護），容易造成使用者混亂。

### 決策
**完全以看板取代 Index 和 Edit**，看板具備：
- 週視圖（拖曳排程到某天）
- 側欄待排清單（直接顯示 `SalesOrderDetail` 中 `PendingQuantity > 0` 的項目）
- 點擊卡片快速編輯（修改狀態、數量、開始生產、完成入庫、結案、刪除）
- 拖曳側欄項目到日期欄，自動建立 `ProductionScheduleItem` 並展開 BOM
- 自動詢問是否建立領料單
- 以 Desktop Modal 方式從導覽列開啟（不再以獨立頁面呈現）

---

## 二、架構概覽

```
MainLayout.razor
  └─ ProductionBoardModalComponent.razor   ← Desktop Modal 包裝器
       └─ ProductionBoardWeekView.razor    ← 主元件：週視圖 + 所有子 Modal
            ├─ ProductionBoardPendingSidebar.razor   ← 左側待排清單（SalesOrderDetail）
            ├─ ProductionBoardDayColumn.razor        ← 每天一欄
            │    └─ ProductionBoardItemCard.razor    ← 卡片
            ├─ ProductionBoardItemEditModal.razor    ← 卡片點擊快速編輯
            ├─ GenericConfirmModalComponent          ← 領料確認對話框
            └─ MaterialIssueEditModalComponent       ← 建立領料單
```

---

## 三、檔案清單與說明

### 新建檔案

| 檔案 | 說明 |
|------|------|
| `ProductionBoardModalComponent.razor` | Desktop Modal 包裝器，無 `@page` 路由，由 MainLayout actionRegistry 觸發開啟。每次開啟時遞增 `boardKey` 以強制子元件重新載入。 |
| `ProductionBoardItemEditModal.razor` | 卡片點擊快速編輯 Modal。可修改狀態/數量/結案，刪除項目，以及觸發「開始生產」（`StartProductionAsync`）和「完成入庫」（`CompleteProductionAsync`）。刪除或數量變更時會回寫 `SalesOrderDetail.ScheduledQuantity`。 |

### 大幅修改

| 檔案 | 修改重點 |
|------|---------|
| `ProductionBoardWeekView.razor` | 側欄資料改從 `SalesOrderDetail` 建立（`BuildPendingItemsAsync`）、拖曳到日期欄改為建立新 `ProductionScheduleItem`（`AssignPendingItemAsync`）、BOM 展開移至此處、注入三個額外 Service |
| `ProductionBoardItemCard.razor` | 新增 `OnClick` EventCallback，點擊觸發編輯 Modal；數量顯示套用 `NumberFormatHelper.FormatSmart` |
| `ProductionBoardDayColumn.razor` | 新增 `OnItemClick` EventCallback，傳遞卡片點擊事件 |
| `ProductionBoardPendingSidebar.razor` | 資料型別改為 `BoardPendingItemDto`（SalesOrderDetail-based）、整合 `GenericSearchModalComponent` 搜尋、數量顯示套用 `NumberFormatHelper.FormatSmart` |
| `ProductionScheduleIndex.razor` | 簡化為 3 行，僅作直連 URL 的備用頁面 |
| `MainLayout.razor` | 加入 `ProductionBoardModalComponent`、`showProductionBoard` 狀態、`OpenProductionScheduleBoard()` 方法、actionRegistry 註冊 |
| `NavigationConfig.cs` | 將生產排程 nav item 從 Route 型改為 Action 型（`CreateActionItem`），點擊後開啟 Modal 而非跳頁 |

### 已刪除

| 檔案 | 原因 |
|------|------|
| `ProductionBoardAddItemModal.razor` | 側欄直接讀取 `SalesOrderDetail`，無需手動挑選新增，該 Modal 不再需要 |

---

## 四、核心流程說明

### 4.1 開啟看板

```
使用者點擊導覽列「生產排程」
  → NavigationConfig 的 ActionId = "OpenProductionScheduleBoard"
  → MainLayout.actionRegistry 觸發 OpenProductionScheduleBoard()
  → showProductionBoard = true
  → ProductionBoardModalComponent 顯示
  → boardKey++ 強制重新掛載 ProductionBoardWeekView
  → ProductionBoardWeekView.OnInitializedAsync() 載入資料
```

### 4.2 資料分類

| 資料來源 | 條件 | 顯示位置 |
|---------|------|---------|
| `SalesOrderDetail` | `PendingQuantity > 0`（= `OrderQuantity - ScheduledQuantity`） | 左側 Sidebar（待排） |
| `ProductionScheduleItem` | `PlannedStartDate` 落在當週範圍內 | 對應日期欄的卡片 |

`ScheduledQuantity` 批次查詢由 `GetScheduledQuantityMapAsync()` 提供 `Dict<SalesOrderDetailId, 已排量>`，避免 N+1。

批次（`ProductionSchedule`）採用「每日批次」模式：`PS-YYYYMMDD`，由 `GetOrCreateDailyBatchAsync` 自動建立或取得。

### 4.3 拖曳排程流程

```
從 Sidebar 拖曳卡片到某天
  → AssignPendingItemAsync(pending, targetDate)
  → GetOrCreateDailyBatchAsync(targetDate)
  → 建立新 ProductionScheduleItem（PlannedStartDate = targetDate）
  → 回寫 SalesOrderDetail.ScheduledQuantity（+= PendingQuantity）
  → ExpandBomAsync → 建立 ProductionScheduleDetail
  → 回傳 (BatchId, ItemId)
  → CheckMaterialIssueAfterDropAsync → 詢問是否建立領料單

從週視圖某天拖曳到另一天
  → MoveExistingItemAsync(itemId, targetDate)
  → UpdatePlannedDateAsync(itemId, targetDate)
```

### 4.4 快速編輯流程

```
點擊卡片 → ProductionBoardItemEditModal
  ├─ 開始生產（Pending 狀態才顯示）
  │    → StartProductionAsync(itemId)（設定 ActualStartDate，狀態 → InProgress）
  ├─ 完成入庫（InProgress 或有剩餘量時顯示）
  │    → CompleteProductionAsync(itemId, qty)（建立 Completion 記錄，累加 CompletedQuantity）
  ├─ 修改狀態 / 數量 / 結案旗標 → 儲存
  │    若數量有變化：回寫 SalesOrderDetail.ScheduledQuantity（差值）
  └─ 刪除（CompletedQuantity == 0 才允許）
       → 回寫 SalesOrderDetail.ScheduledQuantity（減回）
       → PermanentDeleteAsync
```

### 4.5 領料單流程

```
任一新增/拖曳動作完成後
  → 查詢 ProductionScheduleDetail（BOM 展開後的備料明細）
  → 若有 IssuedQuantity < RequiredQuantity 的明細
  → 顯示 GenericConfirmModalComponent 詢問是否建立領料單
  → 確認 → 開啟 MaterialIssueEditModalComponent
           （以 SourceProductionScheduleId 預填批次資料）
  → 儲存後重新載入看板
```

---

## 五、DTO 模型

### `BoardScheduleItemDto`（週視圖卡片）
欄位：Id, ProductionScheduleId, ProductCode/Name, ScheduledQuantity, CompletedQuantity, ProductionItemStatus, Priority, PlannedStartDate, PlannedEndDate, SalesOrderDetailId, SalesOrderCode, CustomerName, ExpectedDeliveryDate, IsClosed

### `BoardPendingItemDto`（Sidebar 待排卡片）
基於 `SalesOrderDetail`，欄位：SalesOrderDetailId, SalesOrderId, SalesOrderCode, OrderDate, ExpectedDeliveryDate, CustomerName, ProductId, ProductCode, ProductName, OrderQuantity, ScheduledQuantity（已排量），PendingQuantity（計算屬性 = OrderQuantity - ScheduledQuantity）

---

## 六、服務介面關鍵方法

| 服務 | 方法 | 用途 |
|------|------|------|
| `IProductionScheduleItemService` | `GetByDateRangeAsync` | 取得週視圖的卡片 |
| | `GetScheduledQuantityMapAsync` | 批次取得 Dict\<SalesOrderDetailId, 已排量\>，避免 N+1 |
| | `UpdatePlannedDateAsync` | 拖曳後更新計畫日期 |
| | `StartProductionAsync` | 設定 ActualStartDate，狀態→InProgress |
| | `CompleteProductionAsync` | 建立入庫記錄，累加 CompletedQuantity |
| `IProductionScheduleService` | `GetOrCreateDailyBatchAsync` | 取得或建立每日批次 |
| `IProductionScheduleDetailService` | `GetByScheduleItemIdAsync` | 查詢 BOM 備料明細（領料前置確認） |
| | `CreateDetailsForItemAsync` | 展開 BOM 建立備料明細 |
| `ISalesOrderService` | `GetAllAsync` | 載入所有銷貨訂單（待排清單來源） |
| `ISalesOrderDetailService` | `GetBySalesOrderIdWithIncludesAsync` | 載入含 Product 導覽的明細 |
| `IProductCompositionService` | `GetCompositionsByProductIdAsync` | 取得 BOM 結構 |

---

## 七、待辦事項與後續評估

### 中優先（UX 改善）

| # | 項目 | 說明 |
|---|------|------|
| 1 | **卡片優先度拖曳排序** | 同一天內的卡片排序目前固定為 `Priority` 數值，無法透過 UI 調整。考慮加入同欄卡片間的拖曳排序，儲存 Priority 值。 |
| 2 | **月視圖 / 過濾條件** | 目前只有週視圖。若訂單量大，可考慮加入按客戶或商品篩選，或切換月視圖。 |
| 3 | **深色模式 CSS** | `ProductionBoardWeekView.razor.css` 等 CSS 檔使用了部分 hardcoded 顏色（如卡片背景、狀態色條）。需補充 `[data-bs-theme=dark]` 覆寫，與系統深色模式整合。 |
| 4 | **i18n 多語系** | 看板內所有文字（卡片狀態、按鈕、提示文字）目前均為中文硬碼。後續若有多語系需求，需注入 `IStringLocalizer<SharedResource>` 並補充 resx key。 |

### 低優先（技術債）

| # | 項目 | 說明 |
|---|------|------|
| 5 | **`/production-schedules` 頁面** | `ProductionScheduleIndex.razor` 現在只是備用，可考慮改為導向說明或直接 redirect；或保留作為不登入 Modal 情況下的直連入口。 |
| 6 | **`BoardScheduleItemDto` 與 `BoardPendingItemDto` 合併評估** | 兩個 DTO 來源不同（一個來自 `ProductionScheduleItem`，一個來自 `SalesOrderDetail`），欄位有所差異，目前分開維護較清晰，合併效益有限。 |

---

## 八、已知限制

- **拖曳為 HTML5 原生 Drag & Drop**：在行動裝置上不支援觸控拖曳（`touchstart/touchmove` 未實作）。目前看板設計以桌面為主。
- **`boardKey++` 強制重建**：每次開啟 Modal 都會完整重新載入所有資料，無快取機制。資料量大時可能稍慢，未來可考慮只在關閉期間有資料變動時才重建。
- **批次模式（每日批次）**：每天的排程項目共享同一個 `ProductionSchedule`（`PS-YYYYMMDD`）。若未來需要「一張生產工單」對應多天的跨日排程，目前架構需調整。
- **側欄效能**：`BuildPendingItemsAsync` 對每筆訂單各發一次 DB 查詢（N+1）。訂單數量大時應改為批次查詢全部 `SalesOrderDetail`。

---

## 九、新增功能規劃

以下五項功能已完成架構評估，依實作難度由低至高排列。

---

### 9.1 工作量摘要列（Summary Bar）

**目標**：在工具列下方新增一列統計摘要，一眼掌握本週生產進度，不需逐欄數卡片。

**顯示內容**（均來自已載入的 `boardItems`，無額外 DB 查詢）：
- 本週總件數
- 等待領料件數（`WaitingMaterial` + `Pending`，UI 呈現相同）
- 生產中件數（`InProgress`）
- 已完成件數（`Completed`）

**實作方式**：
- 修改：`ProductionBoardWeekView.razor` — 新增 4 個 computed property
- 修改：`ProductionBoardWeekView.razor.css` — 新增 `.board-summary-bar` 樣式
- 在工具列與 `.board-body` 之間插入 `<div class="board-summary-bar">`，包含對應 badge
- Badge 顏色沿用現有狀態色（`status-waitingmaterial`、`status-inprogress`、`status-completed`）

**不需新建任何檔案。**

---

### 9.2 批次完工確認（Batch Complete）

**目標**：選定本週「生產中」的項目，批次輸入入庫數量，一次確認，避免逐一點開卡片。

**UX 流程**：
1. 工具列新增「批次完工」按鈕（只在本週有 `InProgress` 項目時才啟用）
2. 開啟新 Modal（`ProductionBoardBatchCompleteModal.razor`）
3. Modal 列出當週所有 `InProgress` 項目（商品名、排程量、已完成量、剩餘量）
4. 每列右側有數量輸入欄，預設值 = 剩餘量（`ScheduledQuantity - CompletedQuantity`）；可勾選/取消
5. 點擊「確認入庫」→ 逐一呼叫 `ScheduleItemService.CompleteProductionAsync(id, qty)`（qty ≤ 0 跳過）
6. 若某項完成量 ≥ 排程量，自動升格為 `Completed`（與單張卡片邏輯一致）
7. 全部處理完畢後關閉 Modal → 呼叫父層 `LoadDataAsync()` 刷新看板

**實作方式**：
- 新建：`ProductionBoardBatchCompleteModal.razor`
  - 參數：`IsVisible`、`IsVisibleChanged`、`Items`（`List<BoardScheduleItemDto>`，父層傳入 InProgress 清單）、`OnCompleted`（`EventCallback`）
  - 注入：`IProductionScheduleItemService`、`INotificationService`
  - 內部類別：`BatchCompleteRow`（持有 item 參考、`InputQty`、`IsSelected`）
- 修改：`ProductionBoardWeekView.razor`
  - 新增 `showBatchCompleteModal` 狀態
  - 工具列加「批次完工」按鈕，傳入 `InProgressItems`（computed from `boardItems`）
  - `OnCompleted` 回調 → `LoadDataAsync()`

---

### 9.3 狀態篩選（Status Filter）

**目標**：在工具列新增篩選器，只顯示特定狀態的卡片，減少視覺雜訊，尤其在排程量多時有用。

**篩選選項**：全部 / 等待領料 / 生產中 / 已完成

**實作方式**：
- 修改：`ProductionBoardWeekView.razor`
  - 新增 `private ProductionItemStatus? statusFilter = null;`
  - 修改 `GetItemsForDay(day)` 加入篩選邏輯：
    - `WaitingMaterial` 篩選同時包含 `Pending`（兩者 UI 呈現相同）
    - `null` = 全部顯示
- 修改：`ProductionBoardWeekView.razor.css` — 篩選按鈕群組樣式
- UI：工具列 `ms-auto` 區域新增 `btn-group btn-group-sm`（全部 / 等待領料 / 生產中 / 已完成）
  - 選中狀態 `btn-primary`；未選 `btn-outline-secondary`
- **純前端操作，不觸發任何 DB 查詢。**

**不需新建任何檔案。**

---

### 9.4 手動新增排程（Manual Add）

**目標**：允許使用者不透過側欄拖曳，直接在某天建立非訂單排程項目（例如：備品生產、補庫存）。

**觸發位置**：每個 `ProductionBoardDayColumn` 標題列右側加「+」圖示按鈕。

**UX 流程**：
1. 點擊某天的「+」按鈕
2. 開啟新 Modal（`ProductionBoardManualAddModal.razor`），標題顯示目標日期
3. 使用者輸入：商品（可搜尋）、排程數量
4. 確認後：
   - `GetOrCreateDailyBatchAsync(targetDate)` 取得每日批次
   - 建立 `ProductionScheduleItem`（`SalesOrderDetailId = null`，不回寫訂單數量）
   - `ExpandBomAsync` 展開 BOM
   - 觸發 `CheckMaterialIssueAfterDropAsync` 詢問是否建立領料單
5. 完成後刷新看板

**實作方式**：
- 新建：`ProductionBoardManualAddModal.razor`
  - 參數：`IsVisible`、`IsVisibleChanged`、`TargetDate`（`DateTime`）、`OnAdded`（`EventCallback`）
  - 注入：`IProductService`、`INotificationService`
  - 商品選擇使用搜尋輸入 + 清單（確認 `SearchableDropdownComponent` 是否可直接套用）
- 修改：`ProductionBoardDayColumn.razor`
  - 新增 `OnAddClick`（`EventCallback<DateTime>`）
  - day-header 右側加「+」按鈕，點擊觸發 `OnAddClick.InvokeAsync(Date)`
- 修改：`ProductionBoardWeekView.razor`
  - 新增 `showManualAddModal`、`manualAddTargetDate` 狀態
  - 處理 `DayColumn` 的 `OnAddClick` → 設定 targetDate → 開啟 Modal
  - `OnAdded` 回調 → `LoadDataAsync()`

**重要架構調整**：
現有 `AssignPendingItemAsync` 同時包含「建立排程項目」與「回寫 SalesOrderDetail」兩段邏輯。
建議提取共用方法 `CreateScheduleItemCoreAsync(productId, qty, targetDate, salesOrderDetailId?)` 供拖曳流程與手動新增共用，手動新增時 `salesOrderDetailId = null` 即可跳過回寫步驟。

---

### 9.5 交期預警（Delivery Alert）

**目標**：在工具列顯示本週逾期或即將到期的排程件數，提醒使用者優先處理。

**判斷規則**（以 `boardItems` 計算，排除已完成項目）：
- 逾期（紅色）：`ExpectedDeliveryDate < 今天` 且狀態非 `Completed`
- 緊急（橙色）：`ExpectedDeliveryDate` 在 1–3 天內 且狀態非 `Completed`

**實作方式**：
- 修改：`ProductionBoardWeekView.razor`
  - 新增 `private int OverdueCount` 與 `private int UrgentCount`（computed，沿用 `ProductionBoardItemCard` 的 `DeliveryUrgencyCss` 相同閾值邏輯）
  - 在工具列 `ms-auto` div 內加入 badge（僅在數量 > 0 時顯示）
  - 選填：點擊 badge 自動套用 9.3 的狀態篩選，縮小到相關項目

**不需新建任何檔案。**

---

### 實作優先序

| 優先 | 功能 | 新建檔案 | 修改檔案 | 難度 |
|------|------|----------|----------|------|
| 1 | 9.1 工作量摘要列 | — | WeekView + .css | 低 |
| 2 | 9.5 交期預警 | — | WeekView + .css | 低 |
| 3 | 9.3 狀態篩選 | — | WeekView + .css | 低 |
| 4 | 9.2 批次完工確認 | BatchCompleteModal | WeekView | 中 |
| 5 | 9.4 手動新增排程 | ManualAddModal | WeekView + DayColumn | 中 |
