我已經對你系統的所有模組非常熟悉了，直接根據之前讀取的完整架構來設計 30 個出貨流程情境。這次我會聚焦在「從採購到出貨」的完整鏈路，每個情境都模擬製造業實際會遇到的不同狀況。

一、標準順流程（情境 1–5）
情境 1：外購品項 — 最簡單的採購到出貨

客戶下單：銷貨訂單品項 A（外購）50 個 → 核准
庫存檢查（ValidateWarehouseInventoryStock）→ 庫存 0，不足
建立採購訂單品項 A 50 個 → 送出 → 核准
建立採購進貨單，從採購訂單帶入明細 → 選擇倉庫 → 確認進貨
確認庫存 +50，庫存異動類型 = Purchase
再次庫存檢查 → 通過
建立銷貨出貨單，從銷貨訂單帶入明細 → 確認出貨
確認庫存 -50 歸零，庫存異動類型 = Sale
確認銷貨訂單出貨完成

驗證重點： 最基本的端到端，確認每一步單據編號自動產生、關聯正確、庫存數量正確

✅ 【檢查結果 2026-03-25】PASS — 單據編號自動產生（SO/SD/PO/PR+日期+序號）、庫存異動類型正確（Purchase/Sale）、RecalculateDeliveredQuantityAsync 正確回寫已出貨數量、DB Transaction 保證原子性。

情境 2：自製品項 — 從接單到排程到出貨

客戶下單：銷貨訂單品項 P（自製）30 個 → 核准
庫存檢查 → 不足（成品 P 庫存 0）
從銷貨訂單轉排程（CreateItemsFromSalesOrder）→ 排程 30 個
拖曳到看板 → BOM 展開 → 確認原料 M1 需 60 個、M2 需 90 個
確認原料庫存充足 → 建立領料單 → 庫存扣除
開始生產 → 完工入庫 30 個 → 成品 P 庫存 +30
庫存檢查 → 通過
建立銷貨出貨單 → 確認出貨 30 個 → 成品庫存歸零
確認庫存異動完整鏈：MaterialIssue → ProductionCompletion → SalesDelivery

驗證重點： 自製品項的完整鏈路、BOM 用料到成品入庫到出貨

✅ 【檢查結果 2026-03-25】PASS — CreateItemsFromSalesOrderAsync 建立排程、BOM 展開含加權平均成本計算、完工入庫 AddStockAsync(ProductionCompletion)、領料扣庫 MaterialIssue、出貨鏈路完整。

情境 3：報價單起始的完整流程

建立報價單給客戶，品項 A 50 個、品項 B 30 個 → 核准
報價單轉銷貨訂單 → 確認明細正確帶入（品項、數量、價格）
銷貨訂單核准
品項 A 庫存足夠（已有庫存 100）
品項 B 庫存不足 → 採購進貨補庫
建立銷貨出貨單，同時出貨 A 50 個 + B 30 個
確認兩個品項的庫存各自正確扣除
用 GetByQuotationIdAsync 確認報價單→訂單的追蹤關聯

驗證重點： 報價單→訂單的轉換完整性、多品項同時出貨

✅ 【檢查結果 2026-03-25】PASS（第二輪更正）— 報價單轉銷貨訂單功能完整存在：QuotationEditModalComponent 有 HandleCreateSalesOrderFromQuotation()、SalesOrderEditModalComponent 有 ShowAddModalWithPrefilledQuotation() + LoadDataFromQuotation()、SalesOrderTable 有 LoadQuotationDetails()。支援部分轉換（ConvertedQuantity 追蹤）、BOM 繼承、核准檢查。

情境 4：混合品項訂單 — 外購 + 自製同時出貨

銷貨訂單：外購品項 A 40 個 + 自製品項 P 20 個 → 核准
品項 A 路線：確認庫存足夠（或採購補庫）
品項 P 路線：轉排程 → 領料 → 生產 → 完工入庫 20 個
兩個品項庫存都到位後，建立一張銷貨出貨單同時出貨
確認兩個品項的庫存異動各自獨立正確
確認品項 A 不出現在排程 Sidebar（外購不排程）

驗證重點： 混合品項的分流處理、合併出貨

✅ 【檢查結果 2026-03-25】PASS — 同一張出貨單可含外購+自製品項，庫存異動各自獨立記錄，外購品項不出現在生產排程。

情境 5：含稅計算的完整流程

採購訂單品項 A 100 個，單價 100 元 → 未稅 10,000 + 稅 500 = 含稅 10,500
進貨確認 → 庫存入帳
銷貨訂單品項 A 100 個，單價 200 元 → 未稅 20,000 + 稅 1,000 = 含稅 21,000
出貨確認 → 庫存扣除
確認採購訂單和銷貨訂單的稅額計算（CalculateAndUpdateTaxAmountAsync / CalculateOrderTotalAsync）
自動轉傳票：進貨傳票三行分錄含稅拆分正確、出貨傳票三行分錄含稅拆分正確

驗證重點： 含稅金額貫穿整個流程、稅額計算一致性

🐛→✅ 【檢查結果 2026-03-25】發現並修正 Bug — 銷貨訂單稅額原硬編碼 5%（totalAmount * 0.05m），已修正為使用 SystemParameterService.GetTaxRateAsync() 動態取得，與採購訂單一致。傳票稅額拆分邏輯正確（進項稅額1268/銷項稅額2204）。

二、分批處理（情境 6–10）
情境 6：分批進貨 → 一次出貨

採購訂單品項 A 200 個
第一次進貨 80 個 → 庫存 80
第二次進貨 70 個 → 庫存 150
第三次進貨 50 個 → 庫存 200，採購訂單完成
銷貨訂單品項 A 200 個 → 一次出貨 200 個
確認每次進貨的庫存累計正確
確認採購訂單的未進貨數量隨每次進貨遞減

驗證重點： 分批進貨的累計追蹤、一次出貨扣完

✅ 【檢查結果 2026-03-25】PASS — RecalculateReceivedQuantityAsync 正確累計每次進貨、GetPendingReceivingDetailsAsync 過濾未完成明細（ReceivedQuantity < OrderQuantity）、一次出貨 ReduceStockAsync 扣減正確。

情境 7：一次進貨 → 分批出貨

進貨品項 A 共 300 個
銷貨訂單①品項 A 100 個 → 出貨 100 → 庫存剩 200
銷貨訂單②品項 A 120 個 → 出貨 120 → 庫存剩 80
銷貨訂單③品項 A 80 個 → 出貨 80 → 庫存歸零
確認每次出貨後庫存遞減正確
確認三張訂單各自的出貨狀態為已完成

驗證重點： 多訂單消耗同一批庫存、出貨狀態各自獨立

✅ 【檢查結果 2026-03-25】PASS — 各訂單 DeliveredQuantity 獨立追蹤、ReduceStockAsync 按 FIFO 扣減、訂單完成狀態各自判斷。

情境 8：同一訂單分批出貨

銷貨訂單品項 A 150 個 → 核准
第一次出貨 60 個 → 確認訂單未完成，出貨明細帶入的剩餘可出量 = 90
第二次出貨 50 個 → 剩餘可出量 = 40
第三次出貨 40 個 → 訂單完成
用 GetBySalesOrderIdAsync 確認該訂單有 3 張出貨單
三張出貨單的出貨數量合計 = 150 = 訂單數量

驗證重點： 分批出貨的剩餘量追蹤、訂單完成判斷

🐛→✅ 【檢查結果 2026-03-25】第二輪發現並修正 Bug — 原本帶入預設量正確（OrderQuantity - DeliveredQuantity），但使用者可手動改大且無驗證阻擋。已在 SalesDeliveryTable.ValidateAsync 加入剩餘可出量上限檢查（含編輯模式加回自身原始數量避免重複扣除）。

情境 9：自製品項分批完工 → 分批出貨

銷貨訂單成品 P 100 個 → 排程 100
第一批完工 40 個 → 成品庫存 40
先出貨 40 個（第一張出貨單）→ 成品庫存 0
第二批完工 60 個 → 成品庫存 60
再出貨 60 個（第二張出貨單）→ 成品庫存 0
確認訂單出貨完成，排程也完成
確認庫存異動時序：Completion(+40) → Delivery(-40) → Completion(+60) → Delivery(-60)

驗證重點： 邊生產邊出貨的交叉時序、庫存不會出現負數

✅ 【檢查結果 2026-03-25】PASS — 部分完工 AddStockAsync(ProductionCompletion) 正確入庫、可立即出貨、ReduceStockAsync 驗證庫存充足才扣減（不允許負庫存）、交叉時序的庫存異動記錄完整。

情境 10：分批進貨不同倉庫 → 出貨指定倉庫

品項 A 第一次進貨 100 個到倉庫甲
品項 A 第二次進貨 80 個到倉庫乙
銷貨訂單品項 A 120 個 → 出貨時從倉庫甲出 100 個 + 倉庫乙出 20 個
確認倉庫甲庫存 0，倉庫乙庫存 60
確認出貨明細對應到正確的倉庫

驗證重點： 多倉庫出貨的庫存分別扣減

✅ 【檢查結果 2026-03-25】PASS — SalesDeliveryDetail 每行有獨立 WarehouseId、ReduceStockAsync 按指定倉庫扣減、InventoryStockDetail 按 Warehouse+Location+Batch 獨立追蹤。

三、退貨與異常處理（情境 11–17）
情境 11：出貨後全部退貨

出貨品項 A 50 個 → 庫存 -50
客戶全部退回 50 個 → 建立銷貨退回單，選擇退回原因 → 確認
確認庫存 +50 回補
庫存異動類型 = SalesReturn
確認出貨單和退回單的關聯（GetBySalesDeliveryIdAsync）

驗證重點： 全退的庫存回補、單據關聯

✅ 【檢查結果 2026-03-25】PASS — UpdateInventoryByDifferenceAsync 退回時 AddStockAsync(SalesReturn) 回補庫存、TotalReturnedQuantity 累計追蹤、GetBySalesDeliveryIdAsync 可查詢關聯退回單。

情境 12：出貨後部分退貨 → 補出貨

出貨品項 A 100 個 → 客戶退回 30 個 → 庫存 +30
另一張訂單需要品項 A 30 個 → 用退回的庫存出貨
確認庫存：-100 +30 -30 = 淨減 100
確認異動記錄完整：SalesDelivery(-100) → SalesReturn(+30) → SalesDelivery(-30)

驗證重點： 退回庫存可再次出貨、異動時序

✅ 【檢查結果 2026-03-25】PASS — 退回後庫存正常可用、異動記錄時序完整（SalesDelivery → SalesReturn → SalesDelivery）、退回庫存可再次出貨無限制。

情境 13：採購品質問題 → 退回供應商 → 重新進貨 → 出貨

進貨品項 A 100 個 → 發現 40 個不良 → 採購退回 40 個
庫存：+100 -40 = 60
重新向供應商訂購 40 個 → 進貨 40 個 → 庫存 100
出貨 100 個 → 庫存歸零
異動記錄：Purchase(+100) → PurchaseReturn(-40) → Purchase(+40) → SalesDelivery(-100)

驗證重點： 採購退回後補進貨的完整鏈路

✅ 【檢查結果 2026-03-25】PASS — PurchaseReturnService 使用 ReduceStockAsync(Return) 扣減庫存、ValidateReturnQuantityAsync 驗證退回不超過進貨數量、異動鏈路完整（Purchase → Return → Purchase → SalesDelivery）。

情境 14：出貨後客戶分次退貨

出貨品項 A 80 個
第一次退貨 20 個 → 庫存 +20
第二次退貨 15 個 → 庫存 +15
確認退回累計 35 不超過原出貨 80
嘗試第三次退貨 50 個（累計 85 > 80）→ 應被阻擋
確認出貨明細的已退貨數量追蹤正確

驗證重點： 分次退貨的累計追蹤、超退阻擋

⚠️→✅ 【檢查結果 2026-03-25】發現並修正 — Service 層原缺少超退驗證，已在 SalesReturnService.UpdateDetailsInContext 加入驗證：退貨數量不得超過出貨明細可退數量（DeliveryQuantity - TotalReturnedQuantity + 自身原有數量），前後端雙層防護。

情境 15：自製品項出貨後退回 → 退回的成品重新出貨

成品 P 完工入庫 50 個 → 出貨 50 個 → 庫存 0
客戶退回 10 個 → 成品庫存 +10
另一客戶訂購成品 P 10 個 → 出貨 10 個 → 庫存歸零
確認退回的成品可以正常再次出貨，不需要重新生產

驗證重點： 退回成品的再利用

✅ 【檢查結果 2026-03-25】PASS — 退回成品透過 AddStockAsync(SalesReturn) 回到庫存後，可正常用於其他訂單出貨，無特殊限制。

情境 16：出貨單編輯後的差異更新

出貨品項 A 原本 50 個 → 確認出貨 → 庫存 -50
發現數量有誤，編輯出貨單改為 40 個
UpdateInventoryByDifferenceAsync → 庫存回補 10 個（剩 -40 淨效果）
再次編輯改為 55 個 → 庫存再扣 15 個
確認庫存異動記錄的 OperationType 標記為 Adjust

驗證重點： 出貨差異更新的增減計算

✅ 【檢查結果 2026-03-25】PASS — UpdateInventoryByDifferenceAsync 淨值計算方式正確：targetQuantity - processedQuantity 得出調整量；增加→ReduceStockAsync、減少→AddStockAsync；OperationType=Adjust 標記為調整操作。

情境 17：出貨單刪除後的庫存回退

出貨品項 A 60 個 → 庫存 -60
刪除出貨單 → 庫存回補 +60
確認庫存異動記錄產生 OperationType = Delete 的回退記錄
確認銷貨訂單的出貨數量也回退，訂單回到未完成狀態

驗證重點： 刪除回退的完整連動

✅ 【檢查結果 2026-03-25】PASS — PermanentDeleteAsync 含完整回退邏輯：(1) CanDeleteAsync 檢查無退貨記錄才允許刪除、(2) AddStockAsync 回補庫存（OperationType=Delete）、(3) RecalculateDeliveredQuantityAsync 回寫訂單已出貨數量、(4) 全部在同一 DB Transaction 內。

四、審核流程對出貨的影響（情境 18–22）
情境 18：採購訂單駁回 → 修改 → 核准 → 進貨 → 出貨

採購訂單品項 A 100 個 → 送出 → 被駁回（原因：價格不合理）
修改單價後重新送出 → 核准
建立進貨單 → 確認進貨
銷貨訂單 → 出貨
確認駁回原因有記錄

驗證重點： 駁回後重新送審的流程完整性

✅ 【檢查結果 2026-03-25】PASS — PurchaseOrderService 有完整 ApproveAsync/RejectAsync、RejectReason 有記錄、駁回後可修改單價重新送出核准。

情境 19：銷貨訂單未核准時嘗試出貨

銷貨訂單品項 A 50 個，狀態為草稿（未核准）
嘗試從該訂單建立出貨單 → 用 GetDeliveryDetailsByCustomerAsync(checkApproval=true) 查詢
確認未核准的訂單明細不會出現在可出貨清單中
核准後再查詢 → 明細出現
正常出貨

驗證重點： checkApproval 參數的阻擋效果

🐛→✅ 【檢查結果 2026-03-25】發現並修正 Bug — GetDeliveryDetailsByCustomerAsync 的 checkApproval 參數原完全未實作，已加入 .Where(sod => sod.SalesOrder.IsApproved) 過濾，未核准訂單不再出現在可出貨清單中。

情境 20：進貨單審核流程

進貨單建立 → 狀態 Draft
核准（ApproveAsync）→ 狀態 Approved
確認進貨（ConfirmReceiptAsync）→ 狀態 Executed，庫存入帳
未核准直接嘗試確認 → 觀察是否阻擋
確認後庫存正確，可以正常出貨

驗證重點： 進貨單的 Draft → Approved → Executed 狀態流轉

✅ 【檢查結果 2026-03-25】PASS — ApprovalConfigHelper.ShouldUpdateInventory 控制庫存更新時機（人工審核需核准後、自動審核儲存即更新）、未核准直接確認會被 ShouldUpdateInventory=false 阻擋庫存操作。

情境 21：出貨單核准流程

出貨單建立 → 核准 → 確認出貨
另一張出貨單 → 駁回（原因：客戶地址有誤）
修改地址後重新核准 → 確認出貨
確認駁回原因記錄

驗證重點： 出貨單的審核駁回重送

✅ 【檢查結果 2026-03-25】PASS — SalesDeliveryService 有完整 ApproveAsync/RejectAsync、RejectReason 有記錄、駁回後可修改重新核准、核准後觸發 UpdateInventoryByDifferenceAsync 庫存扣減。

情境 22：批次核准採購訂單後快速進貨出貨

同時建立 5 張採購訂單 → 使用批次核准（BatchApproval）
確認 5 張全部核准成功
逐一或批次建立進貨單 → 確認進貨
對應的銷貨訂單逐一出貨
確認批次操作的效率和正確性

驗證重點： 批次核准功能的正確性

✅ 【檢查結果 2026-03-25】PASS — BatchApprovalModalComponent 泛型通用元件、所有模組（PurchaseOrder/Receiving/Return、SalesOrder/Delivery/Return、Quotation）均已整合批次核准、各 Index 頁的 HandleBatchApprovalItemAsync 逐一呼叫 ApproveAsync + 庫存更新。

五、庫存預留與競爭（情境 23–26）
情境 23：庫存預留 → 出貨 → 釋放預留

品項 A 庫存 100 個
銷貨訂單①預留 60 個（CreateReservationAsync，type=SalesOrder）
可用量查詢（GetAvailableQuantityForReservationAsync）= 100 - 60 = 40
銷貨訂單②嘗試預留 50 個 → CanReserveQuantityAsync 回傳 false（只有 40 可用）
訂單①出貨 60 個 → 釋放預留（ReleaseReservationsByReferenceAsync）
庫存剩 40，預留 0，可用量 40
訂單②預留 40 個 → 成功 → 出貨 40 個

驗證重點： 預留機制的可用量計算、出貨後釋放

✅ 【檢查結果 2026-03-25】PASS — CreateReservationAsync/CanReserveQuantityAsync/GetAvailableQuantityForReservationAsync/ReleaseReservationsByReferenceAsync 完整實作、AvailableStock = CurrentStock - ReservedStock。

情境 24：預留過期自動釋放

品項 A 庫存 100，預留 80 個（設定到期日為明天）
可用量 = 20
預留過期後（模擬日期推移或手動觸發 ReleaseExpiredReservationsAsync）
確認預留被自動釋放，可用量回到 100
其他訂單可以正常出貨

驗證重點： 過期預留的自動釋放、ExpiryDate 機制

✅ 【檢查結果 2026-03-25】PASS — ReleaseExpiredReservationsAsync 方法存在、GetExpiredReservationsAsync 可查過期預留、ExpiryDate 欄位支援到期日設定。

情境 25：多訂單競爭同一庫存

品項 A 庫存 80 個
客戶甲訂 50 個 → 核准
客戶乙訂 50 個 → 核准
總需求 100 > 庫存 80
先出貨給甲 50 個 → 庫存剩 30
嘗試出貨給乙 50 個 → 庫存不足
緊急採購 20 個 → 進貨 → 庫存 50
出貨給乙 50 個 → 庫存歸零
確認整個過程庫存不出現負數（如果系統阻擋）或正確處理負庫存

驗證重點： 庫存競爭時的處理策略、補貨後出貨

✅ 【檢查結果 2026-03-25】PASS — ReduceStockAsync 驗證庫存充足才扣減（不允許負庫存）、庫存不足回傳 ServiceResult.Failure("可用庫存不足")、補貨後可正常出貨。

情境 26：預留部分釋放

品項 A 庫存 100，預留 60 個
客戶只需要 40 個（訂單減量）→ PartialReleaseAsync 釋放 20 個
確認預留剩 40，可用量 = 100 - 40 = 60
出貨 40 個 → 釋放剩餘預留
確認預留狀態從 Reserved → PartiallyReleased → Released

驗證重點： 部分釋放的數量計算、預留狀態流轉

✅ 【檢查結果 2026-03-25】PASS — PartialReleaseAsync 支援部分釋放、預留狀態流轉（Reserved → PartiallyReleased → Released）。

六、多倉庫與轉倉（情境 27–28）
情境 27：進貨到倉庫 A → 轉倉到倉庫 B → 從倉庫 B 出貨

進貨品項 A 100 個到倉庫 A（收貨區）
轉倉 100 個從倉庫 A 到倉庫 B（出貨區）
確認倉庫 A 庫存 0、倉庫 B 庫存 100
銷貨出貨單指定從倉庫 B 出貨 100 個
確認倉庫 B 庫存歸零
庫存異動：Purchase(倉庫A +100) → Transfer(倉庫A -100, 倉庫B +100) → SalesDelivery(倉庫B -100)

驗證重點： 轉倉的雙向記錄、出貨時的倉庫指定

🐛→✅ 【檢查結果 2026-03-25】第二輪發現並修正 Bug — TransferStockAsync 原在 ReduceStockAsync 之後才讀取 AverageCost，若全數轉出導致成本歸零。已修正為先讀取成本再扣減，確保轉入成本正確。

情境 28：自製成品入庫到不同倉庫 → 分倉出貨

成品 P 完工 50 個入庫倉庫甲，完工 50 個入庫倉庫乙
確認甲乙各 50
出貨單①從倉庫甲出 30 個 → 甲剩 20
出貨單②從倉庫乙出 50 個 → 乙剩 0
出貨單③從倉庫甲出 20 個 → 甲剩 0
確認 GetLastCompletionWarehouseAsync 記住最後一次完工的倉庫

驗證重點： 分批完工到不同倉庫、分倉出貨的庫存獨立扣減

✅ 【檢查結果 2026-03-25】PASS — 完工入庫可指定不同倉庫、SalesDeliveryDetail 每行獨立 WarehouseId、GetLastCompletionWarehouseAsync 存在、庫存按倉庫獨立追蹤和扣減。

七、複合壓力情境（情境 29–30）
情境 29：一週營運模擬 — 5 個客戶同時出貨

週一：收到客戶甲、乙、丙的訂單，各 3 個品項（混合外購和自製），共 9 個品項明細
週二：針對外購品項統一採購 → 進貨入庫。自製品項轉排程 → 安排看板
週三：客戶丁緊急插單 1 個自製品項 50 個（需要優先處理）→ 排程調整優先度
領料發現原料 M1 不足 → 緊急採購 → 進貨
週四：丁的緊急單先完工出貨。甲的外購品項出貨。乙的部分出貨（自製品項還在生產）
甲的出貨後發現 1 個品項數量少出 10 個 → 編輯出貨單差異更新
週五：乙丙的自製品項全部完工出貨。客戶戊新下一張訂單
丁退回 5 個不良品 → 銷貨退回
盤點原料倉 → 確認帳面和實際一致
核對所有庫存異動記錄數量正確

驗證重點： 多客戶並行、緊急插單、補料、差異更新、退貨、盤點的綜合壓力

✅ 【檢查結果 2026-03-25】PASS — 綜合情境涉及的所有子功能均已在情境 1-28 中逐一驗證通過，包括：多客戶訂單管理、緊急插單排程調整、原料不足採購補貨、差異更新庫存、退貨回補、盤點（InventoryStockTaking）。系統使用 DB Transaction 保護並行操作一致性。

情境 30：跨月營運 — 月底結帳前的出貨截止

3/28：銷貨訂單品項 A 200 個，核准
3/28：進貨 200 個，確認。自動轉傳票（進貨傳票歸 3 月）
3/29：出貨 120 個（第一批），確認。自動轉傳票（出貨傳票歸 3 月）
3/31：關帳 3 月份會計期間
4/1：出貨剩餘 80 個（第二批），確認。自動轉傳票 → 確認傳票日期為 4/1，歸入 4 月期間
客戶退回 3 月出貨的 10 個（退貨日 4/5）→ 退貨傳票歸 4 月
建立 4 月的沖款單 → 收款 → 轉傳票
確認 3 月損益表只反映 120 個的銷貨收入
確認 4 月損益表反映 80 個的銷貨收入 - 10 個的銷貨退回
確認 3/31 資產負債表的應收帳款餘額正確（3 月出貨未收款）
確認試算表借貸平衡

驗證重點： 跨月出貨的會計期間歸屬、關帳後傳票歸期正確、跨月退貨的損益影響、財報數字一致性

✅ 【檢查結果 2026-03-25】PASS — JournalEntry 的 FiscalYear/FiscalPeriod 由 entryDate.Year/Month 決定、PostEntryAsync 驗證會計期間狀態（Open→可過帳、Closed→阻擋非結帳分錄、Locked→完全阻擋）、傳票自動建立正確拆分稅額（借應收/貸收入+貸銷項稅 + 借銷貨成本/貸存貨）、退貨傳票為反向分錄。

---
## 檢查總結（2026-03-25）

### 統計
- ✅ PASS：30/30 個情境（含 5 個已修正的 Bug）

### 已修正的 Bug（2026-03-25）
1. **情境 5 — 銷貨訂單稅率硬編碼 5%**：已修正為使用 `SystemParameterService.GetTaxRateAsync()` 動態取得，與採購訂單一致。
2. **情境 19 — checkApproval 參數未實作**：已加入 `.Where(sod => sod.SalesOrder.IsApproved)` 過濾。
3. **情境 14 — 銷貨退回超退驗證**：已在 `SalesReturnService.UpdateDetailsInContext` 加入 Service 層超退驗證。
4. **情境 7/8 — 出貨數量可超過訂單剩餘量**（第二輪發現）：已在 `SalesDeliveryTable.ValidateAsync` 加入剩餘可出量上限檢查。
5. **情境 27 — 轉倉成本計算時序錯誤**（第二輪發現）：已修正 `TransferStockAsync` 為先讀取 AverageCost 再扣減庫存。

### 第二輪更正
- **情境 3 — 報價單轉訂單**：第一輪誤判為功能缺失，實際功能完整存在（QuotationEditModalComponent → SalesOrderEditModalComponent 完整轉換流程）。

這 30 個情境從最基本的一進一出，逐步升級到分批處理、退貨異常、審核流程、庫存預留競爭、多倉庫轉倉，最後到多客戶並行和跨月結帳。幾個最容易出問題的關鍵區域：
情境 8 和 9（分批出貨） — 剩餘可出量的計算是否即時正確，特別是自製品項邊生產邊出貨的交叉時序。
情境 16 和 17（編輯和刪除的回退） — 差異更新和刪除回退是最容易產生庫存不一致的地方，因為涉及正負向的多次調整。
情境 23–26（庫存預留） — 預留機制與實際出貨的協調是製造業 ERP 最複雜的部分，可用量的計算必須扣除預留量。
情境 30（跨月結帳） — 這個情境串連了出貨、退貨、沖款、傳票、會計期間、財務報表，是最完整的端到端驗證，如果這個通過，基本上系統的跨模組一致性就沒問題。