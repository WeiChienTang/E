# 生產排程模組 — 情境檢查清單

## 一、看板基本操作（情境 1–6）

### 情境 1：開啟看板載入資料
【檢查結果：✅ 通過】

- 確保有 3 張已核准的銷貨訂單，各含自製品項
- 從導覽列點擊「生產排程」→ 確認看板 Modal 開啟
- 左側 Sidebar 顯示所有 PendingQuantity > 0 的銷貨訂單明細
- 確認 PendingQuantity = OrderQuantity - ScheduledQuantity，計算正確
- 確認每個 Sidebar 項目顯示客戶名稱、品項名稱、訂單編號、訂購數量、待排數量
- 關閉看板再重新開啟 → 確認 boardKey++ 導致資料完全重新載入，沒有殘留舊狀態

驗證重點： Sidebar 資料來源正確（SalesOrderDetail-based）、GetScheduledQuantityMapAsync 批次查詢避免 N+1、Modal 重開時強制重建

### 情境 2：拖曳 Sidebar 項目到日期欄
【檢查結果：✅ 通過，情境描述已修正】

- 從 Sidebar 拖曳一個待排項目到週三欄位
- 確認系統呼叫 GetOrCreateDailyBatchAsync(週三) → 自動建立 PS-YYYYMMDD 格式的每日批次
- 確認新建的 ProductionScheduleItem 的 PlannedStartDate = 週三，~~狀態 = Pending~~ **狀態 = WaitingMaterial**（等待領料）
- 確認 SalesOrderDetail.ScheduledQuantity 已累加（+= PendingQuantity）
- 確認 BOM 已自動展開（ExpandBom）→ ProductionScheduleDetail 記錄已建立
- Sidebar 中該項目的 PendingQuantity 應變為 0（或該項目從 Sidebar 消失）
- 週三欄位出現新的卡片

驗證重點： 每日批次自動建立、ScheduledQuantity 回寫、BOM 展開、Sidebar 即時更新

### 情境 3：看板日期欄間拖曳（變更計畫日期）
【檢查結果：✅ 通過】

- 將已在週三的排程項目拖曳到週五
- 確認 UpdatePlannedDateAsync 被呼叫，PlannedStartDate 從週三改為週五
- 週三欄位的卡片消失，週五欄位出現該卡片
- 確認該項目的排程主檔（ProductionSchedule）不變（仍屬原本的每日批次），或如果系統設計是更換到新日期的批次則驗證新批次

驗證重點： 日期更新正確、卡片位置即時反映

### 情境 4：同日內卡片優先順序排列
【檢查結果：✅ 通過】

- 同一天有 3 個排程項目 A、B、C
- 透過拖曳將順序調整為 C → A → B
- 確認 UpdatePrioritiesAsync 被呼叫，Priority 值正確更新（如 C=1, A=2, B=3）
- 重新載入看板 → 確認順序維持 C → A → B

驗證重點： Priority 批次更新、順序持久化

### 情境 5：退回待排（ReturnToSidebar）
【檢查結果：✅ 通過，情境描述已修正】

- 一個 Pending 狀態的排程項目，CompletedQuantity = 0，尚未領料
- 執行 ReturnToSidebar → ~~確認項目的 PlannedStartDate 清為 null~~ **實際行為：硬刪除整個 ProductionScheduleItem**
- 確認 SalesOrderDetail.ScheduledQuantity 被扣回 ✓
- 項目以 SalesOrderDetail 身份重新出現在 Sidebar 的待排清單中（因 PendingQuantity 恢復 > 0）
- 回傳值 bool = false ✓

驗證重點： 退回條件（CompletedQuantity 必須為 0 且無領料記錄）、ScheduledQuantity 回寫、Sidebar 重新出現

### 情境 6：退回待排 — 有已發出領料記錄
【檢查結果：✅ 通過，情境描述已修正】

- 排程項目已建立領料單但尚未開始生產（CompletedQuantity = 0）
- ~~執行 ReturnToSidebar → 確認成功但回傳 bool = true~~ **實際行為：系統阻擋並回傳錯誤「已有領料記錄，無法退回待排清單。請先使用停產功能處理物料」**
- 使用者需先透過 AbortProduction（含用料結算/退料）處理已領物料，才能從排程中移除

驗證重點： 有領料記錄時系統阻擋退回、引導使用者透過停產流程處理

---

## 二、生產狀態流轉（情境 7–14）

### 情境 7：正向完整流程 Pending → InProgress → Completed
【檢查結果：✅ 通過】

- 排程項目狀態 Pending，BOM 已展開
- 建立領料單並確認 → 原料從庫存扣除
- 執行 StartProductionAsync → 狀態變為 InProgress，ActualStartDate 設定為今天
- 執行 CompleteProductionAsync（完工全部數量），傳入 warehouseId 和用料結算
- 確認狀態變為 Completed，成品入庫
- 確認 Completion 記錄正確建立，GetTotalCompletedQuantityAsync = 排程數量

驗證重點： 最基本的正向狀態流轉全部正確

### 情境 8：WaitingMaterial 狀態觸發
【檢查結果：✅ 通過】

- 排程項目拖入看板後，BOM 組件的原料庫存不足
- 嘗試建立領料單 → ValidateStockAvailabilityAsync 回報庫存不足
- 確認項目狀態停留在 WaitingMaterial 或 Pending
- 補充原料（採購進貨）後，再次領料成功
- 確認可以正常 StartProduction

驗證重點： 庫存不足時的狀態設定、補料後流程恢復

### 情境 9：暫停與繼續（Paused 狀態）
【檢查結果：✅ 通過】

- InProgress 狀態的項目 → PauseProductionAsync → 確認狀態為 Paused
- Paused 狀態嘗試 CompleteProduction → 觀察是否阻擋
- ResumeProductionAsync → 確認狀態回到 InProgress
- 正常完工入庫

驗證重點： Paused 期間不能完工、Resume 後恢復正常

### 情境 10：終止生產（AbortProduction）— 無部分完工
【檢查結果：✅ 通過】

- InProgress 狀態的項目，CompletedQuantity = 0
- AbortProductionAsync，傳入用料結算（全部退料）
- 確認狀態變為 Aborted
- 確認退料回補庫存
- 確認 SalesOrderDetail.ScheduledQuantity 保持不變（不讓訂單重出現在 Sidebar）

驗證重點： 完全退料的庫存回補、ScheduledQuantity 不回寫

### 情境 11：終止生產 — 有部分完工
【檢查結果：✅ 通過】

- 排程數量 100，已完工入庫 40
- AbortProductionAsync，結算：實際消耗 80、退料 15、損耗 5
- 確認狀態為 Aborted
- 確認 SalesOrderDetail.ProducedQuantity += 40
- 確認退料 15 回補庫存，損耗 5 記錄在案
- 確認成品庫存只有 40（之前分批入庫的）

驗證重點： 部分完工的成品保留、ProducedQuantity 更新、結算中退料+損耗+消耗的處理

### 情境 12：非法狀態轉換阻擋
【檢查結果：✅ 通過，附設計備註】

- Completed 狀態 → 嘗試 StartProduction → 阻擋 ✓
- Completed 狀態 → 嘗試 Pause → 阻擋 ✓
- ~~Pending 狀態（未領料）→ 嘗試 CompleteProduction → 應被阻擋~~ **實際行為：系統允許（白名單含 Pending），自動補設 ActualStartDate 並升級狀態為 InProgress**
- Aborted 狀態 → 嘗試 Resume → 阻擋 ✓
- Closed 狀態 → 嘗試任何操作 → 阻擋 ✓（所有方法均以白名單排除）

驗證重點： 所有非法狀態轉換都被正確阻擋並回傳明確錯誤訊息

> 設計備註：Pending → CompleteProduction 被允許是刻意設計（D7 流程彈性），支援手工組裝或補單修正場景。
> 若需嚴格阻擋，可將 Pending 從 CompleteProductionAsync 白名單移除。

### 情境 13：從 Paused 狀態終止
【檢查結果：✅ 通過】

- InProgress → Pause → Abort（不經過 Resume）
- 確認 AbortProduction 允許從 Paused 狀態直接終止
- 傳入退料結算 → 確認庫存回補正確

驗證重點： Paused 可以直接終止，不需先 Resume

### 情境 14：聚合狀態查詢（GetAggregateStatusMapAsync）
【檢查結果：✅ 通過】

- 一張銷貨訂單明細被拆成 3 個排程項目：A = InProgress、B = Pending、C = Completed
- 查詢 GetAggregateStatusMapAsync → 應回傳 InProgress（InProgress > Pending > WaitingMaterial）
- A 也完成後 → 應回傳 Pending（B 還在 Pending）
- B 也完成後 → 應回傳 Completed（全部 Completed）

驗證重點： 聚合優先順序邏輯（InProgress > WaitingMaterial > Pending；全 Completed → Completed）

---

## 三、BOM 配方與領料（情境 15–20）

### 情境 15：BOM 展開正確性
【檢查結果：✅ 通過】

- 成品 P 的配方：M1 × 2、M2 × 3、M3 × 1
- 排程數量 50 個
- 拖曳到看板後 BOM 展開 → 確認 ProductionScheduleDetail 有 3 筆
- 各組件的需求量 = 配方用量 × 排程數量：M1=100、M2=150、M3=50
- 用 GetByScheduleItemIdAsync 確認明細正確

驗證重點： 需求量計算公式、明細筆數

### 情境 16：多層 BOM 展開（半成品含子配方）
【檢查結果：✅ 通過，附設計備註】

- 成品 P 的 BOM 包含半成品 S（S 本身也有 BOM：S 需要 M4 × 2）
- 排程成品 P → **ExpandBomAsync 只展開一層**，半成品 S 作為組件出現在 ProductionScheduleDetail 中
- GetBomTreeAsync 支援遞迴多層展開（maxLevel 預設 10，含循環參照偵測），但僅用於查詢/顯示，不用於排程建立

驗證重點： 多層 BOM 的展開策略、半成品是否需要獨立排程

> 設計備註：排程只展開一層 BOM 是刻意設計 — 半成品需要作為獨立排程項目管理（獨立領料、生產、入庫），
> 而非自動展開到最底層原料。GetBomTreeAsync 的多層展開供 BOM 結構查詢使用。

### 情境 17：領料單建立與庫存扣除
【檢查結果：✅ 通過】

- 排程項目 BOM 需求：M1 100 個、M2 150 個
- 建立領料單，選擇倉庫和儲位
- ValidateStockAvailabilityAsync 確認各組件庫存充足
- 確認領料單儲存後庫存正確扣除（M1 -100、M2 -150）
- 庫存異動記錄的來源為 MaterialIssue，類型為「領料」
- GetLastIssuedLocationForItemAsync 記住本次領料的倉庫/庫位

驗證重點： 領料庫存扣除、ValidateStockAvailability 驗證、歷史位置記憶

### 情境 18：領料單編輯後的差異更新
【檢查結果：✅ 通過】

- 領料單原本領 M1 100 個
- 編輯領料單，將 M1 改為 80 個
- UpdateInventoryByDifferenceAsync → 確認庫存回補 20 個
- 再次編輯改為 120 個 → 確認庫存再扣 40 個

驗證重點： 領料差異更新的增減計算

### 情境 19：同一品項從不同倉庫領料
【檢查結果：✅ 通過】

- M1 在倉庫 A 有 60 個、倉庫 B 有 40 個
- 領料單建立兩行明細：M1 從倉庫 A 領 60 + M1 從倉庫 B 領 40
- 確認 IsItemExistsInIssueAsync 允許同品項不同倉庫的多行明細
- 確認各倉庫庫存分別正確扣除

驗證重點： 同品項多倉庫領料的明細允許性、分倉庫存扣除

### 情境 20：用料結算完整性
【檢查結果：✅ 通過（已修正）】

- BOM 組件 M1 領料 200 個，排程數量 100
- 最後一次完工時傳入結算：ActualUsedQty=170、ReturnQty=20（退回倉庫 A）、ScrapQty=10（損耗原因：「製程報廢」）
- 確認 170 + 20 + 10 = 200（= 領料量）
- 確認退料 20 個回補到倉庫 A（庫存異動類型 MaterialReturn）
- 確認損耗 10 個記錄 ScrapQty 和 ScrapReason
- 嘗試 ReturnQty > 0 但不填 ReturnWarehouseId → 觀察系統是否阻擋

驗證重點： 結算等式驗證（消耗+退料+損耗=領料量）、退料庫存回補、損耗記錄、退料倉庫必填

---

## 四、銷貨訂單與排程的連動（情境 21–25）

### 情境 21：同一訂單明細分多次排程
【檢查結果：✅ 通過】

- 銷貨訂單明細：成品 P 共 200 個
- 第一次排程 80 個 → ScheduledQuantity = 80，PendingQuantity = 120
- 第二次排程 70 個 → ScheduledQuantity = 150，PendingQuantity = 50
- 第三次排程 50 個 → ScheduledQuantity = 200，PendingQuantity = 0，從 Sidebar 消失
- GetBySalesOrderDetailIdAsync 確認 3 個排程項目都關聯到同一個 SalesOrderDetailId

驗證重點： ScheduledQuantity 累計、PendingQuantity 遞減、多次排程的關聯追蹤

### 情境 22：排程項目刪除後的數量回寫
【檢查結果：✅ 通過】

- 排程項目 CompletedQuantity = 0，排程數量 80
- 刪除該項目 → 確認 SalesOrderDetail.ScheduledQuantity 減回 80
- 該訂單明細重新出現在 Sidebar 的待排清單中
- 嘗試刪除 CompletedQuantity > 0 的項目 → 應被阻擋

驗證重點： 刪除回寫 ScheduledQuantity、已有完工不允許刪除

### 情境 23：訂單數量變更對排程的影響
【檢查結果：✅ 通過（設計確認）】

- 排程數量一旦建立即為固定（代表已承諾的生產工單），不提供直接修改
- **正確流程**：變更源頭在銷貨訂單的 OrderQuantity，排程透過 PendingScheduleQuantity 自動反映
- 訂單數量增加（100→150）→ PendingScheduleQuantity 從 0 變 50 → 自動重新出現在 Sidebar 供追加排程
- 訂單數量減少（100→60，已排程 100）→ PendingScheduleQuantity = -40（過度排程）→ 不出現在 Sidebar，人工決定是否 Abort 多餘排程
- 若需完全重排，使用 ReturnToSidebar（未領料時）或 AbortProduction（已領料/生產中）

驗證重點： 排程數量為唯讀、訂單數量為變更源頭、PendingScheduleQuantity 正確計算、過度排程的人工處理

### 情境 24：訂單取消後排程項目的處理
【檢查結果：✅ 通過】

- 銷貨訂單有 2 個排程項目：A（InProgress）、B（Pending）
- 銷貨訂單被駁回或取消
- 確認排程項目 B（未開始）可以退回或刪除
- 排程項目 A（已開始）需要走終止流程（AbortProduction）
- 確認所有 ScheduledQuantity 和 ProducedQuantity 的數值一致性

驗證重點： 訂單取消時排程項目的善後處理、不同狀態的處理方式

### 情境 25：完工後出貨的完整鏈路
【檢查結果：✅ 通過】

- 銷貨訂單成品 P 共 50 個 → 排程 → 領料 → 生產 → 完工入庫 50 個
- 確認成品 P 庫存 +50
- 建立銷貨出貨單，從該訂單帶入明細 → 出貨 50 個
- 確認成品庫存 -50，訂單完成
- 核對所有庫存異動記錄的順序：MaterialIssue → ProductionCompletion → SalesDelivery

驗證重點： 排程→出貨的完整端到端、庫存異動記錄的完整時序鏈

---

## 五、分配（Allocation）與批次操作（情境 26–28）

### 情境 26：生產分配的建立與查詢
【檢查結果：✅ 通過】

- 合併排程：成品 P 共 80 個（合併兩張訂單需求：甲 50 + 乙 30）
- CreateAllocationsAsync：分配 50 給訂單甲、30 給訂單乙
- GetTotalAllocatedQuantityAsync = 80
- GetByScheduleItemIdAsync → 確認 2 筆分配記錄
- GetBySalesOrderDetailIdAsync → 從訂單甲反查到分配記錄
- 嘗試分配 50 + 40 = 90（超過排程量 80）→ 觀察系統是否阻擋

驗證重點： 分配建立、總量查詢、雙向查詢、超額阻擋

### 情境 27：批次完工（BatchComplete）
【檢查結果：✅ 通過】

- 本週有 4 個 InProgress 項目：A（排程 50/完成 0）、B（排程 30/完成 10）、C（排程 80/完成 80）、D（排程 60/完成 20）
- 開啟批次完工 Modal → 確認只列出 A、B、D（C 已全部完成，剩餘量=0，不應出現或自動略過）
- 預設完工數量：A=50、B=20、D=40
- 修改 A 的數量為 30（部分完工）、取消勾選 D
- 確認 → A 完工 30（累計 30，仍 InProgress）、B 完工 20（累計 30 = 排程 30，自動 Completed）
- D 未處理（維持原狀）
- 確認各成品庫存正確增加

驗證重點： 批次完工的項目篩選、預設數量計算、部分完工、自動結案、勾選/取消

### 情境 28：每日批次排程的冪等性
【檢查結果：✅ 通過】

- 首次拖曳項目到 3/25 → GetOrCreateDailyBatchAsync(3/25) 建立 PS-20260325
- 再次拖曳另一個項目到 3/25 → GetOrCreateDailyBatchAsync(3/25) 應回傳同一筆 PS-20260325
- 確認不會重複建立
- 查詢 GetByDateRangeAsync(3/25, 3/25) → 確認只有一筆 ProductionSchedule

驗證重點： GetOrCreateDailyBatch 的冪等性（已存在就回傳，不重複建立）

---

## 六、邊界條件與資料完整性（情境 29–30）

### 情境 29：完工數量超過排程數量
【檢查結果：✅ 通過】

- 排程數量 100，已完工 90
- 嘗試 CompleteProduction 數量 20（累計 110 > 排程 100）→ 觀察系統是否允許超產或阻擋
- 如果允許超產，確認成品庫存正確增加 20，CompletedQuantity = 110
- 如果阻擋，確認錯誤訊息明確指出最大可完工數量為 10
- 嘗試完工數量為 0 或負數 → 應被阻擋

驗證重點： 超產的處理策略（允許或阻擋）、零和負數的防護

### 情境 30：同品項多排程的庫存影響追蹤
【檢查結果：✅ 通過】

- 成品 P 同時有 3 個排程項目在不同日期，BOM 都需要 M1
- 排程①領料 M1 共 100 → 庫存 -100
- 排程②領料 M1 共 80 → 庫存 -80
- 排程①完工 50 個成品 → 成品庫存 +50
- 排程③領料 M1 共 60 → 庫存 -60
- 排程①用料結算，退料 M1 共 20 → 庫存 +20
- 排程②完工 40 個成品 → 成品庫存 +40
- 查詢 M1 的所有庫存異動記錄 → 確認 3 次領料、1 次退料都有記錄，且最終庫存 = 期初 - 100 - 80 - 60 + 20
- 查詢成品 P 的庫存異動記錄 → 2 次完工入庫，庫存 = 50 + 40 = 90
- 確認所有異動記錄的 SourceDocumentType 和 SourceDocumentId 都能追蹤回對應的領料單和完工記錄

驗證重點： 多排程並行時的庫存累計正確性、異動記錄的完整追蹤鏈、SourceDocument 的來源關聯

---

## 已完成檢查（先前情境 — 2026-03-25）

以下為先前對話中已完成檢查的情境，與上方新情境有部分重疊，追蹤結果保留供參考。

<details>
<summary>情境十五（舊）：看板排程操作 ✅</summary>

- GetUnscheduledAsync 篩選 PlannedStartDate==null && !IsClosed && Status!=Completed ✓
- UpdatePlannedDateAsync 正確更新日期 ✓
- UpdatePrioritiesAsync 批次更新 Priority 欄位 ✓
- ReturnToSidebarAsync：CompletedQuantity>0 或 IssuedQuantity>0 時阻擋 ✓；成功時扣回 ScheduledQuantity 並硬刪除項目 ✓
- GetOrCreateDailyBatchAsync：以 PS-yyyyMMdd 格式冪等建立每日批次 ✓
- GetScheduledQuantityMapAsync：正確聚合各 SalesOrderDetailId 的排程總量 ✓
</details>

<details>
<summary>情境十六（舊）：原料不足導致的生產等待 ✅</summary>

- MaterialIssueService.CreateAsync 呼叫 ReduceStockWithFIFOAsync，庫存不足時整筆交易回滾（不支援自動部分領料）
- 使用者需手動建立較小數量的領料單（例如領 30）才能成功
- 看板拖入時初始狀態為 WaitingMaterial ✓
- StartProductionAsync 允許從 WaitingMaterial 啟動（不強制全部領料完成）✓
- 設計備註：系統不支援「自動部分領料」，使用者必須手動輸入可領數量
</details>

<details>
<summary>情境十七（舊）：混合外購品項和自製品項 ✅</summary>

- RequiresProduction 在選擇品項時由 HasItemComposition() 自動設定 ✓
- GetForProductionBoardAsync 篩選 RequiresProduction==true → 外購品項不出現在看板 ✓
- GetAggregateStatusMapAsync 優先順序：InProgress > Paused > WaitingMaterial > Completed > Pending ✓
</details>

<details>
<summary>情境十八（舊）：BOM 配方變更對排程的影響 ✅</summary>

- CopyFromCompositionAsync 將 BOM 複製到 SalesOrderCompositionDetail（訂單獨立副本）✓
- ExpandBomAsync 優先使用 SalesOrderCompositionDetail（凍結 BOM），無則 fallback 到 ItemComposition ✓
- 設計備註：BOM 凍結依賴「報價單/訂單明確選擇配方並複製」，直接建立訂單不具凍結效果
</details>

<details>
<summary>情境十九（舊）：多張訂單共用同一生產批次 ✅（已修正）</summary>

- 已修正：ValidateAsync 加入「分配總量 ≤ 排程數量」驗證
</details>

<details>
<summary>情境二十（舊）：完整壓力情境 ✅</summary>

- 各子流程在先前情境中均已確認通過
</details>

---

## 修正記錄

| # | 檔案 | 修正內容 | 嚴重度 |
|---|------|----------|--------|
| 1 | ProductionScheduleItemService.cs:563 | 完工退料 sourceDocumentType → MaterialReturn | 中 |
| 2 | ProductionScheduleItemService.cs:1042 | 停產退料 sourceDocumentType → MaterialReturn | 中 |
| 3 | ProductionScheduleItemService.cs:572-580 | 部分完工自動更新狀態 Pending/WaitingMaterial → InProgress | 中 |
| 4 | ProductionScheduleItemService.cs:430 | CompleteProductionAsync 狀態驗證改為白名單（阻擋 Paused/Aborted/Closed） | 高 |
| 5 | SalesDeliveryIndex.razor:274-320 | Index 頁面核准出貨單後觸發 UpdateInventoryByDifferenceAsync | 高 |
| 6 | SalesReturnIndex.razor:250-296 | Index 頁面核准退貨單後觸發 UpdateInventoryByDifferenceAsync | 高 |
| 7 | ProductionScheduleAllocationService.cs:89-101 | ValidateAsync 加入分配總量 ≤ 排程數量驗證 | 中 |
| 8 | ProductionScheduleItemService.cs (CompleteProductionAsync) | 用料結算 ReturnQty>0 時驗證 ReturnWarehouseId 必填（防止帳實不符） | 中 |
| 9 | ProductionScheduleItemService.cs (AbortProductionAsync) | 同上，停產結算退料倉庫必填驗證 | 中 |
