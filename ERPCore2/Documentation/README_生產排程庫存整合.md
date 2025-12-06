# 生產排程庫存整合說明

## 概述

生產排程系統已與庫存管理系統完整整合，實現了從訂單轉排程、組件領料、到完工入庫的完整流程追蹤。

## 庫存異動類型

| 功能模組 | 交易類型 | 單號格式 | 說明 |
|---------|---------|---------|------|
| 組件領料 (MaterialIssueModal) | `MaterialIssue (11)` | `MI{yyyyMMddHHmmss}` | 扣減組件庫存 |
| 完工入庫 (ProductionCompletionModal) | `ProductionCompletion (9)` | `PC{yyyyMMddHHmmss}` | 增加成品庫存 |

## 功能說明

### 1. 組件領料 (MaterialIssueModal)

**位置**: `Components/Shared/BaseModal/Modals/ProductionManagement/MaterialIssueModal.razor`

**功能**:
- 顯示生產排程項目的 BOM 組件清單
- 計算各組件的需求數量、已領數量、待領數量
- 支援選擇來源倉庫和庫位
- 提交時自動呼叫 `InventoryStockService.ReduceStockAsync()` 扣減庫存
- 記錄領料交易至 `InventoryTransaction` 表

**資料流**:
```
用戶點擊「組件領料」
    → 載入 ProductionScheduleDetail 組件清單
    → 用戶選擇倉庫、確認領料數量
    → 呼叫 ReduceStockAsync() 扣減各組件庫存
    → 更新 ProductionScheduleDetail.IssuedQuantity
    → 完成通知
```

### 2. 完工入庫 (ProductionCompletionModal)

**位置**: `Components/Shared/BaseModal/Modals/ProductionManagement/ProductionCompletionModal.razor`

**功能**:
- 登錄生產完工數量
- 支援選擇入庫倉庫和庫位
- 可選擇是否自動建立入庫記錄
- 提交時自動呼叫 `InventoryStockService.AddStockAsync()` 增加庫存
- 記錄完工交易至 `InventoryTransaction` 表

**資料流**:
```
用戶點擊「完工登錄」
    → 載入 ProductionScheduleItem 資訊
    → 用戶輸入完工數量、選擇入庫倉庫
    → 建立 ProductionScheduleCompletion 記錄
    → 更新 ProductionScheduleItem.CompletedQuantity
    → 若勾選自動入庫，呼叫 AddStockAsync() 增加成品庫存
    → 完成通知
```

## 相關服務

### IInventoryStockService

```csharp
// 扣減庫存（領料）
Task<ServiceResult> ReduceStockAsync(
    int productId, 
    int warehouseId, 
    int quantity,
    InventoryTransactionTypeEnum transactionType, 
    string transactionNumber,
    int? locationId = null, 
    string? remarks = null
);

// 增加庫存（入庫）
Task<ServiceResult> AddStockAsync(
    int productId, 
    int warehouseId, 
    int quantity,
    InventoryTransactionTypeEnum transactionType, 
    string transactionNumber,
    decimal? unitCost = null, 
    int? locationId = null, 
    string? remarks = null,
    string? batchNumber = null, 
    DateTime? batchDate = null, 
    DateTime? expiryDate = null
);
```

## 資料表關聯

```
ProductionSchedule (生產排程主檔)
    │
    ├── ProductionScheduleItem (排程項目 - 成品)
    │       │
    │       ├── ProductionScheduleDetail (BOM 組件)
    │       │       ├── IssuedQuantity      (已領數量)
    │       │       └── PendingIssueQuantity (待領數量 - 計算值)
    │       │
    │       └── ProductionScheduleCompletion (完工記錄)
    │               ├── CompletedQuantity (完工數量)
    │               └── WarehouseId       (入庫倉庫)
    │
    └── InventoryStock / InventoryStockDetail (庫存異動)
```

## 注意事項

1. **數量轉換**: 庫存服務使用 `int` 類型，生產系統使用 `decimal`，系統會自動四捨五入轉換

2. **倉庫優先順序**:
   - 領料: 優先使用組件明細的倉庫 → 第一個有效倉庫
   - 入庫: 優先使用完工記錄倉庫 → 排程項目預設倉庫 → 第一個有效倉庫

3. **交易追蹤**: 所有庫存異動都會記錄在 `InventoryTransaction` 表，可透過交易單號追蹤

## 後續優化建議

- [ ] 支援批次領料（多組件一次性處理）
- [ ] 增加庫存不足預警機制
- [ ] 支援領料單據列印
- [ ] 完工記錄與品質檢驗整合
