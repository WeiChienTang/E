# 庫存異動簡化設計 - OperationType 改版說明

## 📋 改版背景

### 舊設計問題
原本的庫存異動設計採用**後綴模式**，一張來源單據可能產生多筆異動主檔：
- `RV20260101001` — 原始入庫
- `RV20260101001_ADJ` — 編輯調整
- `RV20260101001_DEL` — 刪除回退
- `RV20260101001_PRICE_ADJ_IN/OUT` — 價格調整

**問題：**
1. 一張進貨單可能產生 3-4 筆異動記錄，難以追蹤
2. 查詢關聯文件時需要複雜的 `StartsWith` 邏輯
3. 使用者難以理解「同一張單為什麼有這麼多異動記錄」

---

## ✅ 新設計：OperationType 欄位

### 核心概念
**一張來源單據 = 一筆異動主檔**

所有操作（首次入庫、編輯調整、刪除回退）都記錄在**同一個主檔**下的**不同明細**，透過 `OperationType` 欄位區分操作類型。

### 新增欄位

在 `InventoryTransactionDetail` 新增三個欄位：

```csharp
/// <summary>
/// 操作類型：Initial（首次）、Adjust（調整）、Delete（刪除回退）、PriceAdjust（價格調整）
/// </summary>
public InventoryOperationTypeEnum OperationType { get; set; } = InventoryOperationTypeEnum.Initial;

/// <summary>
/// 操作說明（例如：首次入庫、編輯調增 +10、刪除回退）
/// </summary>
public string? OperationNote { get; set; }

/// <summary>
/// 操作時間（用於追蹤變更順序）
/// </summary>
public DateTime OperationTime { get; set; } = DateTime.Now;
```

### InventoryOperationTypeEnum 列舉

```csharp
public enum InventoryOperationTypeEnum
{
    [Description("首次")]
    Initial = 1,
    
    [Description("調整")]
    Adjust = 2,
    
    [Description("刪除")]
    Delete = 3,
    
    [Description("價格調整")]
    PriceAdjust = 4
}
```

---

## 📝 使用範例

### 情境：進貨單 RV20260101001

1. **首次確認**：入庫 100 個 A 商品
   - 明細 1: `OperationType = Initial, Quantity = 100`

2. **編輯修改**：數量改為 80 個（減少 20 個）
   - 明細 2: `OperationType = Adjust, Quantity = -20`

3. **刪除單據**：回退庫存（減少 80 個）
   - 明細 3: `OperationType = Delete, Quantity = -80`

**結果：一筆主檔 + 三筆明細，清楚呈現所有變動歷程**

---

## 🔧 修改的檔案清單

### 資料層
| 檔案 | 修改內容 |
|------|----------|
| `Data/Enums/InventoryEnums.cs` | 新增 `InventoryOperationTypeEnum` |
| `Data/Entities/Inventory/InventoryTransactionDetail.cs` | 新增 `OperationType`, `OperationNote`, `OperationTime` 欄位 |

### 服務層
| 檔案 | 修改內容 |
|------|----------|
| `Services/Warehouses/InventoryStockService.cs` | `GetOrCreateTransactionAsync` 改為只用 TransactionNumber 查詢；`AddStockAsync` 和 `ReduceStockAsync` 新增 `operationType` 參數 |
| `Services/Warehouses/IInventoryStockService.cs` | 介面方法簽章更新 |
| `Services/Purchase/PurchaseReceivingService.cs` | 移除 `_DEL`, `_ADJ` 後綴，改用 `OperationType` |
| `Services/Purchase/PurchaseReturnService.cs` | 移除 `_DEL`, `_ADJ` 後綴，改用 `OperationType` |
| `Services/Sales/SalesDeliveryService.cs` | 移除 `_DEL`, `_ADJ` 後綴，改用 `OperationType` |
| `Services/Sales/SalesReturnService.cs` | 移除 `_DEL`, `_ADJ` 後綴，改用 `OperationType` |
| `Services/Inventory/MaterialIssueService.cs` | 移除 `_DEL`, `_ADJ` 後綴，改用 `OperationType` |
| `Services/Inventory/InventoryTransactionService.cs` | `GetRelatedTransactionsAsync` 改用 `OperationType` 分組 |

### Migration
| 檔案 | 說明 |
|------|------|
| `Migrations/20260126042326_AddOperationTypeToInventoryTransactionDetail.cs` | 新增三個欄位的 Migration |

---

## 🚀 執行步驟

### 1. 更新資料庫

```powershell
cd ERPCore2
dotnet ef database update
```

### 2. 現有資料處理

現有的異動明細預設會使用 `OperationType = Initial (1)`，因為 Entity 預設值已設定。

若需要處理舊資料中的 `_ADJ`, `_DEL` 記錄，可考慮撰寫資料轉換 Script（可選）。

---

## 📊 查詢範例

### 查詢某單據的所有異動明細

```csharp
var details = await context.InventoryTransactionDetails
    .Include(d => d.InventoryTransaction)
    .Where(d => d.InventoryTransaction.TransactionNumber == "RV20260101001")
    .OrderBy(d => d.OperationTime)
    .ToListAsync();

// 依操作類型分組
var grouped = details.GroupBy(d => d.OperationType);
```

### 計算某商品在某單據的淨異動數量

```csharp
var netQuantity = await context.InventoryTransactionDetails
    .Where(d => d.InventoryTransaction.TransactionNumber == code && d.ProductId == productId)
    .SumAsync(d => d.Quantity);
```

---

## ⚠️ 注意事項

1. **向後相容**：`GetOrCreateTransactionAsync` 仍會清除舊格式的後綴（`_ADJ`, `_DEL` 等），確保舊資料能正常查詢

2. **預設值**：新明細預設 `OperationType = Initial`，呼叫端若需要記錄調整或刪除，需明確傳入 `operationType` 參數

3. **查詢邏輯變更**：原本使用 `TransactionNumber.StartsWith(code + "_")` 的查詢需改為 `TransactionNumber == code` + `OperationType` 過濾

---

## 📅 改版日期
2026-01-26
