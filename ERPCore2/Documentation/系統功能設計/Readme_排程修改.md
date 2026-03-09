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
| `ProductionCompletionModal.razor` | 同上 | 單筆完工確認 Modal（目前未使用）|

### 1.2 資料流程（現況說明，含待修正問題）

```
SalesOrder（已審核）
  └── SalesOrderDetail（訂單明細）
        ├── OrderQuantity     = 訂購總量
        ├── ScheduledQuantity = 已排入看板的總量（DB 欄位，供鎖定明細列用）
        ├── ProducedQuantity  = 已實際完工入庫數量（待新增，見§3.4）
        └── PendingQuantity   = OrderQuantity - ScheduledQuantity（待排量，計算屬性）

待排 Sidebar 計算邏輯（BuildPendingItemsAsync）：
  - 取得所有 SalesOrder（目前含未審核訂單 — 待修正，見§3.1）
  - 逐訂單載入 SalesOrderDetail（N+1 問題 — 待優化，見§3.2）
  - 以 GetScheduledQuantityMapAsync() 取得即時已排量 map
  - PendingQuantity = OrderQuantity - scheduledMap[detail.Id]
  - PendingQuantity > 0 才列入 sidebar

排入看板（從 sidebar 拖曳到日期欄）：
  1. GetOrCreateDailyBatchAsync() 取得或建立當天的 ProductionSchedule 批次
  2. 建立 ProductionScheduleItem（狀態：WaitingMaterial）
  3. 回寫 SalesOrderDetail.ScheduledQuantity += PendingQuantity
  4. 展開 BOM → 建立 ProductionScheduleDetail 列表
  5. 若 BOM 無需求量 → 升格狀態為 InProgress（無需領料）
  6. 若 BOM 有需求量 → CheckBomStockWarningAsync（庫存不足顯示警告，不阻擋）
  7. 詢問是否建立領料單（GenericConfirmModalComponent）

退回待排（從看板拖回 sidebar，或在 EditModal 點退回）：
  - 呼叫 ReturnToSidebarAsync(itemId)
  - 條件：CompletedQuantity == 0
  - 原子操作：還原 SalesOrderDetail.ScheduledQuantity + 刪除 ProductionScheduleDetails + 刪除 Item
  - 回傳 ServiceResult<bool>：bool=true 表示有已發領料記錄，UI 顯示額外警告

ProductionScheduleItem 確認的狀態機：
  WaitingMaterial → [材料全部領齊 → 自動] or [手動強制開始 + 警告確認] → InProgress
  InProgress → [最後一次完工入庫] → Completed + IsClosed=true（自動同時發生，卡片消失）
  任何狀態（CompletedQuantity==0）← ReturnToSidebarAsync ← 退回待排
```

### 1.3 雙軌 ScheduledQuantity 設計

| 資料來源 | 類型 | 用途 |
|---------|------|------|
| `SalesOrderDetail.ScheduledQuantity` | DB 欄位（冗餘） | `DetailLockHelper` 判斷能否刪除訂單明細列 |
| `GetScheduledQuantityMapAsync()` | 即時 DB 聚合 | Sidebar 計算 PendingQuantity（準確，不受 race condition 影響） |

兩者必須同步更新：排入 +、退回 -。

---

## 二、已完成的修改

### 2.1 修復退回功能（Bug Fix）

**根本原因：** 原本程式使用 `PermanentDeleteAsync()`，但 `CanDeleteAsync()` 只允許 `ProductionItemStatus.Pending` 狀態，而看板卡片實際狀態為 `WaitingMaterial`，導致刪除永遠靜默失敗。前端未檢查回傳值，顯示成功訊息但資料未異動，`ScheduledQuantity` 未被還原（資料損毀）。

**修正方式：** 新增 `ReturnToSidebarAsync(itemId)`，繞過 `CanDeleteAsync`，只驗證 `CompletedQuantity == 0`，並用原子操作完成全部動作。

**修改的檔案：**
- `Services/ProductionManagement/IProductionScheduleItemService.cs`
- `Services/ProductionManagement/ProductionScheduleItemService.cs`
- `Components/Pages/Products/ProductionBoard/ProductionBoardItemEditModal.razor`
- `Components/Pages/Products/ProductionBoard/ProductionBoardWeekView.razor`

### 2.2 排入時 BOM 庫存警告

拖曳排入看板後，若 BOM 有需求量，自動呼叫 `CheckBomStockWarningAsync()`，庫存不足時顯示 Warning（不阻擋排程），提醒安排採購或調撥。

---

## 三、已確認的設計決策

### 3.1 完工入庫倉庫選擇（Q1）

**決策：** 使用者手動選擇倉庫與庫位，系統智慧預填上一次同商品的入庫位置。

**實作細節：**
- 查詢 `ProductionScheduleCompletion`，JOIN `ProductionScheduleItem`（取 `ProductId`），找最後一筆 `ProductionScheduleItem.ProductId == 目前商品` 的記錄，取其 `WarehouseId` / `WarehouseLocationId` 作為預設值（`ProductionScheduleCompletion` 本身無 `ProductId` 欄位，需透過 join 取得）
- 使用者確認或修改後送出
- `ProductionBoardItemEditModal` 確認入庫區塊需新增倉庫/庫位選擇器
- `ProductionBoardBatchCompleteModal` 批次完工時，每行各自預填（同商品上次位置），可逐一調整

**影響範圍：**
- `ProductionBoardItemEditModal.razor` — 入庫區塊加倉庫/庫位下拉
- `ProductionBoardBatchCompleteModal.razor` — 每行加倉庫/庫位欄（或展開詳細設定）
- `IProductionScheduleItemService` 可新增 `GetLastCompletionWarehouseAsync(int productId)` helper

### 3.2 開始生產的觸發機制（Q2 更新後）

**最終設計：** 以領料狀態自動驅動，減少手動步驟。

**觸發規則：**
- **材料全部領齊**（`materialComplete == true` **且** `hasBom == true`）→ `HandleMaterialIssued` 完成後自動呼叫 `StartProductionAsync`，卡片自動變為 InProgress
- **材料未齊但要強制開始**（BOM 未領完仍要生產）→ EditModal 顯示橘色警告 + 「強制開始生產」按鈕，使用者確認後呼叫 `StartProductionAsync`
- **無 BOM 商品**（`hasBom == false`）→ 拖入看板後停在 WaitingMaterial，需手動點「開始生產」按鈕；`materialComplete` 對無 BOM 商品為 `true`，但**不觸發**自動升格，避免恢復舊的自動升格行為

**EditModal WaitingMaterial 狀態的按鈕配置：**
```
[ 領料 ]  +  [ 開始生產（綠，材料齊全時顯示）]  or  [ 強制開始（橘，材料未齊時顯示）]  +  [ 退回 ]
警告文字（materials not complete）：「尚有 X 項組件未領齊，建議先完成領料再開始生產」
```

**目前問題（需修正）：**
- `StartProductionAsync` 只允許 `Pending` 狀態，看板卡片是 `WaitingMaterial`，該方法是**死碼**
- 無 BOM 商品拖入時，程式直接設成 InProgress，`ActualStartDate` 未設定（漏設）

**修正方向：**
- `StartProductionAsync`：改為允許 `WaitingMaterial` 和 `Pending` 兩種狀態
- 拖入看板後，不管有無 BOM 一律保持 `WaitingMaterial`（移除無 BOM 自動升格）

### 3.3 排程數量不可修改（Q3）

**決策：** 已排入看板的卡片數量來自訂單，不允許在看板中隨意修改。若需調整，應回到銷貨訂單修改訂單數量。

**目前問題：** `ProductionBoardItemEditModal` 的「排程數量」欄位是可編輯的 `<input>`，且儲存邏輯不會同步更新 BOM 需求量，修改會造成資料不一致。

**修正方向：**
- 將排程數量改為唯讀顯示（移除可編輯 input）
- 移除 `HandleSave` 中的 `qtyDelta` 計算與 `SalesOrderDetail.ScheduledQuantity` 更新邏輯
- 整個「儲存」按鈕可移除（若無其他可編輯欄位）

**剩餘料處理（部分完工後的二次領料）：**
- 剩餘數量 = `ScheduledQuantity - CompletedQuantity`（系統自動計算）
- 二次領料時：系統預算各組件剩餘需求 = `RequiredQuantity * (剩餘數量 / ScheduledQuantity)`
- 使用者可在領料單中手動調整各組件的實際領料量
- 退料與損耗：嵌入最後一次完工確認步驟（見§3.5）

### 3.5 完工確認 + 用料結算（已確認設計）

**核心決策：**
- 退料與損耗**不建立獨立模組**，嵌入完工確認步驟
- 只在**最後一次完工**（`CompletedQty + 本次入庫量 >= ScheduledQuantity`）才顯示用料結算
- 分批完工中途不做結算，讓工廠彈性補料

**理由：** 完工確認和用料結算是「同一事件的兩個面向」（同時間、同人、同動作），不像進貨→收料是跨時間的兩個事件，不需要分開成兩張有獨立單號的文件。

**現有領料單角色不變：**

| | 領料單 (MaterialIssue) | 用料結算（嵌入完工） |
|---|---|---|
| 時機 | 生產前/中，隨時可建立 | 最後一次完工時 |
| 目的 | 記錄「領什麼料、從哪領」，扣庫存 | 記錄「料的最終去向」，歸還退料 |
| 單號 | 有（MI-yyyyMMdd-xxxx） | 無，附屬於 ProductionScheduleCompletion |
| 可多次 | 是（補料可再建一張） | 否（最終結算只做一次） |

**完工確認 Modal 擴展設計：**

```
完工確認（最後一次入庫時）
  ├── Section 1：成品入庫
  │     ├── 本次入庫數量（唯讀：剩餘數量）
  │     └── 入庫倉庫 / 庫位（預填：同商品上次完工位置）
  │
  └── Section 2：用料結算（IssuedQuantity > 0 的組件才顯示）
        └── 每個 BOM 組件一行：
              ├── 組件名稱（唯讀）
              ├── 已領量（唯讀，IssuedQuantity）
              ├── 實際用量（使用者填）
              ├── 退料量（使用者填）→ 退料倉庫選擇
              └── 損耗量（使用者填）+ 損耗備註（文字）
              驗證：實際用量 + 退料量 + 損耗量 = 已領量
```

**資料儲存：** 結算欄位加到 `ProductionScheduleDetail`，不需要新資料表

```
ProductionScheduleDetail（新增欄位）
  ├── ActualUsedQty       decimal（實際消耗）
  ├── ReturnQty           decimal（退料量）
  ├── ReturnWarehouseId   int?（退料目標倉庫，使用者選）
  ├── ReturnLocationId    int?（退料目標庫位，可選）
  ├── ScrapQty            decimal（損耗量）
  └── ScrapReason         string?（損耗備註，純文字）
```

**庫存影響：**
- `ReturnQty > 0` → `AddStockAsync(componentProductId, ReturnWarehouseId, ReturnQty, MaterialReturn)`
- `ScrapQty > 0` → 僅記錄，不動庫存（料已在領料時出庫，損耗代表消失）
- `ActualUsedQty` → 不動庫存，供成本分析

**批次完工不觸發結算：** `ProductionBoardBatchCompleteModal` 傳入的 `settlements = null`，`CompleteProductionAsync` 當 settlements 為 null 時跳過結算邏輯。需要退料/損耗記錄的情況應改用單張 EditModal。

**需要新增的 EF Migration：** `ProductionScheduleDetail` 加上述 6 個欄位

### 3.6 自動結案（Q4 更新後）

**最終設計：** 取消獨立結案步驟，最後一次完工入庫時自動結案。

**觸發條件：** `isLastCompletion = (item.CompletedQuantity + completeQty >= item.ScheduledQuantity)`

**自動執行：**
- `ProductionItemStatus = Completed`
- `IsClosed = true`
- `ActualEndDate = DateTime.Now`
- 三個欄位在同一次 `SaveChangesAsync` 寫入

**影響：**
- 不需要獨立的「結案」按鈕
- 不需要新增 `ProductionBoard.Close` 權限
- §4.7（結案按鈕）待辦項目已移除
- 已結案的卡片從看板消失後，仍可在歷史查詢頁面查看（見§4.12）

---

## 四、待辦事項（需要繼續修改或加強）

### 4.1【緊急 Bug】完工後成品庫存未更新 + 成本計算

**問題：** `CompleteProductionAsync` 只建立 `ProductionScheduleCompletion` 記錄，**沒有寫入 InventoryStock**。

**Transaction Number 模式（參照進貨收料）：**
```csharp
// 進貨收料的模式：
AddStockAsync(productId, warehouseId, qty,
    InventoryTransactionTypeEnum.Purchase,
    purchaseReceiving.Code,    // ← 以單據代碼作為交易號
    detail.UnitPrice,          // ← 採購單價作為成本
    ...)

// 生產完工對應：
AddStockAsync(productId, warehouseId, completedQty,
    InventoryTransactionTypeEnum.ProductionCompletion,
    productionSchedule.Code,   // ← 每日批次代碼（PS-yyyyMMdd-xxxx）
    calculatedUnitCost,        // ← 見成本計算設計
    warehouseId, locationId,
    sourceDocumentType: InventorySourceDocumentTypes.ProductionCompletion,
    sourceDocumentId: productionScheduleItemId)
```

**成品成本計算設計（確認後設計，見 §四b D1-D4）：**

三個階段串接：

**階段 1 — 進貨時（已實作）**
- `PurchaseReceivingService` 確認進貨 → `AddStockAsync(batchNumber = detail.BatchNumber)`
- 每批進貨建立獨立 `InventoryStockDetail` 行，各自有 `AverageCost`，FIFO 基礎已到位

**階段 2 — 領料時寫入組件成本**（修改 `MaterialIssueService`）
- 改呼叫 `ReduceStockWithFIFOAsync`（回傳 `ServiceResult<decimal>` 加權成本）
- 將回傳的加權成本寫入 `detail.UnitCost`（不依賴 UI 手動輸入）
- 修正既有 Bug：`ReduceStockAsync` 多批次時 `FirstOrDefault` 取到非最舊批

**階段 3 — 完工時計算成品單位成本**（在 `CompleteProductionAsync`）
```csharp
// 對每個 BOM 組件，從所有領料明細計算加權平均成本
foreach (var bomDetail in bomDetails)
{
    var relatedIssues = await context.MaterialIssueDetails
        .Where(d => d.ProductionScheduleDetailId == bomDetail.Id && d.UnitCost.HasValue)
        .ToListAsync();

    if (relatedIssues.Any())
    {
        var totalQty = relatedIssues.Sum(d => d.IssueQuantity);
        var totalCost = relatedIssues.Sum(d => d.IssueQuantity * d.UnitCost!.Value);
        bomDetail.ActualUnitCost = totalQty > 0 ? totalCost / totalQty : null;
    }
}

// 計算成品單位成本（依排程數量分攤總物料成本）
var totalMaterialCost = bomDetails.Sum(d => d.RequiredQuantity * (d.ActualUnitCost ?? 0));
var productionUnitCost = item.ScheduledQuantity > 0
    ? totalMaterialCost / item.ScheduledQuantity
    : null;
```

**需要修改：**
- `IInventoryStockService`：`ReduceStockWithFIFOAsync` 回傳改為 `ServiceResult<decimal>`
- `InventoryStockService`：FIFO 迴圈累計各批成本，結束後回傳加權平均
- `MaterialIssueService.CreateAsync`：改呼叫 FIFO 版，寫入 `detail.UnitCost`
- `ProductionScheduleItemService`：注入 `IInventoryStockService`，`CompleteProductionAsync` 實作階段 3 + 呼叫 `AddStockAsync(ProductionCompletion)`
- `CompleteProductionAsync` 最後一次完工補設 `item.IsClosed = true`（目前漏設）

**已存在（無需新增）：**
- `InventorySourceDocumentTypes.ProductionCompletion` 常數 — 已在 `InventorySourceDocumentTypes.cs:66`
- `ProductionScheduleCompletion.WarehouseId` / `WarehouseLocationId` — 欄位已存在
- `ProductionScheduleCompletion.InventoryTransactionId` — 暫留 null（D3 決策 A）

### 4.2【高優先】過濾未審核與已拒絕訂單

**問題：** `BuildPendingItemsAsync` 呼叫 `SalesOrderService.GetAllAsync()`，回傳所有訂單（含未審核、已拒絕）。

**期望行為：**
- 已拒絕訂單 → 不顯示
- 未審核訂單 → 顯示但灰化 + 不可拖曳（提示「待審核」）

**修改位置：**
- `ProductionBoardWeekView.razor`：`BuildPendingItemsAsync` 加審核狀態過濾
- `Models/Schedule/BoardPendingItemDto.cs`：加入 `IsApproved` 欄位
- `ProductionBoardPendingSidebar.razor`：加灰化樣式與拖曳鎖定

### 4.3【高優先】N+1 查詢問題（效能）

**問題：** `BuildPendingItemsAsync` 用 `foreach` 逐訂單查詢 `SalesOrderDetail`，N 張訂單 = N+1 次 DB 查詢。

**修正方向：** 一次查詢所有符合條件的 `SalesOrderDetail`（含 Include），在記憶體中分組。

### 4.4【高優先】排入看板非原子操作

**問題：** `AssignPendingItemAsync` 的執行是三步非原子操作（CreateItem → UpdateScheduledQty → ExpandBom）。若中途失敗，資料不一致。

**修正方向：** 用同一個 `DbContext` 包住所有步驟，一次 `SaveChangesAsync`。

### 4.5【中優先】新增「開始生產」按鈕（對應§3.2）

**修改位置：**
- `StartProductionAsync`：移除僅允許 `Pending` 的限制，改為允許 `WaitingMaterial` 和 `Pending`
- `ProductionBoardItemEditModal.razor`：加入「開始生產」按鈕邏輯
- 無 BOM 商品拖入看板時，不再自動升格為 InProgress（保持 WaitingMaterial）

### 4.6【中優先】完工入庫加倉庫/庫位欄位（對應§3.1）

**修改位置：**
- `ProductionBoardItemEditModal.razor`：確認入庫區塊加 Warehouse / WarehouseLocation 選擇器，預填上次同商品入庫位置
- `ProductionBoardBatchCompleteModal.razor`：每行加倉庫欄，預填邏輯同上
- 可新增 `GetLastCompletionWarehouseAsync(int productId)` service 方法

### 4.7【已取消】結案功能 ~~（對應§3.4）~~ → 改為自動結案（見§3.6）

**已取消原因：** 根據§3.6 決策，最後一次完工入庫時系統自動設定 `IsClosed = true`，不需要獨立結案按鈕，也不需要額外權限。此待辦已廢除。

### 4.8【中優先】完工確認擴展：加入用料結算（對應§3.5）

**設計已確認（詳見§3.5），無需新模組。**

**DB 變更：** 新增 EF Migration，`ProductionScheduleDetail` 加 6 個欄位：
`ActualUsedQty`, `ReturnQty`, `ReturnWarehouseId`, `ReturnLocationId`, `ScrapQty`, `ScrapReason`

**Service 變更：** `CompleteProductionAsync` 新增 `settlements` 參數（D5 決策，不拆獨立方法，以確保退料與成品入庫在同一 Transaction）：
- 針對每筆 `ReturnQty > 0`：呼叫 `IInventoryStockService.AddStockAsync(..., MaterialReturn)`
- 針對每筆 `ScrapQty > 0`：寫入 `ProductionScheduleDetail.ScrapQty`，不動庫存
- 所有操作包在同一 DB Transaction（含成品入庫）

**UI 變更：** `ProductionBoardItemEditModal` 確認入庫區塊，當 `isLastCompletion == true` 時展開用料結算 section（見§3.5 UI 設計）。

**觸發條件：** `isLastCompletion = (item.CompletedQuantity + completeQty >= item.ScheduledQuantity)`

### 4.9【中優先】排程數量欄位改唯讀（對應§3.3）

**修改位置：**
- `ProductionBoardItemEditModal.razor`：排程數量改唯讀
- 移除或簡化 `HandleSave` 邏輯（若無其他可編輯欄位，移除儲存按鈕）

### 4.10【中優先】新增 `SalesOrderDetail.ProducedQuantity` 欄位

**目的：** 在銷貨訂單頁面顯示每個明細的實際完工進度。

**實作：**
- `Data/Entities/Sales/SalesOrderDetail.cs`：新增 `ProducedQuantity decimal default 0`
- `CompleteProductionAsync`：完工時累加 `SalesOrderDetail.ProducedQuantity += completedQuantity`
- 新增 EF Migration

### 4.11【低優先】日內優先順序（拖曳排序）

**需求：** 同一天欄內的卡片可上下拖曳，越上面越優先生產。

**設計：**
- 利用現有 `ProductionScheduleItem.Priority` 欄位
- `IProductionScheduleItemService` 新增 `UpdatePrioritiesAsync(List<(int Id, int Priority)>)` 方法
- `ProductionBoardDayColumn.razor` 加入日內拖曳排序邏輯
- Sidebar 的「訂單優先順序」欄位目前不存在，這是更長期的設計（見§4.12）

### 4.12【低優先】歷史查詢頁面

**背景：** 結案後的卡片（`IsClosed = true`）從看板隱藏，需要有歷史頁面查詢。

**待設計：** 獨立的歷史列表頁，可依日期、商品、客戶篩選，顯示已結案排程及其完工紀錄。

### 4.13【長期】多訂單同商品批次生產（Option B）

**業務邏輯：** 同一商品來自不同訂單時，全部合批一次生產；無重複商品依訂單優先順序生產。

目前每筆 `SalesOrderDetail` 對應一張卡片，尚無合批問題。

**未來架構：** 新增 `ProductionScheduleItemSource` 中介表：

```
ProductionScheduleItem（合批卡片）
  ├── ProductId（商品）
  ├── ScheduledQuantity（合計量）
  └── Sources
        ├── SalesOrderDetailId
        ├── AllocatedQuantity（此訂單佔量）
        └── Priority（完工分配順序）
```

---

## 四b、已確認設計決策（成本計算）

### D4【已確認】領料改用 FIFO + 回傳加權成本（選 B：擴充法）

**決策：** 修改 `ReduceStockWithFIFOAsync` 回傳 `ServiceResult<decimal>`（加權平均成本），`MaterialIssueService.CreateAsync` 改呼叫此方法並將回傳成本寫入 `detail.UnitCost`。

**影響範圍：**
- `IInventoryStockService`：`ReduceStockWithFIFOAsync` 回傳型別改為 `ServiceResult<decimal>`
- `InventoryStockService`：FIFO 迴圈內累計 `Σ(qty × batchCost)`，結束後計算加權平均，包入回傳值
- `MaterialIssueService.CreateAsync`：改呼叫 FIFO 版本；將 `result.Data` 寫入 `detail.UnitCost`；更新 `context.SaveChanges`
- 現有銷貨呼叫者：只檢查 `IsSuccess`，不受影響

**修正的既有 Bug：** `ReduceStockAsync` 在多批次情況下 `FirstOrDefault` 取到非最舊批次，FIFO 版本依 `BatchDate` 正確排序。

### D1【已確認】多次領料同組件 → 完工時延遲計算（選 C）

**決策：** `ProductionScheduleDetail.ActualUnitCost` **不在領料時寫入**，改為在完工時（`CompleteProductionAsync`）從所有關聯的 `MaterialIssueDetail` 計算加權平均：

```csharp
// 完工時計算每個 BOM 組件的實際成本
foreach (var bomDetail in bomDetails)
{
    var relatedIssues = await context.MaterialIssueDetails
        .Where(d => d.ProductionScheduleDetailId == bomDetail.Id && d.UnitCost.HasValue)
        .ToListAsync();

    if (relatedIssues.Any())
    {
        var totalQty = relatedIssues.Sum(d => d.IssueQuantity);
        var totalCost = relatedIssues.Sum(d => d.IssueQuantity * d.UnitCost!.Value);
        bomDetail.ActualUnitCost = totalQty > 0 ? totalCost / totalQty : null;
    }
}

// 計算成品單位成本
var totalMaterialCost = bomDetails.Sum(d => d.RequiredQuantity * (d.ActualUnitCost ?? 0));
var productionUnitCost = item.ScheduledQuantity > 0
    ? totalMaterialCost / item.ScheduledQuantity
    : null;
```

**前提：** D4-B 確保 `MaterialIssueDetail.UnitCost` 一定有值（由服務自動填入 FIFO 加權成本，不依賴 UI 手動輸入）。

### D2【已確認，由 D4-B 解決】`MaterialIssueDetail.UnitCost` 自動填入

**決策：** 服務層（`MaterialIssueService.CreateAsync`）在 FIFO 扣減後自動寫入 `detail.UnitCost = fifoResult.Data`，不依賴 UI 填入，也不需要確認現有 UI 行為。

### D3【已確認】`ProductionScheduleCompletion.InventoryTransactionId` → 暫留 null（選 A）

**決策：** 短期暫留 null，不影響業務。`AddStockAsync` 介面不改動，待 Phase 4 視需求以查詢法（B）或介面擴充（C）補充。

### D5【已確認】用料結算的服務層實作 → settlements 參數法

**決策：** 在 `CompleteProductionAsync` 加入 `settlements` 參數，退料的 `AddStockAsync` 與成品入庫的 `AddStockAsync` 在同一個 DB Transaction 內執行，保證原子性。不拆出獨立方法。

```csharp
// 新簽名（概念）
Task<ServiceResult> CompleteProductionAsync(
    int itemId, decimal completedQuantity,
    int? warehouseId = null, int? warehouseLocationId = null,
    List<MaterialSettlementDto>? settlements = null)
// settlements == null → 批次完工，跳過結算邏輯
// settlements != null → 單筆最後一次完工，執行退料 AddStockAsync
```

**`MaterialSettlementDto` 欄位定義**（對應 §3.5 用料結算的每個 BOM 組件行）：

```csharp
public class MaterialSettlementDto
{
    public int ProductionScheduleDetailId { get; set; } // 對應的 BOM 組件行
    public decimal ActualUsedQty { get; set; }           // 實際消耗量
    public decimal ReturnQty { get; set; }               // 退料量（0 = 不退）
    public int? ReturnWarehouseId { get; set; }          // 退料目標倉庫（ReturnQty > 0 時必填）
    public int? ReturnLocationId { get; set; }           // 退料目標庫位（可選）
    public decimal ScrapQty { get; set; }                // 損耗量（0 = 無損耗）
    public string? ScrapReason { get; set; }             // 損耗備註（純文字）
    // 驗證：ActualUsedQty + ReturnQty + ScrapQty == IssuedQuantity
}
```

### D6【已確認】分批完工的成本計算 → 每次完工都計算

**決策：** 每次完工時都從 `MaterialIssueDetail` 計算當下的加權平均成本，傳入 `AddStockAsync`。

**理由：**
- 查詢量極小（BOM 組件通常 5-20 筆），不浪費伺服器資源
- 「最後一次才傳成本，中間傳 null」會導致前幾筆入庫的 `InventoryStockDetail.AverageCost` 無法正確計算，反而更複雜
- 每次傳「當下最佳估計值」，最後一次傳最準確值，InventoryStock 移動平均自動收斂

### D7【已確認】`CompleteProductionAsync` 不強制要求 InProgress 狀態

**決策：** 不在服務層驗證「必須 InProgress 才能完工」，保留彈性（允許小工廠略過「開始生產」步驟直接完工）。

**補充：** 若 `item.ActualStartDate == null` 時呼叫 `CompleteProductionAsync`，自動補設 `ActualStartDate = DateTime.Now`，確保時間紀錄完整，不遺漏生產開始時間。

---

## 五、技術備忘

### 5.1 `ProductionScheduleItem` 關鍵欄位

| 欄位 | 說明 |
|------|------|
| `ProductionScheduleId` | 關聯每日批次主檔 |
| `ProductId` | 生產商品 |
| `SalesOrderDetailId` | 來源訂單明細（nullable） |
| `ScheduledQuantity` | 計畫生產數量（不可在看板隨意修改） |
| `CompletedQuantity` | 已完工累計數量 |
| `ProductionItemStatus` | WaitingMaterial / InProgress / Completed |
| `PlannedStartDate` | 計畫生產日（null = 尚未排入） |
| `ActualStartDate` | 實際開始時間（手動點「開始生產」時記錄） |
| `ActualEndDate` | 實際完工時間（完工時記錄） |
| `Priority` | 同日內的優先順序 |
| `IsClosed` | 是否已結案（**自動**：最後一次完工入庫時自動設定，與 `Completed` 同一次 SaveChanges） |

### 5.2 狀態機完整說明

```
[排入看板] → WaitingMaterial
               │
               ├─（領料）→ MaterialIssue 建立
               │
               ├─ hasBom=true 且材料全部領齊 → 自動呼叫 StartProductionAsync
               │   或
               ├─ hasBom=false → 等待手動點「開始生產」（綠色按鈕，不自動觸發）
               │   或
               └─ 手動「強制開始生產」（hasBom=true 但材料未齊，顯示橘色警告）
                                 ↓
                             InProgress
                               │
                               ↓ [確認入庫]（可多次分批，選擇倉庫/庫位）
                           CompletedQuantity 累加
                               │
                               └─ 最後一次完工（CompletedQty >= ScheduledQty）
                                     ↓ 自動，同一 SaveChanges
                                 Completed + IsClosed = true
                                 （卡片從看板消失，歷史頁面查詢，見§4.12）

任何狀態（CompletedQuantity == 0）←─ 退回待排 ─ ReturnToSidebarAsync
```

### 5.3 `CanDeleteAsync` 限制

`CanDeleteAsync()` 只允許 `Pending` 狀態。**禁止**用 `PermanentDeleteAsync` 退回卡片，必須使用 `ReturnToSidebarAsync`。

### 5.4 批次完工的狀態升格重複問題

`CompleteProductionAsync`（Service）和前端（EditModal / BatchCompleteModal）各自有一段「CompletedQuantity >= ScheduledQuantity → 設 Completed → UpdateAsync」邏輯，執行兩次但結果相同。應整合：前端不再重複設狀態，只呼叫 service，由 service 統一負責狀態升格。

---

## 六、分階段實作計畫

> 每個 Phase 可獨立部署，不會破壞現有功能。Phase 內的項目盡量依序執行。

### Phase 1 — DB 基礎（只跑 Migration，零風險）

| # | 項目 | 對應待辦 |
|---|------|---------|
| 1 | `ProductionScheduleDetail` 新增 6 欄位（結算用） | §4.8 |
| 2 | `SalesOrderDetail` 新增 `ProducedQuantity` 欄位 | §4.10 |

完成後：兩支 Migration 跑完，既有功能完全不受影響，只是多了空欄位。

---

### Phase 2 — 修正服務層邏輯缺陷（後端，無 UI 變更）

| # | 項目 | 對應待辦 |
|---|------|---------|
| 1 | `ReduceStockWithFIFOAsync` 回傳改為 `ServiceResult<decimal>`（回傳加權平均成本） | §四b D4 |
| 2 | `MaterialIssueService.CreateAsync` 改呼叫 FIFO 版，將回傳成本寫入 `detail.UnitCost` | §四b D4 |
| 3 | `ProductionScheduleItemService` 注入 `IInventoryStockService`；`CompleteProductionAsync` 每次完工都從 `MaterialIssueDetail` 計算當下加權成本（D6），呼叫 `AddStockAsync(ProductionCompletion)` 寫入成品庫存；最後一次完工同時設 `item.IsClosed = true`；若 `ActualStartDate == null` 補設為當下時間（D7） | §4.1, §3.6 |
| 4 | `CompleteProductionAsync` 完工時累加 `SalesOrderDetail.ProducedQuantity` | §4.10 |
| 5 | `StartProductionAsync` 移除只允許 `Pending` 的限制，改為允許 `WaitingMaterial` 和 `Pending` | §4.5 |
| 6 | `AssignPendingItemAsync` 改為原子操作（同一 DbContext，一次 SaveChanges） | §4.4 |
| 7 | `BuildPendingItemsAsync` 過濾已拒絕訂單；未審核訂單標記 IsApproved=false 送往 UI | §4.2 |
| 8 | `BuildPendingItemsAsync` 改為一次查詢所有 SalesOrderDetail，消除 N+1 | §4.3 |

完成後：領料正確 FIFO 扣庫並記錄各批成本；完工可計算精確成本並寫入成品庫存；排入看板是原子安全的；Sidebar 不再出現拒絕訂單。

---

### Phase 3 — EditModal 改版（UI 主要改動）

| # | 項目 | 對應待辦 |
|---|------|---------|
| 1 | 排程數量欄位改為唯讀，移除 HandleSave 數量差異邏輯 | §4.9 |
| 2 | 前端移除重複的狀態升格邏輯（由 service 統一負責） | §5.4 |
| 3 | 拖入看板一律保持 `WaitingMaterial`（移除無 BOM 自動升格） | §4.5 |
| 4 | EditModal 加入「開始生產」按鈕（`WaitingMaterial` 狀態時顯示） | §4.5 |
| 5 | 完工確認區塊加倉庫 / 庫位選擇器（預填上次同商品入庫位置） | §4.6 |
| 6 | Sidebar 灰化未審核訂單，鎖定拖曳功能 | §4.2 |
| 7 | 最後一次完工時展開「用料結算」section（實際用量 / 退料 / 損耗） | §4.8 |
| 8 | `ProductionBoardBatchCompleteModal` 每行加倉庫 / 庫位選擇器 | §4.6 |

完成後：EditModal 流程與確認的狀態機完全對齊；完工可同步退料入庫並記錄損耗。

---

### Phase 4 — 補強功能

| # | 項目 | 對應待辦 |
|---|------|---------|
| 1 | 日內卡片拖曳排序，更新 Priority，`UpdatePrioritiesAsync` | §4.11 |
| 2 | 已結案排程歷史查詢頁面 | §4.12 |

---

### Phase 5 — 長期架構（待另立設計文件）

| # | 項目 | 對應待辦 |
|---|------|---------|
| 1 | 多訂單同商品合批生產（`ProductionScheduleItemSource` 中介表） | §4.13 |

---

## 七、相關檔案索引

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
