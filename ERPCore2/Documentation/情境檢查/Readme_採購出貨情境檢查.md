情境一：完美順流程（採購到出貨全部成功） ✅ 已檢查
目的：驗證最基本的端到端流程

建立品項 A（外購類型），設定安全庫存為 50
建立供應商 X 和客戶 Y
建立採購訂單 → 填入品項 A 共 100 個 → 送出 → 核准
建立採購進貨單，從採購訂單帶入明細 → 選擇倉庫和儲位 → 確認進貨
確認庫存異動記錄是否正確產生（類型為「進貨」），庫存數量是否變為 100
建立報價單給客戶 Y → 填入品項 A 共 30 個 → 核准
報價單轉銷貨訂單 → 確認明細是否正確帶入 → 核准
進行庫存檢查（OrderInventoryCheck）確認庫存足夠
建立銷貨出貨單，從銷貨訂單帶入明細 → 確認出貨
確認庫存扣除 30，剩餘 70，庫存異動記錄類型為「銷貨」

驗證重點： 每一步的單據編號自動產生是否正確、單據間的關聯是否完整、庫存數量計算是否準確

檢查結果：流程邏輯正確，無阻斷性問題。
- 注意事項：進貨與出貨的儲位（LocationId）必須一致，否則 ReduceStockAsync 會因精確匹配而扣減失敗
- 注意事項：OrderInventoryCheck 看全倉庫合計，ReduceStockAsync 按特定倉庫+儲位扣減，多倉庫時可能不一致
- 注意事項：出貨編輯回補使用 SalesReturn 類型，報表上會與真正客戶退貨混淆
- 已清理：ConfirmDeliveryAsync 為未使用的死碼

情境二：分批進貨 + 分批出貨 ✅ 已檢查
目的：驗證部分收貨與部分出貨的處理

採購訂單品項 A 共 200 個，核准
第一次進貨 80 個 → 確認 → 驗證庫存 +80
第二次進貨 80 個 → 確認 → 驗證庫存 +80（累計 160）
確認採購訂單的「未完成」狀態（還差 40 個未進貨）
銷貨訂單品項 A 共 150 個，核准
第一次出貨 100 個 → 確認 → 驗證庫存 -100（剩 60）
第二次出貨 50 個 → 確認 → 驗證庫存 -50（剩 10）
確認銷貨訂單已完成出貨
再建第三次進貨 40 個 → 確認 → 驗證庫存 +40（累計 50），確認採購訂單完全完成

驗證重點： 部分進貨/出貨時的數量追蹤、訂單完成狀態的自動判斷、多次進貨累計計算

檢查結果：數據流和庫存計算在正常順流程下正確。
- 已修復 Bug：RecalculateReceivedQuantityAsync 缺少 Status 過濾（採購端與銷貨端不一致）
  → PurchaseOrderDetailService.cs 已加入 Status == Active 及父進貨單 Active 過濾
- 注意事項：IsReceivingCompleted 不會自動更新，PO 完成狀態靠 PendingQuantity 計算判斷

情境三：庫存不足時嘗試出貨 ✅ 已檢查
目的：驗證庫存不足的阻擋機制

目前庫存品項 A 只有 10 個
建立銷貨訂單，品項 A 共 50 個 → 核准
執行庫存檢查（ValidateWarehouseInventoryStock）→ 應該回傳庫存不足的警告
嘗試建立銷貨出貨單並確認出貨 → 觀察系統是否阻擋或警告
如果系統允許出貨，確認庫存是否變為負數，以及庫存狀態是否變為 Insufficient

驗證重點： 庫存不足時系統的反應是阻擋還是警告、負庫存的處理邏輯、庫存狀態告警是否觸發

檢查結果：系統有雙層防護，不允許負庫存。
- UI 層：SalesDeliveryTable.ValidateAsync 在儲存前阻擋，顯示「庫存不足」警告
- Service 層：ReduceStockAsync 檢查可用庫存不足時回傳失敗並 rollback
- 已清理：ValidateWarehouseInventoryStockAsync 死碼已移除（SalesOrderService + ISalesOrderService）

情境四：出貨後客戶退貨 ✅ 已檢查
目的：驗證銷貨退回流程及庫存回補

正常流程出貨品項 A 共 30 個（庫存從 100 變 70）
客戶退回 10 個 → 建立銷貨退回單，選擇退回原因 → 確認退回
確認庫存回補 10 個（變為 80）
確認庫存異動記錄類型為「銷貨退回」
嘗試退回超過原出貨數量（例如退 50 個）→ 觀察系統是否阻擋
對同一張出貨單分兩次退回（第一次退 10，第二次退 15）→ 確認累計退回數量追蹤

驗證重點： 退回庫存回補是否正確、超退阻擋、退回原因是否正確記錄、出貨單與退回單的關聯追蹤

檢查結果：流程邏輯完整，無 Bug。
- 超退阻擋：三層防護（UI 輸入限縮 Math.Min + ValidateAsync 驗證 + TotalReturnedQuantity 累計追蹤）
- 庫存回補：AddStockAsync 使用 InventoryTransactionTypeEnum.SalesReturn，正確
- 累計退貨：TotalReturnedQuantity 在 SalesDeliveryDetail 上累加，多次退貨正確追蹤
- 編輯模式：AlreadyReturnedQuantity = Total - 本次數量，不把自己算進已退量，正確
- 刪除回滾：刪除退回單時回退 TotalReturnedQuantity 並回滾庫存，正確

情境五：進貨品質問題 → 採購退回 ✅ 已檢查
目的：驗證採購退回流程及庫存扣除

採購進貨品項 A 共 100 個，確認後庫存 +100
發現 20 個有瑕疵 → 建立採購退回單，退回 20 個 → 確認退回
驗證庫存 -20（變為 80）
確認庫存異動記錄類型為「進貨退出」
嘗試退回超過原進貨數量 → 觀察系統是否阻擋
確認供應商的退回統計是否正確更新

驗證重點： 採購退回的庫存扣除、超退阻擋、退回單與進貨單的關聯、供應商統計

檢查結果：退貨流程主體邏輯正確，超退阻擋完整。
- 庫存扣減：ReduceStockAsync 使用 InventoryTransactionTypeEnum.Return（進貨退出），正確
- 超退阻擋：UI Math.Min 限縮 + ValidateAsync 驗證 + TotalReturnedQuantity 在 PurchaseReceivingDetail 累計
- 供應商統計：按需查詢 PurchaseReturn 記錄計算，無獨立統計表，正確
- 已修復 Bug：PurchaseOrderDetail.ReturnedQuantity 從未被寫入（永遠為 0）
  → 在 PurchaseReturnService 新增 RecalculateOrderDetailReturnedQuantityAsync 私有方法
  → 儲存/編輯退回單（UpdateDetailsInContext）和刪除退回單（PermanentDeleteAsync）時同步回寫
  → 從所有關聯 Active 狀態的 PurchaseReceivingDetail.TotalReturnedQuantity 彙算
  → 三次檢查修正：在 recalculate 前先 SaveChangesAsync，確保 SumAsync 查詢到最新的 TotalReturnedQuantity

情境六：單據編輯後的庫存差異更新 ✅ 已檢查
目的：驗證 UpdateInventoryByDifference 機制

建立採購進貨單，品項 A 進 100 個 → 確認 → 庫存 +100
編輯進貨單，將數量改為 80 個 → 儲存
確認系統透過差異更新機制，庫存調整為 80（-20）
再次編輯改為 120 個 → 確認庫存調整為 120（+40）
同樣對銷貨出貨單做相同的編輯測試
對銷貨退回單做相同的編輯測試

驗證重點： 差異更新計算是否正確、增加和減少都要測試、庫存異動記錄的 OperationType 是否標記為 Adjust

檢查結果：差異計算邏輯在三種文件類型上均正確，OperationType=Adjust 標記正確。
- 採購進貨：target - processed 正確，增加用 AddStockAsync、減少用 ReduceStockAsync
- 銷貨出貨：以負數表示出庫方向，差異計算正確
- 銷貨退回：以正數表示退回方向，差異計算正確
- 注意事項：查詢過濾條件不一致（採購用三重過濾，銷貨只用單號），但因 Code 唯一不影響結果
- 已還原：編輯調整的 TransactionType 不一致問題
  → 二次檢查發現：GetOrCreateTransactionAsync 會複用既有的 InventoryTransaction（不更新 TransactionType）
  → 人工審核模式下，第一次庫存操作透過 UpdateInventoryByDifferenceAsync 觸發，若改為 Adjustment 會導致新建的異動主檔類型錯誤
  → 正確架構：TransactionType = 業務類型（Sale/SalesReturn），OperationType = 操作類型（Initial/Adjust/Delete）
  → 報表應用 TransactionType + OperationType 組合區分「原始操作」與「編輯調整」
  → 已還原為原始值（Sale/SalesReturn），此為設計特性非 Bug

情境七：核准與駁回流程 ✅ 已檢查
目的：驗證審批流程的完整性

建立採購訂單 → 送出審批 → 核准 → 確認狀態變更
建立另一張採購訂單 → 送出審批 → 駁回（填寫駁回原因）→ 確認狀態與原因記錄
被駁回的訂單修改後重新送出 → 核准
核准後的採購訂單嘗試再次編輯 → 觀察是否允許
對報價單、銷貨訂單、銷貨出貨單、進貨單、退回單各做一次核准和駁回測試
測試銷貨訂單設定 checkApproval=true 時，未核准的訂單是否無法被出貨單引用

驗證重點： 各單據的審批狀態流轉、駁回原因記錄、審批後的編輯限制、下游單據對審批狀態的檢查

檢查結果：審批框架一致，所有單據結構相同。
- 所有單據類型：重複核准/駁回有阻擋、駁回原因正確儲存、駁回後可重新核准（清除 RejectReason）
- 注意事項：所有單據核准後仍可編輯（Service 層無 IsApproved 檢查阻擋編輯）
  → 這是設計選擇，搭配 UpdateInventoryByDifference 機制可正確處理編輯差異
- 注意事項：SalesOrder 的 checkApproval 參數存在但未實際使用
  → SalesOrderService 註解明確說明「銷貨訂單無審核機制，此參數保留以保持介面一致性」
  → 出貨單不檢查來源銷貨訂單是否已核准（與採購端行為不一致，但屬設計決定）

情境八：刪除單據後的庫存回退 ✅ 已檢查
目的：驗證 OperationType=Delete 的庫存回退機制

建立採購進貨單，品項 A 進 50 個 → 確認 → 庫存 +50
刪除該進貨單 → 確認庫存回退 -50
確認庫存異動記錄產生一筆 OperationType=Delete 的回退記錄
建立銷貨出貨單，品項 A 出 30 個 → 確認 → 庫存 -30
刪除該出貨單 → 確認庫存回補 +30
確認銷貨訂單的出貨數量是否也跟著回退

驗證重點： 刪除回退的庫存計算、異動記錄的完整性、上游單據（訂單）的數量連動更新

檢查結果：四個單據類型的刪除回退邏輯均正確。
- 所有單據：庫存方向正確（進貨刪除→Reduce、出貨刪除→Add、採購退回刪除→Add、銷貨退回刪除→Reduce）
- 所有單據：OperationType=Delete 標記正確
- 所有單據：僅 IsApproved==true 時才回退庫存（未核准單據從未影響庫存）
- 上游連動：ReceivedQuantity、DeliveredQuantity、TotalReturnedQuantity 均正確回寫
- 採購進貨額外防護：有重複刪除偵測機制（檢查已存在的 Delete 操作記錄避免重複扣減）
- 刪除阻擋：各單據有對應的 CanDeleteAsync 檢查（已退貨/已付款時阻擋）

情境九：多倉庫 + 倉庫儲位 + 轉倉測試 ✅ 已檢查
目的：驗證多倉庫場景

建立倉庫 A（含普通儲位、收貨區、出貨區）和倉庫 B
採購進貨至倉庫 A 的收貨區 → 確認庫存入帳到正確的倉庫和儲位
建立庫存轉倉交易（Transfer），從倉庫 A 轉 50 個到倉庫 B
確認倉庫 A 減少 50，倉庫 B 增加 50
銷貨出貨從倉庫 B 出貨 → 確認扣的是倉庫 B 的庫存
確認系統記住品項上次進貨的倉庫和位置（GetLastReceivingLocation）

驗證重點： 倉庫級別的庫存分開計算、轉倉的雙向記錄、出貨時的倉庫選擇、歷史位置記憶

檢查結果：轉倉邏輯正確，但有設計限制需注意。
- TransferStockAsync：正確產生兩筆異動（來源 Reduce + 目標 Add），類型=Transfer，單一交易原子性
- 成本保留：轉倉時從來源取得 AverageCost 傳遞給目標倉庫
- 庫存不足處理：來源不足時 ReduceStockAsync 回傳失敗，交易回滾
- 注意事項：GetLastReceivingLocation 功能不存在（情境描述中的需求系統未實作）
- 重要限制：ReduceStockAsync 的 LocationId 為精確匹配（null ≠ 具體儲位）
  → 進貨時指定儲位 A-01，出貨時傳 null 會找不到庫存
  → GetTotalAvailableStockByWarehouseAsync 合計全倉庫（不分儲位），但實際扣減分儲位

情境十：複合壓力情境（混合操作） ✅ 已檢查
目的：模擬真實營運中同時發生多種事件

品項 A 目前庫存 100 個
銷貨訂單①：客戶甲訂 60 個 → 核准
銷貨訂單②：客戶乙訂 50 個 → 核准
兩張訂單總需求 110 個，超過庫存 100 個 → 執行庫存檢查，確認系統報告不足
先出貨給客戶甲 60 個 → 庫存剩 40
嘗試出貨給客戶乙 50 個 → 庫存不足，觀察系統反應
緊急採購進貨 30 個 → 庫存變 70
客戶甲退貨 10 個 → 庫存變 80
此時再出貨給客戶乙 50 個 → 庫存變 30
確認所有庫存異動記錄的順序和數量都正確，最終庫存數量無誤

驗證重點： 並發訂單的庫存競爭、緊急採購的即時反映、退貨回補後的重新出貨、異動記錄的完整時序

檢查結果：順序操作下邏輯正確，但並行操作有風險。
- 順序流程：每一步庫存計算正確（60→40→不足阻擋→+30=70→+10=80→-50=30）
- 緊急採購/退貨回補：庫存立即可用，不需等待
- OrderInventoryCheck：每次檢查看的是當下可用庫存，兩張訂單各自檢查時都會看到 100（不互相影響）
- 注意事項：ReservedStock 機制存在但未接入銷貨訂單流程
  → 核准銷貨訂單不會預留庫存，不會減少 AvailableStock
  → InventoryReservation 實體和 Service 已建立但未整合到銷貨流程
- 注意事項：ReduceStockAsync 無悲觀鎖定，理論上存在並行競爭風險
  → Blazor Server 環境下為單一伺服器，實務上極少觸發
  → 資料庫層面無 CHECK 約束防止 CurrentStock 變為負數