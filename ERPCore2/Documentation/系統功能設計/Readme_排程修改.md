# 生產排程看板設計說明與待辦事項

> 最後更新：2026-03-09

## 一、系統現況說明

### 1.1 整體架構

生產排程看板（Production Board）包含以下元件：

| 元件 | 路徑 | 說明 |
|------|------|------|
| `ProductionBoardWeekView.razor` | Components/Pages/Products/ProductionBoard/ | 主視圖，週導航 + 拖曳邏輯 |
| `ProductionBoardPendingSidebar.razor` | 同上 | 左側待排清單 sidebar |
| `ProductionBoardDayColumn.razor` | 同上 | 每天的欄位卡片容器 |
| `ProductionBoardItemCard.razor` | 同上 | 單張排程卡片 |
| `ProductionBoardItemEditModal.razor` | 同上 | 點擊卡片開啟的快速編輯視窗 |
| `ProductionBoardBatchCompleteModal.razor` | 同上 | 批次完工 Modal |
| `ProductionCompletionModal.razor` | 同上 | 單筆完工確認 Modal |

### 1.2 資料流程

```
SalesOrder（已審核）
  └── SalesOrderDetail（訂單明細）
        ├── OrderQuantity     = 訂購總量
        ├── ScheduledQuantity = 已排入看板的總量（DB 欄位，供鎖定明細列用）
        └── PendingQuantity   = OrderQuantity - ScheduledQuantity（待排量）

待排 Sidebar 計算邏輯（BuildPendingItemsAsync）：
  - 取得所有 SalesOrder（目前包含未審核訂單 — 待修正，見§3）
  - 逐訂單載入 SalesOrderDetail（N+1 問題 — 待優化，見§3）
  - 以 GetScheduledQuantityMapAsync() 取得即時已排量 map
  - PendingQuantity = OrderQuantity - scheduledMap[detail.Id]
  - PendingQuantity > 0 才列入 sidebar

排入看板（從 sidebar 拖曳到日期欄）：
  1. GetOrCreateDailyBatchAsync() 取得或建立當天的 ProductionSchedule 批次
  2. 建立 ProductionScheduleItem（狀態：WaitingMaterial）
  3. 回寫 SalesOrderDetail.ScheduledQuantity += PendingQuantity
  4. 展開 BOM → 建立 ProductionScheduleDetail 列表
  5. 若 BOM 無需求量 → 升格狀態為 InProgress（無需領料）
  6. 若 BOM 有需求量 → 呼叫 CheckBomStockWarningAsync（庫存不足顯示警告，不阻擋）
  7. 詢問是否建立領料單（GenericConfirmModalComponent）

退回待排（從看板拖回 sidebar，或在 EditModal 點退回）：
  - 呼叫 ReturnToSidebarAsync(itemId)
  - 條件：CompletedQuantity == 0
  - 原子操作：還原 SalesOrderDetail.ScheduledQuantity + 刪除 ProductionScheduleDetails + 刪除 Item
  - 回傳 ServiceResult<bool>：bool=true 表示有已發領料記錄，UI 顯示額外警告

ProductionScheduleItem 狀態轉換：
  WaitingMaterial → [手動開始生產] → InProgress → [完工確認] → Completed
  任何狀態（CompletedQuantity==0）← ReturnToSidebarAsync ← 退回待排
```

### 1.3 雙軌 ScheduledQuantity 設計

系統同時維護兩個「已排數量」的資料來源，各有不同用途：

| 資料來源 | 類型 | 用途 |
|---------|------|------|
| `SalesOrderDetail.ScheduledQuantity` | DB 欄位（冗餘） | `DetailLockHelper` 判斷能否刪除訂單明細列 |
| `GetScheduledQuantityMapAsync()` | 即時 DB 聚合 | Sidebar 計算 PendingQuantity（準確，不受 race condition 影響） |

兩者必須同步更新：
- 排入看板時：`ScheduledQuantity += PendingQuantity`
- 退回待排時：`ScheduledQuantity -= item.ScheduledQuantity`

---

## 二、已完成的修改

### 2.1 修復退回功能（Bug Fix）

**根本原因：** 原本程式使用 `PermanentDeleteAsync()`，但 `CanDeleteAsync()` 只允許 `ProductionItemStatus.Pending` 狀態，而看板卡片實際狀態為 `WaitingMaterial`，導致刪除永遠靜默失敗。前端未檢查回傳值，顯示成功訊息但資料未異動，`ScheduledQuantity` 也未被還原（資料損毀）。

**修正方式：** 新增 `ReturnToSidebarAsync(itemId)` 服務方法，繞過 `CanDeleteAsync`，只驗證 `CompletedQuantity == 0`，並用原子操作完成全部動作。

**修改的檔案：**
- `Services/ProductionManagement/IProductionScheduleItemService.cs` — 新增介面方法
- `Services/ProductionManagement/ProductionScheduleItemService.cs` — 新增實作
- `Components/Pages/Products/ProductionBoard/ProductionBoardItemEditModal.razor` — `HandleReturnConfirmed` 改用新方法
- `Components/Pages/Products/ProductionBoard/ProductionBoardWeekView.razor` — `ReturnBoardItemAsync` 改用新方法

### 2.2 排入時 BOM 庫存警告

拖曳排入看板後，若 BOM 有需求量，系統自動呼叫 `CheckBomStockWarningAsync()`：
- 逐一比較每項組件的需求量 vs 可用庫存
- 庫存不足時顯示 Warning 通知（不阻擋排程）
- 提醒使用者安排採購或調撥

---

## 三、待辦事項（需要繼續修改或加強）

### 3.1【高優先】過濾未審核與已拒絕訂單

**問題：** `BuildPendingItemsAsync` 呼叫 `SalesOrderService.GetAllAsync()`，回傳所有訂單，包括：
- 尚未審核的訂單
- 已拒絕（Rejected）的訂單

**期望行為：**
- 已拒絕訂單 → 完全不顯示在 sidebar
- 未審核訂單 → 選擇方案（討論中）：
  - 方案 A：顯示但卡片為灰色 + 不可拖曳（需修改 `SalesOrder.IsApproved` 判斷）
  - 方案 B：完全不顯示（最簡單）

**修改位置：**
- `ProductionBoardWeekView.razor`：`BuildPendingItemsAsync` 加入審核狀態過濾
- `BoardPendingItemDto`：視需要加入 `IsApproved` 欄位
- `ProductionBoardPendingSidebar.razor`：視方案加入灰化樣式

### 3.2【高優先】N+1 查詢問題（效能）

**問題：** `BuildPendingItemsAsync` 用 `foreach` 對每張訂單呼叫 `GetBySalesOrderIdWithIncludesAsync()`，造成 N+1 查詢，當訂單量大時嚴重影響效能。

**修正方向：** 一次載入所有 `SalesOrderDetail`（含 Product、SalesOrder、Customer），再在記憶體中分組。

```csharp
// 建議做法（示意）
var allDetails = await SalesOrderDetailService.GetAllWithIncludesAsync();
var detailsGrouped = allDetails
    .Where(d => d.SalesOrder?.IsApproved == true && ...)
    .GroupBy(d => d.SalesOrderId);
```

### 3.3【中優先】新增 `SalesOrderDetail.ProducedQuantity` 欄位

**目的：** 追蹤已實際生產完工（不只排程）的數量，供銷貨訂單頁面顯示生產進度。

**設計：**
- 新增 DB 欄位 `ProducedQuantity`（decimal，default 0）
- 在 `CompleteProductionAsync()` 執行時累加此欄位
- 需要新增 EF Migration

**影響範圍：**
- `Data/Entities/Sales/SalesOrderDetail.cs`
- `Services/ProductionManagement/ProductionScheduleItemService.cs`（`CompleteProductionAsync`）
- Migration 檔案

### 3.4【中優先】日內優先順序（拖曳排序）

**需求：** 同一天內的排程卡片應可上下拖曳調整優先順序，越上面越先生產。

**設計概念：**
- `ProductionScheduleItem.Priority`（已有此欄位）
- 在同日欄內拖曳時，批次更新被影響卡片的 Priority 值
- UI 渲染時依 Priority 升序排列

**待設計：**
- `IProductionScheduleItemService` 新增 `UpdatePrioritiesAsync(List<(int Id, int Priority)>)` 方法
- `ProductionBoardDayColumn.razor` 加入日內拖曳排序邏輯

### 3.5【低優先，長期】多訂單同商品批次生產（Option B）

**業務邏輯說明：**

工廠生產線只有一條，實際的生產排序規則如下：
1. 相同商品（來自不同訂單）會全部一起生產，不會中途換單
2. 沒有重複的商品，則依訂單優先順序（看板欄位從上到下）生產
3. 拖曳訂單組的上下順序即代表優先度

範例：
```
訂單A：商品A、B、C
訂單B：商品B、C、E

實際生產順序（訂單A優先時）：
  商品B（A+B合批）→ 商品C（A+B合批）→ 商品A（僅A單）→ 商品E（僅B單）
```

**目前狀況：** 系統目前每筆 SalesOrderDetail 對應一張排程卡片，尚無批次合單問題。

**未來架構（Option B）：** 新增 `ProductionScheduleItemSource` 中介表，一張排程卡片可關聯多筆 SalesOrderDetail。

```
ProductionScheduleItem
  ├── ProductId（商品）
  ├── ScheduledQuantity（合計批次量）
  └── Sources（List<ProductionScheduleItemSource>）
        ├── SalesOrderDetailId
        ├── AllocatedQuantity（此訂單佔的量）
        └── Priority（完工時的分配順序）
```

完工分配邏輯：
- 按來源訂單的優先順序依序填滿
- `CompleteProductionAsync` 需能處理部分完工分配

---

## 四、技術說明備忘

### 4.1 `ProductionScheduleItem` 關鍵欄位

| 欄位 | 說明 |
|------|------|
| `ProductionScheduleId` | 關聯 ProductionSchedule（每日批次主檔） |
| `ProductId` | 生產商品 |
| `SalesOrderDetailId` | 來源訂單明細（nullable，無 SalesOrder 來源時為 null） |
| `ScheduledQuantity` | 計畫生產數量 |
| `CompletedQuantity` | 已完工數量 |
| `ProductionItemStatus` | WaitingMaterial / InProgress / Completed |
| `PlannedStartDate` | 計畫生產日期（null = 尚未排入看板） |
| `Priority` | 同日內的優先順序 |
| `IsClosed` | 是否已結案 |

### 4.2 `CanDeleteAsync` 的限制

`ProductionScheduleItemService.CanDeleteAsync()` 只允許 `ProductionItemStatus.Pending` 狀態的項目被 `PermanentDeleteAsync()` 刪除。

**不要**用 `PermanentDeleteAsync` 退回卡片。**必須**使用 `ReturnToSidebarAsync`。

### 4.3 `ServiceResult<bool>` 的語義

`ReturnToSidebarAsync` 回傳 `ServiceResult<bool>`：
- `IsSuccess = false` → 操作失敗，`ErrorMessage` 說明原因
- `IsSuccess = true, Data = false` → 退回成功，無特殊狀況
- `IsSuccess = true, Data = true` → 退回成功，但有已發領料記錄，提示使用者手動沖銷庫存

---

## 五、相關檔案索引

| 類型 | 路徑 |
|------|------|
| 主視圖 | `Components/Pages/Products/ProductionBoard/ProductionBoardWeekView.razor` |
| 服務介面 | `Services/ProductionManagement/IProductionScheduleItemService.cs` |
| 服務實作 | `Services/ProductionManagement/ProductionScheduleItemService.cs` |
| 批次排程服務 | `Services/ProductionManagement/ProductionScheduleService.cs` |
| DTO 模型 | `Models/Schedule/BoardScheduleItemDto.cs`、`BoardPendingItemDto.cs` |
| 實體 | `Data/Entities/ProductionManagement/ProductionScheduleItem.cs` |
| 訂單明細實體 | `Data/Entities/Sales/SalesOrderDetail.cs` |
| 鎖定 Helper | `Helpers/InteractiveTableComponentHelper/DetailLockHelper.cs` |
