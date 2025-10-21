README：庫存重構計畫

目的
----
將現有的庫存結構從「商品-倉庫-倉庫位置三元素唯一組合」改為「商品為主檔（1筆）」，透過中介表（倉庫、倉庫位置）記錄各倉庫與位置的庫存明細。此變更讓庫存主檔（InventoryStock）以商品為中心，便於整體庫存統計與管理，並減少重複商品主檔。

總覽
----
本檔案分為多個實施階段：資料模型、遷移、服務層、UI、測試與回滾。每一階段列出受影響的檔案與建議步驟。

階段 0：前置作業
-----------------
- 在新的 git 分支上進行（例如：feature/inventory-restructure）。
- 建立完整備份或使用測試資料庫（目前為測試環境，此步驟仍建議）。
- 撰寫變更 spec 與測試範圍。

階段 1：資料模型設計（不可逆前快照）
--------------------------------------
目標：新增必要的中介實體，保留原本 `InventoryStock` 的核心屬性。

修改內容：
1. InventoryStock (改造)
   - 移除：WarehouseId、WarehouseLocationId
   - 新增/保留：ProductId、TotalStock/CurrentStockAggregate（或透過查詢聚合）
   - 保留欄位：ReservedStock、AverageCost、Min/MaxStockLevel（視需求）

2. 新增 InventoryStockWarehouse
   - 屬性：Id, InventoryStockId, WarehouseId, CurrentStock, ReservedStock, InTransitStock, LastTransactionDate
   - FK：InventoryStock、Warehouse

3. 新增 InventoryStockWarehouseLocation
   - 屬性：Id, InventoryStockWarehouseId, WarehouseLocationId, CurrentStock, ReservedStock, BatchNumber, BatchDate, ExpiryDate
   - FK：InventoryStockWarehouse、WarehouseLocation

4. 更新 InventoryTransaction / InventoryReservation
   - 把原本的 InventoryStockId 參考更新為新的 InventoryStockWarehouse 或保留 InventoryStockId 並加入 WarehouseId/WarehouseLocationId

檔案（可能要新增或修改）：
- Data/Entities/Warehouses/InventoryStock.cs
- Data/Entities/Warehouses/InventoryStockWarehouse.cs (新增)
- Data/Entities/Warehouses/InventoryStockWarehouseLocation.cs (新增)
- Data/Entities/Inventory/InventoryTransaction.cs (修改)
- Data/Entities/Inventory/InventoryReservation.cs (修改)
- Data/Context/AppDbContext.cs (新增 DbSet 與 OnModelCreating 配置)

注意：欄位命名需與既有邏輯兼容，避免衝突。

階段 2：資料庫遷移與資料轉換
-------------------------------

步驟：
1. 在 dev 分支建立遷移腳本（EF Core migration）：新增表格 `InventoryStockWarehouses`、`InventoryStockWarehouseLocations`；調整 `InventoryStocks` 欄位。
2. 編寫資料轉換腳本（可以是 EF seed 或 SQL）：把現有每筆 InventoryStock（含 Warehouse, Location）轉換為：
   - InventoryStock 保留一筆（如果 ProductId 已存在則合併）
   - 為每個（ProductId, WarehouseId）建立 InventoryStockWarehouse
   - 為每個（ProductId, WarehouseId, WarehouseLocationId）建立 InventoryStockWarehouseLocation
   - 合計與計算：InventoryStock.CurrentStock = SUM(InventoryStockWarehouse.CurrentStock)
3. 測試遷移：使用測試資料庫，驗證資料無遺漏與一致性
4. 執行遷移（於 staging/production 時，先停機或使用版本切換策略）

資料轉換範例：
- 對每筆舊資料：
  - 如果沒有 InventoryStock for ProductId：新增 InventoryStock
  - 取得或建立 InventoryStockWarehouse for (InventoryStockId, WarehouseId)
  - 建立 InventoryStockWarehouseLocation for location
  - 把數量遷移進當前位置記錄

階段 3：服務層（後端）重構
--------------------------
重點檔案：
- Services/Warehouses/InventoryStockService.cs
- Services/Warehouses/IInventoryStockService.cs
- 其他依賴服務：SalesOrderService、PurchaseReceivingService、PurchaseReturnService、StockTakingService、ReservationService 等

建議：
1. 保持原有 API 名稱，但更新實作：
   - AddStockAsync(productId, warehouseId, locationId, ...) 應操作 InventoryStockWarehouse/InventoryStockWarehouseLocation
   - ReduceStockAsync(...) 應從 WarehouseLocation 或按照 FIFO 從 WarehouseLocation 扣減
   - GetAvailableStockAsync(productId, warehouseId) 應以聚合方式回傳
2. 新增內部 helper：
   - GetOrCreateInventoryStock(productId)
   - GetOrCreateInventoryStockWarehouse(inventoryStockId, warehouseId)
   - GetOrCreateInventoryStockWarehouseLocation(...)
3. 更新交易記錄：InventoryTransaction 應能關聯到 WarehouseLocation/InventoryStockWarehouse
4. 更新預留機制：預留分倉庫與位置
5. 單元測試：
   - 新增/減少/轉移/調整/保留的 happy-path 與邊界情境

階段 4：前端（UI）修改
--------------------------
目標：在 `InventoryStockEditModalComponent.razor` 中顯示該商品所有倉庫與位置的列表，並可在此編輯每個倉庫與位置的庫存數。

重點檔案：
- Components/Pages/Warehouse/InventoryStockEditModalComponent.razor
- Components/Shared/SubCollections/InteractiveTableComponent.razor
- Components/Pages/Warehouse/InventoryStockIndex.razor

步驟：
1. 在編輯 Modal 中加入一個 List<InventoryStockWarehouseViewModel>，由 service 提供
2. 使用 `InteractiveTableComponent` 顯示每個倉庫/位置行（可編輯當前值/備註/批號）
3. 在儲存時一併送到 API，由 server 對每個倉庫/位置進行增減或覆寫
4. Index頁面：顯示總可用數量（聚合）與統計資料

階段 5：端到端測試與整合
--------------------------------
- 建立整合測試，模擬採購收貨、銷貨出貨、退貨、盤點
- 驗證復原/回滾功能（Transaction logs）

階段 6：部署與回滾計畫
------------------------
- 準備遷移回滾腳本
- 在 staging/production 環境逐步部署並監控

附錄：受影響的程式檔案快速清單（初步）
------------------------------------------------
- Data/Entities/Warehouses/InventoryStock.cs
- Data/Entities/Warehouses/InventoryStockWarehouse.cs (新增)
- Data/Entities/Warehouses/InventoryStockWarehouseLocation.cs (新增)
- Data/Entities/Inventory/InventoryTransaction.cs
- Data/Entities/Inventory/InventoryReservation.cs
- Data/Context/AppDbContext.cs
- Services/Warehouses/InventoryStockService.cs
- Services/Warehouses/IInventoryStockService.cs
- Services/Purchase/*
- Services/Sales/*
- Components/Pages/Warehouse/InventoryStockEditModalComponent.razor
- Components/Pages/Warehouse/InventoryStockIndex.razor
- Components/Shared/SubCollections/InteractiveTableComponent.razor

結語
----
此重構須謹慎分階段執行。若要我繼續，下一步我可以：
- 實作資料模型檔（新增 InventoryStockWarehouse 與 Location 實體）並同步修改 DbContext（推薦）
- 或者先提供一個更詳細的資料遷移 SQL/EF 腳本範例

請告訴我您要先執行哪一項，我會標記 Todo 並開始執行下一步。