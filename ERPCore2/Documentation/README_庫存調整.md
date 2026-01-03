# 庫存單位轉換修改指南

**文件日期：** 2026年1月3日  
**修改狀態：** 進行中  
**修改目的：** 將庫存數量欄位從 `int` 改為 `decimal(18,4)`，支援製程單位轉換的小數計算

---

## 一、問題描述

### 1.1 現況說明

目前系統在 Product（商品）實體中已設計完善的單位轉換機制：
- `UnitId` — 基本單位（庫存單位）
- `ProductionUnitId` — 製程單位
- `ProductionUnitConversionRate` — 製程單位換算率（1 基本單位 = N 製程單位）

然而，庫存相關實體的數量欄位均使用 `int` 整數型別，導致無法正確記錄單位轉換後的小數數量。

### 1.2 問題實例

以水泥商品為例：
- 基本單位：包
- 製程單位：公斤
- 換算率：80（1 包 = 80 公斤）

製程消耗情境：
- BOM 設定：1 個磚頭需要 0.5 公斤水泥
- 生產 100 個磚頭 × 0.5 公斤 = 50 公斤
- 換算成基本單位：50 ÷ 80 = **0.625 包**

**問題：** `CurrentStock` 為 `int` 型別，無法儲存 0.625，會被截斷為 0 或 1。

---

## 二、修改清單與進度

### 2.1 實體層修改 ✅ 已完成

| 檔案 | 欄位 | 原型別 | 新型別 | 狀態 |
|------|------|--------|--------|------|
| `InventoryStockDetail.cs` | `CurrentStock` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `ReservedStock` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `AvailableStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `InTransitStock` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `InProductionStock` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `MinStockLevel` | `int?` | `decimal(18,4)?` | ✅ 已完成 |
| `InventoryStockDetail.cs` | `MaxStockLevel` | `int?` | `decimal(18,4)?` | ✅ 已完成 |
| `InventoryStock.cs` | `TotalCurrentStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryStock.cs` | `TotalReservedStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryStock.cs` | `TotalAvailableStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryStock.cs` | `TotalInTransitStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryStock.cs` | `TotalInProductionStock` | `int` | `decimal` | ✅ 已完成 |
| `InventoryTransaction.cs` | `Quantity` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryTransaction.cs` | `StockBefore` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryTransaction.cs` | `StockAfter` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryReservation.cs` | `ReservedQuantity` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryReservation.cs` | `ReleasedQuantity` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `InventoryReservation.cs` | `RemainingQuantity` | `int` | `decimal` | ✅ 已完成 |
| `StockTakingDetail.cs` | `SystemStock` | `int` | `decimal(18,4)` | ✅ 已完成 |
| `StockTakingDetail.cs` | `ActualStock` | `int?` | `decimal(18,4)?` | ✅ 已完成 |
| `StockTakingDetail.cs` | `DifferenceQuantity` | `int?` | `decimal?` | ✅ 已完成 |

### 2.2 介面層修改 ✅ 已完成

| 檔案 | 方法/屬性 | 修改內容 | 狀態 |
|------|----------|----------|------|
| `IInventoryStockService.cs` | `GetAvailableStockAsync` | 回傳值 `int` → `decimal` | ✅ 已完成 |
| `IInventoryStockService.cs` | `GetTotalAvailableStockByWarehouseAsync` | 回傳值 `int` → `decimal` | ✅ 已完成 |
| `IInventoryStockService.cs` | `AddStockAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `ReduceStockAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `TransferStockAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `AdjustStockAsync` | 參數 `int newQuantity` → `decimal newQuantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `ReserveStockAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `ReleaseReservationAsync` | 參數 `int? releaseQuantity` → `decimal?` | ✅ 已完成 |
| `IInventoryStockService.cs` | `IsStockAvailableAsync` | 參數 `int requiredQuantity` → `decimal` | ✅ 已完成 |
| `IInventoryStockService.cs` | `ValidateStockOperationAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `ReduceStockWithFIFOAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `RevertStockToOriginalAsync` | 參數 `int quantity` → `decimal quantity` | ✅ 已完成 |
| `IInventoryStockService.cs` | `BatchUpdateStockLevelAlertsAsync` | 參數 tuple 改為 `decimal?` | ✅ 已完成 |
| `IStockTakingService.cs` | `UpdateStockTakingDetailAsync` | 參數 `int actualStock` → `decimal` | ✅ 已完成 |
| `IStockTakingService.cs` | `StockTakingDetailUpdateModel.ActualStock` | `int` → `decimal` | ✅ 已完成 |

### 2.3 服務層修改 ✅ 已完成

| 檔案 | 類別/方法 | 修改內容 | 狀態 |
|------|----------|----------|------|
| `InventoryStockService.cs` | `BatchReductionDetail.ReduceQuantity` | `int` → `decimal` | ✅ 已完成 |
| `InventoryStockService.cs` | 所有庫存操作方法 | 參數型別改為 `decimal` | ✅ 已完成 |
| `InventoryStockService.cs` | `netReductions` 字典 | 值類型 `int` → `decimal` | ✅ 已完成 |
| `InventoryStockService.cs` | `ReduceStockFromSpecificBatchAsync` | 參數 `int quantity` → `decimal` | ✅ 已完成 |
| `StockTakingService.cs` | `UpdateStockTakingDetailAsync` | 參數 `int actualStock` → `decimal` | ✅ 已完成 |
| `InventoryReservationService.cs` | `GetTotalReservedQuantityAsync` | 回傳值 `int` → `decimal` | ✅ 已完成 |
| `InventoryReservationService.cs` | `GetAvailableQuantityForReservationAsync` | 回傳值 `int` → `decimal` | ✅ 已完成 |
| `MaterialIssueDetailService.cs` | `ValidateStockAvailabilityAsync` | tuple 回傳值 `int` → `decimal` | ✅ 已完成 |
| `InventoryTransactionService.cs` | `CreateAdjustmentTransactionAsync` | 參數 `int` → `decimal` | ✅ 已完成 |
| `SalesReturnService.cs` | 庫存處理字典 | 數量類型 `int` → `decimal` | ✅ 已完成 |
| `SalesDeliveryService.cs` | 庫存處理字典 | 數量類型 `int` → `decimal` | ✅ 已完成 |
| `PurchaseReceivingService.cs` | 庫存處理字典 | 數量類型 `int` → `decimal` | ✅ 已完成 |
| `MaterialIssueService.cs` | 庫存處理字典 | 數量類型 `int` → `decimal` | ✅ 已完成 |

### 2.4 UI 組件層修改 ✅ 已完成

| 檔案 | 類別/屬性 | 修改內容 | 狀態 |
|------|----------|----------|------|
| `StockLevelAlertModalComponent.razor` | `StockLevelAlertItem` 類別 | 數量欄位 `int` → `decimal` | ✅ 已完成 |
| `StockAlertViewModalComponent.razor` | `StockAlertViewItem` 類別 | 數量欄位 `int` → `decimal` | ✅ 已完成 |
| `MaterialIssueTable.razor` | `CurrentStockQuantity` | `int?` → `decimal?` | ✅ 已完成 |
| `SalesDeliveryTable.razor` | `DeliveryItem.CurrentStockQuantity` | `int?` → `decimal?` | ✅ 已完成 |

---

## 三、待完成工作

### 3.1 資料庫遷移 ⏳ 待執行

執行以下命令建立並套用遷移：

```powershell
# 建立遷移
dotnet ef migrations add InventoryDecimalQuantity

# 套用遷移（開發環境）
dotnet ef database update
```

### 3.2 編譯驗證 ✅ 已完成

修改完成後已進行完整編譯，確認所有相依程式碼已正確更新：

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3.3 功能測試 ⏳ 待執行

修改後需全面測試以下功能：

| 功能模組 | 測試項目 | 狀態 |
|----------|----------|------|
| 進貨作業 | 進貨入庫、小數數量 | ⏳ 待測試 |
| 出貨作業 | 出貨扣庫、小數數量 | ⏳ 待測試 |
| 領料作業 | 製程領料、單位換算 | ⏳ 待測試 |
| 退料作業 | 退料回庫 | ⏳ 待測試 |
| 盤點作業 | 盤點差異計算 | ⏳ 待測試 |
| 庫存預留 | 預留與釋放 | ⏳ 待測試 |
| 庫存調整 | 盤盈盤虧調整 | ⏳ 待測試 |
| 庫存轉移 | 倉庫間轉移 | ⏳ 待測試 |

---

## 四、注意事項

### 4.1 精度選擇
- 使用 `decimal(18,4)` 提供 4 位小數精度
- 足以應付大多數單位換算需求（如 0.0001 包）

### 4.2 向後相容性
- `int` 轉 `decimal` 資料不會遺失
- 現有整數值會自動轉換（如 10 → 10.0000）

### 4.3 效能考量
- `decimal` 運算比 `int` 稍慢
- 在庫存計算場景下影響極小，可忽略

### 4.4 UI 顯示建議
調整數字格式化，建議顯示方式：
- 整數時：`10`（不顯示小數）
- 小數時：`9.375`（顯示有效小數位）
- 考慮提供雙重顯示：`9.375 包（750 公斤）`

---

## 五、相關文件

- [README_商品單位換算修改.md](README_商品單位換算修改.md) - 商品單位換算設計
- [README_庫存異動正確撰寫方式.md](README_庫存異動正確撰寫方式.md) - 庫存異動規範
- [README_倉庫異動修改(最新版).md](README_倉庫異動修改(最新版).md) - 倉庫異動說明

---

## 六、修改歷程

| 日期 | 版本 | 修改內容 | 執行者 |
|------|------|----------|--------|
| 2026-01-03 | v1.0 | 初始修改：實體層、介面層、服務層、UI層完成，編譯通過 | AI Assistant |

---

## 七、程式碼範例

### 修改前
```csharp
[Display(Name = "現有庫存")]
public int CurrentStock { get; set; } = 0;
```

### 修改後
```csharp
[Display(Name = "現有庫存")]
[Column(TypeName = "decimal(18,4)")]
public decimal CurrentStock { get; set; } = 0;
```

---

**下一步行動：**
1. 執行 `dotnet build` 檢查編譯錯誤
2. 修復可能遺漏的相依程式碼
3. 建立資料庫遷移
4. 進行功能測試
