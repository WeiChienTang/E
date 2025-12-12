# 倉庫異動修改說明（最新版）

**日期：2025-12-12**  
**版本：v2.0**

---

## 📋 目錄

1. [問題發現](#問題發現)
2. [核心問題分析](#核心問題分析)
3. [解決方案](#解決方案)
4. [修改清單](#修改清單)
5. [測試場景](#測試場景)
6. [技術細節](#技術細節)

---

## 🔍 問題發現

### 1. 初始問題：服務層不一致

發現 `PurchaseReceivingService` 和 `SalesDeliveryService` 對於 `InventoryTransaction` 資料表的處理方式不同：

- **PurchaseReceivingService**：
  - 新增時使用 `Purchase` 類型
  - 退貨調整使用 `Return` 類型
  - 刪除時使用 `Return` 類型回沖

- **SalesDeliveryService**：
  - 新增時使用 `Sale` 類型
  - 退貨調整**也使用 `Sale` 類型**（❌ 錯誤）
  - 刪除時使用 `Sale` 類型回沖（❌ 語意不清）

### 2. 交易類型命名問題

- 原有的 `Return` 枚舉（值：4）描述為「退貨」，實際用於「**採購退貨**」
- 缺少「**銷貨退回**」的專用類型
- 導致 `SalesDeliveryService` 和 `SalesReturnService` 混用 `Sale` 類型

### 3. 審計追蹤缺失

在 `PermanentDeleteAsync` 方法中發現會刪除 `_ADJ` 記錄：

```csharp
// 舊邏輯（錯誤）
var adjTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber.StartsWith(currentReceiving.Code + "_ADJ"))
    .ToListAsync();
context.InventoryTransactions.RemoveRange(adjTransactions);
```

這會導致：
- ❌ 遺失歷史編輯記錄
- ❌ 無法追蹤數量異動軌跡
- ❌ 審計追蹤不完整

### 4. 🔥 關鍵 Bug：庫存累加錯誤

#### 問題描述

`UpdateInventoryByDifferenceAsync` 方法**只查詢 `_ADJ` 後綴的記錄**來計算已處理數量：

```csharp
// 舊邏輯（錯誤）
var existingTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber.StartsWith(currentReceiving.Code + "_ADJ"))
    .ToListAsync();
```

這導致：
- ❌ **遺漏初次新增的記錄**（無後綴）
- ❌ 已處理數量永遠為 0
- ❌ 每次編輯都當作「全新新增」，造成累加

#### 實際案例

```
T1: 新增 R001，產品A 44個  → 庫存: 44
T2: 編輯為 60個            → 庫存: 104  ❌（應為 60）
     計算邏輯：
     - 只查到 0 筆 _ADJ 記錄
     - 已處理數量 = 0
     - 調整量 = 60 - 0 = 60（錯誤！應為 60 - 44 = 16）
     - 庫存 = 44 + 60 = 104
```

### 5. 刪除後重建問題

```
T1: 新增 R001 (44個)     → 庫存: 44
T2: 編輯為 60個          → 庫存: 60，產生 R001_ADJ
T3: 刪除 R001            → 庫存: 0，產生 R001_DEL
T4: 重新新增 R001 (50個) → 庫存: 50（正確）
T5: 編輯為 70個          → 庫存: ？
```

問題：T2 的 `R001_ADJ` 記錄仍存在，T5 會錯誤計算已處理數量。

---

## 🔬 核心問題分析

### 問題根源

1. **查詢邏輯錯誤**：只查 `_ADJ` 遺漏了無後綴的初始記錄
2. **命名規範不一致**：交易類型語意不清
3. **資料清理錯誤**：刪除 `_ADJ` 破壞審計追蹤
4. **批次邊界不清**：無法區分「同單號不同批次」的記錄

### 根本原因

ERP 系統設計初期未考慮以下場景：
- 刪除後重新新增相同單號
- 需要完整的審計追蹤（保留所有異動記錄）
- 不同交易類型的語意區分（採購退貨 vs 銷貨退回）

---

## ✅ 解決方案

### 方案選擇：有效批次追蹤（Effective Batch Tracking）

使用 `_DEL` 後綴作為**批次邊界標記**，只統計最後一次刪除之後的記錄。

#### 核心邏輯

```csharp
// 1. 查詢所有相關交易（包含 _ADJ 和無後綴）
var allTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == code || 
                t.TransactionNumber.StartsWith(code + "_"))
    .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
    .ToListAsync();

// 2. 找到最後一次刪除的批次邊界
var lastDeleteTransaction = allTransactions
    .Where(t => t.TransactionNumber.EndsWith("_DEL"))
    .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
    .FirstOrDefault();

// 3. 只取最後刪除之後的有效記錄
var effectiveTransactions = lastDeleteTransaction != null
    ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id && 
                                 !t.TransactionNumber.EndsWith("_DEL")).ToList()
    : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL")).ToList();
```

#### 優勢

- ✅ **正確計算已處理數量**：包含初始記錄 + 所有 _ADJ
- ✅ **支援刪除重建**：自動忽略舊批次的 _ADJ
- ✅ **保留完整審計追蹤**：所有記錄永久保存
- ✅ **語意清晰**：_DEL 明確標記批次結束

---

## 📝 修改清單

### 1. 新增銷貨退回枚舉

**檔案：`Data/Enums/InventoryEnums.cs`**

```csharp
public enum InventoryTransactionTypeEnum
{
    Purchase = 2,           // 採購入庫
    Sale = 3,               // 銷貨出庫
    Return = 4,             // 採購退貨（改名）
    Adjustment = 5,         // 庫存調整
    Transfer = 6,           // 調撥
    SalesReturn = 12        // 銷貨退回（新增）
}
```

**變更說明：**
- `Return` 改描述為「採購退貨」
- 新增 `SalesReturn = 12`（銷貨退回）

### 2. 修改 SalesDeliveryService

**檔案：`Services/SalesDeliveryService.cs`**

#### 2.1 UpdateInventoryByDifferenceAsync

```csharp
// ✅ 新邏輯：查詢所有相關交易並實現有效批次追蹤
var allTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == currentDelivery.Code || 
                t.TransactionNumber.StartsWith(currentDelivery.Code + "_"))
    .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
    .ToListAsync();

var lastDeleteTransaction = allTransactions
    .Where(t => t.TransactionNumber.EndsWith("_DEL"))
    .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
    .FirstOrDefault();

var existingTransactions = lastDeleteTransaction != null
    ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id && 
                                 !t.TransactionNumber.EndsWith("_DEL")).ToList()
    : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL")).ToList();

// 調整庫存時使用 SalesReturn（而非 Sale）
await _inventoryStockService.AdjustStockAsync(
    productId: detail.ProductId,
    warehouseId: currentDelivery.WarehouseId,
    quantityChange: quantityDiff,
    transactionType: InventoryTransactionTypeEnum.SalesReturn,
    transactionNumber: $"{currentDelivery.Code}_ADJ",
    transactionDate: currentDelivery.DeliveryDate,
    notes: notes
);
```

#### 2.2 PermanentDeleteAsync

```csharp
// ✅ 移除刪除 _ADJ 的邏輯
// 舊邏輯已刪除，只保留新增 _DEL 記錄

// 只新增 _DEL 回沖記錄（使用 SalesReturn）
await _inventoryStockService.AdjustStockAsync(
    productId: detail.ProductId,
    warehouseId: delivery.WarehouseId,
    quantityChange: detail.Quantity,
    transactionType: InventoryTransactionTypeEnum.SalesReturn,
    transactionNumber: $"{delivery.Code}_DEL",
    transactionDate: DateTime.UtcNow,
    notes: $"刪除銷貨單 {delivery.Code}，回沖庫存"
);
```

### 3. 修改 SalesReturnService

**檔案：`Services/SalesReturnService.cs`**

#### 3.1 UpdateInventoryByDifferenceAsync

```csharp
// ✅ 實現有效批次追蹤
var allTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == currentReturn.Code || 
                t.TransactionNumber.StartsWith(currentReturn.Code + "_"))
    .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
    .ToListAsync();

var lastDeleteTransaction = allTransactions
    .Where(t => t.TransactionNumber.EndsWith("_DEL"))
    .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
    .FirstOrDefault();

var existingTransactions = lastDeleteTransaction != null
    ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id && 
                                 !t.TransactionNumber.EndsWith("_DEL")).ToList()
    : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL")).ToList();

// 調整庫存時使用 SalesReturn（增加庫存）
await _inventoryStockService.AdjustStockAsync(
    productId: detail.ProductId,
    warehouseId: currentReturn.WarehouseId,
    quantityChange: quantityDiff,
    transactionType: InventoryTransactionTypeEnum.SalesReturn,
    transactionNumber: $"{currentReturn.Code}_ADJ",
    transactionDate: currentReturn.ReturnDate,
    notes: notes
);
```

#### 3.2 PermanentDeleteAsync

```csharp
// ✅ 移除刪除 _ADJ 的邏輯，使用正確的交易類型

// 只新增 _DEL 回沖記錄（使用 Sale 減少庫存）
await _inventoryStockService.AdjustStockAsync(
    productId: detail.ProductId,
    warehouseId: salesReturn.WarehouseId,
    quantityChange: -detail.Quantity,
    transactionType: InventoryTransactionTypeEnum.Sale,
    transactionNumber: $"{salesReturn.Code}_DEL",
    transactionDate: DateTime.UtcNow,
    notes: $"刪除銷貨退回單 {salesReturn.Code}，回沖庫存"
);
```

### 4. 修改 PurchaseReceivingService

**檔案：`Services/PurchaseReceivingService.cs`**

#### 4.1 UpdateInventoryByDifferenceAsync

```csharp
// ✅ 實現有效批次追蹤
var allTransactions = await context.InventoryTransactions
    .Where(t => t.TransactionNumber == currentReceiving.Code || 
                t.TransactionNumber.StartsWith(currentReceiving.Code + "_"))
    .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
    .ToListAsync();

var lastDeleteTransaction = allTransactions
    .Where(t => t.TransactionNumber.EndsWith("_DEL"))
    .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
    .FirstOrDefault();

var existingTransactions = lastDeleteTransaction != null
    ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id && 
                                 !t.TransactionNumber.EndsWith("_DEL")).ToList()
    : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL")).ToList();

// 其餘邏輯維持不變（使用 Purchase 和 Return）
```

#### 4.2 PermanentDeleteAsync

```csharp
// ✅ 移除刪除 _ADJ 的邏輯
// 保持使用 Return 類型（採購退貨）進行回沖
```

### 5. PurchaseReturnService

**檔案：`Services/PurchaseReturnService.cs`**

**結論：無需修改**

原因：
- 該服務在 `SaveWithDetailsAsync` 中直接使用 `quantityDiff` 進行增量調整
- 未實現 `UpdateInventoryByDifferenceAsync` 方法
- 使用不同的庫存處理模式，無累加問題

---

## 🧪 測試場景

### 場景 1：正常編輯流程

```
T1: 新增 R001，產品A 44個
    → InventoryTransaction: R001, Purchase, +44
    → 庫存: 44

T2: 編輯為 60個
    → InventoryTransaction: R001_ADJ, Purchase, +16
    → 庫存: 60 ✅（原邏輯會是 104 ❌）

T3: 再次編輯為 50個
    → InventoryTransaction: R001_ADJ, Return, -10
    → 庫存: 50 ✅
```

### 場景 2：刪除後重建

```
T1: 新增 R001 (44個)
    → InventoryTransaction: R001, Purchase, +44
    → 庫存: 44

T2: 編輯為 60個
    → InventoryTransaction: R001_ADJ, Purchase, +16
    → 庫存: 60

T3: 刪除 R001
    → InventoryTransaction: R001_DEL, Return, -60
    → 庫存: 0

T4: 重新新增 R001 (50個)
    → InventoryTransaction: R001, Purchase, +50
    → 庫存: 50 ✅

T5: 編輯為 70個
    → 查詢有效記錄（只取 T4 之後）
    → 已處理數量 = 50
    → 調整量 = 70 - 50 = 20
    → InventoryTransaction: R001_ADJ, Purchase, +20
    → 庫存: 70 ✅（不會累加 T2 的記錄）
```

### 場景 3：銷貨退回流程

```
T1: 新增銷貨單 S001 (30個)
    → InventoryTransaction: S001, Sale, -30
    → 庫存: -30

T2: 新增銷貨退回單 SR001 (10個)
    → InventoryTransaction: SR001, SalesReturn, +10
    → 庫存: -20 ✅

T3: 編輯退回為 15個
    → InventoryTransaction: SR001_ADJ, SalesReturn, +5
    → 庫存: -15 ✅

T4: 刪除退回單 SR001
    → InventoryTransaction: SR001_DEL, Sale, -15
    → 庫存: -30 ✅（使用 Sale 減少庫存）
```

---

## 🔧 技術細節

### 交易類型對照表

| 業務場景 | 初始新增 | 編輯調整 | 刪除回沖 |
|---------|---------|---------|---------|
| **採購入庫** | Purchase (+) | Purchase (+) / Return (-) | Return (-) |
| **銷貨出庫** | Sale (-) | Sale (-) / SalesReturn (+) | SalesReturn (+) |
| **採購退貨** | Return (-) | Return (-) / Purchase (+) | Purchase (+) |
| **銷貨退回** | SalesReturn (+) | SalesReturn (+) / Sale (-) | Sale (-) |
| **領料出庫** | MaterialIssue (-) | MaterialIssue (-) / MaterialReturn (+) | MaterialReturn (+) |

### 交易編號命名規則

| 格式 | 說明 | 範例 |
|-----|------|------|
| `{Code}` | 初次新增 | `R001` |
| `{Code}_ADJ` | 編輯調整 | `R001_ADJ` |
| `{Code}_DEL` | 刪除回沖（批次邊界） | `R001_DEL` |

### 有效批次追蹤演算法

```
1. 查詢所有相關交易記錄（包含所有後綴）
2. 按時間和 ID 排序
3. 找出最後一筆 _DEL 記錄（批次邊界）
4. 只統計批次邊界之後的記錄（排除 _DEL 本身）
5. 計算已處理數量 = Σ(有效記錄的數量異動)
6. 調整量 = 目標數量 - 已處理數量
```

---

## 📊 影響範圍

### 修改的服務

- ✅ `PurchaseReceivingService`
- ✅ `SalesDeliveryService`
- ✅ `SalesReturnService`
- ⚪ `PurchaseReturnService`（無需修改）
- ✅ `MaterialIssueService`（2025-12-12 新增修正）

### 資料庫影響

- ✅ 不需要資料庫遷移
- ✅ 新增枚舉值（SalesReturn = 12, MaterialReturn = 13）
- ✅ 不刪除任何歷史記錄
- ✅ 完整保留審計追蹤

### 向下相容性

- ✅ 舊有的交易記錄不受影響
- ✅ 現有功能正常運作
- ✅ 只修正計算邏輯，不改變資料結構

---

## 🎯 修改目標達成

- ✅ 統一服務層交易類型命名
- ✅ 修正庫存累加 Bug
- ✅ 支援刪除後重建場景
- ✅ 保留完整審計追蹤
- ✅ 提升系統可維護性
- ✅ 語意清晰，符合業務邏輯

---

## 📚 相關文件

- [README_庫存異動正確撰寫方式.md](README_庫存異動正確撰寫方式.md)
- [Data/Enums/InventoryEnums.cs](../Data/Enums/InventoryEnums.cs)
- [Services/PurchaseReceivingService.cs](../Services/PurchaseReceivingService.cs)
- [Services/SalesDeliveryService.cs](../Services/SalesDeliveryService.cs)
- [Services/SalesReturnService.cs](../Services/SalesReturnService.cs)

---

**文件版本：v2.0**  
**最後更新：2025-12-12**  
**作者：GitHub Copilot**  
**狀態：✅ 已完成並測試**
