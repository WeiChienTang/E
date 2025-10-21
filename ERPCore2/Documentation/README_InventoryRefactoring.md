# 庫存系統重構指南

## 變更概述

將庫存系統從「單層結構」重構為「主檔+明細」的雙層結構：

### 舊結構
```
InventoryStock (庫存記錄)
├─ ProductId (商品)
├─ WarehouseId (倉庫)
├─ WarehouseLocationId (庫位)
├─ CurrentStock (庫存數)
└─ 唯一索引: [ProductId, WarehouseId, WarehouseLocationId]
```

### 新結構
```
InventoryStock (庫存主檔 - 商品總庫存)
├─ ProductId (商品) - 唯一
├─ MinStockLevel (最低警戒線)
├─ MaxStockLevel (最高警戒線)
└─ InventoryStockDetails (1對多)
        ↓
InventoryStockDetail (庫存明細 - 各倉庫庫位分佈)
├─ InventoryStockId (主檔FK)
├─ WarehouseId (倉庫)
├─ WarehouseLocationId (庫位)
├─ CurrentStock (該位置庫存)
└─ 唯一索引: [InventoryStockId, WarehouseId, WarehouseLocationId]
```

## 計算屬性變更

InventoryStock 的以下欄位改為計算屬性（從明細加總）：

- `TotalCurrentStock` - 從 InventoryStockDetails 加總
- `TotalReservedStock` - 從 InventoryStockDetails 加總
- `TotalAvailableStock` - TotalCurrentStock - TotalReservedStock
- `TotalInTransitStock` - 從 InventoryStockDetails 加總
- `WeightedAverageCost` - 加權平均成本
- `LastTransactionDate` - 最後交易日期（計算屬性，唯讀）

## 需要修改的檔案列表

### ✅ 已完成
1. ✅ `Data/Entities/Warehouses/InventoryStockDetail.cs` - 新建明細實體
2. ✅ `Data/Entities/Warehouses/InventoryStock.cs` - 改為主檔結構
3. ✅ `Data/Entities/Warehouses/Warehouse.cs` - 新增導航屬性
4. ✅ `Data/Entities/Warehouses/WarehouseLocation.cs` - 移除舊導航屬性
5. ✅ `Data/Context/AppDbContext.cs` - 新增 DbSet 和配置
6. ✅ `Services/Warehouses/IInventoryStockDetailService.cs` - 新建介面
7. ✅ `Services/Warehouses/InventoryStockDetailService.cs` - 新建服務
8. ✅ `Data/ServiceRegistration.cs` - 註冊服務

### ⏳ 待修改（164個錯誤）
9. ⏳ `Services/Warehouses/InventoryStockService.cs` - 主要服務邏輯（~1500行）
10. ⏳ `Services/Inventory/StockTakingService.cs` - 盤點服務
11. ⏳ `Services/Inventory/InventoryReservationService.cs` - 預留服務
12. ⏳ `Helpers/DependencyCheckHelper.cs` - 依賴檢查
13. ⏳ `Components/Pages/Warehouse/InventoryStockIndex.razor` - 列表頁面
14. ⏳ `Components/Pages/Warehouse/InventoryStockEditModalComponent.razor` - 編輯Modal
15. ⏳ `Components/FieldConfiguration/InventoryStockFieldConfiguration.cs` - 欄位配置
16. ⏳ `Data/SeedDataManager/Seeders/InventoryStockSeeder.cs` - 種子資料

## 修改策略建議

### 方案 A：先執行 Migration（推薦）

**優點：** 
- 資料庫結構立即到位
- 可以逐步修復程式碼
- 測試環境可立即驗證

**步驟：**
```powershell
# 1. 建立 Migration
Add-Migration RefactorInventoryStockToMasterDetail -Context AppDbContext

# 2. 檢查 Migration 檔案（確認資料轉換邏輯）

# 3. 執行 Migration
Update-Database -Context AppDbContext
```

**資料轉換邏輯：**
Migration 會自動：
1. 建立 `InventoryStockDetails` 資料表
2. 將現有 `InventoryStocks` 資料：
   - 相同 ProductId 合併為一筆主檔
   - 原 WarehouseId/LocationId 資料移到明細表
   - CurrentStock 等數值移到明細表
3. 移除 `InventoryStocks` 的 WarehouseId/LocationId/CurrentStock 等欄位

### 方案 B：暫時回退，逐步遷移

如果擔心大規模修改風險，可以：
1. 暫時保留舊結構
2. 先建立 InventoryStockDetail 表（但不強制使用）
3. 逐個模組遷移

## 程式碼修改重點

### InventoryStockService 主要變更

**舊寫法：**
```csharp
// 直接從 InventoryStock 讀取
var stock = await context.InventoryStocks
    .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
if (stock == null) { /* 建立新庫存 */ }
stock.CurrentStock += quantity;
```

**新寫法：**
```csharp
// 先取得或建立主檔
var stock = await context.InventoryStocks
    .Include(s => s.InventoryStockDetails)
    .FirstOrDefaultAsync(i => i.ProductId == productId);
if (stock == null) { 
    stock = new InventoryStock { ProductId = productId };
    context.InventoryStocks.Add(stock);
    await context.SaveChangesAsync();
}

// 再處理明細
var detail = stock.InventoryStockDetails
    .FirstOrDefault(d => d.WarehouseId == warehouseId && d.WarehouseLocationId == locationId);
if (detail == null) {
    detail = new InventoryStockDetail { 
        InventoryStockId = stock.Id,
        WarehouseId = warehouseId,
        WarehouseLocationId = locationId 
    };
    stock.InventoryStockDetails.Add(detail);
}
detail.CurrentStock += quantity;
```

### 前端組件變更

**InventoryStockIndex.razor：**
- 改為顯示商品總庫存（從 InventoryStock 主檔）
- 點擊進入編輯時，顯示各倉庫明細

**InventoryStockEditModalComponent.razor：**
- 改為主檔+明細表格模式
- 顯示商品基本資訊和總庫存設定
- 下方表格列出各倉庫/庫位的明細

## 下一步行動

請選擇您的遷移策略：

### 選項 1：立即執行 Migration（推薦）
```powershell
Add-Migration RefactorInventoryStockToMasterDetail -Context AppDbContext
Update-Database -Context AppDbContext
```
然後我會協助逐一修復 164 個錯誤。

### 選項 2：先修復關鍵服務
優先修復：
1. InventoryStockService（核心邏輯）
2. StockTakingService（盤點）
3. Seeder（種子資料）
然後再執行 Migration。

### 選項 3：分階段遷移
建立相容層，同時支援新舊兩種模式，逐步遷移。

---

**建議：選擇選項 1**
因為資料庫結構變更是最底層的，先完成可以確保之後的程式碼修改是正確的方向。
